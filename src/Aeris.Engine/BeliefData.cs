namespace Aeris.Engine;

public enum BeliefSource : byte
{
    DirectObservation,
    ToldByTrusted,
    ToldByUntrusted,
    CulturalTradition,
    InferredFromEvidence,
    Assumed
}

public enum BeliefStatus : byte
{
    Active,
    Weakening,
    Revised,
    Abandoned,
    Contradicted
}

public struct BeliefData
{
    public uint Id;
    public float Confidence;
    public BeliefSource Source;
    public float FormationTime;
    public float LastConfirmationTime;
    public BeliefStatus Status;
    public uint SupportingMemoryId;
    public uint ContradictingMemoryId;

    public bool IsActive => Status == BeliefStatus.Active && Confidence > 0.1f;
}

public sealed class BeliefStore
{
    private readonly Dictionary<uint, List<BeliefData>> _byEntity = new();
    private uint _nextId = 1;

    public uint AllocateId() => _nextId++;

    public void AddBelief(uint entityId, BeliefData belief)
    {
        if (!_byEntity.TryGetValue(entityId, out var list))
        {
            list = new List<BeliefData>();
            _byEntity[entityId] = list;
        }
        list.Add(belief);
    }

    public List<BeliefData> GetBeliefs(uint entityId)
    {
        return _byEntity.TryGetValue(entityId, out var list) ? list : new List<BeliefData>();
    }

    public bool TryGetBeliefs(uint entityId, out List<BeliefData> beliefs)
    {
        return _byEntity.TryGetValue(entityId, out beliefs!);
    }

    public void RemoveEntity(uint entityId)
    {
        _byEntity.Remove(entityId);
    }

    public int Count => _byEntity.Count;
    public IReadOnlyDictionary<uint, List<BeliefData>> All => _byEntity;
}
