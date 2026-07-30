# Changelog

All notable changes to the Aeris project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Sprint 3A — Cognitive Infrastructure
- **PerceptionSystem**: Structured Percept[] from world events, sensory filtering
- **AttentionSystem**: Fixed computational budget, salience filtering, AffectState modulation
- **WorkingMemorySystem**: Limited capacity (N chunks, configurable), decay and refresh, salience tracking
- **LongTermMemorySystem**: Episodic/semantic/procedural stores, consolidation, forgetting
- **AffectSystem**: 9‑D continuous vector (Curiosity, Stress, Confidence, Trust, Novelty, Attachment, Threat, RewardExpectation, CognitiveLoad), homeostasis
- **GoalSystem**: Activation, suspension, prioritization, progress tracking
- **WorldModelSystem**: Internal map with observed entities and confidence
- 210 tests, 0 failures

### Sprint 3B.1 — Memory Retrieval
- **RetrievalResult**: RetrievedMemoryEntry, RetrievalEvidence, RetrievalOperation
- **IMemoryRetrievalStrategy**: Strategy contract + MemoryRetrievalContext
- **LinearScanStrategy**: Baseline scoring (importance×0.4 + recency×0.3 + contextOverlap×0.2 + attentionRelevance×0.1)
- **MemoryRetrievalSystem**: ECS orchestrator — reads LTM/WM/Affect, invokes strategy, writes WM chunks, emits CausalTrace
- CONTRACT-MR validated (S-001–S-010), determinism verified, strategy reemplazabilidad
- 11 new tests, 221 total, 0 failures

## [0.2.0-semantics] - 2026-07-28

### Sprint 2 — Semantic Extractor
- **SemanticExtractor**: Entity extraction, context extraction, memory extraction, emotion extraction, goal extraction, relationship extraction
- **SemanticState**: Structured output (target entity, nearby entities, current situation, relevant memories, emotional state, active goals, key relationships)
- **FactNormalizer**: Normalizes triples (subject‑predicate‑object) with confidence
- **SemanticValidator**: Schema validation, confidence thresholds
- **PromptBuilder**: System instructions, semantic state serialization, player input formatting, output schema definition
- 202 tests, 0 failures

## [0.1.0-engine] - 2026-07-27

### Sprint 0 — Architecture Specification (Frozen)
- 14 specification documents
- 5 ADRs (all accepted)
- Glossary, validation rules, development roadmap
- Architecture frozen before implementation

### Sprint 1 — Core Engine
- **World**: Entity/Component/Resource CRUD with Arch ECS
- **EntityId**: `readonly record struct` with `uint` identifier
- **EntityBuilder**: Fluent `With<T>().Build()` pattern
- **TimeResource**: `double` SimulationTime, long Tick, calendar, TimeScale
- **SystemManager**: Phase-ordered execution, Freeze/Validate/ExecuteAll
- **EventBus**: Dual-queue (Deferred + Immediate), Subscribe/Emit/Flush
- **SchedulerResource**: Time-ordered scheduling, lazy sort, callback processing
- **JsonPersistence**: Save/Load/Checkpoint with `System.Text.Json`
- **EngineStats**: Per-tick telemetry (duration, systems executed, memory)
- **Engine**: Full tick lifecycle, deterministic, auto-registers resources

### Sprint 1.5 — Cognitive Layer
- **Cognitive Data Models**: MemoryData, BeliefData, KnowledgeData, EmotionData, GoalData, RelationshipData, AttentionData
- **Marker Components**: MemoryMarker, BeliefMarker, KnowledgeMarker, GoalMarker, RelationshipMarker, AttentionComponent
- **EmotionComponent**: Stored directly on entity (unmanaged, small)
- **Cognitive Stores**: MemoryStore, BeliefStore, KnowledgeStore, EmotionStore, GoalStore, RelationshipStore, AttentionStore
- **Cognitive Events**: MemoryCreatedEvent, KnowledgeAcquiredEvent, EmotionChangedEvent, GoalCompletedEvent, GoalActivatedEvent, RelationshipChangedEvent, AttentionChangedEvent
- **Cognitive Systems**: MemoryConsolidationSystem, KnowledgeUpdateSystem, EmotionProcessingSystem, GoalEvaluationSystem, AttentionUpdateSystem, RelationshipSystem
- **Entity.SetComponent<T>()**: Upsert pattern for system updates
- **Engine auto-registration**: Cognitive stores + EventBus registered in constructor

### Infrastructure
- .NET 8.0.423, Arch 2.1.0
- FsCheck property-based tests
- BenchmarkDotNet suite (InProcessNoEmitToolchain)
- `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild`
- 137 tests, 0 failures, 0 warnings
- Persistence: transient resources (EventBus, SchedulerResource) excluded from serialization

### Bug Fixes
- `TimeResource` struct default `TimeScale=0f` caused time to never advance when using `new TimeResource()` instead of `TimeResource.Create()`
- `EventBus` constructor parameter caused `System.Text.Json` deserialization failure — excluded from persistence as transient runtime state
