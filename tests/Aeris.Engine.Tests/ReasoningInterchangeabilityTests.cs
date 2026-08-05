using System;
using System.Collections.Generic;
using System.Linq;
using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class ReasoningInterchangeabilityTests
{
    [Fact]
    public void RI_001_CommonContract_BothStrategiesImplementIReasoningStrategy()
    {
        var strategies = new IReasoningStrategy[]
        {
            new EvidenceBasedReasoningStrategy(),
            new AlternativeReasoningStrategy()
        };

        foreach (var strategy in strategies)
        {
            typeof(IReasoningStrategy).IsAssignableFrom(strategy.GetType()).Should().BeTrue(
                $"{strategy.GetType().Name} must implement IReasoningStrategy");

            var result = strategy.Reason(BuildContext());

            result.Should().NotBeNull();
            result.Inferences.Should().NotBeNull();
            result.Evidence.Should().NotBeNull();
            result.Inferences.Should().NotBeEmpty("both strategies must produce valid inferences");

            foreach (var inference in result.Inferences)
            {
                inference.Id.Should().BeGreaterThan(0);
                inference.RuleId.Should().NotBeNullOrEmpty();
                inference.Transformation.Should().NotBeNullOrEmpty();
                inference.Conclusion.Should().NotBeNullOrEmpty();
                inference.Premises.Should().NotBeNullOrEmpty();
                inference.Confidence.Should().BeInRange(0f, 1f);
            }

            foreach (var evidence in result.Evidence)
            {
                evidence.InferenceId.Should().BeGreaterThan(0);
                evidence.RuleId.Should().NotBeNullOrEmpty();
                evidence.Transformation.Should().NotBeNullOrEmpty();
                evidence.Confidence.Should().BeInRange(0f, 1f);
                evidence.EvidenceStrength.Should().BeInRange(0f, 1f);
                evidence.Strategy.Should().Be(strategy.GetType().Name,
                    "evidence must identify the producing strategy");
            }
        }
    }

    [Fact]
    public void RI_002_Determinism_SameInputSameStrategySameResult()
    {
        var strategies = new IReasoningStrategy[]
        {
            new EvidenceBasedReasoningStrategy(),
            new AlternativeReasoningStrategy()
        };

        foreach (var strategy in strategies)
        {
            string run1 = Serialize(strategy.Reason(BuildContext()));
            string run2 = Serialize(strategy.Reason(BuildContext()));

            run1.Should().Be(run2, $"{strategy.GetType().Name} must be deterministic");
        }
    }

    [Fact]
    public void RI_003_NoSideEffects_SwitchingStrategyPreservesSharedState()
    {
        var world = CreateReasoningWorld();

        string wmHash = HashWorkingMemory(world.GetResource<WorkingMemoryStore>());
        string modelHash = HashWorldModel(world.GetResource<WorldModelState>());
        string goalsHash = HashGoals(world.GetResource<GoalStore>());
        string affectHash = HashAffect(world.GetResource<AffectState>());
        string ltmHash = HashLongTermMemory(world.GetResource<MemoryStore>());

        var system = new ReasoningSystem { Strategy = new EvidenceBasedReasoningStrategy() };
        system.Execute(world, 1f);
        AssertStoresUnchanged(world, wmHash, modelHash, goalsHash, affectHash, ltmHash);

        system.Strategy = new AlternativeReasoningStrategy();
        system.Execute(world, 1f);
        AssertStoresUnchanged(world, wmHash, modelHash, goalsHash, affectHash, ltmHash);
    }

    [Fact]
    public void RI_004_LocalityOfEffect_OnlyInferencesAndEvidenceChange()
    {
        var world = CreateReasoningWorld();

        var planStore = new PlanStore();
        planStore.Plans.Add(new PlanCandidate
        {
            Id = 99,
            GoalId = 1,
            Steps = Array.Empty<PlanStep>(),
            ExpectedOutcome = "sentinel plan",
            Confidence = 1f,
            Feasibility = 1f,
            Preference = 1f,
            Cost = 0f,
            Risk = 0f
        });
        world.AddResource(planStore);

        var actionStore = new ActionStore();
        actionStore.LastResult = new DecisionResult
        {
            Status = DecisionStatus.Selected,
            Action = new SelectedAction { Action = "sentinel action" }
        };
        world.AddResource(actionStore);

        string plansHash = HashPlans(planStore);
        string actionHash = HashAction(actionStore);

        var system = new ReasoningSystem { Strategy = new EvidenceBasedReasoningStrategy() };
        system.Execute(world, 1f);
        string evidenceBased = CaptureInferenceState(world);

        system.Strategy = new AlternativeReasoningStrategy();
        system.Execute(world, 1f);
        string alternative = CaptureInferenceState(world);

        evidenceBased.Should().NotBe(alternative,
            "swapping the strategy must change the generated inferences and evidence");

        HashPlans(planStore).Should().Be(plansHash,
            "planning must not be modified by a reasoning strategy swap");
        HashAction(actionStore).Should().Be(actionHash,
            "decision/execution must not be modified by a reasoning strategy swap");
    }

    [Fact]
    public void RI_005_FullPipeline_BothStrategiesDeterministicTracedAndLocal()
    {
        var (worldEb, engineEb) = CreatePipeline(new EvidenceBasedReasoningStrategy());
        SetUpPipelineState(worldEb);
        engineEb.RunOneTick();

        var (worldAlt, engineAlt) = CreatePipeline(new AlternativeReasoningStrategy());
        SetUpPipelineState(worldAlt);
        engineAlt.RunOneTick();

        var actionEb = worldEb.GetResource<ActionStore>().LastResult;
        var actionAlt = worldAlt.GetResource<ActionStore>().LastResult;
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(actionEb.Status);
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(actionAlt.Status);

        AssertInferenceInvariants(worldEb);
        AssertInferenceInvariants(worldAlt);

        var plans = worldEb.GetResource<PlanStore>().Plans
            .Concat(worldAlt.GetResource<PlanStore>().Plans);
        foreach (var plan in plans)
        {
            plan.Feasibility.Should().BeInRange(0f, 1f);
            plan.Preference.Should().BeInRange(0f, 1f);
        }

        var traceEb = worldEb.GetResource<CognitiveTraceLog>().Entries;
        var traceAlt = worldAlt.GetResource<CognitiveTraceLog>().Entries;
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

        foreach (var system in expectedSystems)
        {
            traceEb.Should().Contain(e => e.System == system);
            traceAlt.Should().Contain(e => e.System == system);
        }

        traceEb.Should().Contain(e =>
            e.System == "ReasoningSystem" && e.Why.Contains("EvidenceBasedReasoningStrategy"));
        traceAlt.Should().Contain(e =>
            e.System == "ReasoningSystem" && e.Why.Contains("AlternativeReasoningStrategy"));

        foreach (var entry in traceEb.Concat(traceAlt))
            entry.TraceId.Should().BeGreaterThan(0);

        var inferencesEb = worldEb.GetResource<InferenceStore>().Inferences;
        var inferencesAlt = worldAlt.GetResource<InferenceStore>().Inferences;
        SerializeInferences(inferencesEb).Should().NotBe(
            SerializeInferences(inferencesAlt),
            "the strategy swap must be observable within the reasoning domain");

        CapturePipelineState(worldEb).Should().Be(
            CapturePipelineState(worldAlt),
            "the strategy swap must not alter any state outside the reasoning domain");
    }

    private static void AssertStoresUnchanged(
        World world, string wmHash, string modelHash, string goalsHash,
        string affectHash, string ltmHash)
    {
        HashWorkingMemory(world.GetResource<WorkingMemoryStore>()).Should().Be(wmHash);
        HashWorldModel(world.GetResource<WorldModelState>()).Should().Be(modelHash);
        HashGoals(world.GetResource<GoalStore>()).Should().Be(goalsHash);
        HashAffect(world.GetResource<AffectState>()).Should().Be(affectHash);
        HashLongTermMemory(world.GetResource<MemoryStore>()).Should().Be(ltmHash);
    }

    private static void AssertInferenceInvariants(World world)
    {
        var store = world.GetResource<InferenceStore>();
        store.Inferences.Should().NotBeEmpty();

        foreach (var inference in store.Inferences)
        {
            inference.Premises.Should().NotBeNullOrEmpty();
            inference.Confidence.Should().BeInRange(0f, 1f);
            inference.RuleId.Should().NotBeNullOrEmpty();
        }

        foreach (var evidence in store.Evidence)
        {
            evidence.Confidence.Should().BeInRange(0f, 1f);
            evidence.EvidenceStrength.Should().BeInRange(0f, 1f);
            evidence.Strategy.Should().NotBeNullOrEmpty();
        }
    }

    private static string CaptureInferenceState(World world)
    {
        var store = world.GetResource<InferenceStore>();
        return SerializeInferences(store.Inferences)
               + "|" + string.Join("|", store.Evidence.Select(e =>
                   $"{e.InferenceId}:{e.RuleId}:{e.PremiseCount}:{e.Transformation}:{e.Confidence:F6}:{e.EvidenceStrength:F6}:{e.Strategy}"));
    }

    private static string SerializeInferences(IEnumerable<Inference> inferences)
    {
        return string.Join("|", inferences.Select(i =>
            $"{i.Id}:{i.RuleId}:{i.Transformation}:{i.Conclusion}:{i.Confidence:F6}:{string.Join(",", i.Premises)}"));
    }

    private static string Serialize(ReasoningResult result)
    {
        return CaptureInferenceStateFromResult(result);
    }

    private static string CaptureInferenceStateFromResult(ReasoningResult result)
    {
        return SerializeInferences(result.Inferences)
               + "|" + string.Join("|", result.Evidence.Select(e =>
                   $"{e.InferenceId}:{e.RuleId}:{e.PremiseCount}:{e.Transformation}:{e.Confidence:F6}:{e.EvidenceStrength:F6}:{e.Strategy}"));
    }

    private static string CapturePipelineState(World world)
    {
        var parts = new List<string>();

        var action = world.GetResource<ActionStore>().LastResult;
        parts.Add($"action={action.Status}:{action.SelectedPlanId}:{action.Action.Action ?? ""}");

        var planStore = world.GetResource<PlanStore>();
        foreach (var plan in planStore.Plans.OrderBy(p => p.Id))
        {
            parts.Add($"plan={plan.Id}:{plan.GoalId}:{plan.Steps.Length}:{plan.Feasibility:F4}:{plan.Preference:F4}:{plan.ExpectedOutcome}");
        }

        parts.Add(HashWorkingMemory(world.GetResource<WorkingMemoryStore>()));
        parts.Add(HashWorldModel(world.GetResource<WorldModelState>()));
        parts.Add(HashGoals(world.GetResource<GoalStore>()));
        parts.Add(HashAffect(world.GetResource<AffectState>()));
        parts.Add(HashLongTermMemory(world.GetResource<MemoryStore>()));

        var trace = world.GetResource<CognitiveTraceLog>().Entries
            .Where(e => e.System != "ReasoningSystem")
            .Select(e => $"{e.TraceId}:{e.ParentTraceId?.ToString() ?? "null"}:{e.System}:{e.InputSummary}:{e.OutputSummary}:{e.Why}");
        parts.Add(string.Join("|", trace));

        return string.Join(";", parts);
    }

    private static string HashWorkingMemory(WorkingMemoryStore wm)
    {
        return string.Join("|", wm.Chunks.Select(c => $"{c.Id}:{c.Content}:{c.Salience:F4}"));
    }

    private static string HashWorldModel(WorldModelState model)
    {
        return string.Join(",", model.KnownEntityIds) + ":" + model.LastUpdateTick;
    }

    private static string HashGoals(GoalStore goals)
    {
        var parts = new List<string>();
        foreach (var kvp in goals.All)
        {
            foreach (var goal in kvp.Value)
                parts.Add($"{goal.Id}:{goal.Type}:{goal.Status}:{goal.Priority}:{goal.Urgency:F4}");
        }
        return string.Join("|", parts);
    }

    private static string HashAffect(AffectState affect)
    {
        return $"{affect.Curiosity:F4}:{affect.Stress:F4}:{affect.Confidence:F4}:{affect.Trust:F4}:" +
               $"{affect.Novelty:F4}:{affect.Attachment:F4}:{affect.Threat:F4}:{affect.RewardExpectation:F4}:{affect.CognitiveLoad:F4}";
    }

    private static string HashLongTermMemory(MemoryStore ltm)
    {
        var parts = new List<string>();
        foreach (var kvp in ltm.All)
        {
            foreach (var memory in kvp.Value)
                parts.Add($"{memory.Id}:{memory.Type}:{memory.Category}:{memory.Importance:F4}:{memory.Certainty:F4}:{memory.Forgotten}");
        }
        return string.Join("|", parts);
    }

    private static string HashPlans(PlanStore plans)
    {
        return string.Join("|", plans.Plans.Select(p => $"{p.Id}:{p.GoalId}:{p.Steps?.Length ?? 0}:{p.Feasibility:F4}:{p.Preference:F4}:{p.ExpectedOutcome}"));
    }

    private static string HashAction(ActionStore store)
    {
        return $"{store.LastResult.Status}:{store.LastResult.SelectedPlanId}:{store.LastResult.Action.Action ?? ""}:{store.LastExecutionTick}";
    }

    private static ReasoningContext BuildContext()
    {
        return new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "tree observed at location north", Salience = 0.8f },
                new() { Id = "w2", Content = "moss is near trees", Salience = 0.7f },
                new() { Id = "w3", Content = "rain observed", Salience = 0.6f },
                new() { Id = "w4", Content = "mushrooms grow after rain", Salience = 0.5f }
            },
            RetrievedMemories = new List<RetrievedMemoryEntry>
            {
                new() { Memory = new MemoryData { Id = 7, Category = MemoryCategory.Environmental }, Score = 0.9f }
            },
            ActiveGoals = new List<GoalData>
            {
                new()
                {
                    Id = 1,
                    Type = GoalType.Exploration,
                    Status = GoalStatus.Active,
                    Priority = GoalPriority.High,
                    Urgency = 0.5f
                }
            }
        };
    }

    private static World CreateReasoningWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new WorkingMemoryStore());
        world.AddResource(new WorldModelState());
        world.AddResource(new GoalStore());
        world.AddResource(new MemoryStore());
        world.AddResource(new CognitiveTraceLog());

        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "w1",
            Content = "tree observed at location north",
            Salience = 0.8f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "w2",
            Content = "moss is near trees",
            Salience = 0.7f,
            FormationTick = 0,
            LastAccessTick = 0
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 2, 3 };

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(1, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        world.SetResource(AffectState.Default);
        return world;
    }

    private static (World world, Engine engine) CreatePipeline(IReasoningStrategy reasoningStrategy)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new MemoryRetrievalSystem());
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem { Strategy = reasoningStrategy });
        engine.RegisterSystem(new PlanningSystem());
        engine.RegisterSystem(new DecisionSystem());

        engine.Initialize();
        return (world, engine);
    }

    private static void SetUpPipelineState(World world)
    {
        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "w1",
            Content = "tree observed at location north",
            Salience = 0.8f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "w2",
            Content = "moss is near trees",
            Salience = 0.7f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });

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
