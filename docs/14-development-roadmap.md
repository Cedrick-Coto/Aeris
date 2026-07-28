# 14. Development Roadmap

**Versión**: 0.2  
**Estado**: Activo  
**Última actualización**: 2026-07-28

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
Sprint 0 ──► Sprint 1 ──► Sprint 2 ──► Sprint 3 ──► Sprint 4 ──► Sprint 5 ──► Sprint 6 ──► Sprint 7 ──► Sprint 8
Arquitec.    Motor ECS    Cognición   Afecto       Self          LLM          Narrativa    Mundo Pokémon  Aeris
(FROZEN)     (COMPL.)     (Determ.)   (Sist. Transv.) (Emergente)  (Verbaliz.)  (Pipeline)   (Modelado)     (Personaje)
```

### Dependencias entre Sprints

```
Sprint 0 (S0)
  └──► Sprint 1 (S1)
         └──► Sprint 2 (S2)
                └──► Sprint 3 (S3)
                       └──► Sprint 4 (S4)
                              └──► Sprint 5 (S5)
                                     └──► Sprint 6 (S6)
                                            └──► Sprint 7 (S7)
                                                   └──► Sprint 8 (S8)
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
**Estado**: ✅ Completado.

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

## 6. Sprint 2 — Arquitectura Cognitiva

**Objetivo**: Implementar el procesamiento cognitivo. No personalidad. No emociones. No self. Solo cognición.

**Estado**: Pendiente.

### Módulos

```
Perception
Attention
Working Memory
Long-Term Memory
Beliefs
Knowledge
Goals
Reasoning
Planning
Prediction
Learning
```

### Issues

#### S2-001 — Perception System
Entrada: World, Events → Salida: Percepts

#### S2-002 — Attention System
Selecciona qué entra, qué ignorar, prioridades.

#### S2-003 — Working Memory
Ventana temporal. Capacidad limitada. Olvido automático.

#### S2-004 — Belief Revision
Actualizar creencias. Resolver contradicciones.

#### S2-005 — Reasoning Engine
Inferencias. Predicciones. Hipótesis.

#### S2-006 — Planning
Generación de planes. Evaluación.

#### S2-007 — Learning
Actualizar conocimiento.

#### S2-008 — World Model
Modelo interno. No copia completa del mundo.

#### S2-009 — Semantic Extractor v2
Extrae únicamente estados cognitivos.

### Definition of Done
1. El sistema de percepción traduce eventos del mundo a perceptos
2. El sistema de atención filtra y prioriza perceptos
3. Working Memory mantiene una ventana temporal con olvido automático
4. El sistema de creencias actualiza y resuelve contradicciones
5. El motor de razonamiento genera inferencias, predicciones e hipótesis
6. El sistema de planning genera y evalúa planes
7. El sistema de aprendizaje actualiza conocimiento
8. World Model es una representación parcial, no una copia del mundo
9. Semantic Extractor v2 extrae exclusivamente estados cognitivos
10. Todo funciona sin emociones, sin self, sin LLM (determinista)
11. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 80% para sistemas cognitivos
- Performance: tick completo < 3ms con 100 entities

### Dependencias
- Sprint 1 completo

---

## 7. Sprint 3 — Arquitectura Afectiva

**Objetivo**: Implementar regulación afectiva funcional. La emoción deja de ser un dato y se convierte en un sistema transversal que modifica atención, memoria, planning, predicción, decisión y aprendizaje.

**Estado**: Pendiente.

### Componentes

```
EmotionState
Mood
Stress
Motivation
Needs
Attachment
Confidence
Uncertainty
```

### Systems

```
Emotion Update
Motivation
Need Satisfaction
Mood Dynamics
Attachment Update
Stress Recovery
```

### Importante

La emoción modifica:
- Attention
- Memory
- Planning
- Prediction
- Decision
- Learning

No solo almacena un enum.

### Definition of Done
1. EmotionState es un sistema continuo (no un enum discreto)
2. Mood tiene inercia y cambia lentamente
3. Stress afecta el rendimiento cognitivo
4. Motivation impulsa la selección de objetivos
5. Needs se satisfacen y degradan con el tiempo
6. Attachment modifica relaciones con entidades significativas
7. Confidence y Uncertainty afectan la toma de decisiones
8. Todos los sistemas afectivos modifican cognición (attention, memory, planning, etc.)
9. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 75%
- Performance: tick completo < 4ms con 100 entities

### Dependencias
- Sprint 2 completo

---

## 8. Sprint 4 — Modelo Emergente del Self

**Objetivo**: Implementar los procesos que producen un modelo funcional del self sin almacenar un componente "Self" persistente. El self emerge de la interacción entre autobiografía, relaciones, objetivos, hábitos, conocimiento y emoción.

**Estado**: Pendiente.

### Módulos

```
Autobiographical Memory
Identity Extractor
Self Model
Self Consistency
Reflection
Meta Reflection
```

### Flujo

```
Historia
+
Relaciones
+
Objetivos
+
Hábitos
+
Conocimiento
+
Emoción
↓
Self Model
```

### Self Model

Debe responder preguntas como:
- ¿Qué sé de mí?
- ¿Qué puedo hacer?
- ¿Qué quiero?
- ¿Cómo he cambiado?
- ¿Quiénes son importantes?
- ¿Qué temo?
- ¿Qué espero?

Nunca existe como objeto fijo. Siempre se reconstruye.

### Reflection

Pensamiento interno. Ejemplo:

```
fallé
↓
¿por qué?
↓
actualizo creencias
↓
cambio estrategia
```

