namespace Aeris.Engine;

public sealed class PlanStore
{
    public List<PlanCandidate> Plans { get; set; } = new();
    public List<PlanningEvidence> Evidence { get; set; } = new();
    public long LastExecutionTick { get; set; }

    public void Clear()
    {
        Plans.Clear();
        Evidence.Clear();
    }
}
