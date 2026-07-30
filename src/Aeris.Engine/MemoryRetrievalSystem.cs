namespace Aeris.Engine;

public sealed class MemoryRetrievalSystem : ISystem
{
    public string Name => "MemoryRetrievalSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 25;

    public IMemoryRetrievalStrategy Strategy { get; set; } = new LinearScanStrategy();
    public int RetrievalBudget { get; set; } = 3;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        float currentTime = (float)time.SimulationTime;

        if (!world.HasResource<MemoryStore>())
            return;
        if (!world.HasResource<WorkingMemoryStore>())
            return;

        var ltm = world.GetResource<MemoryStore>();
        var wm = world.GetResource<WorkingMemoryStore>();
        var affect = world.HasResource<AffectState>() ? world.GetResource<AffectState>() : AffectState.Default;

        int budget = RetrievalBudget;
        if (affect.Stress > 0.7f)
            budget = Math.Max(1, budget - 1);
        if (affect.Curiosity > 0.7f)
            budget += 1;

        var candidates = new List<MemoryData>();
        foreach (var kvp in ltm.All)
        {
            foreach (var mem in kvp.Value)
            {
                if (!mem.Forgotten)
                    candidates.Add(mem);
            }
        }

        var context = new MemoryRetrievalContext
        {
            CandidateMemories = candidates,
            WorkingMemory = wm,
            AffectState = affect,
            CurrentTime = currentTime,
            Budget = budget
        };

        var result = Strategy.Retrieve(context);

        foreach (var entry in result.Memories)
        {
            string chunkId = $"retrieved_{entry.Memory.Id}";
            var existing = wm.Chunks.FindIndex(c => c.Id == chunkId);
            if (existing >= 0)
            {
                var refreshed = wm.Chunks[existing];
                refreshed.LastAccessTick = time.Tick;
                refreshed.Salience = Math.Min(1f, refreshed.Salience + 0.3f);
                wm.Chunks[existing] = refreshed;
            }
            else
            {
                wm.Chunks.Add(new WorkingMemoryChunk
                {
                    Id = chunkId,
                    Content = $"memory_{entry.Memory.Id}_cat_{entry.Memory.Category}",
                    SourceType = null,
                    SourceEntity = null,
                    Salience = entry.Score,
                    DecayRate = 0.08f,
                    FormationTick = time.Tick,
                    LastAccessTick = time.Tick
                });
            }

            for (int i = 0; i < wm.Chunks.Count; i++)
            {
                var chunk = wm.Chunks[i];
                if (chunk.SourceEntity.HasValue && chunk.SourceEntity.Value.Value == entry.Memory.InvolvedEntityId)
                {
                    chunk.Salience = Math.Min(1f, chunk.Salience + 0.2f);
                    wm.Chunks[i] = chunk;
                }
            }
        }

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"{candidates.Count} candidates, budget={budget}",
                $"{result.Memories.Count} retrieved", $"Strategy={Strategy.GetType().Name}");
        }

        if (world.HasResource<EventBus>())
        {
            foreach (var evidence in result.Evidence)
            {
                world.GetResource<EventBus>().Emit(new MemoryRetrievalTraceEvent
                {
                    MemoryId = evidence.MemoryId,
                    Operation = evidence.Operation,
                    FinalScore = evidence.FinalScore,
                    Strategy = evidence.Strategy,
                    Tick = time.Tick
                });
            }
        }
    }
}

public struct MemoryRetrievalTraceEvent
{
    public uint MemoryId;
    public RetrievalOperation Operation;
    public float FinalScore;
    public string Strategy;
    public long Tick;
}
