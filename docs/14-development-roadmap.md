# 14. Development Roadmap

**Versión**: 0.1  
**Estado**: Activo  
**Última actualización**: 2026-07-26

---

## 1. Regla Fundamental

> **Ninguna modificación arquitectónica sin evidencia proveniente de implementación.**

Los documentos de Sprint 0 están congelados. Solo se modifican si la implementación demuestra que una decisión es incorrecta, inviable, o subóptima. En ese caso se crea una ADR nueva, no se edita la existente.

---

## 2. Progresión de Sprints

```
Sprint 0 ──► Sprint 1 ──► Sprint 1.5 ──► Sprint 2 ──► Sprint 3 ──► Sprint 4 ──► Sprint 5
Especif.     Motor Mín.    ECS Cognit.    Sem. Extr.   LLM          Narrativa     Aeris
(FROZEN)     (Tick)        (Determinista) (Transl.)    (Integrac.)  (Pipeline)    (Mundo)
```

### Dependencias entre Sprints

```
Sprint 0 (S0)
  └──► Sprint 1 (S1)
         ├──► Sprint 1.5 (S1.5)
         │      └──► Sprint 2 (S2)
         │             └──► Sprint 3 (S3)
         │                    └──► Sprint 4 (S4)
         │                           └──► Sprint 5 (S5)
         └──► (S1.5 no puede empezar sin S1 completo)
```

**Regla**: Un Sprint no puede iniciar hasta que su dependencia directa esté completa y validada.

---

## 3. Sprint 0 — Especificación (FROZEN)

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

## 4. Sprint 1 — Motor Mínimo

**Objetivo**: Verificar que el motor funciona. Tick completo con ECS, EventBus, Scheduler, Time, y Persistencia. **Motor completamente determinista.**

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

**Objetivo**: Extraer del estado del mundo el subconjunto que el LLM necesita.

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
- Sprint 1.5 completo

---

## 7. Sprint 3 — LLM Integration

**Objetivo**: Integrar el LLM como función pura. El LLM nunca controla, solo narra.

### Alcance

```
ILLMAdapter
├── OpenAI adapter
├── Claude adapter
├── Ollama adapter (local)
├── Mock adapter (testing)
│
LLMRequest / LLMResponse
├── Request: semantic state + player input + constraints
├── Response: narrative + dialogue + thoughts + actions + confidence
│
LLMSystem
├── Build semantic state
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
- `LLMSystem` funcional
- Validación de respuestas
- Tests con Mock adapter
- Test de integración: world → LLM → response

### Definition of Done
1. Se puede conectar a al menos un provider real
2. El Mock adapter funciona para tests
3. Las respuestas se validan contra el esquema
4. El retry logic funciona
5. Los fallback responses funcionan
6. Tests pasan

### Métricas mínimas
- LLM response time: < 5s (excluyendo network)
- Validation success rate: > 95%
- Tests: cobertura > 70%

### Dependencias
- Sprint 2 completo

---

## 8. Sprint 4 — Narrative Pipeline

**Objetivo**: Transformar respuestas del LLM en narrativa para el usuario.

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
Presentation
├── Text formatting
├── Dialogue formatting
├── Action formatting
├── Thought formatting
```

### Entregables
- `NarrativePipeline` funcional
- `NarrativeQueue` con buffering
- Formateo de salida
- Tests de pipeline
- Test de integración: LLM response → formatted narrative

### Definition of Done
1. El pipeline transforma respuestas en texto legible
2. El queue maneja múltiples respuestas
3. El formateo es consistente
4. Tests pasan

### Métricas mínimas
- Pipeline time: < 50ms
- Output consistency: > 90%
- Tests: cobertura > 70%

### Dependencias
- Sprint 3 completo

---

## 9. Sprint 5 — Aeris (Mundo Pokémon)

**Objetivo**: Implementar el mundo Pokémon como instancia del motor.

### Alcance

```
World Model
├── Region graph
├── Routes
├── Settlements
├── Ecosystems
├── Weather system
│
Pokemon
├── Species data
├── Aura system
├── Evolution
├── Combat system
├── Movement patterns
│
NPCs
├── Dialogue system
├── Behavior trees
├── Schedule system
├── Relationship dynamics
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
- NPCs con comportamiento
- Jugador con inventario
- Tests de integración del mundo
- Demo jugable

### Definition of Done
1. El mundo Pokémon es navegable
2. Los Pokémon tienen comportamiento autónomo
3. Los NPCs dialogan y reaccionan
4. El jugador puede explorar
5. Todo funciona sin LLM (determinista)
6. Tests pasan

### Métricas mínimas
- World simulation: < 5ms por tick
- NPCs: > 50 concurrentes
- Tests: cobertura > 60%

### Dependencias
- Sprint 4 completo

---

## 10. Métricas Generales

### Por Sprint

| Sprint | Build | Tests | Performance | Cobertura | Determinismo |
|--------|-------|-------|-------------|-----------|--------------|
| S0 | N/A | N/A | N/A | N/A | N/A |
| S1 | 0 errores | 100% pass | < 1ms/tick | > 80% core | 100% reproducible |
| S1.5 | 0 errores | 100% pass | < 2ms/tick | > 80% | 100% |
| S2 | 0 errores | 100% pass | < 10ms extracción | > 70% | 100% |
| S3 | 0 errores | 100% pass | < 5s LLM | > 70% | N/A (LLM es probabilístico) |
| S4 | 0 errores | 100% pass | < 50ms pipeline | > 70% | N/A |
| S5 | 0 errores | 100% pass | < 5ms/tick | > 60% | 100% (simulación determinista) |

### Acumulativas

- **Cobertura total**: > 70% al final de S5
- **Build stability**: 0 errores en main en todo momento
- **Performance regression**: < 10% de degradación entre sprints
- **Documentación**: actualizada con cada sprint

---

## 11. Reglas de Desarrollo

### 11.1 Antes de empezar un Sprint
1. La dependencia directa está completa
2. Todos los tests de la dependencia pasan
3. No hay errores de compilación
4. La documentación de la dependencia está actualizada

### 11.2 Durante un Sprint
1. Implementar en orden de dependencias
2. Tests unitarios antes de integración
3. Commit frecuente con mensajes descriptivos
4. Revisar métricas antes de continuar

### 11.3 Al finalizar un Sprint
1. Todos los tests pasan
2. Métricas mínimas alcanzadas
3. Documentación actualizada
4. Demo funcional (si aplica)
5. Retrospectiva: qué funcionó, qué no

### 11.4 Si una decisión arquitectónica falla
1. Documentar la evidencia
2. Crear ADR nueva (no editar la existente)
3. Actualizar la especificación afectada
4. Ajustar el plan de sprints si es necesario

---

## 12. Riesgos Conocidos

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|--------------|------------|
| Arch ECS no se adapta al flujo | Alto | Baja | Mock adapter para tests tempranos |
| Semantic State demasiado grande | Medio | Media | Paginación, filtrado agresivo |
| LLM latencia inaceptable | Alto | Media | Cache, respuestas predefinidas |
| Persistencia JSON lenta | Bajo | Baja | SQLite como alternativa |
| Demasiadas dependencias entre sprints | Medio | Media | Sprint 1.5 y 2 pueden paralelizarse parcialmente |
