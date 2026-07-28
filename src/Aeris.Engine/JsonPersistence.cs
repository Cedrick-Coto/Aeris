using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aeris.Engine;

public sealed class ComponentData
{
    public string TypeName { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;
}

public sealed class EntitySnapshot
{
    public uint Id { get; set; }
    public List<ComponentData> Components { get; set; } = new();
}

public sealed class ResourceData
{
    public string TypeName { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;
}

public sealed class WorldSnapshot
{
    public long Tick { get; set; }
    public double SimulationTime { get; set; }
    public uint NextEntityId { get; set; }
    public List<EntitySnapshot> Entities { get; set; } = new();
    public List<ResourceData> Resources { get; set; } = new();
}

public sealed class JsonPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true
    };

    private readonly string _saveDirectory;
    private int _checkpointTickInterval;
    private long _lastCheckpointTick;

    public JsonPersistence(string saveDirectory, int checkpointTickInterval = 1000)
    {
        _saveDirectory = saveDirectory;
        _checkpointTickInterval = checkpointTickInterval;
        Directory.CreateDirectory(_saveDirectory);
    }

    public int CheckpointTickInterval
    {
        get => _checkpointTickInterval;
        set => _checkpointTickInterval = value;
    }

    public void SaveWorld(World world, string? fileName = null)
    {
        var snapshot = CreateSnapshot(world);
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        var path = GetSavePath(fileName ?? "world.json");
        File.WriteAllText(path, json);
    }

    public void LoadWorld(World world, string? fileName = null)
    {
        var path = GetSavePath(fileName ?? "world.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Save file not found: {path}");

        var json = File.ReadAllText(path);
        var snapshot = JsonSerializer.Deserialize<WorldSnapshot>(json, JsonOptions);

        if (snapshot == null)
            throw new InvalidOperationException("Failed to deserialize world snapshot");

        ApplySnapshot(world, snapshot);
    }

    public bool ShouldCheckpoint(long currentTick)
    {
        return currentTick - _lastCheckpointTick >= _checkpointTickInterval;
    }

    public void RecordCheckpoint(long tick)
    {
        _lastCheckpointTick = tick;
    }

    public string GetCheckpointPath()
    {
        return GetSavePath($"checkpoint-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    public string[] GetSaveFiles()
    {
        if (!Directory.Exists(_saveDirectory))
            return Array.Empty<string>();

        return Directory.GetFiles(_saveDirectory, "*.json")
            .OrderBy(f => f)
            .ToArray();
    }

    public void DeleteSave(string fileName)
    {
        var path = GetSavePath(fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    private WorldSnapshot CreateSnapshot(World world)
    {
        var snapshot = new WorldSnapshot();

        if (world.HasResource<TimeResource>())
        {
            var time = world.GetResource<TimeResource>();
            snapshot.Tick = time.Tick;
            snapshot.SimulationTime = time.SimulationTime;
        }

        foreach (var kvp in world.Entities)
        {
            var entitySnapshot = new EntitySnapshot
            {
                Id = kvp.Value.Id.Value
            };

            foreach (var comp in kvp.Value.Components)
            {
                entitySnapshot.Components.Add(new ComponentData
                {
                    TypeName = comp.Key.AssemblyQualifiedName ?? comp.Key.FullName ?? comp.Key.Name,
                    JsonData = JsonSerializer.Serialize(comp.Value, comp.Key, JsonOptions)
                });
            }

            snapshot.Entities.Add(entitySnapshot);
        }

        var excludedTypes = new HashSet<Type>
        {
            typeof(TimeResource),
            typeof(EngineStats),
            typeof(EventBus),
            typeof(SchedulerResource)
        };

        foreach (var resource in world.GetResourceTypes())
        {
            if (excludedTypes.Contains(resource))
                continue;

            var value = world.GetResourceDynamic(resource);
            if (value == null) continue;

            snapshot.Resources.Add(new ResourceData
            {
                TypeName = resource.AssemblyQualifiedName ?? resource.FullName ?? resource.Name,
                JsonData = JsonSerializer.Serialize(value, resource, JsonOptions)
            });
        }

        return snapshot;
    }

    private void ApplySnapshot(World world, WorldSnapshot snapshot)
    {
        foreach (var entityId in world.Entities.Keys.ToList())
        {
            world.RemoveEntity(entityId);
        }

        foreach (var entitySnapshot in snapshot.Entities)
        {
            var id = new EntityId(entitySnapshot.Id);
            var entity = new Entity(id);

            foreach (var compData in entitySnapshot.Components)
            {
                var type = Type.GetType(compData.TypeName);
                if (type == null)
                    throw new InvalidOperationException($"Unknown component type: {compData.TypeName}");

                var component = JsonSerializer.Deserialize(compData.JsonData, type, JsonOptions);
                if (component != null)
                    entity.AddComponentDynamic(type, component);
            }

            world.AddEntity(entity);
        }

        var runtimeTypes = new HashSet<Type>
        {
            typeof(EventBus),
            typeof(SchedulerResource),
            typeof(EngineStats)
        };

        foreach (var resData in snapshot.Resources)
        {
            var type = Type.GetType(resData.TypeName);
            if (type == null)
                throw new InvalidOperationException($"Unknown resource type: {resData.TypeName}");

            if (runtimeTypes.Contains(type))
                continue;

            var resource = JsonSerializer.Deserialize(resData.JsonData, type, JsonOptions);
            if (resource != null)
                world.AddResourceDynamic(type, resource);
        }

        if (world.HasResource<TimeResource>())
        {
            var time = world.GetResource<TimeResource>();
            time.SetFromSnapshot(snapshot.Tick, snapshot.SimulationTime);
            world.SetResource(time);
        }
    }

    private string GetSavePath(string fileName)
    {
        return Path.Combine(_saveDirectory, fileName);
    }
}
