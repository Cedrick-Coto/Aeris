# Formal Contracts

Each contract defines exclusively: **inputs, outputs, invariants, complexity, determinism, dependencies, and forbidden side effects** for a cognitive subsystem.

No theory. No implementation. No literary justification. Only the contract.

## Files

| Contract | Subsystem | Sprint | Status |
|----------|-----------|--------|--------|
| `Perception.contract.md` | PerceptionSystem | 3A | Active |
| `Attention.contract.md` | AttentionSystem | 3A | Active |
| `WorkingMemory.contract.md` | WorkingMemorySystem | 3A | Active |
| `LongTermMemory.contract.md` | LongTermMemorySystem | 3A | Active |
| `Affect.contract.md` | AffectSystem | 3A | Active |
| `Goals.contract.md` | GoalSystem | 3A | Active |
| `WorldModel.contract.md` | WorldModelSystem | 3A | Active |
| `MemoryRetrieval.contract.md` | MemoryRetrievalSystem | 3B.1 | Draft |
| `Reasoning.contract.md` | ReasoningSystem | 3B.2 | Draft |
| `Planning.contract.md` | PlanningSystem | 3B.3 | Draft |
| `Decision.contract.md` | DecisionSystem | 3B.4 | Draft |
| `Auditor.contract.md` | AuditorSystem | 3B.5 | Draft |
| `Enforcement.contract.md` | EnforcementSystem | 3B.5 | Draft |
| `IdentityReconstruction.contract.md` | IdentityReconstructionSystem | 3B.6 | Draft |
| `CausalTrace.contract.md` | CausalTrace (observador transversal) | 3B transversal | Draft |
| `Knowledge.contract.md` | KnowledgeUpdateSystem | 3X | Draft |

## Contract lifecycle

| Status | Meaning |
|--------|---------|
| Draft | Proposed, not yet approved |
| Active | Approved, matches implementation |
| Deprecated | Superseded, kept for history |
| Replaced | Superseded, links to replacement |
| Archived | No longer relevant, kept for record |

Never reuse an ID. If a contract is replaced, mark it Replaced and create a new ID.
