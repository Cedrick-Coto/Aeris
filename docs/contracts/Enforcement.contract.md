# CONTRACT-ENFORCEMENT

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.1  

---

## 1. Purpose

Definir el contrato para un subsistema capaz de **aplicar una política a un resultado de auditoría**, produciendo una decisión sobre qué hacer con la evaluación, sin reinterpretar el `AuditResult` ni modificar el artefacto original.

**No define**:

- qué política debe usar un agente;
- cómo evaluar reglas de auditoría;
- cómo transformar acciones;
- cómo reinterpretar violaciones.

Define un **mecanismo de enforcement puro** que consume el resultado del Auditor y produce una decisión de política.

---

## 2. Position in causal chain

```
Decision
    ↓
Auditor
    ↓
AuditResult
    ↓
Enforcement
    ↓
EnforcementResult
```

### Restricciones

- **Enforcement no modifica el `AuditResult`.** (E-001)
- **Enforcement nunca consulta el mundo ni otros subsistemas.** Solo consume `AuditResult`. (E-002)
- **Enforcement nunca crea nuevas violaciones.** Solo interpreta las existentes. (E-004)
- **Enforcement no transforma acciones.** Sus salidas son comandos: `Approve`, `Reject`, `RequestReplanning`, `Defer`.

---

## 3. Inputs

### EnforcementContext

```csharp
public sealed class EnforcementContext
{
    public AuditResult AuditResult { get; init; } = null!;
}
```

### Puede leer

- Un `AuditResult`.

### No puede leer

- ECS completo.
- `DecisionResult` u otros artefactos.
- Sistemas vecinos (Planning, Reasoning, Decision, Auditor).
- Memoria, WorldModel, Affect, Goals.
- `RuleRegistry` o reglas individuales.

---

## 4. Outputs

### EnforcementResult

```csharp
public enum EnforcementVerdict
{
    Approve,           // La decisión es aceptada
    Reject,            // La decisión es rechazada (solicitar replanificación)
    RequestReplanning, // La decisión se rechaza y se pide un nuevo plan
    Defer              // La decisión se difiere (no ejecutar ahora)
}
```

### EnforcementEvidence

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Verdict` | `EnforcementVerdict` | Decisión de la política |
| `Policy` | string | Nombre de la política usada |
| `ViolationCount` | int | Número de violaciones consideradas |
| `MaxSeverity` | `ViolationSeverity?` | Severidad máxima considerada |
| `Reason` | string | Breve justificación |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

---

## 5. Policy abstraction

```csharp
public interface IEnforcementPolicy
{
    string Name { get; }
    EnforcementResult Apply(EnforcementContext context);
}
```

```
EnforcementSystem (ECS orchestrator)
        |
        ↓
IEnforcementPolicy
        |
 ┌──────┴──────────────────┐
 ↓                         ↓
StrictPolicy              Future Policies
(PermissivePolicy,         (SafetyFirstPolicy,
 baseline)                 ExperimentalPolicy, ...)
```

El `EnforcementSystem`:

1. Construye `EnforcementContext` desde `AuditStore`.
2. Invoca `IEnforcementPolicy`.
3. Almacena `EnforcementResult` en `EnforcementStore`.
4. Emite CausalTrace.

---

## 6. Baseline policies

### StrictPolicy

```
if any violation with severity >= High → Reject
if any violation                      → RequestReplanning
if passed                             → Approve
```

### PermissivePolicy

```
if any violation with severity == Critical → Reject
if any violation with severity >= High     → RequestReplanning
otherwise                                  → Approve
```

### SafetyFirstPolicy

```
if SAFETY-* rule violated → Reject
if any violation         → RequestReplanning
if passed                → Approve
```

---

## 7. Invariants

### E-001 — AuditResult immutability

```text
AuditResult_before == AuditResult_after
```

El Enforcement nunca modifica el `AuditResult`.

### E-002 — World isolation

El Enforcement nunca consulta directamente el mundo ni otros subsistemas. Solo consume `AuditResult`.

### E-003 — Determinism

Misma política + mismo `AuditResult` produce exactamente el mismo `EnforcementResult`.

### E-004 — No violation creation

El Enforcement nunca crea nuevas violaciones; solo interpreta las existentes.

### E-005 — Policy replaceability

Las políticas son reemplazables mediante `IEnforcementPolicy`.

---

## 8. Validation scenarios

### S-E001 — Aprobación

**Entrada**: `AuditResult.Passed = true`, política Strict.
**Salida esperada**: `Approve`.

### S-E002 — Rechazo por severidad alta

**Entrada**: `AuditResult` con violación `Critical`, política Strict.
**Salida esperada**: `Reject`.

### S-E003 — Replanificación

**Entrada**: `AuditResult` con violación `Low`, política Strict.
**Salida esperada**: `RequestReplanning`.

### S-E004 — Permissive aprueba violación baja

**Entrada**: `AuditResult` con violación `Low`, política Permissive.
**Salida esperada**: `Approve`.

### S-E005 — Política reemplazable

Misma entrada, distinta política → distinto resultado, sin cambiar infraestructura.

### S-E006 — Determinismo

Misma política + mismo `AuditResult` → mismo `EnforcementResult`.

### S-E007 — Sin modificación de AuditResult

Después de `Apply()`, el `AuditResult` original está intacto.

---

## 9. Criterio de éxito de 3B.5B

> **Existe un mecanismo de enforcement puro que aplica políticas intercambiables a resultados de auditoría, sin reinterpretar violaciones ni consultar el mundo.**

### Métricas mínimas

- Build: 0 errores, 0 warnings.
- Tests: 100% pass (S-E001–S-E007).
- Rendimiento: enforcement < 100μs.

---

## 10. Dependencies

- **Sprint 3B.5A**: completado. Enforcement consume `AuditResult`.
- **Contrato Auditor**: activo.
- **Contrato Decision**: activo.

---

## Historial

| Versión | Fecha | Cambio |
|---------|-------|--------|
| 0.1 | 2026-07-30 | Draft inicial |
