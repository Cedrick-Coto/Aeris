using System.Diagnostics;

namespace Aeris.Engine;

public sealed class World
{
    private readonly Dictionary<Type, object> _resources = new();
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private uint _nextEntityId = 1;

    public EntityId CreateEntityId()
    {
        var id = new EntityId(_nextEntityId);
        _nextEntityId++;

        Debug.Assert(!_entities.ContainsKey(id), $"Entity {id} already exists");
        return id;
    }

    public EntityBuilder CreateEntity()
    {
        var id = CreateEntityId();
        return new EntityBuilder(this, id);
    }

    public void AddEntity(Entity entity)
    {
        Debug.Assert(entity != null, "Entity cannot be null");
        Debug.Assert(!entity.Id.IsInvalid, "Entity ID cannot be invalid");
        Debug.Assert(!_entities.ContainsKey(entity.Id), $"Entity {entity.Id} already exists");

        _entities[entity.Id] = entity;
    }

    public Entity GetEntity(EntityId id)
    {
        Debug.Assert(!id.IsInvalid, "Entity ID cannot be invalid");
        Debug.Assert(_entities.ContainsKey(id), $"Entity {id} not found");

        return _entities[id];
    }

    public bool HasEntity(EntityId id)
    {
        return !id.IsInvalid && _entities.ContainsKey(id);
    }

    public void RemoveEntity(EntityId id)
    {
        Debug.Assert(!id.IsInvalid, "Entity ID cannot be invalid");
        Debug.Assert(_entities.ContainsKey(id), $"Cannot remove entity {id}: not found");

        _entities.Remove(id);
    }

    public int EntityCount => _entities.Count;

    public IReadOnlyDictionary<EntityId, Entity> Entities => _entities;

    public void AddResource<T>(T resource) where T : struct
    {
        _resources[typeof(T)] = resource;
    }

    public T GetResource<T>() where T : struct
    {
        if (!_resources.TryGetValue(typeof(T), out var obj))
        {
            throw new InvalidOperationException($"Resource {typeof(T).Name} not found.");
        }

        return (T)obj;
    }

    public void SetResource<T>(T resource) where T : struct
    {
        _resources[typeof(T)] = resource;
    }

    public bool HasResource<T>() where T : struct
    {
        return _resources.ContainsKey(typeof(T));
    }
}
