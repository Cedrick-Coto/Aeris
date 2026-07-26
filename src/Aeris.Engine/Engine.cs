using System.Diagnostics;

namespace Aeris.Engine;

public sealed class Engine
{
    private readonly World _world;
    private long _tick;

    public Engine(World world)
    {
        _world = world;
    }

    public void RunOneTick(float deltaTime = 1f)
    {
        _tick++;
        var stats = _world.GetResource<EngineStats>();
        var stopwatch = Stopwatch.StartNew();

        stats.Tick = _tick;
        stats.TickDuration = 0;
        stats.SystemsExecuted = 0;

        stopwatch.Stop();
        stats.TickDuration = stopwatch.Elapsed.TotalMilliseconds;
        _world.SetResource(stats);
    }

    public long Tick => _tick;
}
