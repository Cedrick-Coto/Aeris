using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class SchedulerResourceTests
{
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

    [Fact]
    public void Schedule_ShouldAddEvent()
    {
        var scheduler = new SchedulerResource();

        scheduler.Schedule(10.0, _ => { }, "test event");

        scheduler.PendingCount.Should().Be(1);
        scheduler.HasPendingEvents.Should().BeTrue();
    }

    [Fact]
    public void Process_ShouldExecuteDueEvent()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var executed = false;

        scheduler.Schedule(5.0, _ => executed = true, "test");
        world.AddResource(scheduler);

        scheduler.Process(world, 10.0);

        executed.Should().BeTrue();
        scheduler.PendingCount.Should().Be(0);
    }

    [Fact]
    public void Process_ShouldNotExecuteFutureEvents()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var executed = false;

        scheduler.Schedule(100.0, _ => executed = true, "future");
        world.AddResource(scheduler);

        scheduler.Process(world, 10.0);

        executed.Should().BeFalse();
        scheduler.PendingCount.Should().Be(1);
    }

    [Fact]
    public void Process_ShouldExecuteInTimeOrder()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var order = new List<string>();

        scheduler.Schedule(30.0, _ => order.Add("A"), "A");
        scheduler.Schedule(10.0, _ => order.Add("B"), "B");
        scheduler.Schedule(20.0, _ => order.Add("C"), "C");
        world.AddResource(scheduler);

        scheduler.Process(world, 50.0);

        order.Should().Equal("B", "C", "A");
    }

    [Fact]
    public void Process_ShouldHandleMultipleEventsAtSameTime()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var count = 0;

        scheduler.Schedule(5.0, _ => count++, "first");
        scheduler.Schedule(5.0, _ => count++, "second");
        scheduler.Schedule(5.0, _ => count++, "third");
        world.AddResource(scheduler);

        scheduler.Process(world, 5.0);

        count.Should().Be(3);
    }

    [Fact]
    public void Process_ShouldPassWorldToCallback()
    {
        var world = CreateWorld();
        world.AddResource(new EngineStats { Tick = 42 });
        var scheduler = new SchedulerResource();
        long? receivedTick = null;

        scheduler.Schedule(1.0, w =>
        {
            var stats = w.GetResource<EngineStats>();
            receivedTick = stats.Tick;
        }, "read world");
        world.AddResource(scheduler);

        scheduler.Process(world, 5.0);

        receivedTick.Should().Be(42);
    }

    [Fact]
    public void Process_ShouldExecuteOnlyDueEvents()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var executed = new List<string>();

        scheduler.Schedule(5.0, _ => executed.Add("A"), "A");
        scheduler.Schedule(10.0, _ => executed.Add("B"), "B");
        scheduler.Schedule(15.0, _ => executed.Add("C"), "C");
        world.AddResource(scheduler);

        scheduler.Process(world, 10.0);

        executed.Should().Equal("A", "B");
        scheduler.PendingCount.Should().Be(1);
    }

    [Fact]
    public void Clear_ShouldRemoveAllEvents()
    {
        var scheduler = new SchedulerResource();
        scheduler.Schedule(1.0, _ => { }, "a");
        scheduler.Schedule(2.0, _ => { }, "b");

        scheduler.Clear();

        scheduler.PendingCount.Should().Be(0);
        scheduler.HasPendingEvents.Should().BeFalse();
    }

    [Fact]
    public void Schedule_ShouldAllowChainedCallbacks()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var results = new List<int>();

        scheduler.Schedule(1.0, w =>
        {
            results.Add(1);
            var s = w.GetResource<SchedulerResource>();
            s.Schedule(2.0, _ => results.Add(2), "chain");
        }, "first");
        world.AddResource(scheduler);

        scheduler.Process(world, 1.0);
        results.Should().Equal(1);
        scheduler.PendingCount.Should().Be(1);

        scheduler.Process(world, 2.0);
        results.Should().Equal(1, 2);
        scheduler.PendingCount.Should().Be(0);
    }

    [Fact]
    public void Engine_ShouldProcessSchedulerDuringTick()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var executed = false;

        scheduler.Schedule(0.0, _ => executed = true, "immediate");
        world.AddResource(scheduler);

        var engine = CreateEngine(world);

        engine.RunOneTick(1f);

        executed.Should().BeTrue();
    }

    [Fact]
    public void Engine_ShouldProcessSchedulerAtCorrectTime()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var executedAt = -1.0;

        scheduler.Schedule(2.5, w =>
        {
            var time = w.GetResource<TimeResource>();
            executedAt = time.SimulationTime;
        }, "delayed");
        world.AddResource(scheduler);

        var engine = CreateEngine(world);

        engine.RunOneTick(1f);
        executedAt.Should().Be(-1.0);

        engine.RunOneTick(1f);
        executedAt.Should().Be(-1.0);

        engine.RunOneTick(1f);
        executedAt.Should().Be(3.0);
    }

    [Fact]
    public void PendingCount_ShouldDecreaseAsEventsFire()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();

        scheduler.Schedule(1.0, _ => { }, "a");
        scheduler.Schedule(2.0, _ => { }, "b");
        scheduler.Schedule(3.0, _ => { }, "c");
        world.AddResource(scheduler);

        scheduler.PendingCount.Should().Be(3);

        scheduler.Process(world, 1.0);
        scheduler.PendingCount.Should().Be(2);

        scheduler.Process(world, 2.0);
        scheduler.PendingCount.Should().Be(1);

        scheduler.Process(world, 3.0);
        scheduler.PendingCount.Should().Be(0);
    }
}
