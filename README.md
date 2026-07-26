# Aeris

**Motor de narrativa emergente basado en simulación para un mundo Pokémon.**

Aeris no es un chatbot ni un juego con guion. Es un **simulador de mundo** donde la historia emerge como consecuencia de la simulación, no como producto de un prompt.

---

## Principios Fundamentales

1. **Simulación primero, narrativa después** — La IA nunca decide qué sucede para producir una escena interesante. Primero simula un mundo coherente; la historia emerge como consecuencia.
2. **Separación absoluta entre simulación y presentación** — El motor funciona sin interfaz, sin LLM, sin persistencia. Cada capa es independiente.
3. **El LLM es una función, no un controlador** — Recibe estado estructurado y produce estado estructurado. Nunca muta el World State.
4. **El mundo existe sin el usuario** — El tiempo avanza independientemente de la interacción. Los personajes actúan, el clima cambia, los eventos ocurren.
5. **Extensibilidad por diseño** — Nuevos componentes, sistemas y reglas se añaden sin reescribir el núcleo.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Capa de Presentación                      │
│                        (UI)                                  │
├─────────────────────────────────────────────────────────────┤
│                     Capa de Narrativa                        │
│   Narrative Pipeline → Semantic Extractor → LLM Adapter     │
├─────────────────────────────────────────────────────────────┤
│                    Capa de Simulación                        │
│   EventBus → SystemManager → TimeSystem → WorldOrchestrator │
├─────────────────────────────────────────────────────────────┤
│                      Capa de Datos                           │
│              ECS Core (Arch) → Data Models → Semantic State  │
├─────────────────────────────────────────────────────────────┤
│                   Capa de Persistencia                       │
│                      SQLite + JSON                           │
└─────────────────────────────────────────────────────────────┘
```

### Motores del Sistema

| Motor | Responsabilidad |
|-------|-----------------|
| **Mundo** | Estado del universo (clima, regiones, economía, eventos) |
| **Personajes** | Agentes autónomos con estado, memoria, creencias y objetivos |
| **Cognitivo** | Percepción → Conocimiento → Creencias → Metas → Decisiones |
| **Social** | Relaciones bidireccionales con historia (confianza, respeto, rivalidad) |
| **Memoria** | Almacenamiento con degradación, reinterpretación y olvido |
| **Conocimiento** | Hechos, hipótesis, rumores, mentiras, tradiciones |
| **Simulación** | El mundo avanza aunque nadie interactúe |
| **Narrativo** | Transforma estado del mundo en narrativa |
| **Causalidad** | Cada evento tiene causa y consecuencia |
| **IA (LLM)** | Interpreta estado del mundo, nunca lo controla |

---

## Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Lenguaje | C# (.NET 8.0) |
| Paradigma | ECS (Entity Component System) + Data-Oriented Design |
| ECS Library | [Arch](https://github.com/genaray/Arch) |
| Persistencia | SQLite + JSON |
| LLM | Provider-agnostic (OpenAI, Claude, Ollama, etc.) |

---

## Estructura del Repositorio

```
Aeris/
├── Aeris.sln
├── Directory.Build.props
├── .editorconfig
├── .gitignore
├── LICENSE                          # GPL-3.0
├── README.md
├── src/
│   └── Aeris.Engine/
│       ├── Aeris.Engine.csproj
│       ├── Engine.cs                # SimulationEngine (tick lifecycle)
│       ├── EngineStats.cs           # Telemetría por tick
│       └── World.cs                 # ECS World wrapper
├── tests/
│   └── Aeris.Engine.Tests/
├── benchmarks/
│   └── Aeris.Benchmarks/
└── docs/
    ├── 00-overview.md
    ├── 01-ecs-model.md
    ├── 02-execution-contract.md
    ├── 03-data-models.md
    ├── 04-simulation-systems.md
    ├── 05-semantic-state.md
    ├── 06-llm-contract.md
    ├── 07-persistence.md
    ├── 08-narrative-pipeline.md
    ├── 10-world-model.md
    ├── 11-engine-invariants.md
    ├── 12-extension-points.md
    ├── 13-validation-rules.md
    ├── 14-development-roadmap.md
    ├── 99-glossary.md
    ├── adr/
    └── architecture/
```

---

## Roadmap de Desarrollo

```
Sprint 0 ──► Sprint 1 ──► Sprint 1.5 ──► Sprint 2 ──► Sprint 3 ──► Sprint 4 ──► Sprint 5
Especif.     Motor Mín.    ECS Cognit.    Sem. Extr.   LLM          Narrativa     Aeris
(FROZEN)     (Tick)        (Determinista) (Transl.)    (Integrac.)  (Pipeline)    (Mundo)
```

| Sprint | Estado | Objetivo |
|--------|--------|----------|
| **Sprint 0** | ✅ Completado | Arquitectura y especificación |
| **Sprint 1** | En progreso | Motor mínimo (ECS, EventBus, Scheduler, Time, Persistencia) |
| **Sprint 1.5** | Pendiente | ECS Cognitivo (memoria, emociones, goals, relaciones) |
| **Sprint 2** | Pendiente | Semantic Extractor (estado → contexto para LLM) |
| **Sprint 3** | Pendiente | Integración LLM |
| **Sprint 4** | Pendiente | Narrative Pipeline |
| **Sprint 5** | Pendiente | Mundo Pokémon completo |

---

## Construcción

```bash
# Build
dotnet build

# Tests
dotnet test

# Benchmarks
dotnet run --project benchmarks/Aeris.Benchmarks -c Release
```

---

## Requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Licencia

[GPL-3.0](LICENSE)
