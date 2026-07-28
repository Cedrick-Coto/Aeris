namespace Aeris.Engine;

public sealed class SemanticFact
{
    public string Subject { get; init; } = string.Empty;
    public string Predicate { get; init; } = string.Empty;
    public string Object { get; init; } = string.Empty;
    public string Certainty { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;

    public override string ToString() =>
        $"{Subject} {Predicate} {Object}";
}
