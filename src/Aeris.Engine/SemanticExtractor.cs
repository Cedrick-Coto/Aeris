using System.Diagnostics;
using System.Text;

namespace Aeris.Engine;

public class SemanticExtractor
{
    public ExtractionOptions Options { get; }

    public SemanticExtractor(ExtractionOptions? options = null)
    {
        Options = options ?? new ExtractionOptions();
    }

    public SemanticState Extract(ExtractionContext context)
    {
        var sw = Stopwatch.StartNew();

        var identity = ExtractIdentity(context);
        var situation = ExtractSituation(context);
        var internalState = ExtractInternalState(context);
        var worldModel = ExtractWorldModel(context);
        var attention = ExtractAttention(context);
        var workingMemory = ExtractWorkingMemory(context);
        var longTermMemory = ExtractLongTermMemory(context);
        var social = ExtractSocial(context);

        sw.Stop();

        var state = new SemanticState
        {
            Identity = identity,
            Situation = situation,
            Internal = internalState,
            WorldModel = worldModel,
            Attention = attention,
            WorkingMemory = workingMemory,
            LongTermMemory = longTermMemory,
            Social = social,
            Directives = new SemanticDirectives(),
            ExtractionTime = sw.Elapsed.TotalSeconds
        };

        state.EstimatedTokens = EstimateTokens(state);

        if (Options.EnableBudgetTrim && state.EstimatedTokens > Options.MaxTokens)
        {
            ApplyBudgetTrim(state);
        }

        return state;
    }

    public SemanticSnapshot ExtractSnapshot(ExtractionContext context)
    {
        var state = Extract(context);
        return new SemanticSnapshot
        {
            State = state,
            WorldTick = context.World.GetResource<TimeResource>().Tick,
            SimulationTime = context.World.GetResource<TimeResource>().SimulationTime,
            EntityCount = context.World.EntityCount
        };
    }

    private string Name(uint entityId, ExtractionContext ctx)
    {
        return ctx.EntityNames.TryGetValue(entityId, out var name) ? name : $"Entidad-{entityId}";
    }

    private uint AgentId(ExtractionContext ctx) => ctx.Agent.Id.Value;

    private SemanticIdentity ExtractIdentity(ExtractionContext ctx)
    {
        return new SemanticIdentity();
    }

    private SemanticSituation ExtractSituation(ExtractionContext ctx)
    {
        var time = ctx.World.GetResource<TimeResource>();
        var timeOfDay = ClassifyTimeOfDay(time.SimulationTime);
        var season = ClassifySeason(time.SimulationTime);

        var nearby = new List<SemanticNearbyEntity>();
        if (ctx.World.HasResource<AttentionStore>())
        {
            var attentionStore = ctx.World.GetResource<AttentionStore>();
            if (attentionStore.TryGetNearby(AgentId(ctx), out var nearbyIds))
            {
                var count = Math.Min(nearbyIds.Count, Options.MaxEntities);
                for (int i = 0; i < count; i++)
                {
                    nearby.Add(new SemanticNearbyEntity
                    {
                        Description = Name(nearbyIds[i], ctx),
                        Relationship = string.Empty,
                        Distance = string.Empty
                    });
                }
            }
        }

        var nearbyCount = ctx.World.EntityCount - 1;
        var activity = nearbyCount > 0
            ? $"En presencia de {nearbyCount} entidades"
            : "En solitario";

        return new SemanticSituation
        {
            TimeOfDay = timeOfDay,
            Season = season,
            NearbyEntities = nearby,
            CurrentActivity = activity
        };
    }

    private string ClassifyTimeOfDay(double simulationTime)
    {
        var hour = (simulationTime % 86400.0) / 3600.0;
        return hour switch
        {
            >= 6 and < 12 => "Mañana",
            >= 12 and < 18 => "Tarde",
            >= 18 and < 21 => "Atardecer",
            _ => "Noche"
        };
    }

    private string ClassifySeason(double simulationTime)
    {
        var dayOfYear = (int)((simulationTime / 86400.0) % 365.0);
        return dayOfYear switch
        {
            < 90 => "Primavera",
            < 180 => "Verano",
            < 270 => "Otoño",
            _ => "Invierno"
        };
    }

