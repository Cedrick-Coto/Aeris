# Affect — Formal Contract

## Inputs
- `Percept[]` (attended)
- `WorkingMemoryContent`
- Active Goals
- Previous `AffectState` (homeostasis)

## Outputs
- `AffectState` (new)

## Variables (continuous vector)
- Variable set is defined by the active Cognitive Model
- Each variable has range [0, 1], a baseline, and a return speed

## Invariants
- AffectSystem does not produce text, emotional labels, or actions
- AffectSystem only updates the AffectState vector
- No other system writes to AffectState
- Return to baseline is deterministic

## Complexity
- O(V) where V = number of affect variables

## Determinism
- Fully deterministic

## Dependencies
- AttentionSystem (output)
- WorkingMemorySystem (output, read-only)
- GoalSystem (read-only)

## Forbidden Side Effects
- Writing to Goals
- Writing to Memory
- Emitting actions
- Producing text or labels
