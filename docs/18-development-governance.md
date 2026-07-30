# 18. Development Governance

**Versión**: 0.2  
**Estado**: Activo  
**Última actualización**: 2026-07-30  

---

## 1. Fundamental Rules

### 1.1 Admission Gate

> **Ningún código introduce un concepto arquitectónico nuevo sin completar las cuatro capas previas.**

No se incorporan subsistemas, mecanismos o conceptos por plausibilidad teórica, relevancia literaria o intuición. Se incorporan cuando existe una cadena de evidencia completa desde la literatura hasta el contrato computacional.

### 1.2 Coherence Rule

Antes de aceptar cualquier cambio —código, contrato, modelo, hipótesis, escenario— responder obligatoriamente estas cinco preguntas:

| # | Pregunta | Si la respuesta es "no"... |
|---|----------|----------------------------|
| 1 | ¿Existe un contrato que defina este subsistema? | No se implementa. |
| 2 | ¿Existe al menos un escenario de validación? | No se implementa. |
| 3 | ¿La hipótesis está identificada o explícitamente descartada? | No se implementa. |
| 4 | ¿El cambio preserva todos los invariantes del motor? | No se implementa. |
| 5 | ¿Existe una estrategia para clasificar un posible fallo? | No se implementa. |

No es una regla técnica; es una regla de gobierno del proyecto. Su función es evitar que la presión por avanzar erosione la arquitectura por conveniencia.

### 1.3 Historical Compatibility

> **Un experimento nunca cambia; se vuelve a ejecutar sobre nuevas versiones.**

Los informes `docs/experiments/EXP-NNNN.md` son registros históricos inmutables. Si es necesario repetir un experimento, se genera una nueva ejecución asociada al mismo diseño experimental, no se reescribe el experimento original. Esto permite comparar resultados entre versiones del motor y del modelo con precisión sobre qué cambió y por qué.

---

## 2. Knowledge Pipeline

Todo concepto nuevo debe atravesar esta cadena antes de llegar a la implementación:

```
Research Note (fundamento)
    │
    ▼
Hypothesis (incertidumbre explícita)
    │
    ▼
Specification (contrato computacional, doc-17)
    │
    ▼
Formal Contract (interfaces, invariantes, side effects)
    │
    ▼
ADR (si introduce una decisión arquitectónica)
    │
    ▼
Implementation
    │
    ▼
Validation (tests, determinismo, benchmarks)
```

### 2.1 Research Note

- Resume la literatura relevante para el concepto
- Identifica qué partes tienen respaldo empírico y cuáles son adaptaciones de ingeniería
- Enlaza a fuentes primarias
- Clasifica el nivel epistemológico (E1-E4)

### 2.2 Hypothesis

- Formula una pregunta concreta: qué se espera que ocurra si se implementa este mecanismo
- Define experimento, métricas y criterios de validación
- Especifica consecuencias de validar o rechazar la hipótesis
- Se registra en `docs/hypotheses/`

### 2.3 Specification

- Define el contrato computacional en doc-17
- Entradas, salidas, invariantes, proceso
- Independiente de teoría cognitiva específica

### 2.4 Formal Contract

- Define exclusivamente: inputs, outputs, invariantes, complejidad, determinismo, dependencias, side effects prohibidos
- Sin teoría. Sin implementación. Sin justificación literaria.
- Se registra en `docs/contracts/`

### 2.5 ADR

- Se crea si la decisión implica un cambio arquitectónico
- No se editan ADRs existentes; se crea una nueva
- Se registra en `docs/adr/`

### 2.6 Implementation

- El código implementa el contrato, no la teoría
- No hay «personalidad» ni «emociones» en el código — solo variables, pesos y umbrales

### 2.7 Validation

- Tests unitarios
- Tests de integración (cadena causal)
- Tests de determinismo
- Property-based tests (invariantes)
- Benchmarks (performance)

---

## 3. Governance Rules

### 3.1 Admission Gate

Un subsistema nuevo solo entra al motor si completa:

1. Research Note (fundamento) — ✅
2. Hypothesis (incertidumbre explícita) — ✅
3. Specification (contrato computacional en doc-17) — ✅
4. Formal Contract en `docs/contracts/` — ✅
5. ADR (si introduce decisión arquitectónica) — ✅

Sin estas cinco capas completas, el subsistema no se implementa.

### 3.2 Model/Infrastructure Separation

- La **Cognitive Infrastructure** (Sprint 3A) proporciona mecanismos generales
- El **Cognitive Model** (ACMA v1, v2, etc.) decide cómo se configuran
- Ningún modelo cognitivo puede modificar la infraestructura
- La infraestructura no puede depender de ningún modelo concreto

