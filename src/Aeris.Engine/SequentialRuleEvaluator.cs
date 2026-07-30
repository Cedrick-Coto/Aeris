namespace Aeris.Engine;

public sealed class SequentialRuleEvaluator : IAuditStrategy
{
    public AuditResult Audit(AuditContext context)
    {
        var violations = new List<AuditViolation>();
        int passed = 0;

        foreach (var rule in context.Rules)
        {
            var violation = rule.Evaluate(context.Artifact);

            if (violation == null || violation.Value.Verdict == RuleVerdict.NotApplicable)
            {
                if (violation == null || violation.Value.Verdict == RuleVerdict.NotApplicable)
                    passed++;
                continue;
            }

            if (violation.Value.Verdict == RuleVerdict.Satisfied)
            {
                passed++;
                continue;
            }

            violations.Add(violation.Value);
        }

        var maxSeverity = violations.Count > 0
            ? (ViolationSeverity?)violations.Max(v => v.Severity)
            : null;

        return new AuditResult
        {
            Violations = violations,
            MaxSeverity = maxSeverity,
            Evidence = new AuditEvidence
            {
                ArtifactType = context.Artifact.ArtifactType,
                ArtifactId = context.Artifact.ArtifactId,
                RulesEvaluated = context.Rules.Count,
                RulesPassed = passed,
                RulesFailed = violations.Count,
                Strategy = nameof(SequentialRuleEvaluator),
                ElapsedMicroseconds = 0
            }
        };
    }
}
