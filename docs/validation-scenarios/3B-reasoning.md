# Validation Scenarios — 3B.2 Reasoning

**Status**: Draft (depende de 3B.1)
**ID prefix**: R-

```
Implements:
- CONTRACT-REASONING

Derived from:
- SPEC-3B.2 (doc-17 §Reasoning)

Supports:
- H-0006 (Horizonte de planificación truncado)

Background:
- RN-0001 (Working Memory)
- RN-0005 (Modelos Internos del Mundo)
```

---

## Escenarios previstos

| ID | Escenario | Propósito |
|----|-----------|-----------|
| R-001 | Causal inference from WM | Dado "A → B" en WM, inferir que B probablemente ocurrirá |
| R-002 | Deductive inference | Dada una regla general en LTM y un hecho en WM, inferir conclusión |
| R-003 | Abductive inference | Dado efecto observado, inferir causa probable |
| R-004 | Analogical transfer | Dada situación similar en el pasado, inferir mismo resultado |
| R-005 | Affect modulation — Stress | Stress alto → inferencias más simples (menos saltos causales) |
| R-006 | Affect modulation — Curiosity | Curiosity alto → más hipótesis generadas |
| R-007 | Affect modulation — Confidence | Confidence bajo → inferencias conservadoras (sesgo a no inferir) |
| R-008 | No inference without premises | WM vacía → no se generan inferencias |
| R-009 | Multiple conflicting premises | Resolver conflicto por confidence o recency |
| R-010 | CandidateActions generation | Inferencias sobre acciones posibles → CandidateActions[] |
| R-011 | Trace logging | Todas las inferencias registradas en CognitiveTraceLog |
| R-012 | No side effects | Reasoning no escribe en LTM, WorldModel, AffectState, ni Goals |
| R-013 | Determinism | Mismas premisas → mismas inferencias |

---

## Esquema de un escenario (ejemplo R-001)

```text
Scenario: Causal inference from WM

Purpose
Verificar que una relación causal en WM genera una inferencia.

Initial World
- Un evento A ha ocurrido

Agent State
Working Memory:
  - Chunk: "Evento_A ocurrió"
  - Chunk: "Regla: Evento_A → probablemente Evento_B" (desde Retrieval)

LTM:
  - (no consultada directamente por Reasoning)

Affect:
  - Curiosity = 0.5
  - Stress = 0.2
  - Confidence = 0.6

Expected Output
Inferences[]:
  - Type: Causal
  - Premise: Evento_A
  - Conclusion: Evento_B probablemente ocurrirá
  - Confidence: (modulada por AffectState)

Forbidden
- Modificar WM, LTM, AffectState, Goals, WorldModel
```

---

## Failure Modes (generales)

```
❌ Reasoning consulta directamente LTM (debe usar WM con datos de Retrieval).
❌ Reasoning escribe en AffectState.
❌ Reasoning escribe en WorldModel.
❌ Reasoning modifica chunks de WM (solo puede leerlos).
❌ Reasoning ejecuta acciones.
❌ Reasoning modifica objetivos.
❌ Inference[] vacío cuando hay premisas válidas.
❌ Produce inferencias sin premisas en WM.
❌ No es determinista.
```

---

*Nota: los escenarios detallados se completarán cuando 3B.1 esté implementado y se sepa exactamente qué datos llegan a WM desde Retrieval.*
