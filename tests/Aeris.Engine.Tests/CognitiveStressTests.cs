using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class CognitiveStressTests
{
    [Fact]
    public void S_ST_001_EmptyWorld_PipelineDoesNotCrash()
    {
        var (world, engine) = CreateFullPipeline();

        var act = () => engine.RunOneTick();

        act.Should().NotThrow("the pipeline must handle empty worlds gracefully");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().NotBeEmpty("causal trace should record activity even in empty world");
    }

    [Fact]
    public void S_ST_002_ThousandTicks_NoStateGrowth()
    {
        var (world, engine) = CreateFullPipeline();
        SetUpMinimalState(world);

        for (int i = 0; i < 1000; i++)
            engine.RunOneTick();

        engine.Tick.Should().Be(1000);

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Count.Should().BeLessThanOrEqualTo(5,
            "plan store should not accumulate across ticks (cleared each tick)");

        var actionStore = world.GetResource<ActionStore>();
        actionStore.LastResult.Should().NotBeNull();
        actionStore.LastResult.Status.Should().BeOneOf(DecisionStatus.Selected, DecisionStatus.NoViablePlan);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Count.Should().BeLessThanOrEqualTo(1000 * 10,
            "trace entries should be bounded (at most ~10 per tick)");

        var stats = world.GetResource<EngineStats>();
        stats.Tick.Should().Be(1000);
    }

    [Fact]
    public void S_ST_003_EmptyResources_ProducesNoViablePlan()
    {
        var (world, engine) = CreateFullPipeline();

        world.AddResource(new GoalStore());
        world.AddResource(new WorldModelState());
        world.SetResource(AffectState.Default);

        engine.RunOneTick();

        var actionStore = world.GetResource<ActionStore>();
        actionStore.LastResult.Status.Should().Be(DecisionStatus.NoViablePlan,
            "with no goals and no state, Decision should emit NoViablePlan, not crash");

        world.HasResource<PlanStore>().Should().BeTrue();
        world.HasResource<InferenceStore>().Should().BeTrue();
        world.HasResource<AuditStore>().Should().BeTrue();
        world.HasResource<EnforcementStore>().Should().BeTrue();
    }

    [Fact]
    public void S_ST_004_OneHundredThousandMemories_RetrievalCompletes()
    {
        var (world, engine) = CreateFullPipeline();
        SetUpMinimalState(world);

        var agent = world.CreateEntity()
            .With(new CognitiveAgentMarker { AgentId = 1 })
            .Build();

        var memories = world.GetResource<MemoryStore>();
        for (int i = 0; i < 100_000; i++)
        {
            memories.AddMemory(agent.Id.Value, new MemoryData
            {
                Id = memories.AllocateId(),
                Type = MemoryType.Observed,
                Category = MemoryCategory.Environmental,
                Importance = (float)(i % 1000) / 1000f,
                Certainty = 0.9f,
                Timestamp = i,
                InvolvedEntityId = (uint)(i % 100 + 2)
            });
        }

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("100K memories should not crash the retrieval pipeline");
    }

    [Fact]
    public void S_ST_005_FiveHundredGoals_PlanningAndDecisionScale()
    {
        var (world, engine) = CreateFullPipeline();
        SetUpMinimalState(world);

        var agent = world.CreateEntity()
            .With(new CognitiveAgentMarker { AgentId = 1 })
            .Build();

        var goals = world.GetResource<GoalStore>();
        for (int i = 0; i < 500; i++)
        {
            goals.AddGoal(agent.Id.Value, new GoalData
            {
                Id = (uint)(i + 1),
                Type = (GoalType)(i % 8),
                Status = GoalStatus.Active,
                Priority = (GoalPriority)(i % 5 + 1),
                Urgency = (i % 10) / 10f
            });
        }

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("500 goals should not crash planning and decision");

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().NotBeEmpty("planning should produce plans for active goals");
    }

    [Fact]
    public void S_ST_006_DeterminismAcrossOneHundredTicks()
    {
        var snap1 = RunDeterministicStressSession();
        var snap2 = RunDeterministicStressSession();

        snap1.TickCount.Should().Be(snap2.TickCount);

        snap1.FinalActionStatus.Should().Be(snap2.FinalActionStatus);
        snap1.FinalActionName.Should().Be(snap2.FinalActionName);

        snap1.TraceSystemList.Should().Equal(snap2.TraceSystemList,
            "deterministic replay must produce same trace systems across 100 ticks");

        snap1.CausalChain.Should().Equal(snap2.CausalChain,
            "deterministic replay must produce same causal chain across 100 ticks");
    }

    [Fact]
    public void S_ST_006A_DeterminismSingleTickReplayable()
    {
        var state1 = CaptureTickState();
        var state2 = CaptureTickState();

        state1.Should().Be(state2,
            "identical world setup must produce identical tick output");
    }

    private static string CaptureTickState()
    {
        var (world, engine) = CreateFullPipeline();
        SetUpMinimalState(world);
        engine.RunOneTick();

        var actionStore = world.GetResource<ActionStore>();
        var trace = world.GetResource<CognitiveTraceLog>();
        var planStore = world.GetResource<PlanStore>();

        var parts = new List<string>
        {
            $"Action={actionStore.LastResult.Status}:{actionStore.LastResult.Action.Action}:{actionStore.LastResult.SelectedPlanId}"
        };

        foreach (var e in trace.Entries)
            parts.Add($"{e.TraceId}:{e.ParentTraceId}:{e.System}:{e.OutputSummary}");

        foreach (var p in planStore.Plans.OrderBy(p => p.Id))
            parts.Add($"Plan={p.Id}:F={p.Feasibility:F4}:P={p.Preference:F4}");

        return string.Join("|", parts);
    }

    private sealed class StressSnapshot
    {
        public long TickCount { get; init; }
        public DecisionStatus FinalActionStatus { get; init; }
        public string FinalActionName { get; init; } = "";
        public List<string> TraceSystemList { get; init; } = new();
        public List<string> CausalChain { get; init; } = new();
    }

    private static StressSnapshot RunDeterministicStressSession()
    {
        var (world, engine) = CreateFullPipeline();
        SetUpMinimalState(world);

        for (int tick = 0; tick < 100; tick++)
            engine.RunOneTick();

        var actionStore = world.GetResource<ActionStore>();
        var trace = world.GetResource<CognitiveTraceLog>();

        return new StressSnapshot
        {
            TickCount = engine.Tick,
            FinalActionStatus = actionStore.LastResult.Status,
            FinalActionName = actionStore.LastResult.Action.Action,
            TraceSystemList = trace.Entries
                .Where(e => e.Tick == 100)
                .Select(e => e.System).ToList(),
            CausalChain = trace.Entries
                .Where(e => e.Tick == 100)
                .Select(e => $"{e.TraceId}:{e.ParentTraceId?.ToString() ?? "null"}")
                .ToList()
        };
    }

    private static (World world, Engine engine) CreateFullPipeline()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new MemoryRetrievalSystem());
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem());
        engine.RegisterSystem(new PlanningSystem());
        engine.RegisterSystem(new DecisionSystem());
        engine.RegisterSystem(new AuditSystem());
        engine.RegisterSystem(new EnforcementSystem());

        engine.Initialize();
        return (world, engine);
    }

    private static void SetUpMinimalState(World world)
    {
        var agent = world.CreateEntity()
            .With(new CognitiveAgentMarker { AgentId = 1 })
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 42, Size = 1f })
            .Build();

        var memories = world.GetResource<MemoryStore>();
        var memId = memories.AllocateId();
        memories.AddMemory(agent.Id.Value, new MemoryData
        {
            Id = memId,
            Type = MemoryType.Observed,
            Category = MemoryCategory.Environmental,
            Importance = 0.8f,
            Certainty = 0.9f,
            Timestamp = 0f,
            InvolvedEntityId = 2
        });

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(agent.Id.Value, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 2 };

        world.SetResource(AffectState.Default);
    }
}
