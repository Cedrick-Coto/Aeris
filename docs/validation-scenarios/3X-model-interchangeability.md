# Validation Scenarios — 3X.3.2 Reasoning Strategy Swap (Intercambiabilidad)

**Status**: Active (validado por `ReasoningInterchangeabilityTests`, RI-001…RI-005)
**ID prefix**: RI-

```
Implements:
- CONTRACT-REASONING

Derived from:
- SPEC-3X.3.2 (variabilidad de estrategias de razonamiento)

Supports:
- Axioma A2 (variabilidad en el modelo: la estrategia de razonamiento es un punto de variación)

Background:
- RN-0001 (Working Memory)
- RN-0005 (Modelos Internos del Mundo)
```

---

## Propósito del micro-sprint

Demostrar que `IReasoningStrategy` puede intercambiarse **sin modificar** infraestructura ECS,
`ReasoningSystem`, Planning, Decision, Auditor, Enforcement ni contratos. El sistema de razonamiento
es una caja: entrada vía `ReasoningContext`, salida vía `ReasoningResult`, sin side effects.

Criterio de éxito: la sustitución de estrategia mantiene determinismo, trazabilidad, localidad causal
y separación de responsabilidades (no agrega capacidad, no cambia arquitectura).

## Estrategias bajo validación

| Estrategia | Criterio interno |
|------------|------------------|
| `EvidenceBasedReasoningStrategy` | Baseline: reglas explícitas registradas por keywords sobre premisas en WM |
| `AlternativeReasoningStrategy` | Alternativa: selección de premisas por saliencia (WM) / score (retrieval), agregación única `SalienceAnchoredInference`, sin matching de keywords |

---

## RI-001: Contrato común

Purpose
: Ambas estrategias cumplen `IReasoningStrategy` y producen `ReasoningResult` completo y válido
  (`Inference[]` y `ReasoningEvidence[]` con campos poblados y rangos correctos).

Input
: `ReasoningContext` con 4 chunks en WM y 1 retrieved memory.

Expected Output
: Para cada estrategia: ≥1 inferencia con `Id>0`, `RuleId`, `Transformation`, `Conclusion`,
  `Premises` no vacíos, `Confidence ∈ [0,1]`; evidencia con `InferenceId>0`, `RuleId`,
  `Transformation`, `Confidence ∈ [0,1]`, `EvidenceStrength ∈ [0,1]` y `Strategy == nombre de la estrategia`.

Forbidden
: Salida nula, listas vacías con premisas válidas, campos fuera de rango, `Strategy` no identificable.

## RI-002: Determinismo

Purpose
: Misma entrada + misma estrategia ⇒ mismo resultado serializado (una entrada, dos ejecuciones).

Forbidden
: Variación entre ejecuciones con estado idéntico (orden de iteración, hash, decimales, etc.).

## RI-003: Sin side effects sobre estado compartido

Purpose
: Al invocar `ReasoningSystem.Execute` (con ambas estrategias), los stores compartidos permanecen
  intactos: `WorkingMemoryStore`, `WorldModelState`, `GoalStore`, `AffectState`, `MemoryStore`.

Forbidden
: Cualquier mutación de WM, WorldModel, Goals, Affect o LTM por parte del razonamiento.

## RI-004: Localidad del efecto

Purpose
: El único estado que cambia al sustituir la estrategia es el dominio de razonamiento
  (`InferenceStore`: inferencias y evidencia). `PlanStore` y `ActionStore` permanecen exactamente
  iguales si las inferencias no alteran el resultado de planificación.

Forbidden
: Cambios en Planning o Decision provocados por el propio *swap* (frente a un cambio de modelo legítimo).

## RI-005: Pipeline completo

Purpose
: Pipeline Perception→Attention→MemoryRetrieval→WorkingMemory→Reasoning→Planning→Decision con ambas
  estrategias: invariantes de inferencia satisfechos, cadena de trazas completa (7 sistemas),
  trace de Reasoning identifica la estrategia, outputs downstream y estado final idénticos
  (la diferencia queda confinada a inferencias/evidencia).

Forbidden
: Trazas incompletas, decisión no contemplada (`Selected`/`NoViablePlan`), diferencias fuera del
  dominio de razonamiento.

---

## Failure Modes (generales)

```
❌ Nueva estrategia requiere modificar ReasoningSystem, Planning, Decision, Auditor o infraestructura.
❌ Estrategia escribe en WM, LTM, WorldModel, AffectState, GoalStore o ActionStore.
❌ Estrategia consulta ECS directamente (World/Entities) en lugar de ReasoningContext.
❌ Salida incompleta o no determinista.
❌ Contrato documental alterado para acomodar la estrategia.
```

---

## Trazabilidad

```
CONTRACT-REASONING
      ↓
SPEC-3X.3.2 (variabilidad de estrategias)
      ↓
SCENARIO RI-001 … RI-005
      ↓
TEST RI_001 … RI_005 (ReasoningInterchangeabilityTests)
      ↓
EXP-0002 (evidencia experimental)
```
