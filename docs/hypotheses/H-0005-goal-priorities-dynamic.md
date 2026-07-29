# H-0005: Las prioridades de goals deben ser dinámicas, no jerárquicas

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: GoalSystem (§3.8), AffectSystem (§3.3), DecisionSystem (§3.10)
**Estado epistemológico**: E2 (Evidencia moderada — hay respaldo para prioridades dinámicas, pero la implementación como pesos continuos es diseño propio)

---

## Enunciado

Las prioridades de los goals no deben estar organizadas en una jerarquía fija (como Maslow), sino que deben ser pesos continuos modulados por AffectState y contexto. Esto permite que un goal de «supervivencia» pueda ser menos prioritario que un goal de «exploración» si el agente está seguro y curioso, sin violar ninguna jerarquía predefinida.

## Motivación

Doc-17 §3.8 especifica que GoalSystem tiene prioridades moduladas por AffectState, pero deja abierta la pregunta: «¿Los goals deben tener jerarquía fija o prioridades dinámicas?» (doc-17 §3.8). Una jerarquía fija (ej. Maslow) simplifica la implementación pero fuerza comportamientos predecibles y poco flexibles. Prioridades dinámicas permiten que el agente priorice exploración sobre seguridad cuando está en un entorno seguro, o que priorice vínculos sociales sobre necesidades básicas en ciertos contextos — un comportamiento más realista.

## Evidence Sources

- Maslow (1943): hierarchy of needs
- Simon (1967): motivational and emotional controls of cognition
- Austin & Vancouver (1996): goal constructs in psychology
- RN-0002: Affect models from literature

## Experimento propuesto

1. Implementar dos versiones de GoalSystem:
   - **Jerárquica**: orden fijo (Supervivencia > Seguridad > Social > Conocimiento > Exploración)
   - **Dinámica**: pesos continuos modulados por AffectState (Curiosity → Exploration, Threat → Survival, Attachment → Social)
2. Ejecutar 500 ticks con mismo escenario en ambas versiones
3. Medir qué tipo de goals se activan en cada contexto

## Métricas

- Distribución de tipos de goal activos por contexto (seguro vs amenazante vs social)
- Tiempo de cambio entre goals (latencia de re-priorización)
- Número de cambios de goal activo por tick
- Coherencia: relación entre estado afectivo y goal activo

## Criterio de validación

La versión dinámica debe:
- Activar goals de exploración cuando Curiosity > 0.7 y Threat < 0.3
- Activar goals de supervivencia cuando Threat > 0.7
- Tener transiciones suaves (no saltos bruscos de prioridad)
- La versión jerárquica debe mostrar comportamientos más rígidos (exploración solo cuando necesidades «inferiores» están satisfechas)

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Dinámica produce comportamientos más contextualmente apropiados | Las prioridades deben ser dinámicas |
| Jerárquica es indistinguible en la práctica | La modulación afectiva no añade valor significativo |
| Dinámica produce cambios de goal demasiado frecuentes | Se necesita histéresis o umbrales mínimos |

## Impacto arquitectónico si se valida

- GoalSystem mantiene prioridades como pesos continuos (como ya especifica doc-17)
- Se añade una matriz de modulación explícita: GoalType × AffectVariable → weightDelta
- Se necesita un mecanismo de histéresis para evitar cambios demasiado frecuentes
- La jerarquía fija se elimina como opción predeterminada

## Impacto arquitectónico si se rechaza

- GoalSystem usa jerarquía fija con modulación limitada
- Más simple, más predecible
- Menos realismo en escenarios donde el agente debe priorizar contradictoriamente (ej. seguridad vs curiosidad)
