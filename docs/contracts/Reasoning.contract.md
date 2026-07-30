# CONTRACT-REASONING

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.2  

---

## 1. Purpose

Definir el contrato para un subsistema capaz de transformar información disponible en el estado cognitivo en inferencias estructuradas y trazables.

**No define**:

- inteligencia general;
- conciencia;
- comprensión humana;
- verdad de las inferencias.

Define una **interfaz experimental** para evaluar mecanismos de inferencia artificial dentro de la arquitectura ACMA. Cualquier modelo de razonamiento futuro (simbólico, probabilístico, analógico, conexionista) debe satisfacer este contrato para ser intercambiable.

---

## 2. Position in causal chain

```
WorkingMemory
      +
RetrievedMemory
      +
WorldModel
      +
Goals
      ↓
Reasoning
      ↓
InferenceSet
      ↓
Planning / Decision
```

### Restricciones

- **Reasoning no ejecuta acciones.** La ejecución pertenece a Decision.
- **Reasoning no modifica memoria permanente.** Planning y sistemas aguas abajo deciden qué acciones tomar basándose en las inferencias producidas.
- **Reasoning no modifica WorldModel.** El WorldModel se actualiza mediante percepción y consolidación, no mediante inferencia especulativa.

---

## 3. Inputs

### ReasoningContext

```csharp
ReasoningContext
{
    WorkingMemoryState     WorkingMemory;
    RetrievedMemory[]      RetrievedMemories;
    WorldModelState        WorldModel;
    GoalState              Goals;
}
```

### Puede leer

- Información contextual desde Working Memory.
- Hechos recuperados desde Long Term Memory.
- Estado del modelo del mundo.
- Objetivos activos.

### No puede leer

- ECS completo.
- Sistemas vecinos (Planning, Decision, Auditor, Identity).
- Estado privado de otros subsistemas.
- AffectState directamente (solo indirectamente via lo que afectó WorkingMemory y Retrieval).

---

## 4. Outputs

### ReasoningResult

```csharp
ReasoningResult
{
    Inference[]        Inferences;
    ConfidenceScore    Confidence;
    ReasoningEvidence[] Evidence;
}
```

### Inference

Cada inferencia debe contener:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | uint | Identificador único |
| `Premises` | EvidenceRef[] | Referencias a hechos origen |
| `RuleId` | string | Identificador de la regla que produjo la inferencia |
| `Transformation` | string | Etiqueta del tipo de transformación aplicada |
| `Conclusion` | string | Descripción de la conclusión |
| `Confidence` | float | Soporte interno del algoritmo [0, 1] |

### Ejemplo

```
Premises:
  - (Río observado al norte)
  - (Memoria: aldea cercana a río)

RuleId:
  spatial-association-001

Transformation:
  SpatialAssociation

Conclusion:
  Posible ubicación de aldea al norte

Confidence:
  0.72
```

### ReasoningEvidence

Análogo a RetrievalEvidence:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `InferenceId` | uint | ID de la inferencia |
| `RuleId` | string | Regla que produjo la inferencia |
| `PremiseCount` | int | Número de premisas utilizadas |
| `Transformation` | string | Tipo de transformación |
| `Confidence` | float | Confianza asignada por el algoritmo |
| `EvidenceStrength` | float | Solidez de la transición causal [0, 1] |
| `Strategy` | string | Nombre de la estrategia |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

**Confidence vs EvidenceStrength**: `Confidence` es la valoración interna del algoritmo de razonamiento sobre la inferencia. `EvidenceStrength` es la solidez de la transición causal registrada en el trace (pueden coincidir, pero no son equivalentes — una inferencia puede tener alta confianza y baja evidencia causal si el algoritmo está sesgado, o viceversa).

---

## 5. Baseline algorithm: EvidenceBasedReasoning

No es lógica simbólica compleja. Es un proceso secuencial definido:

