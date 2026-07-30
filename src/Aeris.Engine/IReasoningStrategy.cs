namespace Aeris.Engine;

public sealed class ReasoningContext
{
    public List<WorkingMemoryChunk> WorkingMemory { get; init; } = new();
    public List<RetrievedMemoryEntry> RetrievedMemories { get; init; } = new();
    public WorldModelState WorldModel { get; init; } = null!;
    public List<GoalData> ActiveGoals { get; init; } = new();
}

public interface IReasoningStrategy
{
    ReasoningResult Reason(ReasoningContext context);
}
