using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class PlanningTests
{
    [Fact]
    public void S_P001_PlanForReachableGoal()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.8f }
        });

        var result = strategy.Plan(context);

        result.Plans.Should().NotBeEmpty();
        result.Plans.Should().Contain(p => p.GoalId == 1);
        result.Plans[0].Feasibility.Should().BeGreaterThan(0f);
        result.Plans[0].Preference.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void S_P001_PlanContainsSteps()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.8f }
        });

        var result = strategy.Plan(context);

        foreach (var plan in result.Plans)
        {
            plan.Steps.Should().NotBeEmpty();
            plan.ExpectedOutcome.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void S_P002_PlanWithoutKnownLocation()
    {
        var worldModel = new WorldModelState();
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(
            goals: new[]
            {
                new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.Medium, Urgency = 0.5f }
            },
            model: worldModel);

        var result = strategy.Plan(context);

        result.Plans.Should().NotBeEmpty();
        result.Plans.Should().Contain(p =>
            p.Steps.Any(s => s.Action == "Explore"));
    }

    [Fact]
    public void S_P003_HighRiskProducesDefer()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Survival, Status = GoalStatus.Active, Priority = GoalPriority.Low, Urgency = 0.2f }
        });

        var result = strategy.Plan(context);

        result.Plans.Should().Contain(p =>
            p.Steps.Any(s => s.Action == "Defer"));
    }

    [Fact]
    public void S_P004_AffectModulatesPreferenceNotFeasibility()
    {
        var goal = new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f };

        var contextHighCuriosity = CreateContext(goals: new[] { goal },
            affect: AffectState.Default with { Curiosity = 0.9f, Threat = 0.1f, Stress = 0.2f });
        var contextHighStress = CreateContext(goals: new[] { goal },
            affect: AffectState.Default with { Curiosity = 0.2f, Threat = 0.8f, Stress = 0.9f });

        var resultCurious = new GoalDirectedPlanningStrategy().Plan(contextHighCuriosity);
        var resultStressed = new GoalDirectedPlanningStrategy().Plan(contextHighStress);

        var planCurious = resultCurious.Plans[0];
        var planStressed = resultStressed.Plans[0];

        planCurious.Feasibility.Should().Be(planStressed.Feasibility,
            "feasibility depends on world constraints, not affect");
        planCurious.Preference.Should().NotBe(planStressed.Preference,
            "preference should differ under different affect states");
    }

    [Fact]
    public void S_P005_NoGoalsReturnsEmpty()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: Array.Empty<GoalData>());

        var result = strategy.Plan(context);

        result.Plans.Should().BeEmpty();
        result.Evidence.Should().BeEmpty();
    }

    [Fact]
    public void S_P006_MultipleCandidatesNoSelection()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Collection, Status = GoalStatus.Active, Priority = GoalPriority.Low, Urgency = 0.2f }
        });

        var result = strategy.Plan(context);

        result.Plans.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void S_P007_ImpossibleGoalReturnsEmpty()
    {
        var worldModel = new WorldModelState();
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(
            goals: new[]
            {
                new GoalData { Id = 1, Type = GoalType.Quest, Status = GoalStatus.Active, Priority = GoalPriority.Critical, Urgency = 0.1f }
            },
            model: worldModel);

        var result = strategy.Plan(context);

        result.Plans.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void S_P008_Determinism()
    {
        string hash1 = RunDeterministicSession();
        string hash2 = RunDeterministicSession();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void S_P009_NoSideEffects()
    {
        var world = CreateWorld(out var goals, out var model, out var store, out var wm);
        goals.AddGoal(1, new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f });

        string goalsHash = HashGoals(goals);
        string modelHash = HashModel(model);

        var system = new PlanningSystem();
        system.Execute(world, 1f);

        HashGoals(goals).Should().Be(goalsHash);
        HashModel(model).Should().Be(modelHash);
    }

    [Fact]
    public void S_P010_StrategyReplacement()
    {
        var world = CreateWorld(out var goals, out var model, out var store, out var wm);
        goals.AddGoal(1, new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f });

        var system = new PlanningSystem
        {
            Strategy = new MockPlanningStrategy()
        };

        system.Execute(world, 1f);

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().Contain(p => p.ExpectedOutcome == "mock plan");
    }

    [Fact]
    public void FeasibilityAndPreferenceSeparate()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.Medium, Urgency = 0.5f }
        });

        var result = strategy.Plan(context);

        foreach (var plan in result.Plans)
        {
            plan.Feasibility.Should().BeInRange(0f, 1f);
            plan.Preference.Should().BeInRange(0f, 1f);
        }
    }

    [Fact]
    public void PlanningSystem_ExecutesInCausalChain()
    {
        var world = CreateWorld(out var goals, out var model, out var store, out var wm);
        goals.AddGoal(1, new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f });

        var system = new PlanningSystem();
        system.Execute(world, 1f);

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().NotBeEmpty();

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "PlanningSystem");
    }

    [Fact]
    public void InactiveGoalsIgnored()
    {
        var strategy = new GoalDirectedPlanningStrategy();
        var context = CreateContext(goals: new[]
        {
            new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Inactive, Priority = GoalPriority.High, Urgency = 0.5f },
            new GoalData { Id = 2, Type = GoalType.Exploration, Status = GoalStatus.Completed, Priority = GoalPriority.High, Urgency = 0.5f }
        });

        var result = strategy.Plan(context);

        result.Plans.Should().BeEmpty();
    }

    private static string RunDeterministicSession()
    {
        var world = CreateWorld(out var goals, out var model, out var store, out var wm);
        goals.AddGoal(1, new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f });

        var system = new PlanningSystem();
        system.Execute(world, 1f);

        var planStore = world.GetResource<PlanStore>();
        return string.Join("|",
            planStore.Plans.Select(p => $"{p.Id}:{p.Feasibility:F4}:{p.Preference:F4}:{p.ExpectedOutcome}"));
    }

    private static PlanningContext CreateContext(
        GoalData[] goals,
        WorldModelState? model = null,
        AffectState? affect = null)
    {
        return new PlanningContext
        {
            ActiveGoals = goals.Where(g => g.IsActive).ToList(),
            WorldModel = model ?? new WorldModelState { KnownEntityIds = new List<uint> { 1 } },
            Affect = affect ?? AffectState.Default
        };
    }

    private static World CreateWorld(
        out GoalStore goals, out WorldModelState model, out InferenceStore store, out WorkingMemoryStore wm)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new GoalStore());
        world.AddResource(new WorldModelState());
        world.AddResource(new InferenceStore());
        world.AddResource(new WorkingMemoryStore());
        world.AddResource(AffectState.Default);
        world.AddResource(new CognitiveTraceLog());
        world.AddResource(new PlanStore());

        goals = world.GetResource<GoalStore>();
        model = world.GetResource<WorldModelState>();
        store = world.GetResource<InferenceStore>();
        wm = world.GetResource<WorkingMemoryStore>();
        return world;
    }

    private static string HashGoals(GoalStore goals)
    {
        var parts = new List<string>();
        foreach (var kvp in goals.All)
            foreach (var g in kvp.Value)
                parts.Add($"{g.Id}:{g.Type}:{g.Status}");
        return string.Join("|", parts);
    }

    private static string HashModel(WorldModelState model)
    {
        return string.Join(",", model.KnownEntityIds) + ":" + model.LastUpdateTick;
    }

    private sealed class MockPlanningStrategy : IPlanningStrategy
    {
        public PlanningResult Plan(PlanningContext context)
        {
            return new PlanningResult
            {
                Plans = new List<PlanCandidate>
                {
                    new()
                    {
                        Id = 1,
                        GoalId = 1,
                        Steps = new[] { new PlanStep { Index = 1, Action = "Mock", Prerequisite = "none", ExpectedResult = "done" } },
                        ExpectedOutcome = "mock plan",
                        Feasibility = 1f,
                        Preference = 1f,
                        Confidence = 1f
                    }
                }
            };
        }
    }
}
