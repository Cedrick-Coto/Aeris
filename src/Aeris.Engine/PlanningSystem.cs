namespace Aeris.Engine;

public sealed class PlanningSystem : ISystem
{
    public string Name => "PlanningSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 35;

    public IPlanningStrategy Strategy { get; set; } = new GoalDirectedPlanningStrategy();

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<GoalStore>())
            return;
        if (!world.HasResource<WorldModelState>())
            return;
        if (!world.HasResource<InferenceStore>())
            return;
        if (!world.HasResource<AffectState>())
            return;

        var goalStore = world.GetResource<GoalStore>();
        var model = world.GetResource<WorldModelState>();
        var inferenceStore = world.GetResource<InferenceStore>();
        var affect = world.GetResource<AffectState>();
        var wm = world.GetResource<WorkingMemoryStore>();

        var activeGoals = new List<GoalData>();
        foreach (var kvp in goalStore.All)
            activeGoals.AddRange(kvp.Value.Where(g => g.IsActive));

        var context = new PlanningContext
        {
            ActiveGoals = activeGoals,
            WorldModel = model,
            AvailableInferences = inferenceStore.Inferences,
            Affect = affect,
            WorkingMemory = wm.Chunks
        };

        var result = Strategy.Plan(context);

        if (!world.HasResource<PlanStore>())
            world.AddResource(new PlanStore());

        var store = world.GetResource<PlanStore>();
        store.Clear();
        store.Plans.AddRange(result.Plans);
        store.Evidence.AddRange(result.Evidence);

        var time = world.GetResource<TimeResource>();
        store.LastExecutionTick = time.Tick;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name,
                $"{activeGoals.Count} active goals, {inferenceStore.Inferences.Count} inferences",
                $"{result.Plans.Count} plans, {result.Evidence.Count} evidence entries",
                $"Strategy={Strategy.GetType().Name}");
        }
    }
}
