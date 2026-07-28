using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class EngineTests
{
    [Fact]
    public void RunOneTick_ShouldExecuteWithoutErrors()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        var act = () => engine.RunOneTick();

        act.Should().NotThrow();
    }

    [Fact]
    public void RunOneTick_ShouldIncrementTick()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        engine.RunOneTick();

        engine.Tick.Should().Be(1);
    }

    [Fact]
    public void RunMultipleTicks_ShouldIncrementCorrectly()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        engine.RunOneTick();
        engine.RunOneTick();
        engine.RunOneTick();

        engine.Tick.Should().Be(3);
    }

    [Fact]
    public void RunOneTick_ShouldUpdateStats()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        engine.RunOneTick();

        var stats = world.GetResource<EngineStats>();
        stats.Tick.Should().Be(1);
        stats.TickDuration.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void RunOneTick_ShouldExecuteRegisteredSystems()
    {
        var world = CreateWorld();
        var engine = new Engine(world);

        var counter = new CounterSystem();
        engine.RegisterSystem(counter);
        engine.Initialize();

        engine.RunOneTick();

        counter.ExecutionCount.Should().Be(1);
    }

    private static World CreateWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        return world;
    }

    private static Engine CreateEngine(World world)
    {
        var engine = new Engine(world);
        engine.RegisterSystem(new CounterSystem());
        engine.Initialize();
        return engine;
    }
}
