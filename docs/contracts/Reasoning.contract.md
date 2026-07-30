# Reasoning — Formal Contract

## Inputs
- `WorkingMemoryContent`
- `AffectState`
- `LongTermMemory` (facts, beliefs)
- `WorldModel`

## Outputs
- `Inference[]` — new inferences
- `BeliefChange[]` — belief updates

## Inference Types
- Causal: "event A → probably event B"
- Deductive: "all X are Y, this is X → this is Y"
- Abductive: "observed effect → possible cause"
- Analogical: "similar situation → same solution"

## Modulation
- Confidence high → bolder inferences
- Threat high → threat bias
- Stress high → simpler, faster inferences

## Invariants
- ReasoningSystem does not execute actions
- ReasoningSystem does not write to AffectState
- All inferences are traced (inputs, rule applied, output)

## Complexity
- O(I × R) where I = input items, R = reasoning rules applied

## Determinism
- Fully deterministic

## Dependencies
- WorkingMemorySystem (output)
- AffectSystem (output)
- LongTermMemorySystem (read-only)
- WorldModelSystem (read-only)

## Forbidden Side Effects
- Writing to AffectState
- Executing actions
- Modifying world state
