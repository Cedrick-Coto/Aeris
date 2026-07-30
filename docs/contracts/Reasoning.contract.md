# CONTRACT-REASONING

**Estado**: Draft  
**Última actualización**: 2026-07-30  
**Versión**: 0.1  

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
| `Transformation` | string | Etiqueta del tipo de transformación aplicada |
| `Conclusion` | string | Descripción de la conclusión |
| `Confidence` | float | Soporte interno [0, 1] |

### Ejemplo

```
Premises:
  - (Río observado al norte)
  - (Memoria: aldea cercana a río)

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
| `PremiseCount` | int | Número de premisas utilizadas |
| `Transformation` | string | Tipo de transformación |
| `Confidence` | float | Confianza asignada |
| `Strategy` | string | Nombre de la estrategia |
| `ElapsedMicroseconds` | long | Tiempo de cómputo |

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

### Reglas baseline iniciales (extensibles)

| Regla | Condición | Inferencia |
|-------|-----------|------------|
| SpatialAssociation | Hecho A (ubicación) + Hecho B (relación espacial conocida) | B probable en ubicación de A |
| CausalSequence | Evento A observado + patrón A→B conocido | B probable después de A |
| GoalRelevance | Hecho A + Goal activo relacionado | A es relevante para Goal |
| Contradiction | Hecho A + Hecho B mutuamente excluyentes | Conflicto detectado |

No es un switch:

```csharp
if (river) { village = nearby; }   // ❌
```

Sino un conjunto de transformaciones registradas:

```csharp
ruleSet.ApplyAll(facts) → Inference[]   // ✅
```

Cada inferencia registra qué regla se aplicó y con qué premisas.

### Confidence scoring

```
confidence = baseWeight × premiseCountRatio × recencyFactor × specificityFactor

baseWeight: peso inherente de la regla
premiseCountRatio: cuántas premisas de las necesarias están presentes
recencyFactor: qué tan recientes son las premisas
specificityFactor: qué tan específica es la coincidencia
```

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

`Confidence` representa soporte interno basado en cantidad y calidad de premisas, no certeza ontológica. No puede exceder 1.0 ni ser negativa.

### R-006 — Separation from Decision

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
Confidence: > 0.5 (goal relevance bonus).
```

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
