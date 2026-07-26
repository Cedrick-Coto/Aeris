# ADR-0005: Semantic State como Concepto Transversal

**Estado**: Accepted  
**Fecha**: 2026-07-26  
**Decidido por**: Cedrick

---

## Contexto

El motor necesita un mecanismo para traducir el estado determinista del mundo a un formato que el LLM pueda consumir para generar narrativa coherente. La pregunta es: **¿dónde vive este mecanismo y cómo se estructura?**

## Alternativas Consideradas

### 1. Semantic State como módulo aislado
- **Pros**: Simple, fácil de testear
- **Cons**: Acoplamiento débil con el resto del sistema, puede quedar desincronizado
- **Integración**: Baja

### 2. Semantic State como concepto transversal
- **Pros**: Todo el proyecto gira alrededor de él, coherencia garantizada, diseño central
- **Cons**: Más complejo de implementar, más acoplamiento
- **Integración**: Alta

### 3. Sin Semantic State (enviar Components directos al LLM)
- **Pros**: Sin código adicional
- **Cons**: LLM recibe demasiada información, tokens desperdiciados, información irrelevante, el LLM no debería ver todo
- **Integración**: N/A

## Decisión

Hacer del **Semantic State un concepto transversal**: todo el proyecto gira alrededor de él. El Semantic State es el traductor entre un simulador determinista y un modelo de lenguaje probabilístico.

## Consecuencias

### Positivas
- El Semantic State es el producto principal de la fase de presentación
- El Semantic State es la entrada principal del LLM
- Todo el diseño del motor se orienta a producir un buen Semantic State
- Facilita el debugging (se puede inspeccionar qué ve el LLM)

### Negativas
- Más complejidad en el diseño
- Necesidad de mantener coherencia entre el Semantic State y el estado real del mundo
- Más trabajo de mantenimiento a largo plazo

### Riesgos
- El Semantic State puede ser demasiado grande para el contexto del LLM (mitigado con filtrado agresivo)
- El Semantic State puede omitir información importante (mitigado con filtros configurables)
- El builder puede ser lento (mitigado con caching y optimización)

## Follow-up
- Definir estructura exacta del Semantic State en `05-semantic-state.md`
- Implementar SemanticExtractor en Sprint 1
- Crear tests de coherencia (¿el Semantic State refleja fielmente el estado del mundo?)
