# 14. Development Roadmap

**Versión**: 0.4  
**Estado**: Activo  
**Última actualización**: 2026-07-29

---

## 1. Regla Fundamental

> **Ninguna modificación arquitectónica sin evidencia proveniente de implementación.**

Los documentos de Sprint 0 están congelados. Solo se modifican si la implementación demuestra que una decisión es incorrecta, inviable, o subóptima. En ese caso se crea una ADR nueva, no se edita la existente.

---

## 2. Filosofía del Proyecto

El objetivo no es "hacer que Aeris piense como un humano", sino **construir una arquitectura funcional cuyos fenómenos cognitivos emerjan de la interacción de subsistemas**, sin programar explícitamente un "yo", una "conciencia" o una "experiencia subjetiva".

---

## 3. Progresión de Sprints

```
Sprint 0 ──► Sprint 1 ──► Sprint 2 ──► Sprint 3A ──► Sprint 3B ──► Sprint 3C ──► Sprint 4 ──► Sprint 5 ──► Sprint 6 ──► Sprint 7
Arquitec.    Motor ECS    Sem. Extr.   Infra.       ACMA v1      Observa-     LLM          Narrativa    Mundo Pok.   Aeris
(FROZEN)     (COMPL.)     (Pend.)      Cognitiva    (Modelo      bilidad      (Verbaliz.)  (Pipeline)   (Modelado)   (Personaje)
                                        (Sistemas    Experimental)
                                        Generales)
```

### Dependencias entre Sprints

```
Sprint 0 (S0)
  └──► Sprint 1 (S1)
         └──► Sprint 2 (S2)
                └──► Sprint 3A (S3A)
                       └──► Sprint 3B (S3B)
                       └──► Sprint 3C (S3C)
                              └──► Sprint 4 (S4)
                                     └──► Sprint 5 (S5)
                                            └──► Sprint 6 (S6)
                                                   └──► Sprint 7 (S7)
```

**Regla**: Un Sprint no puede iniciar hasta que su dependencia directa esté completa y validada.

---

## 4. Sprint 0 — Arquitectura (FROZEN)

**Objetivo**: Definir la arquitectura completa del motor.  
**Estado**: ✅ Completado y congelado.

### Entregables
- [x] 14 documentos de especificación
- [x] 5 ADRs
- [x] Glosario de términos
- [x] Reglas de validación
- [x] Invariantes del motor

### Criterio de congelación
Todos los documentos revisados y consistentes entre sí. Ninguna decisión arquitectónica queda sin responder a nivel de especificación.

---

## 5. Sprint 1 — Motor ECS

**Objetivo**: Verificar que el motor funciona. Tick completo con ECS, EventBus, Scheduler, Time, y Persistencia. **Motor completamente determinista.**  
**Estado**: ✅ Completado (v0.1.0-engine)

### Requisito fundamental: Determinismo

Dos ejecuciones con el mismo estado inicial, mismos eventos, y misma semilla del RNG deben producir exactamente el mismo estado final:

```
State₀ + Events + Seed → Engine → State₁

Run A == Run B  (bit a bit o semánticamente idéntico)
```

El determinismo facilita:
- Depuración y reproducción de errores
- Pruebas automáticas fiables
- Integración futura del LLM (trabaja sobre estado ya determinado)

### Alcance

```
World (Arch ECS)
├── Entity CRUD
├── Component CRUD
├── Resource CRUD
├── Query (Archetype filter)
│
TimeSystem
├── TimeResource (double)
├── DeltaTime multiplication
├── Calendar update
│
EventBus
├── Dual-queue (Deferred + Immediate)
├── Subscribe / Emit / Flush
├── AdvanceTick
│
SchedulerResource
├── Schedule future events
├── Process on time threshold
│
SystemManager
├── SystemDescriptor registry
├── SystemPhase ordering
├── System execution loop
│
Persistence (mínimo)
├── Save World state (JSON)
├── Load World state
├── Checkpoint on interval
│
RandomResource
├── Global seed
├── Per-subsystem RNG
├── Deterministic seeding
│
EngineStats (telemetría)
├── Tick, TickDuration
├── EntityCount, ComponentCount
├── EventCount, SchedulerQueueSize
├── SystemsExecuted, SystemsDuration[]
├── AllocatedMemory
│
SimulationEngine
├── Full tick lifecycle
├── Determinism enforcement
└── Integration test: spawn → modify → persist → reload → verify
```

### Entregables
- Proyecto C# con Arch ECS referenciado
- `World` con Entity/Component/Resource CRUD
- `TimeSystem` con TimeResource
- `EventBus` dual-queue funcional
- `SchedulerResource` funcional
- `SystemManager` con fases
- `RandomResource` con seed global
- `PersistenceService` mínimo (JSON)
- `EngineStats` colector de telemetría
- `SimulationEngine` ejecuta tick completo
- Tests unitarios para cada componente
- Test de determinismo: dos runs con misma seed producen mismo resultado
- Tests de integración

