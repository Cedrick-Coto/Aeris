# 17. Computational Agent Model

**Versión**: 0.2  
**Estado**: Borrador  
**Última actualización**: 2026-07-29

---

## Propósito

Este documento define el **modelo computacional del agente** antes de escribir una sola línea de implementación. No describe una implementación concreta, sino el **contrato formal** que cualquier implementación cognitiva (ACMA v1, v2, etc.) debe respetar.

Sirve como puente entre los principios arquitectónicos (ADR-0006, ADR-0008, ADR-0009, ADR-0010) y la implementación del Sprint 3.

### Niveles epistemológicos

Cada subsistema en este documento se clasifica según el tipo de evidencia que lo respalda, no según su importancia funcional:

| Nivel | Etiqueta | Significado |
|-------|----------|-------------|
| E1 | Evidencia fuerte | Mecanismo bien documentado en ciencias cognitivas, con respaldo experimental |
| E2 | Evidencia moderada | Mecanismo con respaldo parcial o adaptado significativamente para esta arquitectura |
| E3 | Hipótesis de ingeniería | Decisión de diseño pragmática sin respaldo cognitivo directo |
| E4 | Hipótesis especulativa | Mecanismo propuesto sin respaldo empírico, implementado para exploración |

Esta clasificación no cambia el código. Documenta **qué parte del sistema intenta aproximar resultados bien estudiados y qué parte es una decisión de diseño**.

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

**Estado epistemológico**: E1 — Evidencia fuerte. La percepción como traducción de estímulos a representaciones internas es un mecanismo bien establecido.

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

**Estado epistemológico**: E1 — Evidencia fuerte. La atención como filtro con presupuesto computacional limitado está extensamente documentada (Broadbent, Treisman, etc.).

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

**Estado epistemológico**: E2 — Evidencia moderada. La modulación afectiva como vector continuo que sesga procesamiento es consistente con la hipótesis del marcador somático (Damasio) y modelos dimensionales (Russell), pero la selección concreta de 9 variables es una decisión de diseño.

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

**Estado epistemológico**: E1 — Evidencia fuerte. La memoria de trabajo con capacidad limitada, decaimiento y refresco está extensamente validada (Baddeley, Cowan, etc.).

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

**Estado epistemológico**: E1 — Evidencia fuerte. La distinción episódica/semántica/procedimental y los procesos de consolidación y olvido están bien documentados (Tulving, Squire, Ebbinghaus).

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

**Estado epistemológico**: E2 — Evidencia moderada. Los humanos mantenemos modelos internos del mundo, pero la implementación como sistema separado con actualización probabilística es una adaptación de ingeniería.

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

**Estado epistemológico**: E2 — Evidencia moderada. La inferencia causal, deductiva y abductiva son procesos reconocidos, pero su implementación como sistema determinista con modulación afectiva es una simplificación de ingeniería.

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

**Estado epistemológico**: E2 — Evidencia moderada. La priorización de objetivos y su modulación por estado afectivo tiene respaldo (Maslow, Simon), pero la estructura concreta de Goal es una decisión de diseño.

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

**Estado epistemológico**: E2 — Evidencia moderada. La planificación como simulación interna y evaluación de cursos de acción tiene respaldo (Hazy, Frith), pero la implementación con WorldModel es una simplificación.

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

**Estado epistemológico**: E2 — Evidencia moderada. La toma de decisiones con modulación por estrés y confianza tiene respaldo (Kahneman, Tversky), pero la arquitectural pipeline es una decisión de ingeniería.

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

**Estado epistemológico**: E3 — Hipótesis de ingeniería. La metaauditoría como sistema separado que observa el razonamiento sin modificarlo es una decisión arquitectónica sin respaldo cognitivo directo.

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

**Estado epistemológico**: E3 — Hipótesis de ingeniería. La reconstrucción del self desde cero cada tick no tiene respaldo empírico directo. Es una consecuencia de ADR-0006 (Self Model Is Reconstructed), una decisión arquitectónica.

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

