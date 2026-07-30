namespace Aeris.Engine;

public sealed class PlanningContext
{
    public List<GoalData> ActiveGoals { get; init; } = new();
    public WorldModelState WorldModel { get; init; } = null!;
    public List<Inference> AvailableInferences { get; init; } = new();
    public AffectState Affect { get; init; }
    public List<WorkingMemoryChunk> WorkingMemory { get; init; } = new();
}

public interface IPlanningStrategy
{
    PlanningResult Plan(PlanningContext context);
}
