namespace Aeris.Engine;

public sealed class LinearScanStrategy : IMemoryRetrievalStrategy
{
    public RetrievalResult Retrieve(MemoryRetrievalContext context)
    {
        var result = new RetrievalResult();
        var candidates = new List<(MemoryData memory, float score)>();

        foreach (var memory in context.CandidateMemories)
        {
            if (memory.Forgotten)
                continue;

            float effectiveImportance = memory.EffectiveImportance(context.CurrentTime);
            if (effectiveImportance <= 0.05f)
                continue;

            float recency = ComputeRecency(memory, context.CurrentTime);
            float contextOverlap = ComputeContextOverlap(memory, context);
            float attentionRelevance = ComputeAttentionRelevance(memory, context);

            float score = effectiveImportance * 0.4f
                        + recency * 0.3f
                        + contextOverlap * 0.2f
                        + attentionRelevance * 0.1f;

            candidates.Add((memory, score));
        }

        candidates.Sort((a, b) =>
        {
            int cmp = b.score.CompareTo(a.score);
            return cmp != 0 ? cmp : a.memory.Id.CompareTo(b.memory.Id);
        });

        int count = Math.Min(context.Budget, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            var (mem, score) = candidates[i];
            result.Memories.Add(new RetrievedMemoryEntry
            {
                Memory = mem,
                Score = score
            });
            result.Evidence.Add(ComputeEvidence(mem, score, context));
        }

        return result;
    }

    private static float ComputeRecency(MemoryData memory, float currentTime)
    {
        float age = currentTime - memory.Timestamp;
        return Math.Clamp(1f - age / 86400f, 0f, 1f);
    }

    private static float ComputeContextOverlap(MemoryData memory, MemoryRetrievalContext context)
    {
        if (memory.InvolvedEntityId == 0)
            return 0f;

        foreach (var chunk in context.WorkingMemory.Chunks)
        {
            if (chunk.SourceEntity.HasValue && chunk.SourceEntity.Value.Value == memory.InvolvedEntityId)
                return 1f;
        }

        return 0f;
    }

    private static float ComputeAttentionRelevance(MemoryData memory, MemoryRetrievalContext context)
    {
        float relevance = 0f;
        if (memory.Category == MemoryCategory.Combat && context.AffectState.Threat > 0.5f)
            relevance = Math.Max(relevance, context.AffectState.Threat);
        if (memory.Category == MemoryCategory.Discovery && context.AffectState.Curiosity > 0.5f)
            relevance = Math.Max(relevance, context.AffectState.Curiosity);
        return relevance;
    }

    private static RetrievalEvidence ComputeEvidence(MemoryData memory, float score, MemoryRetrievalContext context)
    {
        float effectiveImportance = memory.EffectiveImportance(context.CurrentTime);
        float recency = ComputeRecency(memory, context.CurrentTime);
        float contextOverlap = ComputeContextOverlap(memory, context);
        float attentionRelevance = ComputeAttentionRelevance(memory, context);

        return new RetrievalEvidence
        {
            Operation = RetrievalOperation.Retrieved,
            MemoryId = memory.Id,
            ImportanceScore = effectiveImportance,
            RecencyScore = recency,
            ContextOverlapScore = contextOverlap,
            AttentionRelevanceScore = attentionRelevance,
            FinalScore = score,
            Strategy = nameof(LinearScanStrategy)
        };
    }
}
