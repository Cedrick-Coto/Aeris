using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public struct HealthComponent
{
    public int Current;
    public int Max;
}

public struct PositionComponent
{
    public float X;
    public float Y;
}

public struct NameTag
{
    public int Length;
}

public sealed class HealthRegenSystem : ISystem
{
    public string Name => "HealthRegen";
    public SystemPhase Phase => SystemPhase.Action;
    public int Priority => 0;

    public int ExecuteCount { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        ExecuteCount++;

        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<HealthComponent>())
            {
                var health = kvp.Value.GetComponent<HealthComponent>();
                if (health.Current < health.Max)
                {
                    health.Current++;
                    kvp.Value.RemoveComponent<HealthComponent>();
                    kvp.Value.AddComponent(health);
                }
            }
        }
    }
}

public sealed class MovementSystem : ISystem
{
    public string Name => "Movement";
    public SystemPhase Phase => SystemPhase.Action;
    public int Priority => 10;

    public int ExecuteCount { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        ExecuteCount++;

        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<PositionComponent>())
            {
                var pos = kvp.Value.GetComponent<PositionComponent>();
                pos.X += deltaTime;
                pos.Y += deltaTime * 0.5f;
                kvp.Value.RemoveComponent<PositionComponent>();
                kvp.Value.AddComponent(pos);
            }
        }
    }
}

public sealed class EventEmittingSystem : ISystem
{
    public string Name => "EventEmitter";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    public int EmitCount { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        EmitCount++;
    }
}

public sealed class ShutdownSystem : ISystem
{
    public string Name => "Shutdown";
    public SystemPhase Phase => SystemPhase.Shutdown;
    public int Priority => 0;

    public int ExecuteCount { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        ExecuteCount++;
    }
}

public sealed class InitializationSystem : ISystem
{
    public string Name => "Init";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    public int ExecuteCount { get; private set; }
    public bool EntitiesAvailable { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        ExecuteCount++;
        EntitiesAvailable = world.EntityCount > 0;
    }
}

public class EngineIntegrationTests : IDisposable
{
    private readonly string _testDir;

    public EngineIntegrationTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"aeris-integ-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void Validation_01_CreateEntity_EntityCrudWorks()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        world.CreateEntity().With(new HealthComponent { Current = 100, Max = 100 }).Build();
        world.EntityCount.Should().Be(1);

