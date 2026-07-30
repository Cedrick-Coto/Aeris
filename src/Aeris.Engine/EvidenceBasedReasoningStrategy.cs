namespace Aeris.Engine;

public sealed class EvidenceBasedReasoningStrategy : IReasoningStrategy
{
    private static readonly List<ReasoningRule> Rules = new()
    {
        new ReasoningRule
        {
            RuleId = "spatial-association-001",
            Version = 1,
            Label = "SpatialAssociation",
            Description = "If A is at location L and A is spatially related to B, B is likely near L",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.6f,
            PremiseMatcher = facts =>
            {
                bool hasLocation = false;
                bool hasRelation = false;
                foreach (var f in facts)
                {
                    if (f.Contains("location", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("north", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("south", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("east", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("west", StringComparison.OrdinalIgnoreCase))
                        hasLocation = true;
                    if (f.Contains("near", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("close", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("associated", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("relation", StringComparison.OrdinalIgnoreCase))
                        hasRelation = true;
                }
                return hasLocation && hasRelation;
            },
            InferenceBuilder = facts =>
            {
                string location = "";
                string target = "";
                foreach (var f in facts)
                {
                    if (f.Contains("near", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = f.Split("near", StringSplitOptions.TrimEntries);
                        if (parts.Length > 1) target = parts[0].Trim();
                    }
                    if (f.Contains("north", StringComparison.OrdinalIgnoreCase) && target == "")
                        location = "north";
                    else if (f.Contains("south", StringComparison.OrdinalIgnoreCase) && target == "")
                        location = "south";
                    else if (f.Contains("east", StringComparison.OrdinalIgnoreCase) && target == "")
                        location = "east";
                    else if (f.Contains("west", StringComparison.OrdinalIgnoreCase) && target == "")
                        location = "west";
                }
                return target != ""
                    ? $"{target} probablemente ubicado al {location}"
                    : "Probable relación espacial entre entidades observadas";
            }
        },
        new ReasoningRule
        {
            RuleId = "causal-sequence-001",
            Version = 1,
            Label = "CausalSequence",
            Description = "If event A is observed and pattern A→B is known, B is likely after A",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.5f,
            PremiseMatcher = facts =>
            {
                bool hasEvent = false;
                bool hasPattern = false;
                foreach (var f in facts)
                {
                    if (f.Contains("observed", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("seen", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("happened", StringComparison.OrdinalIgnoreCase))
                        hasEvent = true;
                    if (f.Contains("after", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("causes", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("leads", StringComparison.OrdinalIgnoreCase) ||
                        f.Contains("followed", StringComparison.OrdinalIgnoreCase))
                        hasPattern = true;
                }
                return hasEvent && hasPattern;
            },
            InferenceBuilder = facts =>
            {
                string cause = "";
                string effect = "";
                foreach (var f in facts)
                {
                    if (f.Contains("after", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = f.Split("after", StringSplitOptions.TrimEntries);
                        if (parts.Length > 1) { effect = parts[0].Trim(); cause = parts[1].Trim(); }
                    }
                }
                return cause != ""
                    ? $"{effect} probable después de {cause}"
                    : "Probable secuencia causal entre eventos observados";
            }
        },
        new ReasoningRule
        {
            RuleId = "contradiction-001",
            Version = 1,
            Label = "Contradiction",
            Description = "If fact A and fact B are mutually exclusive, report conflict",
            MinPremises = 2,
            MaxPremises = 2,
            BaseWeight = 0f,
            PremiseMatcher = facts =>
            {
                if (facts.Length < 2) return false;
                string a = facts[0].ToLowerInvariant();
                string b = facts[1].ToLowerInvariant();

                bool sameSubject = false;
                foreach (var word in a.Split(' '))
                {
                    if (b.Contains(word) && word.Length > 3)
                    {
                        sameSubject = true;
                        break;
                    }
                }

                bool oppositeLocations = false;
                string[] dirs = { "north", "south", "east", "west" };
                foreach (var d in dirs)
                {
                    if (a.Contains(d) && b.Contains(d))
                    {
                        oppositeLocations = true;
                        break;
                    }
                }
                if (!oppositeLocations)
                {
                    string[] opposites = { "north/south", "south/north", "east/west", "west/east" };
                    foreach (var pair in opposites)
                    {
                        var parts = pair.Split('/');
                        if (a.Contains(parts[0]) && b.Contains(parts[1]) ||
                            a.Contains(parts[1]) && b.Contains(parts[0]))
                        {
                            oppositeLocations = true;
                            break;
                        }
                    }
                }

                return sameSubject && oppositeLocations;
            },
            InferenceBuilder = facts =>
            {
                string subject = "";
                foreach (var f in facts[0].Split(' '))
                {
                    if (f.Length > 3 && facts[1].ToLowerInvariant().Contains(f.ToLowerInvariant()))
                    {
                        subject = f;
                        break;
                    }
                }
                return subject != ""
                    ? $"Conflicto de ubicación ({subject})"
                    : "Conflicto detectado entre afirmaciones incompatibles";
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
            inference.Confidence = ComputeConfidence(inference, rule, facts.Length);
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

        var goals = context.ActiveGoals;
        if (goals.Count > 0)
        {
            var prioritized = new List<Inference>();
            var nonPrioritized = new List<Inference>();
            foreach (var inf in result.Inferences)
            {
                bool relevantToGoal = false;
                foreach (var goal in goals)
                {
                    string typeStr = goal.Type.ToString();
                    if (inf.Conclusion.Contains(typeStr, StringComparison.OrdinalIgnoreCase))
                    {
                        relevantToGoal = true;
                        break;
                    }
                }
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

    private static float ComputeConfidence(Inference inf, ReasoningRule rule, int totalFacts)
    {
        float premiseRatio = totalFacts > 0
            ? Math.Min(1f, (float)inf.Premises.Length / Math.Max(1, totalFacts))
            : 0f;

        return Math.Clamp(rule.BaseWeight * premiseRatio, 0f, 1f);
    }

    private static float ComputeEvidenceStrength(Inference inf, ReasoningRule rule)
    {
        float premiseRatio = rule.MaxPremises > 0
            ? (float)inf.Premises.Length / rule.MaxPremises
            : 0f;
        return Math.Clamp(inf.Confidence * premiseRatio, 0f, 1f);
    }
}
