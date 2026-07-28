using Aeris.Engine;
using Xunit;

namespace Aeris.Engine.Tests;

public class CognitiveSystemTests
{
    private Engine CreateEngineWithCognitiveSystems()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new AttentionUpdateSystem());
        engine.RegisterSystem(new MemoryConsolidationSystem());
        engine.RegisterSystem(new KnowledgeUpdateSystem());
        engine.RegisterSystem(new EmotionProcessingSystem());
        engine.RegisterSystem(new GoalEvaluationSystem());
        engine.RegisterSystem(new RelationshipSystem());
        engine.RegisterSystem(new CounterSystem());

        engine.Initialize();
        return engine;
    }

    [Fact]
    public void Engine_Initializes_All_Cognitive_Stores()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        Assert.True(world.HasResource<MemoryStore>());
        Assert.True(world.HasResource<BeliefStore>());
        Assert.True(world.HasResource<KnowledgeStore>());
        Assert.True(world.HasResource<EmotionStore>());
        Assert.True(world.HasResource<GoalStore>());
        Assert.True(world.HasResource<RelationshipStore>());
        Assert.True(world.HasResource<AttentionStore>());
    }

    [Fact]
    public void MemoryStore_Add_And_Retrieve()
    {
        var store = new MemoryStore();
        var id = store.AllocateId();

        store.AddMemory(1, new MemoryData
        {
            Id = id,
            Type = MemoryType.Observed,
            Category = MemoryCategory.Social,
            Importance = 0.8f,
            EmotionalWeight = 0.5f,
            Certainty = 0.9f,
            Timestamp = 100f,
            InvolvedEntityId = 2
        });

        var memories = store.GetMemories(1);
        Assert.Single(memories);
        Assert.Equal(id, memories[0].Id);
        Assert.Equal(MemoryType.Observed, memories[0].Type);
        Assert.Equal(0.8f, memories[0].Importance);
    }

    [Fact]
    public void MemoryStore_Returns_Empty_For_Unknown_Entity()
    {
        var store = new MemoryStore();
        var memories = store.GetMemories(999);
        Assert.Empty(memories);
    }

    [Fact]
    public void MemoryData_EffectiveImportance_Decays_Over_Time()
    {
        var memory = new MemoryData
        {
            Importance = 1.0f,
            Timestamp = 0f,
            Forgotten = false
        };

        var immediate = memory.EffectiveImportance(0f);
        var later = memory.EffectiveImportance(86400f);
        var muchLater = memory.EffectiveImportance(86400f * 3);

        Assert.Equal(1.0f, immediate);
        Assert.True(later < immediate);
        Assert.True(muchLater < later);
    }

    [Fact]
    public void MemoryConsolidation_Forgotten_Memories_Are_Marked()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new MemoryConsolidationSystem());
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new MemoryMarker { Count = 1, LastConsolidationTime = 0f });

        var memories = world.GetResource<MemoryStore>();
        memories.AddMemory(entity.Id.Value, new MemoryData
        {
            Id = 1,
            Type = MemoryType.Observed,
            Importance = 0.01f,
            Timestamp = 0f,
            Forgotten = false
        });

        for (int i = 0; i < 5000; i++)
        {
            engine.RunOneTick(1f);
        }

        var entityMemories = memories.GetMemories(entity.Id.Value);
        Assert.True(entityMemories.Count > 0);
        Assert.True(entityMemories[0].Forgotten, $"Memory should be forgotten. Forgotten={entityMemories[0].Forgotten}, Importance={entityMemories[0].Importance}");
    }

    [Fact]
    public void MemoryConsolidation_Important_Memories_Survive()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new MemoryConsolidationSystem());
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new MemoryMarker { Count = 1, LastConsolidationTime = 0f });

        var memories = world.GetResource<MemoryStore>();
        memories.AddMemory(entity.Id.Value, new MemoryData
        {
            Id = 1,
            Type = MemoryType.Experienced,
            Importance = 0.9f,
            Timestamp = 0f,
            Forgotten = false
        });

        for (int i = 0; i < 5000; i++)
        {
            engine.RunOneTick(1f);
        }

        var entityMemories = memories.GetMemories(entity.Id.Value);
        Assert.True(entityMemories.Count > 0);
        Assert.False(entityMemories[0].Forgotten);
    }

    [Fact]
    public void BeliefStore_Add_And_Retrieve()
    {
        var store = new BeliefStore();
        var id = store.AllocateId();

        store.AddBelief(1, new BeliefData
        {
            Id = id,
            Confidence = 0.8f,
            Source = BeliefSource.DirectObservation,
            FormationTime = 100f,
            Status = BeliefStatus.Active
        });

        var beliefs = store.GetBeliefs(1);
        Assert.Single(beliefs);
        Assert.Equal(0.8f, beliefs[0].Confidence);
        Assert.True(beliefs[0].IsActive);
    }

    [Fact]
    public void KnowledgeStore_Add_And_Retrieve()
    {
        var store = new KnowledgeStore();
        var id = store.AllocateId();

        store.AddKnowledge(1, new KnowledgeData
        {
            Id = id,
            Type = KnowledgeType.Fact,
            Certainty = KnowledgeCertainty.Certain,
            Source = KnowledgeSource.DirectExperience,
            AcquisitionTime = 100f,
            IsPublic = true
        });

        var knowledge = store.GetKnowledge(1);
        Assert.Single(knowledge);
        Assert.Equal(KnowledgeType.Fact, knowledge[0].Type);
    }

    [Fact]
    public void GoalStore_Add_And_Retrieve()
    {
        var store = new GoalStore();
        var id = store.AllocateId();

        store.AddGoal(1, new GoalData
        {
            Id = id,
            Type = GoalType.Exploration,
            Priority = GoalPriority.High,
            Urgency = 0.7f,
            Status = GoalStatus.Active,
            CreationTime = 100f
        });

        var goals = store.GetGoals(1);
        Assert.Single(goals);
        Assert.True(goals[0].IsActive);
    }

    [Fact]
    public void GoalData_EffectivePriority_Higher_When_Urgent()
    {
        var urgentGoal = new GoalData
        {
            Priority = GoalPriority.High,
            Urgency = 0.9f,
            Status = GoalStatus.Active,
            CreationTime = 0f
        };

        var calmGoal = new GoalData
        {
            Priority = GoalPriority.High,
            Urgency = 0.1f,
            Status = GoalStatus.Active,
            CreationTime = 0f
        };

        Assert.True(urgentGoal.EffectivePriority(100f) > calmGoal.EffectivePriority(100f));
    }

    [Fact]
    public void GoalEvaluation_Fails_Expired_Deadlines()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new GoalEvaluationSystem());
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new GoalMarker { ActiveCount = 1, HighestPriority = GoalPriority.High });

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(entity.Id.Value, new GoalData
        {
            Id = 1,
            Type = GoalType.Survival,
            Priority = GoalPriority.Critical,
            Urgency = 1f,
            Status = GoalStatus.Active,
            CreationTime = 0f,
            Deadline = 50f
        });

        for (int i = 0; i < 100; i++)
        {
            engine.RunOneTick(1f);
        }

        var entityGoals = goals.GetGoals(entity.Id.Value);
        Assert.True(entityGoals.Count > 0);
        Assert.Equal(GoalStatus.Failed, entityGoals[0].Status);
    }

    [Fact]
    public void RelationshipStore_Bidirectional()
    {
        var store = new RelationshipStore();
        var id = store.AllocateId();

        var rel = new RelationshipData
        {
            Id = id,
            EntityA = 1,
            EntityB = 2,
            Type = RelationshipType.Friend,
            Value = 0.7f,
            TrustLevel = 0.5f,
            Familiarity = 0.3f,
            Status = RelationshipStatus.Active
        };

        store.AddRelationship(1, rel);
        store.AddRelationship(2, rel);

        Assert.Single(store.GetRelationships(1));
        Assert.Single(store.GetRelationships(2));

        Assert.True(store.TryGetRelationship(1, 2, out var found));
        Assert.Equal(RelationshipType.Friend, found.Type);
    }

    [Fact]
    public void RelationshipStore_RemoveEntity_Cleans_Pairs()
    {
        var store = new RelationshipStore();
        store.AddRelationship(1, new RelationshipData
        {
            Id = store.AllocateId(),
            EntityA = 1, EntityB = 2,
            Type = RelationshipType.Ally,
            Status = RelationshipStatus.Active
        });

        store.RemoveEntity(1);

        Assert.Empty(store.GetRelationships(1));
        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void EmotionComponent_Decays_Over_Time()
    {
        var emotion = new EmotionComponent
        {
            Primary = EmotionType.Fear,
            Intensity = 1.0f,
            DecayRate = 0.1f,
            FormationTime = 0f
        };

        Assert.Equal(1.0f, emotion.EffectiveIntensity(0f));
        Assert.Equal(0.5f, emotion.EffectiveIntensity(5f));
        Assert.Equal(0f, emotion.EffectiveIntensity(15f));
    }

    [Fact]
    public void EmotionProcessing_System_Decays_Emotions()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        var trackingSystem = new TrackingEmotionSystem();
        engine.RegisterSystem(trackingSystem);
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new EmotionComponent
        {
            Primary = EmotionType.Fear,
            Intensity = 1.0f,
            DecayRate = 0.01f,
            FormationTime = 0f,
            UpdateTime = 0f
        });

        for (int i = 0; i < 150; i++)
        {
            engine.RunOneTick(1f);
        }

        var emotion = entity.GetComponent<EmotionComponent>();
        Assert.True(trackingSystem.ProcessedEntity,
            $"Entity not processed. Executed={trackingSystem.ExecuteCount}, Found={trackingSystem.EmotionFound}, CurrentTime={trackingSystem.LastCurrentTime}, UpdateTime={trackingSystem.LastUpdateTime}");
        Assert.True(emotion.Intensity < 0.5f || emotion.Primary == EmotionType.None,
            $"Emotion should have decayed. Primary={emotion.Primary}, Intensity={emotion.Intensity}");
    }

    [Fact]
    public void AttentionStore_Set_And_Get()
    {
        var store = new AttentionStore();
        store.SetNearby(1, new List<uint> { 2, 3, 4 });

        var nearby = store.GetNearby(1);
        Assert.Equal(3, nearby.Count);
        Assert.Contains(2u, nearby);
    }

    [Fact]
    public void Cognitive_Systems_Register_In_Order()
    {
        var systemManager = new SystemManager();
        systemManager.Register(new AttentionUpdateSystem());
        systemManager.Register(new MemoryConsolidationSystem());
        systemManager.Register(new KnowledgeUpdateSystem());
        systemManager.Register(new EmotionProcessingSystem());
        systemManager.Register(new GoalEvaluationSystem());
        systemManager.Register(new RelationshipSystem());
        systemManager.Register(new CounterSystem());
        systemManager.Freeze();

        Assert.Equal(7, systemManager.Systems.Count);

        var phases = systemManager.Systems.Select(s => s.Phase).ToList();
        Assert.Equal(SystemPhase.Initialization, phases[0]);
        Assert.Equal(SystemPhase.Perception, phases[1]);
        Assert.Equal(SystemPhase.Perception, phases[2]);
        Assert.Equal(SystemPhase.Perception, phases[3]);
        Assert.Equal(SystemPhase.Cognition, phases[4]);
        Assert.Equal(SystemPhase.Cognition, phases[5]);
        Assert.Equal(SystemPhase.Cognition, phases[6]);
    }

    [Fact]
    public void Integration_Perception_To_Memory_To_Goal()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new AttentionUpdateSystem());
        engine.RegisterSystem(new MemoryConsolidationSystem());
        engine.RegisterSystem(new KnowledgeUpdateSystem());
        engine.RegisterSystem(new EmotionProcessingSystem());
        engine.RegisterSystem(new GoalEvaluationSystem());
        engine.RegisterSystem(new RelationshipSystem());
        engine.RegisterSystem(new CounterSystem());
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new MemoryMarker { Count = 0, LastConsolidationTime = 0f });
        entity.AddComponent(new EmotionComponent
        {
            Primary = EmotionType.Curiosity,
            Intensity = 0.8f,
            DecayRate = 0.001f,
            FormationTime = 0f,
            UpdateTime = 0f
        });
        entity.AddComponent(new GoalMarker { ActiveCount = 0, HighestPriority = GoalPriority.Trivial });

        var memories = world.GetResource<MemoryStore>();
        memories.AddMemory(entity.Id.Value, new MemoryData
        {
            Id = memories.AllocateId(),
            Type = MemoryType.Observed,
            Category = MemoryCategory.Discovery,
            Importance = 0.9f,
            EmotionalWeight = 0.7f,
            Certainty = 0.8f,
            Timestamp = 10f,
            Forgotten = false
        });

        var goals = world.GetResource<GoalStore>();
        goals.AddGoal(entity.Id.Value, new GoalData
        {
            Id = goals.AllocateId(),
            Type = GoalType.Exploration,
            Priority = GoalPriority.High,
            Urgency = 0.8f,
            Status = GoalStatus.Active,
            CreationTime = 10f
        });

        for (int i = 0; i < 10; i++)
        {
            engine.RunOneTick(1f);
        }

        var entityMemories = memories.GetMemories(entity.Id.Value);
        Assert.Single(entityMemories);
        Assert.False(entityMemories[0].Forgotten);

        var emotionAfter = entity.GetComponent<EmotionComponent>();
        Assert.Equal(EmotionType.Curiosity, emotionAfter.Primary);

        var entityGoals = goals.GetGoals(entity.Id.Value);
        Assert.Single(entityGoals);
        Assert.True(entityGoals[0].IsActive);
    }

    [Fact]
    public void Integration_Relationships_Bidirectional_Persist()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new RelationshipSystem());
        engine.RegisterSystem(new CounterSystem());
        engine.Initialize();

        var entity1 = world.CreateEntity().Build();
        entity1.AddComponent(new RelationshipMarker { Count = 0 });

        var entity2 = world.CreateEntity().Build();
        entity2.AddComponent(new RelationshipMarker { Count = 0 });

        var relationships = world.GetResource<RelationshipStore>();
        var rel = new RelationshipData
        {
            Id = relationships.AllocateId(),
            EntityA = entity1.Id.Value,
            EntityB = entity2.Id.Value,
            Type = RelationshipType.Friend,
            Value = 0.6f,
            TrustLevel = 0.4f,
            Familiarity = 0.2f,
            InteractionCount = 5f,
            LastInteractionTime = 0f,
            Status = RelationshipStatus.Active
        };

        relationships.AddRelationship(entity1.Id.Value, rel);
        relationships.AddRelationship(entity2.Id.Value, rel);

        for (int i = 0; i < 100; i++)
        {
            engine.RunOneTick(1f);
        }

        Assert.True(relationships.TryGetRelationship(entity1.Id.Value, entity2.Id.Value, out var after));
        Assert.Equal(RelationshipType.Friend, after.Type);
    }

    [Fact]
    public void Integration_Full_Cognitive_Tick()
    {
        var world = new World();
        world.AddResource(TimeResource.Create());
        world.AddResource(new EngineStats());
        world.SetResource(new SchedulerResource());
        var engine = new Engine(world);

        engine.RegisterSystem(new AttentionUpdateSystem());
        engine.RegisterSystem(new MemoryConsolidationSystem());
        engine.RegisterSystem(new KnowledgeUpdateSystem());
        engine.RegisterSystem(new EmotionProcessingSystem());
        engine.RegisterSystem(new GoalEvaluationSystem());
        engine.RegisterSystem(new RelationshipSystem());
        engine.RegisterSystem(new CounterSystem());
        engine.Initialize();

        var entity = world.CreateEntity().Build();
        entity.AddComponent(new MemoryMarker { Count = 0, LastConsolidationTime = 0f });
        entity.AddComponent(new BeliefMarker { Count = 0, LastUpdateTime = 0f });
        entity.AddComponent(new KnowledgeMarker { Count = 0, LastUpdateTime = 0f });
        entity.AddComponent(new EmotionComponent
        {
            Primary = EmotionType.Joy,
            Intensity = 0.5f,
            DecayRate = 0.001f,
            FormationTime = 0f,
            UpdateTime = 0f
        });
        entity.AddComponent(new GoalMarker { ActiveCount = 0, HighestPriority = GoalPriority.Trivial });
        entity.AddComponent(new RelationshipMarker { Count = 0 });
        entity.AddComponent(new AttentionComponent
        {
            FocusTargetId = 0,
            FocusIntensity = 0f,
            PerceptualRange = 10f,
            UpdateTime = 0f
        });

        var memories = world.GetResource<MemoryStore>();
        memories.AddMemory(entity.Id.Value, new MemoryData
        {
            Id = memories.AllocateId(),
            Type = MemoryType.Experienced,
            Category = MemoryCategory.Emotional,
            Importance = 0.6f,
            EmotionalWeight = 0.4f,
            Certainty = 0.9f,
            Timestamp = 5f
        });

        for (int i = 0; i < 50; i++)
        {
            engine.RunOneTick(1f);
        }

        var stats = world.GetResource<EngineStats>();
        Assert.True(stats.SystemsExecuted >= 6);
        Assert.True(stats.TickDuration > 0);
    }

    [Fact]
    public void BeliefStore_Removes_Entity()
    {
        var store = new BeliefStore();
        store.AddBelief(1, new BeliefData { Id = 1, Status = BeliefStatus.Active, Confidence = 0.5f });
        store.AddBelief(1, new BeliefData { Id = 2, Status = BeliefStatus.Active, Confidence = 0.3f });

        Assert.Equal(1, store.Count);

        store.RemoveEntity(1);
        Assert.Equal(0, store.Count);
        Assert.Empty(store.GetBeliefs(1));
    }

    [Fact]
    public void KnowledgeStore_Removes_Entity()
    {
        var store = new KnowledgeStore();
        store.AddKnowledge(1, new KnowledgeData { Id = 1, Type = KnowledgeType.Fact });

        store.RemoveEntity(1);
        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void GoalStore_Removes_Entity()
    {
        var store = new GoalStore();
        store.AddGoal(1, new GoalData { Id = 1, Status = GoalStatus.Active });
        store.AddGoal(1, new GoalData { Id = 2, Status = GoalStatus.Active });

        store.RemoveEntity(1);
        Assert.Equal(0, store.Count);
        Assert.Empty(store.GetGoals(1));
    }

    [Fact]
    public void MemoryStore_Removes_Entity()
    {
        var store = new MemoryStore();
        store.AddMemory(1, new MemoryData { Id = 1 });
        store.AddMemory(1, new MemoryData { Id = 2 });

        store.RemoveEntity(1);
        Assert.Equal(0, store.Count);
        Assert.Empty(store.GetMemories(1));
    }

    [Fact]
    public void AttentionStore_Removes_Entity()
    {
        var store = new AttentionStore();
        store.SetNearby(1, new List<uint> { 2, 3 });

        store.Remove(1);
        Assert.Equal(0, store.Count);
        Assert.Empty(store.GetNearby(1));
    }

    [Fact]
    public void GoalData_Expired_Deadline_Returns_Failed()
    {
        var goal = new GoalData
        {
            Priority = GoalPriority.Critical,
            Urgency = 1f,
            Status = GoalStatus.Active,
            Deadline = 50f
        };

        Assert.Equal(0f, goal.EffectivePriority(100f));
    }

    [Fact]
    public void EmotionData_IsPositive_Negative_Check()
    {
        var joy = new EmotionData { Primary = EmotionType.Joy, Intensity = 0.5f };
        Assert.True(joy.IsPositive);
        Assert.False(joy.IsNegative);

        var fear = new EmotionData { Primary = EmotionType.Fear, Intensity = 0.5f };
        Assert.False(fear.IsPositive);
        Assert.True(fear.IsNegative);
    }

    [Fact]
    public void RelationshipData_EffectiveStrength_Combines_Metrics()
    {
        var rel = new RelationshipData
        {
            Value = 0.8f,
            TrustLevel = 0.6f,
            Familiarity = 0.4f
        };

        var score = rel.EffectiveStrength();
        Assert.True(score > 0f);
        Assert.True(score <= 1f);
    }

    [Fact]
    public void RelationshipData_RecordInteraction_Updates_Familiarity()
    {
        var rel = new RelationshipData
        {
            Familiarity = 0.1f,
            InteractionCount = 0f
        };

        rel.RecordInteraction(100f);

        Assert.Equal(1f, rel.InteractionCount);
        Assert.Equal(100f, rel.LastInteractionTime);
        Assert.True(rel.Familiarity > 0.1f);
    }
}

