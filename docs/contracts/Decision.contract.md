# CONTRACT-DECISION

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.2  

---

## 1. Purpose

Definir el contrato para un subsistema capaz de **seleccionar una acción** entre cursos alternativos generados por Planning, manteniendo separadas posibilidad, preferencia y ejecución.

**No define**:

- libre albedrío;
- motivación humana;
- optimalidad universal;
- política moral.

Define una **interfaz experimental** para evaluar mecanismos de selección artificial dentro de la arquitectura ACMA. Cualquier política de selección futura debe satisfacer este contrato para ser intercambiable.

---

## 2. Position in causal chain

```
CandidatePlan[]
      +
WorldModel
      +
AffectState
      +
Goals
      ↓
Decision
      ↓
SelectedAction
      ↓
Action execution
```

### Restricciones

- **Decision no genera planes.** Solo selecciona entre candidatos existentes.
- **Decision no ejecuta acciones.** Emite `SelectedAction` como evento; la ejecución pertenece al motor.
- **Decision no modifica Feasibility de los planes.** La viabilidad es propiedad del plan, determinada por Planning.
- **Decision no altera Goals, Memory, WorldModel ni Affect.**

---

## 3. Inputs

### DecisionContext

```csharp
DecisionContext
{
    PlanCandidate[]    CandidatePlans;
    WorldModelState    WorldModel;
    AffectState        Affect;
    GoalData[]         ActiveGoals;
}
```

### Puede leer

- Planes candidatos producidos por Planning en el mismo tick.
- Estado del mundo.
- Estado afectivo actual.
- Objetivos activos.

### No puede leer

- ECS completo.
- Sistemas vecinos (Planning, Auditor, Identity).
- Memoria no recuperada.
- Estado privado de otros subsistemas.

---

## 4. Outputs

### DecisionStatus

```csharp
enum DecisionStatus
{
    Selected,        // Se eligió un plan viable
    Deferred,        // Se difiere la decisión (p.ej. esperar más información)
    NoViablePlan     // Ningún plan superó el umbral de viabilidad
}
```

"No decidir también es un resultado válido". Decision nunca inventa un nuevo plan cuando no hay viables.

### DecisionResult

```csharp
DecisionResult
{
    DecisionStatus     Status;
    uint?              SelectedPlanId;     // null si no se seleccionó plan
    SelectedAction     Action;             // Puede ser "Defer" o nulo si no hay acción
    SelectionEvidence  Evidence;
}
```

### SelectedAction

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PlanId` | uint | Plan del que se originó la acción |
| `Action` | string | Descriptor de la acción seleccionada |
| `GoalId` | uint | Objetivo al que responde |
| `Confidence` | float | Confianza en la selección [0, 1] |

### SelectionReason (trace estructurado)

```csharp
SelectionReason
{
    string              Policy;           // Nombre de la política usada
    float               Threshold;        // Umbral de viabilidad aplicado
    RejectedPlan[]      Rejected;         // Planes que no superaron el umbral
    SelectedPlan        Selected;         // Plan seleccionado (si aplica)
    string              TieBreaker;       // Criterio de desempate (si aplicó)
}
```

### SelectionEvidence

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Status` | DecisionStatus | Resultado de la decisión |
| `CandidatesConsidered` | int | Número de candidatos evaluados |
| `SelectionPolicy` | string | Nombre de la política usada |
| `Threshold` | float | Umbral de viabilidad aplicado |
| `Reason` | SelectionReason | Trazabilidad estructurada de la selección |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

### Ejemplo

```
CandidatePlans:
  Plan A: [MoveNorth → Explore]  F:0.9 P:0.4
  Plan B: [Explore → Search]     F:0.5 P:0.9

SelectionPolicy: feasibilityThreshold
Threshold: 0.65

Rejected:
  Plan B (feasibility 0.5 < 0.65)

Selected:
  Plan A (feasibility 0.9, preference 0.4)

Status: Selected
```

Caso sin viables:

```
CandidatePlans:
  Plan A: F:0.2 P:0.9
  Plan B: F:0.1 P:0.8

Status: NoViablePlan
Action: Defer
Reason: "No plan meets feasibility threshold 0.50"
```

---

## 5. Baseline algorithm: FeasibilityThresholdPolicy

Proceso:

```
Filter viable plans (feasibility ≥ threshold)
    ↓
If none: emit defer/wait action
    ↓
If one: select it
    ↓
If multiple: select by highest preference
    ↓
Emit selected action + evidence
```

### Umbral de viabilidad baseline

```
threshold = 0.5 - (stress × 0.2) + (confidence × 0.1)

threshold mínimo: 0.2 (estrés alto puede relajar umbral)
threshold máximo: 0.8 (confianza alta puede endurecerlo)
```

