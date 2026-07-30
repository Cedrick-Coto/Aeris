namespace Aeris.Engine;

public sealed class WorldModelSystem : ISystem
{
    public string Name => "WorldModelSystem";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 5;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        if (!world.HasResource<WorldModelState>())
            world.AddResource(new WorldModelState());

        var model = world.GetResource<WorldModelState>();

        if (world.HasResource<AttendedPercepts>() && model.LastUpdateTick != time.Tick)
        {
            var attended = world.GetResource<AttendedPercepts>();
            foreach (var p in attended.Percepts)
            {
                if (!model.KnownEntityIds.Contains(p.Source.Value))
                    model.KnownEntityIds.Add(p.Source.Value);
            }
            model.LastUpdateTick = time.Tick;
        }

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"known={model.KnownEntityIds.Count}", $"known={model.KnownEntityIds.Count}", $"Track attended entities");
        }
    }
}
