namespace Aeris.Engine;

public sealed class RuleRegistry
{
    private readonly List<IAuditRule> _rules = new();

    public void Register(IAuditRule rule)
    {
        _rules.Add(rule);
    }

    public List<IAuditRule> GetRulesFor(string artifactType)
    {
        return _rules.Where(r =>
            r.SupportedArtifactTypes.Contains(artifactType)).ToList();
    }

    public List<IAuditRule> All => _rules;

    public void Clear()
    {
        _rules.Clear();
    }
}
