using Aeris.Engine;
using Xunit;

namespace Aeris.Engine.Tests;

public class SemanticExtractorTests
{
    [Fact]
    public void SemanticState_HasDefaultValues()
    {
        var state = new SemanticState();

        Assert.NotNull(state.Identity);
        Assert.NotNull(state.Situation);
        Assert.NotNull(state.Internal);
        Assert.NotNull(state.WorldModel);
        Assert.NotNull(state.Attention);
        Assert.NotNull(state.WorkingMemory);
        Assert.NotNull(state.LongTermMemory);
        Assert.NotNull(state.Social);
        Assert.NotNull(state.Directives);
        Assert.Equal(0, state.EstimatedTokens);
        Assert.Equal(0.0, state.ExtractionTime);
    }

    [Fact]
    public void SemanticState_IsImmutable()
    {
        var state = new SemanticState();

        var identity = state.Identity;
        Assert.NotNull(identity);
        Assert.Same(identity, state.Identity);
    }

    [Fact]
    public void SemanticIdentity_HasDefaults()
    {
        var id = new SemanticIdentity();

        Assert.Equal(string.Empty, id.Name);
        Assert.Equal(string.Empty, id.Species);
        Assert.Equal(0, id.AgeYears);
        Assert.Equal(string.Empty, id.Personality);
        Assert.Equal(string.Empty, id.Role);
        Assert.Equal(string.Empty, id.SelfPerception);
    }

    [Fact]
    public void SemanticSituation_Defaults()
    {
        var s = new SemanticSituation();

        Assert.Equal(string.Empty, s.Location);
        Assert.Equal(string.Empty, s.TimeOfDay);
        Assert.Equal(string.Empty, s.Weather);
        Assert.NotNull(s.NearbyEntities);
        Assert.Empty(s.NearbyEntities);
        Assert.NotNull(s.RecentEvents);
        Assert.Empty(s.RecentEvents);
    }

    [Fact]
    public void SemanticInternalState_Defaults()
    {
        var s = new SemanticInternalState();

        Assert.Equal(string.Empty, s.PrimaryEmotion);
        Assert.Equal(string.Empty, s.EmotionalReason);
        Assert.NotNull(s.ActiveGoals);
        Assert.Empty(s.ActiveGoals);
        Assert.NotNull(s.Motivations);
        Assert.Empty(s.Motivations);
    }

    [Fact]
    public void SemanticWorldModel_Defaults()
    {
        var wm = new SemanticWorldModel();

        Assert.NotNull(wm.KnownLocations);
        Assert.NotNull(wm.KnownEntities);
        Assert.NotNull(wm.Beliefs);
        Assert.NotNull(wm.Knowledge);
        Assert.NotNull(wm.Uncertainties);
        Assert.NotNull(wm.Predictions);
        Assert.NotNull(wm.Threats);
    }

    [Fact]
    public void SemanticAttention_Defaults()
    {
        var a = new SemanticAttention();

        Assert.Equal(string.Empty, a.PrimaryFocus);
        Assert.Equal(string.Empty, a.FocusIntensity);
        Assert.NotNull(a.DistractingFactors);
        Assert.Empty(a.DistractingFactors);
    }

    [Fact]
    public void SemanticWorkingMemory_Defaults()
    {
        var wm = new SemanticWorkingMemory();

        Assert.NotNull(wm.ActiveThoughts);
        Assert.NotNull(wm.PendingQuestions);
        Assert.NotNull(wm.RecentConversations);
        Assert.NotNull(wm.ImmediateConcerns);
        Assert.NotNull(wm.ContextualTriggers);
    }

    [Fact]
    public void SemanticLongTermMemory_Defaults()
    {
        var ltm = new SemanticLongTermMemory();

        Assert.NotNull(ltm.Memories);
        Assert.NotNull(ltm.RecurringThoughts);
        Assert.NotNull(ltm.KeyEvents);
        Assert.NotNull(ltm.EmotionalAnchors);
    }

    [Fact]
    public void SemanticSocialContext_Defaults()
    {
        var sc = new SemanticSocialContext();

        Assert.NotNull(sc.Relationships);
        Assert.Equal(string.Empty, sc.SocialSituation);
        Assert.Equal(string.Empty, sc.SocialTension);
    }

    [Fact]
    public void SemanticDirectives_Defaults()
    {
        var d = new SemanticDirectives();

        Assert.NotNull(d.MustInclude);
        Assert.NotNull(d.MustExclude);
        Assert.Equal(string.Empty, d.Tone);
        Assert.Equal(string.Empty, d.Pacing);
        Assert.Equal(0f, d.SuspenseLevel);
    }

