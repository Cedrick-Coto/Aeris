namespace Aeris.Engine;

public sealed class ConfidenceGatePolicy : IDecisionStrategy
{
    public const float ConfidenceThreshold = 0.6f;

    public DecisionResult Decide(DecisionContext context)
    {
        var viable = context.CandidatePlans
            .Where(p => p.Confidence >= ConfidenceThreshold)
            .OrderByDescending(p => p.Preference)
            .ThenBy(p => p.Cost)
            .ToList();

        var rejected = context.CandidatePlans
            .Where(p => p.Confidence < ConfidenceThreshold)
            .Select(p => new RejectedPlanInfo { PlanId = p.Id, Feasibility = p.Confidence })
            .ToArray();

        if (viable.Count == 0)
        {
            return new DecisionResult
            {
                Status = DecisionStatus.NoViablePlan,
                SelectedPlanId = null,
                Action = new SelectedAction { Action = "Defer", Confidence = 1f },
                Evidence = BuildEvidence(context, DecisionStatus.NoViablePlan, null, rejected)
            };
        }

        var selected = viable[0];
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
            Evidence = BuildEvidence(context, DecisionStatus.Selected, selectedInfo, rejected)
        };
    }

    private static DecisionEvidence BuildEvidence(
        DecisionContext context, DecisionStatus status,
        SelectedPlanInfo? selected, RejectedPlanInfo[] rejected)
    {
        return new DecisionEvidence
        {
            Status = status,
            CandidatesConsidered = context.CandidatePlans.Count,
            SelectionPolicy = nameof(ConfidenceGatePolicy),
            Threshold = ConfidenceThreshold,
            Reason = new SelectionReason
            {
                Policy = nameof(ConfidenceGatePolicy),
                Threshold = ConfidenceThreshold,
                Rejected = rejected,
                Selected = selected,
                TieBreaker = selected.HasValue ? "BestPreferenceThenCost" : "NoViable"
            },
            ElapsedMicroseconds = 0
        };
    }
}
