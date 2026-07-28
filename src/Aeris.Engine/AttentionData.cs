namespace Aeris.Engine;

public struct AttentionComponent
{
    public uint FocusTargetId;
    public float FocusIntensity;
    public float PerceptualRange;
    public float UpdateTime;

    public bool HasFocus => FocusTargetId != 0 && FocusIntensity > 0.1f;
    public bool CanPerceive(uint entityId)
    {
        return FocusTargetId == entityId || FocusIntensity < 0.8f;
    }
}

public struct AttentionMarker
{
    public float UpdateTime;
}

public sealed class AttentionStore
{
    private readonly Dictionary<uint, List<uint>> _nearbyEntities = new();

    public void SetNearby(uint entityId, List<uint> nearby)
    {
        _nearbyEntities[entityId] = nearby;
    }

    public List<uint> GetNearby(uint entityId)
    {
        return _nearbyEntities.TryGetValue(entityId, out var list) ? list : new List<uint>();
    }

    public bool TryGetNearby(uint entityId, out List<uint> nearby)
    {
        return _nearbyEntities.TryGetValue(entityId, out nearby!);
    }

    public void Remove(uint entityId)
    {
        _nearbyEntities.Remove(entityId);
    }

    public int Count => _nearbyEntities.Count;
}
