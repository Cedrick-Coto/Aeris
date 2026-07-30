namespace Aeris.Engine;

public sealed class CognitiveTraceLog
{
    public List<TraceEntry> Entries { get; set; } = new();
    public long Tick { get; set; }

    public void Record(string system, string input, string output, string why)
    {
        Entries.Add(new TraceEntry
        {
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
    }
}

public struct TraceEntry
{
    public string System;
    public long Tick;
    public string InputSummary;
    public string OutputSummary;
    public string Why;
}
