# Subsystem Maturity Matrix

**Última actualización**: 2026-08-05

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
| KnowledgeData models | M3 | S1.5 | Tests, CONTRACT-KNOWLEDGE v0.1 | M4: property-based; migración a modelo contractual |
| Emotion system | M3 | S1.5 | Tests | M4 |
| Goal system | M3 | S1.5 | Tests | M4 |
| Relationship system | M3 | S1.5 | Tests | M4 |
| Attention system | M3 | S1.5 | Tests | M4 |

**CONTRACT-KNOWLEDGE v0.1 — estado (2026-08-05)**:

| Campo | Valor |
|-------|-------|
| Estado | Draft |
| Validación | Aprobado metodológicamente |
| Implementación | Pendiente |
| Integración | Pendiente |

**Definición**: subsistema de conocimiento declarado — pipeline (dato observado → evidencia estructurada → patrón identificado → conocimiento candidato → conocimiento aceptado), confianza epistemológica (soporte/consistencia/alcance), campos obligatorios, frontera KNOWLEDGE_BASE/DECISION_POLICY y estados refutado/limitado/deprecado.

**Gaps conocidos**:
- **K-G1 (Gobernanza, doc-18 §2)**: falta Specification en doc-17 (sección Knowledge) + Research Note/Hypothesis. Pendiente de sprint de arquitectura.
- **K-G2 (Implementación)**: inconsistencia entre `KnowledgeData`/`KnowledgeCertainty` (ordinal) y el modelo contractual `KnowledgeEntry` (§4.1). Primer objetivo del sprint de implementación.
- **K-G3 (Capacidad opcional)**: KB→hipótesis debe mantenerse como capacidad opcional; el conocimiento puede informar observación/pregunta de investigación, pero no genera hipótesis automáticamente ni es dependencia del ciclo experimental (CONTRACT-KNOWLEDGE §5.1).

**Siguiente prioridad metodológica**: cerrar EXP-0002 (intercambiabilidad de ReasoningStrategy) antes de abrir el sprint de arquitectura de Knowledge.

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
| MemoryRetrievalSystem | M4 | CONTRACT-MR, S-001–S-010, determinism, reemplazabilidad | M5: cross-sprint regressions |

**Hypothesis**: WorldModel currently stores `EntityId` references. Per ADR-0006 (Self independence from ECS), long-term the WorldModel should represent `ObservedObject { Properties, Relationships, Confidence, LastSeen }` with EntityId as an internal resolution detail. Deferred post-Sprint 3B.

**Closed**: LTM→WM retrieval implemented via MemoryRetrievalSystem (Sprint 3B.1).

### Cognitive Model (Sprint 3B — Planned)

| Subsystem | Level | Evidence | Blocked by |
|-----------|-------|----------|------------|
| ACMA v1 Reasoning | M0 | ACMA-v1.md; EXP-0002 (estrategia reemplazable, RI-001..RI-005) | Sprint 3B.1 |
| ACMA v1 MemoryRetrieval | M0 | ACMA-v1.md; EXP-0003 (estrategia reemplazable, MR-I001..MR-I005) | Sprint 3B.1 |
| ACMA v1 Planning | M0 | ACMA-v1.md; CONTRACT-PLANNING v0.3; EXP-0004 (estrategia reemplazable, P-I001..P-I005) | Sprint 3B.2 |
| ACMA v1 Decision | M0 | ACMA-v1.md; EXP-0004 P-I004 (Decisión desacoplada de la estrategia de planificación) | Sprint 3B.3 |
| ACMA v1 Auditor | M0 | ACMA-v1.md | Sprint 3B.4 |
| ACMA v1 IdentityReconstruction | M0 | ACMA-v1.md | Sprint 3B.5 |
| ACMA v1 SelfSnapshot | M0 | — | Sprint 3B.6 |
| ACMA v1 AffectModel | M0 | ACMA-v1.md | Sprint 3B (merged with Goal) |

