namespace Aeris.Engine;

public sealed class EnforcementContext
{
    public AuditResult AuditResult { get; init; } = null!;
}

public interface IEnforcementPolicy
{
    string Name { get; }
    EnforcementResult Apply(EnforcementContext context);
}
