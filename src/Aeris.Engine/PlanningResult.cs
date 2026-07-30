namespace Aeris.Engine;

public sealed class PlanningResult
{
    public List<PlanCandidate> Plans { get; init; } = new();
    public List<PlanningEvidence> Evidence { get; init; } = new();
}
