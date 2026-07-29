# RN-0002: Modelos de Afecto en Arquitecturas Cognitivas

**Estado**: Referenced
**Última actualización**: 2026-07-29

---

## Tema

Modelos computacionales del afecto y su integración en arquitecturas cognitivas artificiales. Fundamento para el diseño de AffectSystem (doc-17 §3.3).

---

## Resumen de la literatura

### Damasio (1994) — Somatic Marker Hypothesis

Las emociones corporales (somatic markers) guían la toma de decisiones al sesgar la evaluación de opciones. Un marcador somático negativo asociado a una opción la descarta rápidamente sin necesidad de razonamiento completo. En el agente, esto se traduce en modulación afectiva de DecisionSystem: un AffectState con Threat alto descarta planes arriesgados antes de evaluarlos completamente.

### Russell (1980) — Circumplex Model of Affect

El afecto no son categorías discretas sino un espacio continuo de dos dimensiones: valence (placer/displacer) y arousal (activación/calma). El diseño de AffectSystem como vector continuo (9 variables) es una extensión de esta idea: en lugar de dos dimensiones, se usan dimensiones específicas del dominio (Curiosity, Stress, Trust, etc.).

### Rolls (1999) — Reinforcement Sensitivity Theory

Las emociones son estados producidos por refuerzos y castigos. El afecto es un mecanismo de valoración: eventos positivos aumentan RewardExpectation y Confidence; eventos negativos aumentan Threat y Stress. Esto justifica que AffectSystem se actualice from percepts y eventos, no desde un generador interno de emociones.

### Panksepp (2004) — Affective Neuroscience

Siete sistemas emocionales primarios (SEEKING, FEAR, RAGE, LUST, CARE, PANIC, PLAY) con base neurobiológica. Aunque no se implementan como sistemas separados, los 9 vectores de ACMA v1 cubren funciones análogas: Curiosity ≈ SEEKING, Threat ≈ FEAR, Attachment ≈ CARE.

### Oatley & Johnson-Laird (1987) — Cognitive Theory of Emotions

Las emociones son modos de gestión cognitiva: cada emoción prioriza ciertos goals y establece un modo de procesamiento. Es la base de la modulación transversal: AffectState no produce acciones directamente, sino que cambia pesos y umbrales en otros subsistemas.

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| AffectSystem | Diseño directo (vector continuo, homeostasis, modulación) |
| AttentionSystem | Modulación por arousal, novelty, threat |
| WorkingMemorySystem | Modulación por cognitiveLoad, stress |
| ReasoningSystem | Modulación por confidence, threat |
| PlanningSystem | Modulación por confidence, threat |
| DecisionSystem | Modulación por confidence, stress |
| IdentityReconstructionSystem | Afecto como entrada al SelfSnapshot |

---

## Estado de decisión

- El diseño de AffectSystem (doc-17 §3.3) como vector continuo de 9 variables se inspira en Russell (circumplex), Rolls (RST) y Oatley & Johnson-Laird (modos cognitivos).
- La decisión de no usar etiquetas emocionales discretas está respaldada por Russell (1980) y ADR-0008.
- La modulación transversal de todos los subsistemas (en lugar de un sistema separado de «gestión emocional») se basa en Oatley & Johnson-Laird (1987).
- Hipótesis relacionada: H-0002 (presupuesto atencional dinámico por CognitiveLoad).
