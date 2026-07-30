namespace Aeris.Engine;

public enum EnforcementVerdict
{
    Approve,
    Reject,
    RequestReplanning,
    Defer
}

public struct EnforcementEvidence
{
    public EnforcementVerdict Verdict;
    public string Policy;
    public int ViolationCount;
    public ViolationSeverity? MaxSeverity;
    public string Reason;
    public long ElapsedMicroseconds;
}

public sealed class EnforcementResult
{
    public EnforcementVerdict Verdict { get; init; }
    public EnforcementEvidence Evidence { get; init; }
}
