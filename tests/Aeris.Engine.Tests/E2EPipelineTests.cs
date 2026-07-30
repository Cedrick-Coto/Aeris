using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class E2EPipelineTests
{
    [Fact]
    public void E2E_001_PerceptToDecision_HappyPath()
    {
        var (world, engine) = CreatePipeline();
        SetUpHappyPathState(world);

        engine.RunOneTick();

        var store = world.GetResource<ActionStore>();
        store.LastResult.Status.Should().Be(DecisionStatus.Selected);
        store.LastResult.Action.Action.Should().NotBeNullOrEmpty();

        var trace = world.GetResource<CognitiveTraceLog>();
        var expectedSystems = new[]
        {
            "PerceptionSystem",
            "AttentionSystem",
            "MemoryRetrievalSystem",
            "WorkingMemorySystem",
            "ReasoningSystem",
            "PlanningSystem",
            "DecisionSystem"
        };

        var recordedSystems = trace.Entries
            .Where(e => e.Tick == 1)
            .Select(e => e.System)
            .ToList();

        foreach (var sys in expectedSystems)
            recordedSystems.Should().Contain(sys, $"CausalTrace should contain {sys}");

        VerifyTraceChain(trace);
    }

    [Fact]
    public void E2E_002_TwoViablePlans_SelectsByPreference()
    {
        var (world, engine) = CreatePipeline();
        SetUpTwoGoalState(world);

        engine.RunOneTick();

        var store = world.GetResource<ActionStore>();
        store.LastResult.Status.Should().Be(DecisionStatus.Selected);

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().HaveCountGreaterThanOrEqualTo(2);

        var feasiblePlans = planStore.Plans
            .Where(p => p.Feasibility >= 0.5f)
            .OrderByDescending(p => p.Preference)
            .ToList();

        feasiblePlans.Should().NotBeEmpty("at least one plan should be viable");

        var topPlan = feasiblePlans[0];
        store.LastResult.SelectedPlanId.Should().Be(topPlan.Id,
            "the plan with highest preference among viable should be selected");

        foreach (var plan in planStore.Plans)
        {
            plan.Feasibility.Should().BeInRange(0f, 1f,
                $"plan {plan.Id} feasibility must stay in valid range (D-003)");
        }
    }

    [Fact]
    public void E2E_003_NoViablePlan_ProducesStableDefer()
    {
        var (world, engine) = CreatePipeline();
        SetUpNoViablePlanState(world);

        engine.RunOneTick();

        var store = world.GetResource<ActionStore>();
        store.LastResult.Status.Should().Be(DecisionStatus.NoViablePlan);
        store.LastResult.Action.Action.Should().Be("Defer",
            "when no plan is viable, Decision must emit Defer, not invent a new plan (D-002)");

        store.LastResult.SelectedPlanId.Should().BeNull(
            "no plan should be selected when none is viable");

        var trace = world.GetResource<CognitiveTraceLog>();
        var decisionEntry = trace.Entries.FirstOrDefault(e => e.System == "DecisionSystem");
        decisionEntry.OutputSummary.Should().Contain("NoViablePlan",
            "CausalTrace must explicitly record the deferral");

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().NotBeEmpty("planning should still generate candidates");
    }

    [Fact]
    public void E2E_004_DeterministicReplay_FullStateMatch()
    {
        var state1 = RunDeterministicSession();
        var state2 = RunDeterministicSession();

        state1.ActionStatus.Should().Be(state2.ActionStatus);
        state1.ActionName.Should().Be(state2.ActionName);
        state1.SelectedPlanId.Should().Be(state2.SelectedPlanId);
        state1.TraceSystems.Should().Equal(state2.TraceSystems);
        state1.TraceChain.Should().Equal(state2.TraceChain);
        state1.PlanFeasibility.Should().Equal(state2.PlanFeasibility,
            "deterministic replay must produce identical plan feasibility");
        state1.PlanPreference.Should().Equal(state2.PlanPreference,
            "deterministic replay must produce identical plan preference");
    }

    private static PipelineSnapshot RunDeterministicSession()
    {
        var (world, engine) = CreatePipeline();
        SetUpHappyPathState(world);

        engine.RunOneTick();

        var actionStore = world.GetResource<ActionStore>();
        var trace = world.GetResource<CognitiveTraceLog>();
        var planStore = world.GetResource<PlanStore>();

        return new PipelineSnapshot
        {
            ActionStatus = actionStore.LastResult.Status,
            ActionName = actionStore.LastResult.Action.Action,
            SelectedPlanId = actionStore.LastResult.SelectedPlanId,
            TraceSystems = trace.Entries.Where(e => e.Tick == 1).Select(e => e.System).ToList(),
            TraceChain = trace.Entries.Where(e => e.Tick == 1)
                .Select(e => $"{e.TraceId}:{e.ParentTraceId?.ToString() ?? "null"}")
                .ToList(),
            PlanFeasibility = planStore.Plans.OrderBy(p => p.Id).Select(p => p.Feasibility).ToList(),
            PlanPreference = planStore.Plans.OrderBy(p => p.Id).Select(p => p.Preference).ToList()
        };
    }

    private sealed class PipelineSnapshot
    {
        public DecisionStatus ActionStatus { get; init; }
        public string ActionName { get; init; } = "";
        public uint? SelectedPlanId { get; init; }
        public List<string> TraceSystems { get; init; } = new();
        public List<string> TraceChain { get; init; } = new();
        public List<float> PlanFeasibility { get; init; } = new();
        public List<float> PlanPreference { get; init; } = new();
    }

    private static (World world, Engine engine) CreatePipeline()
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

        engine.Initialize();
        return (world, engine);
    }

    private static void SetUpHappyPathState(World world)
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

        world.SetResource(AffectState.Default with
        {
            Curiosity = 0.5f,
            Stress = 0.2f,
            Confidence = 0.6f,
            Novelty = 0.3f,
            Threat = 0.1f
        });
    }

    private static void SetUpTwoGoalState(World world)
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
            Urgency = 0.8f
        });
        goals.AddGoal(agent.Id.Value, new GoalData
        {
            Id = 2,
            Type = GoalType.Survival,
            Status = GoalStatus.Active,
            Priority = GoalPriority.Medium,
            Urgency = 0.3f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 2 };

        world.SetResource(AffectState.Default with
        {
            Curiosity = 0.5f,
            Stress = 0.2f,
            Confidence = 0.6f,
            Novelty = 0.3f,
            Threat = 0.1f
        });
    }

    private static void SetUpNoViablePlanState(World world)
    {
        var agent = world.CreateEntity()
            .With(new CognitiveAgentMarker { AgentId = 1 })
            .Build();

        world.CreateEntity()
            .With(new VisualTag { LabelId = 42, Size = 1f })
            .Build();

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(agent.Id.Value, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.Medium,
            Urgency = 0.5f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint>();

        world.SetResource(AffectState.Default with
        {
            Curiosity = 0.3f,
            Stress = 0.6f,
            Confidence = 0.4f,
            Novelty = 0.2f,
            Threat = 0.5f
        });
    }

    private static void VerifyTraceChain(CognitiveTraceLog trace)
    {
        var tickEntries = trace.Entries.Where(e => e.Tick == 1).ToList();
        tickEntries.Should().NotBeEmpty();

        long? prevId = null;
        foreach (var entry in tickEntries)
        {
            if (prevId == null)
            {
                entry.ParentTraceId.Should().BeNull("first entry has no parent");
            }
            else
            {
                entry.ParentTraceId.Should().Be(prevId,
                    $"entry {entry.System} should link to previous {prevId}");
            }
            prevId = entry.TraceId;
        }
    }
}
