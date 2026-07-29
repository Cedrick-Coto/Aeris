# H-0004: El WorldModel debe ser probabilístico, no simbólico

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: WorldModelSystem (§3.6), ReasoningSystem (§3.7), PlanningSystem (§3.9)
**Estado epistemológico**: E2 (Evidencia moderada — los modelos mentales probabilísticos tienen respaldo, pero la implementación concreta es diseño propio)

---

## Enunciado

El WorldModel (doc-17 §3.6) debe representar el conocimiento del agente como distribuciones de probabilidad sobre estados del mundo, no como proposiciones simbólicas. Esto permite manejar incertidumbre, actualizar creencias por evidencia (Bayes), y modelar teoría de la mente como distribuciones sobre creencias ajenas.

## Motivación

La especificación actual deja la pregunta abierta: «¿El WorldModel debe ser probabilístico o simbólico?» (doc-17 §3.6). Un modelo simbólico requiere un sistema de lógica formal para manejar incertidumbre (costoso y frágil). Un modelo probabilístico permite:
- Incertidumbre como primer ciudadano (no como añadido)
- Actualización por percepción (Bayes)
- Theory of mind como distribuciones sobre creencias de otros agentes
- Integración natural con el vector afectivo (modulación de distribuciones)

## Evidence Sources

- Craik (1943): internal models as cognitive basis
- Johnson-Laird (1983): mental models
- Grush (2004): emulation theory
- Tenenbaum et al. (2011): probabilistic models of cognition
- RN-0005 (aún sin crear): World model in cognitive architectures

## Experimento propuesto

1. Implementar dos versiones del WorldModel:
   - **Simbólico**: creencias como proposiciones lógicas con factores de certeza (CF)
   - **Probabilístico**: creencias como distribuciones Bayesianas (Beta o Dirichlet)
2. Ejecutar ambos en escenarios con:
   - Percepción completa (información perfecta)
   - Percepción parcial (información incompleta)
   - Percepción contradictoria (sensores en conflicto)
3. Medir calidad de inferencias, velocidad de convergencia, y costo computacional

## Métricas

- Tiempo de actualización del WorldModel por tick
- Tasa de inferencias correctas sobre estados no observados
- Velocidad de convergencia ante nueva evidencia (tick hasta estabilizar)
- Precisión en teoría de mente (predecir acciones de otros agentes)

## Criterio de validación

El modelo probabilístico debe:
- Ser superior en escenarios de percepción parcial
- Converger más rápido ante evidencia contradictoria
- Tener costo computacional aceptable (< 2x el simbólico)

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Probabilístico superior en todos los escenarios | El WorldModel debe ser probabilístico |
| Simbólico comparable en percepción completa | El costo de lo probabilístico solo se justifica con incertidumbre alta |
| Probabilístico demasiado lento | El modelo probabilístico es inviable para tiempo real |

## Impacto arquitectónico si se valida

- WorldModelState cambia de estructura (distribuciones en lugar de proposiciones)
- ReasoningSystem debe operar sobre distribuciones (inferencia Bayesiana aproximada)
- PlanningSystem evalúa planes como valor esperado sobre distribuciones
- Mayor complejidad computacional, pero mayor robustez
- Se añade una RN sobre modelos probabilísticos de cognición

## Impacto arquitectónico si se rechaza

- WorldModel se mantiene simbólico con factores de certeza
- Incertidumbre se maneja como metadato, no como parte de la representación
- Theory of mind requiere un subsistema aparte (más complejidad)
- Menor costo computacional
