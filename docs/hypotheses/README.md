# Hipótesis de Investigación

Este directorio contiene hipótesis de investigación del proyecto Aeris.

---

## Diferencia entre ADR e Hipótesis

| | ADR | Hipótesis |
|---|---|---|
| **Propósito** | Documentar una decisión vigente | Documentar una creencia a validar |
| **Estado típico** | Accepted (vigente) | Proposed → Validated / Rejected |
| **Reversibilidad** | Baja (requiere nuevo ADR) | Alta (se espera experimentación) |
| **Impacto** | Afecta la arquitectura | Afecta la implementación |
| **Estabilidad** | Permanente hasta nuevo ADR | Temporal hasta validación |

Las hipótesis permiten explorar diseños sin comprometer la arquitectura. Son especialmente útiles en los sprints de investigación (Sprint 3 en adelante), donde muchas decisiones son experimentales.

Cada hipótesis se origina de una **Open Question** en `docs/17-computational-agent-model.md` y se respalda con **Research Notes** en `docs/research-notes/`. Este flujo mantiene separadas la evidencia (RN), la especulación (Hypothesis) y la decisión (ADR).

---

## Ciclo de vida

```
Propuesta
    ↓
Diseño del experimento
    ↓
Implementación
    ↓
Validación (métricas observables)
    ├── Validada → puede convertirse en ADR
    └── Rechazada → se documenta el resultado y se archiva
```

---

## Formato

Cada hipótesis sigue esta estructura:

```markdown
# H-0001: Título de la hipótesis

**Estado**: Proposed | Validated | Rejected
**Fecha**: YYYY-MM-DD
**Subsistemas afectados**: §X.Y, §X.Z
**Estado epistemológico**: E1–E4

## Enunciado

Enunciado claro de lo que se cree que sucederá.

## Motivación

Por qué es relevante para la arquitectura.

## Evidence Sources

Literatura relevante, referencias a RN.

## Experimento propuesto

Cómo se va a validar o refutar.

## Métricas

Variables observables para evaluar la hipótesis.

## Criterio de validación

Qué valores determinan éxito o fracaso.

## Posibles resultados

Tabla de resultados posibles e interpretación.

## Impacto arquitectónico si se valida

Qué cambios requiere en la especificación.

## Impacto arquitectónico si se rechaza

Qué cambios implica la refutación.
```

---

## Hipótesis activas

| ID | Tema | Subsistemas | Estado epistemológico | Estado |
|----|------|-------------|----------------------|--------|
| H-0001 | Working Memory chunking guiado por atención | WM (§3.4), Attention (§3.2) | E1 | Proposed |
| H-0002 | Presupuesto atencional dinámico por CognitiveLoad | Attention (§3.2), Affect (§3.3) | E1–E2 | Proposed |
| H-0003 | Frecuencia de reconstrucción de identidad | Identity (§3.12) | E3 | Proposed |
| H-0004 | WorldModel probabilístico vs simbólico | WorldModel (§3.6), Reasoning (§3.7) | E2 | Proposed |
| H-0005 | Prioridades de goals dinámicas | Goals (§3.8), Affect (§3.3) | E2 | Proposed |
| H-0006 | Horizonte de planificación truncado | Planning (§3.9), Decision (§3.10) | E2 | Proposed |
