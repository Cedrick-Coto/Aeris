using Aeris.Engine;

var world = new World();
world.AddResource(new EngineStats());

var engine = new Engine(world);

Console.WriteLine("Aeris Engine - Sprint 1.1");
Console.WriteLine("========================");
Console.WriteLine();

for (int i = 0; i < 5; i++)
{
    engine.RunOneTick();
    var stats = world.GetResource<EngineStats>();
    Console.WriteLine($"Tick {stats.Tick}: {stats.TickDuration:F4}ms");
}

Console.WriteLine();
Console.WriteLine("Engine executed 5 ticks successfully.");