### 3.3 Versioning

- Los Cognitive Models se versionan independientemente
- ACMA v1, v2, etc. son experimentos sobre la misma infraestructura
- Cada versión se documenta en `models/`

### 3.4 Traceability

Toda transición de estado en cualquier subsistema debe poder explicarse:

```
System
├── Inputs: qué leyó
├── Computation: qué procesó
├── Outputs: qué produjo
├── SideEffects: qué más modificó
└── Why: qué regla o criterio motivó la transición
```

### 3.5 Coste de cambio por capa

Las modificaciones tienen un coste creciente según la capa que afectan:

| Artefacto | Coste | Justificación |
|-----------|-------|---------------|
| Tests | Muy bajo | Sólo verificación, no cambian el sistema |
| Implementación | Bajo | Refleja el contrato, cambia con frecuencia |
| Escenarios | Bajo–medio | Especificación de comportamiento observable |
| Contratos | Medio | Afectan a todos los modelos que los implementan |
| Modelo (ACMA) | Alto | Cambia supuestos del modelo cognitivo |
| Hipótesis | Alto | Cuestiona el fundamento teórico |
| ADR | Muy alto | Invierte una decisión arquitectónica previa |

Esto no impide cambios, pero fuerza que cada modificación declare explícitamente qué capa toca.

### 3.6 Versionado de experimentos

Todo experimento (`docs/experiments/EXP-NNNN.md`) debe referenciar:

```yaml
Engine version:     git commit o tag
Contracts version:  CONTRACT-XX (ID + Status)
Model version:      ACMA-v1.2 (o la instancia usada)
```

Esto permite reproducir cualquier resultado en el futuro.

### 3.7 Estabilidad de contratos

Los contratos evolucionan más lentamente que los modelos:

```
CONTRACT 1.0
    ↓
ACMA-v1, ACMA-v2, MinimalAgent, ReactiveAgent
    ↓
Todos compatibles con CONTRACT 1.0
```

Solo cuando un contrato limite la investigación se crea `CONTRACT 2.0`. Los contratos son el punto de estabilidad del ecosistema; cambiarlos rompe todos los modelos que los implementan.

### 3.8 Clasificación de fallos

Cuando un experimento no produce el resultado esperado, el informe debe clasificar la causa:

```yaml
Failure Source (seleccionar una):
  □ Bug de implementación
  □ Contrato incompleto
  □ Modelo incorrecto
  □ Hipótesis no apoyada
  □ Escenario insuficiente
  □ Evidencia insuficiente
```

Esto evita atribuir automáticamente un mal resultado al modelo cognitivo cuando el problema puede ser un error de código o un contrato mal definido.

### 3.9 When an Architectural Decision Fails

1. Documentar la evidencia del fallo
2. Crear ADR nueva (no editar la existente)
3. Actualizar la especificación afectada (doc-17)
4. Actualizar el contrato afectado
5. Ajustar el plan de sprints si es necesario

---

## 4. Relationship to Existing Documents

| Document | Role in governance |
|----------|-------------------|
| `docs/14-development-roadmap.md` | Sprint plan, dependencies, milestones |
| `docs/16-agent-architecture.md` | Agent pipeline description |
| `docs/17-computational-agent-model.md` | Formal interfaces and contracts |
| `docs/18-development-governance.md` | **This document** — admission rules |
| `docs/adr/` | Architectural decisions |
| `docs/research-notes/` | Literature foundation |
| `docs/hypotheses/` | Active research hypotheses |
| `docs/contracts/` | Formal subsystem contracts |
| `models/` | Cognitive model registry |
| `docs/experiments/` | Experimental evidence |
| `docs/validation-scenarios/` | Behavior specifications |

---

## 5. Fases del proyecto

El proyecto ha evolucionado a través de cuatro fases:

| Fase | Nombre | Activo principal |
|------|--------|------------------|
| 1 | Motor ECS | Engine determinista, ECS, persistencia |
| 2 | Motor cognitivo | Infraestructura cognitiva (Perception, Attention, Memory, Affect, Goals) |
| 3 | Plataforma de modelos cognitivos | Contratos + `models/` + múltiples instancias intercambiables |
| 4 (potencial) | Plataforma experimental | Experimentos reproducibles, comparación entre modelos, evidencia acumulada |

El activo principal del proyecto ya no es solo el código del agente. Es el **ecosistema completo de contratos, modelos, experimentos y evidencia reproducible**.

---

## 6. Exceptions

La única excepción a la Admission Gate es:

**Corrección de bugs o refactores** que no introduzcan nuevos conceptos arquitectónicos ni cambien el comportamiento observable del sistema.

Cualquier otra modificación debe seguir el pipeline completo.
