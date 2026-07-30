namespace Aeris.Engine;

public struct PlanningEvidence
{
    public uint PlanId;
    public uint GoalId;
    public int StepCount;
    public string Strategy;
    public long ElapsedMicroseconds;
}