### Hitos de validación

Antes de considerar terminado el Sprint 1, verificar estos 10 escenarios:

| # | Escenario | Qué valida |
|---|-----------|------------|
| 1 | Crear una entity | Entity CRUD funciona |
| 2 | Añadir y eliminar components | Component CRUD funciona |
| 3 | Ejecutar varios Systems en orden | SystemManager respeta fases |
| 4 | Emitir eventos | EventBus acepta eventos |
| 5 | Procesar eventos en el siguiente tick | Dual-queue funciona |
| 6 | Programar un evento futuro con Scheduler | Scheduler process on time |
| 7 | Guardar el estado | Persistencia write |
| 8 | Cargar el estado | Persistencia read |
| 9 | Continuar simulación sin diferencias vs ejecución continua | Persistencia + state integrity |
| 10 | Verificar determinismo con misma seed | Motor es determinista |

### Restricciones de ingeniería

Estas restricciones se aplican desde el primer día. No son opcionales.

**API pública mínima** — Congelar antes de escribir implementación:

```
World
Entity
EntityBuilder
SystemManager
EventBus
Scheduler
PersistenceManager
Engine
EngineStats
```

La implementación puede cambiar. La API pública se mantiene estable.

**Invariantes del World** — Enforcados como `Debug.Assert` o validaciones en Debug:

- Una Entity nunca puede existir dos veces
- Un Component no puede duplicarse en una Entity
- Toda Entity tiene un ID único
- Ningún System modifica el conjunto de entidades durante iteración sin operaciones seguras
- Todo Event tiene Tick de origen

**Serialización desde el inicio** — Todo Component debe ser serializable:

```
Component → Serializable → Persistible
```

No es una optimización tardía. Es una restricción desde el primer Component.

**Component = datos, System = lógica** — Nunca invertir esto:

```csharp
// ✅ Component puro
struct EmotionComponent
{
    Emotion Current;
    float Intensity;
}

// ❌ Component con lógica (nunca)
struct EmotionComponent
{
    void UpdateEmotion(...) { }  // Error: lógica en Component
}
```

**Zero allocations per tick** — Objetivo desde el primer tick:

```
Tick() → 0 allocations
```

Pre-asignar buffers, reusar listas, evitar boxing. Hace el comportamiento predecible y evita garbage collection spikes.

**Property-based tests** — Además de unit tests, tests que rompen invariantes:

- Crear/destruir 10,000 entities aleatorias
- Añadir/quitar components en orden aleatorio
- Emitir miles de eventos
- Ejecutar cientos de ticks
- Verificar que invariantes nunca se rompen

### Definition of Done
1. Los 10 hitos de validación pasan
2. El motor ejecuta un tick completo sin errores
3. El EventBus distribuye events entre Systems correctamente
4. El Scheduler ejecuta callbacks programados
5. El Time avanza correctamente (1x, 5x, 60x)
6. Se puede guardar y cargar el estado del mundo
7. EngineStats reporta métricas por tick
8. Dos ejecuciones con misma seed producen mismo estado final
9. API pública definida y estable
10. Todos los tests pasan (unit + property-based)
11. No hay warnings de compilación
12. 0 allocations en tick con workload típico

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: 100% de cobertura para componentes core
- Performance: tick completo < 1ms con 100 entities
- Determinismo: 100% de reproducibilidad
- Property-based: invariantes nunca se rompen

### Micro-sprints del Sprint 1

El Sprint 1 se divide en hitos incrementales. Cada uno deja el motor en un estado funcional y verificable.

| Hito | Entregable | Qué valida |
|------|------------|------------|
| 1.1 | Proyecto compila y ejecuta un tick vacío | Infraestructura OK |
| 1.2 | CRUD completo de entidades y components | World funciona |
| 1.3 | TimeResource y avance temporal | Tiempo avanza correctamente |
| 1.4 | SystemManager funcional | Systems ejecutan en orden |
| 1.5 | EventBus dual-queue | Events diferidos funcionan |
| 1.6 | Scheduler | Eventos futuros se ejecutan |
| 1.7 | Persistencia | Save/Load sin pérdida |
| 1.8 | Integración completa del Engine | Tick completo con todo |

**Regla**: Un hito no puede iniciar hasta que el anterior esté completo y verificado.

### Estructura del repositorio

