namespace Aeris.Engine;

public sealed class EvidenceBasedReasoningStrategy : IReasoningStrategy
{
    private static readonly List<ReasoningRule> Rules = new()
    {
        new ReasoningRule
        {
            RuleId = "evidence-based-001",
            Version = 1,
            Label = "EvidenceBasedInference",
            Description = "Si hay evidencia suficiente en la memoria de trabajo, inferir una conclusión basada en la evidencia acumulada",
            MinPremises = 1,
            MaxPremises = 5,
            BaseWeight = 0.7f,
            PremiseMatcher = facts =>
            {
                // Lógica de coincidencia de premisas para EvidenceBasedReasoningStrategy
                return facts.Length >= 1;
            },
            InferenceBuilder = facts =>
            {
                // Construir conclusión basada en evidencia acumulada
                return "Conclusión basada en evidencia acumulada";
            }
        }
    };

    public static IReadOnlyList<ReasoningRule> RegisteredRules => Rules;

    public ReasoningResult Reason(ReasoningContext context)
    {
        var result = new ReasoningResult();
        var facts = CollectFacts(context);

        uint nextId = 1;
        foreach (var rule in Rules)
        {
            if (!rule.TryApply(facts, out var inference))
                continue;

            inference.Id = nextId++;
            result.Inferences.Add(inference);

            result.Evidence.Add(new ReasoningEvidence
            {
                InferenceId = inference.Id,
                RuleId = inference.RuleId,
                PremiseCount = inference.Premises.Length,
                Transformation = inference.Transformation,
                Confidence = inference.Confidence,
                EvidenceStrength = ComputeEvidenceStrength(inference, rule),
                Strategy = nameof(EvidenceBasedReasoningStrategy)
            });
        }

        // Priorizar por metas si hay
        var goals = context.ActiveGoals;
        if (goals.Count > 0)
        {
            var prioritized = new List<Inference>();
            var nonPrioritized = new List<Inference>();
            foreach (var inf in result.Inferences)
            {
                bool relevantToGoal = goals.Any(g => 
                    inf.Conclusion.Contains(g.Type.ToString(), StringComparison.OrdinalIgnoreCase));
                if (relevantToGoal)
                    prioritized.Add(inf);
                else
                    nonPrioritized.Add(inf);
            }
            result.Inferences.Clear();
            result.Inferences.AddRange(prioritized);
            result.Inferences.AddRange(nonPrioritized);
        }

        return result;
    }

    private static string[] CollectFacts(ReasoningContext context)
    {
        var facts = new List<string>();

        foreach (var chunk in context.WorkingMemory)
        {
            if (!string.IsNullOrEmpty(chunk.Content))
                facts.Add(chunk.Content);
        }

        foreach (var mem in context.RetrievedMemories)
        {
            if (mem.Memory.Category == MemoryCategory.Environmental ||
                mem.Memory.Category == MemoryCategory.Discovery)
            {
                facts.Add($"memory_{mem.Memory.Id}_cat_{mem.Memory.Category}");
            }
        }

        return facts.ToArray();
    }

    private static float ComputeEvidenceStrength(Inference inf, ReasoningRule rule)
    {
        float premiseRatio = rule.MaxPremises > 0
            ? (float)inf.Premises.Length / rule.MaxPremises
            : 0f;
        return Math.Clamp(inf.Confidence * premiseRatio, 0f, 1f);
    }
}