```
CollectFacts
    ↓
FindRelations
    ↓
GenerateCandidateInferences
    ↓
ScoreConfidence
    ↓
EmitInferences
```

### Identidad de reglas

Toda regla tiene una identidad única que permite rastrear qué transformación produjo cada inferencia:

```csharp
RuleDescriptor
{
    string RuleId;          // "spatial-association-001"
    string Label;           // "SpatialAssociation"
    int Version;            // 1
    string Description;     // "If A is at location L and A is spatially related to B, B is likely near L"
}
```

Esto permite después saber:

- qué regla produjo una inferencia;
- qué reglas son responsables de errores;
- comparar modelos por perfil de reglas activadas.

### Reglas baseline iniciales (extensibles)

| RuleId | Regla | Condición | Inferencia |
|--------|-------|-----------|------------|
| `spatial-association-001` | SpatialAssociation | Hecho A (ubicación) + Hecho B (relación espacial conocida) | B probable en ubicación de A |
| `causal-sequence-001` | CausalSequence | Evento A observado + patrón A→B conocido | B probable después de A |
| `goal-relevance-001` | GoalRelevance | Hecho A + Goal activo relacionado | A es prioritario (no más verdadero) para Goal |
| `contradiction-001` | Contradiction | Hecho A + Hecho B mutuamente excluyentes | Conflicto detectado |

No es un switch:

```csharp
if (river) { village = nearby; }   // ❌
```

Sino un conjunto de transformaciones registradas:

```csharp
ruleSet.ApplyAll(facts) → Inference[]   // ✅
```

Cada inferencia registra qué regla se aplicó y con qué premisas, incluyendo el `RuleId` completo.

### Confidence y EvidenceStrength

```
confidence = baseWeight × premiseCountRatio × recencyFactor × specificityFactor

baseWeight: peso inherente de la regla
premiseCountRatio: cuántas premisas de las necesarias están presentes
recencyFactor: qué tan recientes son las premisas
specificityFactor: qué tan específica es la coincidencia
```

`EvidenceStrength` se computa separadamente como la solidez de la cadena causal:

```
evidenceStrength = premiseConfidenceMin × premiseCount / maxPremises

premiseConfidenceMin: la confianza más baja entre las premisas
premiseCount: número de premisas presentes
maxPremises: número máximo de premisas que la regla puede aceptar
```

En el baseline, ambos valores suelen coincidir. La distinción existe para que modelos futuros puedan reportar divergencia entre la confianza del algoritmo y la solidez causal documentada.

---

## 6. Invariants

### R-001 — Determinism

Dos ejecuciones con el mismo `ReasoningContext` y misma estrategia producen exactamente el mismo `ReasoningResult`.

### R-002 — Evidence requirement

Toda inferencia debe tener al menos una premisa documentada. No se permite `Inference[]` con premisas vacías.

### R-003 — No hallucinated state

Reasoning no puede introducir entidades, eventos o hechos no derivados de sus entradas. Toda `Conclusion` debe ser transformación directa de premisas existentes.

### R-004 — No side effects

Reasoning no modifica:

- Memory (LTM, WM);
- WorldModel;
- Goals;
- Affect.

### R-005 — Confidence honesty

`Confidence` representa soporte interno basado en cantidad y calidad de premisas, no certeza ontológica. No puede exceder 1.0 ni ser negativa. `EvidenceStrength` en el trace documenta la solidez causal independientemente de la confianza del algoritmo.

### R-006 — Goal may not alter validity

La relevancia a un Goal activo puede afectar la **priorización** de inferencias, pero nunca la **confianza** de una inferencia. Una inferencia no es más verdadera porque sea útil para un objetivo.

### R-007 — Separation from Decision

Reasoning genera posibilidades. Decision selecciona acciones. Ninguna inferencia implica ejecución.

---

## 7. Strategy abstraction

Mismo patrón que `IMemoryRetrievalStrategy`:

