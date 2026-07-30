namespace Aeris.Engine;

public interface IAuditStrategy
{
    AuditResult Audit(AuditContext context);
}
