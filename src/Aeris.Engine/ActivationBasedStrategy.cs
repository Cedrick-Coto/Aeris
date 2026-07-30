namespace Aeris.Engine;

public sealed class ActivationBasedStrategy : IMemoryRetrievalStrategy
{
    public RetrievalResult Retrieve(MemoryRetrievalContext context)
    {
        var result = new RetrievalResult();
        var candidates = new List<(MemoryData memory, float activation)>();

        foreach (var memory in context.CandidateMemories)
        {
            if (memory.Forgotten)
                continue;

            float effectiveImportance = memory.EffectiveImportance(context.CurrentTime);
            if (effectiveImportance <= 0.05f)
                continue;

            float activation = ComputeActivation(memory, context);
            candidates.Add((memory, activation));
        }

        candidates.Sort((a, b) =>
        {
            int cmp = b.activation.CompareTo(a.activation);
            return cmp != 0 ? cmp : a.memory.Id.CompareTo(b.memory.Id);
        });

        int count = Math.Min(context.Budget, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            var (mem, activation) = candidates[i];
            result.Memories.Add(new RetrievedMemoryEntry
            {
                Memory = mem,
                Score = activation
            });
            result.Evidence.Add(ComputeEvidence(mem, activation, context));
        }

        return result;
    }

    private static float ComputeActivation(MemoryData memory, MemoryRetrievalContext context)
    {
        float activation = memory.Importance;

        float age = context.CurrentTime - memory.Timestamp;
        float decayFactor = MathF.Exp(-age * 0.00005f);
        activation *= decayFactor;

        foreach (var chunk in context.WorkingMemory.Chunks)
        {
            if (chunk.SourceEntity.HasValue && chunk.SourceEntity.Value.Value == memory.InvolvedEntityId)
            {
                activation += 0.3f;
                break;
            }
        }

        if (memory.Category == MemoryCategory.Combat && context.AffectState.Threat > 0.5f)
            activation += 0.2f;
        if (memory.Category == MemoryCategory.Discovery && context.AffectState.Curiosity > 0.5f)
            activation += 0.2f;
        if (memory.Category == MemoryCategory.Social && context.AffectState.Trust > 0.5f)
            activation += 0.15f;

        return Math.Clamp(activation, 0f, 1f);
    }

    private static RetrievalEvidence ComputeEvidence(MemoryData memory, float activation, MemoryRetrievalContext context)
    {
        float decayFactor = MathF.Exp(-(context.CurrentTime - memory.Timestamp) * 0.00005f);

        return new RetrievalEvidence
        {
            Operation = RetrievalOperation.Retrieved,
            MemoryId = memory.Id,
            ImportanceScore = memory.Importance,
            RecencyScore = decayFactor,
            ContextOverlapScore = activation > memory.Importance ? 0.3f : 0f,
            AttentionRelevanceScore = 0f,
            FinalScore = activation,
            Strategy = nameof(ActivationBasedStrategy)
        };
    }
}
