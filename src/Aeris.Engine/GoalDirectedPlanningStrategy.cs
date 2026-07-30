namespace Aeris.Engine;

public sealed class GoalDirectedPlanningStrategy : IPlanningStrategy
{
    public PlanningResult Plan(PlanningContext context)
    {
        var result = new PlanningResult();

        if (context.ActiveGoals.Count == 0)
            return result;

        uint nextId = 1;
        foreach (var goal in context.ActiveGoals)
        {
            var plans = GeneratePlansForGoal(goal, context, ref nextId);
            result.Plans.AddRange(plans);
        }

        result.Plans.Sort((a, b) => b.Feasibility.CompareTo(a.Feasibility));

        foreach (var plan in result.Plans)
        {
            result.Evidence.Add(new PlanningEvidence
            {
                PlanId = plan.Id,
                GoalId = plan.GoalId,
                StepCount = plan.Steps.Length,
                Strategy = nameof(GoalDirectedPlanningStrategy),
                ElapsedMicroseconds = 0
            });
        }

        return result;
    }

    private static List<PlanCandidate> GeneratePlansForGoal(
        GoalData goal, PlanningContext context, ref uint nextId)
    {
        var plans = new List<PlanCandidate>();
        string goalLabel = goal.Type.ToString();

        bool locationKnown = context.WorldModel.KnownEntityIds.Count > 0 ||
            context.AvailableInferences.Any(i =>
                i.Conclusion.Contains(goalLabel, StringComparison.OrdinalIgnoreCase));

        if (locationKnown)
        {
            plans.Add(new PlanCandidate
            {
                Id = nextId++,
                GoalId = goal.Id,
                Steps = new[]
                {
                    new PlanStep { Index = 1, Action = $"MoveToward{goalLabel}", Prerequisite = "location_known", ExpectedResult = $"at_{goalLabel}" },
                    new PlanStep { Index = 2, Action = "Interact", Prerequisite = $"at_{goalLabel}", ExpectedResult = $"{goalLabel}_engaged" }
                },
                ExpectedOutcome = $"{goalLabel} reached and engaged",
                Confidence = 0.7f,
                Feasibility = ComputeFeasibility(goal, context, 0.7f),
                Preference = ComputePreference(goal, context, 0.7f),
                Cost = 0.3f,
                Risk = 0.2f
            });
        }
        else
        {
            plans.Add(new PlanCandidate
            {
                Id = nextId++,
                GoalId = goal.Id,
                Steps = new[]
                {
                    new PlanStep { Index = 1, Action = "Explore", Prerequisite = "unknown_location", ExpectedResult = $"{goalLabel}_area_found" },
                    new PlanStep { Index = 2, Action = $"MoveToward{goalLabel}", Prerequisite = $"{goalLabel}_area_found", ExpectedResult = $"at_{goalLabel}" },
                    new PlanStep { Index = 3, Action = "Interact", Prerequisite = $"at_{goalLabel}", ExpectedResult = $"{goalLabel}_engaged" }
                },
                ExpectedOutcome = $"{goalLabel} located and engaged via exploration",
                Confidence = 0.5f,
                Feasibility = ComputeFeasibility(goal, context, 0.5f),
                Preference = ComputePreference(goal, context, 0.5f),
                Cost = 0.6f,
                Risk = 0.4f
            });
        }

        if (goal.Urgency < 0.3f && goal.Priority <= GoalPriority.Medium)
        {
            plans.Add(new PlanCandidate
            {
                Id = nextId++,
                GoalId = goal.Id,
                Steps = new[]
                {
                    new PlanStep { Index = 1, Action = "Defer", Prerequisite = "low_priority", ExpectedResult = "goal_postponed" }
                },
                ExpectedOutcome = $"{goalLabel} postponed until higher priority or better conditions",
                Confidence = 0.9f,
                Feasibility = 0.9f,
                Preference = ComputePreference(goal, context, 0.9f),
                Cost = 0.0f,
                Risk = 0.0f
            });
        }

        return plans;
    }

    private static float ComputeFeasibility(GoalData goal, PlanningContext context, float baseConfidence)
    {
        float locationKnown = context.WorldModel.KnownEntityIds.Count > 0 ? 0.3f : 0f;
        float inferenceBonus = context.AvailableInferences.Count > 0 ? 0.1f : 0f;
        return Math.Clamp(baseConfidence * 0.5f + locationKnown + inferenceBonus, 0f, 1f);
    }

    private static float ComputePreference(GoalData goal, PlanningContext context, float baseConfidence)
    {
        float priorityScore = (float)goal.Priority / 5f;
        float curiosityBonus = context.Affect.Curiosity > 0.6f ? 0.15f : 0f;
        float threatPenalty = context.Affect.Threat > 0.7f ? -0.2f : 0f;
        float urgencyScore = goal.Urgency;
        return Math.Clamp(priorityScore * 0.4f + urgencyScore * 0.3f + baseConfidence * 0.2f + curiosityBonus + threatPenalty, 0f, 1f);
    }
}
