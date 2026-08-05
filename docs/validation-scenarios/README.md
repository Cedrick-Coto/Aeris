# Validation Scenarios — Sprint 3B

Escenarios de comportamiento esperado para cada micro-sprint de 3B.

No son tests. Son la **especificación del comportamiento esperado**. Los tests se derivan de ellos.

## Cobertura

| Archivo | Micro-sprint | Estado |
|---------|--------------|--------|
| `3B-memory-retrieval.md` | 3B.1 — Memory Retrieval | Completo |
| `3B-reasoning.md` | 3B.2 — Reasoning | Draft |
| `3B-planning.md` | 3B.3 — Planning | Detallado |
| `3B-decision.md` | 3B.4 — Decision | Índice |
| `3B-auditor.md` | 3B.5 — Auditor | Índice |
| `3B-identity.md` | 3B.6 — Identity Reconstruction | Índice |
| `3B-selfsnapshot.md` | 3B.7 — SelfSnapshot | Índice |
| `3X-model-interchangeability.md` | 3X.3.2 — Reasoning Strategy Swap | Completo |

## Metodología

- 3B.1: escenarios completos
- 3B.2: draft (depende de cómo termine Retrieval)
- 3B.3: detallado (tras implementar y validar Planning; evidencia EXP-0004)
- 3B.4–3B.7: solo índice con escenarios previstos

Razón: especificar justo lo necesario para desbloquear la implementación. Los micro-sprints dependientes se detallan cuando el anterior está implementado y validado.

## Formato

Cada escenario incluye:

- **Purpose**: qué verifica
- **Initial World**: estado inicial del ECS
- **Agent State**: WM, LTM, Affect, Goals
- **Input**: qué recibe el subsistema
- **Expected Output**: qué debe producir
- **Expected State Changes**: qué cambia en el agente
- **Forbidden**: qué no debe ocurrir
- **Failure Modes**: casos que deben fallar controladamente

## Trazabilidad entre capas

Cada escenario enlaza con su contrato, especificación, hipótesis y research note:

```
RN-0005 (World Models)
      ↓
H-0004 (WorldModel prob vs simbólico)
      ↓
SPEC-3B.1 (Memory Retrieval)
      ↓
CONTRACT-MR (MemoryRetrieval.contract.md)
      ↓
SCENARIO S-004 (High curiosity broadens recall)
      ↓
TEST TR-004 (test correspondiente)
```

### IDs de contrato

| ID | Contrato |
|----|----------|
| CONTRACT-MR | Memory Retrieval |
| CONTRACT-REASONING | Reasoning |
| CONTRACT-PLANNING | Planning |
| CONTRACT-DECISION | Decision |
| CONTRACT-AUDITOR | Auditor |
| CONTRACT-IDENTITY | Identity Reconstruction |
| CONTRACT-SS | SelfSnapshot |
| CONTRACT-CT | Causal Trace (transversal, Sprint 3C tools) |

### IDs de escenario por micro-sprint

| Micro-sprint | Prefijo |
|--------------|---------|
| 3B.1 | S- |
| 3B.2 | R- |
| 3B.3 | P- |
| 3B.4 | D- |
| 3B.5 | A- |
| 3B.6 | I- |
| 3B.7 | SS- |

### Referencias inversas

Cada escenario incluye una cabecera con enlace ascendente:

```
SCENARIO S-001

Implements:
- CONTRACT-MR

Derived from:
- SPEC-3B.1 (doc-17 §MemoryRetrieval)

Supports:
- H-0004, H-0006

Background:
- RN-0005
```

### Convención para tests

Los tests de integración incluirán una cabecera similar:

```
TEST TR-004

Implements:
- SCENARIO S-004

Validates:
- CONTRACT-MR

Derived from:
- SPEC-3B.1

Supports:
- H-0004

Background:
- RN-0005
```

Esto permite navegar el grafo en ambas direcciones sin herramienta adicional.

### Lifecycle

| Status | Meaning |
|--------|---------|
| Draft | Proposed, not yet reviewed |
| Active | Approved, matches implementation |
| Deprecated | Superseded, kept for history |
| Replaced | Superseded, links to replacement |
| Archived | No longer relevant |
