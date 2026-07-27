using Aeris.Engine;
using FluentAssertions;
using FsCheck.Xunit;

namespace Aeris.Engine.Tests;

public class WorldPropertyTests
{
    [Property]
    public bool CreatedEntities_ShouldAlwaysHaveUniqueIds(int count)
    {
        count = Math.Clamp(count, 1, 1000);
        var world = new World();
        var ids = new HashSet<uint>();

        for (int i = 0; i < count; i++)
        {
            var entity = world.CreateEntity().Build();
            if (!ids.Add(entity.Id.Value)) return false;
        }

        return world.EntityCount == count;
    }

    [Property]
    public bool RemovedEntities_ShouldNotExist(int count)
    {
        count = Math.Clamp(count, 1, 100);
        var world = new World();
        var entities = new List<Entity>();

        for (int i = 0; i < count; i++)
        {
            entities.Add(world.CreateEntity().Build());
        }

        foreach (var entity in entities)
        {
            world.RemoveEntity(entity.Id);
        }

        return world.EntityCount == 0;
    }

    [Property]
    public bool Components_ShouldBeRetrievableAfterAdding(int count)
    {
        count = Math.Clamp(count, 1, 50);
        var world = new World();

        for (int i = 0; i < count; i++)
        {
            var entity = world.CreateEntity()
                .With(new TestComponent { Value = i })
                .Build();

            var comp = entity.GetComponent<TestComponent>();
            if (comp.Value != i) return false;
        }

        return true;
    }

    [Property]
    public bool AddAndRemoveComponents_ShouldMaintainConsistency(int count)
    {
        count = Math.Clamp(count, 1, 20);
        var world = new World();
        var entity = world.CreateEntity().Build();

        for (int i = 0; i < count; i++)
        {
            entity.AddComponent(new TestComponent { Value = i });
            if (!entity.HasComponent<TestComponent>()) return false;
            entity.RemoveComponent<TestComponent>();
            if (entity.HasComponent<TestComponent>()) return false;
        }

        return true;
    }
}
