namespace Aeris.Engine;

public interface ISystem
{
    string Name { get; }
    SystemPhase Phase { get; }
    int Priority { get; }

    void Execute(World world, float deltaTime);
}
