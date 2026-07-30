namespace Aeris.Engine;

public sealed class LongTermMemorySystem : ISystem
{
    public string Name => "LongTermMemorySystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 40;

    public float ConsolidationInterval { get; set; } = 3600f;
    public float ForgetThreshold { get; set; } = 0.05f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        if (!world.HasResource<MemoryStore>())
            return;

        var memories = world.GetResource<MemoryStore>();
        float currentTime = (float)time.SimulationTime;

        foreach (var kvp in memories.All)
        {
            uint entityId = kvp.Key;
            var entityMemories = kvp.Value;
            bool anyForgotten = false;

            for (int i = 0; i < entityMemories.Count; i++)
            {
                var mem = entityMemories[i];
                if (mem.Forgotten)
                    continue;

                float effective = mem.EffectiveImportance(currentTime, ConsolidationInterval);
                if (effective < ForgetThreshold)
                {
                    mem.Forgotten = true;
                    entityMemories[i] = mem;
                    anyForgotten = true;
                }
            }

            if (anyForgotten)
            {
                var worldEntity = world.Entities.Values.FirstOrDefault(e => e.Id.Value == entityId);
                if (worldEntity != null && worldEntity.HasComponent<MemoryMarker>())
                {
                    var marker = worldEntity.GetComponent<MemoryMarker>();
                    marker.LastConsolidationTime = currentTime;
                    worldEntity.SetComponent(marker);
                }
            }
        }

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"consolidation at t={currentTime:F0}", $"forget threshold={ForgetThreshold}", $"Memory decay check across all entities");
        }
    }
}
