namespace Aeris.Engine;

public sealed class ReasoningResult
{
    public List<Inference> Inferences { get; init; } = new();
    public List<ReasoningEvidence> Evidence { get; init; } = new();
}
