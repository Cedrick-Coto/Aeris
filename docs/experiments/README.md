# Experiment Reports

Experimental evidence that feeds back into hypotheses, models, and ADRs.

Not tests. Tests verify contracts. Experiments evaluate hypotheses.

## Format

```yaml
ID:              EXP-NNNN
Objective:       What question does this experiment answer?
Hypothesis:      H-XXXX (link)
Model:           ACMA v1 (or other instance)
Engine version:  git commit or tag
Scenarios run:   S-001, S-002, ... (links)

Results:
  - Metric: <name>
    Value: <observed>

Observations:
  - What was expected vs what happened

Conclusion:
  Supports hypothesis | Weak support | Inconclusive | Contradicts hypothesis

Impact:
  - None / Hypothesis updated / Model updated / Contract updated / ADR created
```

## Lifecycle

```
Hypothesis
    ↓
Experiment
    ↓
Evidence
    ↓
├──► Hypothesis (updated status)
├──► Model (modified)
└──► ADR (if architectural change needed)
```

## Registry

| ID | Objective | Hypothesis | Model | Status |
|----|-----------|------------|-------|--------|
| [EXP-0002](EXP-0002.md) | Intercambiabilidad de estrategias de razonamiento (3X.3.2) | Axioma A2 (variabilidad en el modelo) | ACMA v1 | Complete |
