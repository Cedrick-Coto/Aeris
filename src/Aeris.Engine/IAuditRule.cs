namespace Aeris.Engine;

public interface IAuditRule
{
    string RuleId { get; }
    string RuleVersion { get; }
    string Description { get; }
    string[] SupportedArtifactTypes { get; }
    AuditViolation? Evaluate(IAuditableArtifact artifact);
}

public sealed class AuditContext
{
    public IAuditableArtifact Artifact { get; init; } = null!;
    public List<IAuditRule> Rules { get; init; } = new();
}
