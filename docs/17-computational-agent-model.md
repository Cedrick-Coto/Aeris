# 17. Computational Agent Model

**Versión**: 0.1  
**Estado**: Borrador  
**Última actualización**: 2026-07-29

---

## Propósito

Este documento define el **modelo computacional del agente** antes de escribir una sola línea de implementación. No describe una implementación concreta, sino el **contrato formal** que cualquier implementación cognitiva (ACMA v1, v2, etc.) debe respetar.

Sirve como puente entre los principios arquitectónicos (ADR-0006, ADR-0008, ADR-0009, ADR-0010) y la implementación del Sprint 3.

---

## 1. Arquitectura General

```
                         Mundo ECS
                            │
                     Simulation Tick
                            │
                    Semantic Extractor
                            │
                     ┌──────┴──────┐
                     │   ACMA vN   │
                     │  (módulo    │
                     │  cognitivo  │
                     │  reemplaza- │
                     │  ble)       │
                     └──────┬──────┘
                            │
                   SelfSnapshot
                            │
                     ┌──────┴──────┐
                     │  Narrative  │
                     │  Pipeline   │
                     └──────┬──────┘
                            │
                          LLM
```

### Regla fundamental

ACMA es un **módulo intercambiable**. El motor cognitivo no sabe qué teoría del agente está ejecutando. ACMA v1, v2, v3, etc. son experimentos sobre la misma infraestructura. El contrato entre ACMA y el resto del sistema está definido por las interfaces de este documento.

---

## 2. Causal Chain (cadena causal completa)

Cada tick, la información fluye en este orden estricto:

```
Perception
    │ Entrada: World, eventos del tick
    │ Salida: Percept[]
    ▼
Attention
    │ Entrada: Percept[], AffectState, Goals
    │ Salida: AttendedPercept[]
    ▼
Working Memory (actualización)
    │ Entrada: AttendedPercept[], inferencias del tick anterior
    │ Salida: WorkingMemoryContent
    ▼
Affect Update
    │ Entrada: Percept[], WorkingMemoryContent, Goals, baselines
    │ Salida: AffectState (nuevo)
    ▼
Reasoning
    │ Entrada: WorkingMemoryContent, AffectState, LongTermMemory, WorldModel
    │ Salida: Inferencias, BeliefChanges
    ▼
Goal Selection
    │ Entrada: AffectState, Inferencias, AutobiographicalMemory
    │ Salida: GoalPriorityChanges
    ▼
Planning
    │ Entrada: Goal activo, WorldModel, AffectState
    │ Salida: Plan (secuencia de acciones)
    ▼
Decision
    │ Entrada: Plan, AffectState, estado actual del mundo
    │ Salida: Action (próxima acción)
    ▼
Action Execution
    │ Entrada: Action
    │ Salida: Evento en EventBus → el ECS procesa
    ▼
Audit
    │ Entrada: Action, Inferencias, AffectState, Goals
    │ Salida: ConflictReport[], Corrections
    ▼
Memory Consolidation
    │ Entrada: WorkingMemoryContent, AffectSnapshot, Outcome
    │ Salida: LongTermMemory update
    ▼
Identity Reconstruction
    │ Entrada: AutobiographicalMemory, LongTermMemory, ActiveGoals,
    │           AffectState, Relationships, RecentReflections
    │ Salida: SelfSnapshot (existe solo durante este tick)
```

### Invariante de cadena causal

- Ningún sistema puede leer la salida de otro sistema en el mismo tick si ese otro sistema aparece después en la cadena.
- Todos los sistemas se ejecutan en orden. No hay loops intra-tick.
- La cadena causal es la misma para toda versión de ACMA. ACMA puede cambiar la implementación interna de cada sistema, pero no el orden ni las interfaces.

---

## 3. Interfaces de cada subsistema

### 3.1 PerceptionSystem

```
Entradas:
  - World (ECS): entities, componentes, relaciones espaciales
  - EventBus: eventos del tick actual
  - AgentId: EntityId del agente

Salidas:
  - Percept[] — lista plana de perceptos crudos
  - Cada Percept contiene:
      • Type: Visual | Auditory | Aura | Proprioceptive
      • Source: EntityId
      • Data: struct específica del tipo
      • Confidence: float [0, 1]
      • Timestamp: Tick

Invariantes:
  - PerceptionSystem no escribe memoria, ni afecto, ni goals.
  - Todo percepto tiene confidence > 0.
  - La suma de perceptos por tick está acotada superiormente.
```

### 3.2 AttentionSystem

