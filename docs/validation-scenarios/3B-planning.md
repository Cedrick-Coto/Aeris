# Validation Scenarios — 3B.3 Planning

**Status**: Detailed (baseline implementado y validado; evidencia EXP-0004)
**ID prefix**: P-

```
Implements:
- CONTRACT-PLANNING (v0.3, reconciliado 2026-08-05)

Derived from:
- SPEC-3B.3 (doc-17 §Planning)

Supports:
- H-0006 (Horizonte de planificación truncado)

Background:
- RN-0006 (Planificación en Agentes Cognitivos)

Evidence:
- EXP-0004 (intercambiabilidad de estrategia, P-I001..P-I005)
- tests/Aeris.Engine.Tests/PlanningTests.cs (S_P001..S_P010)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| P-001 | Plan generation from active goal | Goal activo → se genera un plan |
| P-002 | No goal → no plan | Sin goal activo → no se genera plan |
| P-003 | Forward simulation in WorldModel | Plan se evalúa simulando en WorldModel |
| P-004 | Cost/benefit selection | Múltiples planes → seleccionar el mejor |
| P-005 | Affect modulation — Confidence bajo | Planes cortos (menos pasos) |
| P-006 | Affect modulation — Threat alto | Planes conservadores (evitar riesgo) |
| P-007 | Affect modulation — Curiosity alto | Planes exploratorios (rutas alternativas) |
| P-008 | Horizonte truncado | Plan no supera H-0006 max steps |
| P-009 | Trace logging | Plan generado registrado en CognitiveTraceLog |
| P-010 | No side effects | Planning no modifica WM, LTM, Affect, Goals, WorldModel |
| P-011 | Determinism | Mismo goal + estado → mismo plan |

## Estado de validación (baseline implementado, Sprint 3B.3)

| ID | Estado | Evidencia | Nota |
|----|--------|-----------|------|
| P-001 | Validado | `S_P001_PlanForReachableGoal`, `S_P001_PlanContainsSteps`, P-I001 | goal activo → plan con pasos no vacíos |
| P-002 | Validado | `S_P005_NoGoalsReturnsEmpty`, `InactiveGoalsIgnored` | sin goal activo → `PlanStore` vacío |
| P-003 | No cubierto (capacidad futura) | — | el baseline evalúa feasibility por plantillas/WorldModel, no simula hacia adelante |
| P-004 | Parcial (selección en Decision) | `S_P006_MultipleCandidatesNoSelection`, P-I004 | Planning genera candidatos; la selección la hace `DecisionSystem` (`FeasibilityThresholdPolicy`), desacoplado de la estrategia |
| P-005 | No cubierto (estructura) | `S_P004_AffectModulatesPreferenceNotFeasibility` | el afecto modula `Preference` (curiosity/threat), no el número de pasos |
| P-006 | No cubierto (estructura) | `S_P004` | `threatPenalty` solo en `Preference`; no hay planes conservadores estructurales |
| P-007 | No cubierto (rutas alternativas) | `S_P004` | `curiosityBonus` solo en `Preference`; sin rutas alternativas HTN/MCTS |
| P-008 | Satisfecho de facto | plantillas ≤ 3 pasos | no existe constante H-0006 explícita; el horizonte está implícito en las plantillas |
| P-009 | Validado | `PlanningSystem_ExecutesInCausalChain`, P-I003 | `PlanningSystem` registra `Strategy=<nombre>` en `CognitiveTraceLog` |
| P-010 | Validado | `S_P009_NoSideEffects`, P-I002B | `Plan()` no muta WM, WorldModel, Inferencias, Affect ni Goals |
| P-011 | Validado | `S_P008_Determinism`, P-I002A, P-I005A | misma entrada + misma estrategia → resultado idéntico (incl. secuencia de swap) |

**Intercambiabilidad (EXP-0004)**: la propiedad de reemplazabilidad de `IPlanningStrategy` queda validada por P-I001..P-I005 (véase `docs/experiments/EXP-0004.md`): ambas estrategias (`GoalDirectedPlanningStrategy`, `GreedyPlanningStrategy`) cumplen el contrato común, son deterministas, no producen side effects, completan el pipeline Perception→…→Enforcement, mantienen a Decision desacoplada y permiten swap en runtime.

## Failure modes (planificación)

- **Plan inválido**: `GoalId` que no referencia a un goal activo, `Steps` vacío, `ExpectedOutcome` vacío o métricas fuera de [0,1] → detectado por P-I001.
- **Evidencia inválida**: `Strategy` vacío, `StepCount == 0` o `PlanId == 0` → detectado por P-I001.
- **Side effects**: `Plan()` muta WM, WorldModel, Inferencias, Affect o Goals → detectado por P-I002B.
- **Decision acoplada**: `DecisionSystem` depende de la estrategia de planificación (p. ej. filtra por nombre) → detectado por P-I004.
- **Swap no observable**: cambiar `PlanningSystem.Strategy` entre ticks no se refleja en `PlanStore.Evidence` ni en el trace → detectado por P-I005.
- **No determinismo**: misma entrada + misma estrategia → salida distinta → detectado por P-I002A/P-I005A y la suite de determinismo CI.

## Revisiones documentales pendientes (sin cambios de código)

- **S-P007 (Alta)**: el contrato (§S-P007) describe salida vacía para una meta sin ubicación conocida, pero el baseline produce un plan de exploración (`S_P002`); reformular la descripción documental para alinearla al comportamiento.
- **S-P001 / S-P002 (Baja)**: nombres de acciones (`MoveToward<Goal>`, `Interact`) meramente ilustrativos; documentar esa naturaleza.
- **GreedyPlanningStrategy (Media)**: reflejar explícitamente la segunda estrategia en el contrato para trazabilidad de intercambiabilidad.
