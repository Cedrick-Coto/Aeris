# ADR-0008: Affect Is Functional, Not Human

**Estado**: Accepted  
**Fecha**: 2026-07-28  
**Decidido por**: Cedrick

---

## Contexto

El proyecto Aeris define un agente cognitivo con un sistema afectivo que modula atención, memoria, razonamiento, planificación, decisión y aprendizaje. Al diseñar este sistema surge la pregunta: **¿cómo modelamos el afecto del agente?**

Las alternativas van desde implementar un conjunto de emociones discretas humanas (alegría, tristeza, miedo, ira, sorpresa, asco) hasta definir variables funcionales continuas que modulan el comportamiento sin etiquetas emocionales explícitas.

Esta decisión afecta:
- La arquitectura del sistema afectivo
- La interfaz entre afecto y cognición
- La flexibilidad para generar comportamientos diversos
- La coherencia con la filosofía del proyecto (modelo funcional, no reproducción humana)
- La integración futura con el Semantic Extractor y el LLM

## Alternativas Consideradas

### 1. Emociones discretas como enum

```csharp
enum Emotion { Joy, Sadness, Fear, Anger, Surprise, Disgust }
```

**Pros**:
- Familiar y fácil de entender
- Simple de implementar y serializar
- Directo de comunicar al LLM

**Cons**:
- Las emociones humanas no son discretas ni universales
- Fuerza al agente a encajar estados complejos en categorías rígidas
- No captura matices (intensidad, mezcla, ambivalencia)
- Deriva antropocéntrica: asume que el agente siente como humano
- Difícil de extender sin romper el enum

### 2. Dimensiones continuas (PAD — Pleasure, Arousal, Dominance)

```csharp
struct AffectState
{
    float Pleasure;   // -1 a +1
    float Arousal;     // 0 a 1
    float Dominance;   // 0 a 1
}
```

**Pros**:
- Más flexible que emociones discretas
- Permite estados mixtos y graduales
- Modelo validado en psicología (Mehrabian, 1974)

**Cons**:
- Diseñado para humanos, no necesariamente óptimo para un agente funcional
- Dominance no captura bien conceptos como novelty, safety o attachment
- Sigue anclado a un modelo humano

### 3. Variables funcionales (decidido)

```csharp
struct AffectState
{
    float Valence;    // -1 a +1
    float Arousal;    // 0 a 1
    float Control;    // 0 a 1
    float Novelty;    // 0 a 1
    float Safety;     // 0 a 1
    float Attachment; // 0 a 1
    float Stress;     // 0 a 1
    float Curiosity;  // 0 a 1
}
```

**Pros**:
- Cada variable tiene un efecto funcional claro sobre la cognición (ver ADR-0010 y 16-agent-architecture.md)
- No asume que el agente siente como humano
- Las "emociones" observables emergen de la configuración de estas variables, no se programan
- Extensible: añadir una variable funcional nueva no rompe el sistema
- Coherente con la filosofía del proyecto (modelo funcional, no antropocéntrico)
- El LLM puede interpretar estas variables sin necesidad de etiquetas emocionales

**Cons**:
- Menos intuitivo que un enum de emociones
- Requiere documentar qué hace cada variable y cómo modifica la cognición
- Más complejo de ajustar (requiere calibración de pesos e influencias)

## Decisión

**El sistema afectivo implementa variables funcionales continuas, no etiquetas de emociones humanas.**

Las emociones observables (miedo, alegría, tristeza, etc.) son interpretaciones que el LLM puede generar a partir del estado afectivo, pero no existen como datos internos del motor.

El estado afectivo se compone de las siguientes dimensiones funcionales:

| Dimensión | Rango | Efecto funcional |
|-----------|-------|------------------|
| Valence | -1 a +1 | Atrae/evita estímulos |
| Arousal | 0 a 1 | Energía disponible |
| Control | 0 a 1 | Sensación de agencia |
| Novelty | 0 a 1 | Atención a lo nuevo |
| Safety | 0 a 1 | Disposición al riesgo |
| Attachment | 0 a 1 | Vínculo con entidades significativas |
| Stress | 0 a 1 | Degradación cognitiva |
| Curiosity | 0 a 1 | Exploración vs. explotación |

## Consecuencias

### Positivas
- El sistema afectivo es funcional por diseño, no antropocéntrico
- Las emociones observables emergen, no se programan
- Cada dimensión tiene un propósito claro y un efecto sobre la cognición
- El LLM puede interpretar el estado afectivo en lenguaje natural sin categorías rígidas
- Extensible por adición, no por modificación

### Negativas
- Curva de aprendizaje: requiere entender qué hace cada variable
- Calibración inicial: los pesos de influencia sobre la cognición necesitan ajuste experimental

### Riesgos
- Demasiadas variables pueden hacer el sistema difícil de depurar (mitigado: empezar con Valence, Arousal, Stress; añadir el resto progresivamente en Sprint 3.2)
- El LLM podría malinterpretar las variables sin un prompt adecuado (mitigado: el Semantic Extractor traduce el estado afectivo a texto narrativo, no envía las variables crudas)

## Follow-up
- Implementar `AffectState` en Sprint 3.2 como struct de 8 floats
- Definir la función de actualización por tick (eventos → delta en dimensiones)
- Documentar los efectos de cada dimensión sobre Attention, Memory, Reasoning, Planning, Decision, Learning en `16-agent-architecture.md`
- No crear un enum `Emotion` en ninguna capa del motor
