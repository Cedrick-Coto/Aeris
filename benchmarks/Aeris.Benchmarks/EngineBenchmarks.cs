using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

using EngineClass = Aeris.Engine.Engine;

namespace Aeris.Benchmarks;

[Config(typeof(AerisBenchmarkConfig))]
[MemoryDiagnoser]
public class EngineTickBenchmarks
{
    private Aeris.Engine.World _world = null!;
    private EngineClass _engine = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int EntityCount;

    [GlobalSetup]
    public void Setup()
    {
        _world = new Aeris.Engine.World();
        _world.AddResource(Aeris.Engine.TimeResource.Create());
        _world.AddResource(new Aeris.Engine.EngineStats());

        _engine = new EngineClass(_world);
        _engine.RegisterSystem(new BenchmarkMovementSystem());
        _engine.Initialize();

        for (int i = 0; i < EntityCount; i++)
        {
            _world.CreateEntity()
                .With(new PositionComponent { X = i * 0.1f, Y = i * 0.2f })
                .With(new VelocityComponent { Dx = 1f, Dy = 0.5f })
                .Build();
        }
    }

    [Benchmark]
    public void RunOneTick()
    {
        _engine.RunOneTick(0.016f);
    }

    [Benchmark]
    public void RunTenTicks()
    {
        for (int i = 0; i < 10; i++)
            _engine.RunOneTick(0.016f);
    }

    [Benchmark]
    public void RunHundredTicks()
    {
        for (int i = 0; i < 100; i++)
            _engine.RunOneTick(0.016f);
    }
}

[Config(typeof(AerisBenchmarkConfig))]
[MemoryDiagnoser]
public class EventBusBenchmarks
{
    private Aeris.Engine.EventBus _bus = null!;

    [Params(10, 100, 1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        _bus = new Aeris.Engine.EventBus();
        _bus.Subscribe<TestPayloadEvent>(e => { _ = e.Value; });
        _bus.Subscribe<BulkEvent>(e => { _ = e.Data; });
    }

    [IterationSetup]
    public void IterSetup()
    {
        _bus.Clear();
    }

    [Benchmark]
    public void EmitDeferred()
    {
        for (int i = 0; i < EventCount; i++)
            _bus.Emit(new TestPayloadEvent { Value = i });

        _bus.AdvanceTick();
        _bus.Flush();
    }

    [Benchmark]
    public void EmitImmediate()
    {
        for (int i = 0; i < EventCount; i++)
            _bus.Emit(new TestPayloadEvent { Value = i }, Aeris.Engine.EventDispatchType.Immediate);
    }

    [Benchmark]
    public void EmitAndReceiveMultipleHandlers()
    {
        var bus = new Aeris.Engine.EventBus();
        for (int h = 0; h < 5; h++)
            bus.Subscribe<TestPayloadEvent>(e => { _ = e.Value; });

        for (int i = 0; i < EventCount; i++)
            bus.Emit(new TestPayloadEvent { Value = i });

        bus.AdvanceTick();
        bus.Flush();
    }

    [Benchmark]
    public void MixedEventTypes()
    {
        for (int i = 0; i < EventCount; i++)
        {
            _bus.Emit(new TestPayloadEvent { Value = i });
            _bus.Emit(new BulkEvent { Data = i * 2 });
        }

        _bus.AdvanceTick();
        _bus.Flush();
    }
}

[Config(typeof(AerisBenchmarkConfig))]
[MemoryDiagnoser]
public class SchedulerBenchmarks
{
    private Aeris.Engine.SchedulerResource _scheduler = null!;
    private Aeris.Engine.World _world = null!;

    [Params(100, 1_000, 10_000)]
    public int CallbackCount;

    [GlobalSetup]
    public void Setup()
    {
        _world = new Aeris.Engine.World();
        _world.AddResource(Aeris.Engine.TimeResource.Create());
        _world.AddResource(new Aeris.Engine.EngineStats());
        _scheduler = new Aeris.Engine.SchedulerResource();

        for (int i = 0; i < CallbackCount; i++)
        {
            _scheduler.Schedule(i * 1.0, _ => { }, $"event-{i}");
        }

        _world.AddResource(_scheduler);
    }

    [Benchmark]
    public void ProcessAllAtOnce()
    {
        _scheduler.Process(_world, CallbackCount * 1.0);
    }

    [Benchmark]
    public void ProcessIncrementally()
    {
        for (int i = 0; i < CallbackCount; i++)
        {
            _scheduler.Process(_world, i * 1.0);
        }
    }
}

[Config(typeof(AerisBenchmarkConfig))]
[MemoryDiagnoser]
public class PersistenceBenchmarks
{
    private string _tempDir = null!;
    private Aeris.Engine.JsonPersistence _persistence = null!;
    private Aeris.Engine.World _world = null!;

    [Params(100, 1_000, 10_000)]
    public int EntityCount;

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"aeris-bench-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _persistence = new Aeris.Engine.JsonPersistence(_tempDir);

        _world = new Aeris.Engine.World();
        _world.AddResource(Aeris.Engine.TimeResource.Create());
        _world.AddResource(new Aeris.Engine.EngineStats());

