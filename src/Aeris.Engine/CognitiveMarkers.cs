namespace Aeris.Engine;

public struct MemoryMarker
{
    public int Count;
    public float LastConsolidationTime;
    public uint LatestMemoryId;
}

public struct BeliefMarker
{
    public int Count;
    public float LastUpdateTime;
    public uint LatestBeliefId;
}

public struct KnowledgeMarker
{
    public int Count;
    public float LastUpdateTime;
}

public struct GoalMarker
{
    public int ActiveCount;
    public GoalPriority HighestPriority;
}

public struct RelationshipMarker
{
    public int Count;
}
