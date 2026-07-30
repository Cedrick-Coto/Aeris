# Goals — Formal Contract

**ID**: CONTRACT-GOALS
**Status**: Active

## Inputs
- `AffectState`
- Inferences (from Reasoning)
- AutobiographicalMemory
- `WorkingMemoryContent`

## Outputs
- `ActiveGoal[]` — prioritized list of active goals

## Goal Structure
- Type: Exploration | Social | Survival | Knowledge | Protection | ...
- Priority: float [0, 1]
- State: Inactive | Active | Suspended | Completed | Failed
- Progress: float [0, 1]
- Subgoals: Goal[]
- Source: need | event | inference | relationship

## Dynamics
- Goals activate/deactivate based on context
- Priorities modulated by AffectState
- Completed/failed goals → AutobiographicalMemory

## Invariants
- At least one goal is always active
- Sum of priorities does not need to equal 1
- Goals change state only via GoalSystem

## Complexity
- O(G) where G = total goals tracked

## Determinism
- Fully deterministic

## Dependencies
- AffectSystem (output)
- ReasoningSystem (output)
- AutobiographicalMemory (read-only)
- WorkingMemorySystem (read-only)

## Forbidden Side Effects
- Writing to AffectState
- Writing to Memory
- Emitting actions directly
