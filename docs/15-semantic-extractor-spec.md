# 15. Semantic Extractor — Specification

**Version**: 0.1  
**Status**: Sprint 2 — Active  
**Last updated**: 2026-07-27  
**Depends on**: Sprint 1 complete (v0.1.0-engine), Sprint 1.5 complete  
**Related**: [05-semantic-state.md](05-semantic-state.md), [06-llm-contract.md](06-llm-contract.md), [03-data-models.md](03-data-models.md)

---

## 1. Purpose

The Semantic Extractor is the bridge between the deterministic simulation and the probabilistic LLM. It transforms the full world state into a concise, relevant representation that the LLM can use to produce narrative.

```
World State (full, deterministic)
        │
        ▼
  Semantic Extractor
        │
        ▼
  Semantic State (concise, relevant)
        │
        ▼
  Prompt Builder
        │
        ▼
  LLM
        │
        ▼
  Narrative
```

This document specifies the **architecture**, **invariants**, **algorithm**, and **constraints** of the Semantic Extractor. It does not define implementation details — those belong to the code.

---

## 2. Architectural Context

### 2.1 Cognitive Architecture (Revised)

The cognitive architecture distinguishes explicitly between **states** (persistent data) and **processes** (systems that transform data):

| Category | Components | Description |
|----------|-----------|-------------|
| **Persistent States** | Memory, Knowledge, Beliefs, Identity, Relationships, Goals, World Model | What the agent knows, remembers, wants |
| **Processes** | Perception, Reasoning, Planning, Attention, Emotion Processing, Auditing | How the agent transforms information |
| **Metaprocesses** | Meta-Auditing, Learning | How the agent adapts its own processes |

### 2.2 Key Revisions (from architectural review)

1. **Identity Model** (formerly "Ser"): An emergent representation, not a module. Built from autobiography + relationships + goals + memory + preferences. No single "identity" component exists — it is assembled at extraction time.

2. **Emotion as Transversal Modulator**: Emotion does not only produce motivation. It modulates attention, memory formation, perception filters, and learning rates. The extractor must account for emotional influence on all cognitive domains.

3. **Separate Perception from Reasoning**: Perception (what the agent detects) and Reasoning (what the agent infers) have different properties and different data. The extractor processes them independently.

4. **Attention as Selection Mechanism**: Attention is neither emotion nor cognition. It is the mechanism that selects what enters processing. The extractor uses attention to determine which entities, events, and stimuli are relevant.

5. **Working Memory**: Distinguished from long-term memory. Working memory holds the current "context window" of the agent — what it is actively processing right now. The extractor synthesizes working memory from recent events, active goals, and current perception.

6. **World Model**: The agent never reasons about the world. It reasons about its *representation* of the world. The World Model is an explicit, partial, potentially incorrect representation that the agent maintains.

### 2.3 Data Flow

```
┌─────────────────────────────────────────────────────┐
│                   WORLD STATE                        │
│  Entities + Components + Resources + Events          │
└──────────────────────┬──────────────────────────────┘
                       │
         ┌─────────────┼─────────────┐
         ▼             ▼             ▼
    ┌─────────┐  ┌──────────┐  ┌──────────┐
    │Perception│  │ Memory   │  │  Social  │
    │ Extract  │  │ Extract  │  │  Extract │
    └────┬────┘  └────┬─────┘  └────┬─────┘
         │            │             │
         ▼            ▼             ▼
    ┌─────────────────────────────────────┐
    │         Attention Filter            │
    │  (what is relevant RIGHT NOW)       │
    └────────────────┬────────────────────┘
                     │
         ┌───────────┼───────────┐
         ▼           ▼           ▼
    ┌─────────┐ ┌─────────┐ ┌─────────┐
    │Identity │ │  World  │ │ Emotion │
    │ Model   │ │  Model  │ │ Context │
    └────┬────┘ └────┬────┘ └────┬────┘
         │           │           │
         ▼           ▼           ▼
    ┌─────────────────────────────────────┐
    │         Semantic State              │
    │  (concise, relevant, natural)       │
    └─────────────────────────────────────┘
```

