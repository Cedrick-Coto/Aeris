# H-0002: El presupuesto atencional debe ser dinámico según CognitiveLoad

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: AttentionSystem (§3.2), AffectSystem (§3.3)
**Estado epistemológico**: E1–E2 (la modulación afectiva de atención está bien documentada; el mecanismo de presupuesto dinámico es adaptación de ingeniería)

---

## Enunciado

El número de perceptos atendidos por tick (N) no debe ser fijo, sino que debe expandirse o contraerse dinámicamente según CognitiveLoad y Stress del AffectState. A mayor carga cognitiva, menor presupuesto atencional.

## Motivación

Doc-17 §3.2 especifica N fijo por configuración, pero la literatura (Corbetta & Shulman, 2002) muestra que la atención se contrae bajo estrés y se expande en entornos seguros. Un N fijo ignora la retroalimentación del estado interno del agente, lo que puede producir comportamientos no realistas: atender a demasiadas cosas cuando ya hay sobrecarga, o ignorar información relevante cuando el agente está tranquilo.

## Evidence Sources

- Corbetta & Shulman (2002): dorsal/ventral attention networks
- Posner (1980): orienting of attention
- RN-0002 (aún sin crear): Affect models from literature
- Eysenck et al. (2007): attentional control theory (anxiety impairs attentional control)

## Experimento propuesto

1. Implementar dos versiones:
   - **Fijo**: N constante (ej. 7 ± 2) independientemente de AffectState
   - **Dinámico**: N = baseN × (1 − CognitiveLoad × stressFactor)
2. Someter el agente a escenarios de alta carga (múltiples eventos simultáneos, amenazas) y baja carga
3. Medir tasa de información crítica perdida (perceptos relevantes descartados)

## Métricas

- Tasa de perdida de información crítica (perceptos relevantes no atendidos)
- Tiempo de reacción ante eventos importantes
- Varianza del número efectivo de chunks atendidos por tick
- Correlación entre CognitiveLoad y N en la versión dinámica

## Criterio de validación

La versión dinámica debe:
- Perder menos información crítica en escenarios de baja carga (cuando hay capacidad disponible)
- Perder más información no crítica en escenarios de alta carga
- La versión fija debe tener una tasa de pérdida uniforme (independiente del contexto)

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Dinámico pierde menos información crítica | Affect debe modular el presupuesto atencional |
| Ambos pierden la misma cantidad | La modulación no aporta beneficio |
| Dinámico es impredecible | El mecanismo de contracción/expansión introduce inestabilidad |

## Impacto arquitectónico si se valida

- AttentionSystem debe leer CognitiveLoad y Stress del AffectState actual (no el del tick anterior) → requiere reordenar cadena causal o pasar AffectState previsto
- El parámetro N fijo se reemplaza por N base + fórmula de modulación
- Las invariantes de doc-17 §3.2 se actualizan: «el presupuesto atencional es dinámico y acotado superiormente»

## Impacto arquitectónico si se rechaza

- Se mantiene N fijo como especifica doc-17
- La modulación afectiva de atención se limita a sesgo (hacia amenaza o novedad) sin cambiar el tamaño del filtro
- Menos complejidad, pero menos realismo conductual
