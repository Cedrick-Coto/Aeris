namespace Aeris.Engine;

public sealed class MinimalReasoningStrategy : IReasoningStrategy
{
    private static readonly List<ReasoningRule> Rules = new()
    {
        new ReasoningRule
        {
            RuleId = "minimal-001",
            Version = 1,
            Label = "MinimalInference",
            Description = "Devuelve la primera inferencia válida encontrada en las reglas",
            MinPremises = 1,
            MaxPremises = 5,
            BaseWeight = 0.3f,
            PremiseMatcher = facts =>
            {
                // Lógica de coincidencia mínima: al menos una premisa válida
                return facts.Length >= 1;
            },
            InferenceBuilder = facts =>
            {
                // Construye inferencia con la primera regla válida
                return "Inferencia mínima basada en premisa disponible";
            }
        }
    };

    public static IReadOnlyList<ReasoningRule> RegisteredRules => Rules;

    public ReasoningResult Reason(ReasoningContext context)
    {
        var result = new ReasoningResult();
        var facts = CollectFacts(context);

        foreach (var rule in Rules)
        {
            if (!rule.TryApply(facts, out var inference))
                continue;

            inference.Id = 1; // Solo una inferencia por ejecución
            result.Inferences.Add(inference);

            result.Evidence.Add(new ReasoningEvidence
            {
                InferenceId = inference.Id,
                RuleId = inference.RuleId,
                PremiseCount = inference.Premises.Length,
                Transformation = inference.Transformation,
                Confidence = inference.Confidence,
                EvidenceStrength = ComputeEvidenceStrength(inference, rule),
                Strategy = nameof(MinimalReasoningStrategy)
            });

            // Detener después de la primera inferencia válida
            break;
        }

        // No priorización por metas en esta estrategia
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
