# CONTRACT-AUDITOR

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.2  

---

## 1. Purpose

Definir el contrato para un subsistema capaz de **evaluar cualquier artefacto contractual** contra un conjunto de reglas declaradas, sin modificar el artefacto ni decidir qué hacer con la evaluación.

**No define**:

- qué reglas debe tener un agente;
- qué hacer cuando una regla se viola;
- ética, moral o valores;
- una política de enforcement.

Define un **mecanismo de auditoría puro** que puede aplicarse a cualquier capa de la arquitectura.

---

## 2. Position in causal chain

```
Pipeline output
(DecisionResult | Inference | PlanCandidate | Goal | ...)
        ↓
Declared as IAuditableArtifact
        ↓
Auditor
        ↓
AuditResult
        ↓
Enforcement (future)
        ↓
Approved / Reject / RequestReplanning / Defer
```

### Restricciones

- **Auditor no modifica el artefacto.** (A-001)
- **Auditor no decide qué hacer con el resultado.** Solo evalúa.
- **Auditor no conoce Planning, Reasoning ni Decision.** Solo conoce el contrato `IAuditableArtifact`.
- **Auditor es agnóstico al contenido de las reglas.** Una regla de seguridad, experimental o ética son todas `IAuditRule`.

---

## 3. AuditableArtifact abstraction

```csharp
public interface IAuditableArtifact
{
    string ArtifactType { get; }      // e.g. "DecisionResult", "Inference"
    uint ArtifactId { get; }
}
```

Cualquier artefacto contractual puede declararse auditable implementando esta interfaz. En ACMA v1 el primer artefacto será `DecisionResult`, pero en el futuro pueden ser `Inference`, `PlanCandidate`, `Goal` o `SelfSnapshot` sin cambiar el contrato del Auditor.

### Primer artefacto: DecisionResultAuditable

```csharp
public sealed class DecisionResultAuditable : IAuditableArtifact
{
    public string ArtifactType => "DecisionResult";
    public uint ArtifactId { get; init; }
    public DecisionResult Decision { get; init; }
}
```

---

## 4. AuditRule abstraction

```csharp
public interface IAuditRule
{
    string RuleId { get; }
    string RuleVersion { get; }
    string Description { get; }
    string[] SupportedArtifactTypes { get; }
    AuditViolation? Evaluate(IAuditableArtifact artifact);
}
```

| Campo | Descripción |
|-------|-------------|
| `RuleId` | Identificador único de la regla |
| `RuleVersion` | Versión semántica de la regla |
| `Description` | Propósito legible de la regla |
| `SupportedArtifactTypes` | Tipos de artefacto que esta regla puede evaluar (ej. `["DecisionResult"]`) |
| `Evaluate` | Retorna `null` si el artefacto cumple; `AuditViolation` si viola o no es aplicable |

| Campo | Descripción |
|-------|-------------|
| `RuleId` | Identificador único de la regla |
| `RuleVersion` | Versión semántica de la regla |
| `Description` | Propósito legible de la regla |
| `Evaluate` | Retorna `null` si el artefacto cumple; `AuditViolation` si viola |

### RuleVerdict

```csharp
public enum RuleVerdict
{
    NotApplicable,   // La regla no corresponde al contexto del artefacto
    Satisfied,       // La regla se cumple
    Violated         // La regla se incumple
}
```

`NotApplicable` evita tener que asignar severidades a reglas que simplemente no correspondían al tipo de artefacto o contexto evaluado.

### AuditViolation

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RuleId` | string | Regla evaluada |
| `RuleVersion` | string | Versión de la regla |
| `Verdict` | `RuleVerdict` | Resultado de la evaluación |
| `Severity` | `ViolationSeverity` | Bajo, Medio, Alto, Crítico (solo si `Violated`) |
| `Condition` | string | Condición específica evaluada |
| `Evidence` | string | Evidencia que sustenta el veredicto |
| `ArtifactId` | uint | ID del artefacto evaluado |

### ViolationSeverity

```csharp
public enum ViolationSeverity
{
    Low,
    Medium,
    High,
    Critical
}
```

---

## 5. Inputs

### AuditContext

```csharp
public sealed class AuditContext
{
    public IAuditableArtifact Artifact { get; init; } = null!;
    public List<IAuditRule> Rules { get; init; } = new();
}
```

### Puede leer

- Un artefacto auditable (cualquier implementación de `IAuditableArtifact`).
- Un conjunto de reglas (`IAuditRule[]`).

### No puede leer

- ECS completo.
- Sistemas vecinos (Planning, Reasoning, Decision).
- Memoria, WorldModel, Affect, Goals.
- Estado privado de otros subsistemas.

---

## 6. Outputs

### AuditResult

```csharp
public sealed class AuditResult
{
    public bool Passed { get; init; }           // true si 0 violaciones
    public List<AuditViolation> Violations { get; init; } = new();
    public ViolationSeverity? MaxSeverity { get; init; }
    public AuditEvidence Evidence { get; init; }
}
```

### AuditEvidence

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ArtifactType` | string | Tipo del artefacto auditado |
| `ArtifactId` | uint | ID del artefacto |
| `RulesEvaluated` | int | Número de reglas aplicadas |
| `RulesPassed` | int | Reglas que el artefacto cumplió |
| `RulesFailed` | int | Reglas que el artefacto violó |
| `Strategy` | string | Nombre de la estrategia de auditoría |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

