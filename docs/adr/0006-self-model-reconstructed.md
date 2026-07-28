# ADR-0006: Self Model Is Reconstructed, Not Stored

**Estado**: Accepted  
**Fecha**: 2026-07-28  
**Decidido por**: Cedrick

---

## Contexto

El proyecto Aeris define un agente cognitivo con memoria autobiográfica, estado afectivo, objetivos, relaciones y capacidad de reflexión. Al llegar al diseño del "self" del agente, surge la pregunta: **¿cómo representamos quién es el agente?**

Las alternativas van desde almacenar un componente explícito `Self` en el ECS (con campos como `Name`, `PersonalityTraits`, `Values`, etc.) hasta no almacenar nada y reconstruir una representación del self a partir del resto del estado interno.

Esta decisión afecta:
- El diseño de datos del ECS
- La arquitectura de los sistemas cognitivos
- La integración con el Semantic Extractor y el LLM
- La flexibilidad para que el personaje evolucione
- La coherencia con la filosofía del proyecto

## Alternativas Consideradas

### 1. Self como componente ECS explícito

Un componente `SelfComponent` almacenado como cualquier otro componente de datos:

```csharp
struct SelfComponent
{
    string Name;
    float[] PersonalityTraits;  // OCEAN fijo
    List<Principle> Principles;
    List<Goal> LifeGoals;
}
```

**Pros**:
- Simple de implementar y consultar
- Fácil de serializar y persistir
- Acceso O(1) desde cualquier sistema

**Cons**:
- El self queda congelado en el momento de la serialización
- Dificulta que el self evolucione orgánicamente
- El LLM recibe siempre la misma foto del self, no una reconstrucción contextual
- Incoherente con la filosofía del proyecto (self como fenómeno emergente)

### 2. Self como recurso global

Similar a la alternativa 1, pero como `Resource` del ECS en lugar de `Component`.

**Pros**: Mismos que 1, más accesible globalmente.
**Cons**: Mismos que 1, más el riesgo de que cualquier sistema lo modifique.

### 3. Self como sistema de consulta (reconstrucción)

No existe `SelfComponent`. Existe un `SelfModelSystem` que, cuando se le consulta, construye una representación del self a partir de:

- Historia autobiográfica
- Estado afectivo actual
- Objetivos activos
- Relaciones vigentes
- Conocimiento y creencias
- Reflexiones recientes

```csharp
struct SelfModel  // ← NO es un componente, es el resultado de un cómputo
{
    string Summary;           // "Soy un entrenador que..."
    float[] TraitScores;      // Derivados de la historia, no almacenados
    List<string> Principles;  // Inferidos de decisiones pasadas
    float Coherence;          // Qué tan consistente es el self actual
}
```

**Pros**:
- El self siempre refleja el estado actual del agente
- Evoluciona naturalmente sin necesidad de sincronización
- Coherente con la filosofía del proyecto (self emergente)
- El LLM recibe un self contextual, no estático
- Alineado con literatura en ciencias cognitivas (self como proceso dinámico)
- El self puede ser diferente según el contexto de la consulta

**Cons**:
- Más costoso computacionalmente (se reconstruye cada vez)
- Más complejo de implementar
- La coherencia del self depende de la calidad de los subsistemas que lo alimentan

## Decisión

**El Self Model se reconstruye en cada consulta y nunca se almacena como un objeto persistente.**

No existe `SelfComponent` ni `SelfResource` en el ECS. El self es el resultado de un cómputo que integra:

1. **Autobiographical Memory**: episodios significativos
2. **Long-Term Memory**: creencias, conocimiento, principios aprendidos
3. **Active Goals**: lo que el agente quiere ahora
4. **AffectState**: cómo se siente ahora
5. **Relationships**: vínculos activos
6. **Recent Reflections**: conclusiones de ciclos de reflexión recientes

## Consecuencias

### Positivas
- El self es siempre contextual y actualizado
- La evolución del personaje es orgánica, no programada
- El LLM recibe un self rico y situado
- El diseño es coherente con la filosofía del proyecto
- Facilita la extensión: añadir una nueva fuente de información al self no requiere cambiar un componente, solo añadir una entrada al sistema de reconstrucción

### Negativas
- Mayor costo computacional: la reconstrucción debe ser eficiente (< 1ms)
- Mayor complejidad de implementación: el sistema de reconstrucción debe integrar múltiples fuentes
- La coherencia del self depende de la correcta integración de todos los subsistemas

### Riesgos
- Self inconsistente si las fuentes están desincronizadas (mitigado con consultas atómicas al estado)
- Self demasiado volátil si no se pondera correctamente la historia vs. el estado momentáneo (mitigado con pesos temporales: la historia reciente pesa más que el momento actual)
- Rendimiento: si se reconstruye en cada tick, puede ser costoso (mitigado: solo se reconstruye cuando se necesita, no en cada tick)

## Follow-up
- Implementar `SelfModelSystem` en **Sprint 3.4**
- Definir el algoritmo de ponderación temporal para la reconstrucción
- Crear tests de coherencia: el self debe ser estable en ausencia de cambios significativos
- Integrar con el Semantic Extractor en **Sprint 4** para que el LLM reciba el SelfModel reconstruido
