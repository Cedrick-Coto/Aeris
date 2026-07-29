# RN-0003: Identidad y Reconstrucción del Self

**Estado**: Reviewing
**Última actualización**: 2026-07-29

---

## Tema

Modelos del self, identidad narrativa y reconstrucción del yo en psicología cognitiva y filosofía de la mente. Fundamento para IdentityReconstructionSystem (doc-17 §3.12).

---

## Resumen de la literatura

### Dennett (1991) — Multiple Drafts Model

No hay una versión única y definitiva de la experiencia consciente. Hay múltiples «borradores» de contenido que compiten y se revisan continuamente. El self no es un centro de control sino una **narrativa** que el cerebro produce. El IdentityReconstructionSystem implementa esta idea: no hay un componente Self persistente; cada tick se genera un nuevo snapshot.

### Neisser (1988) — Five Kinds of Self-Knowledge

Neisser distingue cinco tipos de conocimiento del self:
1. **Ecológico**: el self en relación al entorno físico
2. **Interpersonal**: el self en interacción con otros
3. **Extendido**: el self a través del tiempo (memoria autobiográfica)
4. **Privado**: experiencias que otros no comparten
5. **Conceptual**: teorías sobre uno mismo (roles, valores, principios)

SelfSnapshot en doc-17 cubre varios de estos: ActivePrinciples (conceptual), SignificantRelationships (interpersonal), NarrativeSummary (extendido), SelfSummary (integración).

### Conway (2005) — Self-Memory System

El self está compuesto por:
- **Working self**: goals activos y prioridades actuales
- **Autobiographical knowledge base**: conocimiento estructurado sobre la propia vida
- **Control processes**: regulación entre el working self y la base de conocimiento

El SelfSnapshot de ACMA integra los goals activos (WorkingSelf) con la memoria autobiográfica, actualizando la narrativa cada tick. Conway es una de las bases más fuertes para el diseño actual.

### Gallagher (2000) — Minimal Self vs Narrative Self

Distinción entre:
- **Minimal self**: sentido del self aquí y ahora (pre-reflexivo)
- **Narrative self**: identidad integrada con pasado y futuro (reflexivo)

ACMA implementa ambos: el minimal self corresponde al SelfSnapshot del tick actual (qué soy ahora); el narrative self a la integración de snapshots a lo largo del tiempo. El NarrativeSummary en SelfSnapshot intenta ser el puente entre ambos.

### ADR-0006 — Self Model Is Reconstructed

Decisión arquitectónica fundacional: el self no se almacena como componente ECS. Se reconstruye cada tick desde memoria autobiográfica, afecto, goals y relaciones. Esta nota proporciona el respaldo teórico para esa decisión.

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| IdentityReconstructionSystem | Diseño directo (SelfSnapshot, NarrativeSummary) |
| LongTermMemorySystem | La memoria autobiográfica alimenta la identidad |
| GoalSystem | Los goals activos forman el «working self» |
| AffectSystem | El afecto actual colorea la narrativa del self |
| Semantic Extractor | Toma el SelfSnapshot para construir el prompt del LLM |

---

## Estado de decisión

- ADR-0006 está respaldado por Dennett (multiple drafts) y Conway (self-memory system).
- La estructura de SelfSnapshot (NarrativeSummary, ActivePrinciples, SignificantRelationships) cubre 4 de los 5 tipos de self-knowledge de Neisser (falta el privado, que no tiene representación explícita en el modelo).
- La frecuencia de reconstrucción (cada tick) es una hipótesis abierta. Ver H-0003.
- No hay una decisión arquitectónica sobre si SelfSnapshot debe incluir una proyección del self futuro.