---

## 3. Semantic State Structure

The Semantic State is a **read-only, serializable, LLM-oriented** representation. It never contains ECS references, internal caches, or engine structures.

### 3.1 Top-Level Structure

```
SemanticState
├── Identity        — Who the agent is (emergent, assembled)
├── Situation       — Where, when, what is happening
├── InternalState   — What the agent feels, wants, worries about
├── WorldModel      — The agent's representation of the world
├── Attention       — What the agent is focusing on
├── WorkingMemory   — What the agent is actively processing
├── LongTermMemory  — Relevant memories from the past
├── SocialContext    — Relationships and social dynamics
└── Directives      — Narrative constraints for the LLM
```

### 3.2 Sections Detail

#### Identity (Emergent)

Not a stored component. Assembled at extraction time from:

```
IdentityComponent (base: name, species, role)
    +
PersonalityComponent (temperament, traits)
    +
Autobiography (derived from memory: key life events)
    +
Relationships (who matters to this agent)
    +
Goals (what this agent pursues)
    +
Preferences (learned likes/dislikes)
    ↓
Identity Model (natural language summary)
```

#### Situation

```
CurrentSituation
├── Location        — Natural language description of where
├── TimeOfDay       — "sunset (18:30)", "dawn", "deep night"
├── Weather         — Current weather as perceived
├── Season          — Current season
├── NearbyEntities  — Who/what is nearby (filtered by attention)
├── CurrentActivity — What the agent was doing
└── RecentEvents    — What just happened (last N seconds)
```

#### InternalState

```
InternalState
├── PrimaryEmotion    — Dominant emotion + intensity
├── EmotionalReason   — Why (triggering context)
├── ActiveGoals       — Goals being pursued, ordered by urgency
├── GoalConflicts     — Competing goals, tensions
├── PhysicalState     — Hunger, energy, health (natural language)
├── MentalState       — Cognitive/emotional state summary
└── Motivations       — What drives current behavior
```

#### WorldModel

The agent's *representation* of the world — potentially incorrect, always partial:

```
WorldModel
├── KnownLocations      — Places the agent knows about
├── KnownEntities       — People/things the agent is aware of
├── Beliefs             — What the agent believes (with confidence)
├── Knowledge           — What the agent knows (with certainty)
├── Uncertainties       — What the agent is unsure about
├── Predictions         — What the agent expects to happen
└── Threats             — Perceived dangers
```

#### Attention

What the agent is focusing on right now:

```
AttentionFocus
├── PrimaryFocus        — Main thing the agent is attending to
├── FocusIntensity      — How absorbed (0.0–1.0)
├── DistractingFactors  — What could break focus
├── PerceptualRange     — What the agent can perceive
└── FilterBias          — How emotion/needs bias attention
```

#### WorkingMemory

What the agent is actively processing (synthesized, not stored):

```
WorkingMemory
├── ActiveThoughts      — Current chain of thought
├── PendingQuestions    — Things the agent wants to know
├── RecentConversations — Last N exchanges
├── ImmediateConcerns   — What needs attention now
└── ContextualTriggers  — Stimuli demanding response
```

#### LongTermMemory

Relevant memories, filtered and prioritized:

```
LongTermMemory
├── Memories            — List of relevant memories (max 10)
├── RecurringThoughts   — Persistent mental patterns
├── KeyEvents           — Life-defining moments
└── EmotionalAnchors    — Memories with high emotional weight
```

Each memory entry:

```
MemoryEntry
├── Description      — What happened (natural language)
├── EmotionalImpact  — How it felt
├── Certainty        — How sure the agent is
├── RelevanceToNow   — Why it matters in this moment
└── Timeframe        — When it happened (relative)
```

#### SocialContext

```
SocialContext
├── Relationships     — Key relationships (max 5)
├── SocialSituation   — Current social dynamics
├── SocialTension     — Unresolved social issues
└── Reputation        — How others perceive the agent
```

Each relationship:

