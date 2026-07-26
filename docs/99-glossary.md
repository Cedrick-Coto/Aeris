# Glosario de Términos

**Versión**: 0.1  
**Estado**: Sprint 0 — Especificación  
**Última actualización**: 2026-07-26

---

## 1. Términos del ECS

### Entity
**Definición**: Identificador único que representa una unidad discreta del mundo. No contiene datos ni lógica. Solo existe.  
**Ver**: `01-ecs-model.md` → 1.1 Entity

### Component
**Definición**: Contenedor de datos puros asociado a una Entity. No contiene lógica. Solo contiene datos.  
**Ver**: `01-ecs-model.md` → 1.2 Component

### System
**Definición**: Transformación que opera sobre Components. Lee Componentes, aplica lógica, modifica Componentes, y emite Events.  
**Ver**: `01-ecs-model.md` → 1.3 System

### Event
**Definición**: Mensaje inmutable que un System emite y que otros Systems reciben. La única forma de comunicación entre Systems.  
**Ver**: `01-ecs-model.md` → 1.4 Event

### Resource
**Definición**: Dato compartido global que no pertenece a ninguna Entity. Acceso directo por Systems.  
**Ver**: `01-ecs-model.md` → 1.5 Resource

### Archetype
**Definición**: Conjunto de Component Types que una Entity debe poseer para ser procesada por un System.  
**Ver**: `01-ecs-model.md` → 4. Archetype

### World
**Definición**: Contenedor de todo el estado ECS. Entitys, Components, Resources.  
**Ver**: `01-ecs-model.md` → 3. World

### World State
**Definición**: Estado completo del mundo en un momento dado. Incluye todas las Entitys, sus Components, y todos los Resources.  
**Ver**: `01-ecs-model.md` → 3. World

---

## 2. Términos de Simulación

### Simulation Tick
**Definición**: Unidad temporal de simulación. Cada tick ejecuta el pipeline completo: Input → Percepción → Cognición → Planificación → Acción → Consecuencias → Narración → Persistencia.  
**Ver**: `02-execution-contract.md` → 2. El Pipeline

### Simulation Time
**Definición**: Tiempo del mundo simulado. No es el tiempo real. Todo el motor consulta únicamente el tiempo de simulación.  
**Ver**: `02-execution-contract.md` → 4. Tiempo de Simulación

### Time Scale
**Definición**: Factor de escala entre tiempo real y tiempo de simulación. 1x = tiempo real, 60x = 1 segundo real = 1 minuto de simulación.  
**Ver**: `02-execution-contract.md` → 4.2 Relación Real → Simulación

### Simulation Engine
**Definición**: Orquestador principal del motor. Ejecuta el pipeline de simulación en cada tick.  
**Ver**: `02-execution-contract.md` → 5. Ciclo de Vida Completo de un Tick

### EventBus
**Definición**: Sistema de cola de eventos dual-queue que distribuye Events entre Systems. Los eventos Deferred se acumulan durante el tick y se procesan al inicio del siguiente. Los eventos Immediate se procesan solo en errores fatales.  
**Ver**: `04-simulation-systems.md` → 9. EventBus

### Scheduler
**Definición**: Resource que permite programar eventos futuros. Ejecuta callbacks en un tiempo de simulación específico. Vive junto a TimeResource, RandomResource, y ConfigurationResource.  
**Ver**: `04-simulation-systems.md` → 10. Scheduler (Resource)

---

## 3. Términos Cognitivos

### Semantic State
**Definición**: El subconjunto del estado del mundo que el LLM necesita para producir narrativa. Es un traductor entre un simulador determinista y un modelo de lenguaje probabilístico. No es memoria, no es conocimiento, no es emoción. Es el estado narrativo.  
**Ver**: `05-semantic-state.md` → 1. Qué es el Semantic State

### Semantic Extractor
**Definición**: Componente que extrae del estado del mundo el subconjunto mínimo que el LLM necesita para producir narrativa. Filtra, prioriza, y traduce datos técnicos a lenguaje natural. Se diferencia del Prompt Builder en que solo extrae datos, no construye prompts.  
**Ver**: `05-semantic-state.md` → 4. Flujo del Semantic Extractor

