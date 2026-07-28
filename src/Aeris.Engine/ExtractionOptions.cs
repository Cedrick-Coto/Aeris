namespace Aeris.Engine;

public sealed class ExtractionOptions
{
    public int MaxTokens { get; init; } = 4000;
    public int MaxEntities { get; init; } = 20;
    public int MaxMemories { get; init; } = 10;
    public int MaxRelationships { get; init; } = 10;
    public int MaxFacts { get; init; } = 30;
    public int MaxRecentEvents { get; init; } = 5;
    public double LookbackWindow { get; init; } = 86400.0;
    public bool IncludeWorldModel { get; init; } = true;
    public bool IncludeDirectives { get; init; } = true;
    public bool EnableBudgetTrim { get; init; } = true;
}
