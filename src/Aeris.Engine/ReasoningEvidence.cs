namespace Aeris.Engine;

public struct ReasoningEvidence
{
    public uint InferenceId;
    public string RuleId;
    public int PremiseCount;
    public string Transformation;
    public float Confidence;
    public float EvidenceStrength;
    public string Strategy;
    public long ElapsedMicroseconds;
}
