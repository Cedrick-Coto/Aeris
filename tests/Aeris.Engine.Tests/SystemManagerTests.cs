using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class SystemManagerTests
{
    [Fact]
    public void Register_ShouldAddSystem()
    {
        var manager = new SystemManager();
        var system = new CounterSystem();

        manager.Register(system);

        manager.Systems.Should().Contain(system);
    }

    [Fact]
    public void Freeze_ShouldSortSystemsByPhase()
    {
        var manager = new SystemManager();
        var initSystem = new TestSystem("Init", SystemPhase.Initialization, 0);
        var actionSystem = new TestSystem("Action", SystemPhase.Action, 0);
        var perceptionSystem = new TestSystem("Perception", SystemPhase.Perception, 0);

        manager.Register(actionSystem);
        manager.Register(initSystem);
        manager.Register(perceptionSystem);
        manager.Freeze();

        manager.Systems[0].Name.Should().Be("Init");
        manager.Systems[1].Name.Should().Be("Perception");
        manager.Systems[2].Name.Should().Be("Action");
    }

    [Fact]
    public void Freeze_ShouldSortByPriorityWithinSamePhase()
    {
        var manager = new SystemManager();
        var systemA = new TestSystem("A", SystemPhase.Perception, 10);
        var systemB = new TestSystem("B", SystemPhase.Perception, 0);
        var systemC = new TestSystem("C", SystemPhase.Perception, 5);

        manager.Register(systemA);
        manager.Register(systemB);
        manager.Register(systemC);
        manager.Freeze();

        manager.Systems[0].Name.Should().Be("B");
        manager.Systems[1].Name.Should().Be("C");
        manager.Systems[2].Name.Should().Be("A");
    }

    [Fact]
    public void ExecuteAll_ShouldRunSystemsInOrder()
    {
        var manager = new SystemManager();
        var executionOrder = new List<string>();

        var systemA = new RecordingSystem("A", SystemPhase.Initialization, 0, executionOrder);
        var systemB = new RecordingSystem("B", SystemPhase.Initialization, 1, executionOrder);
        var systemC = new RecordingSystem("C", SystemPhase.Initialization, 2, executionOrder);

        manager.Register(systemC);
        manager.Register(systemA);
        manager.Register(systemB);
        manager.Freeze();

        var world = new World();
        world.AddResource(new EngineStats());

        manager.ExecuteAll(world, 0.016f);

        executionOrder.Should().Equal("A", "B", "C");
    }

    [Fact]
    public void ExecuteAll_ShouldUpdateStats()
    {
        var manager = new SystemManager();
        manager.Register(new CounterSystem());
        manager.Freeze();

        var world = new World();
        world.AddResource(new EngineStats());

        manager.ExecuteAll(world, 0.016f);

        var stats = world.GetResource<EngineStats>();
        stats.SystemsExecuted.Should().Be(1);
    }

    [Fact]
    public void ExecuteAll_ShouldThrowIfNotFrozen()
    {
        var manager = new SystemManager();
        manager.Register(new CounterSystem());

        var world = new World();
        world.AddResource(new EngineStats());

        var act = () => manager.ExecuteAll(world, 0.016f);

        act.Should().Throw<Exception>();
    }

    private class TestSystem : ISystem
    {
        public string Name { get; }
        public SystemPhase Phase { get; }
        public int Priority { get; }

        public TestSystem(string name, SystemPhase phase, int priority)
        {
            Name = name;
            Phase = phase;
            Priority = priority;
        }

        public void Execute(World world, float deltaTime) { }
    }

    private class RecordingSystem : ISystem
    {
        public string Name { get; }
        public SystemPhase Phase { get; }
        public int Priority { get; }
        private readonly List<string> _executionOrder;

        public RecordingSystem(string name, SystemPhase phase, int priority, List<string> executionOrder)
        {
            Name = name;
            Phase = phase;
            Priority = priority;
            _executionOrder = executionOrder;
        }

        public void Execute(World world, float deltaTime)
        {
            _executionOrder.Add(Name);
        }
    }
}
