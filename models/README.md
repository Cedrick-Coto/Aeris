# Models

Cognitive models are **interchangeable implementations** of the agent architecture. All models share the same infrastructure and contracts; only the theory changes.

```
Infrastructure (ECS, Engine, Persistence)
    ↓
Cognitive Contracts (doc-17, docs/contracts/*)
    ↓
Cognitive Model (e.g. ACMA v1, MinimalAgent)
    ↓
Experiments (hypotheses → scenarios → observation)
```

## Structure

```
models/
├── README.md
├── ACMA-v1.md              ← First experimental cognitive model
├── ACMA-v2.md              ← Future iteration
├── MinimalAgent.md         ← Minimal baseline (perception → action)
├── ReactiveAgent.md        ← No reasoning, no planning
├── DevelopmentalAgent.md   ← Ontogenetic learning model
└── experimental/           ← Drafts, proposals, variants
```

## Registry

| Model | Version | Status | Sprint | Defined |
|-------|---------|--------|--------|---------|
| ACMA v1 | 1.1 | Planned | 3B | `ACMA-v1.md` |
| — | — | — | — | — |

## Terminology

This project implements **instances of cognitive models**, not "the model."

ACMA v1 is not "the" correct architecture. It is one concrete implementation of the contracts, subject to experimental validation. Results will determine whether it is kept, modified, partially replaced, or discarded.

All models can be compared on the same scenarios without touching the engine:

```
Scenario S-031
    ↓
ACMA v1      → Result A
MinimalAgent → Result B
    ↓
Comparison (engine unchanged)
```
