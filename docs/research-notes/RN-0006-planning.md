# RN-0006: Planificación en Agentes Cognitivos

**Estado**: Reviewing
**Última actualización**: 2026-07-29

---

## Tema

Modelos de planificación, simulación mental, evaluación de cursos de acción. Fundamento para PlanningSystem (doc-17 §3.9).

---

## Resumen de la literatura

### Miller, Galanter & Pribram (1960) — Plans and TOTE

La conducta está organizada en planes: estructuras jerárquicas que secuencian acciones para alcanzar metas. El TOTE (Test-Operate-Test-Exit) es la unidad básica: probar si se ha alcanzado el estado deseado, operar si no, probar de nuevo, salir cuando se cumple. El PlanningSystem de ACMA sigue este esquema: evaluar plan → ejecutar paso → re-evaluar.

### Hazy, Frank & O'Reilly (2007) — Prefrontal Cortex and Planning

La corteza prefrontal mantiene representaciones activas de goals y subgoals mientras se ejecutan planes. La capacidad de mantener y manipular estas representaciones depende de WorkingMemory. Esto justifica la conexión entre PlanningSystem y WorkingMemorySystem en la cadena causal.

### Sutton & Barto (1998) — Reinforcement Learning and Planning

Dos aproximaciones a la planificación:
- **Model-based**: construir un modelo del entorno y simular (equivalente al enfoque de ACMA)
- **Model-free**: aprender asociaciones directas estado-acción sin modelo

Sutton & Barto muestran que model-based permite generalización rápida pero es computacionalmente caro. Esto respalda la decisión de ACMA de usar WorldModel para simulación, y justifica la pregunta sobre si la simulación debe ser truncada (H-0006).

### Newell & Simon (1972) — GPS (General Problem Solver)

Búsqueda de medios-fines (means-ends analysis): comparar estado actual con goal, identificar diferencias, buscar operadores que reduzcan las diferencias. Es el algoritmo clásico de planificación, heredado de la tradición GOFAI. ACMA no usa means-ends analysis directamente, pero la generación de planes desde acciones posibles hereda de esta tradición.

### Grush (2004) — Emulation Theory (también cubierto en RN-0005)

Los planes se evalúan mediante simulación mental en un emulador interno (WorldModel). La simulación puede ser forward (desde el estado actual hacia el goal) o backward (desde el goal hacia el estado actual).

---

## Impacto potencial

| Subsistema | Naturaleza del impacto |
|------------|------------------------|
| PlanningSystem | Diseño directo (generación, evaluación, selección de planes) |
| WorldModelSystem | La planificación depende del WorldModel para simular |
| DecisionSystem | Toma el plan y extrae la siguiente acción |
| WorkingMemorySystem | Mantiene el plan activo durante la ejecución |

---

## Estado de decisión

- ACMA usa planificación model-based (Sutton & Barto), no model-free.
- No se ha decidido si la simulación debe ser forward, backward o ambas (doc-17 §3.7, Open Question).
- El horizonte de planificación (simulación completa vs truncada) es una hipótesis abierta. Ver H-0006.
- No hay una decisión arquitectónica sobre el número de planes a evaluar por tick. Ver doc-17 §3.9.
- La modulación afectiva de la planificación (doc-17 §3.9) está inspirada en Damasio (somatic markers) y Oatley & Johnson-Laird (modos cognitivos). Ver RN-0002.