---

## 7. RuleRegistry

Infraestructura para registrar y resolver reglas. No es un subsistema ECS; es una capa de infraestructura que el `AuditSystem` consume.

```csharp
public sealed class RuleRegistry
{
    public void Register(IAuditRule rule);
    public List<IAuditRule> GetRulesFor(string artifactType);
}
```

- ACMA v1 registra sus reglas.
- ACMA v2 registra otras.
- MinimalAgent registra dos reglas.
- DevelopmentalAgent registra veinte.

El `Auditor` permanece idéntico.

---

## 8. Baseline algorithm: SequentialRuleEvaluator

```
For each rule in Rules:
    Evaluate(artifact)
    ↓
    if violation: collect it
    ↓
Next rule

↓

Build AuditResult:
    Passed = (violations.Count == 0)
    MaxSeverity = violations.Max(s)
```

El baseline ejecuta todas las reglas secuencialmente, sin cortocircuito (todas las reglas se evalúan aunque una falle).

---

## 9. Invariants

### A-001 — Pure evaluation

```text
Artifact_before == Artifact_after
```

El Auditor nunca muta el artefacto.

### A-002 — Determinism

Misma entrada (`Artifact` + `Rules`) produce exactamente el mismo `AuditResult`.

### A-003 — Rule identity

Toda violación referencia:
- `RuleId`
- `RuleVersion`

### A-004 — Evidence completeness

Toda violación explica:
- qué regla;
- qué condición;
- qué evidencia.

### A-005 — Independence

El Auditor no conoce:
- Planning;
- Reasoning;
- Decision;
- Goals;
- Memory;
- WorldModel;
- Affect.

Solo conoce el contrato `IAuditableArtifact` y `IAuditRule`.

---

## 10. Strategy abstraction

```csharp
public interface IAuditStrategy
{
    AuditResult Audit(AuditContext context);
}
```

```
AuditSystem (ECS orchestrator)
        |
        ↓
IAuditStrategy
        |
 ┌──────┴────────┐
 ↓               ↓
SequentialRule  Future Strategies
Evaluator       (Parallel, Priority-
(Baseline)       Based, FirstFailure, ...)
```

El `AuditSystem`:

1. Construye `AuditContext` desde el artefacto auditable y las reglas registradas.
2. Invoca `IAuditStrategy`.
3. Almacena `AuditResult` en `AuditStore`.
4. Emite CausalTrace.
5. No aplica enforcement.

---

## 11. Enforcement (separado, no incluido en este contrato)

**No forma parte del Auditor.**

El Enforcement es un subsistema aparte con su propio contrato y responsabilidad:

```
AuditResult
    ↓
EnforcementPolicy
    ↓
Approved
Reject (→ RequestReplanning)
Defer
Escalate
```

El Enforcement nunca modifica directamente el artefacto. Sus salidas son comandos dirigidos al engine o al planificador.

---

## 12. Validation scenarios

### S-A001 — Artifact pasa todas las reglas

**Entrada**: DecisionResult válido, reglas de seguridad básicas.
**Salida esperada**: `Passed = true`, `Violations` vacía.

### S-A002 — Artifact viola una regla

**Entrada**: DecisionResult que viola una regla (ej. acción peligrosa con alta preferencia).
**Salida esperada**: `Passed = false`, 1 violación con `RuleId`, `RuleVersion`, `Condition`, `Evidence`.

### S-A003 — Múltiples reglas, múltiples violaciones

**Entrada**: DecisionResult que viola 3 reglas.
**Salida esperada**: 3 violaciones, `MaxSeverity` = la más alta.

### S-A004 — Pure evaluation (A-001)

**Entrada**: Cualquier artefacto.
**Salida esperada**: El artefacto original está intacto después de `Audit()`.

### S-A005 — Determinism (A-002)

Mismo `AuditContext` produce mismo `AuditResult`.

### S-A006 — Rule identity (A-003)

Toda violación contiene `RuleId` y `RuleVersion` no vacíos.

### S-A007 — Independence (A-005)

El Auditor no accede a recursos ECS como WorldModel, GoalStore, AffectState.

### S-A008 — Reemplazabilidad

Cambiar `IAuditStrategy` sin modificar `AuditSystem`.

### S-A009 — Sin enforcement

El Auditor nunca produce comandos de bloqueo, modificación o replanificación. Solo evaluación.

---

## 13. Criterio de éxito de 3B.5

> **Existe un mecanismo de auditoría puro que puede evaluar cualquier artefacto contractual contra un conjunto intercambiable de reglas, sin modificar el artefacto ni decidir la respuesta.**

### Métricas mínimas

- Build: 0 errores, 0 warnings.
- Tests: 100% pass (S-A001–S-A009).
- Rendimiento: auditoría < 1ms con 10 reglas.

---

## 14. Dependencies

- **Sprint 3B.4**: completado. Primer artefacto auditable: `DecisionResult`.
- **Sprint 3B.1–3B.3**: completados. Artefactos futuros auditables: `Inference`, `PlanCandidate`.

---

## Historial

| Versión | Fecha | Cambio |
|---------|-------|--------|
| 0.1 | 2026-07-30 | Draft inicial |
| 0.2 | 2026-07-30 | +RuleVerdict (NotApplicable/Satisfied/Violated), +RuleRegistry, secciones renumeradas |