```
RelationshipEntry
├── Name              — Who
├── RelationshipType  — Nature of relationship
├── TrustLevel        — Trust (natural language)
├── RecentInteraction — Last meaningful exchange
├── CurrentFeeling    — How the agent feels about them
└── OpenQuestions     — Unresolved aspects
```

#### Directives

Narrative constraints for the LLM:

```
NarratorDirectives
├── MustInclude       — Elements to include in narration
├── MustExclude       — Information to never reveal
├── Tone              — Overall narrative tone
├── Pacing            — Narrative speed
└── SuspenseLevel     — 0.0 (reveal all) to 1.0 (maximum mystery)
```

---

## 4. Invariants

These invariants MUST hold for every Semantic State produced:

| # | Invariant | Rationale |
|---|-----------|-----------|
| I1 | **Same world state → same Semantic State** | Determinism. The extractor is a pure function. |
| I2 | **Never modifies the world** | Read-only extraction. The world is immutable from the extractor's perspective. |
| I3 | **No ECS references in output** | The Semantic State is a standalone data structure. No EntityId, no Component types, no Arch references. |
| I4 | **No information the agent shouldn't know** | Epistemic constraint. The agent cannot know what it hasn't perceived, been told, or inferred. |
| I5 | **Maintains uncertainty** | The extractor never resolves ambiguity. If the agent is unsure, the Semantic State says so. |
| I6 | **Serializable** | The Semantic State can be serialized to JSON and back without loss. |
| I7 | **Bounded size** | The Semantic State fits within token limits (target: < 4000 tokens when serialized). |
| I8 | **Language-neutral output** | Sections use natural language descriptions, not numeric values. The LLM reads words, not numbers. |
| I9 | **Emotion permeates all sections** | Emotional state influences what appears in attention, memory, world model, and social context. |

---

## 5. Extraction Algorithm

### 5.1 High-Level

```
BuildSemanticState(world, entityId):
    1. Extract raw data from ECS
    2. Filter by attention and relevance
    3. Assemble identity model
    4. Build world model representation
    5. Synthesize working memory
    6. Filter and rank long-term memory
    7. Build social context
    8. Generate narrative directives
    9. Validate size constraints
    10. Return Semantic State
```

### 5.2 Extraction Phases

#### Phase 1: Raw Extraction

Read all relevant data from the world:

```
├── Entity components (identity, emotion, attention, physical state)
├── Cognitive stores (memory, beliefs, knowledge, goals, relationships)
├── World resources (time, weather, location, nearby entities)
└── Recent events (EventBus history, scheduler events)
```

#### Phase 2: Attention Filtering

Apply the agent's attention mechanism to determine relevance:

```
For each piece of raw data:
    relevance = BaseRelevance × AttentionBias × EmotionalWeight × RecencyDecay
    
Where:
    BaseRelevance = how important is this inherently?
    AttentionBias = how does the agent's current focus affect perception?
    EmotionalWeight = does emotion amplify or suppress this?
    RecencyDecay = does recent information get priority?
```

The agent's emotional state biases attention:
- **Fear** → amplify threats, suppress non-urgent stimuli
- **Joy** → amplify social stimuli, suppress threats
- **Curiosity** → amplify novel stimuli, suppress familiar ones
- **Sadness** → suppress social stimuli, amplify internal state

#### Phase 3: Translation to Natural Language

Convert numeric/technical values to natural language:

