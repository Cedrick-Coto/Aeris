# World Model — Formal Contract

**ID**: CONTRACT-WORLDMODEL
**Status**: Active

## Inputs
- `Percept[]` (attended, historical)
- Inferences (from Reasoning)
- `LongTermMemory` (world knowledge)

## Outputs
- `WorldModelState` — internal partial representation of the world

## Properties
- Partial: the agent does not know the entire world
- Probabilistic: includes uncertainty about known facts
- Updated through perception and inference
- Contains: mental map, causal relations, theory of other agents

## Invariants
- WorldModel is a separate entity from the World ECS
- The LLM never accesses WorldModel directly
- WorldModel is not serialized as part of the ECS state

## Complexity
- O(K) where K = known world elements

## Determinism
- Fully deterministic

## Dependencies
- PerceptionSystem (historical output)
- ReasoningSystem (output)
- LongTermMemorySystem (read-only)

## Forbidden Side Effects
- Modifying the real World ECS
- Exposing internal state to LLM
- Writing to cognitive stores
