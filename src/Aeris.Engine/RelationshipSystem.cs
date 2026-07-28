namespace Aeris.Engine;

public sealed class RelationshipSystem : ISystem
{
    public string Name => "RelationshipManagement";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 220;

    private const float DECAY_RATE = 0.001f;
    private const float DORMANCY_THRESHOLD = 86400f * 7f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var relationships = world.GetResource<RelationshipStore>();
        float currentTime = (float)time.SimulationTime;

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<RelationshipMarker>()) continue;

            var marker = entity.GetComponent<RelationshipMarker>();
            var entityRelationships = relationships.GetRelationships(entity.Id.Value);

            int activeCount = 0;

            for (int i = entityRelationships.Count - 1; i >= 0; i--)
            {
                var rel = entityRelationships[i];

                if (rel.Status == RelationshipStatus.Broken) continue;

                var timeSinceInteraction = currentTime - rel.LastInteractionTime;

                if (timeSinceInteraction > DORMANCY_THRESHOLD && rel.Status == RelationshipStatus.Active)
                {
                    rel.Status = RelationshipStatus.Dormant;
                }

                if (rel.Status == RelationshipStatus.Active)
                {
                    rel.Familiarity = MathF.Max(0f, rel.Familiarity - DECAY_RATE * deltaTime);
                    rel.Value = Math.Clamp(rel.Value - DECAY_RATE * 0.5f * deltaTime, -1f, 1f);
                }

                entityRelationships[i] = rel;
                activeCount++;
            }

            marker.Count = activeCount;
            entity.SetComponent(marker);
        }
    }
}
