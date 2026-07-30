using Aeris.Engine;

namespace Aeris.Engine.Tests;

public sealed class SafetyRule : IAuditRule
{
    public string RuleId => "SAFETY-001";
    public string RuleVersion => "1.0";
    public string Description => "Prohíbe acciones peligrosas";
    public string[] SupportedArtifactTypes => new[] { "DecisionResult" };

    public AuditViolation? Evaluate(IAuditableArtifact artifact)
    {
        if (artifact is not DecisionResultAuditable da)
            return null;

        var action = da.Decision.Action.Action;
        var prohibited = new[] { "Attack", "SelfDestruct", "Betray" };

        if (prohibited.Contains(action))
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = ViolationSeverity.Critical,
                Condition = $"Acción '{action}' está prohibida",
                Evidence = $"PlanId={da.Decision.SelectedPlanId}, GoalId={da.Decision.Action.GoalId}",
                ArtifactId = artifact.ArtifactId
            };
        }

        return new AuditViolation
        {
            RuleId = RuleId,
            RuleVersion = RuleVersion,
            Verdict = RuleVerdict.Satisfied,
            ArtifactId = artifact.ArtifactId
        };
    }
}

public sealed class ConsistencyRule : IAuditRule
{
    public string RuleId => "CONSISTENCY-001";
    public string RuleVersion => "1.0";
    public string Description => "Verifica que la acción seleccionada corresponde al plan";
    public string[] SupportedArtifactTypes => new[] { "DecisionResult" };

    public AuditViolation? Evaluate(IAuditableArtifact artifact)
    {
        if (artifact is not DecisionResultAuditable da)
            return null;

        if (da.Decision.Status != DecisionStatus.Selected)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.NotApplicable,
                ArtifactId = artifact.ArtifactId
            };
        }

        if (da.Decision.SelectedPlanId == null || da.Decision.SelectedPlanId == 0)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = ViolationSeverity.High,
                Condition = "Acción seleccionada sin ID de plan",
                Evidence = $"Action={da.Decision.Action.Action}",
                ArtifactId = artifact.ArtifactId
            };
        }

        return new AuditViolation
        {
            RuleId = RuleId,
            RuleVersion = RuleVersion,
            Verdict = RuleVerdict.Satisfied,
            ArtifactId = artifact.ArtifactId
        };
    }
}

public sealed class ExperimentalRule : IAuditRule
{
    public string RuleId => "EXPERIMENTAL-001";
    public string RuleVersion => "0.5";
    public string Description => "Marca decisiones de exploración para estudio";
    public string[] SupportedArtifactTypes => new[] { "DecisionResult" };

    public AuditViolation? Evaluate(IAuditableArtifact artifact)
    {
        if (artifact is not DecisionResultAuditable da)
            return null;

        var action = da.Decision.Action.Action;
        var exploratoryActions = new[] { "Explore", "Investigate", "Search" };

        if (exploratoryActions.Contains(action))
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = ViolationSeverity.Low,
                Condition = $"Acción exploratoria '{action}' marcada para estudio",
                Evidence = $"PlanId={da.Decision.SelectedPlanId}",
                ArtifactId = artifact.ArtifactId
            };
        }

        return new AuditViolation
        {
            RuleId = RuleId,
            RuleVersion = RuleVersion,
            Verdict = RuleVerdict.Satisfied,
            ArtifactId = artifact.ArtifactId
        };
    }
}

public sealed class PerformanceRule : IAuditRule
{
    public string RuleId => "PERF-001";
    public string RuleVersion => "1.0";
    public string Description => "Advierte cuando la viabilidad o confianza es baja";
    public string[] SupportedArtifactTypes => new[] { "DecisionResult" };

    public float MinFeasibility { get; }
    public float MinConfidence { get; }

    public PerformanceRule(float minFeasibility = 0.5f, float minConfidence = 0.3f)
    {
        MinFeasibility = minFeasibility;
        MinConfidence = minConfidence;
    }

    public AuditViolation? Evaluate(IAuditableArtifact artifact)
    {
        if (artifact is not DecisionResultAuditable da)
            return null;

        if (da.Decision.Status != DecisionStatus.Selected)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.NotApplicable,
                ArtifactId = artifact.ArtifactId
            };
        }

        var action = da.Decision.Action;
        if (action.Confidence < MinConfidence)
        {
            return new AuditViolation
            {
                RuleId = RuleId,
                RuleVersion = RuleVersion,
                Verdict = RuleVerdict.Violated,
                Severity = ViolationSeverity.Medium,
                Condition = $"Confidence ({action.Confidence:F2}) < mínimo ({MinConfidence})",
                Evidence = $"Action={action.Action}, PlanId={action.PlanId}",
                ArtifactId = artifact.ArtifactId
            };
        }

        return new AuditViolation
        {
            RuleId = RuleId,
            RuleVersion = RuleVersion,
            Verdict = RuleVerdict.Satisfied,
            ArtifactId = artifact.ArtifactId
        };
    }
}
