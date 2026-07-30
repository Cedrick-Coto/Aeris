namespace Aeris.Engine;

public sealed class MemoryRetrievalContext
{
    public List<MemoryData> CandidateMemories { get; init; } = new();
    public WorkingMemoryStore WorkingMemory { get; init; } = null!;
    public AffectState AffectState { get; init; }
    public float CurrentTime { get; init; }
    public int Budget { get; init; } = 3;
}

public interface IMemoryRetrievalStrategy
{
    RetrievalResult Retrieve(MemoryRetrievalContext context);
}
