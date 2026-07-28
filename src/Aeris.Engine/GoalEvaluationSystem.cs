namespace Aeris.Engine;

public sealed class GoalEvaluationSystem : ISystem
{
    public string Name => "GoalEvaluation";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 210;

    private const float EVALUATION_INTERVAL = 300f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var goals = world.GetResource<GoalStore>();
        var eventBus = world.GetResource<EventBus>();
        float currentTime = (float)time.SimulationTime;

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<GoalMarker>()) continue;

            var marker = entity.GetComponent<GoalMarker>();
            var entityGoals = goals.GetGoals(entity.Id.Value);

            int activeCount = 0;
            GoalPriority highest = GoalPriority.Trivial;

            for (int i = entityGoals.Count - 1; i >= 0; i--)
            {
                var goal = entityGoals[i];

                if (goal.Deadline.HasValue && goal.Deadline.Value < currentTime && goal.IsActive)
                {
                    goal.Status = GoalStatus.Failed;
                    entityGoals[i] = goal;

                    eventBus.Emit(new GoalCompletedEvent
                    {
                        EntityId = entity.Id.Value,
                        GoalId = goal.Id,
                        Type = goal.Type,
                        Result = GoalStatus.Failed
                    });
                    continue;
                }

                if (goal.IsActive)
                {
                    activeCount++;
                    if (goal.Priority > highest)
                        highest = goal.Priority;
                }
            }

            marker.ActiveCount = activeCount;
            marker.HighestPriority = highest;
            entity.SetComponent(marker);
        }
    }
}
