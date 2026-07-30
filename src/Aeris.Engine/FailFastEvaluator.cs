namespace Aeris.Engine;

public sealed class FailFastEvaluator : IAuditStrategy
{
    public AuditResult Audit(AuditContext context)
    {
        foreach (var rule in context.Rules)
        {
            var violation = rule.Evaluate(context.Artifact);

            if (violation == null)
                continue;

            if (violation.Value.Verdict == RuleVerdict.NotApplicable)
                continue;

            if (violation.Value.Verdict == RuleVerdict.Satisfied)
                continue;

            return new AuditResult
            {
                Violations = new List<AuditViolation> { violation.Value },
                MaxSeverity = violation.Value.Severity,
                Evidence = new AuditEvidence
                {
                    ArtifactType = context.Artifact.ArtifactType,
                    ArtifactId = context.Artifact.ArtifactId,
                    RulesEvaluated = context.Rules.Count,
                    RulesPassed = 0,
                    RulesFailed = 1,
                    Strategy = nameof(FailFastEvaluator),
                    ElapsedMicroseconds = 0
                }
            };
        }

        return new AuditResult
        {
            Violations = new List<AuditViolation>(),
            MaxSeverity = null,
            Evidence = new AuditEvidence
            {
                ArtifactType = context.Artifact.ArtifactType,
                ArtifactId = context.Artifact.ArtifactId,
                RulesEvaluated = context.Rules.Count,
                RulesPassed = context.Rules.Count,
                RulesFailed = 0,
                Strategy = nameof(FailFastEvaluator),
                ElapsedMicroseconds = 0
            }
        };
    }
}
