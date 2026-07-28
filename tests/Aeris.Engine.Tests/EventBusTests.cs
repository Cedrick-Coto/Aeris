using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public struct TestEvent
{
    public int Value;
    public string Message;
}

public struct AnotherTestEvent
{
    public float Amount;
}

public struct CounterEvent
{
    public int Tick;
}

public sealed class EventCounterSystem : ISystem
{
    public string Name => "EventCounter";
    public SystemPhase Phase => SystemPhase.Initialization;
    public int Priority => 0;

    public int EventsReceived { get; private set; }
    public int LastTick { get; private set; }

    public void Execute(World world, float deltaTime)
    {
        EventsReceived++;
    }

    public void OnCounterEvent(CounterEvent evt)
    {
        EventsReceived++;
        LastTick = evt.Tick;
    }
}

public class EventBusTests
{
    [Fact]
    public void Emit_Deferred_ShouldNotProcessImmediately()
    {
        var bus = new EventBus();
        var received = false;
        bus.Subscribe<TestEvent>(_ => received = true);

        bus.Emit(new TestEvent { Value = 42 });

        received.Should().BeFalse();
    }

    [Fact]
    public void Emit_Immediate_ShouldProcessImmediately()
    {
        var bus = new EventBus();
        var received = false;
        bus.Subscribe<TestEvent>(_ => received = true);

        bus.Emit(new TestEvent { Value = 42 }, EventDispatchType.Immediate);

        received.Should().BeTrue();
    }

    [Fact]
    public void Flush_ShouldProcessDeferredEvents()
    {
        var bus = new EventBus();
        var received = false;
        bus.Subscribe<TestEvent>(_ => received = true);

        bus.Emit(new TestEvent { Value = 42 });
        bus.AdvanceTick();
        bus.Flush();

        received.Should().BeTrue();
    }

    [Fact]
    public void Deferred_EventShouldNotProcessUntilNextTick()
    {
        var bus = new EventBus();
        var received = false;
        bus.Subscribe<TestEvent>(_ => received = true);

        bus.Emit(new TestEvent { Value = 1 });
        bus.AdvanceTick();
        bus.Flush();

        received.Should().BeTrue();
    }

    [Fact]
    public void Flush_ShouldDeliverCorrectEventData()
    {
        var bus = new EventBus();
        TestEvent? captured = null;
        bus.Subscribe<TestEvent>(e => captured = e);

        bus.Emit(new TestEvent { Value = 99, Message = "hello" });
        bus.AdvanceTick();
        bus.Flush();

        captured.Should().NotBeNull();
        captured!.Value.Value.Should().Be(99);
        captured.Value.Message.Should().Be("hello");
    }

