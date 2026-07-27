namespace Aeris.Engine;

public sealed class EntityBuilder
{
    private readonly World _world;
    private readonly EntityId _id;
    private readonly List<(Type Type, object Component)> _components = new();

    internal EntityBuilder(World world, EntityId id)
    {
        _world = world;
        _id = id;
    }

    public EntityBuilder With<T>(T component) where T : unmanaged
    {
        _components.Add((typeof(T), component));
        return this;
    }

    public Entity Build()
    {
        var entity = new Entity(_id);

        foreach (var (type, component) in _components)
        {
            entity.AddComponentDynamic(type, component);
        }

        _world.AddEntity(entity);
        return entity;
    }
}
