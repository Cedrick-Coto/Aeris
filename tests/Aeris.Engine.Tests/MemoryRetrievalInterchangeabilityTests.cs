using System;
using System.Collections.Generic;
using System.Linq;
using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class MemoryRetrievalInterchangeabilityTests
{
    [Fact]
    public void MR_I001_CommonContract_LinearScanAndContextualSpreadImplementSameContract()
    {
        var strategies = new IMemoryRetrievalStrategy[]
        {
            new LinearScanStrategy(),
            new ContextualSpreadStrategy()
        };

        foreach (var strategy in strategies)
        {
            typeof(IMemoryRetrievalStrategy).IsAssignableFrom(strategy.GetType()).Should().BeTrue(
                $"{strategy.GetType().Name} must implement IMemoryRetrievalStrategy");

            var result = strategy.Retrieve(BuildContext());

            result.Should().NotBeNull();
            result.Memories.Should().NotBeEmpty("both strategies must produce valid retrieved memories");
            result.Evidence.Should().NotBeEmpty("both strategies must produce retrieval evidence");

            foreach (var entry in result.Memories)
            {
                entry.Memory.Id.Should().BeGreaterThan(0);
                entry.Score.Should().BeInRange(0f, 1f);
            }

            foreach (var evidence in result.Evidence)
            {
                evidence.MemoryId.Should().BeGreaterThan(0);
                evidence.Operation.Should().Be(RetrievalOperation.Retrieved);
                evidence.FinalScore.Should().BeInRange(0f, 1f);
                evidence.Strategy.Should().Be(strategy.GetType().Name,
                    "evidence must identify the producing strategy");
            }
        }
    }

    [Fact]
    public void MR_I002_Determinism_SameInputSameStrategySameSerializedResult()
    {
        var strategies = new IMemoryRetrievalStrategy[]
        {
            new LinearScanStrategy(),
            new ContextualSpreadStrategy()
        };

        foreach (var strategy in strategies)
        {
            string run1 = Serialize(strategy.Retrieve(BuildContext()));
            string run2 = Serialize(strategy.Retrieve(BuildContext()));

            run1.Should().Be(run2, $"{strategy.GetType().Name} must be deterministic");
        }
    }

    [Fact]
    public void MR_I003_NoSideEffects_StrategyRetrieveDoesNotMutateInputs()
    {
        var context = BuildContext();
        string candidatesHash = HashCandidates(context.CandidateMemories);
        string wmHash = HashWorkingMemory(context.WorkingMemory);
        string affectHash = HashAffect(context.AffectState);

        new LinearScanStrategy().Retrieve(context);

        HashCandidates(context.CandidateMemories).Should().Be(candidatesHash,
            "LinearScanStrategy must not mutate candidate memories (LTM)");
        HashWorkingMemory(context.WorkingMemory).Should().Be(wmHash,
            "LinearScanStrategy must not mutate working memory");
        HashAffect(context.AffectState).Should().Be(affectHash,
            "LinearScanStrategy must not mutate affect state");

        context = BuildContext();
        candidatesHash = HashCandidates(context.CandidateMemories);
        wmHash = HashWorkingMemory(context.WorkingMemory);
        affectHash = HashAffect(context.AffectState);

        new ContextualSpreadStrategy().Retrieve(context);

        HashCandidates(context.CandidateMemories).Should().Be(candidatesHash,
            "ContextualSpreadStrategy must not mutate candidate memories (LTM)");
        HashWorkingMemory(context.WorkingMemory).Should().Be(wmHash,
            "ContextualSpreadStrategy must not mutate working memory");
        HashAffect(context.AffectState).Should().Be(affectHash,
            "ContextualSpreadStrategy must not mutate affect state");
    }

    [Fact]
    public void MR_I004_LocalityOfEffect_OnlyRetrievalOutputChanges()
    {
        var worldLinear = CreateRetrievalWorld();
        new MemoryRetrievalSystem { Strategy = new LinearScanStrategy() }.Execute(worldLinear, 1f);

        var worldSpread = CreateRetrievalWorld();
        new MemoryRetrievalSystem { Strategy = new ContextualSpreadStrategy() }.Execute(worldSpread, 1f);

        var wmLinear = worldLinear.GetResource<WorkingMemoryStore>();
        var wmSpread = worldSpread.GetResource<WorkingMemoryStore>();

        HashRetrievedChunks(wmLinear).Should().NotBe(HashRetrievedChunks(wmSpread),
            "swapping the retrieval strategy must change retrieval output");

        HashLongTermMemory(worldLinear.GetResource<MemoryStore>()).Should().Be(
            HashLongTermMemory(worldSpread.GetResource<MemoryStore>()),
            "LTM must be unchanged by a retrieval strategy swap");
        HashWorldModel(worldLinear.GetResource<WorldModelState>()).Should().Be(
            HashWorldModel(worldSpread.GetResource<WorldModelState>()),
            "world model must be unchanged by a retrieval strategy swap");
        HashGoals(worldLinear.GetResource<GoalStore>()).Should().Be(
            HashGoals(worldSpread.GetResource<GoalStore>()),
            "goals must be unchanged by a retrieval strategy swap");
        HashAffect(worldLinear.GetResource<AffectState>()).Should().Be(
            HashAffect(worldSpread.GetResource<AffectState>()),
            "affect state must be unchanged by a retrieval strategy swap");

        HashStaticChunks(wmLinear).Should().Be(HashStaticChunks(wmSpread),
            "chunks outside the retrieval output channel must be unchanged by a strategy swap");
    }

    [Fact]
    public void MR_I005_FullPipeline_BothStrategiesDeterministicTracedAndLocal()
    {
        var (worldLinear, engineLinear) = CreatePipeline(new LinearScanStrategy());
        SetUpPipelineState(worldLinear);
        engineLinear.RunOneTick();

        var (worldSpread, engineSpread) = CreatePipeline(new ContextualSpreadStrategy());
        SetUpPipelineState(worldSpread);
        engineSpread.RunOneTick();

        var actionLinear = worldLinear.GetResource<ActionStore>().LastResult;
        var actionSpread = worldSpread.GetResource<ActionStore>().LastResult;
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(actionLinear.Status,
            "pipeline with LinearScanStrategy must produce a valid decision result");
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }.Should().Contain(actionSpread.Status,
            "pipeline with ContextualSpreadStrategy must produce a valid decision result");

        var traceLinear = worldLinear.GetResource<CognitiveTraceLog>().Entries;
        var traceSpread = worldSpread.GetResource<CognitiveTraceLog>().Entries;
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
            traceLinear.Should().Contain(e => e.System == system);
            traceSpread.Should().Contain(e => e.System == system);
        }

        traceLinear.Should().Contain(e =>
            e.System == "MemoryRetrievalSystem" && e.Why.Contains("LinearScanStrategy"),
            "linear scan must be registered in the causal trace");
        traceSpread.Should().Contain(e =>
            e.System == "MemoryRetrievalSystem" && e.Why.Contains("ContextualSpreadStrategy"),
            "contextual spread must be registered in the causal trace");

        foreach (var entry in traceLinear.Concat(traceSpread))
            entry.TraceId.Should().BeGreaterThan(0);

        HashRetrievedChunks(worldLinear.GetResource<WorkingMemoryStore>()).Should().NotBe(
            HashRetrievedChunks(worldSpread.GetResource<WorkingMemoryStore>()),
            "the retrieval strategy swap must be observable within the retrieval domain");

        HashLongTermMemory(worldLinear.GetResource<MemoryStore>()).Should().Be(
            HashLongTermMemory(worldSpread.GetResource<MemoryStore>()),
            "LTM must be unchanged by the retrieval strategy swap in a full pipeline");
        HashWorldModel(worldLinear.GetResource<WorldModelState>()).Should().Be(
            HashWorldModel(worldSpread.GetResource<WorldModelState>()),
            "world model must be unchanged by the retrieval strategy swap in a full pipeline");
        HashGoals(worldLinear.GetResource<GoalStore>()).Should().Be(
            HashGoals(worldSpread.GetResource<GoalStore>()),
            "goals must be unchanged by the retrieval strategy swap in a full pipeline");
        HashAffect(worldLinear.GetResource<AffectState>()).Should().Be(
            HashAffect(worldSpread.GetResource<AffectState>()),
            "affect state must be unchanged by the retrieval strategy swap in a full pipeline");
    }

    private static MemoryRetrievalContext BuildContext()
    {
        var wm = new WorkingMemoryStore();
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "c1",
            Content = "entity 7 present",
            SourceEntity = new EntityId(7),
            Salience = 0.9f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "c2",
            Content = "ambient context",
            SourceEntity = null,
            Salience = 0.4f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });

        return new MemoryRetrievalContext
        {
            CandidateMemories = BuildMemories(),
            WorkingMemory = wm,
            AffectState = BuildAffect(),
            CurrentTime = 300f,
            Budget = 3
        };
    }

    private static List<MemoryData> BuildMemories()
    {
        return new List<MemoryData>
        {
            new()
            {
                Id = 1,
                Type = MemoryType.Observed,
                Category = MemoryCategory.Combat,
                Importance = 0.6f,
                Certainty = 0.9f,
                Timestamp = 0f,
                InvolvedEntityId = 0
            },
            new()
            {
                Id = 2,
                Type = MemoryType.Experienced,
                Category = MemoryCategory.Discovery,
                Importance = 0.9f,
                Certainty = 0.9f,
                Timestamp = 100f,
                InvolvedEntityId = 7
            },
            new()
            {
                Id = 3,
                Type = MemoryType.Learned,
                Category = MemoryCategory.Environmental,
                Importance = 0.5f,
                Certainty = 0.6f,
                Timestamp = 200f,
                InvolvedEntityId = 7
            },
            new()
            {
                Id = 4,
                Type = MemoryType.Observed,
                Category = MemoryCategory.Social,
                Importance = 0.8f,
                Certainty = 1f,
                Timestamp = 50f,
                InvolvedEntityId = 0
            }
        };
    }

    private static AffectState BuildAffect()
    {
        return new AffectState
        {
            Curiosity = 0.9f,
            Stress = 0.2f,
            Confidence = 0.6f,
            Trust = 0.6f,
            Novelty = 0.3f,
            Attachment = 0.3f,
            Threat = 0.8f,
            RewardExpectation = 0.5f,
            CognitiveLoad = 0.3f
        };
    }

    private static World CreateRetrievalWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new WorkingMemoryStore());
        world.AddResource(new MemoryStore());
        world.AddResource(new WorldModelState());
        world.AddResource(new GoalStore());
        world.AddResource(new CognitiveTraceLog());

        var time = world.GetResource<TimeResource>();
        time.SimulationTime = 300f;
        world.SetResource(time);

        var wm = world.GetResource<WorkingMemoryStore>();
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "c1",
            Content = "entity 7 present",
            SourceEntity = new EntityId(7),
            Salience = 0.9f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "c2",
            Content = "ambient context",
            SourceEntity = null,
            Salience = 0.4f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });

        var ltm = world.GetResource<MemoryStore>();
        foreach (var memory in BuildMemories())
            ltm.AddMemory(1, memory);

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(1, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 7 };

        world.SetResource(BuildAffect());

        return world;
    }

    private static (World world, Engine engine) CreatePipeline(IMemoryRetrievalStrategy retrievalStrategy)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new MemoryRetrievalSystem { Strategy = retrievalStrategy });
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem());
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
            Id = "c1",
            Content = "entity 7 present",
            SourceEntity = new EntityId(7),
            Salience = 0.9f,
            DecayRate = 0.08f,
            FormationTick = 0,
            LastAccessTick = 0
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "c2",
            Content = "ambient context",
            SourceEntity = null,
            Salience = 0.4f,
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

        var ltm = world.GetResource<MemoryStore>();
        foreach (var memory in BuildMemories())
            ltm.AddMemory(1, memory);

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(1, new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Urgency = 0.5f
        });

        var model = world.GetResource<WorldModelState>();
        model.KnownEntityIds = new List<uint> { 7 };

        world.SetResource(AffectState.Default);
    }

    private static string Serialize(RetrievalResult result)
    {
        return string.Join(";", result.Memories.Select(e => $"M:{e.Memory.Id}:{e.Score:F6}").Concat(
            result.Evidence.Select(e =>
                $"E:{e.MemoryId}:{e.Operation}:{e.ImportanceScore:F6}:{e.RecencyScore:F6}:{e.ContextOverlapScore:F6}:{e.AttentionRelevanceScore:F6}:{e.FinalScore:F6}:{e.Strategy}")));
    }

    private static string HashCandidates(List<MemoryData> memories)
    {
        return string.Join("|", memories.OrderBy(m => m.Id).Select(m =>
            $"{m.Id}:{m.Type}:{m.Category}:{m.Importance:F6}:{m.Certainty:F6}:{m.Timestamp:F6}:{m.InvolvedEntityId}:{m.Forgotten}"));
    }

    private static string HashWorkingMemory(WorkingMemoryStore wm)
    {
        return string.Join("|", wm.Chunks.Select(c =>
            $"{c.Id}:{c.Content}:{c.Salience:F4}:{c.SourceEntity?.Value.ToString() ?? "null"}"));
    }

    private static string HashRetrievedChunks(WorkingMemoryStore wm)
    {
        return string.Join("|", wm.Chunks
            .Where(c => c.Id.StartsWith("retrieved_"))
            .OrderBy(c => c.Id)
            .Select(c => $"{c.Id}:{c.Content}:{c.Salience:F6}"));
    }

    private static string HashStaticChunks(WorkingMemoryStore wm)
    {
        return string.Join("|", wm.Chunks
            .Where(c => !c.Id.StartsWith("retrieved_") && !c.SourceEntity.HasValue)
            .OrderBy(c => c.Id)
            .Select(c => $"{c.Id}:{c.Content}:{c.Salience:F6}"));
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
}
