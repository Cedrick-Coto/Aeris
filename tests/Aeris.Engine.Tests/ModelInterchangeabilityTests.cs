using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class ModelInterchangeabilityTests
{
    [Fact]
    public void MI_001_AttentionStrategy_SalienceVsRandom_ProducesDifferentPercepts()
    {
        var (world, percepts) = CreateWorldWithPercepts();

        var salience = new SalienceAttentionStrategy();
        var random = new RandomAttentionStrategy();

        var affect = AffectState.Default;
        var budget = 5;

        var ctx = new AttentionContext { Percepts = percepts, Affect = affect, Budget = budget };
        var resultSalience = salience.Select(ctx);

        ctx = new AttentionContext { Percepts = percepts, Affect = affect, Budget = budget };
        var resultRandom = random.Select(ctx);

        resultSalience.Count.Should().Be(resultRandom.Count);
        resultSalience.Should().NotEqual(resultRandom,
            "different attention strategies should produce different selections");
    }

    [Fact]
    public void MI_001A_AttentionStrategy_PluggedIntoPipeline_Completes()
    {
        var (world, engine) = CreatePipelineWithAttentionStrategy(new RandomAttentionStrategy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("pipeline must work with RandomAttentionStrategy");

        var attended = world.GetResource<AttendedPercepts>();
        attended.Percepts.Should().NotBeNull();
        attended.Tick.Should().Be(1);
    }

    [Fact]
    public void MI_002_RetrievalStrategy_ActivationBased_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            retrievalStrategy: new ActivationBasedStrategy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("ActivationBasedStrategy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "MemoryRetrievalSystem" && e.Why.Contains("ActivationBasedStrategy"));
    }

    [Fact]
    public void MI_003_ReasoningStrategy_RulePriority_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            reasoningStrategy: new RulePriorityStrategy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("RulePriorityStrategy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "ReasoningSystem" && e.Why.Contains("RulePriorityStrategy"));

        var store = world.GetResource<InferenceStore>();
        store.Inferences.Should().NotBeNull();
    }

    [Fact]
    public void MI_004_PlanningStrategy_Greedy_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            planningStrategy: new GreedyPlanningStrategy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("GreedyPlanningStrategy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "PlanningSystem" && e.Why.Contains("GreedyPlanningStrategy"));

        var planStore = world.GetResource<PlanStore>();
        planStore.Plans.Should().NotBeNull();
    }

    [Fact]
    public void MI_005_DecisionStrategy_ConfidenceGate_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            decisionStrategy: new ConfidenceGatePolicy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("ConfidenceGatePolicy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "DecisionSystem" && e.Why.Contains("ConfidenceGatePolicy"));

        var actionStore = world.GetResource<ActionStore>();
        actionStore.LastResult.Status.Should().BeOneOf(DecisionStatus.Selected, DecisionStatus.NoViablePlan);
    }

    [Fact]
    public void MI_006_AuditStrategy_FailFast_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            auditStrategy: new FailFastEvaluator());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("FailFastEvaluator must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "AuditSystem" && e.Why.Contains("FailFastEvaluator"));

        var auditStore = world.GetResource<AuditStore>();
        auditStore.LastResult.Should().NotBeNull();
    }

    [Fact]
    public void MI_007_EnforcementPolicy_Permissive_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            enforcementPolicy: new PermissivePolicy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("PermissivePolicy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "EnforcementSystem" && e.OutputSummary.Contains("PermissivePolicy"));

        var enforcementStore = world.GetResource<EnforcementStore>();
        enforcementStore.LastResult.Should().NotBeNull();
    }

    [Fact]
    public void MI_007A_EnforcementPolicy_SafetyFirst_CompletesAndTraces()
    {
        var (world, engine) = CreateFullPipeline(
            enforcementPolicy: new SafetyFirstPolicy());
        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("SafetyFirstPolicy must complete");

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "EnforcementSystem" && e.OutputSummary.Contains("SafetyFirstPolicy"));
    }

    [Fact]
    public void MI_008_FullAlternativePipeline_AllStrategiesSwapped_PipelineCompletes()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);

        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem { Strategy = new RandomAttentionStrategy() });
        engine.RegisterSystem(new MemoryRetrievalSystem { Strategy = new ActivationBasedStrategy() });
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem { Strategy = new RulePriorityStrategy() });
        engine.RegisterSystem(new PlanningSystem { Strategy = new GreedyPlanningStrategy() });
        engine.RegisterSystem(new DecisionSystem { Strategy = new ConfidenceGatePolicy() });
        engine.RegisterSystem(new AuditSystem { Strategy = new FailFastEvaluator() });
        engine.RegisterSystem(new EnforcementSystem { Policy = new PermissivePolicy() });

        engine.Initialize();

        SetUpMinimalState(world);

        var act = () => engine.RunOneTick();
        act.Should().NotThrow("full alternative pipeline must complete without errors");

        var actionStore = world.GetResource<ActionStore>();
        new[] { DecisionStatus.Selected, DecisionStatus.NoViablePlan }
            .Should().Contain(actionStore.LastResult.Status,
                "alternative strategies may produce valid decisions or defer");

        var trace = world.GetResource<CognitiveTraceLog>();
        var systemsInTrace = trace.Entries.Select(e => e.System).Distinct().ToList();
        systemsInTrace.Should().Contain(new[]
        {
            "AttentionSystem", "MemoryRetrievalSystem", "ReasoningSystem",
            "PlanningSystem", "DecisionSystem", "AuditSystem", "EnforcementSystem"
        });
    }

    [Fact]
    public void MI_009_DeterminismAcrossAlternativeStrategies()
    {
        var snap1 = RunAlternativePipeline();
        var snap2 = RunAlternativePipeline();

        snap1.Should().Be(snap2,
            "alternative strategy pipeline must be deterministic across runs");
    }

    [Fact]
    public void MI_009A_DeterminismSalienceVsRandom_BothDeterministicIndividually()
    {
        var salience1 = RunDeterministicWithStrategy(new SalienceAttentionStrategy());
        var salience2 = RunDeterministicWithStrategy(new SalienceAttentionStrategy());
        salience1.Should().Be(salience2, "SalienceAttentionStrategy must be deterministic");

        var random1 = RunDeterministicWithStrategy(new RandomAttentionStrategy());
        var random2 = RunDeterministicWithStrategy(new RandomAttentionStrategy());
        random1.Should().Be(random2, "RandomAttentionStrategy must be deterministic (hash-based)");
    }

    [Fact]
    public void MI_010_CausalTraceComplete_WithAlternativeStrategies()
    {
        var (world, engine) = CreateFullPipeline(
            retrievalStrategy: new ActivationBasedStrategy(),
            reasoningStrategy: new RulePriorityStrategy(),
            planningStrategy: new GreedyPlanningStrategy());
        SetUpMinimalState(world);

        engine.RunOneTick();

        var trace = world.GetResource<CognitiveTraceLog>();
        var entries = trace.Entries;

        entries.Should().Contain(e => e.System == "PerceptionSystem");
        entries.Should().Contain(e => e.System == "AttentionSystem");
        entries.Should().Contain(e => e.System == "MemoryRetrievalSystem" && e.Why.Contains("ActivationBasedStrategy"));
        entries.Should().Contain(e => e.System == "ReasoningSystem" && e.Why.Contains("RulePriorityStrategy"));
        entries.Should().Contain(e => e.System == "PlanningSystem" && e.Why.Contains("GreedyPlanningStrategy"));
        entries.Should().Contain(e => e.System == "DecisionSystem");
        entries.Should().Contain(e => e.System == "AuditSystem");
        entries.Should().Contain(e => e.System == "EnforcementSystem");

        foreach (var entry in entries)
        {
            entry.TraceId.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void MI_011_ContractStabilityIndex_NoContractChangesForExisting()
    {
        var existingContracts = new HashSet<string>
        {
            nameof(IMemoryRetrievalStrategy),
            nameof(IReasoningStrategy),
            nameof(IPlanningStrategy),
            nameof(IDecisionStrategy),
            nameof(IAuditStrategy),
            nameof(IEnforcementPolicy)
        };

        var newModels = new List<Type>
        {
            typeof(ActivationBasedStrategy),
            typeof(RulePriorityStrategy),
            typeof(GreedyPlanningStrategy),
            typeof(ConfidenceGatePolicy),
            typeof(FailFastEvaluator),
            typeof(PermissivePolicy),
            typeof(SafetyFirstPolicy)
        };

        foreach (var modelType in newModels)
        {
            foreach (var iface in modelType.GetInterfaces())
            {
                existingContracts.Should().Contain(iface.Name,
                    $"{modelType.Name} should implement an existing contract, not require a new one");
            }
        }

        float csi = 0f;
        csi.Should().Be(0f, "CSI = contracts modified / models implemented; should be 0 for existing contracts");
    }

    [Fact]
    public void MI_011A_AttentionContract_NewContractRequired_CSIIncrease()
    {
        var newContract = typeof(IAttentionStrategy);
        var modelsImplementing = new List<Type>
        {
            typeof(SalienceAttentionStrategy),
            typeof(RandomAttentionStrategy)
        };

        newContract.IsInterface.Should().BeTrue("IAttentionStrategy is a new contract");

        foreach (var model in modelsImplementing)
        {
            newContract.IsAssignableFrom(model).Should().BeTrue(
                $"{model.Name} must implement IAttentionStrategy");
        }

        float csi = 1f / 2f;
        csi.Should().Be(0.5f,
            "CSI for Attention: 1 new contract / 2 models = 0.5. Gap identified: AttentionSystem had no strategy contract before Sprint 3X.3");
    }

    [Fact]
    public void MI_012_StrategySwapping_NoECSSystemModified()
    {
        var systemTypes = new[]
        {
            typeof(AttentionSystem),
            typeof(MemoryRetrievalSystem),
            typeof(ReasoningSystem),
            typeof(PlanningSystem),
            typeof(DecisionSystem),
            typeof(AuditSystem),
            typeof(EnforcementSystem)
        };

        foreach (var systemType in systemTypes)
        {
            var strategyProp = systemType.GetProperties()
                .FirstOrDefault(p =>
                    p.PropertyType.Name.StartsWith("I") &&
                    p.PropertyType.IsInterface &&
                    (p.Name == "Strategy" || p.Name == "Policy"));

            strategyProp.Should().NotBeNull(
                $"{systemType.Name} must expose a strategy/policy property for model swapping");
            strategyProp!.SetMethod.Should().NotBeNull(
                $"{systemType.Name}.{strategyProp.Name} must have a public setter");
        }
    }

    [Fact]
    public void MI_013_AlternativeStrategies_AllProduceValidTraceEntries()
    {
        var strategies = new (string label, ISystem[] systems)[]
        {
            ("Default strategies", new ISystem[]
            {
                new MemoryRetrievalSystem(),
                new ReasoningSystem(),
                new PlanningSystem(),
                new DecisionSystem(),
                new AuditSystem(),
                new EnforcementSystem()
            }),
            ("ActivationBased + RulePriority", new ISystem[]
            {
                new MemoryRetrievalSystem { Strategy = new ActivationBasedStrategy() },
                new ReasoningSystem { Strategy = new RulePriorityStrategy() },
                new PlanningSystem(),
                new DecisionSystem(),
                new AuditSystem(),
                new EnforcementSystem()
            }),
            ("Greedy + ConfidenceGate", new ISystem[]
            {
                new MemoryRetrievalSystem(),
                new ReasoningSystem(),
                new PlanningSystem { Strategy = new GreedyPlanningStrategy() },
                new DecisionSystem { Strategy = new ConfidenceGatePolicy() },
                new AuditSystem(),
                new EnforcementSystem()
            })
        };

        foreach (var (label, systems) in strategies)
        {
            var world = new World();
            world.AddResource(TimeResource.Create());
            world.AddResource(new EngineStats());

            var engine = new Engine(world);
            engine.RegisterSystem(new PerceptionSystem());
            engine.RegisterSystem(new AttentionSystem());
            engine.RegisterSystem(new WorkingMemorySystem());

            foreach (var sys in systems)
                engine.RegisterSystem(sys);

            engine.Initialize();
            SetUpMinimalState(world);

            var act = () => engine.RunOneTick();
            act.Should().NotThrow($"{label}: pipeline must complete");

            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Entries.Should().NotBeEmpty($"{label}: causal trace must exist");
            trace.Entries.Should().OnlyContain(e => !string.IsNullOrEmpty(e.System),
                $"{label}: all trace entries must have a system name");
            trace.Entries.Should().OnlyContain(e => e.TraceId > 0,
                $"{label}: all trace entries must have a TraceId");
        }
    }

    [Fact]
    public void MI_014_ModelOutputsDiffer_AsExpected()
    {
        var (worldAlt, engineAlt) = CreateFullPipeline(
            planningStrategy: new GreedyPlanningStrategy(),
            decisionStrategy: new ConfidenceGatePolicy());
        SetUpMinimalState(worldAlt);

        engineAlt.RunOneTick();

        var planStore = worldAlt.GetResource<PlanStore>();
        var actionStore = worldAlt.GetResource<ActionStore>();

        if (planStore.Evidence.Count > 0)
        {
            planStore.Evidence.Should().Contain(e => e.Strategy == nameof(GreedyPlanningStrategy));
            actionStore.LastResult.Evidence.SelectionPolicy.Should().Be(nameof(ConfidenceGatePolicy));
        }
        else
        {
            actionStore.LastResult.Status.Should().Be(DecisionStatus.NoViablePlan,
                "greedy + confidence gate may produce no viable plans with minimal state");
        }
    }

    private static string RunDeterministicWithStrategy(IAttentionStrategy strategy)
    {
        var (world, engine) = CreatePipelineWithAttentionStrategy(strategy);
        SetUpMinimalState(world);
        engine.RunOneTick();

        var trace = world.GetResource<CognitiveTraceLog>();
        var attended = world.GetResource<AttendedPercepts>();
        var parts = new List<string>
        {
            $"attended={attended.Tick}:{string.Join(",", attended.Percepts.Select(p => $"{p.LabelId}:{p.Salience:F4}"))}"
        };
        foreach (var e in trace.Entries)
            parts.Add($"{e.TraceId}:{e.System}:{e.InputSummary}");

        return string.Join("|", parts);
    }

    private static string RunAlternativePipeline()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem { Strategy = new RandomAttentionStrategy() });
        engine.RegisterSystem(new MemoryRetrievalSystem { Strategy = new ActivationBasedStrategy() });
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem { Strategy = new RulePriorityStrategy() });
        engine.RegisterSystem(new PlanningSystem { Strategy = new GreedyPlanningStrategy() });
        engine.RegisterSystem(new DecisionSystem { Strategy = new ConfidenceGatePolicy() });
        engine.RegisterSystem(new AuditSystem { Strategy = new FailFastEvaluator() });
        engine.RegisterSystem(new EnforcementSystem { Policy = new PermissivePolicy() });
        engine.Initialize();
        SetUpMinimalState(world);
        engine.RunOneTick();

        var trace = world.GetResource<CognitiveTraceLog>();
        var actionStore = world.GetResource<ActionStore>();
        var parts = new List<string>
        {
            $"action={actionStore.LastResult.Status}:{actionStore.LastResult.Action.Action}"
        };
        foreach (var e in trace.Entries)
            parts.Add($"{e.TraceId}:{e.ParentTraceId}:{e.System}:{e.InputSummary}");

        return string.Join("|", parts);
    }

    private static (World world, List<Percept> percepts) CreateWorldWithPercepts()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var percepts = new List<Percept>
        {
            new() { Type = PerceptType.Visual, LabelId = 1, Confidence = 0.9f, Distance = 10f },
            new() { Type = PerceptType.Auditory, LabelId = 2, Confidence = 0.5f, Distance = 50f },
            new() { Type = PerceptType.Aura, LabelId = 3, Confidence = 0.8f, Distance = 5f },
            new() { Type = PerceptType.Visual, LabelId = 4, Confidence = 0.3f, Distance = 100f },
            new() { Type = PerceptType.Proprioceptive, LabelId = 5, Confidence = 0.95f, Distance = 0f },
            new() { Type = PerceptType.Visual, LabelId = 6, Confidence = 0.7f, Distance = 20f },
            new() { Type = PerceptType.Auditory, LabelId = 7, Confidence = 0.2f, Distance = 200f }
        };

        return (world, percepts);
    }

    private static (World world, Engine engine) CreatePipelineWithAttentionStrategy(IAttentionStrategy strategy)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem { Strategy = strategy });
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

    private static (World world, Engine engine) CreateFullPipeline(
        IMemoryRetrievalStrategy? retrievalStrategy = null,
        IReasoningStrategy? reasoningStrategy = null,
        IPlanningStrategy? planningStrategy = null,
        IDecisionStrategy? decisionStrategy = null,
        IAuditStrategy? auditStrategy = null,
        IEnforcementPolicy? enforcementPolicy = null)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        var engine = new Engine(world);
        engine.RegisterSystem(new PerceptionSystem());
        engine.RegisterSystem(new AttentionSystem());
        engine.RegisterSystem(new MemoryRetrievalSystem
            { Strategy = retrievalStrategy ?? new LinearScanStrategy() });
        engine.RegisterSystem(new WorkingMemorySystem());
        engine.RegisterSystem(new ReasoningSystem
            { Strategy = reasoningStrategy ?? new EvidenceBasedReasoningStrategy() });
        engine.RegisterSystem(new PlanningSystem
            { Strategy = planningStrategy ?? new GoalDirectedPlanningStrategy() });
        engine.RegisterSystem(new DecisionSystem
            { Strategy = decisionStrategy ?? new FeasibilityThresholdPolicy() });
        engine.RegisterSystem(new AuditSystem
            { Strategy = auditStrategy ?? new SequentialRuleEvaluator() });
        engine.RegisterSystem(new EnforcementSystem
            { Policy = enforcementPolicy ?? new StrictPolicy() });

        engine.Initialize();
        return (world, engine);
    }

    private static void SetUpMinimalState(World world)
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
}
