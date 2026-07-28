namespace Aeris.Engine;

public enum MemoryType : byte
{
    Observed,
    Experienced,
    Learned,
    Inferred,
    Forgotten
}

public enum MemoryCategory : byte
{
    Social,
    Environmental,
    Combat,
    Discovery,
    Emotional,
    Quest
}

public struct MemoryData
{
    public uint Id;
    public MemoryType Type;
    public MemoryCategory Category;
    public float EmotionalWeight;
    public float Importance;
    public float Certainty;
    public float Timestamp;
    public uint InvolvedEntityId;
    public uint LocationId;
    public bool Forgotten;
    public float DecayStart;

    public bool IsRelevant => !Forgotten && Importance > 0.2f;
    public float EffectiveImportance(float currentTime, float halfLife = 86400f)
    {
        if (Forgotten) return 0f;
        var age = currentTime - Timestamp;
        var decay = MathF.Pow(0.5f, age / halfLife);
        return Importance * decay;
    }
}

public sealed class MemoryStore
{
    private readonly Dictionary<uint, List<MemoryData>> _byEntity = new();
    private uint _nextId = 1;

    public uint AllocateId() => _nextId++;

    public void AddMemory(uint entityId, MemoryData memory)
    {
        if (!_byEntity.TryGetValue(entityId, out var list))
        {
            list = new List<MemoryData>();
            _byEntity[entityId] = list;
        }
        list.Add(memory);
    }

    public List<MemoryData> GetMemories(uint entityId)
    {
        return _byEntity.TryGetValue(entityId, out var list) ? list : new List<MemoryData>();
    }

    public bool TryGetMemories(uint entityId, out List<MemoryData> memories)
    {
        return _byEntity.TryGetValue(entityId, out memories!);
    }

    public void RemoveEntity(uint entityId)
    {
        _byEntity.Remove(entityId);
    }

    public int Count => _byEntity.Count;
    public IReadOnlyDictionary<uint, List<MemoryData>> All => _byEntity;
}