    private SemanticInternalState ExtractInternalState(ExtractionContext ctx)
    {
        var goals = new List<SemanticGoal>();
        if (ctx.World.HasResource<GoalStore>())
        {
            var goalStore = ctx.World.GetResource<GoalStore>();
            if (goalStore.TryGetGoals(AgentId(ctx), out var agentGoals))
            {
                var activeGoals = agentGoals
                    .Where(g => g.IsActive)
                    .OrderByDescending(g => g.EffectivePriority((float)ctx.World.GetResource<TimeResource>().SimulationTime))
                    .Take(Options.MaxMemories);

                foreach (var g in activeGoals)
                {
                    goals.Add(new SemanticGoal
                    {
                        Description = $"Objetivo-{g.Id}",
                        Urgency = g.Urgency > 0.7f ? "Alta" : g.Urgency > 0.3f ? "Media" : "Baja",
                        Status = "Activo"
                    });
                }
            }
        }

        var motivations = new List<string>();
        if (goals.Count > 0)
        {
            motivations.Add($"Perseguir {goals.Count} objetivos activos");
        }

        return new SemanticInternalState
        {
            ActiveGoals = goals,
            Motivations = motivations
        };
    }

    private SemanticWorldModel ExtractWorldModel(ExtractionContext ctx)
    {
        if (!Options.IncludeWorldModel)
            return new SemanticWorldModel();

        var entities = new List<SemanticKnownEntity>();
        foreach (var kvp in ctx.World.Entities)
        {
            if (kvp.Key == ctx.Agent.Id) continue;
            entities.Add(new SemanticKnownEntity
            {
                Description = Name(kvp.Key.Value, ctx),
                Significance = string.Empty
            });
        }

        var beliefs = new List<SemanticBelief>();
        if (ctx.World.HasResource<BeliefStore>())
        {
            var beliefStore = ctx.World.GetResource<BeliefStore>();
            if (beliefStore.TryGetBeliefs(AgentId(ctx), out var agentBeliefs))
            {
                foreach (var b in agentBeliefs.Where(b => b.IsActive))
                {
                    beliefs.Add(new SemanticBelief
                    {
                        Statement = $"Creencia-{b.Id}",
                        Confidence = b.Confidence > 0.7f ? "Alta" : b.Confidence > 0.3f ? "Media" : "Baja",
                        Source = b.Source.ToString()
                    });
                }
            }
        }

        var knowledge = new List<SemanticKnowledge>();
        if (ctx.World.HasResource<KnowledgeStore>())
        {
            var knowledgeStore = ctx.World.GetResource<KnowledgeStore>();
            if (knowledgeStore.TryGetKnowledge(AgentId(ctx), out var agentKnowledge))
            {
                foreach (var k in agentKnowledge)
                {
                    knowledge.Add(new SemanticKnowledge
                    {
                        What = $"Conocimiento-{k.Id}",
                        Certainty = k.Certainty.ToString(),
                        Source = k.Source.ToString()
                    });
                }
            }
        }

        return new SemanticWorldModel
        {
            KnownEntities = entities,
            Beliefs = beliefs,
            Knowledge = knowledge
        };
    }

    private SemanticAttention ExtractAttention(ExtractionContext ctx)
    {
        if (!ctx.Agent.HasComponent<AttentionComponent>())
            return new SemanticAttention
            {
                PrimaryFocus = "Ninguno"
            };

        var ac = ctx.Agent.GetComponent<AttentionComponent>();
        var focus = ac.HasFocus ? Name(ac.FocusTargetId, ctx) : "Ninguno";
        var intensity = ac.FocusIntensity > 0.8f ? "Intensa"
            : ac.FocusIntensity > 0.4f ? "Moderada"
            : "Débil";

        var range = ac.PerceptualRange > 10f ? "Amplio"
            : ac.PerceptualRange > 3f ? "Normal"
            : "Cercano";

        return new SemanticAttention
        {
            PrimaryFocus = focus,
            FocusIntensity = intensity,
            PerceptualRange = range
        };
    }