    [Fact]
    public void Flush_ShouldProcessEventsInFIFOOrder()
    {
        var bus = new EventBus();
        var order = new List<int>();
        bus.Subscribe<TestEvent>(e => order.Add(e.Value));

        bus.Emit(new TestEvent { Value = 1 });
        bus.Emit(new TestEvent { Value = 2 });
        bus.Emit(new TestEvent { Value = 3 });
        bus.AdvanceTick();
        bus.Flush();

        order.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void MultipleSubscribers_ShouldAllReceiveEvent()
    {
        var bus = new EventBus();
        var count1 = 0;
        var count2 = 0;
        bus.Subscribe<TestEvent>(_ => count1++);
        bus.Subscribe<TestEvent>(_ => count2++);

        bus.Emit(new TestEvent { Value = 1 });
        bus.AdvanceTick();
        bus.Flush();

        count1.Should().Be(1);
        count2.Should().Be(1);
    }

    [Fact]
    public void DifferentEventTypes_ShouldBeDeliveredToCorrectHandlers()
    {
        var bus = new EventBus();
        TestEvent? testEvt = null;
        AnotherTestEvent? anotherEvt = null;
        bus.Subscribe<TestEvent>(e => testEvt = e);
        bus.Subscribe<AnotherTestEvent>(e => anotherEvt = e);

        bus.Emit(new TestEvent { Value = 42 });
        bus.Emit(new AnotherTestEvent { Amount = 3.14f });
        bus.AdvanceTick();
        bus.Flush();

        testEvt.Should().NotBeNull();
        testEvt!.Value.Value.Should().Be(42);
        anotherEvt.Should().NotBeNull();
        anotherEvt!.Value.Amount.Should().Be(3.14f);
    }

    [Fact]
    public void EventsEmittedDuringFlush_ShouldGoToNextTick()
    {
        var bus = new EventBus();
        var secondBatch = new List<int>();

        bus.Subscribe<CounterEvent>(e =>
        {
            secondBatch.Add(e.Tick);
            if (e.Tick == 1)
                bus.Emit(new CounterEvent { Tick = 99 });
        });

        bus.Emit(new CounterEvent { Tick = 1 });
        bus.AdvanceTick();
        bus.Flush();

        secondBatch.Should().HaveCount(1);
        secondBatch[0].Should().Be(1);

        bus.AdvanceTick();
        bus.Flush();

        secondBatch.Should().HaveCount(2);
        secondBatch[1].Should().Be(99);
    }

    [Fact]
    public void Clear_ShouldRemoveAllPendingEvents()
    {
        var bus = new EventBus();
        bus.Subscribe<TestEvent>(_ => { });

        bus.Emit(new TestEvent { Value = 1 });
        bus.Emit(new TestEvent { Value = 2 });
        bus.Clear();

        bus.HasPendingEvents.Should().BeFalse();
    }

    [Fact]
    public void HasPendingEvents_ShouldReflectQueueState()
    {
        var bus = new EventBus();

        bus.HasPendingEvents.Should().BeFalse();

        bus.Emit(new TestEvent { Value = 1 });
        bus.HasPendingEvents.Should().BeTrue();

        bus.AdvanceTick();
        bus.HasPendingEvents.Should().BeTrue();

        bus.Flush();
        bus.HasPendingEvents.Should().BeFalse();
    }

    [Fact]
    public void Overflow_ShouldDetectWhenLimitExceeded()
    {
        var bus = new EventBus(maxEventsPerTick: 5);
        bus.Subscribe<TestEvent>(_ => { });

        for (int i = 0; i < 5; i++)
            bus.Emit(new TestEvent { Value = i });

        bus.Emit(new TestEvent { Value = 99 });

        bus.OverflowDetected.Should().BeTrue();
    }

    [Fact]
    public void Overflow_ShouldDiscardExcessEvents()
    {
        var bus = new EventBus(maxEventsPerTick: 3);
        var processed = new List<int>();
        bus.Subscribe<TestEvent>(e => processed.Add(e.Value));

        for (int i = 0; i < 5; i++)
            bus.Emit(new TestEvent { Value = i });

        bus.AdvanceTick();
        bus.Flush();

        processed.Should().HaveCount(3);
        processed.Should().Equal(0, 1, 2);
    }

    [Fact]
    public void OverflowDetected_ShouldResetAfterAdvanceTick()
    {
        var bus = new EventBus(maxEventsPerTick: 2);
        bus.Subscribe<TestEvent>(_ => { });

        bus.Emit(new TestEvent { Value = 1 });
        bus.Emit(new TestEvent { Value = 2 });
        bus.Emit(new TestEvent { Value = 3 });

        bus.OverflowDetected.Should().BeTrue();

        bus.AdvanceTick();
        bus.OverflowDetected.Should().BeFalse();
    }

    [Fact]
    public void CurrentTick_ShouldTrackTickCount()
    {
        var bus = new EventBus();

        bus.CurrentTick.Should().Be(0);

        bus.AdvanceTick();
        bus.CurrentTick.Should().Be(1);

        bus.AdvanceTick();
        bus.CurrentTick.Should().Be(2);
    }

    [Fact]
    public void EventMetadata_ShouldContainCorrectTick()
    {
        var entry = new EventEntry(
            new TestEvent { Value = 10 },
            new EventMetadata(typeof(TestEvent), EventDispatchType.Deferred, 5));

        entry.Metadata.Tick.Should().Be(5);
        entry.Metadata.EventType.Should().Be(typeof(TestEvent));
        entry.Metadata.DispatchType.Should().Be(EventDispatchType.Deferred);
    }

    [Fact]
    public void Subscribe_ShouldNotThrow_WhenSubscribingMultipleTimes()
    {
        var bus = new EventBus();
        var count = 0;
        bus.Subscribe<TestEvent>(_ => count++);
        bus.Subscribe<TestEvent>(_ => count++);

        bus.Emit(new TestEvent { Value = 1 });
        bus.AdvanceTick();
        bus.Flush();

        count.Should().Be(2);
    }

    [Fact]
    public void Immediate_WithNoSubscribers_ShouldNotThrow()
    {
        var bus = new EventBus();

        var act = () => bus.Emit(new TestEvent { Value = 1 }, EventDispatchType.Immediate);

        act.Should().NotThrow();
    }

    [Fact]
    public void Deferred_WithNoSubscribers_ShouldNotThrow()
    {
        var bus = new EventBus();

        var act = () =>
        {
            bus.Emit(new TestEvent { Value = 1 });
            bus.AdvanceTick();
            bus.Flush();
        };

        act.Should().NotThrow();
    }
}
