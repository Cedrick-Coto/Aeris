namespace Aeris.Engine;

public sealed class AuditStore
{
    public AuditResult? LastResult { get; set; }
    public long LastExecutionTick { get; set; }
}
