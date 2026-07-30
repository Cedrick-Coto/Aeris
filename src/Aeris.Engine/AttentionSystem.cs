namespace Aeris.Engine;

public sealed class AttentionSystem : ISystem
{
    public string Name => "AttentionSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 20;

    public int AttentionBudget { get; set; } = 5;

    public IAttentionStrategy Strategy { get; set; } = new SalienceAttentionStrategy();

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var affect = world.HasResource<AffectState>() ? world.GetResource<AffectState>() : AffectState.Default;

        if (!world.HasResource<PerceptBatch>())
        {
            if (!world.HasResource<AttendedPercepts>())
                world.AddResource(new AttendedPercepts { Tick = time.Tick });
            return;
        }

        var allPercepts = world.GetResource<PerceptBatch>();
        int budget = CalculateBudget(affect);

        var context = new AttentionContext
        {
            Percepts = allPercepts.Percepts,
            Affect = affect,
            Budget = budget
        };

        var attended = Strategy.Select(context);

        if (world.HasResource<AttendedPercepts>())
        {
            var existing = world.GetResource<AttendedPercepts>();
            existing.Percepts = attended;
            existing.Tick = time.Tick;
        }
        else
        {
            world.AddResource(new AttendedPercepts { Percepts = attended, Tick = time.Tick });
        }

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"{allPercepts.Percepts.Count} raw, budget={budget}",
                $"{attended.Count} attended",
                $"Strategy={Strategy.Name} (arousal={affect.Curiosity:F2})");
        }
    }

    private int CalculateBudget(AffectState affect)
    {
        int budget = AttentionBudget;
        if (affect.Stress > 0.7f)
            budget = Math.Max(1, budget - 2);
        if (affect.Curiosity > 0.7f)
            budget += 1;
        return budget;
    }
}
