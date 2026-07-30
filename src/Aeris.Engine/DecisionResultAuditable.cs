namespace Aeris.Engine;

public sealed class DecisionResultAuditable : IAuditableArtifact
{
    public string ArtifactType => "DecisionResult";
    public uint ArtifactId { get; init; }
    public DecisionResult Decision { get; init; }
}
