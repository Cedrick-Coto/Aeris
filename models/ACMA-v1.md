# ACMA v1 — Cognitive Model

**Estado**: Planned  
**Versión**: 1.1  
**Última actualización**: 2026-07-30  
**Sprint destino**: 3B (7 micro-sprints)  

---

## Propósito

ACMA (Agente Cognitivo con Memoria y Afecto) es el primer **Cognitive Model** concreto del proyecto Aeris. Define qué variables afectivas existen, cómo se ponderan, qué algoritmos de razonamiento y planificación se usan, y cómo se reconstruye el self.

Este documento contiene **solo el modelo**, no la infraestructura. El motor ECS, los sistemas de infraestructura cognitiva, el Semantic Extractor y el LLM son independientes de ACMA.

---

## Dependencias arquitectónicas

| Documento | Relación |
|-----------|----------|
| doc-17 (Computational Agent Model) | Define las interfaces que ACMA v1 implementa |
| ADR-0006 | Self Model Is Reconstructed, Not Stored |
| ADR-0008 | Affect Is Functional, Not Human |
| ADR-0009 | Identity Is Emergent |
| ADR-0010 | Perception Precedes Cognition |

---

## Qué define

### AffectModel

9 variables continuas que constituyen el estado afectivo:

| Variable | Rango | Homeostasis | Rol |
|----------|-------|-------------|-----|
| Curiosity | [0, 1] | 0.5 | Impulso exploratorio |
| Stress | [0, 1] | 0.2 | Degradación cognitiva |
| Confidence | [0, 1] | 0.6 | Autoeficacia percibida |
| Trust | [0, 1] | 0.4 | Apertura a otros |
| Novelty | [0, 1] | 0.3 | Percepción de novedad |
| Attachment | [0, 1] | 0.3 | Vínculo con entidades significativas |
| Threat | [0, 1] | 0.1 | Percepción de peligro |
| RewardExpectation | [0, 1] | 0.5 | Anticipación de refuerzo |
| CognitiveLoad | [0, 1] | 0.3 | Sobrecarga computacional |

**Reglas ACMA v1:**
- Homeostasis: cada variable tiende a su valor basal con velocidad configurable
- Actualización: por perceptos atendidos, eventos, y estado previo
- No existen etiquetas discretas («Happy», «Sad», «Angry»)

### Identity Reconstruction Algorithm

**Método ACMA v1:**
1. Consultar AutobiographicalMemory (episodios con significancia > umbral)
2. Extraer principios activos desde creencias consolidadas
3. Evaluar capacidades percibidas desde goals completados recientemente
4. Integrar relaciones significativas (strength + valence > umbral)
5. Componer SelfSummary desde: objetivos + recuerdos + relaciones + principios + decisiones anteriores + modelo del mundo
6. Calcular CoherenceScore como consistencia interna del snapshot

**Frecuencia:** cada tick (configurable vía H-0003)

### Goal Activation Strategy

**Método ACMA v1:**
- Goals se activan por: necesidades basales, eventos externos, inferencias de Reasoning
- Prioridad dinámica: `priority = basePriority × affectModulation(urgency, stress)`
- Goals completados o fallidos → AutobiographicalMemory
- Siempre hay al menos un goal activo (goal de exploración por defecto)

### Planning Strategy

**Método ACMA v1:**
- Generación: construir planes desde un espacio de acciones predefinido
- Evaluación: simulación forward en WorldModel (horizonte truncado, H-0006)
- Selección: plan con mejor relación costo/beneficio esperado
- Modulación afectiva: Confidence bajo → planes cortos; Threat alto → planes conservadores; Curiosity alto → planes exploratorios

### Attention Model

**Método ACMA v1:**
- Presupuesto fijo de N perceptos por tick (configurable)
- Saliencia: `saliencia(p) = novelty(p) × relevance(p, goals) × affectModulation(p, affect)`
- Arousal alto → N más grande (atención dispersa)
- Stress alto → sesgo hacia perceptos de amenaza

### World Model Assumptions

**Método ACMA v1:**
- Representación probabilística parcial del mundo
- Incluye: mapa mental de localizaciones conocidas, relaciones causales observadas, teoría de otros agentes (básica)
- No incluye: predicciones de largo plazo, modelo explícito de incertidumbre
- Actualización: por percepción e inferencia

---

## Qué NO define

ACMA v1 no define ni modifica:

| Componente | Responsable |
|------------|-------------|
| ECS World | Motor (Sprint 1) |
| EventBus | Infraestructura (Sprint 1) |
| Scheduler | Infraestructura (Sprint 1) |
| Time System | Infraestructura (Sprint 1) |
| Persistence | Infraestructura (Sprint 1) |
| Semantic Extractor | Sprint 2 |
| PerceptionSystem | Sprint 3A |
| AttentionSystem | Sprint 3A |
| WorkingMemorySystem | Sprint 3A |
| LongTermMemorySystem | Sprint 3A |
| AffectSystem (continuous vector) | Sprint 3A |
| GoalSystem (infraestructura) | Sprint 3A |
| WorldModelSystem | Sprint 3A |
| MemoryRetrievalSystem | Sprint 3B.1 |
| ReasoningSystem | Sprint 3B.2 |
| PlanningSystem | Sprint 3B.3 |
| DecisionSystem | Sprint 3B.4 |
| AuditorSystem | Sprint 3B.5 |
| IdentityReconstructionSystem | Sprint 3B.6 |
| SelfSnapshot | Sprint 3B.7 |
| LLM Integration | Sprint 4 |
| Narrative Pipeline | Sprint 5 |
| Pokémon World | Sprint 6 |

---

## Contrato de versión

| Puede cambiar | No puede cambiar |
|---------------|------------------|
| Implementación interna de cualquier sistema del Cognitive Model | Orden de la cadena causal |
| Variables del AffectState | Interfaces de entrada/salida de infraestructura |
| Algoritmos de modulación afectiva | Determinismo del núcleo |
| Estructura de SelfSnapshot | Contrato con el Semantic Extractor |
| Baselines de personalidad | — |

---

## Reemplazabilidad

ACMA v1 puede reemplazarse por ACMA v2 (o cualquier otro modelo) sin cambiar:

- El motor ECS
- La infraestructura cognitiva (Sprint 3A)
- El Semantic Extractor
- La Narrative Pipeline
- El LLM

Para reemplazar ACMA v1:
1. Implementar las mismas interfaces definidas en doc-17
2. Registrar el nuevo modelo en `Aeris.Agent.ACMAVersion`
3. El motor selecciona el modelo por configuración
