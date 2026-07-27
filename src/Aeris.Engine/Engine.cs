using System.Diagnostics;

namespace Aeris.Engine;

public sealed class Engine
{
    private readonly World _world;

    public Engine(World world)
    {
        _world = world;
    }

    public void RunOneTick(float deltaTime = 1f)
    {
        var time = _world.GetResource<TimeResource>();
        var stats = _world.GetResource<EngineStats>();
        var stopwatch = Stopwatch.StartNew();

        time.Advance(deltaTime);

        stats.Tick = time.Tick;
        stats.TickDuration = 0;
        stats.SystemsExecuted = 0;

        stopwatch.Stop();
        stats.TickDuration = stopwatch.Elapsed.TotalMilliseconds;

        _world.SetResource(time);
        _world.SetResource(stats);
    }

    public long Tick => _world.GetResource<TimeResource>().Tick;
}
