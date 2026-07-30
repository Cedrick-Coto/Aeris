namespace Aeris.Engine;

public sealed class RandomAttentionStrategy : IAttentionStrategy
{
    public string Name => "RandomAttention";

    public List<Percept> Select(AttentionContext context)
    {
        var percepts = context.Percepts;
        int budget = context.Budget;

        for (int i = 0; i < percepts.Count; i++)
        {
            var p = percepts[i];
            p.Salience = DeterministicHash(p);
            percepts[i] = p;
        }

        percepts.Sort((a, b) => b.Salience.CompareTo(a.Salience));

        var attended = new List<Percept>();
        for (int i = 0; i < Math.Min(budget, percepts.Count); i++)
            attended.Add(percepts[i]);

        return attended;
    }

    private static float DeterministicHash(Percept p)
    {
        int hash = p.Type.GetHashCode();
        hash = hash * 31 + (int)(p.LabelId * 100);
        hash = hash * 31 + (int)(p.Confidence * 100);
        return (float)((hash & 0x7FFFFFFF) % 1000) / 1000f;
    }
}
