# Long-Term Memory — Formal Contract

## Inputs
- `WorkingMemoryContent` (for consolidation)
- `AffectSnapshot` (associated with content being consolidated)
- Query (for retrieval)

## Outputs
- Retrieved memories (for Reasoning, Planning, IdentityReconstruction)
- Consolidations (deferred writes)

## Memory Types
- Episodic: events, timestamp, associated affect, significance
- Semantic: facts, beliefs, world knowledge
- Procedural: learned action sequences

## Processes
- Consolidation: WM → LTM during low-load periods
- Reconsolidation: retrieved memories modified with current context
- Forgetting: unused memories lose strength over time
- Reinterpretation: memories updated with new information

## Invariants
- LTM does not read from EventBus directly
- LTM does not execute actions
- Retrieval does not mutate stored memories (reconsolidation is explicit)

## Complexity
- Retrieval: O(log M) where M = total memories (indexed)
- Consolidation: O(C) where C = candidate memories

## Determinism
- Fully deterministic

## Dependencies
- WorkingMemorySystem (output)
- AffectSystem (read-only)

## Forbidden Side Effects
- Writing to WorkingMemory
- Modifying AffectState
- Emitting actions
