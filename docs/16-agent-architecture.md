# 16. Agent Architecture

**Versión**: 0.1  
**Estado**: Borrador  
**Última actualización**: 2026-07-28

---

## Propósito

Este documento describe la arquitectura interna del agente: el pipeline que va desde la percepción del mundo hasta la acción, pasando por atención, memoria, cognición, afecto, planificación, reflexión y reconstrucción del self.

No habla del LLM. No habla de la narrativa. Este documento es el puente entre el **Semantic Extractor** (que produce el estado semántico) y la **Narrative Pipeline** (que genera la verbalización). Describe lo que el agente *es* y *hace* antes de que el LLM lo exprese en lenguaje natural.

---

## Pipeline del Agente

```
                  Mundo (Simulación ECS)
                        │
                        ▼
                  Perception
                        │
                        ▼
                  Attention
                        │
                        ▼
               Working Memory
                        │
               ┌────────┴────────┐
               │                 │
          Reasoning          AffectState
               │                 │
          Planning               │
               │                 │
          Decision               │
               │                 │
               └────────┬────────┘
                        │
                        ▼
                   Action
                        │
                        ▼
                Reflection
                        │
                        ▼
             Long-Term Memory
                        │
                        ▼
           Autobiographical Memory
                        │
                        ▼
              Self Reconstruction
                        │
                        ▼
               Identity (emergente)
```

---

## 1. Perception

### Responsabilidad
Traducir eventos del mundo simulado a **perceptos** que el agente puede procesar.

### Entrada
- Estado del mundo (World ECS)
- Eventos del EventBus

### Salida
- `Percept[]` — Representaciones estructuradas de lo que el agente detecta

### Modalidades
| Modalidad | Fuente | Percepto resultante |
|-----------|--------|---------------------|
| Visual | Entities visibles en rango | Forma, color, movimiento, distancia |
| Auditiva | Sonidos emitidos por entities o eventos | Tipo, dirección, intensidad |
| Aura | Campo de aura de entities cercanas | Firma de aura, intensidad, resonancia |

### Incertidumbre
Cada percepto incluye un nivel de confianza:
- Rango de detección → degradación con distancia
- Oclusión → perceptos parciales
- Ruido → falsos positivos de baja intensidad

---

## 2. Attention

### Responsabilidad
Seleccionar qué perceptos ingresan a Working Memory y cuáles se ignoran.

### Mecanismo
Presupuesto computacional fijo por tick:
- Perceptos con mayor **saliencia** (novedad, relevancia para objetivos activos, intensidad afectiva) entran primero
- El resto se descarta o se degrada

### Influencias externas
- **AffectState**: alta arousal → atención dispersa; baja arousal → atención lenta
- **Goals**: perceptos relevantes a objetivos activos tienen prioridad
- **Stress**: reduce el ancho atencional

---

## 3. Working Memory

### Responsabilidad
Ventana temporal de la experiencia inmediata.

### Propiedades
- Capacidad limitada (7 ± 2 chunks)
- Olvido automático (tiempo de vida por chunk)
- Refrescamiento por re-atención

### Contenido
| Tipo | Ejemplo |
|------|---------|
| Perceptos activos | "un Pokémon salvaje se acerca" |
| Inferencias recientes | "probablemente es de tipo eléctrico" |
| Estado afectivo actual | valencia baja, arousal alto |
| Objetivo activo | "explorar el bosque" |

---

## 4. Reasoning

### Responsabilidad
Generar inferencias, predicciones e hipótesis a partir del contenido de Working Memory.

### Procesos
- **Inferencia causal**: "X sucedió, luego Y podría suceder"
- **Inferencia deductiva**: "si todos los Pokémon de esta zona son de tipo agua, este también lo será"
- **Inferencia abductiva**: "hay huellas grandes → podría ser un Pokémon grande"
- **Simulación mental**: ejecutar escenarios hipotéticos en un modelo interno del mundo

### Dependencia afectiva
- Alta **valencia** → inferencias optimistas
- Alta **stress** → inferencias orientadas a amenaza
- Baja **control** → subestimación de la propia capacidad

---

## 5. AffectState

### Responsabilidad
Sistema de regulación afectiva funcional. No almacena "emociones" como etiquetas discretas, sino variables continuas que modulan todos los demás subsistemas.

### Dimensiones

| Dimensión | Rango | Efecto |
|-----------|-------|--------|
| Valence | -1 a +1 | Atrae/evita estímulos |
| Arousal | 0 a 1 | Energía disponible |
| Control | 0 a 1 | Sensación de agencia |
| Novelty | 0 a 1 | Atención a lo nuevo |
| Safety | 0 a 1 | Disposición al riesgo |
| Attachment | 0 a 1 | Vínculo con entities significativas |
| Stress | 0 a 1 | Degradación cognitiva |
| Curiosity | 0 a 1 | Exploración vs. explotación |