    private SemanticWorkingMemory ExtractWorkingMemory(ExtractionContext ctx)
    {
        var thoughts = new List<string>();
        var concerns = new List<string>();

        if (ctx.World.HasResource<EmotionStore>())
        {
            var emotionStore = ctx.World.GetResource<EmotionStore>();
            if (emotionStore.TryGet(AgentId(ctx), out var emotion) && emotion.HasEmotion)
            {
                thoughts.Add($"Emoción activa: {emotion.Primary}");
                if (emotion.Intensity > 0.7f)
                    concerns.Add($"Emoción intensa ({emotion.Intensity:F2})");
            }
        }

        return new SemanticWorkingMemory
        {
            ActiveThoughts = thoughts,
            ImmediateConcerns = concerns
        };
    }

    private SemanticLongTermMemory ExtractLongTermMemory(ExtractionContext ctx)
    {
        var memories = new List<SemanticMemoryEntry>();

        if (ctx.World.HasResource<MemoryStore>())
        {
            var memoryStore = ctx.World.GetResource<MemoryStore>();
            if (memoryStore.TryGetMemories(AgentId(ctx), out var agentMemories))
            {
                var currentTime = (float)ctx.World.GetResource<TimeResource>().SimulationTime;
                var relevant = agentMemories
                    .Where(m => m.IsRelevant)
                    .OrderByDescending(m => m.EffectiveImportance(currentTime))
                    .Take(Options.MaxMemories);

                foreach (var m in relevant)
                {
                    memories.Add(new SemanticMemoryEntry
                    {
                        Description = $"Memoria-{m.Id} ({m.Type}/{m.Category})",
                        EmotionalImpact = m.EmotionalWeight > 0.5f ? "Alta" : "Normal",
                        Certainty = m.Certainty > 0.7f ? "Alta" : "Media",
                        RelevanceToNow = m.EffectiveImportance(currentTime) > 0.5f ? "Alta" : "Baja",
                        Timeframe = FormatTimeDelta(currentTime - m.Timestamp)
                    });
                }
            }
        }

        return new SemanticLongTermMemory
        {
            Memories = memories
        };
    }

    private string FormatTimeDelta(float seconds)
    {
        if (seconds < 60f) return "Hace instantes";
        if (seconds < 3600f) return $"Hace {(int)(seconds / 60f)} minutos";
        if (seconds < 86400f) return $"Hace {(int)(seconds / 3600f)} horas";
        return $"Hace {(int)(seconds / 86400f)} días";
    }

    private SemanticSocialContext ExtractSocial(ExtractionContext ctx)
    {
        var relationships = new List<SemanticRelationship>();

        if (ctx.World.HasResource<RelationshipStore>())
        {
            var relStore = ctx.World.GetResource<RelationshipStore>();
            var agentRels = relStore.GetRelationships(AgentId(ctx));
            var count = Math.Min(agentRels.Count, Options.MaxRelationships);

            for (int i = 0; i < count; i++)
            {
                var r = agentRels[i];
                var otherId = r.EntityA == AgentId(ctx) ? r.EntityB : r.EntityA;
                var otherName = Name(otherId, ctx);

                relationships.Add(new SemanticRelationship
                {
                    Name = otherName,
                    Type = r.Type.ToString(),
                    TrustLevel = r.TrustLevel >= 0.7f ? "Alta" : r.TrustLevel >= 0.3f ? "Media" : "Baja",
                    CurrentFeeling = r.Value > 0.5f ? "Positivo" : r.Value < -0.5f ? "Negativo" : "Neutro"
                });
            }
        }

        var tension = relationships.Any(r => r.CurrentFeeling == "Negativo")
            ? "Tensión presente"
            : "Sin tensión detectada";

        return new SemanticSocialContext
        {
            Relationships = relationships,
            SocialTension = tension
        };
    }

