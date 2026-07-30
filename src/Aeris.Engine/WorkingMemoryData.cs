namespace Aeris.Engine;

public struct WorkingMemoryChunk
{
    public string Id;
    public string Content;
    public PerceptType? SourceType;
    public EntityId? SourceEntity;
    public float Salience;
    public float DecayRate;
    public long FormationTick;
    public long LastAccessTick;
}

public sealed class WorkingMemoryStore
{
    public List<WorkingMemoryChunk> Chunks { get; set; } = new();
    public int Capacity { get; set; } = 7;
    public float MinSalience { get; set; } = 0.05f;
}
