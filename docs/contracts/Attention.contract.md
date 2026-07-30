# Attention — Formal Contract

## Inputs
- `Percept[]` (from PerceptionSystem)
- `AffectState` (from previous tick, or baselines if tick 1)
- Active Goals

## Outputs
- `Percept[]` — filtered subset, ordered by salience

## Invariants
- Attention budget (N) is fixed and configurable
- AttentionSystem does not modify AffectState
- Total output ≤ total input
- Same input + same affect + same goals → same output

## Algorithm
- Salience(p) = novelty(p) × relevance(p, goals) × affectModulation(p, affect)
- Select top N percepts by salience
- Rest are discarded or degraded

## Complexity
- O(P) where P = Percept[] length

## Determinism
- Fully deterministic

## Dependencies
- PerceptionSystem (output)
- AffectState (read-only)
- GoalSystem (read-only)

## Forbidden Side Effects
- Writing to AffectState
- Writing to Goals
- Modifying world state