El estrés puede hacer que Decision acepte planes menos viables (presión por actuar). Pero nunca puede hacer que un plan inviable (Feasibility < 0.2) sea seleccionable.

### Regla de desempate

Si múltiples planes superan el umbral, se selecciona el de mayor `Preference`. Si hay empate, se selecciona el de menor `Risk`.

---

## 6. Invariants

### D-001 — Determinism

Mismo `DecisionContext` produce exactamente el mismo `SelectedAction`.

### D-002 — No plan generation

Decision nunca genera nuevos planes ni modifica los candidatos existentes. Solo selecciona.

### D-003 — No feasibility override

Decision no puede alterar `Feasibility` de ningún plan. La viabilidad es propiedad del plan.

### D-004 — No side effects

Decision no modifica:
- Goals;
- Memory (LTM, WM);
- WorldModel;
- Affect;
- InferenceStore;
- PlanStore.

### D-005 — Always selects or defers

Decision siempre produce una acción. Si ningún plan es viable, emite `Defer` con justificación.

### D-006 — Evidence completeness

Toda selección debe documentar cuántos candidatos fueron considerados y por qué se seleccionó el elegido.

---

## 7. Strategy abstraction

```csharp
IDecisionStrategy
{
    DecisionResult Decide(DecisionContext context);
}
```

```
DecisionSystem (ECS orchestrator)
        |
        ↓
IDecisionStrategy
        |
 ┌──────┴────────┐
 ↓               ↓
Feasibility     Future Policies
Threshold       (ExpectedUtility,
(Baseline)       RiskAverse,
                Exploratory, ...)
```

El `DecisionSystem`:

1. Construye `DecisionContext` desde PlanStore, WorldModel, AffectState, GoalStore.
2. Invoca `IDecisionStrategy`.
3. Registra la acción seleccionada en `ActionStore`.
4. Emite CausalTrace.

---

## 8. Validation scenarios

### S-D001 — Selección por viabilidad

**Entrada**:
```
Plan A: F:0.9 P:0.3
Plan B: F:0.4 P:0.8
```

**Salida esperada**: Plan A seleccionado (el único sobre umbral baseline 0.5).

### S-D002 — Selección por preferencia entre viables

**Entrada**:
```
Plan A: F:0.8 P:0.3
Plan B: F:0.8 P:0.7
```

**Salida esperada**: Plan B seleccionado (mayor preferencia entre igualmente viables).

### S-D003 — Ningún plan viable produce NoViablePlan

**Entrada**:
```
Plan A: F:0.2 P:0.9
Plan B: F:0.1 P:0.8
```

**Salida esperada**: `Status = NoViablePlan`, `Action = "Defer"`, candidatesConsidered = 2.

### S-D004 — Estrés relaja umbral

**Entrada**: Mismos planes con `Stress = 0.1` vs `Stress = 0.9`.

**Salida esperada**: Con estrés bajo, plan F:0.4 es inviable. Con estrés alto, plan F:0.4 se vuelve seleccionable.

### S-D005 — No plan generation

Decision nunca añade nuevos `PlanCandidate[]`. Solo selecciona o difiere.

### S-D006 — Reemplazabilidad

Cambiar `IDecisionStrategy` sin modificar `DecisionSystem`.

### S-D007 — Determinismo

Dos ejecuciones con mismo contexto producen misma acción seleccionada.

### S-D008 — Sin side effects

Ejecutar Decision no altera Goals, Memory, WorldModel, Affect, ni PlanStore.

---

## 9. Criterio de éxito de 3B.4

No es "Aeris decide".

Es:

> Existe un mecanismo de selección de acciones que puede elegir entre futuros hipotéticos manteniendo separadas posibilidad (Feasibility), preferencia (Preference) y ejecución (Action).

### Métricas mínimas

- Build: 0 errores, 0 warnings.
- Tests: 100% pass (S-D001–S-D008).
- Rendimiento: selección < 2ms con 10 planes candidatos.
- Cobertura baseline: umbral de viabilidad, desempate por preferencia, defer cuando ningún plan es viable.

---

## 10. Dependencies

- **CONTRACT-PLANNING v0.3**: activo. Decision consume `CandidatePlan[]`.
- **Sprint 3B.3**: completado. Planning pipeline cerrado.
- **Sprint 3A**: completado. Decision consume `WorldModelState`, `AffectState`, `GoalStore`.

---

## Historial

| Versión | Fecha | Cambio |
|---------|-------|--------|
| 0.1 | 2026-07-30 | Draft inicial |