```
Aeris/
├── Aeris.sln
├── Directory.Build.props
├── .editorconfig
├── .gitignore
├── README.md
├── src/
│   └── Aeris.Engine/
│       ├── Aeris.Engine.csproj
│       └── ...
├── tests/
│   └── Aeris.Engine.Tests/
│       ├── Aeris.Engine.Tests.csproj
│       └── ...
├── benchmarks/
│   └── Aeris.Benchmarks/
│       ├── Aeris.Benchmarks.csproj
│       └── ...
└── docs/
    └── architecture/
        ├── world.mmd
        ├── ecs.mmd
        ├── tick.mmd
        └── eventbus.mmd
```

### Diagramas vivos

Los diagramas en `docs/architecture/` reflejan la implementación real, no la especificación. Se actualizan cuando cambia el código.

---

## 5. Sprint 1.5 — ECS Cognitivo

**Objetivo**: Añadir la capa cognitiva determinista. Todo funciona sin LLM.
**Estado**: ✅ Completado (v0.1.0-engine)

### Alcance

```
Components
├── MemoryComponent
├── KnowledgeComponent
├── BeliefComponent
├── EmotionComponent
├── GoalComponent
├── AttentionComponent
├── RelationshipComponent
│
Systems
├── MemoryConsolidationSystem
├── KnowledgeUpdateSystem
├── EmotionProcessingSystem
├── GoalEvaluationSystem
├── AttentionUpdateSystem
├── RelationshipSystem
│
Events
├── MemoryCreatedEvent
├── KnowledgeAcquiredEvent
├── EmotionChangedEvent
├── GoalCompletedEvent
├── RelationshipChangedEvent
```

### Entregables
- 7 Components cognitivos
- 6 Systems cognitivos
- Events correspondientes
- Tests para cada System
- Test de integración: percepción → memoria → emoción → goal

### Definition of Done
1. Un Entity puede percibir, recordar, sentir, y tener goals
2. Las memorias se degradan correctamente
3. Las emociones se activan por triggers y se disipan
4. Los goals se evalúan y priorizan
5. Las relationships se mantienen bidireccionales
6. Todo funciona sin LLM (determinista)
7. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 80% para Systems cognitivos
- Performance: tick completo < 2ms con 100 entities

### Dependencias
- Sprint 1 completo

---

## 6. Sprint 2 — Semantic Extractor

**Objetivo**: Extraer del estado del mundo el subconjunto que el LLM necesita. Este sprint ocurre antes de la arquitectura cognitiva porque el Semantic Extractor define el contrato de datos que los subsistemas cognitivos deben producir.
**Estado**: ✅ Completado (v0.2.0-semantics)

### Alcance

```
SemanticExtractor
├── Entity extraction (qué entities son relevantes)
├── Context extraction (qué está pasando alrededor)
├── Memory extraction (qué recuerda el entity)
├── Emotion extraction (qué siente)
├── Goal extraction (qué quiere)
├── Relationship extraction (con quién se relaciona)
│
SemanticState (output)
├── Target entity info
├── Nearby entities summary
├── Current situation
├── Relevant memories
├── Emotional state
├── Active goals
├── Key relationships
│
PromptBuilder
├── System instructions
├── Semantic state serialization
├── Player input formatting
├── Output schema definition
```

### Entregables
- `SemanticExtractor` que produce `SemanticState`
- `PromptBuilder` que construye prompts
- Tests de extracción
- Test de integración: world state → semantic state → prompt

### Definition of Done
1. Dado un world state, el extractor produce un SemanticState válido
2. El SemanticState contiene solo información relevante
3. El tamaño del SemanticState es razonable (< 4000 tokens)
4. El PromptBuilder genera prompts válidos
5. Tests pasan

### Métricas mínimas
- SemanticState size: < 4000 tokens promedio
- Extracción time: < 10ms
- Tests: cobertura > 70%

### Dependencias
- Sprint 1 completo

---

## 7. Sprint 3A — Infraestructura Cognitiva

**Objetivo**: Construir los mecanismos generales sobre los que cualquier teoría cognitiva pueda implementarse. No existe todavía "Aeris". Existe únicamente la maquinaria.

**Estado**: Pendiente.

### Naturaleza del Sprint

El Sprint 3 se divide en dos capas para proteger la transición de riesgo **arquitectónico** (Sprints 0–2) a riesgo **científico** (Sprint 3B en adelante). La infraestructura cognitiva (3A) es puramente ingenieril: sistemas ECS deterministas con interfaces formales. El modelo experimental (3B) es científico: una hipótesis implementada.

### Precondición

Antes de iniciar 3A, debe existir el documento de especificación formal del modelo computacional del agente (`docs/17-computational-agent-model.md`), que define:
- Variables de estado de cada subsistema
- Entradas y salidas de cada sistema
- Invariantes
- Cadena causal completa desde percepción hasta acción
- Qué información puede leer y modificar cada sistema

### Sistemas

Todos implementados como **sistemas ECS deterministas** sin contenido de "personalidad" ni teoría cognitiva específica.

