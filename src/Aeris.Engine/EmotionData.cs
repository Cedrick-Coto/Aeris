namespace Aeris.Engine;

public enum EmotionType : byte
{
    None,
    Joy,
    Trust,
    Affection,
    Excitement,
    Pride,
    Relief,
    Gratitude,
    Fear,
    Anger,
    Sadness,
    Disgust,
    Shame,
    Guilt,
    Jealousy,
    Curiosity,
    Surprise,
    Confusion,
    Anticipation,
    Boredom,
    Fatigue,
    Nostalgia,
    Melancholy,
    Hope,
    Despair,
    Determination,
    Ambivalence
}

public struct EmotionData
{
    public EmotionType Primary;
    public float Intensity;
    public float DecayRate;
    public float FormationTime;
    public uint TriggerEntityId;
    public uint TriggerMemoryId;

    public bool IsSignificant => Intensity > 0.1f;
    public bool IsPositive => Primary is EmotionType.Joy or EmotionType.Trust or EmotionType.Affection
        or EmotionType.Excitement or EmotionType.Pride or EmotionType.Relief or EmotionType.Gratitude
        or EmotionType.Hope or EmotionType.Determination;
    public bool IsNegative => Primary is EmotionType.Fear or EmotionType.Anger or EmotionType.Sadness
        or EmotionType.Disgust or EmotionType.Shame or EmotionType.Guilt or EmotionType.Jealousy
        or EmotionType.Despair;

    public float Decay(float currentTime)
    {
        var elapsed = currentTime - FormationTime;
        return MathF.Max(0f, Intensity - DecayRate * elapsed);
    }
}

public struct EmotionComponent
{
    public EmotionType Primary;
    public float Intensity;
    public float DecayRate;
    public float FormationTime;
    public uint TriggerEntityId;
    public uint TriggerMemoryId;
    public float UpdateTime;

    public bool HasEmotion => Primary != EmotionType.None && Intensity > 0.01f;
    public float EffectiveIntensity(float currentTime)
    {
        var elapsed = currentTime - FormationTime;
        return MathF.Max(0f, Intensity - DecayRate * elapsed);
    }
}

public sealed class EmotionStore
{
    private readonly Dictionary<uint, EmotionComponent> _emotions = new();

    public void Set(uint entityId, EmotionComponent emotion)
    {
        _emotions[entityId] = emotion;
    }

    public EmotionComponent Get(uint entityId)
    {
        return _emotions.TryGetValue(entityId, out var e) ? e : default;
    }

    public bool TryGet(uint entityId, out EmotionComponent emotion)
    {
        return _emotions.TryGetValue(entityId, out emotion);
    }

    public void Remove(uint entityId)
    {
        _emotions.Remove(entityId);
    }

    public int Count => _emotions.Count;
    public IReadOnlyDictionary<uint, EmotionComponent> All => _emotions;
}