**Reasoning — estrategia reemplazable (EXP-0002, validado)**: la propiedad de intercambiabilidad de `IReasoningStrategy` queda validada por RI-001..RI-005 (ReasoningInterchangeabilityTests). Capacidades validadas:
- estrategia reemplazable sin modificar infraestructura (ECS, ReasoningSystem, Planning, Decision, Auditor, Enforcement);
- determinismo preservado (RI-002);
- ausencia de side effects (RI-003);
- localidad causal (RI-004);
- compatibilidad con pipeline downstream Perception→…→Decision (RI-005).

No implica validación completa de Reasoning como modelo cognitivo (ACMA v1); valida la propiedad de reemplazabilidad de la estrategia.

**Memory Retrieval — estrategia reemplazable (EXP-0003, validado)**: la propiedad de intercambiabilidad de `IMemoryRetrievalStrategy` queda validada por MR-I001..MR-I005 (MemoryRetrievalInterchangeabilityTests). Capacidades validadas:
- estrategia reemplazable sin modificar infraestructura (ECS, MemoryRetrievalSystem, WorkingMemory, Reasoning, Planning, Decision, Auditor, Enforcement);
- determinismo preservado (MR-I002);
- ausencia de side effects a nivel estrategia (MR-I003);
- localidad del efecto (MR-I004);
- compatibilidad con pipeline downstream Perception→Attention→WM→LTM→Retrieval→Reasoning→Planning→Decision (MR-I005).

No implica validación completa de Memory Retrieval como modelo cognitivo (ACMA v1); valida la propiedad de reemplazabilidad de la estrategia. Limitación registrada: `MemoryData` no permite modelar frecuencia de acceso (gap documentado en EXP-0003).

**Planning — estrategia reemplazable (EXP-0004, validado)**: la propiedad de intercambiabilidad de `IPlanningStrategy` queda validada por P-I001..P-I005 (PlanningInterchangeabilityTests). Capacidades validadas:
- estrategia reemplazable sin modificar infraestructura (ECS, PlanningSystem, Decision, Auditor, Enforcement);
- determinismo preservado y secuencia de swap replayable (P-I002, P-I005A);
- ausencia de side effects a nivel estrategia sobre WM, WorldModel, Inferencias, Affect y Goals (P-I002B);
- compatibilidad con pipeline completo Perception→…→Enforcement (P-I003);
- Decisión desacoplada: `DecisionSystem` consume planes vía `PlanStore` sin conocer la estrategia que los produjo (P-I004);
- swap en runtime observable en `PlanStore.Evidence` y en el trace causal (P-I005).

No implica validación completa de Planning como modelo cognitivo (ACMA v1); valida la propiedad de reemplazabilidad de la estrategia. El baseline y la alternativa son plantillas sin forward-simulation en WorldModel (escenarios P-003 y P-005..P-007 de 3B-planning son capacidad futura del modelo).

**CONTRACT-PLANNING v0.3 — estado (2026-08-05)**:

| Campo | Valor |
|-------|-------|
| Estado | Draft |
| Validación | Reconciliado con la implementación (auditoría F1–F11; v0.3 refleja el baseline existente) |
| Implementación | Existente (baseline) |
| Integración | Existente (pipeline 3A) |

**Revisiones documentales pendientes** (de la auditoría de CONTRACT-PLANNING, sin cambios de código):
- **S-P007 (Alta)**: el contrato describe que una meta sin ubicación conocida puede producir un plan de exploración y el baseline así lo hace; pendiente reformular la descripción de salida vacía para alinearla al comportamiento sin tocar código.
- **S-P001 / S-P002 (Baja)**: nombres de acciones (`MoveToward<Goal>`, `Interact`) meramente ilustrativos; documentar esa naturaleza.
- **GreedyPlanningStrategy (Media)**: reflejar explícitamente la segunda estrategia en el contrato para trazabilidad de intercambiabilidad.

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
