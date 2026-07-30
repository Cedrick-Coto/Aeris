namespace Aeris.Engine;

public sealed class SalienceAttentionStrategy : IAttentionStrategy
{
    public string Name => "SalienceAttention";

    public List<Percept> Select(AttentionContext context)
    {
        var percepts = context.Percepts;
        var affect = context.Affect;
        int budget = context.Budget;

        for (int i = 0; i < percepts.Count; i++)
        {
            var p = percepts[i];
            float noveltyMod = 1f + affect.Novelty * 0.5f;
            float threatMod = p.Type == PerceptType.Aura ? affect.Threat * 0.3f : 0f;
            p.Salience = noveltyMod + threatMod + p.Confidence;
            percepts[i] = p;
        }

        percepts.Sort((a, b) => b.Salience.CompareTo(a.Salience));

        var attended = new List<Percept>();
        for (int i = 0; i < Math.Min(budget, percepts.Count); i++)
            attended.Add(percepts[i]);

        return attended;
    }
}
