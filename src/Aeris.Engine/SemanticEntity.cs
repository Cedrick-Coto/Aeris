namespace Aeris.Engine;

public sealed class SemanticEntity
{
    public string Description { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string RelationToAgent { get; init; } = string.Empty;
    public string EmotionalCharge { get; init; } = string.Empty;
    public List<string> NotableTraits { get; init; } = new();
}
