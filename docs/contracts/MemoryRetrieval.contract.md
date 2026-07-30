# Memory Retrieval — Formal Contract

**ID**: CONTRACT-MR
**Status**: Active

## Rationale
MemoryRetrieval is a separate subsystem to allow experimenting with different retrieval algorithms (similarity, activation, context) without modifying the reasoner.

## Architecture

```
MemoryRetrievalSystem (orchestrator)
    │
    ▼
IMemoryRetrievalStrategy (plugin)
    │
    ├── LinearScan          ← baseline (first implementation)
    ├── ActivationBased     ← future
    ├── EmbeddingBased      ← future
    └── ...
```

The orchestrator:
- obtains inputs (LTM, WM context, AffectState)
- invokes the strategy
- validates contract invariants
- writes results to WorkingMemory
- records evidence in CausalTrace (separate path from WM update)

## Inputs
- `LongTermMemory` (query target, read-only)
- `WorkingMemoryContent` (current context for relevance scoring)
- `AffectState` (modulation: stress → narrower recall, curiosity → broader recall)

## Outputs
- `RetrievedMemory[]` — memories from LTM scored by relevance
- `ActivationBoost[]` — boosts to specific WM chunks

## Baseline algorithm (LinearScan)

```
1. Candidate generation
   - All non-forgotten memories with Importance > 0.2
2. Contract-defined filtering
   - Remove memories with EffectiveImportance(currentTime) < forgetThreshold
3. Deterministic scoring
   score = EffectiveImportance × 0.4
         + recency × 0.3
         + contextOverlap × 0.2
         + attentionRelevance × 0.1
4. Stable sort by score descending
5. Top-N results (budget from contract or config)
```

Weights are fixed and deterministic. This is the baseline; alternative strategies may use different scoring functions.

## Trace outputs (parallel, separate)

```
RetrievedMemory[]  ─────► WorkingMemory (WM chunks)
RetrievalEvidence  ─────► CausalTrace  (never derived from WM)
```

Cada `RetrievalEvidence` incluye:

```yaml
Trace:
    Operation: Retrieved | RetrievedNone
    MemoryId: uint
    Factors:
        Importance: float
        Recency: float
        ContextOverlap: float
        AttentionRelevance: float
    FinalScore: float
    Strategy: string (strategy name)
```

No es una explicación humana — es una reconstrucción computacional del camino causal.

## ContextOverlap

Inicialmente es una función explícita sin embeddings:

```text
ContextOverlap(chunkA, chunkB) =
    matchCount(tokenize(chunkA.Content), tokenize(chunkB.Content)) / max(lenA, lenB)
```

Donde `tokenize` divide por espacios. Esto es un baseline reemplazable. No introducir embeddings todavía.

## Interfaz de estrategia

```csharp
public interface IMemoryRetrievalStrategy
{
    RetrievalResult Retrieve(MemoryRetrievalContext context);
}
```

Donde `RetrievalResult` contiene únicamente `RetrievedMemory[]` y `RetrievalEvidence[]`. No modifica ECS. La estrategia no sabe nada sobre WorkingMemory ni CausalTrace — solo computa puntuaciones.

## Invariants
- MemoryRetrieval does not modify LongTermMemory
- MemoryRetrieval does not modify AffectState
- All retrievals are traced (query, candidates, selected, why)
- Retrieved memories are loaded into WorkingMemory (not consumed directly by Reasoning)

## Complexity
- O(L × log L) where L = candidate memories in LTM for the entity

## Determinism
- Fully deterministic given same LTM state, WM context, and AffectState

## Dependencies
- LongTermMemorySystem (read-only)
- WorkingMemorySystem (write: add retrieved chunks)
- AffectSystem (read-only)

## Forbidden Side Effects
- Writing to LongTermMemory
- Writing to AffectState
- Executing actions
- Modifying world state
