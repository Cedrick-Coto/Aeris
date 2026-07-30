namespace Aeris.Engine;

public sealed class CognitiveTraceLog
{
    public List<TraceEntry> Entries { get; set; } = new();
    public long Tick { get; set; }

    private long _nextTraceId;

    public void ResetTick()
    {
        _nextTraceId = 0;
    }

    public void Record(string system, string input, string output, string why)
    {
        long prevId = _nextTraceId;
        long traceId = ++_nextTraceId;

        Entries.Add(new TraceEntry
        {
            TraceId = traceId,
            ParentTraceId = prevId > 0 ? prevId : null,
            System = system,
            Tick = Tick,
            InputSummary = input,
            OutputSummary = output,
            Why = why
        });
    }

    public void Clear()
    {
        Entries.Clear();
        _nextTraceId = 0;
    }
}

public struct TraceEntry
{
    public long TraceId;
    public long? ParentTraceId;
    public string System;
    public long Tick;
    public string InputSummary;
    public string OutputSummary;
    public string Why;
}
