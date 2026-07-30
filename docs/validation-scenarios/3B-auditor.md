# Validation Scenarios — 3B.5 Auditor

**Status**: Index (to be detailed after 3B.4)
**ID prefix**: A-

```
Implements:
- CONTRACT-AUDITOR

Derived from:
- SPEC-3B.5 (doc-17 §Auditor)

Background:
- ADR-0008 (Affect Is Functional)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| A-001 | Approve valid action | Acción respeta principios → ConflictReport vacío |
| A-002 | Reject conflicting action | Acción viola principio → ConflictReport con al menos un conflicto |
| A-003 | Correction suggestion | Auditor propone acción alternativa |
| A-004 | Affect modulation | Stress alto → auditor más permisivo (umbral más alto) |
| A-005 | Multiple conflicts | Reporte enumera todos los conflictos, no solo el primero |
| A-006 | Trace logging | Auditoría registrada en CognitiveTraceLog |
| A-007 | No side effects | Auditor no modifica plan, WM, LTM, Affect, Decision |
| A-008 | Determinism | Misma acción + principios → mismo reporte |