```
Entradas:
  - Percept[] (de PerceptionSystem)
  - AffectState (del tick anterior, o valores basales si es tick 1)
  - Goals activos

Salida:
  - Percept[] — subconjunto filtrado, ordenado por saliencia

Algoritmo:
  - Asignar puntuación de saliencia a cada percepto:
        saliencia(p) = novelty(p) × relevance(p, goals) × affectModulation(p, affect)
  - Seleccionar los N perceptos con mayor saliencia (N fijo por configuración).
  - El resto se descarta o degrada.

Modulación afectiva:
  - Arousal alto → N más grande (atención dispersa)
  - Stress alto → sesgo hacia perceptos de amenaza
  - Curiosity alto → sesgo hacia perceptos novedosos

Invariantes:
  - El presupuesto atencional (N) es fijo y configurable.
  - AttentionSystem no modifica AffectState.
```

### 3.3 AffectSystem

```
Entradas:
  - Percept[] (atendidos)
  - WorkingMemoryContent
  - Goals activos
  - AffectState anterior (homeostasis)

Salida:
  - AffectState (nuevo)

Variables de estado (continuas):
  Variable           Rango       Rol
  ─────────────────────────────────────────────────────
  Curiosity          [0, 1]      Impulso exploratorio
  Stress             [0, 1]      Degradación cognitiva
  Confidence         [0, 1]      Autoeficacia percibida
  Trust              [0, 1]      Apertura a otros
  Novelty            [0, 1]      Percepción de novedad
  Attachment         [0, 1]      Vínculo con entidades
  Threat             [0, 1]      Percepción de peligro
  RewardExpectation  [0, 1]      Anticipación de refuerzo
  CognitiveLoad      [0, 1]      Sobrecarga computacional

No existen variables discreta «Happy», «Sad», «Angry».
Esas son etiquetas narrativas, no estado interno.

Regulación:
  - Cada variable tiende a un valor basal (homeostasis).
  - Los perceptos y eventos pueden desplazar temporalmente cada variable.
  - La velocidad de回归 (return to baseline) es configurable por variable.

Sistemas que modifica (modulación):
  - Attention: arousal/novelty/threat → filtro atencional
  - WorkingMemory: stress/cognitiveLoad → capacidad efectiva
  - Reasoning: confidence/threat → sesgo inferencial
  - Planning: confidence/threat → audacia de planes
  - Memory: novelty/rewardExpectation → peso de codificación
  - Decision: confidence/stress → velocidad y optimalidad

Invariantes:
  - AffectSystem no produce texto, ni etiquetas emocionales, ni acciones.
  - AffectSystem solo actualiza el vector AffectState.
  - Ningún otro sistema escribe AffectState.
```

### 3.4 WorkingMemorySystem

```
Entradas:
  - Percept[] atendidos
  - AffectState
  - Contenido anterior de WorkingMemory (decaimiento)

Salida:
  - WorkingMemoryContent (contenido actualizado)

Propiedades:
  - Capacidad máxima: N chunks (configurable, default 7 ± 2)
  - Cada chunk tiene: data, timestamp, saliencia, decayRate
  - Decaimiento: chunks no refrescados pierden saliencia cada tick
  - Si un chunk cae por debajo de un umbral, se descarta

Contenido típico:
  - Perceptos activos
  - Inferencias recientes
  - Estado afectivo actual (solo lectura, copia)
  - Goal activo
  - Plan en curso

Invariantes:
  - WM no escribe en LTM directamente.
  - WM no modifica AffectState.
```

### 3.5 LongTermMemorySystem

```
Entradas:
  - WorkingMemoryContent (para consolidación)
  - AffectSnapshot (asociado al contenido a consolidar)
  - Query (para recuperación)

Salida:
  - Recuerdos recuperados (para Reasoning, Planning, IdentityReconstruction)
  - Consolidaciones (escritura diferida)

Tipos de memoria:
  - Episódica: eventos, con timestamp, afecto asociado, significancia
  - Semántica: hechos, creencias, conocimiento del mundo
  - Procedimental: secuencias de acción aprendidas

Procesos:
  - Consolidación: WM → LTM (en momentos de baja carga)
  - Reconsolidación: al recuperar, el recuerdo se modifica con contexto actual
  - Olvido: recuerdos no accedidos pierden fuerza
  - Reinterpretación: recuerdos se actualizan con nueva información
```

### 3.6 WorldModelSystem

```
Entradas:
  - Percept[] (atendidos, histórico)
  - Inferencias (de Reasoning)
  - LongTermMemory (conocimiento del mundo)

Salida:
  - WorldModelState — representación interna parcial del mundo

Propiedades:
  - Es parcial: el agente no conoce todo el mundo
  - Es probabilístico: incluye incertidumbre sobre lo conocido
  - Se actualiza por percepción e inferencia
  - Contiene: mapa mental, relaciones causales, teoría de otros agentes

Invariantes:
  - WorldModel es una entidad separada del World ECS.
  - El LLM nunca accede al WorldModel directamente.
```

### 3.7 ReasoningSystem

