namespace Aeris.Engine;

public sealed class ContextualSpreadStrategy : IMemoryRetrievalStrategy
{
    private const float ContextHalfLife = 172800f;
    private const float AffectModulationFactor = 0.25f;

    public RetrievalResult Retrieve(MemoryRetrievalContext context)
    {
        var result = new RetrievalResult();
        var candidates = new List<(MemoryData memory, float score, float encoding, float decay, float spread, float affect)>();

        foreach (var memory in context.CandidateMemories)
        {
            if (memory.Forgotten)
                continue;

            float effectiveImportance = memory.EffectiveImportance(context.CurrentTime);
            if (effectiveImportance <= 0.05f)
                continue;

            float encoding = ComputeEncoding(memory);
            float decay = ComputeDecay(memory, context.CurrentTime);
            float spread = ComputeContextSpread(memory, context);
            float affect = ComputeAffectModulation(memory, context);

            float score = Math.Clamp(encoding * decay * (1f + spread + affect), 0f, 1f);
            candidates.Add((memory, score, encoding, decay, spread, affect));
        }

        candidates.Sort((a, b) =>
        {
            int cmp = b.score.CompareTo(a.score);
            return cmp != 0 ? cmp : a.memory.Id.CompareTo(b.memory.Id);
        });

        int count = Math.Min(context.Budget, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            var (mem, score, encoding, decay, spread, affect) = candidates[i];
            result.Memories.Add(new RetrievedMemoryEntry
            {
                Memory = mem,
                Score = score
            });
            result.Evidence.Add(ComputeEvidence(mem, score, encoding, decay, spread, affect));
        }

        return result;
    }

    private static float ComputeEncoding(MemoryData memory)
    {
        float typeWeight = memory.Type switch
        {
            MemoryType.Observed => 1f,
            MemoryType.Experienced => 0.9f,
            MemoryType.Learned => 0.7f,
            MemoryType.Inferred => 0.5f,
            _ => 0f
        };

        return memory.Certainty * typeWeight;
    }

    private static float ComputeDecay(MemoryData memory, float currentTime)
    {
        float age = currentTime - memory.Timestamp;
        return MathF.Exp(-age / ContextHalfLife);
    }

    private static float ComputeContextSpread(MemoryData memory, MemoryRetrievalContext context)
    {
        if (memory.InvolvedEntityId == 0)
            return 0f;

        float spread = 0f;
        foreach (var chunk in context.WorkingMemory.Chunks)
        {
            if (chunk.SourceEntity.HasValue && chunk.SourceEntity.Value.Value == memory.InvolvedEntityId)
                spread += chunk.Salience;
        }

        return spread;
    }

    private static float ComputeAffectModulation(MemoryData memory, MemoryRetrievalContext context)
    {
        float modulation = 0f;
        if (memory.Category == MemoryCategory.Combat && context.AffectState.Threat > 0.5f)
            modulation += context.AffectState.Threat * AffectModulationFactor;
        if (memory.Category == MemoryCategory.Discovery && context.AffectState.Curiosity > 0.5f)
            modulation += context.AffectState.Curiosity * AffectModulationFactor;
        if (memory.Category == MemoryCategory.Social && context.AffectState.Trust > 0.5f)
            modulation += context.AffectState.Trust * AffectModulationFactor;
        return modulation;
    }

    private static RetrievalEvidence ComputeEvidence(
        MemoryData memory, float score, float encoding, float decay, float spread, float affect)
    {
        return new RetrievalEvidence
        {
            Operation = RetrievalOperation.Retrieved,
            MemoryId = memory.Id,
            ImportanceScore = encoding,
            RecencyScore = decay,
            ContextOverlapScore = Math.Clamp(spread, 0f, 1f),
            AttentionRelevanceScore = affect,
            FinalScore = score,
            Strategy = nameof(ContextualSpreadStrategy)
        };
    }
}
