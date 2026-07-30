namespace Aeris.Engine;

public enum PerceptType : byte
{
    Visual, Auditory, Aura, Proprioceptive
}

public struct Percept
{
    public PerceptType Type;
    public EntityId Source;
    public uint LabelId;
    public float Confidence;
    public long Timestamp;
    public float Distance;
    public float Salience;
    public float VisualSize;
    public float AuditoryIntensity;
    public float AuraSignature;
    public float ProprioValue;
}

public sealed class PerceptBatch
{
    public List<Percept> Percepts { get; set; } = new();
    public long Tick { get; set; }
}

public sealed class AttendedPercepts
{
    public List<Percept> Percepts { get; set; } = new();
    public long Tick { get; set; }
}