    private int EstimateTokens(SemanticState state)
    {
        var sb = new StringBuilder();

        AppendNonEmpty(sb, state.Identity.Name);
        AppendNonEmpty(sb, state.Identity.Species);
        AppendNonEmpty(sb, state.Identity.Personality);
        AppendNonEmpty(sb, state.Identity.Role);
        AppendNonEmpty(sb, state.Identity.SelfPerception);

        AppendNonEmpty(sb, state.Situation.Location);
        AppendNonEmpty(sb, state.Situation.TimeOfDay);
        AppendNonEmpty(sb, state.Situation.Weather);
        AppendNonEmpty(sb, state.Situation.Season);
        AppendNonEmpty(sb, state.Situation.CurrentActivity);
        foreach (var e in state.Situation.NearbyEntities)
        {
            AppendNonEmpty(sb, e.Description);
            AppendNonEmpty(sb, e.Relationship);
        }
        foreach (var ev in state.Situation.RecentEvents)
            AppendNonEmpty(sb, ev);

        AppendNonEmpty(sb, state.Internal.PrimaryEmotion);
        AppendNonEmpty(sb, state.Internal.EmotionalReason);
        AppendNonEmpty(sb, state.Internal.PhysicalState);
        AppendNonEmpty(sb, state.Internal.MentalState);
        AppendNonEmpty(sb, state.Internal.GoalConflicts);
        foreach (var g in state.Internal.ActiveGoals)
        {
            AppendNonEmpty(sb, g.Description);
            AppendNonEmpty(sb, g.Urgency);
        }
        foreach (var m in state.Internal.Motivations)
            AppendNonEmpty(sb, m);

        AppendNonEmpty(sb, state.Attention.PrimaryFocus);
        AppendNonEmpty(sb, state.Attention.FocusIntensity);
        AppendNonEmpty(sb, state.Attention.PerceptualRange);
        AppendNonEmpty(sb, state.Attention.FilterBias);
        foreach (var d in state.Attention.DistractingFactors)
            AppendNonEmpty(sb, d);

        foreach (var t in state.WorkingMemory.ActiveThoughts)
            AppendNonEmpty(sb, t);
        foreach (var q in state.WorkingMemory.PendingQuestions)
            AppendNonEmpty(sb, q);
        foreach (var c in state.WorkingMemory.RecentConversations)
        {
            AppendNonEmpty(sb, c.Speaker);
            AppendNonEmpty(sb, c.Content);
        }
        foreach (var i in state.WorkingMemory.ImmediateConcerns)
            AppendNonEmpty(sb, i);
        foreach (var t in state.WorkingMemory.ContextualTriggers)
            AppendNonEmpty(sb, t);

        foreach (var m in state.LongTermMemory.Memories)
        {
            AppendNonEmpty(sb, m.Description);
            AppendNonEmpty(sb, m.EmotionalImpact);
            AppendNonEmpty(sb, m.RelevanceToNow);
            AppendNonEmpty(sb, m.Timeframe);
        }
        foreach (var r in state.LongTermMemory.RecurringThoughts)
            AppendNonEmpty(sb, r);
        foreach (var k in state.LongTermMemory.KeyEvents)
            AppendNonEmpty(sb, k);
        foreach (var a in state.LongTermMemory.EmotionalAnchors)
            AppendNonEmpty(sb, a);

        foreach (var l in state.WorldModel.KnownLocations)
        {
            AppendNonEmpty(sb, l.Name);
            AppendNonEmpty(sb, l.Description);
            AppendNonEmpty(sb, l.Significance);
        }
        foreach (var e in state.WorldModel.KnownEntities)
        {
            AppendNonEmpty(sb, e.Description);
            AppendNonEmpty(sb, e.Significance);
        }
        foreach (var b in state.WorldModel.Beliefs)
        {
            AppendNonEmpty(sb, b.Statement);
            AppendNonEmpty(sb, b.Confidence);
            AppendNonEmpty(sb, b.Source);
        }
        foreach (var k in state.WorldModel.Knowledge)
        {
            AppendNonEmpty(sb, k.What);
            AppendNonEmpty(sb, k.Certainty);
            AppendNonEmpty(sb, k.Source);
        }
        foreach (var u in state.WorldModel.Uncertainties)
            AppendNonEmpty(sb, u);
        foreach (var p in state.WorldModel.Predictions)
            AppendNonEmpty(sb, p);
        foreach (var t in state.WorldModel.Threats)
            AppendNonEmpty(sb, t);

        foreach (var rel in state.Social.Relationships)
        {
            AppendNonEmpty(sb, rel.Name);
            AppendNonEmpty(sb, rel.Type);
            AppendNonEmpty(sb, rel.TrustLevel);
            AppendNonEmpty(sb, rel.RecentInteraction);
            AppendNonEmpty(sb, rel.CurrentFeeling);
            foreach (var q in rel.OpenQuestions)
                AppendNonEmpty(sb, q);
        }
        AppendNonEmpty(sb, state.Social.SocialSituation);
        AppendNonEmpty(sb, state.Social.SocialTension);
        AppendNonEmpty(sb, state.Social.Reputation);

        foreach (var inc in state.Directives.MustInclude)
            AppendNonEmpty(sb, inc);
        foreach (var exc in state.Directives.MustExclude)
            AppendNonEmpty(sb, exc);
        AppendNonEmpty(sb, state.Directives.Tone);
        AppendNonEmpty(sb, state.Directives.Pacing);

        var charCount = sb.Length;
        return (int)Math.Ceiling(charCount / 4.0);
    }

