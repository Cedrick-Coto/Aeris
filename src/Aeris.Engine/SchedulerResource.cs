using System.Diagnostics;

namespace Aeris.Engine;

public readonly struct ScheduledEvent
{
    public readonly double TriggerTime;
    public readonly Action<World> Callback;
    public readonly string Description;

    public ScheduledEvent(double triggerTime, Action<World> callback, string description)
    {
        TriggerTime = triggerTime;
        Callback = callback;
        Description = description;
    }
}

public sealed class SchedulerResource
{
    private readonly List<ScheduledEvent> _events = new();
    private bool _dirty;

    public void Schedule(double triggerTime, Action<World> callback, string description)
    {
        Debug.Assert(callback != null, "Callback cannot be null");
        Debug.Assert(!string.IsNullOrEmpty(description), "Description cannot be null or empty");

        _events.Add(new ScheduledEvent(triggerTime, callback, description));
        _dirty = true;
    }

    public void Process(World world, double currentTime)
    {
        if (_dirty)
        {
            _events.Sort((a, b) => a.TriggerTime.CompareTo(b.TriggerTime));
            _dirty = false;
        }

        while (_events.Count > 0 && _events[0].TriggerTime <= currentTime)
        {
            var evt = _events[0];
            _events.RemoveAt(0);

            evt.Callback(world);
        }
    }

    public int PendingCount => _events.Count;

    public bool HasPendingEvents => _events.Count > 0;

    public void Clear()
    {
        _events.Clear();
        _dirty = false;
    }
}