| Source | Translation Rule |
|--------|-----------------|
| `Emotion.Intensity < 0.2` | "barely noticeable" |
| `Emotion.Intensity 0.2–0.5` | "mild" |
| `Emotion.Intensity 0.5–0.8` | "strong" |
| `Emotion.Intensity > 0.8` | "overwhelming" |
| `Hunger.CurrentValue < 20` | "not hungry" |
| `Hunger.CurrentValue 20–50` | "somewhat hungry" |
| `Hunger.CurrentValue 50–80` | "quite hungry" |
| `Hunger.CurrentValue > 80` | "desperately hungry" |
| `Energy.CurrentValue < 20` | "exhausted" |
| `Energy.CurrentValue 20–50` | "tired" |
| `Energy.CurrentValue 50–80` | "rested" |
| `Energy.CurrentValue > 80` | "full of energy" |
| `Time.SimulationHour 5–7` | "early morning" |
| `Time.SimulationHour 7–12` | "morning" |
| `Time.SimulationHour 12–14` | "midday" |
| `Time.SimulationHour 14–18` | "afternoon" |
| `Time.SimulationHour 18–20` | "sunset/dusk" |
| `Time.SimulationHour 20–23` | "evening" |
| `Time.SimulationHour 23–5` | "night" |
| `Belief.Confidence < 0.3` | "unsure" |
| `Belief.Confidence 0.3–0.7` | "somewhat confident" |
| `Belief.Confidence > 0.7` | "quite confident" |

#### Phase 4: Size Budget

The Semantic State has a token budget. Allocation:

```
Total budget: ~4000 tokens (target)

├── Identity:           ~200 tokens
├── Situation:          ~400 tokens
├── InternalState:      ~400 tokens
├── WorldModel:         ~600 tokens
├── Attention:          ~200 tokens
├── WorkingMemory:      ~400 tokens
├── LongTermMemory:     ~800 tokens (10 memories × ~80 tokens)
├── SocialContext:      ~600 tokens (5 relationships × ~120 tokens)
├── Directives:         ~200 tokens
└── Overhead/formatting: ~200 tokens
```

If the budget is exceeded:
1. Reduce LongTermMemory (fewer memories)
2. Reduce SocialContext (fewer relationships)
3. Simplify WorldModel (fewer known entities)
4. Never reduce Identity, Situation, or InternalState below minimum

#### Phase 5: Validation

Before returning the Semantic State, validate:

```
├── Size within budget
├── No null required fields
├── No ECS references leaked
├── All translations are natural language
├── Emotional state is consistent across sections
└── Identity is coherent with memory and goals
```

---

## 6. What the Extractor NEVER Includes

| Category | Reason |
|----------|--------|
| Entity IDs | Technical, meaningless to LLM |
| Component types | ECS internals |
| Numeric precision | "HP: 73/100" → "slightly wounded" |
| Game mechanics | Damage formulas, probability tables |
| Future events | The agent cannot know the future |
| Other agents' private states | Epistemic constraint |
| World truth | Only the agent's *representation* of the world |
| Scheduler/EventBus internals | Engine implementation |
| System execution order | Irrelevant to narrative |
| Random seeds | Technical detail |
| Performance metrics | EngineStats |

---

## 7. What the Extractor ALWAYS Includes

| Section | Minimum Content |
|---------|----------------|
| Identity | Name, species, role, personality summary |
| Situation | Location, time, weather, nearby entities |
| InternalState | Primary emotion, active goal, physical state |
| WorkingMemory | Current focus, immediate concerns |
| Directives | Tone, pacing, must-include elements |

---

## 8. Interface Specification

### 8.1 SemanticExtractor

```csharp
public sealed class SemanticExtractor
{
    // Core extraction: world state → semantic state
    public SemanticState Extract(World world, uint entityId);
    
    // Size estimation (for budget management)
    public int EstimateTokens(SemanticState state);
    
    // Validation
    public ExtractionResult Validate(SemanticState state);
}
```

### 8.2 ExtractionResult

```csharp
public sealed class ExtractionResult
{
    public bool IsValid;
    public int EstimatedTokens;
    public List<string> Warnings;    // Non-fatal issues
    public List<string> Errors;      // Fatal issues (state invalid)
}
```

### 8.3 PromptBuilder

```csharp
public sealed class PromptBuilder
{
    // Semantic state + player input → LLM request
    public LLMRequest Build(SemanticState state, string playerInput);
    
    // System prompt generation
    public string BuildSystemPrompt(SemanticState.Identity identity);
    
    // Conversation history management
    public string BuildHistory(IEnumerable<ConversationEntry> history, int maxTokens);
}
```

---

## 9. Determinism Contract

The Semantic Extractor is a **pure function**:

