namespace Aeris.Engine;

public struct AffectState
{
    public float Curiosity;
    public float Stress;
    public float Confidence;
    public float Trust;
    public float Novelty;
    public float Attachment;
    public float Threat;
    public float RewardExpectation;
    public float CognitiveLoad;

    public readonly AffectState Clamped()
    {
        return new AffectState
        {
            Curiosity = Math.Clamp(Curiosity, 0f, 1f),
            Stress = Math.Clamp(Stress, 0f, 1f),
            Confidence = Math.Clamp(Confidence, 0f, 1f),
            Trust = Math.Clamp(Trust, 0f, 1f),
            Novelty = Math.Clamp(Novelty, 0f, 1f),
            Attachment = Math.Clamp(Attachment, 0f, 1f),
            Threat = Math.Clamp(Threat, 0f, 1f),
            RewardExpectation = Math.Clamp(RewardExpectation, 0f, 1f),
            CognitiveLoad = Math.Clamp(CognitiveLoad, 0f, 1f)
        };
    }

    public static AffectState Default => new()
    {
        Curiosity = 0.5f,
        Stress = 0.2f,
        Confidence = 0.6f,
        Trust = 0.4f,
        Novelty = 0.3f,
        Attachment = 0.3f,
        Threat = 0.1f,
        RewardExpectation = 0.5f,
        CognitiveLoad = 0.3f
    };
}
