# Validation Scenarios — 3B.4 Decision

**Status**: Index (to be detailed after 3B.3)
**ID prefix**: D-

```
Implements:
- CONTRACT-DECISION

Derived from:
- SPEC-3B.4 (doc-17 §Decision)

Background:
- RN-0006 (Planificación en Agentes Cognitivos)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| D-001 | Select next action from plan | Plan disponible → extraer primera acción |
| D-002 | No plan → no action | Sin plan → no se ejecuta acción |
| D-003 | Affect modulation — Urgency alta | Decision acelera selección (menos evaluación) |
| D-004 | Affect modulation — Confidence bajo | Decision reevalúa o pospone |
| D-005 | Constraint satisfaction | Acción respeta constraints del agente |
| D-006 | Action rejection | Acción que viola constraint → no se selecciona |
| D-007 | Trace logging | Acción seleccionada registrada en CognitiveTraceLog |
| D-008 | No side effects | Decision no modifica plan, WM, LTM, Affect, Goals |
| D-009 | Determinism | Mismo plan + estado → misma acción |
