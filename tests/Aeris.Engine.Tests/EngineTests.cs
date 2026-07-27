using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class EngineTests
{
    [Fact]
    public void RunOneTick_ShouldExecuteWithoutErrors()
    {
        var world = CreateWorld();

        var engine = new Engine(world);

        var act = () => engine.RunOneTick();

        act.Should().NotThrow();
    }

    [Fact]
    public void RunOneTick_ShouldIncrementTick()
    {
        var world = CreateWorld();

        var engine = new Engine(world);

        engine.RunOneTick();

        engine.Tick.Should().Be(1);
    }

    [Fact]
    public void RunMultipleTicks_ShouldIncrementCorrectly()
    {
        var world = CreateWorld();

        var engine = new Engine(world);

        engine.RunOneTick();
        engine.RunOneTick();
        engine.RunOneTick();

        engine.Tick.Should().Be(3);
    }

    [Fact]
    public void RunOneTick_ShouldUpdateStats()
    {
        var world = CreateWorld();

        var engine = new Engine(world);

        engine.RunOneTick();

        var stats = world.GetResource<EngineStats>();
        stats.Tick.Should().Be(1);
        stats.TickDuration.Should().BeGreaterThanOrEqualTo(0);
    }

    private static World CreateWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        return world;
    }
}