```
PerceptionSystem
├── Traduce eventos del mundo a Percept[] estructurados
├── Sin interpretación semántica (solo filtrado sensorial)
├── Incertidumbre como confidence float [0, 1]
└── Salida: Percept[]

AttentionSystem
├── Presupuesto computacional fijo por tick
├── Filtra Percept[] por saliencia
├── Modulado por AffectState (arousal, novelty, threat)
└── Salida: Percept[] (atendidos)

WorkingMemorySystem
├── Capacidad limitada (N chunks, configurable)
├── Decaimiento y refresco por re-atención
└── Salida: WorkingMemoryContent

LongTermMemorySystem
├── Episódica, semántica, procedimental
├── Consolidación, olvido, reinterpretación
└── Salida: Recuerdos recuperados vía query

AffectSystem
├── Vector continuo (no etiquetas discretas)
├── Dimensiones: Curiosity, Stress, Confidence, Trust,
│   Novelty, Attachment, Threat, RewardExpectation,
│   CognitiveLoad
├── Homeostasis: cada variable tiende a valor basal
└── Salida: AffectState (modula otros subsistemas)

GoalSystem
├── Activar, suspender, priorizar objetivos
├── Goals con tipo, prioridad, progreso, subgoals
└── Salida: ActiveGoal[]

ReasoningSystem
├── Inferencia causal, deductiva, abductiva, analógica
├── Modulado por AffectState
├── Sin simulación mental (será en 3B)
└── Salida: Inference[], BeliefChange[]

PlanningSystem
├── Generar, evaluar, seleccionar planes
├── Evaluación sobre WorldModel interno
└── Salida: Plan

DecisionSystem
├── Seleccionar próxima acción desde el plan
├── Emitir Action como evento del EventBus
└── Salida: Action

AuditorSystem
├── Observa razonamiento, detecta conflictos
├── Sin modificar estado (solo reporta)
└── Salida: ConflictReport[], Correction[]

IdentityReconstructionSystem
├── Construye SelfSnapshot desde cero cada tick
├── Entradas: memoria autobiográfica, goals, afecto, relaciones
├── SelfSnapshot existe solo durante el tick
└── No hay un componente «Self» en el ECS
```

### Entregables
- Los 11 sistemas implementados como Systems ECS
- `SelfSnapshot` como struct inmutable (no componente persistente)
- `AffectState` como vector continuo con homeostasis
- Interfaces formales para cada sistema (según doc-17)
- Tests unitarios para cada sistema
- Test de cadena causal: todos los sistemas se ejecutan en orden
- Test de determinismo: misma seed → mismo SelfSnapshot

### Definition of Done (3A)
1. Todos los sistemas implementados con interfaces formales
2. La cadena causal se ejecuta en orden cada tick
3. SelfSnapshot se reconstruye desde cero cada tick
4. AffectState modula otros sistemas (pesos, umbrales)
5. Ningún sistema escribe fuera de su declaración de salida
6. Todo funciona sin LLM (determinista)
7. Tests pasan (unitarios + cadena causal + determinismo)

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 75%
- Performance: tick completo < 5ms con 100 entities

### Dependencias
- Sprint 2 completo
- docs/17-computational-agent-model.md especificado y revisado

---

## 8. Sprint 3B — ACMA v1 (Modelo Experimental)

**Objetivo**: Implementar la primera hipótesis experimental del agente. ACMA (Agente Cognitivo con Memoria y Afecto) no es "la mente". Es un modelo concreto y reemplazable.

**Estado**: Pendiente.

### Naturaleza del Sprint

Este sprint marca la transición a riesgo **científico**. ACMA v1 es una hipótesis implementada. Puede haber ACMA v2, v3, etc. La infraestructura (3A) permite intercambiar modelos sin cambiar el motor.

### Qué aporta ACMA v1 sobre la infraestructura 3A

```
Sprint 3A                          Sprint 3B
─────────────────────────          ─────────────────────────
PerceptionSystem         ▶         Misma implementación
AttentionSystem          ▶         + Umbrales afectivos iniciales
WorkingMemorySystem      ▶         + Chunk types específicos
LongTermMemorySystem     ▶         + Consolidación con afecto
AffectSystem             ▶         + Baselines de personalidad
GoalSystem               ▶         + Goals iniciales de Aeris
ReasoningSystem          ▶         + Sesgos por personalidad
PlanningSystem           ▶         + WorldModel básico
DecisionSystem           ▶         + Árbol de decisión inicial
AuditorSystem            ▶         + Reglas de coherencia
IdentityReconstruction   ▶         + SelfSnapshot con narrativa
                                       autobiográfica
WorldModelSystem         ▶         Nuevo en 3B (mapa interno,
                                    relaciones causales,
                                    teoría de otros)
```

### Estructura

ACMA vive en su propio namespace y es intercambiable por configuración:

