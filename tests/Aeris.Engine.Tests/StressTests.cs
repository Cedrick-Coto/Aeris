using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class StressTests : IDisposable
{
    private readonly string _tempDir;

    public StressTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"aeris-stress-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void OneMillionTicks_ConsistentState()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        engine.RegisterSystem(new StressMovementSystem());
        engine.Initialize();

        world.CreateEntity().With(new StressPosition { X = 0, Y = 0 }).Build();

        for (int i = 0; i < 1_000_000; i++)
            engine.RunOneTick(0.016f);

        engine.Tick.Should().Be(1_000_000);

        var time = world.GetResource<TimeResource>();
        time.Tick.Should().Be(1_000_000);
        time.SimulationTime.Should().BeApproximately(16_000.0, 0.01);
    }

    [Fact]
    public void OneHundredThousandEntities_CreateDestroyCycle()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        engine.RegisterSystem(new StressMovementSystem());
        engine.Initialize();

        for (int cycle = 0; cycle < 10; cycle++)
        {
            var ids = new List<EntityId>();
            for (int i = 0; i < 100_000; i++)
            {
                var entity = world.CreateEntity()
                    .With(new StressPosition { X = i, Y = i })
                    .Build();
                ids.Add(entity.Id);
            }

            world.EntityCount.Should().Be(100_000);

            engine.RunOneTick(0.016f);

            foreach (var id in ids)
                world.RemoveEntity(id);

            world.EntityCount.Should().Be(0);
        }
    }

    [Fact]
    public void EventBus_HighVolumeDeferred()
    {
        var bus = new EventBus();
        var received = 0;
        bus.Subscribe<StressTestEvent>(e => received++);

        for (int tick = 0; tick < 1000; tick++)
        {
            for (int i = 0; i < 100; i++)
                bus.Emit(new StressTestEvent { Value = tick * 100 + i });

            bus.AdvanceTick();
            bus.Flush();
        }

        received.Should().Be(100_000);
    }

    [Fact]
    public void Scheduler_LargeQueueProcessAll()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var fired = 0;

        for (int i = 0; i < 10_000; i++)
        {
            scheduler.Schedule(i * 0.1, _ => fired++, $"event-{i}");
        }

        world.AddResource(scheduler);

        scheduler.Process(world, 1_000.0);

        fired.Should().Be(10_000);
    }

    [Fact]
    public void Persistence_LargeWorldRoundTrip()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_tempDir);

        for (int i = 0; i < 10_000; i++)
        {
            world.CreateEntity()
                .With(new StressPosition { X = i * 0.1, Y = i * 0.2 })
                .With(new StressHealth { Current = i % 100, Max = 100 })
                .Build();
        }

        var time = world.GetResource<TimeResource>();
        time.Advance(100.0f);
        world.SetResource(time);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(10_000);

        var loadedTime = loadedWorld.GetResource<TimeResource>();
        loadedTime.Tick.Should().Be(time.Tick);
        loadedTime.SimulationTime.Should().Be(time.SimulationTime);
    }

    [Fact]
    public void Determinism_LongRun()
    {
        var world1 = CreateWorld();
        var engine1 = new Engine(world1);
        engine1.RegisterSystem(new StressMovementSystem());
        engine1.Initialize();

        world1.CreateEntity().With(new StressPosition { X = 0, Y = 0 }).Build();

        for (int i = 0; i < 10_000; i++)
            engine1.RunOneTick(0.016f);

        var pos1 = world1.Entities.Values.First().GetComponent<StressPosition>();

        var world2 = CreateWorld();
        var engine2 = new Engine(world2);
        engine2.RegisterSystem(new StressMovementSystem());
        engine2.Initialize();

        world2.CreateEntity().With(new StressPosition { X = 0, Y = 0 }).Build();

        for (int i = 0; i < 10_000; i++)
            engine2.RunOneTick(0.016f);

        var pos2 = world2.Entities.Values.First().GetComponent<StressPosition>();

        pos1.X.Should().Be(pos2.X);
        pos1.Y.Should().Be(pos2.Y);
    }

    [Fact]
    public void MixedWorkload_IntensiveTick()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        engine.RegisterSystem(new StressMovementSystem());
        engine.RegisterSystem(new StressHealthSystem());
        engine.Initialize();

        for (int i = 0; i < 10_000; i++)
        {
            world.CreateEntity()
                .With(new StressPosition { X = i, Y = i })
                .With(new StressHealth { Current = i % 100, Max = 100 })
                .Build();
        }

        var scheduler = new SchedulerResource();
        scheduler.Schedule(500.0, w => { }, "halfway");
        scheduler.Schedule(1000.0, w => { }, "end");
        world.AddResource(scheduler);

        var bus = engine.EventBus;
        var eventsReceived = 0;
        bus.Subscribe<StressTestEvent>(e => eventsReceived++);

        for (int tick = 0; tick < 1000; tick++)
        {
            engine.RunOneTick(0.016f);

            if (tick % 100 == 0)
                bus.Emit(new StressTestEvent { Value = tick });
        }

        engine.Tick.Should().Be(1000);
        eventsReceived.Should().Be(10);
    }

    [Fact]
    public void PropertyBased_ManyEntities_ComponentIntegrity()
    {
        var rng = new Random(42);
        var world = CreateWorld();

        for (int i = 0; i < 10_000; i++)
        {
            world.CreateEntity()
                .With(new StressPosition { X = rng.NextDouble(), Y = rng.NextDouble() })
                .Build();
        }

        world.EntityCount.Should().Be(10_000);

        foreach (var kvp in world.Entities)
        {
            kvp.Value.HasComponent<StressPosition>().Should().BeTrue();
            var pos = kvp.Value.GetComponent<StressPosition>();
            pos.X.Should().BeGreaterThanOrEqualTo(0);
            pos.X.Should().BeLessThanOrEqualTo(1);
            pos.Y.Should().BeGreaterThanOrEqualTo(0);
            pos.Y.Should().BeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    public void Engine_NoExceptions_AfterMillionTicks()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        var persistence = new JsonPersistence(_tempDir);
        engine.SetPersistence(persistence);

        engine.RegisterSystem(new StressMovementSystem());
        engine.Initialize();

        world.CreateEntity().With(new StressPosition { X = 0, Y = 0 }).Build();

        var act = () =>
        {
            for (int i = 0; i < 1_000_000; i++)
                engine.RunOneTick(0.016f);
        };

        act.Should().NotThrow();
    }

    private static World CreateWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        return world;
    }
}

public struct StressPosition
{
    public double X;
    public double Y;
}

public struct StressHealth
{
    public int Current;
    public int Max;
}

public struct StressTestEvent
{
    public int Value;
}

public sealed class StressMovementSystem : ISystem
{
    public string Name => "StressMovement";
    public SystemPhase Phase => SystemPhase.Action;
    public int Priority => 0;

    public void Execute(World world, float deltaTime)
    {
        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<StressPosition>())
            {
                var pos = kvp.Value.GetComponent<StressPosition>();
                pos.X += deltaTime;
                pos.Y += deltaTime * 0.5;
                kvp.Value.RemoveComponent<StressPosition>();
                kvp.Value.AddComponent(pos);
            }
        }
    }
}

public sealed class StressHealthSystem : ISystem
{
    public string Name => "StressHealth";
    public SystemPhase Phase => SystemPhase.Consequences;
    public int Priority => 0;

    public void Execute(World world, float deltaTime)
    {
        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<StressHealth>())
            {
                var health = kvp.Value.GetComponent<StressHealth>();
                if (health.Current < health.Max)
                {
                    health.Current++;
                    kvp.Value.RemoveComponent<StressHealth>();
                    kvp.Value.AddComponent(health);
                }
            }
        }
    }
}
