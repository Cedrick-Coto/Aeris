namespace Aeris.Engine;

public enum KnowledgeType : byte
{
    Fact,
    Rumor,
    Tradition,
    Skill,
    Location,
    Relationship,
    WorldKnowledge
}

public enum KnowledgeCertainty : byte
{
    Certain,
    Probable,
    Possible,
    Doubtful,
    Impossible
}

public enum KnowledgeSource : byte
{
    DirectExperience,
    Witnessed,
    ToldByAnother,
    Research,
    Inherited,
    Intuited
}

public struct KnowledgeData
{
    public uint Id;
    public KnowledgeType Type;
    public KnowledgeCertainty Certainty;
    public KnowledgeSource Source;
    public float AcquisitionTime;
    public float? ExpirationTime;
    public bool IsPublic;
}

public sealed class KnowledgeStore
{
    private readonly Dictionary<uint, List<KnowledgeData>> _byEntity = new();
    private uint _nextId = 1;

    public uint AllocateId() => _nextId++;

    public void AddKnowledge(uint entityId, KnowledgeData knowledge)
    {
        if (!_byEntity.TryGetValue(entityId, out var list))
        {
            list = new List<KnowledgeData>();
            _byEntity[entityId] = list;
        }
        list.Add(knowledge);
    }

    public List<KnowledgeData> GetKnowledge(uint entityId)
    {
        return _byEntity.TryGetValue(entityId, out var list) ? list : new List<KnowledgeData>();
    }

    public bool TryGetKnowledge(uint entityId, out List<KnowledgeData> knowledge)
    {
        return _byEntity.TryGetValue(entityId, out knowledge!);
    }

    public void RemoveEntity(uint entityId)
    {
        _byEntity.Remove(entityId);
    }

    public int Count => _byEntity.Count;
    public IReadOnlyDictionary<uint, List<KnowledgeData>> All => _byEntity;
}
