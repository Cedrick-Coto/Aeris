namespace Aeris.Engine;

public sealed class DecisionSystem : ISystem
{
    public string Name => "DecisionSystem";
    public SystemPhase Phase => SystemPhase.Planning;
    public int Priority => 50;

    public IDecisionStrategy Strategy { get; set; } = new FeasibilityThresholdPolicy();

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<PlanStore>())
            return;
        if (!world.HasResource<WorldModelState>())
            return;
        if (!world.HasResource<AffectState>())
            return;
        if (!world.HasResource<GoalStore>())
            return;

        var planStore = world.GetResource<PlanStore>();
        var model = world.GetResource<WorldModelState>();
        var affect = world.GetResource<AffectState>();
        var goalStore = world.GetResource<GoalStore>();

        var activeGoals = new List<GoalData>();
        foreach (var kvp in goalStore.All)
            activeGoals.AddRange(kvp.Value.Where(g => g.IsActive));

        var context = new DecisionContext
        {
            CandidatePlans = planStore.Plans,
            WorldModel = model,
            Affect = affect,
            ActiveGoals = activeGoals
        };

        var result = Strategy.Decide(context);

        if (!world.HasResource<ActionStore>())
            world.AddResource(new ActionStore());

        var store = world.GetResource<ActionStore>();
        store.LastResult = result;
        store.LastExecutionTick = world.GetResource<TimeResource>().Tick;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name,
                $"{context.CandidatePlans.Count} candidates, {activeGoals.Count} goals",
                $"Status={result.Status}, Action={result.Action.Action}, PlanId={result.SelectedPlanId}",
                $"Strategy={Strategy.GetType().Name}");
        }
    }
}
