# RN-0004: Modelos de Atención en Ciencias Cognitivas

**Estado**: Referenced
**Última actualización**: 2026-07-29

---

## Tema

Modelos de atención selectiva, filtrado temprano vs tardío, redes atencionales y modulación por contexto. Fundamento para AttentionSystem (doc-17 §3.2).

---

## Resumen de la literatura

### Broadbent (1958) — Filter Model of Attention

La atención es un filtro temprano: la información sensorial pasa por un canal de capacidad limitada que selecciona basándose en propiedades físicas. Broadbent justifica que el AttentionSystem tenga un presupuesto computacional fijo (N perceptos por tick) y que el filtrado ocurra antes del procesamiento semántico.

### Treisman (1964) — Attenuation Theory

La información no seleccionada no se bloquea completamente, sino que se atenúa. Puede activar representaciones si tiene suficiente intensidad o relevancia. Esto justifica que en el diseño los perceptos descartados no se eliminen por completo: se degradan con posibilidad de reintroducción si cambia el contexto.

### Posner (1980) — Orienting of Attention

Tres componentes de la atención:
1. **Alerting**: alcanzar y mantener un estado de alerta
2. **Orienting**: seleccionar información sensorial de ubicaciones específicas
3. **Executive control**: resolver conflictos entre respuestas

El AttentionSystem de doc-17 cubre orienting (selección por saliencia) y executive control (modulación por goals y afecto). El componente alerting se modela indirectamente via CognitiveLoad.

### Corbetta & Shulman (2002) — Dorsal/Ventral Attention Networks

Dos redes neurales:
- **Dorsal (top-down)**: atención voluntaria guiada por goals y expectativas
- **Ventral (bottom-up)**: atención involuntaria guiada por estímulos salientes

El AttentionSystem combina ambas: top-down via Goals y AffectState (modulación por goals activos), bottom-up via novelty y threat (estímulos salientes independientes de goals).

### Lavie (1995, 2005) — Load Theory of Attention

La eficacia del filtrado atencional depende de la carga perceptual y cognitiva. Baja carga → más distracción (se procesa más información irrelevante). Alta carga → filtrado más temprano. Esto es la base de H-0002: el presupuesto atencional N debe ser dinámico según CognitiveLoad.

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| AttentionSystem | Diseño directo (presupuesto fijo, saliencia, modulación) |
| WorkingMemorySystem | Lo que Attention selecciona determina qué entra a WM |
| PerceptionSystem | La saliencia retroalimenta qué perceptos se generan |
| ReasoningSystem | Los perceptos atendidos limitan las inferencias disponibles |

---

## Estado de decisión

- El algoritmo de saliencia como producto de novelty × relevance × affectModulation combina el modelo de Broadbent (filtro) con la modulación top-down de Corbetta & Shulman.
- La separación entre perceptos atendidos y descartados (vs atenuados) sigue a Treisman: los descartados no se pierden, solo se degradan.
- No se ha tomado una decisión arquitectónica sobre si N (presupuesto atencional) debe ser fijo o dinámico. Ver H-0002.
- El modelo no implementa aún el componente de «alerting» de Posner; CognitiveLoad es un proxy parcial.
- Hipótesis relacionada: H-0002 (presupuesto dinámico).