### Regulación
- Actualización por tick basada en perceptos, eventos y estado interno
- Homeostasis: las dimensiones tienden a valores basales
- Modulación externa: objetivos, necesidades, relaciones

### Sistemas que modifica
- **Attention**: arousal y novelty afectan el filtro atencional
- **Working Memory**: stress reduce capacidad
- **Reasoning**: valencia sesga inferencias
- **Planning**: control y safety afectan la audacia de los planes
- **Decision**: valence y arousal inclinan elecciones
- **Learning**: novelty determina codificación en memoria

---

## 6. Goals

### Responsabilidad
Mantener y priorizar objetivos activos.

### Estructura
```
Goal
├── Type: Exploration | Social | Survival | Knowledge | ...
├── Priority: float (0–1)
├── State: Active | Suspended | Completed | Failed
├── Subgoals: Goal[]
└── Progress: float (0–1)
```

### Dinámica
- Goals se activan por necesidades, eventos externos o inferencias
- Prioridades cambian según contexto y estado afectivo
- Goals completados o fallidos se archivan en Autobiographical Memory

---

## 7. Planning

### Responsabilidad
Generar y evaluar secuencias de acciones para alcanzar objetivos.

### Procesos
- **Generación**: construir planes a partir del espacio de acciones posibles
- **Evaluación**: simular cada plan en el World Model interno
- **Selección**: escoger el plan con mejor relación costo/beneficio

### Influencia afectiva
- **Control** bajo → planes cortos y conservadores
- **Safety** bajo → planes que evitan riesgo
- **Curiosity** alto → planes que incluyen exploración

---

## 8. Decision

### Responsabilidad
Seleccionar la próxima acción a ejecutar.

### Algoritmo
```
1. Evaluar plan seleccionado vs. estado actual
2. Si el plan sigue siendo válido → ejecutar siguiente paso
3. Si no → re-planificar o seleccionar acción reactiva
4. Emitir Action como evento en el ECS
```

### Moduladores
- **Stress**: decisiones más rápidas y menos óptimas bajo estrés alto
- **Uncertainty**: duda entre opciones → delay en la decisión

---

## 9. Action

### Responsabilidad
Ejecutar la acción seleccionada sobre el mundo.

### Formato
```
Action
├── Type: Move | Interact | Communicate | Observe | Wait
├── Target: Entity | Position | void
├── Parameters: dict
└── Confidence: float (0–1)
```

Las acciones se traducen a eventos del EventBus que los Systems del ECS procesan.

---

## 10. Reflection

### Responsabilidad
Revisar acciones pasadas, sus resultados, y actualizar modelos internos.

### Ciclo
```
acción
  ↓
resultado
  ↓
¿fue exitoso?
  ├── sí → reforzar estrategia
  └── no → ¿por qué?
           ↓
         actualizar creencias
           ↓
         cambiar estrategia
```

### Producto
- Actualización de creencias (BeliefRevision)
- Reforzamiento o debilitamiento de conexiones en memoria
- Ajuste de prioridades de objetivos

---

## 11. Long-Term Memory

### Responsabilidad
Almacenamiento persistente con degradación, reinterpretación y olvido.

### Tipos

| Tipo | Contenido | Retención |
|------|-----------|-----------|
| Episódica | Eventos vividos | Días/semanas (degradación progresiva) |
| Semántica | Hechos, conocimiento | Permanente (refuerzo por uso) |
| Procedimental | Secuencias de acción | Permanente (hábitos) |

### Procesos
- **Consolidación**: Working Memory → Long-Term Memory al dormir o en periodos de baja carga
- **Reconsolidación**: al recuperar un recuerdo, este se modifica con el contexto actual
- **Olvido**: recuerdos no usados pierden fuerza hasta desaparecer
- **Reinterpretación**: recuerdos se actualizan con nueva información

---

## 12. Autobiographical Memory

### Responsabilidad
Registro estructurado de la historia del agente.

### Estructura
```
Episode
├── Timestamp
├── Location
├── Participants: Entity[]
├── Percepts: Percept[]
├── AffectAtTime: AffectSnapshot
├── GoalActive: Goal
├── Outcome: Success | Failure | Mixed
└── Significance: float (0–1)
```

### Uso
- Fuente primaria para la reconstrucción del Self
- Base para Reflection y Meta-Reflection
- Material para la Narrative Pipeline

---

## 13. Self Reconstruction

### Responsabilidad
Producir una representación integrada del self a partir del estado interno actual.

### Entradas
```
Autobiographical Memory
  +
Long-Term Memory (conocimiento, creencias)
  +
Active Goals
  +
Current AffectState
  +
Relationships (active)
  +
Recent Reflections
  ↓
SelfModel (reconstruido)
```

### Consultas que responde
- ¿Qué sé de mí?
- ¿Qué puedo hacer?
- ¿Qué quiero?
- ¿Cómo he cambiado?
- ¿Quiénes son importantes?
- ¿Qué temo?
- ¿Qué espero?

