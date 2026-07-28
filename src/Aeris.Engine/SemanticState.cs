namespace Aeris.Engine;

public sealed class SemanticState
{
    public SemanticIdentity Identity { get; init; } = new();
    public SemanticSituation Situation { get; init; } = new();
    public SemanticInternalState Internal { get; init; } = new();
    public SemanticWorldModel WorldModel { get; init; } = new();
    public SemanticAttention Attention { get; init; } = new();
    public SemanticWorkingMemory WorkingMemory { get; init; } = new();
    public SemanticLongTermMemory LongTermMemory { get; init; } = new();
    public SemanticSocialContext Social { get; init; } = new();
    public SemanticDirectives Directives { get; init; } = new();

    public int EstimatedTokens { get; set; }
    public double ExtractionTime { get; init; }
}

public sealed class SemanticIdentity
{
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public int AgeYears { get; init; }
    public string Personality { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string SelfPerception { get; init; } = string.Empty;
}

public sealed class SemanticSituation
{
    public string Location { get; init; } = string.Empty;
    public string TimeOfDay { get; init; } = string.Empty;
    public string Weather { get; init; } = string.Empty;
    public string Season { get; init; } = string.Empty;
    public List<SemanticNearbyEntity> NearbyEntities { get; init; } = new();
    public string CurrentActivity { get; init; } = string.Empty;
    public List<string> RecentEvents { get; init; } = new();
}

public sealed class SemanticNearbyEntity
{
    public string Description { get; init; } = string.Empty;
    public string Relationship { get; init; } = string.Empty;
    public string Distance { get; init; } = string.Empty;
}

public sealed class SemanticInternalState
{
    public string PrimaryEmotion { get; init; } = string.Empty;
    public string EmotionalReason { get; init; } = string.Empty;
    public List<SemanticGoal> ActiveGoals { get; init; } = new();
    public string GoalConflicts { get; init; } = string.Empty;
    public string PhysicalState { get; init; } = string.Empty;
    public string MentalState { get; init; } = string.Empty;
    public List<string> Motivations { get; init; } = new();
}

public sealed class SemanticGoal
{
    public string Description { get; init; } = string.Empty;
    public string Urgency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class SemanticWorldModel
{
    public List<SemanticKnownLocation> KnownLocations { get; init; } = new();
    public List<SemanticKnownEntity> KnownEntities { get; init; } = new();
    public List<SemanticBelief> Beliefs { get; init; } = new();
    public List<SemanticKnowledge> Knowledge { get; init; } = new();
    public List<string> Uncertainties { get; init; } = new();
    public List<string> Predictions { get; init; } = new();
    public List<string> Threats { get; init; } = new();
}

public sealed class SemanticKnownLocation
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Significance { get; init; } = string.Empty;
}

public sealed class SemanticKnownEntity
{
    public string Description { get; init; } = string.Empty;
    public string Significance { get; init; } = string.Empty;
}

public sealed class SemanticBelief
{
    public string Statement { get; init; } = string.Empty;
    public string Confidence { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}

public sealed class SemanticKnowledge
{
    public string What { get; init; } = string.Empty;
    public string Certainty { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}

public sealed class SemanticAttention
{
    public string PrimaryFocus { get; init; } = string.Empty;
    public string FocusIntensity { get; init; } = string.Empty;
    public List<string> DistractingFactors { get; init; } = new();
    public string PerceptualRange { get; init; } = string.Empty;
    public string FilterBias { get; init; } = string.Empty;
}

public sealed class SemanticWorkingMemory
{
    public List<string> ActiveThoughts { get; init; } = new();
    public List<string> PendingQuestions { get; init; } = new();
    public List<SemanticConversationEntry> RecentConversations { get; init; } = new();
    public List<string> ImmediateConcerns { get; init; } = new();
    public List<string> ContextualTriggers { get; init; } = new();
}

public sealed class SemanticConversationEntry
{
    public string Speaker { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Timeframe { get; init; } = string.Empty;
}

public sealed class SemanticLongTermMemory
{
    public List<SemanticMemoryEntry> Memories { get; init; } = new();
    public List<string> RecurringThoughts { get; init; } = new();
    public List<string> KeyEvents { get; init; } = new();
    public List<string> EmotionalAnchors { get; init; } = new();
}

public sealed class SemanticMemoryEntry
{
    public string Description { get; init; } = string.Empty;
    public string EmotionalImpact { get; init; } = string.Empty;
    public string Certainty { get; init; } = string.Empty;
    public string RelevanceToNow { get; init; } = string.Empty;
    public string Timeframe { get; init; } = string.Empty;
}

public sealed class SemanticSocialContext
{
    public List<SemanticRelationship> Relationships { get; init; } = new();
    public string SocialSituation { get; init; } = string.Empty;
    public string SocialTension { get; init; } = string.Empty;
    public string Reputation { get; init; } = string.Empty;
}

public sealed class SemanticRelationship
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TrustLevel { get; init; } = string.Empty;
    public string RecentInteraction { get; init; } = string.Empty;
    public string CurrentFeeling { get; init; } = string.Empty;
    public List<string> OpenQuestions { get; init; } = new();
}

public sealed class SemanticDirectives
{
    public List<string> MustInclude { get; init; } = new();
    public List<string> MustExclude { get; init; } = new();
    public string Tone { get; init; } = string.Empty;
    public string Pacing { get; init; } = string.Empty;
    public float SuspenseLevel { get; init; }
}
