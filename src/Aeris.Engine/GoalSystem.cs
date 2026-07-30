namespace Aeris.Engine;

public sealed class GoalSystem : ISystem
{
    public string Name => "GoalSystem";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 20;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        if (!world.HasResource<GoalStore>())
            return;

        var goals = world.GetResource<GoalStore>();
        var affect = world.HasResource<AffectState>() ? world.GetResource<AffectState>() : AffectState.Default;
        float currentTime = (float)time.SimulationTime;

        foreach (var kvp in goals.All)
        {
            uint entityId = kvp.Key;
            var entityGoals = kvp.Value;

            foreach (var goal in entityGoals)
            {
                    if (goal.Status == GoalStatus.Active)
                {
                    if (goal.Deadline.HasValue && currentTime > goal.Deadline.Value)
                    {
                        var updated = goal;
                        updated.Status = GoalStatus.Failed;
                        UpdateGoal(goals, entityId, goal.Id, updated);
                        continue;
                    }

                    float modulatedPriority = goal.EffectivePriority(currentTime);
                    modulatedPriority = ModulateByAffect(modulatedPriority, goal.Type, affect);
                }
            }
        }

        EnsureDefaultGoal(world, goals, currentTime);
    }

    private static float ModulateByAffect(float basePriority, GoalType type, AffectState affect)
    {
        float mod = basePriority;
        if (type == GoalType.Exploration)
            mod *= 1f + affect.Curiosity * 0.5f;
        if (type == GoalType.Survival)
            mod *= 1f + affect.Threat * 0.8f;
        if (type == GoalType.Social)
            mod *= 1f + affect.Trust * 0.3f;
        return mod;
    }

    private static void EnsureDefaultGoal(World world, GoalStore goals, float currentTime)
    {
        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<GoalMarker>())
                continue;

            var entityGoals = goals.GetGoals(entity.Id.Value);
            if (entityGoals.Count == 0 || !entityGoals.Any(g => g.Status == GoalStatus.Active))
            {
                uint id = goals.AllocateId();
                goals.AddGoal(entity.Id.Value, new GoalData
                {
                    Id = id,
                    Type = GoalType.Exploration,
                    Priority = GoalPriority.Low,
                    Urgency = 0.3f,
                    Status = GoalStatus.Active,
                    CreationTime = currentTime
                });

                var marker = entity.GetComponent<GoalMarker>();
                marker.ActiveCount = 1;
                marker.HighestPriority = GoalPriority.Low;
                entity.SetComponent(marker);
            }
        }
    }

    private static void UpdateGoal(GoalStore goals, uint entityId, uint goalId, GoalData updated)
    {
        var entityGoals = goals.GetGoals(entityId);
        int idx = entityGoals.FindIndex(g => g.Id == goalId);
        if (idx >= 0)
        {
            entityGoals[idx] = updated;
        }
    }
}
