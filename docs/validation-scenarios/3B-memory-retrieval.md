# Validation Scenarios — 3B.1 Memory Retrieval

**Status**: Active
**ID prefix**: S-

```
Implements:
- CONTRACT-MR

Derived from:
- SPEC-3B.1 (doc-17 §Memory Retrieval)

Supports:
- H-0004 (WorldModel prob vs simbólico)
- H-0002 (Presupuesto atencional dinámico)

Background:
- RN-0005 (Modelos Internos del Mundo)
- RN-0004 (Modelos de Atención)
```

---

## S-001: Retrieve familiar location

Purpose
: Verificar que un recuerdo relevante es recuperado correctamente desde LTM.

Initial World
- Río visible
- Árbol visible

Agent State

```
Working Memory:
  - Chunk: "Río" (percept, salience=0.9)
  - Chunk: "Árbol" (percept, salience=0.9)

Long-Term Memory:
  - Memory#1: "El río conduce a la aldea" (Importance=0.8, Timestamp=100, Category=Environmental)

Affect:
  - Curiosity = 0.6
  - Stress   = 0.2
  - Defaults for others
```

Input
- LTM query: candidates matching `Category=Environmental OR relevance to WM > 0.5`

Expected Output

```
RetrievedMemory[]:
  - Memory#1 with relevance score > 0.7

ActivationBoost[]:
  - WM chunk "Río" receives +0.2 salience boost
```

Expected Working Memory (after retrieval)
- Contiene "Río" (boosted)
- Contiene "Árbol" (unchanged)
- Contiene chunk recuperado: "El río conduce a la aldea"

Forbidden
- Modificar Long-Term Memory (no escritura, no cambios de importancia)
- Crear objetivos
- Modificar AffectState
- Eliminar chunks existentes de WM

Failure Modes

```
❌ Recupera un recuerdo no relacionado.
❌ No recupera nada cuando hay candidatos relevantes.
❌ Modifica Long-Term Memory.
❌ Elimina chunks de WM.
❌ Produce resultados distintos con la misma seed.
```

---

## S-002: No relevant memory

Purpose
: Verificar que el sistema retorna vacío cuando no hay recuerdos relevantes.

Initial World
- Entidad desconocida visible

Agent State

```
Working Memory:
  - Chunk: "Entidad_Desconocida" (percept, salience=0.7)

Long-Term Memory:
  - Vacía (ningún recuerdo almacenado)

Affect:
  - Curiosity = 0.5
  - Stress   = 0.2
```

Input
- LTM query sobre la entidad

Expected Output

```
RetrievedMemory[]: vacío
ActivationBoost[]: vacío
```

Expected Working Memory
- Sin cambios aparte de decay normal

Forbidden
- Crear recuerdos falsos en LTM
- Eliminar chunks existentes de WM

Failure Modes

```
❌ Retorna un recuerdo cuando no existe ninguno.
❌ Crea un recuerdo nuevo en LTM como "compensación".
❌ Bloquea la ejecución del sistema.
```

---

## S-003: High stress narrows recall

Purpose
: Verificar que Stress alto restringe la recuperación a los recuerdos de mayor relevancia.

Initial World
- Dos entidades visibles

Agent State

```
Working Memory:
  - Chunk: "Entidad_A" (salience=0.8)
  - Chunk: "Entidad_B" (salience=0.6)

Long-Term Memory:
  - Memory#1: "Entidad_A es hostil" (Importance=0.9, Category=Combat)
  - Memory#2: "Entidad_A tiene un mineral" (Importance=0.4, Category=Discovery)
  - Memory#3: "Entidad_B es amigable" (Importance=0.7, Category=Social)

Affect:
  - Stress   = 0.9   ← alto
  - Threat   = 0.7
  - Curiosity = 0.3
```

Input
- Retrieval threshold elevado por Stress (solo recuerdos con relevance > 0.8 pasan)

Expected Output

