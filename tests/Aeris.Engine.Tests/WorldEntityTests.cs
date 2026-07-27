using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public struct TestComponent
{
    public int Value;
}

public struct OtherComponent
{
    public float X, Y;
}

public class WorldEntityTests
{
    [Fact]
    public void CreateEntity_ShouldReturnValidId()
    {
        var world = new World();

        var entity = world.CreateEntity().Build();

        entity.Id.Should().NotBe(EntityId.Invalid);
        world.HasEntity(entity.Id).Should().BeTrue();
    }

    [Fact]
    public void CreateMultipleEntities_ShouldHaveUniqueIds()
    {
        var world = new World();

        var e1 = world.CreateEntity().Build();
        var e2 = world.CreateEntity().Build();
        var e3 = world.CreateEntity().Build();

        e1.Id.Should().NotBe(e2.Id);
        e2.Id.Should().NotBe(e3.Id);
        e1.Id.Should().NotBe(e3.Id);
    }

    [Fact]
    public void RemoveEntity_ShouldRemoveFromWorld()
    {
        var world = new World();
        var entity = world.CreateEntity().Build();

        world.RemoveEntity(entity.Id);

        world.HasEntity(entity.Id).Should().BeFalse();
    }

    [Fact]
    public void AddComponent_ShouldBeRetrievable()
    {
        var world = new World();
        var entity = world.CreateEntity()
            .With(new TestComponent { Value = 42 })
            .Build();

        var comp = entity.GetComponent<TestComponent>();
        comp.Value.Should().Be(42);
    }

    [Fact]
    public void HasComponent_ShouldReturnTrueForExistingComponent()
    {
        var world = new World();
        var entity = world.CreateEntity()
            .With(new TestComponent { Value = 1 })
            .Build();

        entity.HasComponent<TestComponent>().Should().BeTrue();
        entity.HasComponent<OtherComponent>().Should().BeFalse();
    }

    [Fact]
    public void RemoveComponent_ShouldRemoveFromEntity()
    {
        var world = new World();
        var entity = world.CreateEntity()
            .With(new TestComponent { Value = 1 })
            .With(new OtherComponent { X = 1f, Y = 2f })
            .Build();

        entity.RemoveComponent<TestComponent>();

        entity.HasComponent<TestComponent>().Should().BeFalse();
        entity.HasComponent<OtherComponent>().Should().BeTrue();
    }

    [Fact]
    public void EntityCount_ShouldTrackEntities()
    {
        var world = new World();

        world.CreateEntity().Build();
        world.CreateEntity().Build();

        world.EntityCount.Should().Be(2);
    }

    [Fact]
    public void EntityBuilder_ShouldSupportMultipleComponents()
    {
        var world = new World();

        var entity = world.CreateEntity()
            .With(new TestComponent { Value = 10 })
            .With(new OtherComponent { X = 1.5f, Y = 2.5f })
            .Build();

        entity.HasComponent<TestComponent>().Should().BeTrue();
        entity.HasComponent<OtherComponent>().Should().BeTrue();
    }
}
