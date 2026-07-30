# Decision — Formal Contract

## Inputs
- `Plan` (from PlanningSystem)
- `AffectState`
- `WorldModel` (current simulated state)
- `WorkingMemoryContent`

## Outputs
- `Action` — next action to execute

## Action Structure
- Type: Move | Interact | Communicate | Observe | Wait | ...
- Target: EntityId | Position | null
- Parameters: Dictionary<string, float>
- Confidence: float [0, 1]
- Tick: long

## Algorithm
1. Evaluate if plan is still valid (current state vs expected)
2. If valid → extract next step
3. If invalid → re-plan or default reactive action
4. Emit Action as EventBus event

## Modulation
- Stress high → faster, less optimal decisions
- Confidence low → delay, hesitation

## Invariants
- Every Action must be translatable to an EventBus event
- DecisionSystem does not modify the world directly

## Complexity
- O(1) per tick (single decision)

## Determinism
- Fully deterministic

## Dependencies
- PlanningSystem (output)
- AffectSystem (read-only)
- WorldModelSystem (read-only)
- WorkingMemorySystem (read-only)

## Forbidden Side Effects
- Modifying world state directly
- Writing to cognitive stores
