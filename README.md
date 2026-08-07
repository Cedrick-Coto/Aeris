# Aeris

**Deterministic cognitive simulation engine with emergent narrative for a Pokémon world.**

[![CI](https://github.com/Cedrick-Coto/Aeris/actions/workflows/ci.yml/badge.svg)](https://github.com/Cedrick-Coto/Aeris/actions/workflows/ci.yml)
[![Determinism](https://github.com/Cedrick-Coto/Aeris/actions/workflows/determinism.yml/badge.svg)](https://github.com/Cedrick-Coto/Aeris/actions/workflows/determinism.yml)
[![License: GPL-3.0](https://img.shields.io/badge/License-GPL--3.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)

Aeris is not a chatbot or a scripted game. It's a **world simulator** where story emerges as a consequence of simulation, not as a product of a prompt. The LLM does not think: it verbalizes [...]

---

## Architectural Principles

- **ECS + Data-Oriented Design** as the engine foundation.
- **Determinism** as a requirement of the simulation core.
- **Strict separation** between simulation, cognition, affect, and narrative.
- The **LLM never modifies world state**; it only interprets and expresses internal state.
- **Self is not a component**: it is reconstructed each tick as `SelfSnapshot`.
- **Affect is a continuous vector** (curiosity, stress, trust, etc.), not discrete emotions.
- **ACMA is an experimental, interchangeable cognitive module** (v1, v2, ...).
- The project implements a **functional model of agency**, not a claim to reproduce human consciousness.

### Decision Stability

| Level | Scope | Expected Changes |
|-------|--------|-------------------|
| A0 | Epistemological principles | Exceptional |
| A1 | Cognitive axioms | Rare |
| A2 | Engine architecture | Infrequent |
| A3 | Platform and implementation | Frequent |

See [ADR hierarchy](docs/adr/README.md) for the complete classification.

---

## Architecture

```
                         ECS World
                            │
                      Simulation Tick
                            │
                     Semantic Extractor
                            │
                   ┌────────┴────────┐
                   │ Cognitive Infra.│
                   │ (mechanisms)    │
                   │ Perception      │
                   │ Attention       │
                   │ Memory          │
                   │ Affect (vector) │
                   │ Goals           │
                   └────────┬────────┘
                            │
                   ┌────────┴────────┐
                   │ Cognitive Model │
                   │ (theory)        │
                   │ ┌─ ACMA v1 ───┐ │
                   │ │ Reasoning   │ │
                   │ │ Planning    │ │
                   │ │ Decision    │ │
                   │ │ Auditor     │ │
                   │ │ Identity    │ │
                   │ │ World Model │ │
                   │ └─────────────┘ │
                   └────────┬────────┘
                            │
                     SelfSnapshot
                  (exists only this tick)
                            │
                   ┌────────┴────────┐
                   │ Narrative       │
                   │ Pipeline        │
                   └────────┬────────┘
                            │
                          LLM
                            │
                  Narrative / Dialogue
```

The LLM operates at the **determinism/probabilism boundary**: it receives deterministic `SelfSnapshot` + `SemanticState` and produces probabilistic narrative. It never modifies the agent's internal state [...]

**Cognitive Infrastructure** provides general mechanisms (perception, attention, memory, affect vectorization, goals). **Cognitive Model** (ACMA v1, v2, ...) implements a cognitive theory [...]

For a detailed description of each subsystem: [`docs/16-agent-architecture.md`](docs/16-agent-architecture.md).

---

## Technology Stack

| Component | Technology |
|------------|------------|
| Language | C# (.NET 10.0) |
| Paradigm | ECS (Entity Component System) + Data-Oriented Design |
| ECS Library | [Arch](https://github.com/genaray/Arch) |
| Tests | xUnit + FsCheck (property-based) + FluentAssertions |
| Persistence | SQLite + JSON |
| LLM | Provider-agnostic (OpenAI, Claude, Ollama, etc.) |

---

## Development Roadmap

```
Sprint 0 ──► Sprint 1 ──► Sprint 2 ──► Sprint 3A ──► Sprint 3B ──► Sprint 3C ──► Sprint 4 ──► Sprint 5 ──► Sprint 6 ──► Sprint 7
Architec.    ECS Motor    Sem. Extr.   Cognitive   ACMA v1      Observa-     LLM          Narrative    Pokémon     Aeris
(FROZEN)     (COMPL.)     (COMPL.)     Infra.      (In progress)bility       (Verbaliz.)  (Pipeline)   (Modeling)  (Character)
                                       (COMPL.)                  (Planned)    (Planned)    (Planned)    (Planned)   (Planned)
```

| Sprint   | Status        | Goal                                                                                       |
| -------- | ------------- | ---------------------------------------------------------------------------------------------- |
| Sprint 0 | ✅ Frozen      | Architectural specification and ADRs                                                           |
| Sprint 1 | ✅ Complete    | Deterministic ECS engine (World, Systems, EventBus, Scheduler, Persistence)                      |
| Sprint 2 | ✅ Complete    | Semantic Extractor (extract world state → SemanticState for LLM)                              |
| Sprint 3A| ✅ Complete    | Cognitive infrastructure (7 deterministic ECS systems, 210 tests)                            |
| Sprint 3B| 🔄 In progress | ACMA v1 — 3B.1 Memory Retrieval ✅, 3B.2 Reasoning ✅, 3B.3 Planning ✅ (baseline + EXP-0004), 3B.4–3B.6 pending |
| Sprint 3C| ⏳ Planned     | Observability (SelfSnapshot Inspector, Decision Trace, Reason Trace, etc.)                    |
| Sprint 4 | ⏳ Planned     | LLM integration (verbalizer, not thinker)                                                    |
| Sprint 5 | ⏳ Planned     | Narrative (Dialogue, Internal Monologue, Contextual Narration)                                   |
| Sprint 6 | ⏳ Planned     | Pokémon World (Biology, Aura, Ecosystems, Culture, Language, Factions)                      |
| Sprint 7 | ⏳ Planned     | Aeris (Complete character and final integration)                                                 |

---

## How to Contribute

1. Read [`CONTRIBUTING.md`](CONTRIBUTING.md)
2. Review [`docs/adr/`](docs/adr/) to understand architectural decisions
3. Explore [`docs/hypotheses/`](docs/hypotheses/) to see active research hypotheses
4. Open an issue or submit a PR

All contributions must respect the 6 architectural principles (determinism, causal pressure, traceability, computational contract, causal locality, affective modulation).

---

## Build

```bash
# Clone
git clone https://github.com/Cedrick-Coto/Aeris

# Build
dotnet build

# Tests
dotnet test

# Benchmarks
dotnet run --project benchmarks/Aeris.Benchmarks -c Release
```

**Requires**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## License

[GPL-3.0](LICENSE). See [`SECURITY.md`](SECURITY.md) to report vulnerabilities.
