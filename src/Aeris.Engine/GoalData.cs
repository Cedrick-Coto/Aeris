namespace Aeris.Engine;

public enum GoalType : byte
{
    Survival,
    Social,
    Exploration,
    Combat,
    Collection,
    Knowledge,
    Emotional,
    Quest
}

public enum GoalPriority : byte
{
    Trivial = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}

public enum GoalStatus : byte
{
    Inactive,
    Active,
    Paused,
    Completed,
    Failed,
    Abandoned
}

public struct GoalData
{
    public uint Id;
    public GoalType Type;
    public GoalPriority Priority;
    public float Urgency;
    public GoalStatus Status;
    public float CreationTime;
    public float? Deadline;
    public uint AssignedToEntity;

    public bool IsActive => Status == GoalStatus.Active;
    public float EffectivePriority(float currentTime)
    {
        if (!IsActive) return 0f;
        if (Deadline.HasValue && Deadline.Value < currentTime) return 0f;
        var priorityScore = (float)Priority / 5f;
        var urgencyScore = Urgency;
        var deadlinePressure = Deadline.HasValue
            ? MathF.Max(0f, 1f - (Deadline.Value - currentTime) / 86400f)
            : 0f;
        return priorityScore * 0.5f + urgencyScore * 0.3f + deadlinePressure * 0.2f;
    }
}

public sealed class GoalStore
{
    private readonly Dictionary<uint, List<GoalData>> _byEntity = new();
    private uint _nextId = 1;

    public uint AllocateId() => _nextId++;

    public void AddGoal(uint entityId, GoalData goal)
    {
        if (!_byEntity.TryGetValue(entityId, out var list))
        {
            list = new List<GoalData>();
            _byEntity[entityId] = list;
        }
        list.Add(goal);
    }

    public List<GoalData> GetGoals(uint entityId)
    {
        return _byEntity.TryGetValue(entityId, out var list) ? list : new List<GoalData>();
    }

    public bool TryGetGoals(uint entityId, out List<GoalData> goals)
    {
        return _byEntity.TryGetValue(entityId, out goals!);
    }

    public void RemoveEntity(uint entityId)
    {
        _byEntity.Remove(entityId);
    }

    public int Count => _byEntity.Count;
    public IReadOnlyDictionary<uint, List<GoalData>> All => _byEntity;
}
