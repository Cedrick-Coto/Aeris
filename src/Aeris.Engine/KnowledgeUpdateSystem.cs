namespace Aeris.Engine;

public sealed class KnowledgeUpdateSystem : ISystem
{
    public string Name => "KnowledgeUpdate";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 110;

    private const float UPDATE_INTERVAL = 7200f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var knowledge = world.GetResource<KnowledgeStore>();

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<KnowledgeMarker>()) continue;

            var marker = entity.GetComponent<KnowledgeMarker>();

            if (time.SimulationTime - marker.LastUpdateTime < UPDATE_INTERVAL)
                continue;

            var entityKnowledge = knowledge.GetKnowledge(entity.Id.Value);
            float currentTime = (float)time.SimulationTime;
            int count = 0;

            for (int i = entityKnowledge.Count - 1; i >= 0; i--)
            {
                var k = entityKnowledge[i];

                if (k.ExpirationTime.HasValue && k.ExpirationTime.Value < currentTime)
                {
                    entityKnowledge.RemoveAt(i);
                    continue;
                }

                count++;
            }

            marker.Count = count;
            marker.LastUpdateTime = currentTime;
            entity.SetComponent(marker);
        }
    }
}
