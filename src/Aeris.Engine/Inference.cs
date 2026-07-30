namespace Aeris.Engine;

public struct Inference
{
    public uint Id;
    public string[] Premises;
    public string RuleId;
    public string Transformation;
    public string Conclusion;
    public float Confidence;
}
