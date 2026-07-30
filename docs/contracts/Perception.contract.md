# Perception — Formal Contract

**ID**: CONTRACT-PERCEPTION
**Status**: Active

## Inputs
- World (ECS): entities, components, spatial relationships
- EventBus: events from current tick
- AgentId: EntityId of the agent

## Outputs
- `Percept[]` — flat list of raw percepts
  - Type: Visual | Auditory | Aura | Proprioceptive
  - Source: EntityId
  - Data: type-specific struct
  - Confidence: float [0, 1]
  - Timestamp: Tick

## Invariants
- PerceptionSystem does not write memory, affect, or goals
- Every percept has confidence > 0
- Total percepts per tick is bounded above
- No semantic interpretation (only sensory filtering)

## Complexity
- O(E) where E = entities in perception range

## Determinism
- Fully deterministic (same world state → same Percept[])

## Dependencies
- World (ECS)
- EventBus

## Forbidden Side Effects
- Writing to any cognitive store
- Modifying world state
- Emitting actions
