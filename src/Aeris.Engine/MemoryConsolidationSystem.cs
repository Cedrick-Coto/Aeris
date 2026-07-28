namespace Aeris.Engine;

public sealed class MemoryConsolidationSystem : ISystem
{
    public string Name => "MemoryConsolidation";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 100;

    private const float CONSOLIDATION_INTERVAL = 3600f;
    private const float DECAY_HALF_LIFE = 86400f;
    private const float FORGET_THRESHOLD = 0.05f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var memories = world.GetResource<MemoryStore>();

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<MemoryMarker>()) continue;

            var marker = entity.GetComponent<MemoryMarker>();

            if (time.SimulationTime - marker.LastConsolidationTime < CONSOLIDATION_INTERVAL)
                continue;

            var entityMemories = memories.GetMemories(entity.Id.Value);
            float currentTime = (float)time.SimulationTime;

            for (int i = entityMemories.Count - 1; i >= 0; i--)
            {
                var memory = entityMemories[i];
                var effectiveImportance = memory.EffectiveImportance(currentTime, DECAY_HALF_LIFE);

                if (effectiveImportance < FORGET_THRESHOLD && !memory.Forgotten)
                {
                    memory.Forgotten = true;
                    memory.DecayStart = currentTime;
                    entityMemories[i] = memory;
                }
            }

            int activeCount = 0;
            uint latestId = 0;
            foreach (var m in entityMemories)
            {
                if (!m.Forgotten)
                {
                    activeCount++;
                    if (m.Id > latestId) latestId = m.Id;
                }
            }

            marker.Count = activeCount;
            marker.LastConsolidationTime = currentTime;
            marker.LatestMemoryId = latestId;
            entity.SetComponent(marker);
        }
    }
}
