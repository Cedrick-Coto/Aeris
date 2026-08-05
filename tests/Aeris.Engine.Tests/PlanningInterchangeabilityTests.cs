using System;
using System.Collections.Generic;
using System.Linq;
using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class PlanningInterchangeabilityTests
{
    [Fact]
    public void P_I001_CommonContract_BothStrategiesProduceValidPlansAndEvidence()
    {
        var strategies = new IPlanningStrategy[]
        {
            new GoalDirectedPlanningStrategy(),
            new GreedyPlanningStrategy()
        };

        var goalIds = BuildContext().ActiveGoals.Select(g => g.Id).ToList();

        foreach (var strategy in strategies)
        {
            typeof(IPlanningStrategy).IsAssignableFrom(strategy.GetType()).Should().BeTrue(
                $"{strategy.GetType().Name} must implement IPlanningStrategy");

            var result = strategy.Plan(BuildContext());

            result.Should().NotBeNull();
            result.Plans.Should().NotBeEmpty("both strategies must produce candidate plans");
            result.Evidence.Should().NotBeEmpty("both strategies must produce planning evidence");

            foreach (var plan in result.Plans)
            {
                goalIds.Should().Contain(plan.GoalId,
                    "every plan must reference an active goal (P-002)");
                plan.Steps.Should().NotBeEmpty("every plan must have steps");
                plan.ExpectedOutcome.Should().NotBeNullOrEmpty();
                plan.Confidence.Should().BeInRange(0f, 1f);
                plan.Feasibility.Should().BeInRange(0f, 1f);
                plan.Preference.Should().BeInRange(0f, 1f);
                plan.Cost.Should().BeInRange(0f, 1f);
                plan.Risk.Should().BeInRange(0f, 1f);

                foreach (var step in plan.Steps)
                {
                    step.Index.Should().BeGreaterThan(0);
                    step.Action.Should().NotBeNullOrEmpty();
                }
            }

            foreach (var evidence in result.Evidence)
            {
                evidence.PlanId.Should().BeGreaterThan(0);
                goalIds.Should().Contain(evidence.GoalId,
                    "evidence must reference an active goal");
                evidence.StepCount.Should().BeGreaterThan(0);
                evidence.Strategy.Should().Be(strategy.GetType().Name,
                    "evidence must identify the producing strategy");
                evidence.ElapsedMicroseconds.Should().BeGreaterThanOrEqualTo(0);
            }

            result.Evidence.Select(e => e.Strategy).Distinct().Should().ContainSingle(
                "all evidence within one run must come from the same strategy");
        }
    }

    [Fact]
    public void P_I002A_Determinism_SameInputSameStrategySameSerializedResult()
    {
        var strategies = new IPlanningStrategy[]
        {
            new GoalDirectedPlanningStrategy(),
            new GreedyPlanningStrategy()
        };

        foreach (var strategy in strategies)
        {
            string run1 = Serialize(strategy.Plan(BuildContext()));
            string run2 = Serialize(strategy.Plan(BuildContext()));

            run1.Should().Be(run2, $"{strategy.GetType().Name} must be deterministic");
        }
    }

    [Fact]
    public void P_I002B_NoSideEffects_StrategyPlanDoesNotMutateInputs()
    {
        var context = BuildContext();
        string wmHash = HashWorkingMemory(context.WorkingMemory);
        string modelHash = HashWorldModel(context.WorldModel);
        string affectHash = HashAffect(context.Affect);
        string inferenceHash = HashInferences(context.AvailableInferences);
        string goalsHash = HashGoals(context.ActiveGoals);

        new GoalDirectedPlanningStrategy().Plan(context);

        HashWorkingMemory(context.WorkingMemory).Should().Be(wmHash,
            "GoalDirectedPlanningStrategy must not mutate working memory");
        HashWorldModel(context.WorldModel).Should().Be(modelHash,
            "GoalDirectedPlanningStrategy must not mutate the world model");
        HashAffect(context.Affect).Should().Be(affectHash,
            "GoalDirectedPlanningStrategy must not mutate affect state");
        HashInferences(context.AvailableInferences).Should().Be(inferenceHash,
            "GoalDirectedPlanningStrategy must not mutate inferences");
        HashGoals(context.ActiveGoals).Should().Be(goalsHash,
            "GoalDirectedPlanningStrategy must not mutate active goals");

        context = BuildContext();
        wmHash = HashWorkingMemory(context.WorkingMemory);
        modelHash = HashWorldModel(context.WorldModel);
        affectHash = HashAffect(context.Affect);
        inferenceHash = HashInferences(context.AvailableInferences);
        goalsHash = HashGoals(context.ActiveGoals);

        new GreedyPlanningStrategy().Plan(context);

        HashWorkingMemory(context.WorkingMemory).Should().Be(wmHash,
            "GreedyPlanningStrategy must not mutate working memory");
        HashWorldModel(context.WorldModel).Should().Be(modelHash,
            "GreedyPlanningStrategy must not mutate the world model");
        HashAffect(context.Affect).Should().Be(affectHash,
            "GreedyPlanningStrategy must not mutate affect state");
        HashInferences(context.AvailableInferences).Should().Be(inferenceHash,
            "GreedyPlanningStrategy must not mutate inferences");
        HashGoals(context.ActiveGoals).Should().Be(goalsHash,
            "GreedyPlanningStrategy must not mutate active goals");
    }

    [Fact]
    public void P_I003_FullPipeline_GreedyCompletesTracedAndDownstreamValid()
    {
        var (world, engine) = CreatePipeline(new GreedyPlanningStrategy());
        SetUpPipelineState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("pipeline with GreedyPlanningStrategy must complete");

        var trace = world.GetResource<CognitiveTraceLog>().Entries;
        trace.Should().Contain(e =>
                e.System == "PlanningSystem" && e.Why.Contains("GreedyPlanningStrategy"),
            "greedy planning must be registered in the causal trace");

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().NotBeEmpty("greedy must produce plans with the pipeline state");

        var actionStore = world.GetResource<ActionStore>();
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(
            actionStore.LastResult.Status,
            "decision must consume plans produced by greedy");

        var expectedSystems = new[]
        {
            "PerceptionSystem",
            "AttentionSystem",
            "MemoryRetrievalSystem",
            "WorkingMemorySystem",
            "ReasoningSystem",
            "PlanningSystem",
            "DecisionSystem",
            "AuditSystem",
            "EnforcementSystem"
        };

        foreach (var system in expectedSystems)
            trace.Should().Contain(e => e.System == system,
                $"causal trace must include {system}");

        foreach (var entry in trace)
            entry.TraceId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void P_I004_DecisionIsDecoupled_BothStrategiesConsumedViaPlanStore()
    {
        var (worldBaseline, engineBaseline) = CreatePipeline(new GoalDirectedPlanningStrategy());
        SetUpPipelineState(worldBaseline);
        engineBaseline.RunOneTick();

        var (worldGreedy, engineGreedy) = CreatePipeline(new GreedyPlanningStrategy());
        SetUpPipelineState(worldGreedy);
        engineGreedy.RunOneTick();

        var actionBaseline = worldBaseline.GetResource<ActionStore>().LastResult;
        var actionGreedy = worldGreedy.GetResource<ActionStore>().LastResult;

        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(
            actionBaseline.Status,
            "decision must handle plans from GoalDirectedPlanningStrategy");
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(
            actionGreedy.Status,
            "decision must handle plans from GreedyPlanningStrategy");

        worldBaseline.GetResource<PlanStore>().Evidence.Should().Contain(e =>
            e.Strategy == nameof(GoalDirectedPlanningStrategy));
        worldGreedy.GetResource<PlanStore>().Evidence.Should().Contain(e =>
            e.Strategy == nameof(GreedyPlanningStrategy));

        var decisionTraceBaseline = worldBaseline.GetResource<CognitiveTraceLog>().Entries;
        var decisionTraceGreedy = worldGreedy.GetResource<CognitiveTraceLog>().Entries;

        decisionTraceBaseline.Should().Contain(e =>
                e.System == "DecisionSystem" && e.Why.Contains(nameof(FeasibilityThresholdPolicy)),
            "decision runs its own policy regardless of the planning strategy");
        decisionTraceGreedy.Should().Contain(e =>
                e.System == "DecisionSystem" && e.Why.Contains(nameof(FeasibilityThresholdPolicy)),
            "decision runs its own policy regardless of the planning strategy");

        decisionTraceGreedy.Should().NotContain(e =>
                e.System == "DecisionSystem" && e.Why.Contains(nameof(GreedyPlanningStrategy)),
            "decision must not depend on which planning strategy produced the plans");
    }

    [Fact]
    public void P_I005_RuntimeSwap_BetweenTicksReflectsActiveStrategy()
    {
        var world = CreatePlanningWorld();
        var system = new PlanningSystem();

        var time = world.GetResource<TimeResource>();
        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(1, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        system.Execute(world, 1f);
        var baselineEvidence = world.GetResource<PlanStore>().Evidence;
        baselineEvidence.Should().NotBeEmpty();
        baselineEvidence.Should().OnlyContain(e => e.Strategy == nameof(GoalDirectedPlanningStrategy),
            "default strategy is GoalDirectedPlanningStrategy");

        time.Advance(1f);
        world.SetResource(time);

        system.Strategy = new GreedyPlanningStrategy();
        system.Execute(world, 1f);

        var greedyEvidence = world.GetResource<PlanStore>().Evidence;
        greedyEvidence.Should().NotBeEmpty();
        greedyEvidence.Should().OnlyContain(e => e.Strategy == nameof(GreedyPlanningStrategy),
            "after swapping the strategy, evidence must reflect GreedyPlanningStrategy");

        world.GetResource<PlanStore>().Plans.Should().NotBeEmpty();

        world.GetResource<WorkingMemoryStore>().Chunks.Should().BeEmpty(
            "planning must not write to working memory (P-004)");
        world.GetResource<WorldModelState>().KnownEntityIds.Should().BeEmpty(
            "planning must not modify the world model (P-004)");
    }

    [Fact]
    public void P_I005A_Determinism_RuntimeSwapReplayable()
    {
        string seq1 = RunSwapSequence();
        string seq2 = RunSwapSequence();

        seq1.Should().Be(seq2, "runtime strategy swap must be deterministic and replayable");
    }

    private static string RunSwapSequence()
    {
        var world = CreatePlanningWorld();
        var time = world.GetResource<TimeResource>();
        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(1, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        var system = new PlanningSystem();
        system.Execute(world, 1f);
        var store1 = world.GetResource<PlanStore>();
        string s1 = Serialize(new PlanningResult { Plans = store1.Plans, Evidence = store1.Evidence });

        time.Advance(1f);
        world.SetResource(time);

        system.Strategy = new GreedyPlanningStrategy();
        system.Execute(world, 1f);
        var store2 = world.GetResource<PlanStore>();
        string s2 = Serialize(new PlanningResult { Plans = store2.Plans, Evidence = store2.Evidence });

        return s1 + "||" + s2;
    }

    private static PlanningContext BuildContext()
    {
        return new PlanningContext
        {
            ActiveGoals = new List<GoalData>
            {
                new() { Id = 1, Type = GoalType.Exploration, Status = GoalStatus.Active, Priority = GoalPriority.High, Urgency = 0.5f },
                new() { Id = 2, Type = GoalType.Collection, Status = GoalStatus.Active, Priority = GoalPriority.Low, Urgency = 0.2f }
            },
            WorldModel = new WorldModelState { KnownEntityIds = new List<uint> { 1 } },
            AvailableInferences = new List<Inference>
            {
                new() { Id = 1, Conclusion = "location_tracked", Confidence = 0.8f }
            },
            Affect = AffectState.Default,
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "c1", Content = "entity 1 present", SourceEntity = new EntityId(1), Salience = 0.8f, DecayRate = 0.08f, FormationTick = 0, LastAccessTick = 0 },
                new() { Id = "c2", Content = "ambient context", SourceEntity = null, Salience = 0.4f, DecayRate = 0.08f, FormationTick = 0, LastAccessTick = 0 }
            }
        };
    }

    private static World CreatePlanningWorld()
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
        return world;
    }

    private static (World world, Engine engine) CreatePipeline(IPlanningStrategy planningStrategy)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new MemoryRetrievalSystem { Strategy = new LinearScanStrategy() });
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem { Strategy = new EvidenceBasedReasoningStrategy() });
        engine.RegisterSystem(new PlanningSystem { Strategy = planningStrategy });
        engine.RegisterSystem(new DecisionSystem { Strategy = new FeasibilityThresholdPolicy() });
        engine.RegisterSystem(new AuditSystem { Strategy = new SequentialRuleEvaluator() });
        engine.RegisterSystem(new EnforcementSystem { Policy = new StrictPolicy() });

        engine.Initialize();
        return (world, engine);
    }

    private static void SetUpPipelineState(World world)
    {
        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_2",
            Content = "entity 2 observed north",
            SourceType = PerceptType.Visual,
            SourceEntity = new EntityId(2),
            Salience = 0.8f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_3",
            Content = "entity 3 observed south",
            SourceType = PerceptType.Auditory,
            SourceEntity = new EntityId(3),
            Salience = 0.5f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });

        var agent = world.CreateEntity()
            .With(new CognitiveAgentMarker { AgentId = 1 })
            .Build();

        var memories = world.GetResource<MemoryStore>();
        var memId = memories.AllocateId();
        memories.AddMemory(agent.Id.Value, new MemoryData
        {
            Id = memId,
            Type = MemoryType.Observed,
            Category = MemoryCategory.Social,
            Importance = 0.9f,
            Certainty = 0.9f,
            Timestamp = 0f,
            InvolvedEntityId = 2
        });

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(agent.Id.Value, new GoalData
        {
            Id = 1,
            Type = GoalType.Social,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 2, 3 };

        world.SetResource(AffectState.Default);

        world.CreateEntity()
            .With(new VisualTag { LabelId = 42, Size = 1f })
            .Build();
    }

    private static string Serialize(PlanningResult result)
    {
        return string.Join(";",
            result.Plans.Select(p =>
                    $"P:{p.Id}:{p.GoalId}:{p.Steps.Length}:{p.Confidence:F6}:{p.Feasibility:F6}:{p.Preference:F6}:{p.Cost:F6}:{p.Risk:F6}:{p.ExpectedOutcome}:{string.Join(",", p.Steps.Select(s => s.Action))}")
                .Concat(
                    result.Evidence.Select(e =>
                        $"E:{e.PlanId}:{e.GoalId}:{e.StepCount}:{e.Strategy}:{e.ElapsedMicroseconds}")));
    }

    private static string HashGoals(List<GoalData> goals)
    {
        return string.Join("|", goals.Select(g =>
            $"{g.Id}:{g.Type}:{g.Status}:{g.Priority}:{g.Urgency:F4}"));
    }

    private static string HashWorldModel(WorldModelState model)
    {
        return string.Join(",", model.KnownEntityIds) + ":" + model.LastUpdateTick;
    }

    private static string HashAffect(AffectState affect)
    {
        return $"{affect.Curiosity:F4}:{affect.Stress:F4}:{affect.Confidence:F4}:{affect.Trust:F4}:" +
               $"{affect.Novelty:F4}:{affect.Attachment:F4}:{affect.Threat:F4}:{affect.RewardExpectation:F4}:{affect.CognitiveLoad:F4}";
    }

    private static string HashInferences(List<Inference> inferences)
    {
        return string.Join("|", inferences.Select(i =>
            $"{i.Id}:{i.Conclusion}:{i.Confidence:F4}"));
    }

    private static string HashWorkingMemory(List<WorkingMemoryChunk> chunks)
    {
        return string.Join("|", chunks.Select(c =>
            $"{c.Id}:{c.Content}:{c.Salience:F4}:{c.SourceEntity?.Value.ToString() ?? "null"}"));
    }
}