```
RetrievedMemory[]:
  - Memory#1 (Importance=0.9, combat-related, pasa el umbral)

ActivationBoost[]:
  - WM chunk "Entidad_A" recibe boost

```

Expected Working Memory
- "Entidad_A" boosteado
- "Entidad_B" sin cambio (no hay recuerdo recuperado para ella)
- Memory#1 cargada como chunk

Forbidden
- Recuperar Memory#2 (baja importancia) o Memory#3 (social, no amenazante)

Failure Modes

```
❌ Stress alto no tiene efecto en el umbral de recuperación.
❌ Recupera recuerdos de baja relevancia bajo stress alto.
❌ Recupera recuerdos sociales cuando Threat es alto.
```

---

## S-004: High curiosity broadens recall

Purpose
: Verificar que Curiosity alta expande el conjunto de candidatos.

Initial World
- Misma configuración que S-003

Agent State

```
Working Memory:
  - Chunk: "Entidad_A" (salience=0.8)

Long-Term Memory:
  - Memory#1: "Entidad_A es hostil" (Importance=0.9, Category=Combat)
  - Memory#2: "Entidad_A tiene un mineral" (Importance=0.4, Category=Discovery)

Affect:
  - Curiosity = 0.9   ← alto
  - Stress   = 0.1
  - Threat   = 0.1
```

Input
- Threshold reducido por Curiosity (recuerdos con relevance > 0.3 pasan)

Expected Output

```
RetrievedMemory[]:
  - Memory#1 (Importance=0.9)
  - Memory#2 (Importance=0.4, pasa el umbral reducido)
```

Expected Working Memory
- Ambos recuerdos cargados como chunks

Forbidden
- No recuperar Memory#2 cuando Curiosity es alta.

Failure Modes

```
❌ Curiosity alta no reduce el umbral de recuperación.
❌ Recupera recuerdos irrelevantes incluso con umbral bajo.
```

---

## S-005: Recency tiebreaker

Purpose
: Verificar que dos recuerdos con igual relevancia se ordenan por recencia.

Initial World
- Una entidad visible

Agent State

```
Working Memory:
  - Chunk: "Entidad_C" (salience=0.7)

Long-Term Memory:
  - Memory#1: "Entidad_C visitó la colina" (Importance=0.6, Timestamp=50)
  - Memory#2: "Entidad_C cruzó el río"   (Importance=0.6, Timestamp=200)

Affect:
  - Curiosity = 0.5
  - Stress   = 0.2
```

Retrieval Budget: 1 (solo un recuerdo puede cargarse en WM)

Expected Output

```
RetrievedMemory[]:
  - Memory#2 (misma importancia, más reciente)
```

Expected Working Memory
- Solo Memory#2 cargado como chunk

Failure Modes

```
❌ Recupera el más antiguo cuando el presupuesto es 1.
❌ Recupera ambos cuando el presupuesto es 1.
```

---

## S-006: Cued retrieval by entity ID

Purpose
: Verificar que un recuerdo se recupera por ID de entidad explícita.

Initial World
- Entidad con EntityId=42 visible

Agent State

```
Working Memory:
  - Chunk: "Entidad_42" (percept, source=EntityId=42)

Long-Term Memory:
  - Memory#1: "EntityId=42 dejó un rastro al norte" (InvolvedEntityId=42, Importance=0.8)

Affect:
  - Curiosity = 0.5
  - Stress   = 0.2
```

Input
- Cue explícita: EntityId=42

Expected Output

```
RetrievedMemory[]:
  - Memory#1 (matched por InvolvedEntityId)
```

Forbidden
- Recuperar recuerdos de otras entidades
- No recuperar el recuerdo cuando existe el cue exacto

Failure Modes

```
❌ Ignora el cue de entity ID.
❌ Recupera recuerdos de entidades diferentes.
```

---

## S-007: Budget cap on retrieved memories

Purpose
: Verificar que no se excede el límite de recuerdos recuperados por tick.