### Meta Reflection

Evalúa principios, estrategias y hábitos. No modifica el código. Modifica el estado interno.

### Definition of Done
1. Autobiographical Memory registra experiencias con contexto temporal y afectivo
2. Identity Extractor produce rasgos emergentes del estado interno
3. Self Model se reconstruye cada vez que se consulta (nunca se almacena como objeto fijo)
4. Self Consistency detecta contradicciones en el modelo del self
5. Reflection ejecuta ciclos de causa-efecto sobre eventos pasados
6. Meta Reflection evalúa principios y estrategias
7. El self responde preguntas sobre identidad, capacidad, deseos y cambios
8. Tests pasan

### Métricas mínimas
- Build: 0 errores, 0 warnings
- Tests: cobertura > 70%
- Performance: tick completo < 5ms con 100 entities

### Dependencias
- Sprint 3 completo

---

## 9. Sprint 5 — Integración LLM

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
- Sprint 4 completo

---

## 10. Sprint 6 — Narrativa

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
- Sprint 5 completo

---

## 11. Sprint 7 — Mundo Pokémon

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
- Sprint 6 completo

---

## 12. Sprint 8 — Aeris

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
- Sprint 7 completo

---

## 13. Nueva Estructura de la Arquitectura

```
                 Mundo
                   │
            Perception
                   │
             Attention
                   │
          Working Memory
                   │
     ┌─────────────┴─────────────┐
     │                           │
 Cognition                 Affect System
     │                           │
     └─────────────┬─────────────┘
                   │
             World Model
                   │
          Autobiographical Memory
                   │
             Reflection Layer
                   │
          Meta-Reflection Layer
                   │
          Emergent Self Model
                   │
           Semantic Extractor
                   │
            Prompt Builder
                   │
                  LLM
                   │
             Acción/Narrativa
```

---

## 14. Issues Arquitectónicos

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

## 15. Métricas Generales

### Por Sprint

| Sprint | Build | Tests | Performance | Cobertura | Determinismo |
|--------|-------|-------|-------------|-----------|--------------|
| S0 | N/A | N/A | N/A | N/A | N/A |
| S1 | 0 errores | 100% pass | < 1ms/tick | > 80% core | 100% reproducible |
| S2 | 0 errores | 100% pass | < 3ms/tick | > 80% | 100% |
| S3 | 0 errores | 100% pass | < 4ms/tick | > 75% | 100% |
| S4 | 0 errores | 100% pass | < 5ms/tick | > 70% | 100% |
| S5 | 0 errores | 100% pass | < 5s LLM | > 70% | N/A (LLM es probabilístico) |
| S6 | 0 errores | 100% pass | < 50ms pipeline | > 70% | N/A |
| S7 | 0 errores | 100% pass | < 5ms/tick | > 60% | 100% (simulación determinista) |
| S8 | 0 errores | 100% pass | — | > 60% | — |

### Acumulativas

- **Cobertura total**: > 70% al final de S8
- **Build stability**: 0 errores en main en todo momento
- **Performance regression**: < 10% de degradación entre sprints
- **Documentación**: actualizada con cada sprint

---

## 16. Reglas de Desarrollo

### 16.1 Antes de empezar un Sprint
1. La dependencia directa está completa
2. Todos los tests de la dependencia pasan
3. No hay errores de compilación
4. La documentación de la dependencia está actualizada

### 16.2 Durante un Sprint
1. Implementar en orden de dependencias
2. Tests unitarios antes de integración
3. Commit frecuente con mensajes descriptivos
4. Revisar métricas antes de continuar

### 16.3 Al finalizar un Sprint
1. Todos los tests pasan
2. Métricas mínimas alcanzadas
3. Documentación actualizada
4. Demo funcional (si aplica)
5. Retrospectiva: qué funcionó, qué no

### 16.4 Si una decisión arquitectónica falla
1. Documentar la evidencia
2. Crear ADR nueva (no editar la existente)
3. Actualizar la especificación afectada
4. Ajustar el plan de sprints si es necesario

---

## 17. Riesgos Conocidos

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|--------------|------------|
| Arch ECS no se adapta al flujo | Alto | Baja | Mock adapter para tests tempranos |
| Semantic State demasiado grande | Medio | Media | Paginación, filtrado agresivo |
| LLM latencia inaceptable | Alto | Media | Cache, respuestas predefinidas |
| Persistencia JSON lenta | Bajo | Baja | SQLite como alternativa |
| Demasiadas dependencias entre sprints | Medio | Media | Sprint 2, 3, y 4 pueden solaparse parcialmente |
| Self Model emergente es incoherente | Alto | Media | Métricas observables de identidad (AC-009) |
| Afecto transversal aumenta complejidad | Medio | Alta | Contratos claros entre Affect System y Cognitive Systems |

---

## 18. Límite Epistemológico

El proyecto Aeris implementa un **modelo funcional de agencia y experiencia subjetiva**. No afirma que Aeris tenga conciencia fenomenológica, experiencia subjetiva (qualia), o un "yo" real. El sistema produce comportamientos que *simulan* estos fenómenos, pero no se hacen afirmaciones ontológicas sobre la presencia o ausencia de conciencia.

Esta distinción mantiene el proyecto:
- **Sólido desde el punto de vista de ingeniería**: no dependemos de resolver el hard problem of consciousness.
- **Defendible desde el punto de vista científico**: no hacemos afirmaciones que no podamos respaldar.
- **Enfocado en lo construible**: priorizamos la arquitectura funcional sobre la metafísica.
