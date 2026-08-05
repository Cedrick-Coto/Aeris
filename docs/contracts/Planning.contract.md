# CONTRACT-PLANNING

**Estado**: Draft  
**Última actualización**: 2026-08-05  
**Versión**: 0.3  

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
WorkingMemory (incluye retrieved_*)
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

### Patrón de publicación de recuerdos

```
Memory Retrieval
→ publica chunks `retrieved_*` en WorkingMemory
→ los consumidores operan sobre WorkingMemory
```

Planning consume los recuerdos recuperados desde `WorkingMemory`; no existe un canal separado de recuerdos recuperados. Este patrón no introduce dependencias adicionales entre subsistemas.

---

## 3. Inputs

### PlanningContext

```csharp
PlanningContext
{
    Inference[]             AvailableInferences;
    WorldModelState         WorldModel;
    GoalData[]              ActiveGoals;
    WorkingMemoryChunk[]    WorkingMemory;
    AffectState             Affect;
}
```

### Puede leer

- Inferencias producidas por Reasoning en el mismo tick.
- Estado del modelo del mundo.
- Objetivos activos con prioridad y urgencia.
- WorkingMemory, incluyendo los chunks `retrieved_*` publicados por Memory Retrieval.

### No puede leer

- ECS completo.
- Sistemas vecinos (Decision, Auditor, Identity).
- Estado no publicado de otros subsistemas.
- Memoria episódica completa (WorkingMemory es un scratchpad acotado; no se expone la memoria completa).

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
| `Feasibility` | float | Viabilidad: estimación de si el plan puede lograr el objetivo [0, 1] |
| `Preference` | float | Preferencia: deseabilidad del plan según objetivo y afecto [0, 1] |
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
| `defer-goal` | Goal de baja urgencia y prioridad (`Urgency < 0.3`, `Priority ≤ Medium`) | Plan: diferir (esperar mejor condición) |
| `gather-resource` | *(capacidad futura)* Goal + recurso necesario conocido | Plan: obtener recurso → usar recurso |

`gather-resource` no está implementada en el baseline: su incorporación depende de la futura adición de un modelo de recursos (el modelo del mundo actual no modela recursos). Se mantiene como capacidad futura.

### Scoring — viabilidad y preferencia separadas

La **viabilidad** responde a: ¿puede este plan lograr el objetivo?
La **preferencia** responde a: ¿qué tan deseable es este plan?

El afecto puede modular preferencia (prioridad, exploración, coste percibido), pero nunca la viabilidad ni la causalidad del mundo.

Heurísticas del baseline (`GoalDirectedPlanningStrategy`):

```
feasibility = clamp(baseConfidence × 0.5
                     + locationKnown  (0.3 si hay entidades conocidas en el modelo del mundo, si no 0)
                     + inferenceBonus  (0.1 si hay inferencias disponibles, si no 0),
                     0, 1)

preference  = clamp(priorityScore × 0.4
                     + urgency × 0.3
                     + baseConfidence × 0.2
                     + curiosityBonus  (0.15 si Curiosity > 0.6, si no 0)
                     + threatPenalty   (-0.2 si Threat > 0.7, si no 0),
                     0, 1)
```

donde `priorityScore = goal.Priority / 5`.

**Ranking**: Planning ordena los candidatos principalmente por `Feasibility` descendente. La `Preference` no determina el orden en el baseline y no existe un `planScore` combinado. Decision aplica posteriormente su política de selección, utilizando preferencia cuando corresponda.

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

### P-006 — Estimaciones deterministas y documentadas

Las estimaciones producidas por la estrategia (`Confidence`, `Cost`, `Risk`, `Feasibility`, `Preference`) deben ser deterministas, documentadas y consistentes con el modelo implementado. Cada estrategia debe poder describir cómo produce sus estimaciones a partir de la información disponible. No se exige reconstrucción exacta desde la evidencia.

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

### S-P003 — Goal de baja urgencia y prioridad produce defer

**Entrada**:
```
Goal: Tarea opcional (prioridad: baja, urgencia: 0.2)
WorldModel: sin condiciones de riesgo (el baseline no modela riesgo)
```

**Salida esperada**:
```
Plan: [Defer]
```

El defer del baseline se dispara por la urgencia y prioridad del objetivo (`Urgency < 0.3`, `Priority ≤ Medium`), no por riesgo del mundo: el modelo actual no expone variables de riesgo.

### S-P004 — Affect modulation

**Entrada**: Mismo mundo, mismo Goal, diferente AffectState (alta curiosidad vs. alto estrés).

**Salida esperada**: Diferente ranking de planes (preferencia), misma viabilidad (causalidad física no alterada). El afecto cambia prioridad relativa, no verdad del plan.

### S-P005 — No goal creation

**Entrada**: Sin objetivos activos (`GoalState` vacío).

**Salida esperada**: `PlanCandidate[]` vacío. No se crea un "objetivo de exploración" implícito. Eso pertenece a GoalSystem.

### S-P006 — Múltiples candidatos sin selección

**Entrada**: Goal de baja urgencia y prioridad con ubicación conocida.

**Salida esperada**: Dos o más `CandidatePlan[]` (plan de logro + plan de diferimiento). Ninguno seleccionado. Planning no elige.

La generación de múltiples rutas alternativas de logro (p. ej. corto/riesgoso vs. largo/seguro) es propia de modelos futuros (HTN, MCTS, etc.) y no forma parte del baseline.

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
- Cobertura baseline: al menos 3 reglas implementadas (direct-approach, explore-first, defer-goal). `gather-resource` queda registrada como capacidad futura, dependiente de la incorporación de un modelo de recursos.

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
| 0.3 | 2026-08-05 | Reconciliación con el baseline implementado: PlanningContext expone `WorkingMemory` (patrón retrieved_*); CandidatePlan incorpora `Feasibility`/`Preference`; fórmulas de scoring sustituidas por heurísticas reales sin `planScore`; S-P003 y S-P006 reformulados a comportamiento verificable; P-006 reformulado; `gather-resource` registrada como capacidad futura |
