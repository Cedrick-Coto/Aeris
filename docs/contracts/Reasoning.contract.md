# CONTRACT-REASONING

**Estado**: Draft  
**Última actualización**: 2026-07-31  
**Versión**: 0.3  

---

# 1. Purpose

Definir el contrato para un subsistema capaz de transformar información disponible en el estado cognitivo en inferencias estructuradas, reproducibles y trazables.

Este contrato define una interfaz experimental para evaluar mecanismos de inferencia artificial dentro de la arquitectura ACMA.

Cualquier modelo de razonamiento futuro debe satisfacer este contrato para ser intercambiable.

Ejemplos de modelos compatibles:

- simbólico;
- probabilístico;
- analógico;
- conexionista.

---

## No define

Este contrato no define:

- inteligencia general;
- conciencia;
- comprensión humana;
- verdad ontológica de las inferencias;
- procesos internos privados del modelo.

Una inferencia representa una conclusión derivada bajo un modelo determinado, no un hecho verdadero del mundo.

---

# 2. Position in causal chain

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

````

---

# 3. Responsibility boundaries

## Reasoning puede:

- leer información contextual permitida;
- generar inferencias;
- generar trazabilidad del proceso;
- asignar métricas internas de soporte.

---

## Reasoning no puede:

- ejecutar acciones;
- modificar memoria permanente;
- modificar WorkingMemory;
- modificar WorldModel;
- modificar Goals;
- modificar Affect;
- introducir hechos externos no presentes en sus entradas.

---

# 4. Inputs

## ReasoningContext

```csharp
ReasoningContext
{
    WorkingMemoryState     WorkingMemory;
    RetrievedMemory[]      RetrievedMemories;
    WorldModelState        WorldModel;
    GoalState              Goals;
}
````

---

## Lectura permitida

Reasoning puede consumir:

* información contextual de WorkingMemory;
* memorias recuperadas;
* estado actual del WorldModel;
* objetivos activos.

---

## Lectura prohibida

Reasoning no puede acceder directamente a:

* ECS completo;
* Planning;
* Decision;
* Identity;
* Auditor;
* estados privados de otros subsistemas.

Affect no es una entrada directa.

Si información afectiva existe, debe estar representada mediante estados disponibles en las entradas permitidas.

---

# 5. Outputs

## ReasoningResult

```csharp
ReasoningResult
{
    Inference[]          Inferences;
    ReasoningEvidence[]  Evidence;
}
````

---

## Nota contractual

La confianza pertenece a cada inferencia individual.

No existe `ReasoningResult.Confidence`.

Motivo:

La confianza evalúa soporte de una conclusión específica, no la calidad global del proceso completo.

---

# 6. Inference

Una `Inference` representa una conclusión derivada mediante una transformación registrada.

```csharp
Inference
{
    uint          Id;
    EvidenceRef[] Premises;
    string        RuleId;
    string        Transformation;
    string        Conclusion;
    float         Confidence;
}
````

---

## Propiedades

### Id

Identificador único dentro del resultado generado.

---

### Premises

Referencias a los elementos utilizados como evidencia de origen.

Toda inferencia debe contener al menos una premisa.

---

### RuleId

Identificador de la regla responsable de generar la inferencia.

Debe permitir rastrear:

* qué transformación fue aplicada;
* qué versión de regla produjo el resultado.

---

### Transformation

Etiqueta que identifica el tipo de transformación aplicada.

No representa una explicación narrativa.

---

### Conclusion

Resultado derivado.

Debe representar una transformación basada únicamente en las premisas disponibles.

No puede introducir entidades, eventos o hechos externos.

---

### Confidence

Valor entre:

```
0 <= Confidence <= 1
```

Representa soporte interno asignado por el algoritmo de razonamiento.

No representa:

* certeza;
* verdad;
* probabilidad real del mundo.

---

# 7. Rule

Una `Rule` representa una transformación reutilizable del conocimiento.

Una regla:

* no posee Confidence propia;
* no representa una conclusión concreta;
* no depende de un caso individual.

---

## RuleDescriptor

```csharp
RuleDescriptor
{
    string RuleId;
    string Label;
    int    Version;
    string Description;
}
````

---

## Responsabilidad

Una Rule permite identificar:

* qué transformación fue aplicada;
* qué versión produjo una inferencia;
* comparar comportamiento entre modelos.

---

# 8. ReasoningRule

`ReasoningRule` representa la adaptación operativa de una regla dentro del motor de razonamiento.

