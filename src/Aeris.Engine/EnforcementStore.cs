namespace Aeris.Engine;

public sealed class EnforcementStore
{
    public EnforcementResult? LastResult { get; set; }
    public long LastExecutionTick { get; set; }
}
