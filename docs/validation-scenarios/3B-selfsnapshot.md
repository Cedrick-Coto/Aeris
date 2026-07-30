# Validation Scenarios — 3B.7 SelfSnapshot

**Status**: Index (to be detailed after 3B.6)
**ID prefix**: SS-

```
Implements:
- CONTRACT-SS

Derived from:
- SPEC-3B.7 (doc-17 §SelfSnapshot)

Supports:
- ADR-0006 (Self Model Is Reconstructed, Not Stored)

Background:
- RN-0003 (Identidad y Reconstrucción del Self)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| S-001 | Snapshot captures full state | SelfSnapshot contiene Affect, Goals, Memories, Identity |
| S-002 | Snapshot is immutable | Una vez generado, no se modifica durante el tick |
| S-003 | Snapshot per tick | Se genera exactamente un snapshot por tick |
| S-004 | Accessible to Narrative | El snapshot puede ser consumido por Narrative Pipeline |
| S-005 | Not persisted — ephemeral | El snapshot no se guarda en disco ni en LTM |
| S-006 | No side effects | SelfSnapshot no modifica ningún subsistema |
| S-007 | Determinism | Mismo estado interno → mismo snapshot |
