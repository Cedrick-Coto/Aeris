using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public sealed class MemoryRetrievalTests
{
    [Fact]
    public void S001_RetrieveFamiliarLocation()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint memId = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData
        {
            Id = memId,
            Importance = 0.8f,
            Timestamp = 100f,
            Category = MemoryCategory.Environmental,
            InvolvedEntityId = 1
        });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            Content = "River",
            SourceEntity = new EntityId(1),
            Salience = 0.9f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id == $"retrieved_{memId}").ToList();
        retrieved.Should().HaveCount(1);
        retrieved[0].Salience.Should().BeGreaterThan(0.5f);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e => e.System == "MemoryRetrievalSystem");
    }

    [Fact]
    public void S002_NoRelevantMemory_ReturnsEmpty()
    {
        var (world, system, wm) = CreateWorld(out _, out var time);
        time.Advance(5f);
        system.Execute(world, 5f);

        wm.Chunks.Should().NotContain(c => c.Id.StartsWith("retrieved_"));
    }

    [Fact]
    public void S003_HighStressNarrowsRecall()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        var affect = world.GetResource<AffectState>();
        affect.Stress = 0.9f;
        affect.Threat = 0.7f;
        affect.Curiosity = 0.3f;
        world.SetResource(affect);

        uint id1 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id1, Importance = 0.9f, Timestamp = 50f, Category = MemoryCategory.Combat, InvolvedEntityId = 1 });
        uint id2 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id2, Importance = 0.4f, Timestamp = 50f, Category = MemoryCategory.Discovery, InvolvedEntityId = 1 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            SourceEntity = new EntityId(1),
            Salience = 0.8f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.RetrievalBudget = 2;
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_")).ToList();
        retrieved.Should().Contain(c => c.Id == $"retrieved_{id1}");
    }

    [Fact]
    public void S004_HighCuriosityBroadensRecall()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        var affect = world.GetResource<AffectState>();
        affect.Curiosity = 0.9f;
        affect.Stress = 0.1f;
        world.SetResource(affect);

        uint id1 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id1, Importance = 0.9f, Timestamp = 50f, Category = MemoryCategory.Combat, InvolvedEntityId = 1 });
        uint id2 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id2, Importance = 0.4f, Timestamp = 50f, Category = MemoryCategory.Discovery, InvolvedEntityId = 1 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            SourceEntity = new EntityId(1),
            Salience = 0.8f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.RetrievalBudget = 4;
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_")).ToList();
        retrieved.Should().Contain(c => c.Id == $"retrieved_{id1}");
        retrieved.Should().Contain(c => c.Id == $"retrieved_{id2}");
    }

    [Fact]
    public void S005_RecencyTiebreaker()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint id1 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id1, Importance = 0.6f, Timestamp = 50f, InvolvedEntityId = 1 });
        uint id2 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id2, Importance = 0.6f, Timestamp = 200f, InvolvedEntityId = 1 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            SourceEntity = new EntityId(1),
            Salience = 0.7f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.RetrievalBudget = 1;
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_")).ToList();
        retrieved.Should().HaveCount(1);
        retrieved[0].Id.Should().Be($"retrieved_{id2}");
    }

    [Fact]
    public void S006_CuedRetrievalByEntityId()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint id1 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id1, Importance = 0.8f, Timestamp = 100f, InvolvedEntityId = 42 });
        uint id2 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id2, Importance = 0.8f, Timestamp = 100f, InvolvedEntityId = 99 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_42",
            SourceEntity = new EntityId(42),
            Salience = 0.7f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_"))
            .OrderByDescending(c => c.Salience)
            .ToList();
        retrieved.Should().Contain(c => c.Id == $"retrieved_{id1}");
        retrieved.Should().Contain(c => c.Id == $"retrieved_{id2}");
        retrieved[0].Id.Should().Be($"retrieved_{id1}", "entity match (42) scores higher");
    }

    [Fact]
    public void S007_BudgetCap()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        for (int i = 0; i < 10; i++)
        {
            uint id = ltm.AllocateId();
            ltm.AddMemory(1, new MemoryData { Id = id, Importance = 0.7f, Timestamp = 100f + i });
        }

        time.Advance(5f);
        system.RetrievalBudget = 3;
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_")).ToList();
        retrieved.Count.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void S008_TraceLogging()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint id = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id, Importance = 0.8f, Timestamp = 100f, InvolvedEntityId = 1 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            SourceEntity = new EntityId(1),
            Salience = 0.7f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.Execute(world, 5f);

        var trace = world.GetResource<CognitiveTraceLog>();
        trace.Entries.Should().Contain(e =>
            e.System == "MemoryRetrievalSystem" &&
            e.OutputSummary.Contains("retrieved"));
    }

    [Fact]
    public void S009_NoSideEffects_LTMUnchanged()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint id = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id, Importance = 0.8f, Timestamp = 100f, InvolvedEntityId = 1 });

        string hashBefore = ComputeLtmHash(ltm);

        time.Advance(5f);
        system.Execute(world, 5f);

        string hashAfter = ComputeLtmHash(ltm);
        hashAfter.Should().Be(hashBefore);
    }

    [Fact]
    public void S010_Determinism()
    {
        string result1 = RunDeterministicSession();
        string result2 = RunDeterministicSession();

        result1.Should().Be(result2);
    }

    [Fact]
    public void Strategy_Replacement()
    {
        var (world, system, _) = CreateWorld(out _, out var time);
        var mock = new MockStrategy();
        system.Strategy = mock;

        time.Advance(5f);
        system.Execute(world, 5f);

        mock.WasCalled.Should().BeTrue();
    }

    private static string RunDeterministicSession()
    {
        var (world, system, wm) = CreateWorld(out var ltm, out var time);
        uint id1 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id1, Importance = 0.9f, Timestamp = 100f, InvolvedEntityId = 1 });
        uint id2 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id2, Importance = 0.5f, Timestamp = 200f, InvolvedEntityId = 2 });
        uint id3 = ltm.AllocateId();
        ltm.AddMemory(1, new MemoryData { Id = id3, Importance = 0.3f, Timestamp = 50f, InvolvedEntityId = 1 });

        wm.Chunks.Add(new WorkingMemoryChunk
        {
            Id = "percept_1",
            SourceEntity = new EntityId(1),
            Salience = 0.7f,
            FormationTick = 1,
            LastAccessTick = 1
        });

        time.Advance(5f);
        system.RetrievalBudget = 3;
        system.Execute(world, 5f);

        var retrieved = wm.Chunks.Where(c => c.Id.StartsWith("retrieved_"))
            .OrderBy(c => c.Id)
            .ToList();
        return string.Join(",", retrieved.Select(c => $"{c.Id}:{c.Salience:F4}"));
    }

    private static string ComputeLtmHash(MemoryStore store)
    {
        var parts = new List<string>();
        foreach (var kvp in store.All)
            foreach (var m in kvp.Value)
                parts.Add($"{m.Id}:{m.Importance}:{m.Timestamp}:{m.Forgotten}:{m.Category}:{m.InvolvedEntityId}");
        return string.Join("|", parts);
    }

    private static (World world, MemoryRetrievalSystem system, WorkingMemoryStore wm) CreateWorld(
        out MemoryStore ltm, out TimeResource time)
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.AddResource(new MemoryStore());
        world.AddResource(new WorkingMemoryStore());
        world.AddResource(AffectState.Default);
        world.AddResource(new CognitiveTraceLog());

        ltm = world.GetResource<MemoryStore>();
        time = world.GetResource<TimeResource>();

        var wm = world.GetResource<WorkingMemoryStore>();
        var system = new MemoryRetrievalSystem();
        return (world, system, wm);
    }

    private sealed class MockStrategy : IMemoryRetrievalStrategy
    {
        public bool WasCalled { get; private set; }
        public RetrievalResult Retrieve(MemoryRetrievalContext context)
        {
            WasCalled = true;
            return new RetrievalResult();
        }
    }
}
