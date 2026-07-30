namespace Aeris.Engine;

public interface IAuditableArtifact
{
    string ArtifactType { get; }
    uint ArtifactId { get; }
}

public enum RuleVerdict
{
    NotApplicable,
    Satisfied,
    Violated
}

public enum ViolationSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public struct AuditViolation
{
    public string RuleId;
    public string RuleVersion;
    public RuleVerdict Verdict;
    public ViolationSeverity Severity;
    public string Condition;
    public string Evidence;
    public uint ArtifactId;
}

public struct AuditEvidence
{
    public string ArtifactType;
    public uint ArtifactId;
    public int RulesEvaluated;
    public int RulesPassed;
    public int RulesFailed;
    public string Strategy;
    public long ElapsedMicroseconds;
}

public sealed class AuditResult
{
    public bool Passed => Violations.Count == 0;
    public List<AuditViolation> Violations { get; init; } = new();
    public ViolationSeverity? MaxSeverity { get; init; }
    public AuditEvidence Evidence { get; init; }
}
