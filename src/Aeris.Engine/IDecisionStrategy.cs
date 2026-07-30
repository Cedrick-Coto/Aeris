namespace Aeris.Engine;

public sealed class DecisionContext
{
    public List<PlanCandidate> CandidatePlans { get; init; } = new();
    public WorldModelState WorldModel { get; init; } = null!;
    public AffectState Affect { get; init; }
    public List<GoalData> ActiveGoals { get; init; } = new();
}

public interface IDecisionStrategy
{
    DecisionResult Decide(DecisionContext context);
}
