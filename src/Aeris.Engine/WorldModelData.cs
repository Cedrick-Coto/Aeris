namespace Aeris.Engine;

public sealed class WorldModelState
{
    public List<uint> KnownEntityIds { get; set; } = new();
    public long LastUpdateTick { get; set; }
    public int EntityCount => KnownEntityIds.Count;
}
