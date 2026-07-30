using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class DecisionTests
{
    [Fact]
    public void S_D001_SelectByFeasibility()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.3f, Steps = new[] { new PlanStep { Index = 1, Action = "MoveNorth" } }, Confidence = 0.7f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.4f, Preference = 0.8f, Steps = new[] { new PlanStep { Index = 1, Action = "Explore" } }, Confidence = 0.5f }
        });

        var result = strategy.Decide(context);

        result.Status.Should().Be(DecisionStatus.Selected);
        result.SelectedPlanId.Should().Be(1);
        result.Action.Action.Should().Be("MoveNorth");
    }

    [Fact]
    public void S_D002_SelectByPreferenceAmongViable()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.8f, Preference = 0.3f, Steps = new[] { new PlanStep { Index = 1, Action = "MoveNorth" } }, Confidence = 0.7f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.8f, Preference = 0.7f, Steps = new[] { new PlanStep { Index = 1, Action = "Explore" } }, Confidence = 0.6f }
        });

        var result = strategy.Decide(context);

        result.Status.Should().Be(DecisionStatus.Selected);
        result.SelectedPlanId.Should().Be(2);
        result.Action.Action.Should().Be("Explore");
    }

    [Fact]
    public void S_D003_NoViablePlanReturnsNoViablePlan()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.2f, Preference = 0.9f, Steps = new[] { new PlanStep { Index = 1, Action = "RiskyMove" } }, Confidence = 0.3f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.1f, Preference = 0.8f, Steps = new[] { new PlanStep { Index = 1, Action = "Desperate" } }, Confidence = 0.2f }
        });

        var result = strategy.Decide(context);

        result.Status.Should().Be(DecisionStatus.NoViablePlan);
        result.SelectedPlanId.Should().BeNull();
        result.Action.Action.Should().Be("Defer");
        result.Evidence.CandidatesConsidered.Should().Be(2);
    }

    [Fact]
    public void S_D004_StressRelaxesThreshold()
    {
        var plan = new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.4f, Preference = 0.8f, Steps = new[] { new PlanStep { Index = 1, Action = "Explore" } }, Confidence = 0.6f };

        var lowStressContext = CreateContext(new[] { plan },
            affect: AffectState.Default with { Stress = 0.1f, Confidence = 0.5f });
        var highStressContext = CreateContext(new[] { plan },
            affect: AffectState.Default with { Stress = 0.9f, Confidence = 0.5f });

        var lowStressResult = new FeasibilityThresholdPolicy().Decide(lowStressContext);
        var highStressResult = new FeasibilityThresholdPolicy().Decide(highStressContext);

        lowStressResult.Status.Should().Be(DecisionStatus.NoViablePlan,
            "low stress keeps threshold high, excluding feasibility 0.4");
        highStressResult.Status.Should().Be(DecisionStatus.Selected,
            "high stress lowers threshold, making feasibility 0.4 selectable");
    }

    [Fact]
    public void S_D005_NoPlanGeneration()
    {
        var candidates = new List<PlanCandidate>
        {
            new() { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.5f, Steps = new[] { new PlanStep { Index = 1, Action = "Move" } }, Confidence = 0.7f }
        };

        var context = CreateContext(candidates.ToArray());
        var result = new FeasibilityThresholdPolicy().Decide(context);

        result.Status.Should().Be(DecisionStatus.Selected);
        context.CandidatePlans.Count.Should().Be(1,
            "Decision must not create, remove, or modify candidate plans");
    }

    [Fact]
    public void S_D006_StrategyReplacement()
    {
        var world = CreateWorld(out var planStore, out _, out _);
        planStore.Plans.AddRange(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.5f, Steps = new[] { new PlanStep { Index = 1, Action = "MockAction" } }, Confidence = 0.7f }
        });

        var system = new DecisionSystem
        {
            Strategy = new MockDecisionStrategy()
        };

        system.Execute(world, 1f);

        var actionStore = world.GetResource<ActionStore>();
        actionStore.LastResult.Status.Should().Be(DecisionStatus.Selected);
        actionStore.LastResult.Action.Action.Should().Be("MockSelected");
    }

    [Fact]
    public void S_D007_Determinism()
    {
        var candidates = new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.3f, Steps = new[] { new PlanStep { Index = 1, Action = "A" } }, Confidence = 0.7f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.8f, Preference = 0.7f, Steps = new[] { new PlanStep { Index = 1, Action = "B" } }, Confidence = 0.6f },
            new PlanCandidate { Id = 3, GoalId = 1, Feasibility = 0.6f, Preference = 0.9f, Steps = new[] { new PlanStep { Index = 1, Action = "C" } }, Confidence = 0.5f }
        };

        string result1 = RunDeterministicSession(candidates);
        string result2 = RunDeterministicSession(candidates);

        result1.Should().Be(result2);
    }

    [Fact]
    public void S_D008_NoSideEffects()
    {
        var world = CreateWorld(out var planStore, out var goals, out var model);
        planStore.Plans.Add(new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.5f, Steps = new[] { new PlanStep { Index = 1, Action = "Move" } }, Confidence = 0.7f });

        string goalsHash = HashGoals(goals);
        string modelHash = HashModel(model);

        var system = new DecisionSystem();
        system.Execute(world, 1f);

        HashGoals(goals).Should().Be(goalsHash);
        HashModel(model).Should().Be(modelHash);
    }

    [Fact]
    public void D_T001_NoFeasibilityOverride()
    {
        var candidates = new List<PlanCandidate>
        {
            new() { Id = 1, GoalId = 1, Feasibility = 0.4f, Preference = 0.9f, Steps = new[] { new PlanStep { Index = 1, Action = "Risky" } }, Confidence = 0.3f }
        };

        var context = CreateContext(candidates.ToArray());
        float originalFeasibility = candidates[0].Feasibility;

        new FeasibilityThresholdPolicy().Decide(context);

        candidates[0].Feasibility.Should().Be(originalFeasibility,
            "Decision must never modify plan feasibility (D-003)");
    }

    [Fact]
    public void D_T003_TieBreakingByPreference()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.4f, Risk = 0.3f, Steps = new[] { new PlanStep { Index = 1, Action = "LowPref" } }, Confidence = 0.7f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.9f, Preference = 0.8f, Risk = 0.1f, Steps = new[] { new PlanStep { Index = 1, Action = "HighPref" } }, Confidence = 0.7f }
        });

        var result = strategy.Decide(context);

        result.SelectedPlanId.Should().Be(2);
        result.Evidence.Reason.TieBreaker.Should().Be("HighestPreference");
    }

    [Fact]
    public void DecisionSystem_ExecutesInCausalChain()
    {
        var world = CreateWorld(out var planStore, out _, out _);
        planStore.Plans.Add(new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.5f, Steps = new[] { new PlanStep { Index = 1, Action = "Move" } }, Confidence = 0.7f });

        var system = new DecisionSystem();
        system.Execute(world, 1f);

        var actionStore = world.GetResource<ActionStore>();
        actionStore.LastResult.Status.Should().Be(DecisionStatus.Selected);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "DecisionSystem");
    }

    [Fact]
    public void EmptyCandidatePlansReturnsNoViablePlan()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(Array.Empty<PlanCandidate>());

        var result = strategy.Decide(context);

        result.Status.Should().Be(DecisionStatus.NoViablePlan);
        result.Action.Action.Should().Be("Defer");
    }

    [Fact]
    public void EvidenceContainsSelectionReason()
    {
        var strategy = new FeasibilityThresholdPolicy();
        var context = CreateContext(new[]
        {
            new PlanCandidate { Id = 1, GoalId = 1, Feasibility = 0.9f, Preference = 0.5f, Steps = new[] { new PlanStep { Index = 1, Action = "Move" } }, Confidence = 0.7f },
            new PlanCandidate { Id = 2, GoalId = 1, Feasibility = 0.3f, Preference = 0.8f, Steps = new[] { new PlanStep { Index = 1, Action = "Skip" } }, Confidence = 0.4f }
        });

        var result = strategy.Decide(context);

        result.Evidence.Reason.Policy.Should().Be(nameof(FeasibilityThresholdPolicy));
        result.Evidence.Reason.Threshold.Should().BeGreaterThan(0f);
        result.Evidence.Reason.Rejected.Should().Contain(r => r.PlanId == 2);
        result.Evidence.Reason.Selected.Should().NotBeNull();
        result.Evidence.Reason.Selected.Value.PlanId.Should().Be(1);
    }

    private static string RunDeterministicSession(PlanCandidate[] candidates)
    {
        var world = CreateWorld(out var planStore, out _, out _);
        planStore.Plans.AddRange(candidates);

        var system = new DecisionSystem();
        system.Execute(world, 1f);

        var store = world.GetResource<ActionStore>();
        return $"{store.LastResult.Status}|{store.LastResult.SelectedPlanId}|{store.LastResult.Action.Action}";
    }

    private static DecisionContext CreateContext(PlanCandidate[] plans, AffectState? affect = null)
    {
        return new DecisionContext
        {
            CandidatePlans = plans.ToList(),
            WorldModel = new WorldModelState { KnownEntityIds = new List<uint> { 1 } },
            Affect = affect ?? AffectState.Default,
            ActiveGoals = new List<GoalData>
            {
                new() { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High }
            }
        };
    }

    private static World CreateWorld(
        out PlanStore planStore, out GoalStore goals, out WorldModelState model)
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
        goals.AddGoal(1, new GoalData { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High });
        model = world.GetResource<WorldModelState>();
        planStore = world.GetResource<PlanStore>();
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

    private sealed class MockDecisionStrategy : IDecisionStrategy
    {
        public DecisionResult Decide(DecisionContext context)
        {
            return new DecisionResult
            {
                Status = DecisionStatus.Selected,
                SelectedPlanId = 99,
                Action = new SelectedAction { PlanId = 99, Action = "MockSelected", GoalId = 1, Confidence = 1f },
                Evidence = new DecisionEvidence
                {
                    Status = DecisionStatus.Selected,
                    CandidatesConsidered = context.CandidatePlans.Count,
                    SelectionPolicy = "Mock",
                    Threshold = 0.5f,
                    Reason = new SelectionReason { Policy = "Mock", Threshold = 0.5f }
                }
            };
        }
    }
}