    [Fact]
    public void SemanticEntity_Defaults()
    {
        var e = new SemanticEntity();

        Assert.Equal(string.Empty, e.Description);
        Assert.Equal(string.Empty, e.EntityType);
        Assert.Equal(string.Empty, e.Location);
        Assert.Equal(string.Empty, e.RelationToAgent);
        Assert.Equal(string.Empty, e.EmotionalCharge);
        Assert.NotNull(e.NotableTraits);
        Assert.Empty(e.NotableTraits);
    }

    [Fact]
    public void SemanticFact_ToString_FormatsCorrectly()
    {
        var fact = new SemanticFact
        {
            Subject = "Aeris",
            Predicate = "confía en",
            Object = "Cedrick",
            Certainty = "alta",
            Source = "experiencia"
        };

        Assert.Equal("Aeris confía en Cedrick", fact.ToString());
    }

    [Fact]
    public void SemanticFact_Defaults()
    {
        var f = new SemanticFact();

        Assert.Equal(string.Empty, f.Subject);
        Assert.Equal(string.Empty, f.Predicate);
        Assert.Equal(string.Empty, f.Object);
        Assert.Equal(string.Empty, f.Certainty);
        Assert.Equal(string.Empty, f.Source);
    }

    [Fact]
    public void SemanticSnapshot_Defaults()
    {
        var snap = new SemanticSnapshot();

        Assert.NotNull(snap.State);
        Assert.Equal(0, snap.WorldTick);
        Assert.Equal(0.0, snap.SimulationTime);
        Assert.Equal(0, snap.EntityCount);
        Assert.True(snap.ExtractionTimestamp <= DateTime.UtcNow);
        Assert.Null(snap.DebugSummary);
    }

    [Fact]
    public void ExtractionOptions_HasSensibleDefaults()
    {
        var opts = new ExtractionOptions();

        Assert.Equal(4000, opts.MaxTokens);
        Assert.Equal(20, opts.MaxEntities);
        Assert.Equal(10, opts.MaxMemories);
        Assert.Equal(10, opts.MaxRelationships);
        Assert.Equal(30, opts.MaxFacts);
        Assert.Equal(5, opts.MaxRecentEvents);
        Assert.Equal(86400.0, opts.LookbackWindow);
        Assert.True(opts.IncludeWorldModel);
        Assert.True(opts.IncludeDirectives);
    }

    [Fact]
    public void ExtractionOptions_OverrideValues()
    {
        var opts = new ExtractionOptions
        {
            MaxTokens = 2000,
            MaxEntities = 5
        };

        Assert.Equal(2000, opts.MaxTokens);
        Assert.Equal(5, opts.MaxEntities);
    }

    [Fact]
    public void ExtractionContext_RequiresWorld()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        var entity = world.CreateEntity().Build();

        var ctx = new ExtractionContext
        {
            World = world,
            Agent = entity,
            Options = new ExtractionOptions()
        };

