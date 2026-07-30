namespace Aeris.Engine;

public sealed class RetrievalResult
{
    public List<RetrievedMemoryEntry> Memories { get; init; } = new();
    public List<RetrievalEvidence> Evidence { get; init; } = new();
}

public struct RetrievedMemoryEntry
{
    public MemoryData Memory;
    public float Score;
}

public struct RetrievalEvidence
{
    public RetrievalOperation Operation;
    public uint MemoryId;
    public float ImportanceScore;
    public float RecencyScore;
    public float ContextOverlapScore;
    public float AttentionRelevanceScore;
    public float FinalScore;
    public string Strategy;
}

public enum RetrievalOperation : byte
{
    Retrieved,
    RetrievedNone
}
