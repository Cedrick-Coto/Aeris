namespace Aeris.Engine;

public sealed class FeasibilityThresholdPolicy : IDecisionStrategy
{
    public const float BaselineThreshold = 0.5f;
    public const float MinThreshold = 0.2f;
    public const float MaxThreshold = 0.8f;

    public DecisionResult Decide(DecisionContext context)
    {
        float threshold = ComputeThreshold(context.Affect);

        var viable = context.CandidatePlans
            .Where(p => p.Feasibility >= threshold)
            .OrderByDescending(p => p.Preference)
            .ThenBy(p => p.Risk)
            .ToList();

        var rejected = context.CandidatePlans
            .Where(p => p.Feasibility < threshold)
            .Select(p => new RejectedPlanInfo { PlanId = p.Id, Feasibility = p.Feasibility })
            .ToArray();

        if (viable.Count == 0)
        {
            return new DecisionResult
            {
                Status = DecisionStatus.NoViablePlan,
                SelectedPlanId = null,
                Action = new SelectedAction { Action = "Defer", Confidence = 1f },
                Evidence = BuildEvidence(context, threshold, DecisionStatus.NoViablePlan, null, rejected, string.Empty)
            };
        }

        var selected = viable[0];
        string tieBreaker = DetermineTieBreaker(viable);
        var selectedInfo = new SelectedPlanInfo
        {
            PlanId = selected.Id,
            Feasibility = selected.Feasibility,
            Preference = selected.Preference
        };

        return new DecisionResult
        {
            Status = DecisionStatus.Selected,
            SelectedPlanId = selected.Id,
            Action = new SelectedAction
            {
                PlanId = selected.Id,
                Action = selected.Steps.Length > 0 ? selected.Steps[0].Action : "NoAction",
                GoalId = selected.GoalId,
                Confidence = selected.Confidence
            },
            Evidence = BuildEvidence(context, threshold, DecisionStatus.Selected, selectedInfo, rejected, tieBreaker)
        };
    }

    private static float ComputeThreshold(AffectState affect)
    {
        float stressModulation = affect.Stress * 0.2f;
        float confidenceModulation = affect.Confidence * 0.1f;
        return Math.Clamp(BaselineThreshold - stressModulation + confidenceModulation, MinThreshold, MaxThreshold);
    }

    private static string DetermineTieBreaker(List<PlanCandidate> viable)
    {
        if (viable.Count <= 1)
            return "SingleViable";
        if (viable[0].Preference != viable[1].Preference)
            return "HighestPreference";
        return "LowestRisk";
    }

    private static DecisionEvidence BuildEvidence(
        DecisionContext context, float threshold, DecisionStatus status,
        SelectedPlanInfo? selected, RejectedPlanInfo[] rejected, string tieBreaker)
    {
        return new DecisionEvidence
        {
            Status = status,
            CandidatesConsidered = context.CandidatePlans.Count,
            SelectionPolicy = nameof(FeasibilityThresholdPolicy),
            Threshold = threshold,
            Reason = new SelectionReason
            {
                Policy = nameof(FeasibilityThresholdPolicy),
                Threshold = threshold,
                Rejected = rejected,
                Selected = selected,
                TieBreaker = tieBreaker
            },
            ElapsedMicroseconds = 0
        };
    }
}
