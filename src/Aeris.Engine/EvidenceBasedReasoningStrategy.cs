using System;
using System.Collections.Generic;
using System.Linq;

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
            Description = "Genera una inferencia cuando existe una relación espacial conocida entre evidencias.",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.6f,
            PremiseMatcher = facts =>
            {
                // Detect spatial relation keywords
                return facts.Any(f => f.Contains("near", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("adjacent", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("next to", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("behind", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("in front of", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("left of", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("right of", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("above", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("below", StringComparison.OrdinalIgnoreCase));
            },
            InferenceBuilder = facts =>
            {
                return "Spatial association inferred";
            }
        },
        new ReasoningRule
        {
            RuleId = "causal-sequence-001",
            Version = 1,
            Label = "CausalSequence",
            Description = "Genera una inferencia cuando existe un patrón causal conocido entre evidencias.",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.6f,
            PremiseMatcher = facts =>
            {
                // Detect causal keywords
                bool hasEvent = facts.Any(f => f.Contains("observed", StringComparison.OrdinalIgnoreCase) ||
                                              f.Contains("seen", StringComparison.OrdinalIgnoreCase) ||
                                              f.Contains("happened", StringComparison.OrdinalIgnoreCase) ||
                                              f.Contains("occurred", StringComparison.OrdinalIgnoreCase));
                bool hasPattern = facts.Any(f => f.Contains("after", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("cause", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("causes", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("because", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("leads to", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("leads", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("results in", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("followed", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("therefore", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("thus", StringComparison.OrdinalIgnoreCase));
                return hasEvent && hasPattern;
            },
            InferenceBuilder = facts =>
            {
                return "Causal sequence inferred";
            }
        },
        new ReasoningRule
        {
            RuleId = "goal-relevance-001",
            Version = 1,
            Label = "GoalRelevance",
            Description = "Detecta inferencias relacionadas con Goals activos sin modificar Confidence, Conclusion o Premises.",
            MinPremises = 1,
            MaxPremises = 5,
            BaseWeight = 0.5f,
            PremiseMatcher = facts =>
            {
                // Detect goal‑related keywords
                return facts.Any(f => f.Contains("goal", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("objective", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("target", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("want", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("need", StringComparison.OrdinalIgnoreCase) ||
                                     f.Contains("intend", StringComparison.OrdinalIgnoreCase));
            },
            InferenceBuilder = facts =>
            {
                // No modification of conclusion; return a generic placeholder that indicates relevance
                return "Goal relevance inferred";
            }
        },
        new ReasoningRule
        {
            RuleId = "contradiction-001",
            Version = 1,
            Label = "Contradiction",
            Description = "Detecta evidencias mutuamente incompatibles y genera una inferencia de conflicto.",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.4f,
            PremiseMatcher = facts =>
            {
                // Detect contradictory keywords
                bool hasAffirmative = facts.Any(f => f.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                                                    f.Contains("positive", StringComparison.OrdinalIgnoreCase) ||
                                                    f.Contains("accept", StringComparison.OrdinalIgnoreCase) ||
                                                    f.Contains("confirm", StringComparison.OrdinalIgnoreCase));
                bool hasNegative = facts.Any(f => f.Contains("no", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("negative", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("reject", StringComparison.OrdinalIgnoreCase) ||
                                                 f.Contains("deny", StringComparison.OrdinalIgnoreCase));
                if (hasAffirmative && hasNegative)
                    return true;

                if (facts.Length < 2)
                    return false;

                string a = facts[0].ToLowerInvariant();
                string b = facts[1].ToLowerInvariant();

                if (TryGetFirstEntityId(a, out int idA) && TryGetFirstEntityId(b, out int idB) && idA != idB)
                    return false;

                bool sameSubject = false;
                foreach (var word in a.Split(' '))
                {
                    if (word.Length > 3 && b.Contains(word))
                    {
                        sameSubject = true;
                        break;
                    }
                }
                if (!sameSubject)
                    return false;

                string[] opposites = { "north/south", "south/north", "east/west", "west/east" };
                foreach (var pair in opposites)
                {
                    var parts = pair.Split('/');
                    if ((a.Contains(parts[0]) && b.Contains(parts[1])) ||
                        (a.Contains(parts[1]) && b.Contains(parts[0])))
                        return true;
                }

                return false;
            },
            InferenceBuilder = facts =>
            {
                string subject = "";
                foreach (var word in facts[0].Split(' '))
                {
                    if (word.Length > 3 && facts[1].ToLowerInvariant().Contains(word.ToLowerInvariant()))
                    {
                        subject = word;
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

    private static bool TryGetFirstEntityId(string text, out int id)
    {
        id = 0;
        foreach (var token in text.Split(' '))
        {
            if (int.TryParse(token, out int parsed))
            {
                id = parsed;
                return true;
            }
        }
        return false;
    }
}
