namespace Aeris.Engine;

public sealed class AffectSystem : ISystem
{
    public string Name => "AffectSystem";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 10;

    public float HomeostasisRate { get; set; } = 0.01f;

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<AffectState>())
            world.AddResource(AffectState.Default);

        var affect = world.GetResource<AffectState>();
        var time = world.GetResource<TimeResource>();

        affect = ApplyHomeostasis(affect);
        affect = ApplyPerceptModulation(affect, world);
        affect = affect.Clamped();

        world.SetResource(affect);

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"Cur={affect.Curiosity:F2} Str={affect.Stress:F2} Thr={affect.Threat:F2}",
                $"homeostasis rate={HomeostasisRate}", $"Continuous vector updated, no discrete emotions");
        }
    }

    private AffectState ApplyHomeostasis(AffectState state)
    {
        var defaults = AffectState.Default;
        return new AffectState
        {
            Curiosity = MoveToward(state.Curiosity, defaults.Curiosity, HomeostasisRate),
            Stress = MoveToward(state.Stress, defaults.Stress, HomeostasisRate),
            Confidence = MoveToward(state.Confidence, defaults.Confidence, HomeostasisRate),
            Trust = MoveToward(state.Trust, defaults.Trust, HomeostasisRate),
            Novelty = MoveToward(state.Novelty, defaults.Novelty, HomeostasisRate),
            Attachment = MoveToward(state.Attachment, defaults.Attachment, HomeostasisRate),
            Threat = MoveToward(state.Threat, defaults.Threat, HomeostasisRate),
            RewardExpectation = MoveToward(state.RewardExpectation, defaults.RewardExpectation, HomeostasisRate),
            CognitiveLoad = MoveToward(state.CognitiveLoad, defaults.CognitiveLoad, HomeostasisRate)
        };
    }

    private static AffectState ApplyPerceptModulation(AffectState state, World world)
    {
        if (!world.HasResource<AttendedPercepts>())
            return state;

        var attended = world.GetResource<AttendedPercepts>();
        if (attended.Percepts.Count == 0)
            return state;

        int novelCount = 0;
        int auraThreatCount = 0;

        foreach (var p in attended.Percepts)
        {
            if (p.Type == PerceptType.Visual && p.Distance < 5f)
                state.Threat = Math.Min(1f, state.Threat + 0.05f);

            if (p.Type == PerceptType.Visual && p.Confidence > 0.9f)
                state.Novelty = Math.Min(1f, state.Novelty + 0.02f);

            if (p.Type == PerceptType.Aura)
            {
                novelCount++;
                if (p.AuraSignature > 0.7f)
                    auraThreatCount++;
            }
        }

        if (novelCount > 0)
            state.Novelty = Math.Min(1f, state.Novelty + novelCount * 0.03f);

        if (auraThreatCount > 0)
            state.Threat = Math.Min(1f, state.Threat + auraThreatCount * 0.08f);

        return state;
    }

    private static float MoveToward(float current, float target, float rate)
    {
        if (current < target)
            return Math.Min(target, current + rate);
        return Math.Max(target, current - rate);
    }
}
