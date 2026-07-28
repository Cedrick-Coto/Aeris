using System.Diagnostics;

namespace Aeris.Engine;

public sealed class CounterSystem : ISystem
{
    public string Name => "Counter";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    public int ExecutionCount { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        ExecutionCount++;
        Debug.Assert(ExecutionCount > 0, "Counter overflow");
    }
}
