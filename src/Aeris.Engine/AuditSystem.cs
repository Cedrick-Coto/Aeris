namespace Aeris.Engine;

public sealed class AuditSystem : ISystem
{
    public string Name => "AuditSystem";
    public SystemPhase Phase => SystemPhase.Planning;
    public int Priority => 60;

    public IAuditStrategy Strategy { get; set; } = new SequentialRuleEvaluator();
    public RuleRegistry Registry { get; set; } = new();

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<ActionStore>())
            return;

        var actionStore = world.GetResource<ActionStore>();
        var decision = actionStore.LastResult;

        var artifact = new DecisionResultAuditable
        {
            ArtifactId = decision.SelectedPlanId ?? 0,
            Decision = decision
        };

        var rules = Registry.GetRulesFor(artifact.ArtifactType);

        var context = new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        };

        var result = Strategy.Audit(context);

        if (!world.HasResource<AuditStore>())
            world.AddResource(new AuditStore());

        var store = world.GetResource<AuditStore>();
        store.LastResult = result;
        store.LastExecutionTick = world.GetResource<TimeResource>().Tick;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name,
                $"Artifact={artifact.ArtifactType}:{artifact.ArtifactId}, {rules.Count} rules",
                $"Passed={result.Passed}, Violations={result.Violations.Count}, MaxSeverity={result.MaxSeverity}",
                $"Strategy={Strategy.GetType().Name}");
        }
    }
}