    private static void AppendNonEmpty(StringBuilder sb, string value)
    {
        if (!string.IsNullOrEmpty(value))
            sb.Append(' ').Append(value);
    }

    private void ApplyBudgetTrim(SemanticState state)
    {
        const int maxIterations = 10;
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            state.EstimatedTokens = EstimateTokens(state);
            if (state.EstimatedTokens <= Options.MaxTokens) return;

            var trimmed = TrimLowestPriority(state);
            if (!trimmed) break;
        }
        state.EstimatedTokens = EstimateTokens(state);
    }

    private bool TrimLowestPriority(SemanticState state)
    {
        if (TrimList(state.LongTermMemory.Memories, 1)) return true;
        if (TrimWorldModel(state.WorldModel)) return true;
        if (TrimList(state.Social.Relationships, 1)) return true;
        if (TrimList(state.WorkingMemory.ActiveThoughts, 1)) return true;
        if (TrimList(state.WorkingMemory.ImmediateConcerns, 1)) return true;
        if (TrimList(state.WorkingMemory.PendingQuestions, 1)) return true;
        if (TrimList(state.WorkingMemory.RecentConversations, 1)) return true;
        if (TrimList(state.Internal.ActiveGoals, 1)) return true;
        if (TrimList(state.Internal.Motivations, 1)) return true;
        if (TrimList(state.Situation.NearbyEntities, 1)) return true;
        if (TrimList(state.Situation.RecentEvents, 1)) return true;
        if (TrimList(state.WorldModel.Uncertainties, 1)) return true;
        if (TrimList(state.WorldModel.Predictions, 1)) return true;
        if (TrimList(state.WorldModel.Threats, 1)) return true;
        if (TrimList(state.Attention.DistractingFactors, 1)) return true;
        if (TrimList(state.LongTermMemory.RecurringThoughts, 1)) return true;
        if (TrimList(state.LongTermMemory.KeyEvents, 1)) return true;
        if (TrimList(state.LongTermMemory.EmotionalAnchors, 1)) return true;
        return false;
    }

    private static bool TrimList<T>(List<T> items, int minTrim)
    {
        if (items.Count == 0) return false;
        var toRemove = Math.Max(minTrim, items.Count / 2);
        toRemove = Math.Min(toRemove, items.Count);
        items.RemoveRange(items.Count - toRemove, toRemove);
        return true;
    }

    private static bool TrimWorldModel(SemanticWorldModel wm)
    {
        if (wm.KnownEntities.Count > 0)
        {
            var half = Math.Max(1, wm.KnownEntities.Count / 2);
            wm.KnownEntities.RemoveRange(wm.KnownEntities.Count - (wm.KnownEntities.Count - half), wm.KnownEntities.Count - half);
            return true;
        }
        if (wm.Beliefs.Count > 0)
        {
            var half = Math.Max(1, wm.Beliefs.Count / 2);
            wm.Beliefs.RemoveRange(wm.Beliefs.Count - (wm.Beliefs.Count - half), wm.Beliefs.Count - half);
            return true;
        }
        if (wm.Knowledge.Count > 0)
        {
            var half = Math.Max(1, wm.Knowledge.Count / 2);
            wm.Knowledge.RemoveRange(wm.Knowledge.Count - (wm.Knowledge.Count - half), wm.Knowledge.Count - half);
            return true;
        }
        return false;
    }
}
