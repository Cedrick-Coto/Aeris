# ADR-0010: Perception Precedes Cognition

**Estado**: Accepted  
**Fecha**: 2026-07-28  
**Decidido por**: Cedrick

---

## Contexto

El pipeline del agente (definido en `16-agent-architecture.md`) establece un flujo desde el mundo simulado hasta la acción. Sin embargo, no está formalizado como invariante arquitectónico si **todo acceso cognitivo al mundo debe pasar por la percepción** o si ciertos sistemas pueden leer el estado del ECS directamente.

Esta decisión afecta:
- La integridad del pipeline cognitivo
- La consistencia de la información que reciben los subsistemas
- El determinismo del procesamiento interno
- La separación entre simulación (mundo) y cognición (agente)

## Alternativas Consideradas

### 1. Acceso directo al mundo

Cualquier sistema cognitivo puede leer componentes del ECS directamente:

```csharp
// Sistema de razonamiento lee el mundo directamente
var nearbyEntities = world.Query<Position, Species>();
```

**Pros**:
- Simple y eficiente
- Sin overhead de traducción

**Cons**:
- El razonamiento recibe información sin filtrar por atención
- No hay incertidumbre perceptiva (el agente "sabe" todo con certeza absoluta)
- Se salta el pipeline: percepción, atención, working memory
- Incoherente con la arquitectura de agente: el mundo y el agente no están separados

### 2. Pipeline estricto (decidido)

Todo acceso cognitivo al mundo pasa por el pipeline perceptual:

```
World
    ↓
PerceptionSystem → Percept[]
    ↓
AttentionSystem → FilteredPercepts
    ↓
WorkingMemory → Chunks
    ↓
Reasoning / Planning / Decision
```

**Pros**:
- El agente nunca accede al mundo directamente, solo a su representación percibida
- La incertidumbre y el sesgo atencional son parte natural del pipeline
- Separación clara entre simulación (mundo) y cognición (agente)
- Coherente con la arquitectura de agente cognitivo
- El determinismo del motor no se ve afectado (el pipeline es determinista)

**Cons**:
- Overhead: cada tick debe ejecutar PerceptionSystem y AttentionSystem antes que cualquier sistema cognitivo
- Complejidad: los sistemas cognitivos deben suscribirse a WorkingMemory, no al World

### 3. Pipeline con excepciones documentadas

Similar a la opción 2, pero algunos sistemas (ej. World Model) pueden leer el mundo directamente para tareas específicas.

**Pros**:
- Flexibilidad para casos donde la percepción sería redundante

**Cons**:
- Rompe el invariante
- Difícil de auditar qué sistemas leen el mundo directamente
- Pueden aparecer inconsistencias entre la representación percibida y la "real"

## Decisión

**Ningún sistema cognitivo accede al estado del mundo directamente. Todo acceso cognitivo pasa por el pipeline: Percepción → Atención → Memoria de Trabajo.**

Los únicos sistemas que pueden leer el World ECS directamente son:
- **PerceptionSystem**: traduce eventos y estado del mundo a Percepts
- **SemanticExtractor**: produce SemanticState para el LLM (ocurre después de la cognición)
- **PersistenceSystem**: guarda/carga el estado completo del mundo

Los sistemas cognitivos (Attention, WorkingMemory, Reasoning, Planning, Decision, Learning, Reflection, MetaReflection, SelfReconstruction) **nunca** consultan el World ECS directamente. Trabajan exclusivamente sobre:

- Percepts (salida de Perception)
- WorkingMemory chunks (salida de Attention)
- LongTermMemory (consolidación diferida)
- AffectState (sistema independiente que modula cognición)

### Pipeline formal

```
Fase 0: Simulación (World ECS avanza)
Fase 1: Percepción (World → Percepts)
Fase 2: Atención (Percepts → WorkingMemory)
Fase 3: Cognición (WorkingMemory → Reasoning → Planning → Decision)
Fase 4: Afecto (modula Fases 2 y 3)
Fase 5: Acción (Decision → EventBus → World)
Fase 6: Reflexión (post-acción → Memory)
```

## Consecuencias

### Positivas
- Separación clara entre simulación y cognición
- Incertidumbre y sesgo perceptivo son naturales, no añadidos
- El agente no "sabe" nada que no haya percibido
- Invariante auditable: ningún sistema cognitivo referencia World directamente
- Coherente con la arquitectura de agente cognitivo

### Negativas
- Los sistemas cognitivos no pueden acceder a información "perfecta" del mundo
- Overhead de traducción en cada tick (PerceptionSystem debe ejecutarse siempre)

### Riesgos
- Un sistema cognitivo podría necesitar acceso directo al World por eficiencia (mitigado: si surge un caso legítimo, se documenta como excepción explícita y se crea un ADR de enmienda)
- WorkingMemory podría no contener suficiente información para razonar (mitigado: AttentionSystem debe configurarse con un presupuesto adecuado al contexto)

## Follow-up
- Implementar PerceptionSystem en Sprint 3.1 como primer sistema del pipeline cognitivo
- Implementar AttentionSystem en Sprint 3.1 como filtro entre Perception y WorkingMemory
- Agregar análisis estático o code review para verificar que ningún sistema cognitivo reference World directamente
- Documentar las excepciones (PerceptionSystem, SemanticExtractor, PersistenceSystem) de forma explícita
