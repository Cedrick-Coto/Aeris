# Aeris

**Motor de narrativa emergente basado en simulación para un mundo Pokémon.**

Aeris no es un chatbot ni un juego con guion. Es un **simulador de mundo** donde la historia emerge como consecuencia de la simulación, no como producto de un prompt.

---

## Principios Arquitectónicos

- **ECS + Data-Oriented Design** como base del motor.
- **Determinismo** como requisito del núcleo de simulación.
- **Separación estricta** entre simulación, cognición, afecto y narrativa.
- El **LLM nunca modifica el estado del mundo**; únicamente interpreta y expresa el estado interno.
- El **Self** no es un componente explícito: emerge de la integración persistente de memoria, cognición, afecto, relaciones y autobiografía.
- El proyecto implementa un **modelo funcional de agencia**, no afirma reproducir la conciencia humana.

---

## Arquitectura

```
                  Mundo
                    │
              Simulación ECS
                    │
      ┌─────────────┴─────────────┐
      │                           │
 Cognición                  Afecto
      │                           │
      └─────────────┬─────────────┘
                    │
         Modelo Emergente del Self
                    │
          Semantic Extractor
                    │
            Prompt Builder
                    │
                  LLM
                    │
          Narrativa / Diálogo
```

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
Sprint 0 ──► Sprint 1 ──► Sprint 2 ──► Sprint 3 ──► Sprint 4 ──► Sprint 5 ──► Sprint 6 ──► Sprint 7 ──► Sprint 8
Arquitec.    Motor ECS    Cognición   Afecto       Self          LLM          Narrativa    Mundo Pokémon  Aeris
(FROZEN)     (COMPL.)     (Determ.)   (Sist. Transv.) (Emergente)  (Verbaliz.)  (Pipeline)   (Modelado)     (Personaje)
```

| Sprint   | Estado     | Objetivo                                                                                       |
| -------- | ---------- | ---------------------------------------------------------------------------------------------- |
| Sprint 0 | ✅ Frozen   | Especificación arquitectónica y ADRs                                                           |
| Sprint 1 | ✅ Complete | Motor ECS determinista (World, Systems, EventBus, Scheduler, Persistence)                      |
| Sprint 2 | ⏳ Planned  | Arquitectura Cognitiva (Percepción, Atención, Memoria, Creencias, Razonamiento, Planificación) |
| Sprint 3 | ⏳ Planned  | Arquitectura Afectiva (Emoción, Motivación, Necesidades, Apego, Regulación)                    |
| Sprint 4 | ⏳ Planned  | Modelo Emergente del Self (Autobiografía, Reflexión, Meta-Reflexión, Identidad emergente)      |
| Sprint 5 | ⏳ Planned  | Integración LLM (Semantic Extractor → Prompt Builder → LLM)                                    |
| Sprint 6 | ⏳ Planned  | Narrativa (Diálogo, Monólogo interno, Narración contextual)                                    |
| Sprint 7 | ⏳ Planned  | Mundo Pokémon (Biología, Aura, Ecosistemas, Cultura, Lenguaje)                                 |
| Sprint 8 | ⏳ Planned  | Aeris (Personaje completo e integración final)                                                 |

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
