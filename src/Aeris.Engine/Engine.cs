using System.Diagnostics;

namespace Aeris.Engine;

public sealed class Engine
{
    private readonly World _world;
    private readonly SystemManager _systemManager;
    private readonly EventBus _eventBus;
    private JsonPersistence? _persistence;
    private bool _initialized;

    public Engine(World world)
    {
        _world = world;
        _systemManager = new SystemManager();

        if (_world.HasResource<EventBus>())
            _eventBus = _world.GetResource<EventBus>();
        else
        {
            _eventBus = new EventBus();
            _world.AddResource(_eventBus);
        }

        if (!_world.HasResource<MemoryStore>())
            _world.AddResource(new MemoryStore());
        if (!_world.HasResource<BeliefStore>())
            _world.AddResource(new BeliefStore());
        if (!_world.HasResource<KnowledgeStore>())
            _world.AddResource(new KnowledgeStore());
        if (!_world.HasResource<EmotionStore>())
            _world.AddResource(new EmotionStore());
        if (!_world.HasResource<GoalStore>())
            _world.AddResource(new GoalStore());
        if (!_world.HasResource<RelationshipStore>())
            _world.AddResource(new RelationshipStore());
        if (!_world.HasResource<AttentionStore>())
            _world.AddResource(new AttentionStore());

        if (!_world.HasResource<CognitiveTraceLog>())
            _world.AddResource(new CognitiveTraceLog { Tick = 0 });
        if (!_world.HasResource<WorkingMemoryStore>())
            _world.AddResource(new WorkingMemoryStore());
        if (!_world.HasResource<WorldModelState>())
            _world.AddResource(new WorldModelState());
    }

    public void SetPersistence(JsonPersistence persistence)
    {
        _persistence = persistence;
    }

    public void RegisterSystem(ISystem system)
    {
        Debug.Assert(!_initialized, "Cannot register systems after initialization");
        _systemManager.Register(system);
    }

    public void Initialize()
    {
        Debug.Assert(!_initialized, "Engine already initialized");

        _systemManager.Freeze();
        _initialized = true;
    }

    public void RunOneTick(float deltaTime = 1f)
    {
        Debug.Assert(_initialized, "Engine must be initialized before running");

        var time = _world.GetResource<TimeResource>();
        var stopwatch = Stopwatch.StartNew();

        _eventBus.AdvanceTick();

        time.Advance(deltaTime);
        _world.SetResource(time);

        if (_world.HasResource<CognitiveTraceLog>())
        {
            var trace = _world.GetResource<CognitiveTraceLog>();
            trace.Tick = time.Tick;
            trace.ResetTick();
        }

        if (_world.HasResource<SchedulerResource>())
        {
            var scheduler = _world.GetResource<SchedulerResource>();
            scheduler.Process(_world, time.SimulationTime);
            _world.SetResource(scheduler);
        }

        var stats = _world.GetResource<EngineStats>();
        stats.Tick = time.Tick;
        stats.TickDuration = 0;
        stats.SystemsExecuted = 0;
        _world.SetResource(stats);

        _systemManager.ExecuteAll(_world, deltaTime);

        _eventBus.Flush();

        if (_persistence != null && _persistence.ShouldCheckpoint(time.Tick))
        {
            _persistence.SaveWorld(_world, _persistence.GetCheckpointPath());
            _persistence.RecordCheckpoint(time.Tick);
        }

        stopwatch.Stop();

        stats = _world.GetResource<EngineStats>();
        stats.TickDuration = stopwatch.Elapsed.TotalMilliseconds;
        _world.SetResource(stats);
    }

    public long Tick => _world.GetResource<TimeResource>().Tick;
    public EventBus EventBus => _eventBus;
}
