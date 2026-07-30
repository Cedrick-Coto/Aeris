# Subsystem Maturity Matrix

**Última actualización**: 2026-07-30

---

## Maturity Levels

| Level | Label | Meaning |
|-------|-------|---------|
| M0 | Idea | Concept identified, no formalization |
| M1 | Specification | Computational contract defined (doc-17) |
| M2 | Implemented | Code exists in the engine |
| M3 | Tested | Unit + integration tests pass |
| M4 | Validated | Property-based tests, determinism verified, benchmarks exist |
| M5 | Stable | Production-ready, no regressions across sprints |

---

## Subsystem Status

### Engine Core

| Subsystem | Level | Sprint | Evidence | Next |
|-----------|-------|--------|----------|------|
| World (ECS) | M4 | S1 | Tests, determinism, benchmarks | M5: no regressions |
| Entity CRUD | M4 | S1 | Tests, property-based tests | M5 |
| Time System | M4 | S1 | Tests, determinism | M5 |
| EventBus | M4 | S1 | Tests, determinism, benchmarks | M5 |
| Scheduler | M4 | S1 | Tests, benchmarks | M5 |
| SystemManager | M4 | S1 | Tests | M5 |
| Persistence (JSON) | M4 | S1 | Tests, benchmarks | M5 |
| Engine (tick loop) | M4 | S1 | Tests, determinism, benchmarks | M5 |

### Cognitive Layer (Sprint 1.5)

| Subsystem | Level | Sprint | Evidence | Next |
|-----------|-------|--------|----------|------|
| MemoryData models | M3 | S1.5 | Tests | M4: property-based |
| BeliefData models | M3 | S1.5 | Tests | M4 |
| KnowledgeData models | M3 | S1.5 | Tests | M4 |
| Emotion system | M3 | S1.5 | Tests | M4 |
| Goal system | M3 | S1.5 | Tests | M4 |
| Relationship system | M3 | S1.5 | Tests | M4 |
| Attention system | M3 | S1.5 | Tests | M4 |

### Semantic Layer (Sprint 2)

| Subsystem | Level | Sprint | Evidence | Next |
|-----------|-------|--------|----------|------|
| SemanticExtractor | M4 | S2 | Tests, determinism, validation benchmarks | M5 |
| SemanticState | M4 | S2 | Tests, validation | M5 |
| FactNormalizer | M3 | S2 | Tests | M4 |
| SemanticValidator | M3 | S2 | Tests | M4 |

### Cognitive Infrastructure (Sprint 3A)

| Subsystem | Level | Evidence | Next |
|-----------|-------|----------|------|
| PerceptionSystem | M4 | Integration + determinism tests | M5 |
| AttentionSystem | M4 | Integration + determinism tests | M5 |
| WorkingMemorySystem | M4 | Integration + determinism tests | M5 |
| LongTermMemorySystem | M4 | Integration test | M5 |
| AffectSystem (continuous vector) | M4 | Integration + purity tests | M5 |
| GoalSystem (infrastructure) | M4 | Integration + auto-creation tests | M5 |
| WorldModelSystem | M4 | Integration test | M5 |

**Hypothesis**: WorldModel currently stores `EntityId` references. Per ADR-0006 (Self independence from ECS), long-term the WorldModel should represent `ObservedObject { Properties, Relationships, Confidence, LastSeen }` with EntityId as an internal resolution detail. Deferred post-Sprint 3B.

**Gap**: LTM→WM retrieval (memory query/recall) not yet implemented. Required for Sprint 3B Reasoning.

### Cognitive Model (Sprint 3B — Planned)

| Subsystem | Level | Evidence | Blocked by |
|-----------|-------|----------|------------|
| ACMA v1 Reasoning | M0 | ACMA-v1.md | Sprint 3B.1 |
| ACMA v1 Planning | M0 | ACMA-v1.md | Sprint 3B.2 |
| ACMA v1 Decision | M0 | ACMA-v1.md | Sprint 3B.3 |
| ACMA v1 Auditor | M0 | ACMA-v1.md | Sprint 3B.4 |
| ACMA v1 IdentityReconstruction | M0 | ACMA-v1.md | Sprint 3B.5 |
| ACMA v1 SelfSnapshot | M0 | — | Sprint 3B.6 |
| ACMA v1 AffectModel | M0 | ACMA-v1.md | Sprint 3B (merged with Goal) |

### Observability (Sprint 3C — Planned)

| Subsystem | Level | Evidence | Blocked by |
|-----------|-------|----------|------------|
| SelfSnapshot Inspector | M0 | — | Sprint 3B |
| AffectState Visualizer | M0 | — | Sprint 3B |
| Causal Decision Trace | M0 | — | Sprint 3B |

### LLM & Narrative (Sprints 4-5 — Planned)

| Subsystem | Level | Evidence | Blocked by |
|-----------|-------|----------|------------|
| LLM Integration | M0 | — | Sprint 3C |
| Narrative Pipeline | M0 | — | Sprint 4 |

---

## Coverage Summary

| Layer | M0 | M1 | M2 | M3 | M4 | M5 |
|-------|----|----|----|----|----|----|
| Engine Core | 0 | 0 | 0 | 0 | 8 | 0 |
| Cognitive Layer | 0 | 0 | 0 | 7 | 0 | 0 |
| Semantic Layer | 0 | 0 | 0 | 2 | 2 | 0 |
| Cognitive Infrastructure | 0 | 0 | 0 | 0 | 7 | 0 |
| Cognitive Model | 6 | 0 | 0 | 0 | 0 | 0 |
| Observability | 3 | 0 | 0 | 0 | 0 | 0 |
| LLM & Narrative | 2 | 0 | 0 | 0 | 0 | 0 |
| **Total** | **11** | **0** | **0** | **9** | **17** | **0** |