```csharp
IReasoningStrategy
{
    ReasoningResult Reason(ReasoningContext context);
}
```

```
ReasoningSystem (ECS orchestrator)
        |
        ↓
IReasoningStrategy
        |
 ┌──────┴────────┐
 ↓               ↓
EvidenceBased   Future Models
(Baseline)      (RuleBased, Probabilistic,
                 Analogical, Neural, ...)
```

El `ReasoningSystem`:

1. Construye `ReasoningContext` desde WM, LTM, WorldModel, Goals.
2. Invoca `IReasoningStrategy`.
3. Escribe inferencias a `InferenceStore`.
4. Emite CausalTrace.

---

## 8. Validation scenarios

### S-R001 — Inferencia directa

**Entrada**:
```
Fact: Aeris vio un árbol.
Fact: Árboles aparecen cerca de bosques (memoria).
```

**Salida esperada**:
```
Inference: Probable bosque cercano.
Evidence: 2 premisas.
Confidence: > 0.
```

### S-R002 — No evidencia suficiente

**Entrada**:
```
Fact: Aeris vio una piedra.
(ninguna regla aplicable, ninguna relación conocida)
```

**Salida esperada**:
```
InferenceSet vacío.
```

### S-R003 — Contradicción

**Entrada**:
```
Memoria recuperada: Río al norte.
WorldModel: Río al sur.
```

**Salida esperada**:
```
Inference: Conflicto de ubicación (Río).
Confidence: n/a (contradicción señalada, no resuelta).
Evidence: ambas fuentes documentadas.
```

No elegir una versión automáticamente. La contradicción se reporta.

### S-R004 — Reemplazabilidad

Cambiar `IReasoningStrategy` sin modificar `ReasoningSystem`.

### S-R005 — Determinismo

Dos ejecuciones con mismo `ReasoningContext` producen mismo DAG causal (mismas inferencias, mismo orden, mismas confianzas).

### S-R006 — Sin side effects

Ejecutar Reasoning no altera LTM, WM, WorldModel, Goals ni Affect.

### S-R007 — Inferencia con Goal activo

**Entrada**:
```
Goal: Encontrar agua.
Fact: Musgo denso observado al este.
Memoria: Musgo denso asociado a fuentes de agua.
```

**Salida esperada**:
```
Inference: Posible fuente de agua al este.
Evidence: 3 premisas (Goal espacial, hecho, memoria).
Confidence: basada en soporte factual (premisas), no en el Goal.
Prioridad: mayor que inferencias sin relevancia al Goal.
```

El Goal **no altera la confianza** de la inferencia. Solo afecta su **posición en el orden de salida**. Esto mantiene la separación cognición/motivación: la verdad de una inferencia no depende de si es útil.

---

## 9. Criterio de éxito de 3B.2

No es "Aeris piensa".

Es:

> Existe un mecanismo de inferencia artificial capaz de producir conclusiones nuevas a partir de estado interno disponible, con trazabilidad causal completa y sin introducir conocimiento externo.

### Métricas mínimas

- Build: 0 errores, 0 warnings.
- Tests: 100% pass (S-R001–S-R007 + determinismo + side effects + reemplazabilidad).
- Rendimiento: inferencia < 5ms con 20 hechos en WM.
- Cobertura baseline: al menos 4 reglas de transformación (SpatialAssociation, CausalSequence, GoalRelevance, Contradiction).

---

## 10. Dependencies

- **CONTRACT-MR**: activo. Reasoning consume `RetrievedMemory[]`.
- **Sprint 3A**: completado. Reasoning consume `WorkingMemoryState`, `WorldModelState`, `GoalState`.
- **Sprint 3B.1**: completado. Memory Retrieval pipeline cerrado.

---

## Historial

| Versión | Fecha | Cambio |
|---------|-------|--------|
| 0.1 | 2026-07-30 | Draft inicial |
