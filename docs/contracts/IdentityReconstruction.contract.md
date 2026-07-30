# Identity Reconstruction — Formal Contract

**ID**: CONTRACT-IDENTITY
**Status**: Draft

## Inputs
- `AutobiographicalMemory` (significant episodes)
- `LongTermMemory` (beliefs, principles)
- `ActiveGoals`
- `AffectState` (current)
- `Relationships` (active)
- `RecentReflections` (from AuditorSystem)
- `WorkingMemoryContent`

## Outputs
- `SelfSnapshot` — integrated self representation, exists only during the tick

## Snapshot Structure
- NarrativeSummary: string
- ActivePrinciples: Principle[]
- PerceivedCapabilities: Capability[]
- SignificantRelationships: Relationship[]
- CurrentPriorities: string[]
- SelfSummary: string (integration of:
    "My goals: ...
     My memories: ...
     My relationships: ...
     My principles: ...
     My past decisions: ...
     My world model: ...")
- CoherenceScore: float [0, 1]

## Rules
- SelfSnapshot is built from scratch every tick
- No `SelfComponent` exists in the ECS
- SelfSnapshot lasts only for the duration of the tick
- If no system queries SelfSnapshot in a tick, it is not built (optimization)

## Complexity
- O(M + R + G) where M = memories, R = relationships, G = goals

## Determinism
- Fully deterministic

## Dependencies
- LongTermMemorySystem (read-only)
- GoalSystem (read-only)
- AffectSystem (read-only)
- RelationshipSystem (read-only)
- AuditorSystem (read-only)
- WorkingMemorySystem (read-only)

## Forbidden Side Effects
- Writing to any cognitive store
- Persisting SelfSnapshot between ticks
- Modifying world state
