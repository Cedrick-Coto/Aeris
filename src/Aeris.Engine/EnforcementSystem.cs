namespace Aeris.Engine;

public sealed class EnforcementSystem : ISystem
{
    public string Name => "EnforcementSystem";
    public SystemPhase Phase => SystemPhase.Planning;
    public int Priority => 70;

    public IEnforcementPolicy Policy { get; set; } = new StrictPolicy();

    public void Execute(World world, float deltaTime)
    {
        if (!world.HasResource<AuditStore>())
            return;

        var auditStore = world.GetResource<AuditStore>();
        if (auditStore.LastResult == null)
            return;

        var context = new EnforcementContext
        {
            AuditResult = auditStore.LastResult
        };

        var result = Policy.Apply(context);

        if (!world.HasResource<EnforcementStore>())
            world.AddResource(new EnforcementStore());

        var store = world.GetResource<EnforcementStore>();
        store.LastResult = result;
        store.LastExecutionTick = world.GetResource<TimeResource>().Tick;

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name,
                $"AuditResult: Passed={context.AuditResult.Passed}, Violations={context.AuditResult.Violations.Count}",
                $"Verdict={result.Verdict}, Policy={Policy.Name}",
                $"Reason={result.Evidence.Reason}");
        }
    }
}