## 4. Cognitive Infrastructure vs Cognitive Model

Este documento distingue dos capas dentro del agente:

```
Motor ECS
    │
    ├── Cognitive Infrastructure (mecanismos)
    │       Perception, Attention, Memory, Affect, Goals
    │       └── No dependen de una teoría cognitiva específica
    │
    └── Cognitive Model (teoría)
            ACMA v1, ACMA v2, Minimal Agent, etc.
            └── Cada uno implementa la misma infraestructura
                con parámetros, pesos y algoritmos distintos
```

La **Cognitive Infrastructure** proporciona los mecanismos generales. El **Cognitive Model** decide cómo se configuran, qué pesos tienen y qué teoría implementan.

### 4.1 Cognitive Infrastructure

Son los subsistemas que actúan como **mecanismos** independientes de la teoría cognitiva:

| Subsistema | Rol en infraestructura |
|------------|----------------------|
| Perception | Traducir estímulos del ECS a Percept[] |
| Attention | Filtrar Percept[] por saliencia (presupuesto fijo) |
| WorkingMemory | Ventana temporal de experiencia inmediata |
| LongTermMemory | Almacenamiento persistente con olvido |
| Affect | Vector continuo de modulación (variables intercambiables) |
| Goals | Priorizar objetivos (estructura genérica) |

La infraestructura no «cree» en ninguna teoría. Proporciona los mecanismos para ejecutar cualquier modelo cognitivo.

### 4.2 Cognitive Model

Es la **teoría cognitiva concreta** que decide:

- Qué variables componen el AffectState (p. ej. ACMA v1 usa Curiosity, Stress, Trust; otro modelo podría usar ExplorationDrive, SocialSafety)
- Cómo se calcula la saliencia en Attention
- Los algoritmos de inferencia en Reasoning
- La estructura de SelfSnapshot
- Los baselines afectivos (personalidad)
- Las reglas de Auditor

```
Motor
│
├── Cognitive Infrastructure
│   ├── PerceptionSystem
│   ├── AttentionSystem
│   ├── WorkingMemorySystem
│   ├── LongTermMemorySystem
│   ├── AffectSystem (esqueleto: provee el vector)
│   └── GoalSystem
│
└── Cognitive Model (ACMA v1, v2, ...)
    ├── AffectModel (define las variables concretas)
    ├── ReasoningStrategy
    ├── PlanningStrategy
    ├── DecisionStrategy
    ├── AuditorRules
    ├── IdentityReconstruction
    └── WorldModel
```

### 4.3 ACMA v1

ACMA (Agente Cognitivo con Memoria y Afecto) es el nombre del primer Cognitive Model concreto.

```
Aeris.Agent/               ← namespace del Cognitive Model
├── ACMAVersion.cs         ← "v1"
├── AffectModel/
│   └── ACMAAffectState.cs ← 9 variables específicas
├── Reasoning/
│   └── ACMAReasoning.cs
├── Goals/
│   └── ACMAGoalPriorities.cs
├── Planning/
│   └── ACMAPlanner.cs
├── Decision/
│   └── ACMADecisionTree.cs
├── WorldModel/
│   └── ACMAWorldModel.cs
├── Audit/
│   └── ACMAAuditorRules.cs
└── Identity/
    └── ACMAIdentityReconstruction.cs
```

### 4.4 Contrato de versión

| Versión | Estado | Base teórica |
|---------|--------|--------------|
| ACMA v1 | Planned | Modelo funcional con afecto vectorial y self reconstruido |
| ACMA v2 | — | Abierto a futuras hipótesis |
| Minimal Agent | — | Versión mínima para testing sin teoría cognitiva |

Cada versión puede:
- Cambiar la implementación interna de cualquier sistema del Cognitive Model
- Añadir nuevas variables al AffectState
- Cambiar algoritmos de modulación afectiva
- Cambiar la estructura de SelfSnapshot

No puede:
- Cambiar el orden de la cadena causal
- Cambiar las interfaces de entrada/salida de los sistemas de infraestructura
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
