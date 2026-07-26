# ADR-0004: LLM como Función, no como Controlador

**Estado**: Accepted  
**Fecha**: 2026-07-26  
**Decidido por**: Cedrick

---

## Contexto

El motor necesita usar un LLM para generar narrativa. La pregunta clave es: **¿cuánto control tiene el LLM sobre el mundo?**

## Alternativas Consideradas

### 1. LLM como función (Estado → LLM → Resultado)
- **Pros**: Separación clara, mundo determinista, LLM no puede corromper estado
- **Cons**: Menos creatividad (limitada por el estado), más trabajo de prompt engineering
- **Control del mundo**: Ninguno

### 2. LLM como director (Usuario → LLM → Inventa el mundo)
- **Pros**: Máxima creatividad, menos código de simulación
- **Cons**: Mundo incoherente, sin causalidad, impredecible, no es un simulador
- **Control del mundo**: Total

### 3. LLM como asistente (Usuario → LLM → Sugiere → Sistema decide)
- **Pros**: Balance entre creatividad y coherencia
- **Cons**: Más complejo de implementar, riesgo de confusión entre sugerencia y decisión
- **Control del mundo**: Parcial

## Decisión

Usar el **LLM como función pura**: recibe estado estructurado y produce estado estructurado. El LLM nunca controla el mundo.

## Consecuencias

### Positivas
- Mundo determinista y coherente
- Separación clara entre simulación y narrativa
- Fácil de cambiar proveedor LLM
- Fácil de testear (mock del LLM)
- El LLM no puede corromper el estado del mundo

### Negativas
- Más trabajo de prompt engineering
- La creatividad del LLM está limitada por el estado
- Necesidad de construir Semantic State completo antes de cada turno
- Respuestas del LLM pueden no ser coherentes con el estado (mitigado con validación)

### Riesgos
- El LLM puede generar información que el personaje no debería saber (mitigado con validación)
- El LLM puede romper la personalidad del personaje (mitigado con prompts estrictos)
- El LLM puede ser lento para simulación en tiempo real (mitigado con fallback local)

## Follow-up
- Definir formato exacto de Semantic State en ADR-0005
- Implementar validador de respuestas del LLM
- Probar con múltiples proveedores
