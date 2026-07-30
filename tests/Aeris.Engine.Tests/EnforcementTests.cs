using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class EnforcementTests
{
    [Fact]
    public void S_E001_StrictPolicyApprovesWhenPassed()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>(),
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.Approve);
        result.Evidence.Reason.Should().Contain("All rules passed");
    }

    [Fact]
    public void S_E002_StrictPolicyRejectsHighSeverity()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Critical, Condition = "danger", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.Reject);
        result.Evidence.Reason.Should().Contain("Rejected");
    }

    [Fact]
    public void S_E003_StrictPolicyRequestsReplanningForLowSeverity()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Low, Condition = "minor", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.RequestReplanning);
        result.Evidence.Reason.Should().Contain("replanning");
    }

    [Fact]
    public void S_E004_PermissivePolicyApprovesLowSeverity()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Low, Condition = "minor", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new PermissivePolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.Approve);
        result.Evidence.Reason.Should().Contain("below threshold");
    }

    [Fact]
    public void S_E005_PolicyReplaceability()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Medium, Condition = "x", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var strictResult = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });
        var permissiveResult = new PermissivePolicy().Apply(new EnforcementContext { AuditResult = audit });

        strictResult.Verdict.Should().Be(EnforcementVerdict.RequestReplanning);
        permissiveResult.Verdict.Should().Be(EnforcementVerdict.Approve,
            "PermissivePolicy allows Medium severity");
    }

    [Fact]
    public void S_E006_Determinism()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.High, Condition = "x", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var policy = new StrictPolicy();
        var ctx = new EnforcementContext { AuditResult = audit };

        var r1 = policy.Apply(ctx);
        var r2 = policy.Apply(ctx);

        r1.Verdict.Should().Be(r2.Verdict);
        r1.Evidence.Reason.Should().Be(r2.Evidence.Reason);
    }

    [Fact]
    public void S_E007_AuditResultUnchanged()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "R001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.High, Condition = "x", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        int originalViolationCount = audit.Violations.Count;
        ViolationSeverity? originalMax = audit.MaxSeverity;

        new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        audit.Violations.Count.Should().Be(originalViolationCount);
        audit.MaxSeverity.Should().Be(originalMax);
    }

    [Fact]
    public void E_001_NoAuditResultMutation()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>(),
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var ctx = new EnforcementContext { AuditResult = audit };
        new StrictPolicy().Apply(ctx);

        ctx.AuditResult.Violations.Count.Should().Be(0);
        shouldNotHaveMutated(ctx.AuditResult);
    }

    [Fact]
    public void E_002_WorldIsolation()
    {
        var policy = new StrictPolicy();
        var ctx = new EnforcementContext
        {
            AuditResult = new AuditResult
            {
                Violations = new List<AuditViolation>(),
                Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
            }
        };

        var result = policy.Apply(ctx);

        result.Verdict.Should().Be(EnforcementVerdict.Approve);
        result.GetType().GetField("_world").Should().BeNull(
            "Enforcement should not hold world references");
    }

    [Fact]
    public void E_004_NoNewViolations()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "EXISTING", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Medium, Condition = "x", Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.RequestReplanning);
        result.Evidence.ViolationCount.Should().Be(1,
            "Enforcement must not invent new violations (E-004)");
    }

    [Fact]
    public void E_005_PolicyReplaceableViaInterface()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>(),
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        IEnforcementPolicy[] policies =
        {
            new StrictPolicy(),
            new PermissivePolicy(),
            new SafetyFirstPolicy()
        };

        foreach (var policy in policies)
        {
            var result = policy.Apply(new EnforcementContext { AuditResult = audit });

            result.Verdict.Should().Be(EnforcementVerdict.Approve,
                $"Policy {policy.Name} should approve a clean audit");
            result.GetType().Should().Be<EnforcementResult>();
        }
    }

    [Fact]
    public void SafetyFirstPolicy_RejectsSafetyViolation()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "SAFETY-001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Critical, Condition = "Attack prohibited",
                    Evidence = "action=Attack", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new SafetyFirstPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.Reject);
        result.Evidence.Reason.Should().Contain("safety");
    }

    [Fact]
    public void SafetyFirstPolicy_IgnoresNonSafetyViolationsForReject()
    {
        var audit = new AuditResult
        {
            Violations = new List<AuditViolation>
            {
                new() { RuleId = "EXPERIMENTAL-001", RuleVersion = "1.0", Verdict = RuleVerdict.Violated,
                    Severity = ViolationSeverity.Low, Condition = "exploration flagged",
                    Evidence = "x", ArtifactId = 1 }
            },
            Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
        };

        var result = new SafetyFirstPolicy().Apply(new EnforcementContext { AuditResult = audit });

        result.Verdict.Should().Be(EnforcementVerdict.RequestReplanning,
            "non-safety violations should trigger replanning, not rejection");
    }

    [Fact]
    public void EnforcementSystem_ExecutesAndStoresResult()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new AuditStore
        {
            LastResult = new AuditResult
            {
                Violations = new List<AuditViolation>(),
                Evidence = new AuditEvidence { ArtifactType = "DecisionResult", ArtifactId = 1 }
            }
        });
        world.AddResource(new CognitiveTraceLog { Tick = 1 });

        var system = new EnforcementSystem();
        system.Execute(world, 1f);

        var store = world.GetResource<EnforcementStore>();
        store.LastResult.Should().NotBeNull();
        store.LastResult.Verdict.Should().Be(EnforcementVerdict.Approve);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "EnforcementSystem");
    }

    [Fact]
    public void FullChain_AuditThenEnforcement()
    {
        var artifact = new DecisionResultAuditable
        {
            ArtifactId = 1,
            Decision = new DecisionResult
            {
                Status = DecisionStatus.Selected,
                SelectedPlanId = 1,
                Action = new SelectedAction { PlanId = 1, Action = "Attack", GoalId = 1, Confidence = 0.9f },
                Evidence = new DecisionEvidence { Status = DecisionStatus.Selected }
            }
        };

        var audit = new SequentialRuleEvaluator().Audit(new AuditContext
        {
            Artifact = artifact,
            Rules = new List<IAuditRule> { new SafetyRule() }
        });

        audit.Passed.Should().BeFalse();
        audit.Violations.Should().ContainSingle(v => v.RuleId == "SAFETY-001");

        var enforcement = new StrictPolicy().Apply(new EnforcementContext { AuditResult = audit });

        enforcement.Verdict.Should().Be(EnforcementVerdict.Reject,
            "Safety violation should be rejected by StrictPolicy");
        enforcement.Evidence.ViolationCount.Should().Be(1);
    }

    private static void shouldNotHaveMutated(AuditResult audit)
    {
        audit.Passed.Should().BeTrue();
        audit.MaxSeverity.Should().BeNull();
    }
}
