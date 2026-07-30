using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class CognitiveInfrastructureTests : IDisposable
{
    private readonly string _testDir;

    public CognitiveInfrastructureTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"aeris-cog-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void FullPipeline_PerceptionToTrace()
    {
        var (world, engine) = CreateEngine();

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .With(new PositionComponent { X = 0f, Y = 0f })
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 1, Size = 2f })
            .With(new PositionComponent { X = 3f, Y = 4f })
            .Build();

        for (int tick = 0; tick < 10; tick++)
            engine.RunOneTick(1f);

        world.HasResource<PerceptBatch>().Should().BeTrue();
        var percepts = world.GetResource<PerceptBatch>();
        percepts.Percepts.Should().Contain(p => p.LabelId == 1);

        world.HasResource<AttendedPercepts>().Should().BeTrue();
        var attended = world.GetResource<AttendedPercepts>();
        attended.Percepts.Should().Contain(p => p.LabelId == 1);

        world.HasResource<WorkingMemoryStore>().Should().BeTrue();
        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Should().NotBeEmpty();

        world.HasResource<CognitiveTraceLog>().Should().BeTrue();
        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "PerceptionSystem");
        trace.Entries.Should().Contain(e => e.System == "AttentionSystem");
        trace.Entries.Should().Contain(e => e.System == "WorkingMemorySystem");

        world.HasResource<AffectState>().Should().BeTrue();
        var affect = world.GetResource<AffectState>();
        affect.Curiosity.Should().BeGreaterThan(0f);

        world.HasResource<WorldModelState>().Should().BeTrue();
        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds.Should().HaveCount(1);
    }

    [Fact]
    public void Determinism_SameSeedSameResults()
    {
        var (world1, engine1) = CreateEngine();
        SetupEntities(world1);
        for (int i = 0; i < 5; i++)
            engine1.RunOneTick(1f);

        var (world2, engine2) = CreateEngine();
        SetupEntities(world2);
        for (int i = 0; i < 5; i++)
            engine2.RunOneTick(1f);

        var wm1 = world1.GetResource<WorkingMemoryStore>();
        var wm2 = world2.GetResource<WorkingMemoryStore>();

        wm1.Chunks.Count.Should().Be(wm2.Chunks.Count);
        for (int i = 0; i < wm1.Chunks.Count; i++)
        {
            wm1.Chunks[i].Id.Should().Be(wm2.Chunks[i].Id);
            wm1.Chunks[i].Salience.Should().BeApproximately(wm2.Chunks[i].Salience, 0.001f);
        }

        var affect1 = world1.GetResource<AffectState>();
        var affect2 = world2.GetResource<AffectState>();
        affect1.Curiosity.Should().BeApproximately(affect2.Curiosity, 0.001f);
        affect1.Stress.Should().BeApproximately(affect2.Stress, 0.001f);
        affect1.Confidence.Should().BeApproximately(affect2.Confidence, 0.001f);
    }

    [Fact]
    public void AllTaggedEntities_ArePerceived()
    {
        var (world, engine) = CreateEngine();

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 1, Size = 1f })
            .Build();

        world.CreateEntity()
            .With(new AuraTag { LabelId = 2, Signature = 0.5f })
            .Build();

        for (int tick = 0; tick < 3; tick++)
            engine.RunOneTick(1f);

        var percepts = world.GetResource<PerceptBatch>();
        percepts.Percepts.Should().Contain(p => p.LabelId == 1);
        percepts.Percepts.Should().Contain(p => p.LabelId == 2);

        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Should().HaveCount(2);
    }

    [Fact]
    public void WorkingMemory_DecayAndCapacityEviction()
    {
        var (world, engine) = CreateEngine();

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .Build();

        for (int tick = 0; tick < 20; tick++)
            engine.RunOneTick(1f);

        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Should().BeEmpty();
    }

    [Fact]
    public void WorldModel_TracksKnownEntities()
    {
        var (world, engine) = CreateEngine();

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .With(new PositionComponent { X = 0f, Y = 0f })
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 3, Size = 1f })
            .With(new PositionComponent { X = 2f, Y = 3f })
            .Build();

        for (int tick = 0; tick < 3; tick++)
            engine.RunOneTick(1f);

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds.Should().HaveCount(1);
        model.LastUpdateTick.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AffectState_IsContinuousVectorNoDiscreteEmotions()
    {
        var affect = AffectState.Default;
        var fields = affect.GetType().GetFields();
        fields.Should().OnlyContain(f => f.FieldType == typeof(float));
        fields.Should().HaveCount(9);
    }

    [Fact]
    public void LongTermMemorySystem_RunsWithoutError()
    {
        var (world, engine) = CreateEngine();

        var store = world.GetResource<MemoryStore>();
        store.AddMemory(1, new MemoryData
        {
            Id = store.AllocateId(),
            Importance = 0.8f,
            Timestamp = 0f,
            Type = MemoryType.Observed
        });

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .With(new MemoryMarker { Count = 1, LastConsolidationTime = 0f })
            .Build();

        for (int tick = 0; tick < 5; tick++)
            engine.RunOneTick(1f);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "LongTermMemorySystem");
    }

    [Fact]
    public void GoalSystem_DefaultExplorationGoalCreated()
    {
        var (world, engine) = CreateEngine();

        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .With(new GoalMarker { ActiveCount = 0, HighestPriority = GoalPriority.Trivial })
            .Build();

        for (int tick = 0; tick < 3; tick++)
            engine.RunOneTick(1f);

        var goals = world.GetResource<GoalStore>();
        var entityId = world.Entities.Values.First(e => e.HasComponent<CognitiveAgentMarker>());
        uint id = entityId.Id.Value;
        var entityGoals = goals.GetGoals(id);
        entityGoals.Should().Contain(g => g.Type == GoalType.Exploration && g.Status == GoalStatus.Active);
    }

    private static (World, Engine) CreateEngine()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new LongTermMemorySystem());
        engine.RegisterSystem(new AffectSystem());
        engine.RegisterSystem(new GoalSystem());
        engine.RegisterSystem(new WorldModelSystem());
        engine.Initialize();

        return (world, engine);
    }

    private static void SetupEntities(World world)
    {
        world.CreateEntity()
            .With(new CognitiveAgentMarker())
            .With(new PositionComponent { X = 0f, Y = 0f })
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 10, Size = 2f })
            .With(new PositionComponent { X = 5f, Y = 5f })
            .Build();

        world.CreateEntity()
            .With(new AuraTag { LabelId = 11, Signature = 0.6f })
            .With(new PositionComponent { X = 8f, Y = 2f })
            .Build();
    }
}
