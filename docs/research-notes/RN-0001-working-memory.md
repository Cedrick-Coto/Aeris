# RN-0001: Working Memory (Baddeley)

**Estado**: Referenced  
**Última actualización**: 2026-07-29

---

## Tema

Modelo de memoria de trabajo y sus implicaciones para la arquitectura cognitiva del agente.

---

## Resumen de la literatura

### Baddeley & Hitch (1974) — Modelo multicomponente

La memoria de trabajo no es un almacén único sino un sistema compuesto por:

- **Ejecutivo central**: control atencional, coordinación de subsistemas
- **Bucle fonológico**: información verbal y auditiva
- **Agenda visoespacial**: información visual y espacial
- **Buffer episódico** (Baddeley, 2000): integración multimodalde información en episodios

### Cowan (2001) — Límite de capacidad

Cowan propone que la capacidad de la memoria de trabajo es de aproximadamente 4 ± 1 chunks, significativamente menor que los 7 ± 2 de Miller (1956). La diferencia radica en qué se cuenta como "chunk" y si se incluye el material decayendo.

### Oberauer (2002) — Tres niveles

Oberauer distingue tres niveles dentro del foco atencional:
1. **Foco interno**: un único chunk en procesamiento activo
2. **Región de acceso directo**: ~4 chunks disponibles inmediatamente
3. **Memoria de trabajo expandida**: material recuperable pero no activo

### Aportación al diseño del agente

El WorkingMemorySystem (doc-17 §3.4) implementa:
- Capacidad limitada configurable (default 7 ± 2, con opción a 4 ± 1)
- Decaimiento por falta de refresco
- Refrescamiento por re-atención desde AttentionSystem
- Un único chunk como "foco activo" (la inferencia o percepto en procesamiento)

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| WorkingMemorySystem | Diseño directo (capacidad, decaimiento, refresco) |
| AttentionSystem | La atención determina qué entra a WM |
| ReasoningSystem | Opera sobre el contenido de WM |
| DecisionSystem | Consulta WM para evaluar planes |

---

## Estado de decisión

- El diseño de WorkingMemorySystem (doc-17 §3.4) está inspirado en Baddeley y Cowan.
- No se ha tomado una decisión arquitectónica sobre si implementar el bucle fonológico y la agenda visoespacial como buffers separados, o tratarlos como un solo sistema multimodalde perceptos.
- ADR-0010 (Perception Precedes Cognition) es consistente con la separación entre atención y WM.
