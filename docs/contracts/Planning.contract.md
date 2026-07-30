# CONTRACT-PLANNING

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.2  

---

## 1. Purpose

Definir el contrato para un subsistema capaz de transformar inferencias, estado del mundo y objetivos activos en **planes candidatos** — secuencias estructuradas de acciones hipotéticas que el sistema podría ejecutar.

**No define**:

- inteligencia general;
- conciencia;
- intencionalidad real;
- optimalidad.

Define una **interfaz experimental** para evaluar mecanismos de planificación artificial dentro de la arquitectura ACMA. Cualquier modelo de planificación futuro (baseline, basado en árboles, probabilístico, simbólico) debe satisfacer este contrato.

---

## 2. Position in causal chain

```
InferenceSet
      +
WorldModel
      +
Goals
      +
RetrievedMemory
      ↓
Planning
      ↓
CandidatePlan[]
      ↓
Decision
```

### Restricciones

- **Planning no ejecuta acciones.** La ejecución pertenece a Decision.
- **Planning no modifica el mundo.** Genera cursos de acción hipotéticos.
- **Planning no selecciona un plan.** Decision selecciona entre candidatos.
- **Planning no altera Goals, Memory, WorldModel ni Affect.**

---

## 3. Inputs

### PlanningContext

```csharp
PlanningContext
{
    Inference[]         Inferences;
    WorldModelState     WorldModel;
    GoalData[]          ActiveGoals;
    RetrievedMemory[]   RetrievedMemories;
    AffectState         AffectState;
}
```

### Puede leer

- Inferencias producidas por Reasoning en el mismo tick.
- Estado del modelo del mundo.
- Objetivos activos con prioridad y urgencia.
- Recuerdos recuperados relevantes para contexto.

### No puede leer

- ECS completo.
- Sistemas vecinos (Decision, Auditor, Identity).
- Estado no publicado de otros subsistemas.
- Memoria episódica completa (solo lo recuperado).

---

## 4. Outputs

### PlanningResult

```csharp
PlanningResult
{
    CandidatePlan[]     Plans;
    PlanningEvidence[]  Evidence;
}
```

### CandidatePlan

Cada plan debe contener:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | uint | Identificador único |
| `GoalId` | uint | Objetivo al que responde |
| `Steps` | PlanStep[] | Secuencia de acciones hipotéticas |
| `ExpectedOutcome` | string | Descripción del estado esperado |
| `Confidence` | float | Soporte interno [0, 1] |
| `Cost` | float | Coste estimado del plan |
| `Risk` | float | Riesgo estimado [0, 1] |

### PlanStep

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Index` | int | Posición en la secuencia |
| `Action` | string | Descriptor de la acción |
| `Prerequisite` | string | Condición necesaria para ejecutar |
| `ExpectedResult` | string | Estado esperado tras el paso |

### PlanningEvidence

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PlanId` | uint | ID del plan |
| `GoalId` | uint | Objetivo origen |
| `StepCount` | int | Número de pasos |
| `Strategy` | string | Nombre de la estrategia |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

### Ejemplo

```
Plan:
  Goal: Encontrar agua
  Steps:
    [1] MoveEast → "llegar a zona de musgo"
    [2] Search → "encontrar fuente de agua"
  ExpectedOutcome: "fuente de agua localizada"
  Confidence: 0.65
  Cost: 0.3
  Risk: 0.2
```

---

## 5. Baseline algorithm: Goal decomposition planning

Evitar empezar con planificación compleja. Proceso secuencial:

```
Active Goal
    ↓
Find possible actions
    ↓
Generate candidate sequences
    ↓
Estimate outcome
    ↓
Score candidates (viabilidad + preferencia separadas)
    ↓
Return ranked plans
```

### Reglas de generación baseline

| Regla | Entrada | Salida |
|-------|---------|--------|
| `direct-approach` | Goal + ubicación conocida del objetivo | Plan: dirigirse a ubicación |
| `explore-first` | Goal + ubicación desconocida | Plan: explorar área probable |
| `gather-resource` | Goal + recurso necesario conocido | Plan: obtener recurso → usar recurso |
| `defer-goal` | Goal + riesgo alto o recursos insuficientes | Plan: diferir (esperar mejor condición) |

### Scoring — viabilidad y preferencia separadas

```
viability = confidence × 0.6 + (1 - risk) × 0.4
preference = goalPriority × 0.5 + affectModulation × 0.3 + costEfficiency × 0.2

planScore = viability × 0.7 + preference × 0.3
```

La **viabilidad** responde a: ¿puede este plan lograr el objetivo?
La **preferencia** responde a: ¿qué tan deseable es este plan?

El afecto puede modular preferencia (prioridad, exploración, coste percibido), pero nunca la viabilidad ni la causalidad del mundo.

---

## 6. Invariants

### P-001 — Determinism

Mismo `PlanningContext` produce exactamente los mismos `CandidatePlan[]` en el mismo orden.

### P-002 — Goal-bound

Todo plan debe estar asociado a un `GoalId` válido. No se permiten planes sin objetivo.

### P-003 — No execution

