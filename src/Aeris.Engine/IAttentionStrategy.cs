namespace Aeris.Engine;

public sealed class AttentionContext
{
    public List<Percept> Percepts { get; init; } = new();
    public AffectState Affect { get; init; }
    public int Budget { get; init; }
}

public interface IAttentionStrategy
{
    string Name { get; }
    List<Percept> Select(AttentionContext context);
}
