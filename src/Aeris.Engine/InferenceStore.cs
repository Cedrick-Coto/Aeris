namespace Aeris.Engine;

public sealed class InferenceStore
{
    public List<Inference> Inferences { get; set; } = new();
    public List<ReasoningEvidence> Evidence { get; set; } = new();
    public long LastExecutionTick { get; set; }

    public void Clear()
    {
        Inferences.Clear();
        Evidence.Clear();
    }
}
