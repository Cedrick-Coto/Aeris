using System.Diagnostics;

namespace Aeris.Engine;

public enum EventDispatchType
{
    Deferred,
    Immediate
}

public readonly struct EventMetadata
{
    public readonly Type EventType;
    public readonly EventDispatchType DispatchType;
    public readonly long Tick;

    public EventMetadata(Type eventType, EventDispatchType dispatchType, long tick)
    {
        EventType = eventType;
        DispatchType = dispatchType;
        Tick = tick;
    }
}

public readonly struct EventEntry
{
    public readonly object Data;
    public readonly EventMetadata Metadata;

    public EventEntry(object data, EventMetadata metadata)
    {
        Data = data;
        Metadata = metadata;
    }
}

public sealed class EventBus
{
    public const int DefaultMaxEventsPerTick = 10_000;

    private readonly Dictionary<Type, List<Action<object>>> _subscribers = new();
    private Queue<EventEntry> _currentQueue;
    private Queue<EventEntry> _nextQueue;
    private readonly int _maxEventsPerTick;
    private long _currentTick;
    private bool _overflowDetected;

    public EventBus(int maxEventsPerTick = DefaultMaxEventsPerTick)
    {
        _maxEventsPerTick = maxEventsPerTick;
        _currentQueue = new Queue<EventEntry>();
        _nextQueue = new Queue<EventEntry>();
    }

    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Debug.Assert(handler != null, "Handler cannot be null");

        var type = typeof(T);
        if (!_subscribers.TryGetValue(type, out var handlers))
        {
            handlers = new List<Action<object>>();
            _subscribers[type] = handlers;
        }

        handlers.Add(evt => handler((T)evt));
    }

    public void Emit<T>(T evt, EventDispatchType dispatchType = EventDispatchType.Deferred) where T : struct
    {
        if (_nextQueue.Count >= _maxEventsPerTick)
        {
            _overflowDetected = true;
            return;
        }

        var entry = new EventEntry(
            evt,
            new EventMetadata(typeof(T), dispatchType, _currentTick));

        if (dispatchType == EventDispatchType.Immediate)
        {
            ProcessImmediate(entry);
        }
        else
        {
            _nextQueue.Enqueue(entry);
        }
    }

    public void AdvanceTick()
    {
        (_currentQueue, _nextQueue) = (_nextQueue, _currentQueue);
        _currentTick++;
        _overflowDetected = false;
    }

    public void Flush()
    {
        while (_currentQueue.Count > 0)
        {
            var entry = _currentQueue.Dequeue();

            if (_subscribers.TryGetValue(entry.Metadata.EventType, out var handlers))
            {
                foreach (var handler in handlers)
                    handler(entry.Data);
            }
        }
    }

    public bool HasPendingEvents => _currentQueue.Count > 0 || _nextQueue.Count > 0;
    public bool OverflowDetected => _overflowDetected;
    public long CurrentTick => _currentTick;

    public void Clear()
    {
        _currentQueue.Clear();
        _nextQueue.Clear();
        _overflowDetected = false;
    }

    private void ProcessImmediate(EventEntry entry)
    {
        if (_subscribers.TryGetValue(entry.Metadata.EventType, out var handlers))
        {
            foreach (var handler in handlers)
                handler(entry.Data);
        }
    }
}
