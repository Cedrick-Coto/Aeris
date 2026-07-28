using System.Diagnostics;

namespace Aeris.Engine;

public sealed class SystemManager
{
    private readonly List<ISystem> _systems = new();
    private bool _isExecuting;
    private bool _isFrozen;

    public void Register(ISystem system)
    {
        Debug.Assert(system != null, "System cannot be null");
        Debug.Assert(!_isExecuting, "Cannot register systems during execution");
        Debug.Assert(!_isFrozen, "Cannot register systems after freeze");

        _systems.Add(system);
    }

    public void Freeze()
    {
        Debug.Assert(!_isExecuting, "Cannot freeze during execution");

        _isFrozen = true;
        Validate();
        SortSystems();
    }

    public void ExecuteAll(World world, float deltaTime)
    {
        Debug.Assert(_isFrozen, "Systems must be frozen before execution");

        _isExecuting = true;

        var stats = world.GetResource<EngineStats>();
        var systemDurations = new double[_systems.Count];

        for (int i = 0; i < _systems.Count; i++)
        {
            var system = _systems[i];
            var stopwatch = Stopwatch.StartNew();

            system.Execute(world, deltaTime);

            stopwatch.Stop();
            systemDurations[i] = stopwatch.Elapsed.TotalMilliseconds;
            stats.SystemsExecuted++;
        }

        stats.SystemsDuration = systemDurations;
        world.SetResource(stats);

        _isExecuting = false;
    }

    public IReadOnlyList<ISystem> Systems => _systems;

    private void Validate()
    {
        Debug.Assert(_systems.Count > 0, "At least one system must be registered");

        var names = new HashSet<string>();
        foreach (var system in _systems)
        {
            Debug.Assert(system != null, "Registered system cannot be null");
            Debug.Assert(!string.IsNullOrEmpty(system.Name), $"System name cannot be null or empty");
            Debug.Assert(names.Add(system.Name), $"Duplicate system name: {system.Name}");
        }
    }

    private void SortSystems()
    {
        _systems.Sort((a, b) =>
        {
            var phaseCompare = a.Phase.CompareTo(b.Phase);
            if (phaseCompare != 0) return phaseCompare;
            return a.Priority.CompareTo(b.Priority);
        });
    }
}