### Memory
**Definición**: Registro de un evento que una Entity ha experimentado o percibido. Cada memoria tiene carga emocional, certeza, importancia, y se degrada con el tiempo.  
**Ver**: `03-data-models.md` → 4.1 MemoryData

### Belief
**Definición**: Algo que un personaje cree que es verdad. Las creencias pueden estar equivocadas. Se revisan con nueva evidencia.  
**Ver**: `03-data-models.md` → 4.2 BeliefData

### Knowledge
**Definición**: Información que un personaje posee. Puede ser hechos, rumores, tradiciones, o habilidades. Tiene un nivel de certeza asociado.  
**Ver**: `03-data-models.md` → 4.3 KnowledgeData

### Goal
**Definición**: Objetivo que un personaje persigue. Tiene prioridad, urgencia, y un estado (activo, pausado, completado, fallido).  
**Ver**: `03-data-models.md` → 4.4 GoalData

### Emotion
**Definición**: Estado emocional temporal de un personaje. Se activa por triggers y se disipa con el tiempo. No es permanente.  
**Ver**: `03-data-models.md` → 4.5 EmotionData

### Attention
**Definición**: Enfoque perceptual de un personaje. Determina qué puede percibir en su entorno.  
**Ver**: `03-data-models.md` → 4.6 AttentionData

---

## 4. Términos Sociales

### Relationship
**Definición**: Conexión entre dos Entitys. Tiene un tipo (amigo, rival, etc.), un valor (-1 a 1), y un nivel de confianza (0 a 1).  
**Ver**: `03-data-models.md` → 5.1 RelationshipData

### Relationship Type
**Definición**: Categoría de la relación: Neutral, Friend, Rival, Mentor, Student, Family, Romantic, Enemy, Ally, Stranger.  
**Ver**: `03-data-models.md` → 5.1 RelationshipData

### Relationship Strength
**Definición**: Qué fuerte es la relación: Acquaintance, Associate, Friend, CloseFriend, BestFriend, Soulmate.  
**Ver**: `03-data-models.md` → 5.1 RelationshipData

### Social Perception
**Definición**: Capacidad de un personaje para percibir el estado social de su entorno (quién está con quién, qué sienten, qué relationship tienen).  
**Ver**: `04-simulation-systems.md` → 3.3 SocialPerceptionSystem

---

## 5. Términos del Mundo

### Region
**Definición**: Unidad espacial principal del mundo. Contiene rutas, asentamientos y ecosistemas. Se modela como un nodo en un grafo.  
**Ver**: `10-world-model.md` → 2.1 Región

### Route
**Definición**: Camino que conecta dos o más regiones. Se modela como una arista en un grafo.  
**Ver**: `10-world-model.md` → 2.2 Ruta

### Settlement
**Definición**: Lugar donde habitan personajes de forma permanente. Ciudad, pueblo, aldea, campamento.  
**Ver**: `10-world-model.md` → 2.3 Asentamiento

### Ecosystem
**Definición**: Comunidad de seres vivos y su entorno. Incluye especies, recursos, y condiciones ambientales.  
**Ver**: `10-world-model.md` → 2.4 Ecosistema

### Population
**Definición**: Grupo de la misma especie que habita un ecosistema. Tiene densidad, estado, y patrones de migración.  
**Ver**: `10-world-model.md` → 2.5 Población

### World Event
**Definición**: Evento que afecta al mundo: cambio de clima, festival, desastre natural, migración. Ocurre independientemente del usuario.  
**Ver**: `10-world-model.md` → 5. Eventos del Mundo

---

## 6. Términos de Persistencia

### Persistence
**Definición**: Sistema de guardado y carga del estado del mundo. Usa SQLite para estado y JSON para configuración.  
**Ver**: `07-persistence.md` → 1. Estrategia Dual

### Save Strategy
**Definición**: Determina cuándo guardar el estado del mundo. Puede ser por tiempo, por eventos, o manual.  
**Ver**: `07-persistence.md` → 2.4 Estrategia de Guardado

### Migration
**Definición**: Cambio en el esquema de la base de datos que debe aplicarse para mantener compatibilidad con versiones anteriores.  
**Ver**: `07-persistence.md` → 4. Migraciones

---

## 7. Términos de Narrativa

