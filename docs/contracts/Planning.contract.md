# Planning — Formal Contract

## Inputs
- `ActiveGoal` (highest priority)
- `WorldModel`
- `AffectState`
- `LongTermMemory` (procedural)

## Outputs
- `Plan` — ordered sequence of actions

## Processes
- Generation: build plans from possible action space
- Evaluation: simulate each plan in WorldModel (forward)
- Selection: plan with best cost/benefit ratio

## Modulation
- Confidence low → short, conservative plans
- Threat high → risk-avoiding plans
- Curiosity high → exploration-including plans

## Invariants
- PlanningSystem does not execute actions directly
- PlanningSystem does not access the real World ECS
- Plan evaluation uses the internal WorldModel only

## Complexity
- O(P × D) where P = plans considered, D = plan depth (horizon)

## Determinism
- Fully deterministic (same state → same plan)

## Dependencies
- GoalSystem (output)
- WorldModelSystem (read-only)
- AffectSystem (read-only)
- LongTermMemorySystem (read-only, procedural)

## Forbidden Side Effects
- Executing actions
- Modifying WorldModel
- Writing to AffectState
