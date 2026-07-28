using System.Text.Json;

namespace Aeris.Engine;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
}

public static class SemanticValidator
{
    private static readonly HashSet<string> EcsLeakPatterns = new()
    {
        "EntityId",
        "Entity(",
        "Arch.",
        "Component",
        "MemoryStore",
        "BeliefStore",
        "KnowledgeStore",
        "GoalStore",
        "RelationshipStore",
        "EmotionStore",
        "AttentionStore",
        "EmotionComponent",
        "AttentionComponent",
        "MemoryData",
        "BeliefData",
        "KnowledgeData",
        "GoalData",
        "RelationshipData"
    };

    public static ValidationResult Validate(SemanticState state, ExtractionOptions? options = null)
    {
        var result = new ValidationResult();

        if (state == null)
        {
            result.Errors.Add("SemanticState is null");
            return result;
        }

        ValidateNoEcsLeaks(state, result);
        ValidateNoEntityIds(state, result);

        if (options != null)
        {
            ValidateTokenBudget(state, options, result);
        }

        ValidateStructure(state, result);

        return result;
    }

    public static ValidationResult ValidateDeterministic(
        Func<SemanticState> extract1,
        Func<SemanticState> extract2)
    {
        var result = new ValidationResult();

        var state1 = extract1();
        var state2 = extract2();

        var opts = new JsonSerializerOptions { IncludeFields = true };
        var json1 = JsonSerializer.Serialize(state1, opts);
        var json2 = JsonSerializer.Serialize(state2, opts);

        if (json1 != json2)
        {
            result.Errors.Add("Non-deterministic extraction: JSON outputs differ");
        }

        if (state1.EstimatedTokens != state2.EstimatedTokens)
        {
            result.Errors.Add($"Non-deterministic token estimate: {state1.EstimatedTokens} vs {state2.EstimatedTokens}");
        }

        if (state1.Situation?.TimeOfDay != state2.Situation?.TimeOfDay)
        {
            result.Errors.Add($"Non-deterministic time: {state1.Situation?.TimeOfDay} vs {state2.Situation?.TimeOfDay}");
        }

        return result;
    }

    public static ValidationResult ValidateSerializable(SemanticState state)
    {
        var result = new ValidationResult();

        try
        {
            var opts = new JsonSerializerOptions { IncludeFields = true };
            var json = JsonSerializer.Serialize(state, opts);
            var deserialized = JsonSerializer.Deserialize<SemanticState>(json, opts);

            if (deserialized == null)
            {
                result.Errors.Add("Deserialization returned null");
                return result;
            }

            if (state.EstimatedTokens != deserialized.EstimatedTokens)
            {
                result.Errors.Add($"EstimatedTokens mismatch: {state.EstimatedTokens} vs {deserialized.EstimatedTokens}");
            }

            if (state.ExtractionTime != deserialized.ExtractionTime)
            {
                result.Errors.Add($"ExtractionTime mismatch: {state.ExtractionTime} vs {deserialized.ExtractionTime}");
            }

            if (state.Situation?.TimeOfDay != deserialized.Situation?.TimeOfDay)
            {
                result.Errors.Add($"Situation.TimeOfDay mismatch");
            }

            if (state.Internal?.PrimaryEmotion != deserialized.Internal?.PrimaryEmotion)
            {
                result.Errors.Add($"Internal.PrimaryEmotion mismatch");
            }

            if (state.LongTermMemory?.Memories?.Count != deserialized.LongTermMemory?.Memories?.Count)
            {
                result.Errors.Add($"LongTermMemory.Memories.Count mismatch");
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Serialization failed: {ex.Message}");
        }

        return result;
    }

    private static void ValidateNoEcsLeaks(SemanticState state, ValidationResult result)
    {
        var json = JsonSerializer.Serialize(state);

        foreach (var pattern in EcsLeakPatterns)
        {
            if (json.Contains(pattern, StringComparison.Ordinal))
            {
                result.Errors.Add($"ECS leak detected: found '{pattern}' in serialized output");
            }
        }
    }

    private static void ValidateNoEntityIds(SemanticState state, ValidationResult result)
    {
        var json = JsonSerializer.Serialize(state);

        if (json.Contains("EntityId", StringComparison.Ordinal))
        {
            result.Errors.Add("EntityId found in output");
        }

        if (json.Contains("\"Value\":", StringComparison.Ordinal))
        {
            result.Errors.Add("EntityId.Value wrapper found in output");
        }
    }

    private static void ValidateTokenBudget(SemanticState state, ExtractionOptions options, ValidationResult result)
    {
        if (options.EnableBudgetTrim && state.EstimatedTokens > options.MaxTokens)
        {
            result.Warnings.Add(
                $"Token budget exceeded: {state.EstimatedTokens} > {options.MaxTokens}");
        }
    }

    private static void ValidateStructure(SemanticState state, ValidationResult result)
    {
        if (state.Identity == null)
            result.Errors.Add("Identity is null");
        if (state.Situation == null)
            result.Errors.Add("Situation is null");
        if (state.Internal == null)
            result.Errors.Add("Internal is null");
        if (state.WorldModel == null)
            result.Errors.Add("WorldModel is null");
        if (state.Attention == null)
            result.Errors.Add("Attention is null");
        if (state.WorkingMemory == null)
            result.Errors.Add("WorkingMemory is null");
        if (state.LongTermMemory == null)
            result.Errors.Add("LongTermMemory is null");
        if (state.Social == null)
            result.Errors.Add("Social is null");
        if (state.Directives == null)
            result.Errors.Add("Directives is null");

        if (state.ExtractionTime < 0)
            result.Errors.Add("ExtractionTime is negative");
        if (state.EstimatedTokens < 0)
            result.Errors.Add("EstimatedTokens is negative");
    }
}
