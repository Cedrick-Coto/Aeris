# Causal Trace — Formal Contract

**ID**: CONTRACT-CT
**Status**: Draft (Sprint 3B transversal)

## Rationale

CausalTrace is not a cognitive subsystem. It is an observability artifact that records the causal chain across all subsystems without participating in the simulation. It separates **explanation** from **execution** — the trace is a verifiable artifact independent of the agent's decisions.

## Interface

```
Input (passive observation):
    World                 (read-only)
    Tick
    SubsystemOutput[]     (output of every subsystem this tick)

Output:
    CausalRecord[]        (ordered DAG of causal records for this tick)
```

### CausalRecord structure

```
CausalRecord:
    TraceId               (unique, stable — never reused)
    ParentTraceId         (links to prior record in the causal chain, null if root)
    Tick
    Subsystem             (name of the producing subsystem)
    EntityId              (agent or entity this record refers to)

    TransitionType:       (enum)
        Observed          — direct perception of world state
        Retrieved         — memory recall from LTM
        Inferred          — reasoning step (causal, deductive, abductive, analogical)
        Predicted         — forward simulation / expected outcome
        Evaluated         — utility / cost-benefit computation
        Selected          — decision / action chosen
        Reconstructed     — identity reconstruction step

    EvidenceStrength:     [0.0, 1.0]
                          Internal soundness of this transition.
                          Not absolute certainty — represents the coherence
                          of the computation given available data.

    Inputs                (references to World state + prior outputs)
    Computation           (rule or algorithm applied)
    Outputs
    DecisionMetrics       (utilities, weights, confidence)
    Evidence              (direct observations)
    RetrievedMemories     (LTM references)
    Assumptions           (active hypotheses)
    InferenceSteps        (reasoning chain, if applicable)
```

### Causal DAG

ParentTraceId + TraceId form a directed acyclic graph of causality:

```
Perception (TraceId=P1, ParentTraceId=null)
    │
    ▼
Attention (TraceId=A1, ParentTraceId=P1)
    │
    ▼
MemoryRetrieval (TraceId=MR1, ParentTraceId=A1)
    │
    ▼
Reasoning (TraceId=R1, ParentTraceId=MR1)
    │
    ├────► Decision (TraceId=D1, ParentTraceId=R1, TransitionType=Selected)
    │
    └────► Justification (TraceId=J1, ParentTraceId=R1, TransitionType=Inferred)
```

This enables:
- Cognitive replay: step-by-step reconstruction of any decision
- Cross-model comparison: same initial state, compare ACMA v1 vs v2 traces
- Experimental analysis: measure which hypotheses produce more consistent causal chains
- Visualization: auto-generate reasoning graph per tick or sequence

## Invariants

- Never modifies World
- Never participates in simulation
- Deterministic (same tick → same CausalRecord DAG)
- Serializable (can be persisted and reconstructed offline)
- Zero side effects (disable CausalTrace → identical simulation outcome)
- TraceId never reused; if a record is deprecated, a new TraceId is issued

## Structure

```
Subsystem
    │
    ├────► Next Subsystem
    │
    └────► CausalTrace Collector (observes only)
```

## Dependencies

- All subsystems (passive observation, read-only)

## Forbidden Side Effects

- Writing to World
- Writing to any subsystem state
- Writing to EventBus
- Altering execution order
- Consuming decision output