```
Aeris.Agent/               ← namespace
├── ACMAVersion.cs         ← "v1"
├── Perception/
├── Attention/
├── Affect/
├── Memory/
├── WorldModel/
├── Reasoning/
├── Goals/
├── Planning/
├── Decision/
├── Audit/
└── Identity/
```

### Entregables
- Módulo `Aeris.Agent` con ACMA v1
- WorldModelSystem (interno, no ECS)
- SelfSnapshot con capacidad de resumen autobiográfico
- Tests de integración: percepción → self → narrativa
- Tests de coherencia del self a lo largo del tiempo
- Documentación de la hipótesis ACMA v1

### Definition of Done (3B)
1. ACMA v1 produce SelfSnapshot consistente
2. El SelfSnapshot puede alimentar al Semantic Extractor
3. El sistema funciona sin LLM (determinista)
4. Tests de coherencia de identidad pasan
5. ACMA v1 puede reemplazarse por ACMA v2 sin cambiar el motor
6. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 75%
- Performance: tick completo < 10ms con 100 entities
- SelfSnapshot generation: < 1ms

### Dependencias
- Sprint 3B completo

---

## 9. Sprint 3C — Observabilidad

**Objetivo**: Añadir herramientas de observabilidad para investigar el comportamiento del agente. No añade inteligencia nueva, pero permite analizar y falsar hipótesis sobre el agente sin depender únicamente de la narrativa generada.

**Estado**: Pendiente.

### Naturaleza del Sprint

Si Aeris aspira a ser también un proyecto de investigación, esta capa es casi tan valiosa como la propia arquitectura. Permite responder preguntas como "¿por qué el agente tomó esta decisión?" sin recurrir a la introspección del LLM.

### Alcance

```
ObservabilityLayer
├── SelfSnapshot Inspector
│   ├── Ver contenido completo del snapshot actual
│   ├── Comparar con snapshot de ticks anteriores
│   └── Visualizar evolución de principios y prioridades
│
├── AffectState Visualizer
│   ├── Serie temporal de cada variable del vector afectivo
│   ├── Correlación entre variables y eventos externos
│   └── Detección de cambios bruscos
│
├── Goal Activation Graph
│   ├── Árbol de objetivos activos con prioridades
│   ├── Historial de activación/desactivación
│   └── Trazabilidad: qué evento activó cada goal
│
├── Causal Decision Trace
│   ├── Cadena causal completa de una acción:
│       Percept → Attention → WM → Affect → Reasoning → Goal → Plan → Decision
│   ├── Pesos y umbrales en cada paso
│   └── Alternativas consideradas y descartadas
│
├── Attention Tree
│   ├── Perceptos recibidos vs atendidos
│   ├── Puntuación de saliencia por percepto
│   └── Perceptos descartados (y por qué)
│
├── Identity Timeline
│   ├── Snapshots anteriores (resumen)
│   ├── Índice de cambio entre ticks
│   ├── Detección de puntos de inflexión
│   └── Coherencia narrativa a lo largo del tiempo
│
└── Reason Trace
    ├── Explicación automática de cada acción
    ├── Formato: "Action X porque {evidence} → {inference}"
    ├── Distinción explícita entre evidencia e inferencia
    └── Exportable a texto para depuración
```

### Entregables
- Módulo `Aeris.Observability` (namespace separado, sin dependencia del Cognitive Model)
- SelfSnapshot Inspector funcional
- AffectState Visualizer con serie temporal
- Goal Activation Graph con trazabilidad
- Causal Decision Trace para cualquier acción
- Attention Tree por tick
- Identity Timeline con detección de cambios
- Reason Trace exportable

### Criterio de diseño

La capa de observabilidad debe poder activarse/desactivarse en tiempo de compilación o configuración. Cuando está desactivada, debe producir **cero allocations por tick**. No debe afectar el rendimiento ni el determinismo del núcleo cuando no está en uso.

### Definition of Done
1. Cada herramienta produce salida válida para un agente funcionando
2. El Causal Decision Trace muestra la cadena completa de una acción
3. El Reason Trace produce explicaciones en formato legible
4. La capa se puede desactivar sin afectar el comportamiento del agente
5. Con observabilidad desactivada: 0 allocations adicionales
6. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Overhead (activado): < 1ms por tick
- Overhead (desactivado): 0 allocations
- Cobertura de trazabilidad: 100% de las acciones tienen trace

### Dependencias
- Sprint 3C completo

---

## 10. Sprint 4 — Integración LLM

**Objetivo**: Integrar el LLM como verbalizador, no como pensador. El LLM nunca modifica beliefs, emotion, memory, goals o world. Solo propone y narra.

**Estado**: Pendiente.

### Flujo

