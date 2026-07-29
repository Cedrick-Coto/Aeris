using Aeris.Engine;

// Parse --seed for deterministic RNG (infrastructure for when randomness is added)
int? seed = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--seed" && i + 1 < args.Length)
        seed = int.Parse(args[i + 1]);
}

var world = new World();
world.AddResource(TimeResource.Create());
world.AddResource(new EngineStats());

var engine = new Engine(world);

if (seed.HasValue)
    Console.WriteLine($"Seed: {seed}");

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
