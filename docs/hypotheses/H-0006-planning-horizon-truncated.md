# H-0006: La simulación de planes debe ser truncada por horizonte fijo

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: PlanningSystem (§3.9), WorldModelSystem (§3.6), DecisionSystem (§3.10)
**Estado epistemológico**: E2 (Evidencia moderada — planificación con horizonte truncado es práctica estándar en IA, pero su aplicación en un agente cognitivo determinista es diseño propio)

---

## Enunciado

La simulación de planes durante la evaluación (doc-17 §3.9) debe truncarse a un horizonte fijo de N pasos en lugar de simular hasta alcanzar el goal o agotar recursos. Esto mantiene el costo computacional predecible y evita que planes complejos consuman todos los recursos del tick.

## Motivación

Doc-17 §3.9 deja abierta la pregunta: «¿La simulación de planes debe ser completa o truncada?». Una simulación completa es costosa e impredecible: si un goal requiere 50 pasos y hay 5 planes candidatos, evaluarlos todos puede exceder el presupuesto del tick. Un horizonte fijo garantiza que PlanningSystem tenga costo O(H × P) donde H es el horizonte y P el número de planes, independientemente de la complejidad del goal.

## Evidence Sources

- Miller, Galanter & Pribram (1960): plans and TOTE
- Sutton & Barto (1998): reinforcement learning (horizonte en planificación)
- Hazy, Frank & O'Reilly (2007): prefrontal cortex and planning
- RN-0006 (aún sin crear): Planning in cognitive agents

## Experimento propuesto

1. Implementar dos versiones de PlanningSystem:
   - **Completa**: simular hasta alcanzar goal o detectar imposibilidad
   - **Truncada**: simular hasta H pasos, luego evaluar progreso parcial
2. Ejecutar 500 ticks con goals de diferente complejidad en cada versión
3. Medir costo computacional, tasa de éxito, y calidad de planes seleccionados

## Métricas

- Tiempo de planificación por tick
- Tasa de éxito de planes ejecutados (acción lleva al resultado esperado)
- Progreso promedio hacia el goal después de H pasos
- Número de planes evaluados por tick

## Criterio de validación

La versión truncada debe:
- Tener tiempo de planificación acotado y predecible
- No tener una tasa de éxito significativamente menor que la versión completa
- Perder calidad solo en goals que requieran planificación de largo plazo (> H pasos)

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Truncada comparable en calidad, mejor en costo | El horizonte fijo es la opción correcta |
| Completa produce planes notablemente mejores | La truncatura introduce miopía significativa |
| Completa es demasiado lenta para tiempo real | La truncatura es necesaria aunque degrade calidad |

## Impacto arquitectónico si se valida

- PlanningSystem acepta un parámetro H (horizonte) en su configuración
- La evaluación de planes incluye una métrica de «progreso parcial» además de «goal alcanzado»
- El costo es O(H × P) garantizado
- Se documenta el horizonte como parámetro configurable del Cognitive Model

## Impacto arquitectónico si se rechaza

- PlanningSystem permite simulación completa sin límite
- El costo computacional depende de la complejidad del goal y la profundidad del plan
- Se necesita un mecanismo de timeout para evitar ticks demasiado largos
