namespace Aeris.Engine;

public sealed class ReasoningRule
{
    public string RuleId { get; init; } = null!;
    public int Version { get; init; }
    public string Label { get; init; } = null!;
    public string Description { get; init; } = null!;

    public Func<string[], bool> PremiseMatcher { get; init; } = null!;
    public Func<string[], string> InferenceBuilder { get; init; } = null!;
    public int MinPremises { get; init; } = 1;
    public int MaxPremises { get; init; } = 5;
    public float BaseWeight { get; init; } = 0.5f;

    public bool TryApply(string[] facts, out Inference inference)
    {
        inference = default;

        if (facts.Length < MinPremises || facts.Length > MaxPremises)
            return false;

        if (!PremiseMatcher(facts))
            return false;

        inference = new Inference
        {
            RuleId = $"{RuleId}-v{Version}",
            Transformation = Label,
            Conclusion = InferenceBuilder(facts),
            Premises = facts,
            Confidence = BaseWeight
        };

        return true;
    }
}
