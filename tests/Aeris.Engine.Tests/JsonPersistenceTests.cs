using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public struct PersistTestComponent
{
    public int Value;
    public float Speed;
}

public struct PersistNameComponent
{
    public int Length;
}

public class JsonPersistenceTests : IDisposable
{
    private readonly string _testDir;

    public JsonPersistenceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"aeris-test-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void SaveWorld_ShouldCreateFile()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        File.Exists(Path.Combine(_testDir, "world.json")).Should().BeTrue();
    }

    [Fact]
    public void SaveWorld_ShouldSerializeEntities()
    {
        var world = CreateWorld();
        world.CreateEntity().With(new PersistTestComponent { Value = 42, Speed = 1.5f }).Build();
        world.CreateEntity().With(new PersistNameComponent { Length = 5 }).Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var json = File.ReadAllText(Path.Combine(_testDir, "world.json"));
        json.Should().Contain("value");
        json.Should().Contain("42");
    }

    [Fact]
    public void LoadWorld_ShouldRestoreEntities()
    {
        var world = CreateWorld();
        world.CreateEntity().With(new PersistTestComponent { Value = 42, Speed = 1.5f }).Build();
        world.CreateEntity().With(new PersistNameComponent { Length = 5 }).Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(2);
    }

    [Fact]
    public void LoadWorld_ShouldRestoreComponentData()
    {
        var world = CreateWorld();
        world.CreateEntity().With(new PersistTestComponent { Value = 42, Speed = 1.5f }).Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        var entities = loadedWorld.Entities.Values.ToList();
        entities.Should().HaveCount(1);

        var entity = entities[0];
        entity.HasComponent<PersistTestComponent>().Should().BeTrue();
        var comp = entity.GetComponent<PersistTestComponent>();
        comp.Value.Should().Be(42);
        comp.Speed.Should().Be(1.5f);
    }

    [Fact]
    public void LoadWorld_ShouldRestoreTimeResource()
    {
        var world = CreateWorld();
        var time = world.GetResource<TimeResource>();
        time.Advance(5.0f);
        time.Advance(3.0f);
        world.SetResource(time);
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        var loadedTime = loadedWorld.GetResource<TimeResource>();
        loadedTime.Tick.Should().Be(2);
        loadedTime.SimulationTime.Should().Be(8.0);
    }

    [Fact]
    public void RoundTrip_MultipleEntities_ShouldPreserveAll()
    {
        var world = CreateWorld();
        for (int i = 0; i < 10; i++)
        {
            world.CreateEntity().With(new PersistTestComponent { Value = i, Speed = i * 0.1f }).Build();
        }
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(10);

        var values = loadedWorld.Entities.Values
            .OrderBy(e => e.GetComponent<PersistTestComponent>().Value)
            .Select(e => e.GetComponent<PersistTestComponent>().Value)
            .ToList();

        values.Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
    }

    [Fact]
    public void RoundTrip_MultipleComponents_ShouldPreserveAll()
    {
        var world = CreateWorld();
        world.CreateEntity()
            .With(new PersistTestComponent { Value = 10, Speed = 2.0f })
            .With(new PersistNameComponent { Length = 5 })
            .Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        var entity = loadedWorld.Entities.Values.First();
        entity.HasComponent<PersistTestComponent>().Should().BeTrue();
        entity.HasComponent<PersistNameComponent>().Should().BeTrue();
        entity.GetComponent<PersistNameComponent>().Length.Should().Be(5);
    }

    [Fact]
    public void LoadWorld_ShouldThrowIfFileNotFound()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        var act = () => persistence.LoadWorld(world);

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetSaveFiles_ShouldReturnSavedFiles()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world, "save1.json");
        persistence.SaveWorld(world, "save2.json");

        var files = persistence.GetSaveFiles();
        files.Should().HaveCount(2);
    }

    [Fact]
    public void DeleteSave_ShouldRemoveFile()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world, "to-delete.json");
        persistence.DeleteSave("to-delete.json");

        File.Exists(Path.Combine(_testDir, "to-delete.json")).Should().BeFalse();
    }

    [Fact]
    public void ShouldCheckpoint_ShouldReturnTrueAfterInterval()
    {
        var persistence = new JsonPersistence(_testDir, checkpointTickInterval: 100);

        persistence.ShouldCheckpoint(0).Should().BeFalse();
        persistence.ShouldCheckpoint(50).Should().BeFalse();
        persistence.ShouldCheckpoint(100).Should().BeTrue();
    }

    [Fact]
    public void ShouldCheckpoint_ShouldResetAfterRecording()
    {
        var persistence = new JsonPersistence(_testDir, checkpointTickInterval: 100);

        persistence.ShouldCheckpoint(100).Should().BeTrue();
        persistence.RecordCheckpoint(100);
        persistence.ShouldCheckpoint(150).Should().BeFalse();
        persistence.ShouldCheckpoint(200).Should().BeTrue();
    }

    [Fact]
    public void SaveWorld_ShouldPreserveEntityIds()
    {
        var world = CreateWorld();
        var e1 = world.CreateEntity().With(new PersistTestComponent { Value = 1 }).Build();
        var e2 = world.CreateEntity().With(new PersistTestComponent { Value = 2 }).Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        var ids = loadedWorld.Entities.Keys.Select(k => k.Value).OrderBy(x => x).ToList();
        ids.Should().Equal(e1.Id.Value, e2.Id.Value);
    }

    [Fact]
    public void LoadWorld_ShouldOverwriteExistingEntities()
    {
        var world = CreateWorld();
        world.CreateEntity().With(new PersistTestComponent { Value = 99 }).Build();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        loadedWorld.CreateEntity().With(new PersistTestComponent { Value = 1 }).Build();

        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(1);
        loadedWorld.Entities.Values.First().GetComponent<PersistTestComponent>().Value.Should().Be(99);
    }

    [Fact]
    public void SaveWorld_WithEmptyWorld_ShouldWork()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        var act = () => persistence.SaveWorld(world);

        act.Should().NotThrow();
    }

    [Fact]
    public void LoadWorld_ShouldRestoreEmptyWorld()
    {
        var world = CreateWorld();
        var persistence = new JsonPersistence(_testDir);

        persistence.SaveWorld(world);

        var loadedWorld = CreateWorld();
        persistence.LoadWorld(loadedWorld);

        loadedWorld.EntityCount.Should().Be(0);
    }

    private static World CreateWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        return world;
    }
}