Initial World
- Múltiples entidades visibles

Agent State

```
Working Memory:
  - Varios chunks

Long-Term Memory:
  - 10 recuerdos relevantes para el contexto actual

Retrieval Budget: 3
```

Expected Output

```
RetrievedMemory[]: exactamente 3 recuerdos (los de mayor relevance)
```

Forbidden
- Recuperar más de 3 recuerdos
- Devolver menos de 3 si hay suficientes candidatos (a menos que umbral los filtre)

Failure Modes

```
❌ Recupera más del presupuesto permitido.
❌ No respeta el orden de relevance dentro del presupuesto.
```

---

## S-008: Trace logging

Purpose
: Verificar que toda recuperación queda registrada en CognitiveTraceLog.

Initial World
- Cualquier escenario con recuperación exitosa (ej: S-001)

Expected Output

```
CognitiveTraceLog.Entries contiene:
  System = "MemoryRetrievalSystem"
  InputSummary  → query, affect state
  OutputSummary → memoria recuperada, scores
  Why           → regla de selección aplicada
```

Forbidden
- No registrar la recuperación
- Registrar información incompleta (sin query o sin scores)

Failure Modes

```
❌ No hay entrada en el log tras una recuperación exitosa.
❌ La entrada en el log no contiene los scores de relevancia.
```

---

## S-009: Retrieval does not create side effects

Purpose
: Verificar que MemoryRetrieval no modifica nada fuera de su declaración de salida (invariante #10).

Initial World
- Agente con recuerdos en LTM

Agent State
- WM con chunks
- LTM con recuerdos
- AffectState con valores conocidos

Expected State Changes

```
WorkingMemory:     nuevos chunks agregados
CognitiveTraceLog: nuevo entry
```

Forbidden (sin cambios)

```
LongTermMemory:    sin cambios (no se modifica importancia, timestamp, ni se olvida nada)
AffectState:       sin cambios
WorldModel:        sin cambios
Goals:             sin cambios
EventBus:          sin eventos emitidos
```

Failure Modes

```
❌ MemoryRetrieval modifica AffectState.
❌ MemoryRetrieval escribe en LongTermMemory.
❌ MemoryRetrieval crea o modifica objetivos.
❌ MemoryRetrieval emite eventos.
```

---

## S-010: Determinism

Purpose
: Verificar que el mismo estado inicial produce exactamente los mismos recuerdos recuperados.

Initial World
- Idéntico en dos ejecuciones

Agent State
- Idéntico en dos ejecuciones

Seed: misma

Expected Output

```
Ejecución 1: RetrievedMemory[] = [M1, M2]
Ejecución 2: RetrievedMemory[] = [M1, M2] (mismos IDs, mismos scores, mismo orden)
```

Failure Modes

```
❌ Las recuperaciones difieren entre ejecuciones con la misma seed.
❌ El orden de los recuerdos no es determinista.
```

---

## Criterios de éxito para 3B.1

| Propiedad | Evidencia |
|-----------|-----------|
| Correctitud | S-001–S-010 pasan |
| Determinismo | Misma seed + estado inicial → mismos recuerdos recuperados |
| Reemplazabilidad | Segundo algoritmo (IMemoryRetrievalStrategy) implementable sin cambios fuera de la estrategia |
| Observabilidad | CausalTrace explica cada recuperación sin alterar la simulación |

## Resumen de cobertura

| Escenario | Aspecto validado |
|-----------|-----------------|
| S-001 | Recuperación por relevancia contextual |
| S-002 | Ausencia de recuerdos → resultado vacío |
| S-003 | Modulación por Stress (estrechar) |
| S-004 | Modulación por Curiosity (ampliar) |
| S-005 | Recency como tiebreaker |
| S-006 | Cued retrieval por EntityId |
| S-007 | Budget cap |
| S-008 | Trace logging |
| S-009 | Sin side effects (invariante #10) |
| S-010 | Determinismo |
