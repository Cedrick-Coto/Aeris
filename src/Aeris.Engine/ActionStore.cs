namespace Aeris.Engine;

public sealed class ActionStore
{
    public DecisionResult LastResult { get; set; }
    public long LastExecutionTick { get; set; }
}
