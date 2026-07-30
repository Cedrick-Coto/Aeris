# Auditor — Formal Contract

**ID**: CONTRACT-AUDITOR
**Status**: Draft

## Inputs
- Selected Action
- Inferences from current tick
- `AffectState`
- Active Goals
- `SelfSnapshot` (from previous tick)

## Outputs
- `ConflictReport[]` — detected conflicts
- `Correction[]` — suggested corrections

## Report Structure
- Severity: float [0, 1]
- Source: which subsystem originated the conflict
- Description: conflict type
- Suggestion: proposed correction (if applicable)

## Audit Scope
- Logical consistency of inferences
- Coherence between action and registered principles
- Biases from excessive affective state
- Alignment with long-term goals

## Invariants
- AuditorSystem does not modify any state directly
- Its outputs are suggestions; other systems decide whether to apply them
- Auditor runs after Decision, before Memory consolidation

## Complexity
- O(A + I) where A = actions, I = inferences in current tick

## Determinism
- Fully deterministic

## Dependencies
- DecisionSystem (output)
- ReasoningSystem (output, read-only)
- AffectSystem (read-only)
- GoalSystem (read-only)
- IdentityReconstructionSystem (read-only, previous snapshot)

## Forbidden Side Effects
- Modifying any cognitive state directly
- Executing actions
- Emitting events to EventBus
