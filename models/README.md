# Models

Cognitive models are **interchangeable implementations** of the agent architecture. The engine and cognitive infrastructure remain the same; only the model changes.

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

## Structure

```
models/
├── README.md
├── ACMA-v1.md          ← First experimental cognitive model
└── experimental/       ← Proposals, drafts, future models
```

## Registry

| Model | Version | Status | Sprint | Defined |
|-------|---------|--------|--------|---------|
| ACMA v1 | 1.0 | Planned | 3B | `ACMA-v1.md` |
| — | — | — | — | — |
