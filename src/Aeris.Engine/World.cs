namespace Aeris.Engine;

public sealed class World
{
    private readonly Dictionary<Type, object> _resources = new();

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
