namespace Aeris.Engine;

public sealed class StrictPolicy : IEnforcementPolicy
{
    public string Name => "StrictPolicy";

    public EnforcementResult Apply(EnforcementContext context)
    {
        var audit = context.AuditResult;
        string reason;

        if (audit.Passed)
        {
            reason = "All rules passed";
            return MakeResult(EnforcementVerdict.Approve, audit, reason);
        }

        var maxSeverity = GetMaxSeverity(audit);
        if (maxSeverity >= ViolationSeverity.High)
        {
            reason = $"Rejected: {audit.Violations.Count} violations, max severity {maxSeverity}";
            return MakeResult(EnforcementVerdict.Reject, audit, reason);
        }

        reason = $"Requesting replanning: {audit.Violations.Count} violations (max {maxSeverity})";
        return MakeResult(EnforcementVerdict.RequestReplanning, audit, reason);
    }

    private static ViolationSeverity? GetMaxSeverity(AuditResult audit)
    {
        if (audit.MaxSeverity.HasValue)
            return audit.MaxSeverity.Value;
        if (audit.Violations.Count == 0)
            return null;
        return audit.Violations.Max(v => v.Severity);
    }

    private static EnforcementResult MakeResult(
        EnforcementVerdict verdict, AuditResult audit, string reason)
    {
        return new EnforcementResult
        {
            Verdict = verdict,
            Evidence = new EnforcementEvidence
            {
                Verdict = verdict,
                Policy = "StrictPolicy",
                ViolationCount = audit.Violations.Count,
                MaxSeverity = GetMaxSeverity(audit),
                Reason = reason,
                ElapsedMicroseconds = 0
            }
        };
    }
}

public sealed class PermissivePolicy : IEnforcementPolicy
{
    public string Name => "PermissivePolicy";

    public EnforcementResult Apply(EnforcementContext context)
    {
        var audit = context.AuditResult;
        string reason;

        if (audit.Passed)
        {
            reason = "All rules passed";
            return MakeResult(EnforcementVerdict.Approve, audit, reason);
        }

        var maxSeverity = GetMaxSeverity(audit);
        if (maxSeverity == ViolationSeverity.Critical)
        {
            reason = $"Rejected: critical violation present";
            return MakeResult(EnforcementVerdict.Reject, audit, reason);
        }

        if (maxSeverity >= ViolationSeverity.High)
        {
            reason = $"Requesting replanning: severity {maxSeverity}";
            return MakeResult(EnforcementVerdict.RequestReplanning, audit, reason);
        }

        reason = $"Approved: violations below threshold (max {maxSeverity})";
        return MakeResult(EnforcementVerdict.Approve, audit, reason);
    }

    private static ViolationSeverity? GetMaxSeverity(AuditResult audit)
    {
        if (audit.MaxSeverity.HasValue)
            return audit.MaxSeverity.Value;
        if (audit.Violations.Count == 0)
            return null;
        return audit.Violations.Max(v => v.Severity);
    }

    private static EnforcementResult MakeResult(
        EnforcementVerdict verdict, AuditResult audit, string reason)
    {
        return new EnforcementResult
        {
            Verdict = verdict,
            Evidence = new EnforcementEvidence
            {
                Verdict = verdict,
                Policy = "PermissivePolicy",
                ViolationCount = audit.Violations.Count,
                MaxSeverity = GetMaxSeverity(audit),
                Reason = reason,
                ElapsedMicroseconds = 0
            }
        };
    }
}

public sealed class SafetyFirstPolicy : IEnforcementPolicy
{
    public string Name => "SafetyFirstPolicy";

    public EnforcementResult Apply(EnforcementContext context)
    {
        var audit = context.AuditResult;
        string reason;

        bool hasSafetyViolation = audit.Violations.Any(v =>
            v.RuleId.StartsWith("SAFETY-", StringComparison.OrdinalIgnoreCase));

        if (hasSafetyViolation)
        {
            reason = $"Rejected: safety rule violated";
            return MakeResult(EnforcementVerdict.Reject, audit, reason);
        }

        if (!audit.Passed)
        {
            reason = $"Requesting replanning: {audit.Violations.Count} non-safety violations";
            return MakeResult(EnforcementVerdict.RequestReplanning, audit, reason);
        }

        reason = "All rules passed";
        return MakeResult(EnforcementVerdict.Approve, audit, reason);
    }

    private static EnforcementResult MakeResult(
        EnforcementVerdict verdict, AuditResult audit, string reason)
    {
        return new EnforcementResult
        {
            Verdict = verdict,
            Evidence = new EnforcementEvidence
            {
                Verdict = verdict,
                Policy = "SafetyFirstPolicy",
                ViolationCount = audit.Violations.Count,
                MaxSeverity = audit.MaxSeverity,
                Reason = reason,
                ElapsedMicroseconds = 0
            }
        };
    }
}