### Narrative Pipeline
**Definición**: Sistema que transforma el estado del mundo en narrativa para el usuario. Incluye construcción de contexto, generación con LLM, y formateo.  
**Ver**: `08-narrative-pipeline.md` → 1. Definición

### Prompt Builder
**Definición**: Componente que construye el prompt que se envía al LLM. Incluye Semantic State, instrucciones, y restricciones.  
**Ver**: `08-narrative-pipeline.md` → 2.3 Construcción del Prompt

### Narrator
**Definición**: Entidad que transforma el estado del mundo en texto narrativo. Puede ser el LLM o un generador local.  
**Ver**: `08-narrative-pipeline.md` → 1. Definición

### Suspense Level
**Definición**: Nivel de misterio en la narrativa. Desde None (narrar todo) hasta Extreme (solo lo esencial).  
**Ver**: `08-narrative-pipeline.md` → 3.2 Niveles de Suspense

---

## 8. Términos de IA

### LLM (Large Language Model)
**Definición**: Modelo de lenguaje que interpreta el estado del mundo y lo traduce en narrativa. Es una función, no un controlador.  
**Ver**: `06-llm-contract.md` → 1. Principio Fundamental

### LLM Adapter
**Definición**: Interfaz que abstrae la comunicación con diferentes proveedores LLM (OpenAI, Claude, Ollama, etc.).  
**Ver**: `06-llm-contract.md` → 2. Abstracción del Proveedor

### LLM Request
**Definición**: Estructura que se envía al LLM. Incluye Semantic State, input del jugador, historial, y restricciones.  
**Ver**: `06-llm-contract.md` → 2. Abstracción del Proveedor

### LLM Response
**Definición**: Estructura que devuelve el LLM. Incluye narración, diálogo, pensamientos, acciones, y confianza.  
**Ver**: `06-llm-contract.md` → 2. Abstracción del Proveedor

---

## 9. Términos de Diseño

### Architecture Decision Record (ADR)
**Definición**: Documento que registra una decisión arquitectónica importante: contexto, alternativas, decisión, consecuencias, estado.  
**Ver**: `docs/adr/`

### Invariante
**Definición**: Regla que nunca puede romperse. Si se viola, el motor está en estado inválido.  
**Ver**: `11-engine-invariants.md` → 1. Definición

### Extension Point
**Definición**: Lugar del motor diseñado para ser extendido sin modificar el núcleo.  
**Ver**: `12-extension-points.md` → 1. Definición

### Data-Oriented Design (DOD)
**Definición**: Paradigma de diseño que prioriza la organización de datos sobre la jerarquía de objetos. El ECS es un ejemplo de DOD.  
**Ver**: `00-overview.md` → 4. Arquitectura de Alto Nivel

### Entity Component System (ECS)
**Definición**: Patrón de diseño donde las Entitys son IDs, los Components son datos, y los Systems son transformaciones.  
**Ver**: `01-ecs-model.md` → 1. Definiciones Formales

---

## 10. Términos Pokémon

### Aura
**Definición**: Firma energética única de cada Pokémon. Puede ser detectada por otros Pokémon con la habilidad adecuada.  
**Ver**: `03-data-models.md` → 3.4 AuraData

### Aura Signature
**Definición**: Firma espectral única de un aura. Compuesta por frecuencias, amplitud, y coherencia.  
**Ver**: `03-data-models.md` → 3.4 AuraData

### Species
**Definición**: Tipo de Pokémon. Determina las estadísticas base, movimientos, y comportamiento.  
**Ver**: `03-data-models.md` → 2.1 IdentityData

### Evolution
**Definición**: Proceso por el cual un Pokémon cambia de forma y mejora sus estadísticas.  
**Ver**: `12-extension-points.md` → 2. Nuevos Systems

---

## 11. Convenciones de Uso

### Mayúsculas en definiciones
Cuando un término aparece en **negrita** al inicio de una entrada del glosario, indica que es la definición canónica del término.

### Referencias cruzadas
Las referencias a documentos específicos (`Ver: 01-ecs-model.md → 1.1`) indican dónde encontrar la definición completa y los detalles de implementación.

### Términos no definidos
Si un término aparece en el código o documentación pero no está en este glosario, debe ser añadido aquí antes de su uso generalizado.
