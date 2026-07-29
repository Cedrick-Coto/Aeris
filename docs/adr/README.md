# Architecture Decision Records

Este directorio contiene las decisiones arquitectónicas del proyecto Aeris, clasificadas por nivel de estabilidad.

---

## Jerarquía de Estabilidad

| Nivel | Alcance | Cambios esperados | ADRs |
|-------|---------|-------------------|------|
| **A0** | Principios epistemológicos | Muy excepcionales | — |
| **A1** | Axiomas cognitivos | Muy raros | 0006, 0008, 0009, 0010 |
| **A2** | Arquitectura del motor | Poco frecuentes | 0001, 0004, 0005 |
| **A3** | Plataforma e implementación | Frecuentes | 0002, 0003, 0007 |

---

## Clasificación

### A0 — Principios Epistemológicos

No son ADR tradicionales. Son el fundamento del proyecto y rara vez cambian:

- Aeris implementa un **modelo funcional de agencia**, no una reproducción de la mente humana.
- La evidencia empírica limita las afirmaciones sobre cognición.
- La experiencia subjetiva (qualia) no se presupone; solo se modelan procesos funcionales.
- El proyecto prioriza lo construible sobre lo metafísico.

### A1 — Axiomas Cognitivos (muy raros)

Describen cómo funciona el agente independientemente de la implementación:

| ADR | Decisión |
|-----|----------|
| 0006 | Self Model Is Reconstructed, Not Stored |
| 0008 | Affect Is Functional, Not Human |
| 0009 | Identity Is Emergent |
| 0010 | Perception Precedes Cognition |

### A2 — Arquitectura del Motor (poco frecuentes)

Definen la estructura técnica del sistema:

| ADR | Decisión |
|-----|----------|
| 0001 | Use Arch ECS |
| 0004 | LLM as Function |
| 0005 | Semantic State as Transversal |

### A3 — Plataforma e Implementación (frecuentes)

Decisiones tecnológicas reemplazables según el contexto:

| ADR | Decisión |
|-----|----------|
| 0002 | Use SQLite + JSON |
| 0003 | Use C# |
| 0007 | Target .NET 10 |

---

## Convenciones

- Los ADR no se editan después de aceptados. Si una decisión cambia, se crea un ADR nuevo.
- Las hipótesis de investigación se registran en `docs/hypotheses/`, no como ADR.
- Los ADR de nivel A1 requieren consenso explícito antes de modificarse.