Diferencia conceptual:

```
Rule
 |
 | conocimiento reutilizable
 ↓

ReasoningRule
 |
 | aplicación dentro del proceso Reasoning
 ↓

Inference
```

Una ReasoningRule no modifica el significado epistemológico de una Rule.

---

# 9. ReasoningEvidence

`ReasoningEvidence` representa trazabilidad del proceso de generación.

No representa:

* introspección completa;
* explicación cognitiva humana;
* justificación narrativa.

---

## Structure

```csharp
ReasoningEvidence
{
    uint   InferenceId;

    string RuleId;

    EvidenceRef[] Premises;

    string Transformation;

    float  Confidence;

    float  EvidenceStrength;

    string Strategy;
}
````

---

## Confidence vs EvidenceStrength

### Confidence

Soporte interno asignado por el algoritmo a una inferencia.

---

### EvidenceStrength

Solidez de la transición causal registrada en el trace.

Ambos valores pueden diferir.

Ejemplo:

```
Confidence alta
EvidenceStrength baja
```

Puede indicar un modelo con exceso de confianza.

---

# 10. Determinism

## R-001 — Determinism

Dado:

* mismo ReasoningContext;
* misma estrategia;
* mismo conjunto de reglas;
* misma versión de reglas;

el resultado debe ser equivalente.

La equivalencia incluye:

* mismas inferencias;
* mismas premisas;
* mismas reglas;
* mismos valores de Confidence;
* mismo orden de salida.

---

Los valores derivados exclusivamente de ejecución física u observabilidad no forman parte de la equivalencia determinista.

---

# 11. Runtime metadata

Los datos de rendimiento no pertenecen al resultado epistemológico.

Ejemplos:

* tiempo de ejecución;
* consumo de recursos;
* métricas del sistema.

Pueden registrarse externamente mediante mecanismos de observabilidad.

---

# 12. Baseline algorithm

Proceso mínimo:

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

---

# 13. Invariants

## R-001 — Determinism

Misma entrada produce mismo resultado.

---

## R-002 — Evidence requirement

Toda inferencia debe tener premisas documentadas.

No existen inferencias sin evidencia.

---

## R-003 — No hallucinated state

Reasoning no puede introducir conocimiento no derivado de sus entradas.

---

## R-004 — No side effects

Reasoning no modifica:

* Memory;
* WorldModel;
* Goals;
* Affect.

---

## R-005 — Confidence honesty

Confidence:

* está en rango [0,1];
* representa soporte interno;
* no representa verdad.

---

## R-006 — Goal separation

Goals pueden modificar:

* prioridad;
* orden de salida.

Goals no pueden modificar:

* Confidence;
* validez epistemológica;
* contenido de una inferencia.

---

## R-007 — Separation from Decision

Reasoning genera posibilidades.

Decision selecciona acciones.

Una inferencia nunca implica ejecución.

---

# 14. Strategy abstraction

```csharp
IReasoningStrategy
{
    ReasoningResult Reason(ReasoningContext context);
}
````

---

Una estrategia puede cambiar siempre que mantenga:

* entradas;
* salidas;
* invariantes;
* trazabilidad requerida.

---

# 15. Reemplazabilidad

Una implementación alternativa debe producir:

* Inference válido;
* RuleId rastreable;
* Premises verificables;
* Confidence válida;
* ReasoningEvidence asociado.

---

# 16. Validation scenarios

Se mantienen:

* S-R001 Inferencia directa.
* S-R002 Sin evidencia suficiente.
* S-R003 Contradicción.
* S-R004 Reemplazabilidad.
* S-R005 Determinismo.
* S-R006 Sin side effects.
* S-R007 Inferencia con Goal activo.

---

# 17. Criterion of success

El sistema cumple cuando:

* produce inferencias nuevas desde información disponible;
* mantiene trazabilidad causal;
* mantiene determinismo;
* no introduce conocimiento externo;
* mantiene separación respecto a Decision.

---

# 18. Dependencies

* CONTRACT-MR
* WorkingMemoryState
* WorldModelState
* GoalState

---

# History

| Version | Date       | Change                                                               |
| ------- | ---------- | -------------------------------------------------------------------- |
| 0.1     | 2026-07-30 | Draft inicial                                                        |
| 0.2     | 2026-07-30 | Definición ampliada                                                  |
| 0.3     | 2026-07-31 | Corrección contractual: determinismo, evidencia, confianza y límites |
