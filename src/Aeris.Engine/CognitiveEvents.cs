namespace Aeris.Engine;

public struct MemoryCreatedEvent
{
    public uint EntityId;
    public uint MemoryId;
    public MemoryType Type;
    public MemoryCategory Category;
    public float Importance;
    public float EmotionalWeight;
}

public struct KnowledgeAcquiredEvent
{
    public uint EntityId;
    public uint KnowledgeId;
    public KnowledgeType Type;
    public KnowledgeCertainty Certainty;
}

public struct EmotionChangedEvent
{
    public uint EntityId;
    public EmotionType PreviousEmotion;
    public EmotionType NewEmotion;
    public float Intensity;
    public uint TriggerEntityId;
}

public struct GoalCompletedEvent
{
    public uint EntityId;
    public uint GoalId;
    public GoalType Type;
    public GoalStatus Result;
}

public struct GoalActivatedEvent
{
    public uint EntityId;
    public uint GoalId;
    public GoalType Type;
    public GoalPriority Priority;
}

public struct RelationshipChangedEvent
{
    public uint EntityA;
    public uint EntityB;
    public RelationshipType NewType;
    public float ValueChange;
    public float TrustChange;
}

public struct AttentionChangedEvent
{
    public uint EntityId;
    public uint PreviousFocusId;
    public uint NewFocusId;
    public float Intensity;
}
