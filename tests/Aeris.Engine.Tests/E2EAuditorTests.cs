using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class E2EAuditorTests
{
    [Fact]
    public void A_E2E_001_ValidDecision_NoViolations()
    {
        var artifact = MakeDecision(action: "Move", confidence: 0.8f);
        var registry = new RuleRegistry();
        registry.Register(new SafetyRule());
        registry.Register(new ConsistencyRule());
        registry.Register(new PerformanceRule());

        var strategy = new SequentialRuleEvaluator();
        var result = strategy.Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = registry.GetRulesFor(artifact.ArtifactType)
        });

        result.Passed.Should().BeTrue("a valid decision with safe action should pass all rules");
        result.Violations.Should().BeEmpty();
        result.Evidence.RulesEvaluated.Should().Be(3);
        result.Evidence.RulesPassed.Should().Be(3);
        result.Evidence.RulesFailed.Should().Be(0);
    }

    [Fact]
    public void A_E2E_002_MultipleRules_OneViolation()
    {
        var artifact = MakeDecision(action: "Attack", confidence: 0.9f);
        var registry = new RuleRegistry();
        registry.Register(new SafetyRule());
        registry.Register(new ConsistencyRule());
        registry.Register(new PerformanceRule(minFeasibility: 0.3f, minConfidence: 0.2f));

        var strategy = new SequentialRuleEvaluator();
        var result = strategy.Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = registry.GetRulesFor(artifact.ArtifactType)
        });

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleId == "SAFETY-001",
            "Attack is a prohibited action");
        result.MaxSeverity.Should().Be(ViolationSeverity.Critical);
        result.Evidence.RulesEvaluated.Should().Be(3);
        result.Evidence.RulesFailed.Should().Be(1);
        result.Evidence.RulesPassed.Should().Be(2);
    }

    [Fact]
    public void A_E2E_003_NoApplicableRules_Passed()
    {
        var artifact = new DecisionResultAuditable
        {
            ArtifactId = 1,
            Decision = MakeDecision(action: "Move", confidence: 0.7f).Decision
        };

        var registry = new RuleRegistry();
        registry.Register(new SafetyRule());

        var strategy = new SequentialRuleEvaluator();
        var irrelevantContext = new AuditContext
        {
            Artifact = new NoOpArtifact(),
            Rules = registry.GetRulesFor("NoOpArtifact")
        };

        var result = strategy.Audit(irrelevantContext);

        result.Passed.Should().BeTrue(
            "when no rules are applicable, the result should pass (0 rules evaluated)");
        result.Evidence.RulesEvaluated.Should().Be(0);
    }

    [Fact]
    public void A_E2E_004_HundredRules_DeterminismAndOrder()
    {
        var artifact = MakeDecision(action: "Explore", confidence: 0.6f);
        var registry = new RuleRegistry();

        for (int i = 0; i < 50; i++)
            registry.Register(new PerformanceRule(minFeasibility: 0.1f, minConfidence: 0.1f));

        for (int i = 0; i < 50; i++)
            registry.Register(new ExperimentalRule());

        var strategy = new SequentialRuleEvaluator();
        var ctx = new AuditContext
        {
            Artifact = artifact,
            Rules = registry.GetRulesFor(artifact.ArtifactType)
        };

        ctx.Rules.Should().HaveCount(100);

        var result1 = strategy.Audit(ctx);
        var result2 = strategy.Audit(ctx);

        result1.Passed.Should().Be(result2.Passed);
        result1.Violations.Count.Should().Be(result2.Violations.Count);
        result1.MaxSeverity.Should().Be(result2.MaxSeverity);
        result1.Evidence.RulesEvaluated.Should().Be(100);
        result1.Evidence.RulesFailed.Should().Be(50,
            "50 ExperimentalRules flag Explore as violation (Low severity)");
        result1.Evidence.RulesPassed.Should().Be(50);

        for (int i = 0; i < result1.Violations.Count; i++)
        {
            result1.Violations[i].RuleId.Should().Be(result2.Violations[i].RuleId,
                $"violation {i} must have same RuleId across runs");
            result1.Violations[i].Severity.Should().Be(result2.Violations[i].Severity,
                $"violation {i} must have same Severity across runs");
        }
    }

    [Fact]
    public void DiverseRules_SameInterface_AllProduceAuditViolation()
    {
        var artifact = MakeDecision(action: "Attack", confidence: 0.2f);
        IAuditRule[] rules = {
            new SafetyRule(),
            new ConsistencyRule(),
            new ExperimentalRule(),
            new PerformanceRule()
        };

        var results = rules.Select(r =>
        {
            var v = r.Evaluate(artifact);
            return new { r.RuleId, r.RuleVersion, r.Description, Verdict = v?.Verdict };
        }).ToList();

        results.Should().HaveCount(4);
        results.All(r => !string.IsNullOrEmpty(r.RuleId)).Should().BeTrue();
        results.All(r => !string.IsNullOrEmpty(r.RuleVersion)).Should().BeTrue();
        results.All(r => !string.IsNullOrEmpty(r.Description)).Should().BeTrue();

        results.Should().Contain(r => r.RuleId == "SAFETY-001" && r.Verdict == RuleVerdict.Violated);
        results.Should().Contain(r => r.RuleId == "CONSISTENCY-001" && r.Verdict == RuleVerdict.Satisfied);
        results.Should().Contain(r => r.RuleId == "EXPERIMENTAL-001" && r.Verdict == RuleVerdict.Satisfied, "Attack is not an exploratory action");
        results.Should().Contain(r => r.RuleId == "PERF-001" && r.Verdict == RuleVerdict.Violated);
    }

    [Fact]
    public void Auditor_KnownsNoRuleImplementation()
    {
        var artifact = MakeDecision(action: "Move", confidence: 0.7f);
        var strategy = new SequentialRuleEvaluator();
        var registry = new RuleRegistry();
        registry.Register(new SafetyRule());
        registry.Register(new ConsistencyRule());
        registry.Register(new ExperimentalRule());
        registry.Register(new PerformanceRule());

        var context = new AuditContext
        {
            Artifact = artifact,
            Rules = registry.GetRulesFor(artifact.ArtifactType)
        };

        var result = strategy.Audit(context);

        result.Evidence.RulesEvaluated.Should().Be(4);
        result.Passed.Should().BeTrue();

        result.GetType().GetMethod("ApplyEnforcement").Should().BeNull(
            "Auditor should not have enforcement methods (A-005)");
        result.GetType().GetProperty("ActionOverride").Should().BeNull(
            "Auditor should not have action modification capabilities");
    }

    private static DecisionResultAuditable MakeDecision(string action, float confidence)
    {
        return new DecisionResultAuditable
        {
            ArtifactId = 1,
            Decision = new DecisionResult
            {
                Status = DecisionStatus.Selected,
                SelectedPlanId = 1,
                Action = new SelectedAction
                {
                    PlanId = 1,
                    Action = action,
                    GoalId = 1,
                    Confidence = confidence
                },
                Evidence = new DecisionEvidence
                {
                    Status = DecisionStatus.Selected,
                    CandidatesConsidered = 1,
                    SelectionPolicy = "FeasibilityThresholdPolicy",
                    Threshold = 0.5f
                }
            }
        };
    }

    private sealed class NoOpArtifact : IAuditableArtifact
    {
        public string ArtifactType => "NoOpArtifact";
        public uint ArtifactId => 0;
    }
}
