namespace Aeris.Engine;

public sealed class EmotionProcessingSystem : ISystem
{
    public string Name => "EmotionProcessing";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 200;

    private const float EMOTION_UPDATE_INTERVAL = 60f;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var eventBus = world.GetResource<EventBus>();
        float currentTime = (float)time.SimulationTime;

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<EmotionComponent>()) continue;

            var emotion = entity.GetComponent<EmotionComponent>();

            if (currentTime - emotion.UpdateTime < EMOTION_UPDATE_INTERVAL)
                continue;

            var previousEmotion = emotion.Primary;
            var effectiveIntensity = emotion.EffectiveIntensity(currentTime);

            if (effectiveIntensity <= 0.01f)
            {
                emotion.Primary = EmotionType.None;
                emotion.Intensity = 0f;
            }
            else
            {
                emotion.Intensity = effectiveIntensity;
            }

            emotion.UpdateTime = currentTime;
            entity.SetComponent(emotion);

            if (previousEmotion != emotion.Primary && previousEmotion != EmotionType.None)
            {
                eventBus.Emit(new EmotionChangedEvent
                {
                    EntityId = entity.Id.Value,
                    PreviousEmotion = previousEmotion,
                    NewEmotion = emotion.Primary,
                    Intensity = emotion.Intensity,
                    TriggerEntityId = emotion.TriggerEntityId
                });
            }
        }
    }
}
