using System;
using System.Collections.Generic;

namespace Aeris.Engine;

public sealed class AlternativeReasoningStrategy : IReasoningStrategy
{
    public const string RuleId = "alternative-salience-anchor-001";
    public const int RuleVersion = 1;
    public const string Transformation = "SalienceAnchoredInference";
    public const int MaxPremises = 3;

    public ReasoningResult Reason(ReasoningContext context)
    {
        var result = new ReasoningResult();
        var supported = CollectSupportedFacts(context);

        if (supported.Count == 0)
            return result;

        supported.Sort((a, b) =>
        {
            int cmp = b.Weight.CompareTo(a.Weight);
            return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
        });

        int count = Math.Min(MaxPremises, supported.Count);
        var premises = new string[count];
        float weightSum = 0f;
        for (int i = 0; i < count; i++)
        {
            premises[i] = supported[i].Content;
            weightSum += supported[i].Weight;
        }

        float confidence = Math.Clamp(weightSum / count, 0f, 1f);
        float evidenceStrength = Math.Clamp(
            supported[0].Weight * ((float)count / MaxPremises), 0f, 1f);

        var inference = new Inference
        {
            Id = 1,
            RuleId = $"{RuleId}-v{RuleVersion}",
            Transformation = Transformation,
            Conclusion = $"Inference anchored on highest-support evidence ({supported[0].Content})",
            Premises = premises,
            Confidence = confidence
        };

        result.Inferences.Add(inference);
        result.Evidence.Add(new ReasoningEvidence
        {
            InferenceId = inference.Id,
            RuleId = inference.RuleId,
            PremiseCount = premises.Length,
            Transformation = inference.Transformation,
            Confidence = inference.Confidence,
            EvidenceStrength = evidenceStrength,
            Strategy = nameof(AlternativeReasoningStrategy)
        });

        return result;
    }

    private static List<SupportedFact> CollectSupportedFacts(ReasoningContext context)
    {
        var facts = new List<SupportedFact>();
        int index = 0;

        foreach (var chunk in context.WorkingMemory)
        {
            index++;
            if (string.IsNullOrEmpty(chunk.Content))
                continue;
            if (chunk.Salience <= 0f)
                continue;
            facts.Add(new SupportedFact(index, chunk.Content, chunk.Salience));
        }

        foreach (var mem in context.RetrievedMemories)
        {
            index++;
            if (mem.Score <= 0f)
                continue;
            facts.Add(new SupportedFact(index, $"memory_{mem.Memory.Id}_cat_{mem.Memory.Category}", mem.Score));
        }

        return facts;
    }

    private readonly record struct SupportedFact(int Index, string Content, float Weight);
}
