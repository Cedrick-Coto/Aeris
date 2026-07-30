namespace Aeris.Engine;

public sealed class WorkingMemorySystem : ISystem
{
    public string Name => "WorkingMemorySystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 30;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        if (!world.HasResource<WorkingMemoryStore>())
            world.AddResource(new WorkingMemoryStore());

        var wm = world.GetResource<WorkingMemoryStore>();
        var affect = world.HasResource<AffectState>() ? world.GetResource<AffectState>() : AffectState.Default;

        int effectiveCapacity = wm.Capacity;
        if (affect.Stress > 0.7f)
            effectiveCapacity = Math.Max(2, effectiveCapacity - 2);
        if (affect.CognitiveLoad > 0.8f)
            effectiveCapacity = Math.Max(1, effectiveCapacity - 1);

        DecayChunks(wm, time.Tick, affect);
        RemoveLowSalience(wm);

        if (world.HasResource<AttendedPercepts>())
        {
            var attended = world.GetResource<AttendedPercepts>();
            foreach (var percept in attended.Percepts)
            {
                string chunkId = $"percept_{percept.Source.Value}_{percept.Type}";

                int existingIdx = wm.Chunks.FindIndex(c => c.Id == chunkId);
                if (existingIdx >= 0)
                {
                    var refreshed = wm.Chunks[existingIdx];
                    refreshed.LastAccessTick = time.Tick;
                    refreshed.Salience = Math.Min(1f, refreshed.Salience + 0.2f);
                    wm.Chunks[existingIdx] = refreshed;
                }
                else
                {
                    if (wm.Chunks.Count >= effectiveCapacity)
                    {
                        wm.Chunks.RemoveAll(c => c.Salience <= wm.MinSalience);
                        if (wm.Chunks.Count >= effectiveCapacity)
                        {
                            var lowest = wm.Chunks.OrderBy(c => c.Salience).First();
                            wm.Chunks.Remove(lowest);
                        }
                    }

                    wm.Chunks.Add(new WorkingMemoryChunk
                    {
                        Id = chunkId,
                        Content = $"entity_{percept.Source.Value}_{percept.Type}",
                        SourceType = percept.Type,
                        SourceEntity = percept.Source,
                        Salience = percept.Salience,
                        DecayRate = 0.1f + affect.CognitiveLoad * 0.2f,
                        FormationTick = time.Tick,
                        LastAccessTick = time.Tick
                    });
                }
            }
        }

        wm.Capacity = effectiveCapacity;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"{wm.Chunks.Count} chunks, capacity={effectiveCapacity}", $"{wm.Chunks.Count} after decay", $"Affect modulation: stress={affect.Stress:F2}, load={affect.CognitiveLoad:F2}");
        }
    }

    private static void DecayChunks(WorkingMemoryStore wm, long currentTick, AffectState affect)
    {
        for (int i = 0; i < wm.Chunks.Count; i++)
        {
            var chunk = wm.Chunks[i];
            long age = currentTick - chunk.LastAccessTick;
            float decayFactor = 1f - chunk.DecayRate * age;
            chunk.Salience = Math.Max(0f, chunk.Salience * decayFactor);
            wm.Chunks[i] = chunk;
        }
    }

    private static void RemoveLowSalience(WorkingMemoryStore wm)
    {
        wm.Chunks.RemoveAll(c => c.Salience <= wm.MinSalience);
    }
}
