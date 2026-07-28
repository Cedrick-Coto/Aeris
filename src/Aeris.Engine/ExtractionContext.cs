namespace Aeris.Engine;

public sealed class ExtractionContext
{
    public required World World { get; init; }
    public required Entity Agent { get; init; }
    public required ExtractionOptions Options { get; init; }
    public Dictionary<uint, string> EntityNames { get; init; } = new();
}
