# H-0001: El chunking en Working Memory debe ser guiado por atención

**Estado**: Proposed
**Fecha**: 2026-07-29
**Subsistemas afectados**: WorkingMemorySystem (§3.4), AttentionSystem (§3.2)
**Estado epistemológico**: E1 (mecanismo bien documentado, pero la integración es diseño propio)

---

## Enunciado

El chunking en WorkingMemorySystem debe ser guiado por el AttentionSystem en lugar de ser automático. Los chunks se forman a partir de perceptos que Attention seleccionó como unidad coherente, no por heurísticas internas de WM.

## Motivación

Baddeley y Cowan proponen que la capacidad de WM está limitada a 4±1 chunks, pero no especifican un mecanismo de chunking independiente de la atención. Este diseño asigna a Attention la responsabilidad de agrupar perceptos relacionados (misma fuente, mismo tipo, proximidad temporal) antes de que entren a WM, simplificando el sistema de WM y manteniendo la separación de responsabilidades.

## Evidence Sources

- RN-0001 (Working Memory / Baddeley)
- Broadbent (1958): filter model of attention
- Cowan (2001): attention is what binds features into chunks

## Experimento propuesto

1. Implementar dos versiones de chunking:
   - **Automático**: WM agrupa perceptos por similitud interna (co-ocurrencia, tipo, fuente)
   - **Guiado por atención**: AttentionSystem marca perceptos como «misma unidad atencional» y WM los trata como un solo chunk
2. Ejecutar 1000 ticks con misma seed en ambas versiones
3. Comparar número de chunks en WM por tick, coherencia semántica de inferencias, y tiempo de razonamiento

## Métricas

- Número promedio de chunks en WM (valor esperado: 4±1 en versión guiada, más variable en automática)
- Tasa de inferencias exitosas por tick
- Tiempo de razonamiento (más chunks → más inferencias)
- Coherencia: relación entre perceptos agrupados y eventos del mundo real

## Criterio de validación

La versión guiada por atención produce:
- Menor varianza en número de chunks (más estable)
- Mayor tasa de inferencias correctas
- Menor carga cognitiva en WM (CognitiveLoad menor o igual)

## Posibles resultados

| Resultado | Interpretación |
|-----------|----------------|
| Guiado produce inferencias más coherentes | La atención debe definir los chunks |
| Automático produce resultados equivalentes | El chunking puede ser interno de WM |
| Guiado es más lento | El overhead del marcado atencional no compensa |

## Impacto arquitectónico si se valida

- AttentionSystem debe incluir un paso de agrupación (binding) entre la evaluación de saliencia y la salida
- WorkingMemorySystem se simplifica: recibe chunks preformados
- No requiere cambios en interfaces actuales de doc-17 (solo ampliar la especificación de AttentionSystem)

## Impacto arquitectónico si se rechaza

- WorkingMemorySystem necesita un subsistema interno de chunking
- Mayor complejidad en WM
- AttentionSystem no necesita el paso de binding
