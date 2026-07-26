namespace Aeris.Engine;

public struct EngineStats
{
    public long Tick;
    public double TickDuration;
    public int EntityCount;
    public int ComponentCount;
    public int EventCount;
    public int SchedulerQueueSize;
    public int SystemsExecuted;
    public double[] SystemsDuration;
    public long AllocatedMemory;
}
