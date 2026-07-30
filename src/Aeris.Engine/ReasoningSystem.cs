namespace Aeris.Engine;

public sealed class ReasoningSystem : ISystem
{
    public string Name => "ReasoningSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 30;

    public IReasoningStrategy Strategy { get; set; } = new EvidenceBasedReasoningStrategy();

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<WorkingMemoryStore>())
            return;
        if (!world.HasResource<WorldModelState>())
            return;
        if (!world.HasResource<GoalStore>())
            return;

        var wm = world.GetResource<WorkingMemoryStore>();
        var model = world.GetResource<WorldModelState>();
        var goalStore = world.GetResource<GoalStore>();

        var context = new ReasoningContext
        {
            WorkingMemory = wm.Chunks,
            RetrievedMemories = wm.Chunks
                .Where(c => c.Id.StartsWith("retrieved_"))
                .Select(c =>
                {
                    uint.TryParse(c.Id.Replace("retrieved_", ""), out uint memId);
                    return new RetrievedMemoryEntry
                    {
                        Memory = new MemoryData { Id = memId, Category = MemoryCategory.Environmental },
                        Score = c.Salience
                    };
                }).ToList(),
            WorldModel = model,
            ActiveGoals = goalStore.All.SelectMany(kvp => kvp.Value).ToList()
        };

        var result = Strategy.Reason(context);

        if (!world.HasResource<InferenceStore>())
            world.AddResource(new InferenceStore());

        var store = world.GetResource<InferenceStore>();
        store.Clear();
        store.Inferences.AddRange(result.Inferences);
        store.Evidence.AddRange(result.Evidence);

        var time = world.GetResource<TimeResource>();
        store.LastExecutionTick = time.Tick;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name,
                $"{context.WorkingMemory.Count} chunks, {context.RetrievedMemories.Count} retrieved, {context.ActiveGoals.Count} goals",
                $"{result.Inferences.Count} inferences, {result.Evidence.Count} evidence entries",
                $"Strategy={Strategy.GetType().Name}");
        }
    }
}
