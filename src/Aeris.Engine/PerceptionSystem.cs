namespace Aeris.Engine;

public sealed class PerceptionSystem : ISystem
{
    public string Name => "PerceptionSystem";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 10;

    public float PerceptionRange { get; set; } = 50f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var percepts = new PerceptBatch { Tick = time.Tick };

        foreach (var kvp in world.Entities)
        {
            var entity = kvp.Value;
            if (!entity.HasComponent<CognitiveAgentMarker>())
                continue;

            ScanWorld(world, entity, percepts, time);
        }

        if (world.HasResource<PerceptBatch>())
            world.SetResource(percepts);
        else
            world.AddResource(percepts);

        if (world.HasResource<CognitiveTraceLog>())
        {
            var trace = world.GetResource<CognitiveTraceLog>();
            trace.Record(Name, $"{world.EntityCount} entities", $"{percepts.Percepts.Count} percepts", "Scan world for visible entities");
        }
    }

    private static void ScanWorld(World world, Entity agent, PerceptBatch batch, TimeResource time)
    {
        foreach (var otherKvp in world.Entities)
        {
            var other = otherKvp.Value;
            if (other.Id == agent.Id)
                continue;

            float confidence = 0.9f;

            if (other.HasComponent<VisualTag>())
            {
                var vis = other.GetComponent<VisualTag>();
                batch.Percepts.Add(new Percept
                {
                    Type = PerceptType.Visual,
                    Source = other.Id,
                    LabelId = vis.LabelId,
                    Confidence = confidence,
                    Timestamp = time.Tick,
                    Distance = 0f,
                    VisualSize = vis.Size
                });
            }

            if (other.HasComponent<AuraTag>())
            {
                var aura = other.GetComponent<AuraTag>();
                batch.Percepts.Add(new Percept
                {
                    Type = PerceptType.Aura,
                    Source = other.Id,
                    LabelId = aura.LabelId,
                    Confidence = confidence * 0.8f,
                    Timestamp = time.Tick,
                    Distance = 0f,
                    AuraSignature = aura.Signature
                });
            }
        }
    }
}
