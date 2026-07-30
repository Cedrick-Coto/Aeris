# Validation Scenarios — 3B.3 Planning

**Status**: Index (to be detailed after 3B.2)
**ID prefix**: P-

```
Implements:
- CONTRACT-PLANNING

Derived from:
- SPEC-3B.3 (doc-17 §Planning)

Supports:
- H-0006 (Horizonte de planificación truncado)

Background:
- RN-0006 (Planificación en Agentes Cognitivos)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| P-001 | Plan generation from active goal | Goal activo → se genera un plan |
| P-002 | No goal → no plan | Sin goal activo → no se genera plan |
| P-003 | Forward simulation in WorldModel | Plan se evalúa simulando en WorldModel |
| P-004 | Cost/benefit selection | Múltiples planes → seleccionar el mejor |
| P-005 | Affect modulation — Confidence bajo | Planes cortos (menos pasos) |
| P-006 | Affect modulation — Threat alto | Planes conservadores (evitar riesgo) |
| P-007 | Affect modulation — Curiosity alto | Planes exploratorios (rutas alternativas) |
| P-008 | Horizonte truncado | Plan no supera H-0006 max steps |
| P-009 | Trace logging | Plan generado registrado en CognitiveTraceLog |
| P-010 | No side effects | Planning no modifica WM, LTM, Affect, Goals, WorldModel |
| P-011 | Determinism | Mismo goal + estado → mismo plan |
