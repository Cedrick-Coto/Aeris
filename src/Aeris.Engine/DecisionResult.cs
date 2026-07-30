namespace Aeris.Engine;

public enum DecisionStatus
{
    Selected,
    Deferred,
    NoViablePlan
}

public struct SelectedAction
{
    public uint PlanId;
    public string Action;
    public uint GoalId;
    public float Confidence;
}

public struct RejectedPlanInfo
{
    public uint PlanId;
    public float Feasibility;
}

public struct SelectedPlanInfo
{
    public uint PlanId;
    public float Feasibility;
    public float Preference;
}

public struct SelectionReason
{
    public string Policy;
    public float Threshold;
    public RejectedPlanInfo[] Rejected;
    public SelectedPlanInfo? Selected;
    public string TieBreaker;
}

public struct DecisionEvidence
{
    public DecisionStatus Status;
    public int CandidatesConsidered;
    public string SelectionPolicy;
    public float Threshold;
    public SelectionReason Reason;
    public long ElapsedMicroseconds;
}

public struct DecisionResult
{
    public DecisionStatus Status;
    public uint? SelectedPlanId;
    public SelectedAction Action;
    public DecisionEvidence Evidence;
}
