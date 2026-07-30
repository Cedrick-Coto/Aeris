namespace Aeris.Engine;

public sealed class RulePriorityStrategy : IReasoningStrategy
{
    private readonly List<(ReasoningRule rule, int priority)> _rules;

    public RulePriorityStrategy()
    {
        _rules = new List<(ReasoningRule, int)>
        {
            (new ReasoningRule
            {
                RuleId = "contradiction-check",
                Version = 1,
                Label = "ContradictionCheck",
                Description = "Highest priority: detect conflicting location assertions",
                MinPremises = 2,
                MaxPremises = 2,
                BaseWeight = 0.9f,
                PremiseMatcher = facts =>
                {
                    if (facts.Length < 2) return false;
                    string a = facts[0].ToLowerInvariant();
                    string b = facts[1].ToLowerInvariant();

                    bool sameSubject = false;
                    foreach (var word in a.Split(' '))
                    {
                        if (b.Contains(word) && word.Length > 3)
                        { sameSubject = true; break; }
                    }
                    if (!sameSubject) return false;

                    string[] dirs = { "north", "south", "east", "west" };
                    foreach (var d in dirs)
                    {
                        if (a.Contains(d) && b.Contains(d)) return true;
                    }

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
                        { subject = word; break; }
                    }
                    return subject != "" ? $"Conflicto de ubicación ({subject})" : "Conflicto detectado";
                }
            }, 0),
            (new ReasoningRule
            {
                RuleId = "causal-sequence",
                Version = 1,
                Label = "CausalSequence",
                Description = "If event observed and pattern known, infer likely consequence",
                MinPremises = 2,
                MaxPremises = 5,
                BaseWeight = 0.7f,
                PremiseMatcher = facts =>
                {
                    bool hasEvent = false, hasPattern = false;
                    foreach (var f in facts)
                    {
                        if (f.Contains("observed") || f.Contains("seen") || f.Contains("happened"))
                            hasEvent = true;
                        if (f.Contains("after") || f.Contains("causes") || f.Contains("leads") || f.Contains("followed"))
                            hasPattern = true;
                    }
                    return hasEvent && hasPattern;
                },
                InferenceBuilder = facts =>
                {
                    string cause = "", effect = "";
                    foreach (var f in facts)
                    {
                        if (f.Contains("after"))
                        {
                            var parts = f.Split("after", StringSplitOptions.TrimEntries);
                            if (parts.Length > 1) { effect = parts[0].Trim(); cause = parts[1].Trim(); }
                        }
                    }
                    return cause != "" ? $"{effect} probable después de {cause}" : "Secuencia causal probable";
                }
            }, 1),
            (new ReasoningRule
            {
                RuleId = "spatial-association",
                Version = 1,
                Label = "SpatialAssociation",
                Description = "Entities spatially related share location likelihood",
                MinPremises = 2,
                MaxPremises = 5,
                BaseWeight = 0.5f,
                PremiseMatcher = facts =>
                {
                    bool hasLocation = false, hasRelation = false;
                    foreach (var f in facts)
                    {
                        if (f.Contains("location") || f.Contains("north") || f.Contains("south") || f.Contains("east") || f.Contains("west"))
                            hasLocation = true;
                        if (f.Contains("near") || f.Contains("close") || f.Contains("associated") || f.Contains("relation"))
                            hasRelation = true;
                    }
                    return hasLocation && hasRelation;
                },
                InferenceBuilder = facts => "Probable relación espacial entre entidades observadas"
            }, 2)
        };
    }

    public IReadOnlyList<(ReasoningRule rule, int priority)> RegisteredRules => _rules;

    public ReasoningResult Reason(ReasoningContext context)
    {
        var result = new ReasoningResult();
        var facts = CollectFacts(context);

        uint nextId = 1;
        foreach (var (rule, priority) in _rules)
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
                EvidenceStrength = inference.Confidence,
                Strategy = nameof(RulePriorityStrategy)
            });
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
            if (mem.Memory.Category is MemoryCategory.Environmental or MemoryCategory.Discovery)
                facts.Add($"memory_{mem.Memory.Id}_cat_{mem.Memory.Category}");
        }
        return facts.ToArray();
    }
}