```
World
↓
Semantic Extractor
↓
Self Model
↓
Prompt Builder
↓
LLM
↓
Narrativa
```

### Alcance

```
ILLMAdapter
├── OpenAI adapter
├── Claude adapter
├── Ollama adapter (local)
├── Mock adapter (testing)
│
SemanticExtractor
├── Entity extraction
├── Context extraction
├── Memory extraction
├── Emotion extraction
├── Goal extraction
├── Relationship extraction
│
SelfModel → PromptBuilder
├── System instructions
├── Semantic state serialization
├── Player input formatting
├── Output schema definition
│
LLMRequest / LLMResponse
├── Request: semantic state + player input + constraints
├── Response: narrative + dialogue + thoughts + actions + confidence
│
LLMSystem
├── Build semantic state
├── Extract Self Model
├── Build prompt
├── Call LLM adapter
├── Parse response
├── Validate response
├── Emit events
│
Validation
├── Schema validation
├── Confidence threshold
├── Retry logic
├── Fallback responses
```

### Entregables
- `ILLMAdapter` interface
- 4 adapters (OpenAI, Claude, Ollama, Mock)
- `SemanticExtractor` que produce `SemanticState`
- `SelfModel` integrado en el prompt builder
- `LLMSystem` funcional
- Validación de respuestas
- Tests con Mock adapter
- Test de integración: world → Self Model → LLM → response

### Definition of Done
1. El LLM nunca modifica directamente Beliefs, Emotion, Memory, Goals o World
2. Se puede conectar a al menos un provider real
3. El Mock adapter funciona para tests
4. Las respuestas se validan contra el esquema
5. El retry logic funciona
6. Los fallback responses funcionan
7. Tests pasan

### Métricas mínimas
- LLM response time: < 5s (excluyendo network)
- Validation success rate: > 95%
- Tests: cobertura > 70%

### Dependencias
- Sprint 3B completo

---

## 11. Sprint 5 — Narrativa

**Objetivo**: El agente ya existe. Ahora aprende a hablar.

**Estado**: Pendiente.

### Alcance

```
NarrativePipeline
├── Input formatting
├── Context assembly
├── Output formatting
├── Tense management
├── Perspective management
│
NarrativeQueue
├── Buffer responses
├── Priority ordering
├── Deduplication
│
Dialogue Generation
├── Character voice
├── Emotional tone mapping
├── Context-appropriate responses
│
Internal Monologue
├── Self-reflection verbalization
├── Goals articulation
├── Conflict expression
│
Presentation
├── Text formatting
├── Dialogue formatting
├── Action formatting
├── Thought formatting
├── Descriptions adapted to context
```

### Entregables
- `NarrativePipeline` funcional
- `NarrativeQueue` con buffering
- Generación de diálogo
- Monólogo interno
- Descripciones adaptadas al contexto
- Coherencia lingüística con el estado interno
- Tests de pipeline
- Test de integración: LLM response → formatted narrative

### Definition of Done
1. El pipeline transforma respuestas en texto legible
2. El queue maneja múltiples respuestas
3. El formateo es consistente
4. El diálogo refleja el estado interno del agente
5. El monólogo interno verbaliza pensamientos y conflictos
6. Las descripciones se adaptan al contexto
7. Tests pasan

### Métricas mínimas
- Pipeline time: < 50ms
- Output consistency: > 90%
- Tests: cobertura > 70%

### Dependencias
- Sprint 4 completo

---

## 12. Sprint 6 — Mundo Pokémon

**Objetivo**: Modelar el universo Pokémon: biología, aura, ecosistemas, cultura, lenguaje, evolución, regiones y facciones.

**Estado**: Pendiente.

### Alcance

```
World Model
├── Region graph
├── Routes
├── Settlements
├── Ecosystems
├── Weather system
│
Pokemon Biology
├── Species data
├── Aura system
├── Evolution
├── Combat system
├── Movement patterns
│
Culture & Language
├── Regional cultures
├── Dialogue conventions
├── Naming conventions
│
NPCs
├── Dialogue system
├── Behavior trees
├── Schedule system
├── Relationship dynamics
│
Factions
├── Organizations
├── Goals and conflicts
├── Reputation system
│
Player
├── Input handling
├── Inventory
├── Party management
├── Exploration
```

### Entregables
- Mundo Pokémon completo
- Sistema de auras
- Biología Pokémon
- Ecosistemas
- Cultura y lenguaje
- NPCs con comportamiento
- Facciones
- Jugador con inventario
- Tests de integración del mundo
- Demo jugable

### Definition of Done
1. El mundo Pokémon es navegable
2. Los Pokémon tienen comportamiento autónomo
3. Los Pokémon tienen aura y biología modelada
4. Los NPCs dialogan y reaccionan
5. Las facciones tienen metas y conflictos
6. El jugador puede explorar
7. Todo funciona sin LLM (determinista)
8. Tests pasan

