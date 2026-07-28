using System.Diagnostics;

namespace Aeris.Engine;

public sealed class Entity
{
    public EntityId Id { get; }
    private readonly Dictionary<Type, object> _components = new();

    public Entity(EntityId id)
    {
        Debug.Assert(!id.IsInvalid, "Entity cannot have invalid ID");
        Id = id;
    }

    public void AddComponent<T>(T component) where T : unmanaged
    {
        Debug.Assert(!_components.ContainsKey(typeof(T)), $"Component {typeof(T).Name} already exists on entity {Id}");
        _components[typeof(T)] = component;
    }

    public void SetComponent<T>(T component) where T : unmanaged
    {
        _components[typeof(T)] = component;
    }

    public T GetComponent<T>() where T : unmanaged
    {
        Debug.Assert(_components.ContainsKey(typeof(T)), $"Component {typeof(T).Name} not found on entity {Id}");
        return (T)_components[typeof(T)];
    }

    public bool HasComponent<T>() where T : unmanaged
    {
        return _components.ContainsKey(typeof(T));
    }

    public void RemoveComponent<T>() where T : unmanaged
    {
        Debug.Assert(_components.ContainsKey(typeof(T)), $"Cannot remove {typeof(T).Name}: not found on entity {Id}");
        _components.Remove(typeof(T));
    }

    internal void AddComponentDynamic(Type type, object component)
    {
        Debug.Assert(!_components.ContainsKey(type), $"Component {type.Name} already exists on entity {Id}");
        _components[type] = component;
    }

    public IReadOnlyDictionary<Type, object> Components => _components;
}