        for (int i = 0; i < EntityCount; i++)
        {
            _world.CreateEntity()
                .With(new PositionComponent { X = i * 0.1f, Y = i * 0.2f })
                .With(new VelocityComponent { Dx = 1f, Dy = 0.5f })
                .Build();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Benchmark]
    public void SaveWorld()
    {
        _persistence.SaveWorld(_world, "bench.json");
    }

    [Benchmark]
    public void LoadWorld()
    {
        _persistence.SaveWorld(_world, "bench.json");

        var loadWorld = new Aeris.Engine.World();
        loadWorld.AddResource(Aeris.Engine.TimeResource.Create());
        loadWorld.AddResource(new Aeris.Engine.EngineStats());

        _persistence.LoadWorld(loadWorld, "bench.json");
    }

    [Benchmark]
    public void SaveAndLoadRoundTrip()
    {
        _persistence.SaveWorld(_world, "bench.json");

        var loadWorld = new Aeris.Engine.World();
        loadWorld.AddResource(Aeris.Engine.TimeResource.Create());
        loadWorld.AddResource(new Aeris.Engine.EngineStats());

        _persistence.LoadWorld(loadWorld, "bench.json");
    }
}

[Config(typeof(AerisBenchmarkConfig))]
[MemoryDiagnoser]
public class FullPipelineBenchmarks
{
    private Aeris.Engine.World _world = null!;
    private EngineClass _engine = null!;
    private Aeris.Engine.JsonPersistence _persistence = null!;
    private string _tempDir = null!;

    [Params(100, 1_000, 10_000)]
    public int EntityCount;

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"aeris-pipe-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _world = new Aeris.Engine.World();
        _world.AddResource(Aeris.Engine.TimeResource.Create());
        _world.AddResource(new Aeris.Engine.EngineStats());

        _engine = new EngineClass(_world);
        _engine.RegisterSystem(new BenchmarkMovementSystem());
        _engine.RegisterSystem(new BenchmarkHealthRegenSystem());
        _engine.Initialize();

        _persistence = new Aeris.Engine.JsonPersistence(_tempDir);

        var scheduler = new Aeris.Engine.SchedulerResource();
        scheduler.Schedule(50.0, _ => { }, "scheduled-event");
        _world.AddResource(scheduler);

        for (int i = 0; i < EntityCount; i++)
        {
            _world.CreateEntity()
                .With(new PositionComponent { X = i * 0.1f, Y = i * 0.2f })
                .With(new VelocityComponent { Dx = 1f, Dy = 0.5f })
                .With(new BenchHealthComponent { Current = 50, Max = 100 })
                .Build();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Benchmark]
    public void FullTickWithSystems()
    {
        _engine.RunOneTick(0.016f);
    }

    [Benchmark]
    public void FullTickWithPersistence()
    {
        _persistence.SaveWorld(_world, "bench.json");
        _engine.RunOneTick(0.016f);
    }

    [Benchmark]
    public void TenTickSimulation()
    {
        for (int i = 0; i < 10; i++)
            _engine.RunOneTick(0.016f);
    }
}

public struct PositionComponent
{
    public float X;
    public float Y;
}

public struct VelocityComponent
{
    public float Dx;
    public float Dy;
}

public struct BenchHealthComponent
{
    public int Current;
    public int Max;
}

public struct TestPayloadEvent
{
    public int Value;
}

public struct BulkEvent
{
    public int Data;
}

public sealed class BenchmarkMovementSystem : Aeris.Engine.ISystem
{
    public string Name => "BenchMovement";
    public Aeris.Engine.SystemPhase Phase => Aeris.Engine.SystemPhase.Action;
    public int Priority => 0;

    public void Execute(Aeris.Engine.World world, float deltaTime)
    {
        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<PositionComponent>() && kvp.Value.HasComponent<VelocityComponent>())
            {
                var pos = kvp.Value.GetComponent<PositionComponent>();
                var vel = kvp.Value.GetComponent<VelocityComponent>();

                pos.X += vel.Dx * deltaTime;
                pos.Y += vel.Dy * deltaTime;

                kvp.Value.RemoveComponent<PositionComponent>();
                kvp.Value.AddComponent(pos);
            }
        }
    }
}

public sealed class BenchmarkHealthRegenSystem : Aeris.Engine.ISystem
{
    public string Name => "BenchHealthRegen";
    public Aeris.Engine.SystemPhase Phase => Aeris.Engine.SystemPhase.Consequences;
    public int Priority => 0;

    public void Execute(Aeris.Engine.World world, float deltaTime)
    {
        foreach (var kvp in world.Entities)
        {
            if (kvp.Value.HasComponent<BenchHealthComponent>())
            {
                var health = kvp.Value.GetComponent<BenchHealthComponent>();
                if (health.Current < health.Max)
                {
                    health.Current++;
                    kvp.Value.RemoveComponent<BenchHealthComponent>();
                    kvp.Value.AddComponent(health);
                }
            }
        }
    }
}

public class AerisBenchmarkConfig : ManualConfig
{
    public AerisBenchmarkConfig()
    {
        AddJob(Job.ShortRun.WithWarmupCount(3).WithIterationCount(5));
        AddColumn(StatisticColumn.Median);
        AddColumn(StatisticColumn.StdDev);
    }
}