```
Entradas:
  - WorkingMemoryContent
  - AffectState
  - LongTermMemory (hechos, creencias)
  - WorldModel

Salida:
  - Inference[] — nuevas inferencias
  - BeliefChange[] — actualizaciones a creencias

Tipos de inferencia:
  - Causal: "evento A → probablemente evento B"
  - Deductiva: "todos los X son Y, esto es X → esto es Y"
  - Abductiva: "efecto observado → posible causa"
  - Analógica: "situación similar a la anterior → misma solución"

Modulación afectiva:
  - Confidence alta → inferencias más audaces
  - Threat alta → sesgo de amenaza
  - Stress alto → inferencias más simples y rápidas

Invariantes:
  - ReasoningSystem no ejecuta acciones.
  - ReasoningSystem no escribe en AffectState.
```

### 3.8 GoalSystem

```
Entradas:
  - AffectState
  - Inferencias (de Reasoning)
  - AutobiographicalMemory
  - WorkingMemoryContent

Salida:
  - ActiveGoal[] — lista priorizada de objetivos activos

Estructura de un Goal:
  - Type: Exploration | Social | Survival | Knowledge | Protection | ...
  - Priority: float [0, 1]
  - State: Inactive | Active | Suspended | Completed | Failed
  - Progress: float [0, 1]
  - Subgoals: Goal[]
  - Source: necesidad | evento | inferencia | relación

Dinámica:
  - Goals se activan/desactivan según contexto
  - Prioridades moduladas por AffectState
  - Goals completados/failed → AutobiographicalMemory

Invariantes:
  - Siempre hay al menos un goal activo.
  - La suma de prioridades no necesita ser 1.
```

### 3.9 PlanningSystem

```
Entradas:
  - ActiveGoal (el de mayor prioridad)
  - WorldModel
  - AffectState
  - LongTermMemory (procedimental)

Salida:
  - Plan — secuencia ordenada de acciones

Procesos:
  - Generación: construir planes desde acciones posibles
  - Evaluación: simular cada plan en WorldModel (forward)
  - Selección: plan con mejor relación costo/beneficio esperado

Modulación afectiva:
  - Confidence bajo → planes cortos y conservadores
  - Threat alto → planes que evitan riesgo
  - Curiosity alto → planes que incluyen exploración

Invariantes:
  - PlanningSystem no ejecuta acciones directamente.
  - PlanningSystem no accede al World ECS real.
```

### 3.10 DecisionSystem

```
Entradas:
  - Plan (de PlanningSystem)
  - AffectState
  - WorldModel (estado actual simulado)
  - WorkingMemoryContent

Salida:
  - Action — próxima acción a ejecutar

Algoritmo:
  1. Evaluar si el plan sigue siendo válido (estado actual vs esperado)
  2. Si válido → extraer siguiente paso
  3. Si no válido → re-planificar o acción reactiva por defecto
  4. Emitir Action como evento en el EventBus

Estructura de Action:
  - Type: Move | Interact | Communicate | Observe | Wait | ...
  - Target: EntityId | Position | null
  - Parameters: Dictionary<string, float>
  - Confidence: float [0, 1]
  - Tick: long

Modulación:
  - Stress alto → decisiones más rápidas, menos óptimas
  - Confidence bajo → delay, duda

Invariantes:
  - Toda Action debe ser traducible a un evento del EventBus.
  - DecisionSystem no modifica el mundo directamente.
```

### 3.11 AuditorSystem

```
Entradas:
  - Action seleccionada
  - Inferencias del tick
  - AffectState
  - Goals activos
  - SelfSnapshot (del tick anterior)

Salida:
  - ConflictReport[] — conflictos detectados
  - Correction[] — sugerencias de corrección

Qué audita:
  - Consistencia lógica de inferencias
  - Coherencia entre acción y principios registrados
  - Sesgos por estado afectivo excesivo
  - Alineación con objetivos de largo plazo

Cada report contiene:
  - Severity: float [0, 1]
  - Source: qué subsistema originó el conflicto
  - Description: tipo de conflicto
  - Suggestion: corrección propuesta (si aplica)

Invariantes:
  - AuditorSystem no modifica ningún estado directamente.
  - Sus salidas son sugerencias; otros sistemas deciden si aplicarlas.
```

### 3.12 IdentityReconstructionSystem

