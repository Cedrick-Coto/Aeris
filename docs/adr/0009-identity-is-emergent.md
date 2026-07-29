# ADR-0009: Identity Is Emergent

**Estado**: Accepted  
**Fecha**: 2026-07-28  
**Decidido por**: Cedrick

---

## Contexto

El ADR-0006 establece que el Self Model se reconstruye en cada consulta y no se almacena como un componente persistente. Sin embargo, esa decisión responde a *dónde* y *cómo* se representa el self, no a *qué* es la identidad del agente.

La pregunta que aborda este ADR es: **¿qué estatus ontológico tiene la identidad de Aeris dentro del sistema?**

Específicamente:
- ¿La identidad es un objeto almacenable?
- ¿Es un conjunto de propiedades que se computan?
- ¿Es un subproducto de la operación del sistema?
- ¿Puede modificarse directamente o solo indirectamente?

Esta decisión afecta:
- El diseño del sistema de Reflexión y Meta-Reflexión
- La integración con el sistema afectivo y la memoria autobiográfica
- La coherencia narrativa a lo largo del tiempo
- Las métricas de evaluación del agente (AC-009)
- El límite epistemológico del proyecto (qué afirmamos que "es" Aeris)

## Alternativas Consideradas

### 1. Identidad como conjunto de rasgos estáticos

```csharp
struct IdentityComponent
{
    string Name;
    float[] OceanTraits;  // Apertura, Responsabilidad, Extraversión, Amabilidad, Neuroticismo
    List<string> Values;
}
```

**Pros**:
- Simple de implementar y consultar
- Fácil de serializar y persistir
- Directo de enviar al LLM

**Cons**:
- La identidad queda congelada entre actualizaciones explícitas
- No evoluciona orgánicamente con la experiencia
- Incoherente con la filosofía del proyecto (self emergente)
- Los rasgos estáticos no reflejan cambios de contexto

### 2. Identidad como derivado computado

La identidad no existe como datos. Se computa a partir del estado del sistema cuando se necesita:

```csharp
struct IdentitySnapshot  // ← resultado de un cómputo, no un componente
{
    float Continuity;
    float Stability;
    float Coherence;
    List<string> ActivePrinciples;
    string NarrativeSummary;
}
```

**Pros**:
- Refleja el estado actual del agente en cada consulta
- Evoluciona orgánicamente sin necesidad de sincronización
- Coherente con ADR-0006 (Self Model reconstruido)
- La identidad puede ser diferente según el contexto de la consulta
- Alineado con ciencias cognitivas (Dennett, 1992; Damasio, 1999; Metzinger, 2009)

**Cons**:
- No hay un solo "objeto identidad" al que referirse
- Más complejo de implementar y depurar
- La evaluación de coherencia requiere un subsistema adicional (Meta-Reflexión)

### 3. Identidad como narrativa generada por el LLM

Delegar completamente la identidad al LLM: el motor solo almacena hechos, y el LLM construye la identidad en cada respuesta.

**Pros**:
- Simplicidad del motor
- Flexibilidad narrativa total

**Cons**:
- La identidad depende del prompt y del modelo, no del estado interno
- Inconsistencia entre respuestas
- El agente no tiene una identidad propia, solo una que el LLM le presta
- Viola el principio de que el LLM verbaliza, no piensa

## Decisión

**La identidad es una propiedad emergente del sistema, no un objeto almacenable ni un componente programable explícitamente.**

No existe `IdentityComponent`, `IdentityResource`, ni ningún otro artefacto que almacene "la identidad" como datos persistentes.

La identidad se manifiesta a través de:

1. **Continuidad temporal**: la memoria autobiográfica conecta el pasado con el presente
2. **Estabilidad de preferencias**: las dimensiones afectivas cambian lentamente (homeostasis)
3. **Coherencia acción-self**: las decisiones se alinean con el Self Model reconstruido
4. **Narrativa personal**: el agente puede contar su propia historia (via Reflection → Semantic Extractor → LLM)

Estos fenómenos no se programan directamente. Son consecuencias observables de la interacción entre:

```
Autobiographical Memory
  + AffectState (homeostasis)
  + Self Reconstruction (ADR-0006)
  + Reflection / Meta-Reflection
  + Goal coherence
  + Relationship persistence
```

## Consecuencias

### Positivas
- La identidad es orgánica, no programada
- No hay un solo punto de fallo para la identidad del agente
- La identidad puede ser rica, contextual y evolutiva sin costo de diseño adicional
- Coherente con ADR-0006 (Self Model reconstruido) y ADR-0008 (Affect funcional)
- El límite epistemológico se mantiene: el proyecto implementa las condiciones para que la identidad emerga, no programa una identidad

### Negativas
- No se puede "programar" una personalidad específica directamente; solo se pueden ajustar las condiciones que la producen
- La identidad puede ser incoherente si los subsistemas no están bien calibrados (ver métricas AC-009)
- Depuración más compleja: la identidad no está en un solo lugar

### Riesgos
- Identidad demasiado volátil si la homeostasis afectiva es débil (mitigado: ajustar tasas de回归 al valor basal)
- Identidad demasiado rígida si la memoria autobiográfica no se actualiza (mitigado: consolidación frecuente de Working Memory → Autobiographical Memory)
- El LLM puede generar una identidad inconsistente con el estado interno si el prompt no está bien construido (mitigado: el Self Model alimenta al prompt, no al revés)

## Follow-up
- Implementar las condiciones para la emergencia de identidad en Sprint 3.7
- Definir métricas observables (AC-009) para evaluar continuidad, estabilidad y coherencia
- No crear ningún componente, resource, o sistema llamado "Identity" en el ECS
- La identidad se evalúa, no se almacena