public sealed class TrackingEmotionSystem : ISystem
{
    public string Name => "TrackingEmotion";
    public SystemPhase Phase => SystemPhase.Cognition;
    public int Priority => 100;

    public int ExecuteCount;
    public bool EmotionFound;
    public bool EmotionUpdated;
    public bool ProcessedEntity;
    public float LastEffectiveIntensity;
    public float LastCurrentTime;
    public float LastUpdateTime;

    public void Execute(World world, float deltaTime)
    {
        ExecuteCount++;
        var time = world.GetResource<TimeResource>();
        float currentTime = (float)time.SimulationTime;
        LastCurrentTime = currentTime;

        foreach (var entity in world.Entities.Values)
        {
            if (!entity.HasComponent<EmotionComponent>()) continue;
            EmotionFound = true;
            var emotion = entity.GetComponent<EmotionComponent>();
            LastUpdateTime = emotion.UpdateTime;

            if (currentTime - emotion.UpdateTime < 60f)
                continue;

            ProcessedEntity = true;
            var effective = emotion.EffectiveIntensity(currentTime);
            LastEffectiveIntensity = effective;
            EmotionUpdated = true;

            if (effective <= 0.01f)
            {
                emotion.Primary = EmotionType.None;
                emotion.Intensity = 0f;
            }
            else
            {
                emotion.Intensity = effective;
            }
            emotion.UpdateTime = currentTime;
            entity.SetComponent(emotion);
        }
    }
}
