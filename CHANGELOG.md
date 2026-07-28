# Changelog

All notable changes to the Aeris project will be documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/).

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
