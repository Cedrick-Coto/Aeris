# RN-0005: Modelos Internos del Mundo en Arquitecturas Cognitivas

**Estado**: Reviewing
**Última actualización**: 2026-07-29

---

## Tema

Modelos mentales, representaciones internas del entorno, y su uso en razonamiento y planificación. Fundamento para WorldModelSystem (doc-17 §3.6).

---

## Resumen de la literatura

### Craik (1943) — Internal Models as Cognitive Basis

El cerebro construye modelos a escala del mundo exterior para anticipar eventos y planificar acciones. Un modelo interno permite «probar» acciones mentalmente antes de ejecutarlas. Es la justificación fundamental de que WorldModel exista como entidad separada del ECS World.

### Johnson-Laird (1983) — Mental Models

Razonamos no con lógica formal, sino construyendo modelos mentales de situaciones. Un modelo mental es una representación analógica (no proposicional) que refleja la estructura de lo que representa. Los modelos pueden ser parciales, inconsistentes, y múltiples. Esto justifica que WorldModel sea probabilístico (H-0004): un modelo puede contener simultáneamente representaciones alternativas del mundo.

### Grush (2004) — Emulation Theory of Representation

El sistema nervioso construye emuladores internos que simulan interacciones con el entorno. Estos emuladores pueden ejecutarse offline (sin entrada sensorial) para planificar. El WorldModel de ACMA funciona como emulador: PlanningSystem ejecuta simulaciones forward sobre el modelo, no sobre el mundo real.

### Tenenbaum et al. (2011) — Probabilistic Models of Cognition

La cognición humana puede modelarse como inferencia probabilística sobre modelos generativos. Las creencias son distribuciones de probabilidad, no proposiciones verdaderas/falsas. Esto respalda la aproximación probabilística del WorldModel (H-0004).

### Theory of Mind (Premack & Woodruff, 1978; Baron-Cohen, 1995)

La capacidad de atribuir estados mentales a otros agentes. Requiere que el WorldModel represente no solo el estado del mundo, sino también las creencias, deseos e intenciones de otros. Actualmente doc-17 §3.6 deja abierta esta pregunta: «¿Debe modelar explícitamente las creencias de otros agentes?».

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| WorldModelSystem | Diseño directo (estructura, granularidad, teoría de mente) |
| PlanningSystem | Evalúa planes simulando sobre el WorldModel |
| ReasoningSystem | Opera sobre el WorldModel para inferencias causales |
| DecisionSystem | Consulta el WorldModel para validar el plan actual |

---

## Estado de decisión

- La existencia de WorldModel como entidad separada del ECS World está justificada por Craik (1943) y Grush (2004).
- La pregunta «probabilístico vs simbólico» (doc-17 §3.6) no está resuelta. Johnson-Laird y Tenenbaum apuntan a modelos probabilísticos; la ingeniería sugiere una solución híbrida. Ver H-0004.
- El soporte para theory of mind no está decidido arquitectónicamente. Tendría impacto significativo en la complejidad del WorldModel.
- RN-0006 (Planning) complementa esta nota: la planificación depende críticamente del WorldModel.