### Métricas mínimas
- World simulation: < 5ms por tick
- NPCs: > 50 concurrentes
- Tests: cobertura > 60%

### Dependencias
- Sprint 5 completo

---

## 13. Sprint 7 — Aeris

**Objetivo**: Aquí aparece el personaje. No antes.

**Estado**: Pendiente.

### Alcance

```
Personal History
├── Backstory
├── Key life events
├── Formative experiences
│
Persistent Traits
├── Personality configuration
├── Behavioral tendencies
├── Communication style
│
Initial Relationships
├── Relationship graph seed
├── Historical bonds
├── Reputation baseline
│
Goals & Principles
├── Core values
├── Long-term aspirations
├── Personal code
│
Capabilities
├── Skills
├── Knowledge domains
├── Special abilities
│
Narrative Development
├── Character arc definition
├── Growth pathways
├── Transformation triggers
```

### Entregables
- Historia personal de Aeris
- Rasgos persistentes
- Relaciones iniciales
- Objetivos personales
- Principios y valores
- Capacidades
- Desarrollo narrativo

### Definition of Done
1. Aeris tiene una historia personal coherente
2. Los rasgos de personalidad son persistentes pero evolucionan
3. Las relaciones iniciales están definidas
4. Los objetivos y principios guían el comportamiento
5. Las capacidades están modeladas
6. El desarrollo narrativo es posible
7. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 60%

### Dependencias
- Sprint 6 completo

---

## 14. Nueva Estructura de la Arquitectura

```
                         Mundo ECS
                            │
                      Simulation Tick
                            │
                     Semantic Extractor
                            │
                     ┌──────┴──────┐
                     │  ┌─ ACMA ──┐│
                     │  │(módulo  ││
                     │  │cognitivo││
                     │  │intercam-││
                     │  │biable)  ││
                     │  └─────────┘│
                     └──────┬──────┘
                            │
                    SelfSnapshot
                            │
                     ┌──────┴──────┐
                     │  Narrative │
                     │  Pipeline  │
                     └──────┬──────┘
                            │
                          LLM
                            │
                   Acción / Narrativa


ACMA internamente (Sprint 3B):
                            │
                      Perception
                            │
                      Attention
                            │
                   Working Memory
                            │
             ┌──────────────┼──────────────┐
             │              │              │
        Reasoning     AffectState      Auditor
             │                             │
         Planning                      Corrections
             │
         Decision
             │
             └──────────┬──┘
                        │
              Long-Term Memory
                        │
              Identity Reconstruction
                        │
                   SelfSnapshot
```

---

## 15. Issues Arquitectónicos

| ID     | Prioridad | Descripción |
| ------ | --------- | ----------- |
| AC-001 | Alta      | Definir el contrato formal de `Percept`. |
| AC-002 | Alta      | Diseñar el `AttentionSystem` con presupuesto computacional fijo. |
| AC-003 | Alta      | Separar formalmente `WorkingMemory` y `LongTermMemory`. |
| AC-004 | Alta      | Definir el algoritmo de actualización de creencias (`BeliefRevision`). |
| AC-005 | Alta      | Diseñar el `WorldModel` como representación parcial del mundo. |
| AC-006 | Alta      | Definir un modelo afectivo pragmático donde las emociones sean variables reguladoras continuas, no etiquetas discretas. |
| AC-007 | Alta      | Especificar cómo se reconstruye el `SelfModel` a partir de autobiografía, estado afectivo, relaciones y objetivos, sin almacenarlo como un componente persistente. |
| AC-008 | Media     | Diseñar `Reflection` y `MetaReflection` como procesos internos periódicos y deterministas. |
| AC-009 | Media     | Definir métricas observables para evaluar la coherencia de la identidad emergente a lo largo del tiempo. |
| AC-010 | Alta      | Documentar explícitamente el límite epistemológico: el proyecto implementa un **modelo funcional de agencia y experiencia subjetiva**, sin afirmar conciencia fenomenológica. |

---

## 16. Métricas Generales

### Por Sprint

| Sprint | Build | Tests | Performance | Cobertura | Determinismo |
|--------|-------|-------|-------------|-----------|--------------|
| S0 | N/A | N/A | N/A | N/A | N/A |
| S1 | 0 errores | 100% pass | < 1ms/tick | > 80% core | 100% reproducible |
| S2 | 0 errores | 100% pass | < 10ms extracción | > 70% | 100% |
| S3A | 0 errores | 100% pass | < 5ms/tick | > 75% | 100% |
| S3B | 0 errores | 100% pass | < 10ms/tick | > 75% | 100% |
| S3C | 0 errores | 100% pass | < 1ms (activado) / 0 alloc (desactivado) | — | 100% |
| S4 | 0 errores | 100% pass | < 5s LLM | > 70% | N/A (LLM es probabilístico) |
| S5 | 0 errores | 100% pass | < 50ms pipeline | > 70% | N/A |
| S6 | 0 errores | 100% pass | < 5ms/tick | > 60% | 100% (simulación determinista) |
| S7 | 0 errores | 100% pass | — | > 60% | — |

