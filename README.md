# Aeris

**Motor de simulación cognitiva con capa narrativa para un mundo Pokémon.**

[![CI](https://github.com/Cedrick-Coto/Aeris/actions/workflows/ci.yml/badge.svg)](https://github.com/Cedrick-Coto/Aeris/actions/workflows/ci.yml)
[![Determinism](https://github.com/Cedrick-Coto/Aeris/actions/workflows/determinism.yml/badge.svg)](https://github.com/Cedrick-Coto/Aeris/actions/workflows/determinism.yml)
[![License: GPL-3.0](https://img.shields.io/badge/License-GPL--3.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)

Aeris no es un chatbot ni un juego con guion. Es un **simulador de mundo** donde la historia emerge como consecuencia de la simulación, no como producto de un prompt. El LLM no piensa: verbaliza el estado interno del agente.

---

## Principios Arquitectónicos

- **ECS + Data-Oriented Design** como base del motor.
- **Determinismo** como requisito del núcleo de simulación.
- **Separación estricta** entre simulación, cognición, afecto y narrativa.
- El **LLM nunca modifica el estado del mundo**; únicamente interpreta y expresa el estado interno.
- El **Self** no es un componente: se reconstruye cada tick como `SelfSnapshot`.
- El **afecto** es un vector continuo (curiosidad, estrés, confianza, etc.), no emociones discretas.
- ACMA es un **módulo cognitivo experimental e intercambiable** (v1, v2, ...).
- El proyecto implementa un **modelo funcional de agencia**, no afirma reproducir la conciencia humana.

### Estabilidad de decisiones

| Nivel | Ámbito | Cambios esperados |
|-------|--------|-------------------|
| A0 | Principios epistemológicos | Muy excepcionales |
| A1 | Axiomas cognitivos | Muy raros |
| A2 | Arquitectura del motor | Poco frecuentes |
| A3 | Plataforma e implementación | Frecuentes |

Ver [ADR hierarchy](docs/adr/README.md) para la clasificación completa.

---

## Arquitectura

```
                    Mundo ECS
                       │
                 Simulation Tick
                       │
                Semantic Extractor
                       │
                ┌──────┴──────┐
                │   ACMA vN   │
                │  (módulo    │
                │  cognitivo  │
                │  intercam-  │
                │  biable)    │
                └──────┬──────┘
                       │
               SelfSnapshot
            (existe solo este tick)
                       │
                ┌──────┴──────┐
                │  Narrative  │
                │  Pipeline   │
                └──────┬──────┘
                       │
                     LLM
                       │
             Narrativa / Diálogo
```

El LLM opera sobre la **frontera determinismo/probabilismo**: recibe `SelfSnapshot` + `SemanticState` deterministas y produce narrativa probabilística. Nunca modifica el estado interno del agente.

Para una descripción detallada de cada subsistema: [`docs/16-agent-architecture.md`](docs/16-agent-architecture.md).

---

## Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Lenguaje | C# (.NET 10.0) |
| Paradigma | ECS (Entity Component System) + Data-Oriented Design |
| ECS Library | [Arch](https://github.com/genaray/Arch) |
| Tests | xUnit + FsCheck (property-based) + FluentAssertions |
| Persistencia | SQLite + JSON |
| LLM | Provider-agnostic (OpenAI, Claude, Ollama, etc.) |

---

## Roadmap de Desarrollo

```
Sprint 0 ──► Sprint 1 ──► Sprint 2 ──► Sprint 3A ──► Sprint 3B ──► Sprint 4 ──► Sprint 5 ──► Sprint 6 ──► Sprint 7
Arquitec.    Motor ECS    Sem. Extr.   Infra.       ACMA v1      LLM          Narrativa    Mundo Pok.   Aeris
(FROZEN)     (COMPL.)     (Pend.)      Cognitiva    (Modelo      (Verbaliz.)  (Pipeline)   (Modelado)   (Personaje)
                                        (Sistemas    Experimental)
                                        Generales)
```

| Sprint   | Estado     | Objetivo                                                                                       |
| -------- | ---------- | ---------------------------------------------------------------------------------------------- |
| Sprint 0 | ✅ Frozen   | Especificación arquitectónica y ADRs                                                           |
| Sprint 1 | ✅ Complete | Motor ECS determinista (World, Systems, EventBus, Scheduler, Persistence)                      |
| Sprint 2 | ⏳ Planned  | Semantic Extractor (extraer estado del mundo → SemanticState para el LLM)                      |
| Sprint 3A| ⏳ Planned  | Infraestructura cognitiva (11 sistemas ECS deterministas)                                      |
| Sprint 3B| ⏳ Planned  | ACMA v1 — primera hipótesis experimental del agente                                            |
| Sprint 4 | ⏳ Planned  | Integración LLM (verbalizador, no pensador)                                                    |
| Sprint 5 | ⏳ Planned  | Narrativa (Diálogo, Monólogo interno, Narración contextual)                                    |
| Sprint 6 | ⏳ Planned  | Mundo Pokémon (Biología, Aura, Ecosistemas, Cultura, Lenguaje, Facciones)                      |
| Sprint 7 | ⏳ Planned  | Aeris (Personaje completo e integración final)                                                 |

---

## Cómo contribuir

1. Lee [`CONTRIBUTING.md`](CONTRIBUTING.md)
2. Revisa los [`docs/adr/`](docs/adr/) para entender las decisiones arquitectónicas
3. Explora [`docs/hypotheses/`](docs/hypotheses/) para ver hipótesis de investigación activas
4. Abre un issue o envía un PR

Toda contribución debe respetar los 6 principios arquitectónicos (determinismo, presión de causalidad, trazabilidad, contrato computacional, localidad causal, modulación afectiva).

---

## Construcción

```bash
# Build
dotnet build

# Tests
dotnet test

# Benchmarks
dotnet run --project benchmarks/Aeris.Benchmarks -c Release
```

**Requiere**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## Licencia

[GPL-3.0](LICENSE). Ver [`SECURITY.md`](SECURITY.md) para reportar vulnerabilidades.
