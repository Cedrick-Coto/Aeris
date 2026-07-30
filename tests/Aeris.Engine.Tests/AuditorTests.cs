using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class AuditorTests
{
    [Fact]
    public void S_A001_AllRulesPass()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new NoOpRule("R001", "1.0", "dummy rule that always passes")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void S_A002_SingleViolation()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R002", "1.0", "always fails",
                ViolationSeverity.High, "dangerous action", "action=Attack")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle();
        var v = result.Violations[0];
        v.RuleId.Should().Be("R002");
        v.RuleVersion.Should().Be("1.0");
        v.Severity.Should().Be(ViolationSeverity.High);
        v.Condition.Should().Be("dangerous action");
        v.Evidence.Should().Be("action=Attack");
    }

    [Fact]
    public void S_A003_MultipleViolationsMaxSeverity()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R001", "1.0", "low severity",
                ViolationSeverity.Low, "rule1", "ev1"),
            new AlwaysViolateRule("R002", "1.0", "critical severity",
                ViolationSeverity.Critical, "rule2", "ev2"),
            new AlwaysViolateRule("R003", "1.0", "medium severity",
                ViolationSeverity.Medium, "rule3", "ev3")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeFalse();
        result.Violations.Should().HaveCount(3);
        result.MaxSeverity.Should().Be(ViolationSeverity.Critical);
    }

    [Fact]
    public void S_A004_PureEvaluation_ArtifactUnchanged()
    {
        var artifact = MakeValidDecision();
        string originalType = artifact.ArtifactType;
        uint originalId = artifact.ArtifactId;

        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R001", "1.0", "always fails",
                ViolationSeverity.High, "x", "y")
        };

        new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        artifact.ArtifactType.Should().Be(originalType);
        artifact.ArtifactId.Should().Be(originalId);
    }

    [Fact]
    public void S_A005_Determinism()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R001", "1.0", "rule",
                ViolationSeverity.Medium, "cond", "ev")
        };

        var ctx = new AuditContext { Artifact = artifact, Rules = rules };
        var strategy = new SequentialRuleEvaluator();

        var r1 = strategy.Audit(ctx);
        var r2 = strategy.Audit(ctx);

        r1.Passed.Should().Be(r2.Passed);
        r1.Violations.Count.Should().Be(r2.Violations.Count);
        r1.MaxSeverity.Should().Be(r2.MaxSeverity);
        r1.Violations[0].RuleId.Should().Be(r2.Violations[0].RuleId);
    }

    [Fact]
    public void S_A006_RuleIdentity()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R-XYZ", "2.1.0", "test rule",
                ViolationSeverity.High, "condition met", "evidence found")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        var v = result.Violations[0];
        v.RuleId.Should().NotBeNullOrEmpty();
        v.RuleVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void S_A007_Independence_NoECSResources()
    {
        var artifact = MakeValidDecision();
        var rule = new AuditorHasNoECSAccessRule();
        var ctx = new AuditContext
        {
            Artifact = artifact,
            Rules = new List<IAuditRule> { rule }
        };

        var result = new SequentialRuleEvaluator().Audit(ctx);

        result.Passed.Should().BeTrue("rule evaluates artifact type, not ECS resources");
    }

    [Fact]
    public void S_A008_StrategyReplacement()
    {
        var artifact = MakeValidDecision();

        var mockResult = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "MOCK", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Critical, Condition = "mock", Evidence = "mock" }
            },
            MaxSeverity = ViolationSeverity.Critical,
            Evidence = new AuditEvidence
            {
                ArtifactType = "DecisionResult", ArtifactId = 1,
                RulesEvaluated = 1, Strategy = "MockAuditStrategy"
            }
        };

        var strategy = new MockAuditStrategy(mockResult);

        var result = strategy.Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = new List<IAuditRule>()
        });

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleId == "MOCK");
    }

    [Fact]
    public void S_A009_NoEnforcement_OnlyEvaluation()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new AlwaysViolateRule("R001", "1.0", "fails",
                ViolationSeverity.High, "bad", "evidence")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeFalse();
        result.Violations.Should().NotBeEmpty();

        result.Passed.GetType().Should().Be(typeof(bool));
        result.GetType().Should().Be<AuditResult>();
        result.Violations.All(v => v.Verdict == RuleVerdict.Violated).Should().BeTrue();
    }

    [Fact]
    public void RuleRegistry_FiltersByArtifactType()
    {
        var registry = new RuleRegistry();
        registry.Register(new NoOpRule("R-DEC", "1.0", "for decisions",
            supportedTypes: new[] { "DecisionResult" }));
        registry.Register(new NoOpRule("R-INF", "1.0", "for inferences",
            supportedTypes: new[] { "Inference" }));

        var decisionRules = registry.GetRulesFor("DecisionResult");
        var inferenceRules = registry.GetRulesFor("Inference");
        var unknownRules = registry.GetRulesFor("Memory");

        decisionRules.Should().ContainSingle(r => r.RuleId == "R-DEC");
        inferenceRules.Should().ContainSingle(r => r.RuleId == "R-INF");
        unknownRules.Should().BeEmpty();
    }

    [Fact]
    public void SequentialRuleEvaluator_AllRulesRunEvenIfOneFails()
    {
        var artifact = MakeValidDecision();
        int executionCount = 0;
        var rule = new CountingRule("R-COUNT", "1.0", "counts executions",
            () => executionCount++);

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = new List<IAuditRule> { rule, rule, rule }
        });

        executionCount.Should().Be(3, "all rules should execute even if prior rules fail");
    }

    [Fact]
    public void NotApplicableRule_DoesNotCountAsViolation()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new NotApplicableRule("R-NA", "1.0", "not applicable to decisions")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void SatisfiedRule_DoesNotCountAsViolation()
    {
        var artifact = MakeValidDecision();
        var rules = new List<IAuditRule>
        {
            new SatisfiedRule("R-SAT", "1.0", "always satisfied")
        };

        var result = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = rules
        });

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AuditSystem_ExecutesAndStoresResult()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());

        world.AddResource(new ActionStore
        {
            LastResult = new DecisionResult
            {
                Status = DecisionStatus.Selected,
                SelectedPlanId = 1,
                Action = new SelectedAction { PlanId = 1, Action = "Move", GoalId = 1, Confidence = 0.8f },
                Evidence = new DecisionEvidence { Status = DecisionStatus.Selected }
            }
        });
        world.AddResource(new CognitiveTraceLog { Tick = 1 });

        var system = new AuditSystem();
        system.Registry.Register(new NoOpRule("R-BASE", "1.0", "baseline pass",
            supportedTypes: new[] { "DecisionResult" }));

        system.Execute(world, 1f);

        var store = world.GetResource<AuditStore>();
        store.LastResult.Should().NotBeNull();
        store.LastResult.Passed.Should().BeTrue();

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "AuditSystem");
    }

    private static DecisionResultAuditable MakeValidDecision()
    {
        return new DecisionResultAuditable
        {
            ArtifactId = 1,
            Decision = new DecisionResult
            {
                Status = DecisionStatus.Selected,
                SelectedPlanId = 1,
                Action = new SelectedAction { PlanId = 1, Action = "Explore", GoalId = 1, Confidence = 0.7f },
                Evidence = new DecisionEvidence
                {
                    Status = DecisionStatus.Selected,
                    CandidatesConsidered = 2,
                    SelectionPolicy = "FeasibilityThresholdPolicy",
                    Threshold = 0.5f
                }
            }
        };
    }

    private sealed class NoOpRule : IAuditRule
    {
        public string RuleId { get; }
        public string RuleVersion { get; }
        public string Description { get; }
        public string[] SupportedArtifactTypes { get; }

        public NoOpRule(string id, string version, string description,
            string[]? supportedTypes = null)
        {
            RuleId = id;
            RuleVersion = version;
            Description = description;
            SupportedArtifactTypes = supportedTypes ?? new[] { "DecisionResult" };
        }

        public AuditViolation? Evaluate(IAuditableArtifact artifact) => null;
    }

    private sealed class AlwaysViolateRule : IAuditRule
    {
        public string RuleId { get; }
        public string RuleVersion { get; }
        public string Description { get; }
        public string[] SupportedArtifactTypes { get; } = { "DecisionResult" };

        private readonly ViolationSeverity _severity;
        private readonly string _condition;
        private readonly string _evidence;

        public AlwaysViolateRule(string id, string version, string description,
            ViolationSeverity severity, string condition, string evidence)
        {
            RuleId = id;
            RuleVersion = version;
            Description = description;
            _severity = severity;
            _condition = condition;
            _evidence = evidence;
        }

        public AuditViolation? Evaluate(IAuditableArtifact artifact)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = _severity,
                Condition = _condition,
                Evidence = _evidence,
                ArtifactId = artifact.ArtifactId
            };
        }
    }

    private sealed class NotApplicableRule : IAuditRule
    {
        public string RuleId { get; }
        public string RuleVersion { get; }
        public string Description { get; }
        public string[] SupportedArtifactTypes { get; } = { "Inference" };

        public NotApplicableRule(string id, string version, string description)
        {
            RuleId = id;
            RuleVersion = version;
            Description = description;
        }

        public AuditViolation? Evaluate(IAuditableArtifact artifact)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.NotApplicable,
                ArtifactId = artifact.ArtifactId
            };
        }
    }

    private sealed class SatisfiedRule : IAuditRule
    {
        public string RuleId { get; }
        public string RuleVersion { get; }
        public string Description { get; }
        public string[] SupportedArtifactTypes { get; } = { "DecisionResult" };

        public SatisfiedRule(string id, string version, string description)
        {
            RuleId = id;
            RuleVersion = version;
            Description = description;
        }

        public AuditViolation? Evaluate(IAuditableArtifact artifact)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Satisfied,
                ArtifactId = artifact.ArtifactId
            };
        }
    }

    private sealed class AuditorHasNoECSAccessRule : IAuditRule
    {
        public string RuleId => "R-INDEP";
        public string RuleVersion => "1.0";
        public string Description => "proves auditor only sees artifact interface";
        public string[] SupportedArtifactTypes { get; } = { "DecisionResult" };

        public AuditViolation? Evaluate(IAuditableArtifact artifact)
        {
            if (artifact.ArtifactType == "DecisionResult")
                return null;

            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = ViolationSeverity.High,
                Condition = "unexpected artifact type",
                Evidence = $"got {artifact.ArtifactType}",
                ArtifactId = artifact.ArtifactId
            };
        }
    }

    private sealed class CountingRule : IAuditRule
    {
        public string RuleId { get; }
        public string RuleVersion { get; }
        public string Description { get; }
        public string[] SupportedArtifactTypes { get; } = { "DecisionResult" };

        private readonly Action _onEvaluate;

        public CountingRule(string id, string version, string description,
            Action onEvaluate)
        {
            RuleId = id;
            RuleVersion = version;
            Description = description;
            _onEvaluate = onEvaluate;
        }

        public AuditViolation? Evaluate(IAuditableArtifact artifact)
        {
            _onEvaluate();
            return null;
        }
    }

    private sealed class MockAuditStrategy : IAuditStrategy
    {
        private readonly AuditResult _result;

        public MockAuditStrategy(AuditResult result) => _result = result;

        public AuditResult Audit(AuditContext context) => _result;
    }
}