### Principio fundamental
El SelfModel **nunca se almacena** como un componente persistente. Se reconstruye en el momento de la consulta. No existe una variable `Self` en el ECS. El self es el resultado de un cómputo, no una entidad.

---

## 14. Identity (emergente)

### Responsabilidad
La identidad no es un módulo. Es una **propiedad observada** del sistema que emerge de la interacción de todos los subsistemas anteriores.

### Indicadores
- **Continuidad**: el agente se percibe como el mismo a lo largo del tiempo
- **Estabilidad**: las preferencias y principios cambian lentamente
- **Coherencia**: las acciones son consistentes con el SelfModel
- **Narrativa personal**: el agente puede contar su propia historia

### Medición
La identidad se evalúa mediante métricas observables (ver AC-009), no mediante componentes internos.

---

## 15. Metauditor

### Responsabilidad
Observar el proceso de razonamiento y detectar conflictos.

### Ubicación en el pipeline
```
Reasoning
  ↓
Audit ──► Conflicts detectados?
  │         ├── sí → emitir corrección → re-razonar
  │         └── no → continuar
  ▼
Planning
```

### Qué audita
- Consistencia lógica de inferencias
- Coherencia entre creencias nuevas y existentes
- Alineación entre acciones y principios
- Sesgos introducidos por el estado afectivo

### Producto
- Correcciones que retroalimentan a Reasoning
- Ajustes en el peso de ciertas creencias
- Señales para Reflection

---

## 16. Learning

### Responsabilidad
Actualizar el contenido del agente (no el código) basado en la experiencia.

### Qué aprende
| Tipo | Fuente | Destino |
|------|--------|---------|
| Creencias | Inferencias, eventos | BeliefSystem |
| Relaciones | Interacciones repetidas | RelationshipSystem |
| Preferencias | Evaluaciones afectivas recurrentes | AffectState tendencias |
| Principios | Generalización de reflexiones | SelfModel (principios derivados) |

### Mecanismo
- Reforzamiento: patrones que producen resultados positivos se fortalecen
- Extinción: patrones que no producen resultados se debilitan
- Generalización: patrones similares se agrupan
- Discriminación: patrones distintos se separan

---

## 17. Principios de Arquitectura del Agente

Estos principios aplican a todos los subsistemas cognitivos definidos en este documento y son verificables en revisiones de código y pruebas.

### 17.1 Determinismo

El mismo estado inicial, mismos eventos y misma semilla del RNG producen exactamente el mismo estado final. No hay aleatoriedad no controlada en ningún subsistema cognitivo.

### 17.2 Presión de Causalidad (bidireccional)

- **Hacia atrás**: toda conducta observable debe poder trazarse hasta un estado interno simulado.
- **Hacia adelante**: todo estado interno relevante debe poder influir potencialmente en alguna conducta.

No existen respuestas "programadas" directamente. Ningún subsistema contiene lógica del tipo `if (emotion == X) { Say("...") }`. El estado afectivo modula pesos, umbrales y ruido en el procesamiento; no selecciona respuestas directamente.

### 17.3 Trazabilidad

Toda transición de estado en cualquier subsistema puede explicarse. Cada subsistema expone una **cadena de explicación causal** con estructura uniforme:

```
System
├── Inputs: qué leyó
├── Computation: qué procesó
├── Outputs: qué produjo
├── SideEffects: qué más modificó
└── Why: qué regla o criterio motivó la transición
```

La explicación distingue entre **evidencia** (hechos mensurables del estado) e **inferencia** (resultados del proceso de decisión del subsistema).

Ejemplo para una decisión de Planning:

```
Action: Retreat
Evidence
├── Threat = 0.82
├── SafetyGoal = Active
└── Distance = 1.2 m

Inference
├── Planner selected: RetreatPlan
└── Utility = 0.74
```

### 17.4 Contrato Computacional

Un concepto solo entra al motor si puede expresarse como un contrato computacional con:

- **Entradas** formalizadas (lee estos datos)
- **Procesamiento** definido (transforma estos datos así)
- **Salidas** formalizadas (produce estos datos)
- **Invariantes** verificables (garantiza estas propiedades)

No se incorporan conceptos por plausibilidad teórica o relevancia literaria. Se incorporan cuando existe una especificación computacional verificable.

### 17.5 Localidad Causal

Cada subsistema modifica únicamente el estado que declara explícitamente como salida.

```
PerceptionSystem
├── Lee:    World, Sensors
├── Escribe: Percepts
└── No escribe: Goals, Memory, Identity, Relationships
```

Si un subsistema necesita afectar un estado fuera de su declaración, debe hacerlo mediante un evento o estado intermedio explícito, no como efecto secundario.

### 17.6 Modulación Afectiva

El estado afectivo modifica el procesamiento cognitivo (pesos, umbrales, ruido), pero nunca selecciona respuestas directamente. No existe código del tipo `if (affect == X) → branch Y`. Las emociones observables son interpretaciones que el LLM puede generar, no datos internos del motor.

---
