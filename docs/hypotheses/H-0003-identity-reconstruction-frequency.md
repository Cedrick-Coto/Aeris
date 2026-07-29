# H-0003: La reconstrucción de identidad cada tick produce inestabilidad narrativa

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: IdentityReconstructionSystem (§3.12)
**Estado epistemológico**: E3 (Hipótesis de ingeniería — no hay respaldo empírico directo)

---

## Enunciado

Reconstruir SelfSnapshot desde cero cada tick (como especifica doc-17 §3.12) introduce inestabilidad en la identidad del agente porque pequeñas variaciones en AffectState o WorkingMemory de un tick a otro amplifican diferencias en el snapshot, produciendo una narrativa autobiográfica inconsistente. Es probable que una frecuencia menor (cada N ticks, con N ≥ 5) produzca un self más coherente.

## Motivación

La decisión de reconstruir SelfSnapshot cada tick (ADR-0006, doc-17) es una hipótesis de ingeniería sin respaldo empírico directo. En humanos, el sentido de identidad es estable a lo largo de segundos y minutos, no fluctúa tick a tick (Neisser, 1988). Si el snapshot cambia demasiado rápido, la narrativa autobiográfica será incoherente, lo que podría afectar la calidad de la verbalización del LLM.

## Evidence Sources

- ADR-0006: Self Model Is Reconstructed
- Dennett (1991): multiple drafts model
- Neisser (1988): five kinds of self-knowledge
- Conway (2005): self-memory system
- RN-0003 (aún sin crear): Identity / Self reconstruction literature

## Experimento propuesto

1. Implementar IdentityReconstruction con frecuencia variable: cada 1 tick, cada 5, cada 10, cada 20
2. Ejecutar 500 ticks con misma seed en cada configuración
3. Medir CoherenceScore del SelfSnapshot en cada tick

## Métricas

- CoherenceScore promedio por tick (auto-reportado por SelfSnapshot)
- Diferencia media entre snapshots consecutivos (medida de estabilidad)
- Número de cambios de principios activos (volatilidad del self)
- Tiempo de construcción del snapshot

## Criterio de validación

La configuración cada 1 tick produce:
- CoherenceScore significativamente menor que cada 5 o 10 ticks
- Mayor volatilidad en principios activos
- Mayor diferencia media entre snapshots consecutivos

Si no hay diferencia significativa, la reconstrucción cada tick no introduce inestabilidad.

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Cada 1 tick muestra menor coherencia y mayor volatilidad | La frecuencia debe reducirse |
| No hay diferencia significativa | El costo de reconstruir cada tick es solo computacional |
| Cada 1 tick produce mayor coherencia (paradójico) | El self se beneficia de estar «siempre actualizado» |

## Impacto arquitectónico si se valida

- IdentityReconstructionSystem se ejecuta cada N ticks (configurable)
- La cadena causal incluye una nota: «IdentityReconstruction no se ejecuta todos los ticks»
- SelfSnapshot no está disponible en todos los ticks → los sistemas que lo consultan deben manejarlo como opcional
- El Semantic Extractor debe usar el snapshot más reciente disponible, no necesariamente el del tick actual

## Impacto arquitectónico si se rechaza

- Se mantiene la reconstrucción cada tick
- Se elimina una fuente de incertidumbre en el diseño
- Mayor simplicidad en el pipeline (snapshot siempre disponible)
