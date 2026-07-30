# Reasoning — Formal Contract

**ID**: CONTRACT-REASONING
**Status**: Draft

## Interface

```
Inputs
  - WorkingMemory       (current context, read-only)
  - WorldModel          (known entities, relationships, read-only)
  - Goals               (active goals, priorities, read-only)
  - AffectState         (continuous vector, read-only)

Outputs
  - Inferences[]        (new derived facts by type)
  - Predictions[]       (expected future states)
  - CandidateActions[]  (proposed actions for Planning)

Invariants
  - ReasoningSystem does NOT modify long-term memory
  - ReasoningSystem does NOT modify WorldModel
  - ReasoningSystem does NOT write to AffectState
  - ReasoningSystem does NOT execute actions
  - Fully deterministic
```

## Inference Types
- **Causal**: "event A → probably event B"
- **Deductive**: "all X are Y, this is X → this is Y"
- **Abductive**: "observed effect → possible cause"
- **Analogical**: "similar situation → same solution"

## Modulation
- Confidence high → bolder inferences
- Threat high → threat bias
- Stress high → simpler, faster inferences
- Curiosity high → broader hypothesis generation

## Complexity
- O(I × R) where I = input items, R = reasoning rules applied

## Determinism
- Fully deterministic

## Dependencies
- MemoryRetrievalSystem (output: retrieved memories → WM)
- WorkingMemorySystem (read)
- WorldModelSystem (read-only)
- GoalSystem (read-only)
- AffectSystem (read-only)

## Forbidden Side Effects
- Writing to AffectState
- Writing to LongTermMemory
- Modifying WorldModel
- Executing actions
- Modifying world state