```
f(world_state, entity_id) → semantic_state

Given identical inputs:
    f(A, 1) == f(A, 1)  // Always
```

This means:
- No random number generation
- No external state (no LLM calls, no file I/O)
- No time-dependent behavior (only world time, not system time)
- No floating-point non-determinism (use consistent comparison thresholds)

---

## 10. Testing Strategy

### 10.1 Unit Tests

| Test | What it verifies |
|------|-----------------|
| Empty world | Extractor handles minimal state gracefully |
| Single entity, no data | Identity + empty sections |
| Entity with emotions | Emotional state appears in InternalState |
| Entity with memories | Memories are filtered and ranked correctly |
| Entity with relationships | SocialContext is populated |
| Entity with goals | Goals appear in InternalState |
| Size budget | Output stays within token limits |
| No ECS leaks | SemanticState contains no EntityId or Component types |
| Determinism | Same input → same output (100 iterations) |

### 10.2 Integration Tests

| Test | What it verifies |
|------|-----------------|
| Full pipeline | World → SemanticState → Prompt → (mock LLM) → Response |
| Save/Load roundtrip | SemanticState serializes and deserializes correctly |
| Attention filtering | Only relevant entities appear in output |
| Emotional modulation | Fear amplifies threats in WorldModel |
| Memory ranking | Most important memories appear first |
| Token budget | Complex world state still produces bounded output |

### 10.3 Property-Based Tests (FsCheck)

| Property | Invariant |
|----------|-----------|
| Output size | Always ≤ 4000 tokens |
| Determinism | Same input → same output |
| Read-only | World state unchanged after extraction |
| No null fields | All required sections populated |
| Emotional consistency | Emotion in InternalState matches emotion in Attention and Memory |

---

## 11. Sprint 2 Deliverables

### 11.1 New Files

```
src/Aeris.Engine/
├── SemanticState.cs          — SemanticState struct + all sub-structs
├── SemanticExtractor.cs      — Core extraction logic
├── PromptBuilder.cs          — Prompt construction
├── TranslationRules.cs       — Numeric → natural language mapping
├── TokenBudget.cs            — Size management
└── ExtractionResult.cs       — Validation result

tests/Aeris.Engine.Tests/
├── SemanticExtractorTests.cs — Unit + integration tests
├── PromptBuilderTests.cs     — Prompt construction tests
└── TranslationRulesTests.cs  — Translation tests
```

### 11.2 Definition of Done

1. `SemanticExtractor.Extract()` produces a valid `SemanticState`
2. All 9 invariants hold (verified by tests)
3. Token budget is respected
4. Determinism verified (100 identical extractions)
5. No ECS references in output
6. All numeric values translated to natural language
7. `PromptBuilder` produces valid `LLMRequest`
8. Unit tests pass (>80% coverage)
9. Integration tests pass
10. Build: 0 errors, 0 warnings

### 11.3 Metrics

| Metric | Target |
|--------|--------|
| Extraction time | < 10ms per entity |
| Output size | < 4000 tokens average |
| Test coverage | > 80% |
| Build | 0 errors, 0 warnings |
| Existing tests | 137/137 still pass |

---

## 12. What Sprint 2 Does NOT Include

- LLM adapter (Sprint 3)
- Narrative pipeline (Sprint 4)
- Actual world content (Sprint 5)
- Streaming responses
- Multi-entity extraction (one entity at a time for now)
- Caching of semantic states
- Configuration files for extraction rules

---

## 13. Open Decisions

| Decision | Status | Who resolves | When |
|----------|--------|-------------|------|
| Should extraction rules be data-driven (JSON config) or code-driven? | Open | Sprint 2 | During implementation |
| How to handle entities with very little data (minimal NPCs)? | Open | Sprint 2 | During testing |
| Should the extractor cache intermediate results within a tick? | Open | Sprint 2 | During profiling |
| How to handle conflicting beliefs (belief revision)? | Open | Sprint 2+ | After first working version |
| Should WorkingMemory be computed or stored? | Open | Sprint 2 | During implementation |
