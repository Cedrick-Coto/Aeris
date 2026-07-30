namespace Aeris.Engine;

public struct PlanStep
{
    public int Index;
    public string Action;
    public string Prerequisite;
    public string ExpectedResult;
}

public struct PlanCandidate
{
    public uint Id;
    public uint GoalId;
    public PlanStep[] Steps;
    public string ExpectedOutcome;
    public float Confidence;
    public float Feasibility;
    public float Preference;
    public float Cost;
    public float Risk;
}