Planning no ejecuta acciones, no modifica estado del mundo, no emite eventos de acción.

### P-004 — No side effects

Planning no modifica:
- Memory (LTM, WM);
- WorldModel;
- Goals;
- Affect;
- InferenceStore.

### P-005 — No hidden objectives

Planning no puede crear nuevos objetivos. Solo responde a `GoalId` existentes. La creación de objetivos pertenece a GoalSystem.

### P-006 — Cost honesty

`Cost` y `Risk` reflejan estimaciones internas basadas en información disponible, no valores arbitrarios. Deben poder reconstruirse desde la evidencia.

### P-007 — Separation from Decision

Planning genera `CandidatePlan[]`. Decision selecciona uno. Ningún plan implica ejecución automática.

---

## 7. Strategy abstraction

```csharp
IPlanningStrategy
{
    PlanningResult Plan(PlanningContext context);
}
```

```
PlanningSystem (ECS orchestrator)
        |
        ↓
IPlanningStrategy
        |
 ┌──────┴────────┐
 ↓               ↓
GoalDirected    Future Models
(Baseline)      (TreeSearch, MCTS,
                 HTN, RAP, ...)
```

El `PlanningSystem`:

1. Construye `PlanningContext` desde InferenceStore, WorldModel, GoalStore, WM.
2. Invoca `IPlanningStrategy`.
3. Escribe planes candidatos a `PlanStore`.
4. Emite CausalTrace.

---

## 8. Validation scenarios

### S-P001 — Plan para Goal alcanzable

**Entrada**:
```
Goal: Explorar bosque (prioridad: alta)
WorldModel: bosque al norte (conocido)
Inference: —
```

**Salida esperada**:
```
Plan: [MoveNorth → Explore]
Confidence: > 0
GoalId: asociado al goal de exploración
```

### S-P002 — Plan para Goal sin ubicación conocida

**Entrada**:
```
Goal: Encontrar agua (prioridad: media)
WorldModel: ubicación de agua desconocida
```

**Salida esperada**:
```
Plan: [Explore → Search]
Confidence: < direct-approach pero > 0
Tipo: explore-first
```

### S-P003 — Goal con riesgo alto produce defer

**Entrada**:
```
Goal: Cruzar río (prioridad: baja)
WorldModel: río caudaloso (riesgo alto)
```

**Salida esperada**:
```
Plan: [Defer] o [Wait]
Risk: alto
```

### S-P004 — Affect modulation

**Entrada**: Mismo mundo, mismo Goal, diferente AffectState (alta curiosidad vs. alto estrés).

**Salida esperada**: Diferente ranking de planes (preferencia), misma viabilidad (causalidad física no alterada). El afecto cambia prioridad relativa, no verdad del plan.

### S-P005 — No goal creation

**Entrada**: Sin objetivos activos (`GoalState` vacío).

**Salida esperada**: `PlanCandidate[]` vacío. No se crea un "objetivo de exploración" implícito. Eso pertenece a GoalSystem.

### S-P006 — Múltiples candidatos sin selección

**Entrada**: Goal alcanzable por dos caminos (corto/riesgoso vs. largo/seguro).

**Salida esperada**: Dos `CandidatePlan[]`. Ninguno seleccionado. Planning no elige.

### S-P007 — Goal imposible

**Entrada**:
```
Goal: Llegar a la montaña
WorldModel: sin camino conocido, sin inferencia de ruta
```

**Salida esperada**: `CandidatePlan[]` vacío. No se genera un plan inviable.

### S-P008 — Determinismo

Dos ejecuciones con mismo contexto producen mismos planes (mismo orden, mismos scores).

### S-P009 — Sin side effects

Ejecutar Planning no altera Goals, Memory, WorldModel, Affect ni InferenceStore.

### S-P010 — Reemplazabilidad

Cambiar `IPlanningStrategy` sin modificar `PlanningSystem`.

---

## 9. Criterio de éxito de 3B.3

No es:

> "Aeris sabe qué hacer."

Sino:

> Existe un mecanismo capaz de construir futuros hipotéticos explicables a partir del estado interno, manteniendo separación entre imaginación (Planning), evaluación (scoring) y acción (Decision).

### Métricas mínimas

- Build: 0 errores, 0 warnings.
- Tests: 100% pass (S-P001–S-P010).
- Rendimiento: planificación < 10ms con 3 goals activos.
- Cobertura baseline: al menos 4 reglas (direct-approach, explore-first, gather-resource, defer-goal).

---

## 10. Dependencies

- **CONTRACT-REASONING**: activo. Planning consume `Inference[]`.
- **Sprint 3B.2**: completado. Reasoning pipeline cerrado.
- **Sprint 3A**: completado. Planning consume `WorldModelState`, `GoalStore`.

---

## Historial

| Versión | Fecha | Cambio |
|---------|-------|--------|
| 0.1 | 2026-07-30 | Draft inicial |
| 0.2 | 2026-07-30 | Scoring separa viabilidad/preferencia; P-005 (no hidden objectives); S-P004 affect modulation; S-P005 no goal creation; S-P006 múltiples candidatos; S-P007 goal imposible; S-P010 reemplazabilidad |
