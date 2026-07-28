namespace Aeris.Engine;

public sealed class AttentionUpdateSystem : ISystem
{
    public string Name => "AttentionUpdate";
    public SystemPhase Phase => SystemPhase.Perception;
    public int Priority => 90;

    public void Execute(World world, float deltaTime)
    {
        var time = world.GetResource<TimeResource>();
        var attentionStore = world.GetResource<AttentionStore>();
        var eventBus = world.GetResource<EventBus>();
        float currentTime = (float)time.SimulationTime;

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<AttentionComponent>()) continue;

            var attention = entity.GetComponent<AttentionComponent>();
            var previousFocus = attention.FocusTargetId;

            if (currentTime - attention.UpdateTime < deltaTime)
                continue;

            if (attention.HasFocus)
            {
                if (!world.HasEntity(new EntityId(attention.FocusTargetId)))
                {
                    attention.FocusTargetId = 0;
                    attention.FocusIntensity = 0f;
                }
            }

            attention.UpdateTime = currentTime;
            entity.SetComponent(attention);

            if (previousFocus != attention.FocusTargetId)
            {
                eventBus.Emit(new AttentionChangedEvent
                {
                    EntityId = entity.Id.Value,
                    PreviousFocusId = previousFocus,
                    NewFocusId = attention.FocusTargetId,
                    Intensity = attention.FocusIntensity
                });
            }
        }
    }
}
