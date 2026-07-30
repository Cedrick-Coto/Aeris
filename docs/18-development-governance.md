# 18. Development Governance

**Versión**: 0.1  
**Estado**: Activo  
**Última actualización**: 2026-07-30  

---

## 1. Fundamental Rule

> **Ningún código introduce un concepto arquitectónico nuevo sin completar las cuatro capas previas.**

No se incorporan subsistemas, mecanismos o conceptos por plausibilidad teórica, relevancia literaria o intuición. Se incorporan cuando existe una cadena de evidencia completa desde la literatura hasta el contrato computacional.

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

### 3.5 When an Architectural Decision Fails

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

---

## 5. Exceptions

La única excepción a la Admission Gate es:

**Corrección de bugs o refactores** que no introduzcan nuevos conceptos arquitectónicos ni cambien el comportamiento observable del sistema.

Cualquier otra modificación debe seguir el pipeline completo.
