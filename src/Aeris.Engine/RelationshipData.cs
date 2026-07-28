namespace Aeris.Engine;

public enum RelationshipType : byte
{
    Neutral,
    Friend,
    Rival,
    Mentor,
    Student,
    Family,
    Romantic,
    Enemy,
    Ally,
    Stranger
}

public enum RelationshipStrength : byte
{
    Acquaintance,
    Associate,
    Friend,
    CloseFriend,
    BestFriend,
    Soulmate
}

public enum RelationshipStatus : byte
{
    Active,
    Dormant,
    Strained,
    Broken,
    Evolving
}

public struct RelationshipData
{
    public uint Id;
    public uint EntityA;
    public uint EntityB;
    public RelationshipType Type;
    public float Value;
    public RelationshipStrength Strength;
    public float TrustLevel;
    public float Familiarity;
    public float InteractionCount;
    public float LastInteractionTime;
    public RelationshipStatus Status;

    public float EffectiveStrength()
    {
        return Value * 0.4f + TrustLevel * 0.4f + Familiarity * 0.2f;
    }

    public void RecordInteraction(float time)
    {
        InteractionCount++;
        LastInteractionTime = time;
        Familiarity = MathF.Min(1f, Familiarity + 0.02f);
    }
}

public sealed class RelationshipStore
{
    private readonly Dictionary<uint, List<RelationshipData>> _byEntity = new();
    private readonly Dictionary<(uint, uint), RelationshipData> _byPair = new();
    private uint _nextId = 1;

    public uint AllocateId() => _nextId++;

    public void AddRelationship(uint entityId, RelationshipData relationship)
    {
        if (!_byEntity.TryGetValue(entityId, out var list))
        {
            list = new List<RelationshipData>();
            _byEntity[entityId] = list;
        }
        list.Add(relationship);

        var pairKey = relationship.EntityA < relationship.EntityB
            ? (relationship.EntityA, relationship.EntityB)
            : (relationship.EntityB, relationship.EntityA);
        _byPair[pairKey] = relationship;
    }

    public List<RelationshipData> GetRelationships(uint entityId)
    {
        return _byEntity.TryGetValue(entityId, out var list) ? list : new List<RelationshipData>();
    }

    public bool TryGetRelationship(uint entityA, uint entityB, out RelationshipData relationship)
    {
        var pairKey = entityA < entityB ? (entityA, entityB) : (entityB, entityA);
        return _byPair.TryGetValue(pairKey, out relationship);
    }

    public void RemoveEntity(uint entityId)
    {
        if (_byEntity.TryGetValue(entityId, out var list))
        {
            foreach (var rel in list)
            {
                var otherId = rel.EntityA == entityId ? rel.EntityB : rel.EntityA;
                var pairKey = rel.EntityA < rel.EntityB
                    ? (rel.EntityA, rel.EntityB)
                    : (rel.EntityB, rel.EntityA);
                _byPair.Remove(pairKey);
            }
            _byEntity.Remove(entityId);
        }
    }

    public int Count => _byPair.Count;
    public IReadOnlyDictionary<uint, List<RelationshipData>> AllByEntity => _byEntity;
}