### Acumulativas

- **Cobertura total**: > 70% al final de S7
- **Build stability**: 0 errores en main en todo momento
- **Performance regression**: < 10% de degradación entre sprints
- **Documentación**: actualizada con cada sprint

---

## 17. Reglas de Desarrollo

### 17.1 Antes de empezar un Sprint
1. La dependencia directa está completa
2. Todos los tests de la dependencia pasan
3. No hay errores de compilación
4. La documentación de la dependencia está actualizada

### 17.2 Durante un Sprint
1. Implementar en orden de dependencias
2. Tests unitarios antes de integración
3. Commit frecuente con mensajes descriptivos
4. Revisar métricas antes de continuar

### 17.3 Al finalizar un Sprint
1. Todos los tests pasan
2. Métricas mínimas alcanzadas
3. Documentación actualizada
4. Demo funcional (si aplica)
5. Retrospectiva: qué funcionó, qué no

### 17.4 Si una decisión arquitectónica falla
1. Documentar la evidencia
2. Crear ADR nueva (no editar la existente)
3. Actualizar la especificación afectada
4. Ajustar el plan de sprints si es necesario

---

## 18. Riesgos Conocidos

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|--------------|------------|
| Arch ECS no se adapta al flujo | Alto | Baja | Mock adapter para tests tempranos |
| Semantic State demasiado grande | Medio | Media | Paginación, filtrado agresivo |
| LLM latencia inaceptable | Alto | Media | Cache, respuestas predefinidas |
| Persistencia JSON lenta | Bajo | Baja | SQLite como alternativa |
| Sprint 3 (3A + 3B) demasiado ambicioso | Alto | Media | Separar infraestructura (3A) de modelo experimental (3B); priorizar sistemas base |
| Self Model emergente es incoherente | Alto | Media | Métricas observables de identidad (AC-009) |
| Afecto transversal aumenta complejidad | Medio | Alta | Contratos claros entre AffectState y subsistemas cognitivos |

---

## 19. Límite Epistemológico

El proyecto Aeris implementa un **modelo funcional de agencia y experiencia subjetiva**. No afirma que Aeris tenga conciencia fenomenológica, experiencia subjetiva (qualia), o un "yo" real. El sistema produce comportamientos que *simulan* estos fenómenos, pero no se hacen afirmaciones ontológicas sobre la presencia o ausencia de conciencia.

Esta distinción mantiene el proyecto:
- **Sólido desde el punto de vista de ingeniería**: no dependemos de resolver el hard problem of consciousness.
- **Defendible desde el punto de vista científico**: no hacemos afirmaciones que no podamos respaldar.
- **Enfocado en lo construible**: priorizamos la arquitectura funcional sobre la metafísica.

---

## 20. Evolución Futura — Experimental Framework

Una vez que ACMA v1 y la capa de observabilidad (Sprint 3C) existan, el proyecto tendrá la capacidad de responder preguntas experimentales como:

- ¿Qué ocurre si elimino el afecto?
- ¿Qué ocurre si duplico la capacidad de Working Memory?
- ¿Qué pasa si Identity Reconstruction se ejecuta cada 10 ticks en lugar de cada tick?
- ¿Qué cambia si el agente no tiene autobiografía?

Para responderlas sistemáticamente, se propone un **Experimental Framework** como evolución futura (posiblemente renombrando o extendiendo Sprint 6):

```
Experiment
│
├── World Configuration (definir condiciones iniciales)
├── Simulation Run (ejecutar N ticks con seed X)
├── Observability (métricas por tick)
├── Metrics (registro automático)
└── Analysis (comparar resultados entre configuraciones)
```

### Capacidades previstas

- Definir experimentos mediante archivos de configuración YAML/JSON
- Ejecutar múltiples simulaciones con semillas distintas para cada configuración
- Registrar métricas automáticamente (afecto, decisiones, self snapshots)
- Comparar dos modelos cognitivos: ACMA v1 vs ACMA v2, o ACMA vs agente mínimo
- Aislar variables: cambiar un solo parámetro entre dos ejecuciones
- Generar informes reproducibles (datos + visualizaciones)

### Estado

Este framework **no está planificado** para un sprint concreto. Se documenta aquí como evolución natural del proyecto una vez que la infraestructura cognitiva, el modelo experimental y la observabilidad estén operativos. Cuando el riesgo principal pase de «construir el agente» a «entender el agente», este framework será el siguiente paso lógico.