```
Entradas:
  - AutobiographicalMemory (episodios significativos)
  - LongTermMemory (creencias, principios)
  - ActiveGoals
  - AffectState (actual)
  - Relationships (activas)
  - RecentReflections (de AuditorSystem)
  - WorkingMemoryContent

Salida:
  - SelfSnapshot — representación integrada del self, existe solo durante el tick

Estructura de SelfSnapshot:
  - NarrativeSummary: string (resumen compuesto de quién soy)
  - ActivePrinciples: Principle[] (principios activos actuales)
  - PerceivedCapabilities: Capability[] (qué creo poder hacer)
  - SignificantRelationships: Relationship[] (vínculos activos)
  - CurrentPriorities: string[] (qué es importante ahora)
  - SelfSummary: string (integración narrative de:
      "Mis objetivos: ...
       Mis recuerdos: ...
       Mis relaciones: ...
       Mis principios: ...
       Mis decisiones anteriores: ...
       Mi modelo del mundo: ...")
  - CoherenceScore: float [0, 1] (consistencia interna del snapshot)

Reglas:
  - SelfSnapshot se construye cada tick desde cero.
  - No existe un componente «SelfComponent» en el ECS.
  - SelfSnapshot dura únicamente lo que dura el tick.
  - Si ningún sistema lo consulta en un tick, no se construye (optimización).
```

---

## 4. ACMA como módulo experimental

ACMA (Agente Cognitivo con Memoria y Afecto) es el nombre de la primera implementación concreta de este modelo computacional.

```
Aeris.Agent/               ← namespace del módulo ACMA
├── ACMAVersion.cs         ← const string con la versión (v1, v2, ...)
├── Perception/
│   └── PerceptionSystem.cs
├── Attention/
│   └── AttentionSystem.cs
├── Affect/
│   └── AffectSystem.cs
├── Memory/
│   ├── WorkingMemorySystem.cs
│   └── LongTermMemorySystem.cs
├── WorldModel/
│   └── WorldModelSystem.cs
├── Reasoning/
│   └── ReasoningSystem.cs
├── Goals/
│   └── GoalSystem.cs
├── Planning/
│   └── PlanningSystem.cs
├── Decision/
│   └── DecisionSystem.cs
├── Audit/
│   └── AuditorSystem.cs
└── Identity/
    └── IdentityReconstructionSystem.cs
```

### Contrato de versión

| Versión | Estado | Base teórica |
|---------|--------|--------------|
| ACMA v1 | Planned | Modelo funcional con afecto vectorial y self reconstruido |

Cada nueva versión de ACMA puede:
- Cambiar la implementación interna de cualquier sistema
- Añadir nuevos sistemas
- Cambiar algoritmos de modulación afectiva
- Cambiar la estructura de SelfSnapshot

No puede:
- Cambiar el orden de la cadena causal
- Cambiar las interfaces de entrada/salida de los sistemas
- Romper el determinismo del núcleo

---

## 5. SelfSnapshot vs Narrative

```
SelfSnapshot (interno, determinista)
    │
    ▼
Semantic Extractor (toma subsistemas relevantes)
    │
    ▼
SemanticState + SelfSnapshot
    │
    ▼
Prompt Builder
    │
    ▼
LLM
    │
    ▼
Narrative (texto)

Proceso:
  1. SelfSnapshot contiene: afecto, prioridades, relaciones, principios, metas
  2. Semantic Extractor toma el SelfSnapshot + WorkingMemory + WorldModel
  3. Prompt Builder construye el prompt con ese estado
  4. LLM verbaliza: produce narrativa consistente con el snapshot
  5. La narrativa no inventa: es la verbalización del estado interno

Regla: el LLM nunca ve:
  - El ECS World directamente
  - Componentes internos no relevantes
  - El código o la configuración del motor
```

---

## 6. Invariantes globales

| # | Invariante | Verificación |
|---|------------|-------------|
| 1 | Ningún sistema escribe fuera de su declaración de salida | Revisión de código |
| 2 | SelfSnapshot se reconstruye cada tick | Test de integración |
| 3 | AffectState solo se escribe desde AffectSystem | Revisión de código |
| 4 | La cadena causal se ejecuta en orden | Test de orden |
| 5 | No hay loops intra-tick | Test de orden |
| 6 | El LLM nunca recibe el ECS World | Test de integración |
| 7 | Toda acción es un evento en el EventBus | Test de salida |
| 8 | El sistema completo es determinista (misma seed, mismas entradas → mismo SelfSnapshot) | Determinism check |
| 9 | ACMA puede reemplazarse sin cambiar el motor | Test de interfaz |

---

## 7. Principios verificables

| Principio (doc-16 §17) | Cómo se verifica en este modelo |
|------------------------|---------------------------------|
| 17.1 Determinismo | Seed global + sin aleatoriedad no controlada |
| 17.2 Presión de causalidad | Cadena causal completa documentada y enforced |
| 17.3 Trazabilidad | Cada sistema expone inputs, outputs y regla de decisión |
| 17.4 Contrato computacional | Todas las interfaces están definidas en este documento |
| 17.5 Localidad causal | Cada sistema declara exactamente qué escribe |
| 17.6 Modulación afectiva | AffectState modula pesos/umbrales, no selecciona respuestas |