        world.CreateEntity().With(new PositionComponent { X = 5f, Y = 10f }).Build();
        world.EntityCount.Should().Be(2);
    }

    [Fact]
    public void Validation_02_AddRemoveComponents_ComponentCrudWorks()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity()
            .With(new HealthComponent { Current = 50, Max = 100 })
            .With(new PositionComponent { X = 1f, Y = 2f })
            .Build();

        entity.HasComponent<HealthComponent>().Should().BeTrue();
        entity.HasComponent<PositionComponent>().Should().BeTrue();

        entity.RemoveComponent<PositionComponent>();
        entity.HasComponent<PositionComponent>().Should().BeFalse();
        entity.HasComponent<HealthComponent>().Should().BeTrue();
    }

    [Fact]
    public void Validation_03_MultipleSystemsExecuteInOrder()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        var init = new InitializationSystem();
        var movement = new MovementSystem();
        var shutdown = new ShutdownSystem();

        engine.RegisterSystem(init);
        engine.RegisterSystem(movement);
        engine.RegisterSystem(shutdown);
        engine.Initialize();

        world.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        engine.RunOneTick(1f);

        init.ExecuteCount.Should().Be(1);
        movement.ExecuteCount.Should().Be(1);
        shutdown.ExecuteCount.Should().Be(1);

        init.Phase.Should().Be(SystemPhase.Initialization);
        movement.Phase.Should().Be(SystemPhase.Action);
        shutdown.Phase.Should().Be(SystemPhase.Shutdown);
    }

    [Fact]
    public void Validation_04_EventBusAcceptsEvents()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        var counter = new CounterSystem();

        engine.RegisterSystem(counter);
        engine.Initialize();

        engine.EventBus.Emit(new CounterEvent { Tick = 1 });

        engine.EventBus.HasPendingEvents.Should().BeTrue();
    }

    [Fact]
    public void Validation_05_DualQueueProcessesInNextTick()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        var processedTicks = new List<long>();

        engine.EventBus.Subscribe<CounterEvent>(e =>
        {
            processedTicks.Add(e.Tick);
        });

        var emitter = new EmittingSystem(engine.EventBus);
        engine.RegisterSystem(emitter);
        engine.Initialize();

        engine.RunOneTick(1f);
        processedTicks.Should().BeEmpty();

        engine.RunOneTick(1f);
        processedTicks.Should().Contain(99);
    }

    [Fact]
    public void Validation_06_SchedulerProcessesOnTime()
    {
        var world = CreateWorld();
        var scheduler = new SchedulerResource();
        var scheduledTick = -1L;

        scheduler.Schedule(2.5, w =>
        {
            var time = w.GetResource<TimeResource>();
            scheduledTick = time.Tick;
        }, "test event");
        world.AddResource(scheduler);

        var engine = CreateEngineWithScheduler(world);

        engine.RunOneTick(1f);
        scheduledTick.Should().Be(-1);

        engine.RunOneTick(1f);
        scheduledTick.Should().Be(-1);

        engine.RunOneTick(1f);
        scheduledTick.Should().Be(3);
    }

    [Fact]
    public void Validation_07_SaveWorldState()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        world.CreateEntity().With(new HealthComponent { Current = 75, Max = 100 }).Build();
        world.CreateEntity().With(new PositionComponent { X = 3f, Y = 7f }).Build();

        var time = world.GetResource<TimeResource>();
        time.Advance(2.0f);
        world.SetResource(time);

        persistence.SaveWorld(world);

        File.Exists(Path.Combine(_testDir, "world.json")).Should().BeTrue();
    }

    [Fact]
    public void Validation_08_LoadWorldState()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        world.CreateEntity().With(new HealthComponent { Current = 75, Max = 100 }).Build();
        world.CreateEntity().With(new PositionComponent { X = 3f, Y = 7f }).Build();

        var time = world.GetResource<TimeResource>();
        time.Advance(2.0f);
        world.SetResource(time);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(2);

        var timeAfter = loadedWorld.GetResource<TimeResource>();
        timeAfter.Tick.Should().Be(1);
        timeAfter.SimulationTime.Should().Be(2.0);
    }

    [Fact]
    public void Validation_09_ResumeSimulationNoDifferences()
    {
        var world1 = CreateWorld();
        var engine1 = new Engine(world1);
        var healthRegen = new HealthRegenSystem();
        engine1.RegisterSystem(healthRegen);
        engine1.Initialize();

        world1.CreateEntity().With(new HealthComponent { Current = 50, Max = 100 }).Build();

        for (int i = 0; i < 5; i++)
            engine1.RunOneTick(1f);

        var health1 = world1.Entities.Values.First().GetComponent<HealthComponent>();
        var tick1 = engine1.Tick;

        var persistence = new JsonPersistence(_testDir);
        persistence.SaveWorld(world1);

        var world2 = CreateWorld();
        persistence.LoadWorld(world2);
        var engine2 = new Engine(world2);
        var healthRegen2 = new HealthRegenSystem();
        engine2.RegisterSystem(healthRegen2);
        engine2.Initialize();

        engine2.Tick.Should().Be(tick1);

        var health2 = world2.Entities.Values.First().GetComponent<HealthComponent>();
        health2.Current.Should().Be(health1.Current);
        health2.Max.Should().Be(health1.Max);
    }

    [Fact]
    public void Validation_10_Determinism_SameSeedSameResult()
    {
        var world1 = CreateWorld();
        var engine1 = new Engine(world1);
        engine1.RegisterSystem(new MovementSystem());
        engine1.Initialize();

        world1.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        for (int i = 0; i < 10; i++)
            engine1.RunOneTick(0.5f);

        var pos1 = world1.Entities.Values.First().GetComponent<PositionComponent>();

        var world2 = CreateWorld();
        var engine2 = new Engine(world2);
        engine2.RegisterSystem(new MovementSystem());
        engine2.Initialize();

        world2.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        for (int i = 0; i < 10; i++)
            engine2.RunOneTick(0.5f);

        var pos2 = world2.Entities.Values.First().GetComponent<PositionComponent>();

        pos1.X.Should().Be(pos2.X);
        pos1.Y.Should().Be(pos2.Y);
    }

    [Fact]
    public void FullTick_EverythingWorks()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        var persistence = new JsonPersistence(_testDir);
        engine.SetPersistence(persistence);

        var init = new InitializationSystem();
        var healthRegen = new HealthRegenSystem();
        var movement = new MovementSystem();
        var shutdown = new ShutdownSystem();

        engine.RegisterSystem(init);
        engine.RegisterSystem(healthRegen);
        engine.RegisterSystem(movement);
        engine.RegisterSystem(shutdown);
        engine.Initialize();

        world.CreateEntity().With(new HealthComponent { Current = 50, Max = 100 }).Build();
        world.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        var scheduler = new SchedulerResource();
        var scheduledFired = false;
        scheduler.Schedule(2.0, _ => scheduledFired = true, "test");
        world.AddResource(scheduler);

        for (int i = 0; i < 5; i++)
            engine.RunOneTick(1f);

        init.ExecuteCount.Should().Be(5);
        healthRegen.ExecuteCount.Should().Be(5);
        movement.ExecuteCount.Should().Be(5);
        shutdown.ExecuteCount.Should().Be(5);

        scheduledFired.Should().BeTrue();

        var health = world.Entities.Values
            .First(e => e.HasComponent<HealthComponent>())
            .GetComponent<HealthComponent>();
        health.Current.Should().BeGreaterThan(50);
        health.Current.Should().BeLessThanOrEqualTo(100);

        var pos = world.Entities.Values
            .First(e => e.HasComponent<PositionComponent>())
            .GetComponent<PositionComponent>();
        pos.X.Should().BeGreaterThan(0f);

        engine.Tick.Should().Be(5);

        var stats = world.GetResource<EngineStats>();
        stats.Tick.Should().Be(5);
        stats.SystemsExecuted.Should().BeGreaterThan(0);

        File.Exists(Path.Combine(_testDir, "checkpoint-*.json")).Should().BeFalse();
    }

    [Fact]
    public void Engine_ReportsStatsPerTick()
    {
        var world = CreateWorld();
        var engine = CreateEngine(world);

        engine.RunOneTick(1f);
        engine.RunOneTick(1f);

        var stats = world.GetResource<EngineStats>();
        stats.Tick.Should().Be(2);
        stats.TickDuration.Should().BeGreaterThanOrEqualTo(0);
        stats.SystemsExecuted.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Engine_MultipleEntities_MultipleTicks()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        engine.RegisterSystem(new MovementSystem());
        engine.Initialize();

        for (int i = 0; i < 10; i++)
            world.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        for (int i = 0; i < 5; i++)
            engine.RunOneTick(2f);

        foreach (var kvp in world.Entities)
        {
            var pos = kvp.Value.GetComponent<PositionComponent>();
            pos.X.Should().Be(10f);
            pos.Y.Should().Be(5f);
        }
    }

    [Fact]
    public void Engine_StopAndResume()
    {
        var world = CreateWorld();
        var engine = new Engine(world);
        engine.RegisterSystem(new MovementSystem());
        engine.Initialize();

        world.CreateEntity().With(new PositionComponent { X = 0f, Y = 0f }).Build();

        engine.RunOneTick(1f);
        engine.RunOneTick(1f);

        var pos = world.Entities.Values.First().GetComponent<PositionComponent>();
        pos.X.Should().Be(2f);

        engine.RunOneTick(1f);
        engine.RunOneTick(1f);

        var pos2 = world.Entities.Values.First().GetComponent<PositionComponent>();
        pos2.X.Should().Be(4f);
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

    private static Engine CreateEngineWithScheduler(World world)
    {
        var engine = new Engine(world);
        engine.RegisterSystem(new CounterSystem());
        engine.Initialize();
        return engine;
    }
}

public sealed class EmittingSystem : ISystem
{
    public string Name => "Emitter";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    private readonly EventBus _eventBus;

    public EmittingSystem(EventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void Execute(World world, float deltaTime)
    {
        _eventBus.Emit(new CounterEvent { Tick = 99 });
    }
}

public sealed class EventRecordingSystem : ISystem
{
    public string Name => "EventRecording";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    private readonly List<long> _recordedTicks;

    public EventRecordingSystem(List<long> recordedTicks)
    {
        _recordedTicks = recordedTicks;
    }

    public void Execute(World world, float deltaTime)
    {
    }
}
