namespace Aeris.Engine;

public sealed class SemanticSnapshot
{
    public SemanticState State { get; init; } = new();
    public long WorldTick { get; init; }
    public double SimulationTime { get; init; }
    public int EntityCount { get; init; }
    public DateTime ExtractionTimestamp { get; init; } = DateTime.UtcNow;
    public string? DebugSummary { get; init; }
}
