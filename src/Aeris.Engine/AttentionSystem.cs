namespace Aeris.Engine;

public sealed class AttentionSystem : ISystem
{
    public string Name => "AttentionSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 20;

    public int AttentionBudget { get; set; } = 5;

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
        var attended = new List<Percept>();
        int budget = CalculateBudget(affect);

        for (int i = 0; i < allPercepts.Percepts.Count; i++)
        {
            var p = allPercepts.Percepts[i];
            float noveltyMod = 1f + affect.Novelty * 0.5f;
            float threatMod = p.Type == PerceptType.Aura ? affect.Threat * 0.3f : 0f;
            p.Salience = noveltyMod + threatMod + p.Confidence;
            allPercepts.Percepts[i] = p;
        }

        allPercepts.Percepts.Sort((a, b) => b.Salience.CompareTo(a.Salience));

        for (int i = 0; i < Math.Min(budget, allPercepts.Percepts.Count); i++)
        {
            attended.Add(allPercepts.Percepts[i]);
        }

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
            trace.Record(Name, $"{allPercepts.Percepts.Count} raw, budget={budget}", $"{attended.Count} attended", $"Salience filter with affect modulation (arousal={affect.Curiosity:F2})");
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
