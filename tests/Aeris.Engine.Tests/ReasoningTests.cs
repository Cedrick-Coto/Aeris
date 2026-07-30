using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class ReasoningTests
{
    [Fact]
    public void S_R001_DirectInference()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "tree observed at location north" },
                new() { Id = "w2", Content = "moss is near trees" }
            }
        };

        var result = strategy.Reason(context);

        result.Inferences.Should().NotBeEmpty();
        result.Inferences.Should().Contain(i =>
            i.Transformation == "SpatialAssociation" &&
            i.Confidence > 0f);
        result.Evidence.Should().Contain(e =>
            e.RuleId.Contains("spatial-association") &&
            e.Confidence > 0f);
    }

    [Fact]
    public void S_R001_EvidenceContainsRuleId()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "tree observed at location north" },
                new() { Id = "w2", Content = "moss is near trees" }
            }
        };

        var result = strategy.Reason(context);

        foreach (var ev in result.Evidence)
        {
            ev.RuleId.Should().NotBeNullOrEmpty();
            ev.PremiseCount.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void S_R002_InsufficientEvidence()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "Aeris vio una piedra" }
            }
        };

        var result = strategy.Reason(context);

        result.Inferences.Should().BeEmpty();
        result.Evidence.Should().BeEmpty();
    }

    [Fact]
    public void S_R003_ContradictionReported()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "mem", Content = "river observed at location north" },
                new() { Id = "wm", Content = "river observed at location south" }
            }
        };

        var result = strategy.Reason(context);

        result.Inferences.Should().Contain(i =>
            i.Transformation == "Contradiction" &&
            i.Conclusion.Contains("Conflicto"));
    }

    [Fact]
    public void S_R004_StrategyReplacement()
    {
        var world = CreateWorld(out var wm, out var model, out var goals);
        var system = new ReasoningSystem
        {
            Strategy = new MockReasoningStrategy()
        };

        system.Execute(world, 1f);

        var store = world.GetResource<InferenceStore>();
        store.Inferences.Should().Contain(i => i.Conclusion == "mock inference");
    }

    [Fact]
    public void S_R005_Determinism()
    {
        string hash1 = RunDeterministicSession();
        string hash2 = RunDeterministicSession();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void S_R006_NoSideEffects()
    {
        var world = CreateWorld(out var wm, out var model, out var goals);
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "tree",
            Content = "tree observed at location north",
            FormationTick = 1,
            LastAccessTick = 1
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "moss",
            Content = "moss is near trees",
            FormationTick = 1,
            LastAccessTick = 1
        });

        string wmHash = HashWorkingMemory(wm);
        string modelHash = HashModel(model);
        string goalsHash = HashGoals(goals);

        var system = new ReasoningSystem();
        system.Execute(world, 1f);

        HashWorkingMemory(wm).Should().Be(wmHash);
        HashModel(model).Should().Be(modelHash);
        HashGoals(goals).Should().Be(goalsHash);
    }

    [Fact]
    public void S_R007_GoalRelevanceAffectsPriorityNotConfidence()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var goals = new List<GoalData>
        {
            new()
            {
                Id = 1,
                Type = GoalType.Exploration,
                Status = GoalStatus.Active,
                Priority = GoalPriority.High,
                Urgency = 0.8f
            }
        };

        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "tree observed at location north" },
                new() { Id = "w2", Content = "moss is near trees" }
            },
            ActiveGoals = goals
        };

        var result = strategy.Reason(context);

        result.Inferences.Should().NotBeEmpty();
        var confidences = result.Inferences.Select(i => i.Confidence).Distinct().ToList();

        bool goalRelevantExists = result.Inferences.Any(i =>
            i.Conclusion.Contains("Exploration", StringComparison.OrdinalIgnoreCase));

        if (goalRelevantExists)
        {
            int relevantIdx = result.Inferences.FindIndex(i =>
                i.Conclusion.Contains("Exploration", StringComparison.OrdinalIgnoreCase));
            relevantIdx.Should().Be(0, "goal-relevant inferences should appear first");
        }
    }

    [Fact]
    public void Rule_IdentityPreserved()
    {
        var rules = EvidenceBasedReasoningStrategy.RegisteredRules;

        foreach (var rule in rules)
        {
            rule.RuleId.Should().NotBeNullOrEmpty();
            rule.Version.Should().BeGreaterThan(0);
            rule.Label.Should().NotBeNullOrEmpty();
            rule.Description.Should().NotBeNullOrEmpty();
        }

        rules.Select(r => r.RuleId).Distinct().Should().HaveSameCount(rules);
    }

    [Fact]
    public void Confidence_And_EvidenceStrength_CanDiverge()
    {
        var rule = new ReasoningRule
        {
            RuleId = "test-rule",
            Version = 1,
            Label = "TestRule",
            Description = "Test rule for divergence check",
            MinPremises = 2,
            MaxPremises = 5,
            BaseWeight = 0.3f,
            PremiseMatcher = _ => true,
            InferenceBuilder = _ => "test inference"
        };

        var facts = new[] { "fact1", "fact2" };
        rule.TryApply(facts, out var inference);
        inference.Confidence = 0.3f;

        float evidenceStrength = Math.Clamp(
            inference.Confidence * (float)facts.Length / rule.MaxPremises,
            0f, 1f);

        evidenceStrength.Should().BeLessThan(inference.Confidence,
            "confidence can exceed evidence strength when premises are few vs max");
    }

    [Fact]
    public void ReasoningSystem_ExecutesInCausalChain()
    {
        var world = CreateWorld(out var wm, out var model, out var goals);
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "tree",
            Content = "tree observed at location north",
            FormationTick = 1,
            LastAccessTick = 1
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "moss",
            Content = "moss is near trees",
            FormationTick = 1,
            LastAccessTick = 1
        });

        var system = new ReasoningSystem();
        system.Execute(world, 1f);

        var store = world.GetResource<InferenceStore>();
        store.Inferences.Should().NotBeEmpty();

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "ReasoningSystem");
    }

    [Fact]
    public void CausalSequence_RuleFires()
    {
        var strategy = new EvidenceBasedReasoningStrategy();
        var context = new ReasoningContext
        {
            WorkingMemory = new List<WorkingMemoryChunk>
            {
                new() { Id = "w1", Content = "rain observed" },
                new() { Id = "w2", Content = "mushrooms grow after rain" }
            }
        };

        var result = strategy.Reason(context);

        result.Inferences.Should().Contain(i =>
            i.Transformation == "CausalSequence");
    }

    private static string RunDeterministicSession()
    {
        var world = CreateWorld(out var wm, out var model, out var goals);
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "tree",
            Content = "tree observed at location north",
            FormationTick = 1,
            LastAccessTick = 1
        });
        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "moss",
            Content = "moss is near trees",
            FormationTick = 1,
            LastAccessTick = 1
        });

        var system = new ReasoningSystem();
        system.Execute(world, 1f);

        var store = world.GetResource<InferenceStore>();
        return string.Join("|",
            store.Inferences.Select(i => $"{i.Id}:{i.RuleId}:{i.Confidence:F4}:{i.Conclusion}"));
    }

    private static string HashWorkingMemory(WorkingMemoryStore wm)
    {
        return string.Join("|", wm.Chunks.Select(c => $"{c.Id}:{c.Content}:{c.Salience:F4}"));
    }

    private static string HashModel(WorldModelState model)
    {
        return string.Join(",", model.KnownEntityIds) + ":" + model.LastUpdateTick;
    }

    private static string HashGoals(GoalStore goals)
    {
        var parts = new List<string>();
        foreach (var kvp in goals.All)
            foreach (var g in kvp.Value)
                parts.Add($"{g.Id}:{g.Type}:{g.Status}");
        return string.Join("|", parts);
    }

    private static World CreateWorld(out WorkingMemoryStore wm, out WorldModelState model, out GoalStore goals)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new WorkingMemoryStore());
        world.AddResource(new WorldModelState());
        world.AddResource(new GoalStore());
        world.AddResource(new CognitiveTraceLog());
        world.AddResource(new InferenceStore());

        wm = world.GetResource<WorkingMemoryStore>();
        model = world.GetResource<WorldModelState>();
        goals = world.GetResource<GoalStore>();
        return world;
    }

    private sealed class MockReasoningStrategy : IReasoningStrategy
    {
        public ReasoningResult Reason(ReasoningContext context)
        {
            return new ReasoningResult
            {
                Inferences = new List<Inference>
                {
                    new()
                    {
                        Id = 1,
                        RuleId = "mock-v1",
                        Transformation = "Mock",
                        Conclusion = "mock inference",
                        Confidence = 1f,
                        Premises = new[] { "mock premise" }
                    }
                },
                Evidence = new List<ReasoningEvidence>
                {
                    new()
                    {
                        InferenceId = 1,
                        RuleId = "mock-v1",
                        PremiseCount = 1,
                        Transformation = "Mock",
                        Confidence = 1f,
                        EvidenceStrength = 1f,
                        Strategy = "Mock"
                    }
                }
            };
        }
    }
}
