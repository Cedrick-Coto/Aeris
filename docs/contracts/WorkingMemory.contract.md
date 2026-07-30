# Working Memory — Formal Contract

## Inputs
- `Percept[]` attended (from AttentionSystem)
- `AffectState`
- Previous WorkingMemory content (for decay)

## Outputs
- `WorkingMemoryContent` — updated working memory state

## Invariants
- Max capacity: N chunks (configurable)
- Each chunk has: data, timestamp, salience, decayRate
- Chunks below decay threshold are discarded
- WM does not write to LTM directly
- WM does not modify AffectState

## Properties
- Decay: unchecked chunks lose salience each tick
- Refresh: re-attention resets chunk decay timer

## Complexity
- O(C) where C = current chunk count

## Determinism
- Fully deterministic

## Dependencies
- AttentionSystem (output)
- AffectState (read-only)

## Forbidden Side Effects
- Writing to LongTermMemory
- Modifying AffectState
- Emitting actions