        Assert.NotNull(ctx.World);
        Assert.NotNull(ctx.Options);
    }

    [Fact]
    public void SemanticExtractor_DefaultOptions()
    {
        var extractor = new SemanticExtractor();
        Assert.NotNull(extractor.Options);
        Assert.Equal(4000, extractor.Options.MaxTokens);
    }

    [Fact]
    public void SemanticExtractor_CustomOptions()
    {
        var opts = new ExtractionOptions { MaxTokens = 2000 };
        var extractor = new SemanticExtractor(opts);

        Assert.Equal(2000, extractor.Options.MaxTokens);
    }

    [Fact]
    public void SemanticExtractor_Extract_ReturnsSemanticState()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        var entity = world.CreateEntity().Build();

        var extractor = new SemanticExtractor();
        var ctx = new ExtractionContext
        {
            World = world,
            Agent = entity,
            Options = new ExtractionOptions()
        };

        var state = extractor.Extract(ctx);
        Assert.NotNull(state);
    }

    [Fact]
    public void SemanticExtractor_ExtractSnapshot_ReturnsSnapshot()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        var entity = world.CreateEntity().Build();

        var extractor = new SemanticExtractor();
        var ctx = new ExtractionContext
        {
            World = world,
            Agent = entity,
            Options = new ExtractionOptions()
        };

        var snapshot = extractor.ExtractSnapshot(ctx);
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.State);
        Assert.Equal(0, snapshot.WorldTick);
    }

    // --- Sprint 2.2: Extraction tests ---

    private (World world, Entity agent, ExtractionContext ctx) CreateWorldWithAgent()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new MemoryStore());
        world.AddResource(new BeliefStore());
        world.AddResource(new KnowledgeStore());
        world.AddResource(new GoalStore());
        world.AddResource(new RelationshipStore());
        world.AddResource(new EmotionStore());
        world.AddResource(new AttentionStore());
        var agent = world.CreateEntity().Build();
        var extractor = new SemanticExtractor();
        var ctx = new ExtractionContext
        {
            World = world,
            Agent = agent,
            Options = new ExtractionOptions()
        };
        return (world, agent, ctx);
    }

    [Fact]
    public void Extract_EmptyWorld_ReturnsValidState()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor();

        var state = extractor.Extract(ctx);

        Assert.NotNull(state);
        Assert.NotNull(state.Identity);
        Assert.NotNull(state.Situation);
        Assert.NotNull(state.Internal);
        Assert.NotNull(state.WorldModel);
        Assert.NotNull(state.Attention);
        Assert.NotNull(state.WorkingMemory);
        Assert.NotNull(state.LongTermMemory);
        Assert.NotNull(state.Social);
        Assert.NotNull(state.Directives);
    }

    [Fact]
    public void Extract_TimeOfDay_ExtractedFromTimeResource()
    {
        var (world, _, ctx) = CreateWorldWithAgent();
        var time = world.GetResource<TimeResource>();
        world.SetResource(time with { SimulationTime = 21600.0 }); // 6:00 AM

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Mañana", state.Situation.TimeOfDay);
    }

    [Fact]
    public void Extract_TimeOfDay_Afternoon()
    {
        var (world, _, ctx) = CreateWorldWithAgent();
        var time = world.GetResource<TimeResource>();
        world.SetResource(time with { SimulationTime = 43200.0 }); // 12:00 PM

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Tarde", state.Situation.TimeOfDay);
    }

    [Fact]
    public void Extract_TimeOfDay_Night()
    {
        var (world, _, ctx) = CreateWorldWithAgent();
        var time = world.GetResource<TimeResource>();
        world.SetResource(time with { SimulationTime = 72000.0 }); // 20:00 = Atardecer
        world.SetResource(time with { SimulationTime = 82800.0 }); // 23:00 = Noche

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Noche", state.Situation.TimeOfDay);
    }

    [Fact]
    public void Extract_Season_DeterminedByDayOfYear()
    {
        var (world, _, ctx) = CreateWorldWithAgent();
        var time = world.GetResource<TimeResource>();
        world.SetResource(time with { SimulationTime = 50 * 86400.0 }); // day 50 = Primavera

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Primavera", state.Situation.Season);
    }

    [Fact]
    public void Extract_EmptyWorld_AgentAlone()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor();

        var state = extractor.Extract(ctx);

        Assert.Equal("En solitario", state.Situation.CurrentActivity);
    }

    [Fact]
    public void Extract_WithEmotion_ExtractsToWorkingMemory()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var emotionStore = world.GetResource<EmotionStore>();
        emotionStore.Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Joy,
            Intensity = 0.9f,
            DecayRate = 0.01f,
            FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Contains("Joy", state.WorkingMemory.ActiveThoughts[0]);
    }

    [Fact]
    public void Extract_WithHighIntensityEmotion_CreatesConcern()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var emotionStore = world.GetResource<EmotionStore>();
        emotionStore.Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Fear,
            Intensity = 0.85f,
            DecayRate = 0.01f,
            FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Single(state.WorkingMemory.ImmediateConcerns);
        Assert.Contains("intensa", state.WorkingMemory.ImmediateConcerns[0]);
    }

    [Fact]
    public void Extract_WithMemory_ExtractsRelevantMemories()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var memoryStore = world.GetResource<MemoryStore>();
        memoryStore.AddMemory(AgentId(agent), new MemoryData
        {
            Id = 1,
            Type = MemoryType.Experienced,
            Category = MemoryCategory.Social,
            EmotionalWeight = 0.6f,
            Importance = 0.8f,
            Certainty = 0.9f,
            Timestamp = 0f,
            Forgotten = false
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Single(state.LongTermMemory.Memories);
        Assert.Contains("Memoria-1", state.LongTermMemory.Memories[0].Description);
    }

    [Fact]
    public void Extract_ForgottenMemory_NotExtracted()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var memoryStore = world.GetResource<MemoryStore>();
        memoryStore.AddMemory(AgentId(agent), new MemoryData
        {
            Id = 1,
            Type = MemoryType.Forgotten,
            Importance = 0.8f,
            Forgotten = true
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Empty(state.LongTermMemory.Memories);
    }

    [Fact]
    public void Extract_WithBeliefs_ExtractsActiveBeliefs()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var beliefStore = world.GetResource<BeliefStore>();
        beliefStore.AddBelief(AgentId(agent), new BeliefData
        {
            Id = 1,
            Confidence = 0.8f,
            Source = BeliefSource.DirectObservation,
            Status = BeliefStatus.Active
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Single(state.WorldModel.Beliefs);
        Assert.Equal("Alta", state.WorldModel.Beliefs[0].Confidence);
    }

    [Fact]
    public void Extract_WithGoals_ExtractsActiveGoals()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var goalStore = world.GetResource<GoalStore>();
        goalStore.AddGoal(AgentId(agent), new GoalData
        {
            Id = 1,
            Type = GoalType.Exploration,
            Priority = GoalPriority.High,
            Urgency = 0.8f,
            Status = GoalStatus.Active,
            CreationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Single(state.Internal.ActiveGoals);
        Assert.Equal("Alta", state.Internal.ActiveGoals[0].Urgency);
    }

    [Fact]
    public void Extract_WithRelationship_ExtractsToSocial()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var other = world.CreateEntity().Build();
        var relStore = world.GetResource<RelationshipStore>();
        relStore.AddRelationship(AgentId(agent), new RelationshipData
        {
            Id = 1,
            EntityA = AgentId(agent),
            EntityB = other.Id.Value,
            Type = RelationshipType.Friend,
            Value = 0.6f,
            TrustLevel = 0.7f,
            Familiarity = 0.5f,
            Status = RelationshipStatus.Active
        });

        ctx.EntityNames[other.Id.Value] = "OtroNPC";

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Single(state.Social.Relationships);
        Assert.Equal("OtroNPC", state.Social.Relationships[0].Name);
        Assert.Equal("Friend", state.Social.Relationships[0].Type);
        Assert.Equal("Alta", state.Social.Relationships[0].TrustLevel);
        Assert.Equal("Positivo", state.Social.Relationships[0].CurrentFeeling);
    }

    [Fact]
    public void Extract_NegativeRelationship_DetectedTension()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var other = world.CreateEntity().Build();
        var relStore = world.GetResource<RelationshipStore>();
        relStore.AddRelationship(AgentId(agent), new RelationshipData
        {
            Id = 1,
            EntityA = AgentId(agent),
            EntityB = other.Id.Value,
            Type = RelationshipType.Enemy,
            Value = -0.8f,
            TrustLevel = 0.1f,
            Status = RelationshipStatus.Active
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Negativo", state.Social.Relationships[0].CurrentFeeling);
        Assert.Equal("Tensión presente", state.Social.SocialTension);
    }

    [Fact]
    public void Extract_WithAttention_ExtractsFocus()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var other = world.CreateEntity().Build();
        agent.SetComponent(new AttentionComponent
        {
            FocusTargetId = other.Id.Value,
            FocusIntensity = 0.9f,
            PerceptualRange = 15f
        });

        ctx.EntityNames[other.Id.Value] = "Objetivo";

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal("Objetivo", state.Attention.PrimaryFocus);
        Assert.Equal("Intensa", state.Attention.FocusIntensity);
        Assert.Equal("Amplio", state.Attention.PerceptualRange);
    }

    [Fact]
    public void Extract_NoFocus_AttentionNinguno()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor();

        var state = extractor.Extract(ctx);

        Assert.Equal("Ninguno", state.Attention.PrimaryFocus);
    }

    [Fact]
    public void Extract_OtherEntities_InWorldModel()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var other1 = world.CreateEntity().Build();
        var other2 = world.CreateEntity().Build();
        ctx.EntityNames[other1.Id.Value] = "NPC-1";
        ctx.EntityNames[other2.Id.Value] = "NPC-2";

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        Assert.Equal(2, state.WorldModel.KnownEntities.Count);
        Assert.Contains(state.WorldModel.KnownEntities, e => e.Description == "NPC-1");
        Assert.Contains(state.WorldModel.KnownEntities, e => e.Description == "NPC-2");
    }

    [Fact]
    public void Extract_NoEntityIds_InOutput()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var other = world.CreateEntity().Build();
        world.GetResource<MemoryStore>().AddMemory(AgentId(agent), new MemoryData
        {
            Id = 1, Type = MemoryType.Observed, Category = MemoryCategory.Discovery,
            Importance = 0.9f, Certainty = 0.8f, Timestamp = 0f
        });
        world.GetResource<GoalStore>().AddGoal(AgentId(agent), new GoalData
        {
            Id = 1, Priority = GoalPriority.High, Status = GoalStatus.Active, CreationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);

        var json = System.Text.Json.JsonSerializer.Serialize(state);
        Assert.DoesNotContain("EntityId", json);
        Assert.DoesNotContain("\"Value\":", json);
    }

    [Fact]
    public void Extract_Deterministic_SameInputSameOutput()
    {
        var (world1, agent1, ctx1) = CreateWorldWithAgent();
        world1.GetResource<TimeResource>();
        var time = world1.GetResource<TimeResource>();
        world1.SetResource(time with { SimulationTime = 36000.0 });
        world1.GetResource<EmotionStore>().Set(AgentId(agent1), new EmotionComponent
        {
            Primary = EmotionType.Curiosity,
            Intensity = 0.5f,
            DecayRate = 0.01f,
            FormationTime = 0f
        });

        var (world2, agent2, ctx2) = CreateWorldWithAgent();
        var time2 = world2.GetResource<TimeResource>();
        world2.SetResource(time2 with { SimulationTime = 36000.0 });
        world2.GetResource<EmotionStore>().Set(AgentId(agent2), new EmotionComponent
        {
            Primary = EmotionType.Curiosity,
            Intensity = 0.5f,
            DecayRate = 0.01f,
            FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state1 = extractor.Extract(ctx1);
        var state2 = extractor.Extract(ctx2);

        Assert.Equal(state1.Situation.TimeOfDay, state2.Situation.TimeOfDay);
        Assert.Equal(state1.Situation.Season, state2.Situation.Season);
        Assert.Equal(state1.WorkingMemory.ActiveThoughts.Count, state2.WorkingMemory.ActiveThoughts.Count);
    }

    [Fact]
    public void ExtractSnapshot_IncludesWorldTick()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var time = world.GetResource<TimeResource>();
        world.SetResource(time with { SimulationTime = 7200.0 });

        var extractor = new SemanticExtractor();
        var snapshot = extractor.ExtractSnapshot(ctx);

        Assert.Equal(7200.0, snapshot.SimulationTime);
        Assert.NotNull(snapshot.State);
    }

    private static uint AgentId(Entity agent) => agent.Id.Value;

    // --- Sprint 2.3: Budget tests ---

    [Fact]
    public void Budget_EmptyWorld_RespectsBudget()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor(new ExtractionOptions { MaxTokens = 200 });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens <= 200,
            $"Expected <= 200 tokens, got {state.EstimatedTokens}");
    }

    [Fact]
    public void Budget_WithData_TrimsToBudget()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var memStore = world.GetResource<MemoryStore>();
        for (uint i = 1; i <= 50; i++)
        {
            memStore.AddMemory(AgentId(agent), new MemoryData
            {
                Id = i,
                Type = MemoryType.Experienced,
                Category = MemoryCategory.Combat,
                Importance = 0.9f,
                EmotionalWeight = 0.8f,
                Certainty = 0.7f,
                Timestamp = 0f
            });
        }

        var extractor = new SemanticExtractor(new ExtractionOptions
        {
            MaxTokens = 300,
            MaxMemories = 50
        });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens <= 300,
            $"Expected <= 300 tokens, got {state.EstimatedTokens}");
        Assert.True(state.LongTermMemory.Memories.Count < 50,
            "Memories should have been trimmed");
    }

    [Fact]
    public void Budget_DisableBudget_NoTrim()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var memStore = world.GetResource<MemoryStore>();
        for (uint i = 1; i <= 20; i++)
        {
            memStore.AddMemory(AgentId(agent), new MemoryData
            {
                Id = i,
                Type = MemoryType.Experienced,
                Category = MemoryCategory.Discovery,
                Importance = 0.9f,
                Certainty = 0.8f,
                Timestamp = 0f
            });
        }

        var extractor = new SemanticExtractor(new ExtractionOptions
        {
            MaxTokens = 100,
            EnableBudgetTrim = false,
            MaxMemories = 20
        });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens > 100,
            "Without budget trim, state should exceed budget");
        Assert.Equal(20, state.LongTermMemory.Memories.Count);
    }

    [Fact]
    public void Budget_TrimPriority_LongTermMemoryFirst()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        var memStore = world.GetResource<MemoryStore>();
        for (uint i = 1; i <= 20; i++)
        {
            memStore.AddMemory(AgentId(agent), new MemoryData
            {
                Id = i,
                Type = MemoryType.Experienced,
                Category = MemoryCategory.Emotional,
                Importance = 0.5f,
                Certainty = 0.6f,
                Timestamp = 0f
            });
        }
        var goalStore = world.GetResource<GoalStore>();
        goalStore.AddGoal(AgentId(agent), new GoalData
        {
            Id = 1,
            Priority = GoalPriority.Critical,
            Urgency = 0.95f,
            Status = GoalStatus.Active,
            CreationTime = 0f
        });

        var extractorFull = new SemanticExtractor(new ExtractionOptions
        {
            MaxTokens = 10000,
            MaxMemories = 20
        });
        var fullState = extractorFull.Extract(ctx);
        var fullTokenCount = fullState.EstimatedTokens;

        var extractor = new SemanticExtractor(new ExtractionOptions
        {
            MaxTokens = fullTokenCount - 50,
            MaxMemories = 20
        });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens <= fullTokenCount - 40,
            $"Expected trimming from {fullTokenCount} to ~{fullTokenCount - 50}, got {state.EstimatedTokens}");
        Assert.True(state.LongTermMemory.Memories.Count < 20,
            "LongTermMemory should be trimmed first (lowest priority)");
    }

    [Fact]
    public void Budget_VerySmallTokenBudget_ClearsLowPriority()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        for (uint i = 1; i <= 10; i++)
        {
            world.GetResource<MemoryStore>().AddMemory(AgentId(agent), new MemoryData
            {
                Id = i, Type = MemoryType.Observed, Category = MemoryCategory.Social,
                Importance = 0.5f, Certainty = 0.5f, Timestamp = 0f
            });
        }
        for (uint i = 1; i <= 10; i++)
        {
            var npc = world.CreateEntity().Build();
            world.GetResource<RelationshipStore>().AddRelationship(AgentId(agent), new RelationshipData
            {
                Id = i,
                EntityA = AgentId(agent),
                EntityB = npc.Id.Value,
                Type = RelationshipType.Neutral,
                Status = RelationshipStatus.Active
            });
        }

        var extractor = new SemanticExtractor(new ExtractionOptions
        {
            MaxTokens = 80,
            MaxMemories = 10,
            MaxRelationships = 10
        });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens <= 120,
            $"Expected roughly <= 120 tokens after aggressive trim, got {state.EstimatedTokens}");
        Assert.True(state.LongTermMemory.Memories.Count < 10,
            "Memories should be trimmed (lowest priority)");
    }

    [Fact]
    public void Budget_AlreadyUnderBudget_NoChange()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor(new ExtractionOptions { MaxTokens = 10000 });

        var state = extractor.Extract(ctx);

        Assert.True(state.EstimatedTokens <= 10000);
    }

    // --- Sprint 2.4: Fact Normalization tests ---

    [Fact]
    public void Normalize_EmptyState_NoFacts()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState();

        var facts = normalizer.Normalize(state);

        Assert.Empty(facts);
    }

    [Fact]
    public void Normalize_TimeOfDay_ProducesNaturalFact()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            Situation = new SemanticSituation { TimeOfDay = "Noche" }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Subject == "El mundo" &&
            f.Predicate == "está en" &&
            f.Object == "es de noche");
    }

    [Fact]
    public void Normalize_Emotion_TranslatesToSpanish()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            Internal = new SemanticInternalState
            {
                PrimaryEmotion = "Fear"
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Predicate == "siente" &&
            f.Object == "miedo");
    }

    [Fact]
    public void Normalize_Relationship_ProducesFact()
    {
        var normalizer = new FactNormalizer(new Dictionary<uint, string> { { 2, "Cedrick" } });
        var state = new SemanticState
        {
            Social = new SemanticSocialContext
            {
                Relationships = new List<SemanticRelationship>
                {
                    new SemanticRelationship
                    {
                        Name = "Cedrick",
                        Type = "Friend",
                        TrustLevel = "Alta",
                        CurrentFeeling = "Positivo"
                    }
                }
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Object.Contains("cedrick"));
        Assert.Contains(facts, f =>
            f.Predicate.Contains("confía alta"));
    }

    [Fact]
    public void Normalize_Goal_ProducesFact()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            Internal = new SemanticInternalState
            {
                ActiveGoals = new List<SemanticGoal>
                {
                    new SemanticGoal { Description = "Explorar la cueva", Urgency = "Alta" }
                }
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Predicate == "quiere" &&
            f.Object == "explorar la cueva");
    }

    [Fact]
    public void Normalize_AllSections_ProduceFacts()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            Identity = new SemanticIdentity { Name = "Aeris", Species = "Elfa" },
            Situation = new SemanticSituation { TimeOfDay = "Mañana", Season = "Primavera" },
            Internal = new SemanticInternalState { PrimaryEmotion = "Curiosity" },
            Attention = new SemanticAttention { PrimaryFocus = "Objetivo" },
            WorkingMemory = new SemanticWorkingMemory
            {
                ActiveThoughts = new List<string> { "Pensamiento activo" }
            },
            LongTermMemory = new SemanticLongTermMemory
            {
                Memories = new List<SemanticMemoryEntry>
                {
                    new SemanticMemoryEntry { Description = "Un recuerdo", Certainty = "Alta" }
                }
            },
            WorldModel = new SemanticWorldModel
            {
                Beliefs = new List<SemanticBelief>
                {
                    new SemanticBelief { Statement = "Algo es verdad", Confidence = "Alta" }
                }
            },
            Social = new SemanticSocialContext
            {
                Relationships = new List<SemanticRelationship>
                {
                    new SemanticRelationship { Name = "Amigo", Type = "Friend" }
                }
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.True(facts.Count >= 8, $"Expected at least 8 facts, got {facts.Count}");
        Assert.Contains(facts, f => f.Source == "identidad");
        Assert.Contains(facts, f => f.Source == "tiempo");
        Assert.Contains(facts, f => f.Source == "emoción");
        Assert.Contains(facts, f => f.Source == "atención");
        Assert.Contains(facts, f => f.Source == "memoria de trabajo");
        Assert.Contains(facts, f => f.Source == "memoria");
        Assert.Contains(facts, f => f.Source == "creencias");
        Assert.Contains(facts, f => f.Source == "relaciones");
    }

    [Fact]
    public void Normalize_Tension_DetectsNegativeRelationship()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            Social = new SemanticSocialContext
            {
                SocialTension = "Tensión presente"
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Subject == "La situación social" &&
            f.Object == "tensión presente");
    }

    [Fact]
    public void Normalize_FactToString_IsReadable()
    {
        var fact = new SemanticFact
        {
            Subject = "Aeris",
            Predicate = "conoce a",
            Object = "Cedrick",
            Certainty = "seguro",
            Source = "relaciones"
        };

        Assert.Equal("Aeris conoce a Cedrick", fact.ToString());
    }

    [Fact]
    public void Normalize_Knowledge_ProducesFact()
    {
        var normalizer = new FactNormalizer();
        var state = new SemanticState
        {
            WorldModel = new SemanticWorldModel
            {
                Knowledge = new List<SemanticKnowledge>
                {
                    new SemanticKnowledge
                    {
                        What = "La llave abre la puerta",
                        Certainty = "Certain"
                    }
                }
            }
        };

        var facts = normalizer.Normalize(state);

        Assert.Contains(facts, f =>
            f.Predicate == "sabe que" &&
            f.Object == "la llave abre la puerta");
    }

    // --- Sprint 2.5: Validation tests ---

    [Fact]
    public void Validation_EmptyState_IsValid()
    {
        var state = new SemanticState();
        var result = SemanticValidator.Validate(state);

        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void Validation_NoEcsLeaks_InExtractedState()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        world.GetResource<EmotionStore>().Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Joy,
            Intensity = 0.5f,
            DecayRate = 0.01f,
            FormationTime = 0f
        });
        world.GetResource<MemoryStore>().AddMemory(AgentId(agent), new MemoryData
        {
            Id = 1, Type = MemoryType.Observed, Category = MemoryCategory.Discovery,
            Importance = 0.8f, Certainty = 0.7f, Timestamp = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);
        var result = SemanticValidator.Validate(state);

        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void Validation_Deterministic_SameInputSameOutput()
    {
        var (world1, agent1, ctx1) = CreateWorldWithAgent();
        var time1 = world1.GetResource<TimeResource>();
        world1.SetResource(time1 with { SimulationTime = 36000.0 });
        world1.GetResource<EmotionStore>().Set(AgentId(agent1), new EmotionComponent
        {
            Primary = EmotionType.Curiosity, Intensity = 0.5f,
            DecayRate = 0.01f, FormationTime = 0f
        });

        var (world2, agent2, ctx2) = CreateWorldWithAgent();
        var time2 = world2.GetResource<TimeResource>();
        world2.SetResource(time2 with { SimulationTime = 36000.0 });
        world2.GetResource<EmotionStore>().Set(AgentId(agent2), new EmotionComponent
        {
            Primary = EmotionType.Curiosity, Intensity = 0.5f,
            DecayRate = 0.01f, FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state1 = extractor.Extract(ctx1);
        var state2 = extractor.Extract(ctx2);

        Assert.Equal(state1.EstimatedTokens, state2.EstimatedTokens);
        Assert.Equal(state1.Situation.TimeOfDay, state2.Situation.TimeOfDay);
        Assert.Equal(state1.Situation.Season, state2.Situation.Season);
        Assert.Equal(state1.Situation.NearbyEntities.Count, state2.Situation.NearbyEntities.Count);
        Assert.Equal(state1.Internal.ActiveGoals.Count, state2.Internal.ActiveGoals.Count);
        Assert.Equal(state1.LongTermMemory.Memories.Count, state2.LongTermMemory.Memories.Count);
        Assert.Equal(state1.Social.Relationships.Count, state2.Social.Relationships.Count);
        Assert.Equal(state1.WorldModel.Beliefs.Count, state2.WorldModel.Beliefs.Count);
    }

    [Fact]
    public void Validation_Serialization_RoundTrip()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        world.GetResource<EmotionStore>().Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Nostalgia, Intensity = 0.6f,
            DecayRate = 0.01f, FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);
        var result = SemanticValidator.ValidateSerializable(state);

        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void Validation_TokenBudget_Respected()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        for (uint i = 1; i <= 20; i++)
        {
            world.GetResource<MemoryStore>().AddMemory(AgentId(agent), new MemoryData
            {
                Id = i, Type = MemoryType.Experienced, Category = MemoryCategory.Combat,
                Importance = 0.8f, Certainty = 0.7f, Timestamp = 0f
            });
        }

        var extractor = new SemanticExtractor(new ExtractionOptions { MaxTokens = 250 });
        var state = extractor.Extract(ctx);
        var result = SemanticValidator.Validate(state, extractor.Options);

        Assert.True(result.IsValid, string.Join(", ", result.Errors));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validation_Structure_AllSectionsPresent()
    {
        var (_, _, ctx) = CreateWorldWithAgent();
        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);
        var result = SemanticValidator.Validate(state);

        Assert.True(result.IsValid, string.Join(", ", result.Errors));
        Assert.NotNull(state.Identity);
        Assert.NotNull(state.Situation);
        Assert.NotNull(state.Internal);
        Assert.NotNull(state.WorldModel);
        Assert.NotNull(state.Attention);
        Assert.NotNull(state.WorkingMemory);
        Assert.NotNull(state.LongTermMemory);
        Assert.NotNull(state.Social);
        Assert.NotNull(state.Directives);
    }

    [Fact]
    public void Validation_JsonOutput_IsCleanString()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        world.GetResource<EmotionStore>().Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Hope, Intensity = 0.7f,
            DecayRate = 0.01f, FormationTime = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);
        var json = System.Text.Json.JsonSerializer.Serialize(state);

        Assert.DoesNotContain("EntityId", json);
        Assert.DoesNotContain("Entity(", json);
        Assert.DoesNotContain("Arch.", json);
        Assert.DoesNotContain("Store", json);
    }

    [Fact]
    public void Validation_Facts_AreCleanStrings()
    {
        var (world, agent, ctx) = CreateWorldWithAgent();
        world.GetResource<EmotionStore>().Set(AgentId(agent), new EmotionComponent
        {
            Primary = EmotionType.Joy, Intensity = 0.5f,
            DecayRate = 0.01f, FormationTime = 0f
        });
        world.GetResource<MemoryStore>().AddMemory(AgentId(agent), new MemoryData
        {
            Id = 1, Type = MemoryType.Learned, Category = MemoryCategory.Discovery,
            Importance = 0.9f, Certainty = 0.8f, Timestamp = 0f
        });

        var extractor = new SemanticExtractor();
        var state = extractor.Extract(ctx);
        var normalizer = new FactNormalizer();
        var facts = normalizer.Normalize(state);

        foreach (var fact in facts)
        {
            Assert.False(fact.ToString().Contains("EntityId"),
                $"Fact contains EntityId: {fact}");
            Assert.False(fact.ToString().Contains("uint"),
                $"Fact contains uint: {fact}");
        }
    }
}
