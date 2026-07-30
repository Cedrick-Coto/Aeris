namespace Aeris.Engine;

public sealed class GreedyPlanningStrategy : IPlanningStrategy
{
    public PlanningResult Plan(PlanningContext context)
    {
        var result = new PlanningResult();

        if (context.ActiveGoals.Count == 0)
            return result;

        uint nextId = 1;
        foreach (var goal in context.ActiveGoals)
        {
            string goalLabel = goal.Type.ToString();

            var plan = new PlanCandidate
            {
                Id = nextId++,
                GoalId = goal.Id,
                Steps = new[]
                {
                    new PlanStep { Index = 1, Action = $"Approach{goalLabel}", Prerequisite = "ready", ExpectedResult = $"at_{goalLabel}" },
                    new PlanStep { Index = 2, Action = "Execute", Prerequisite = $"at_{goalLabel}", ExpectedResult = $"{goalLabel}_done" }
                },
                ExpectedOutcome = $"{goalLabel} completed via direct approach",
                Confidence = 1f,
                Feasibility = ComputeFeasibility(goal),
                Preference = ComputePreference(goal, context.Affect),
                Cost = 0.5f,
                Risk = 0.3f
            };

            result.Plans.Add(plan);
        }

        foreach (var plan in result.Plans)
        {
            result.Evidence.Add(new PlanningEvidence
            {
                PlanId = plan.Id,
                GoalId = plan.GoalId,
                StepCount = plan.Steps.Length,
                Strategy = nameof(GreedyPlanningStrategy),
                ElapsedMicroseconds = 0
            });
        }

        return result;
    }

    private static float ComputeFeasibility(GoalData goal)
    {
        return Math.Clamp(0.7f + (float)goal.Priority * 0.05f, 0f, 1f);
    }

    private static float ComputePreference(GoalData goal, AffectState affect)
    {
        float priorityScore = (float)goal.Priority / 5f;
        float urgencyScore = goal.Urgency;
        float curiosityBonus = affect.Curiosity > 0.5f ? 0.1f : 0f;
        return Math.Clamp(priorityScore * 0.5f + urgencyScore * 0.3f + curiosityBonus, 0f, 1f);
    }
}
