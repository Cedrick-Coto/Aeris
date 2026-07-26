# ADR-0001: Uso de Arch ECS

**Estado**: Accepted  
**Fecha**: 2026-07-26  
**Decidido por**: Cedrick

---

## Contexto

El proyecto Aeris necesita un motor de simulación que maneje cientos de entidades activas con múltiples Components cada una. El motor debe ser:
- Rápido (miles de operaciones por tick)
- Flexible (nuevos Components y Systems sin reescribir el núcleo)
- Memory-friendly (data-oriented, no object-oriented)

## Alternativas Consideradas

### 1. Arch ECS
- **Pros**: Ligero, rápido, open-source, bien mantenido, soporta archetype queries
- **Cons**: Comunidad más pequeña que Entitas, menos documentación
- **Rendimiento**: ~100k entity updates/ms

### 2. Entitas
- **Pros**: Maduro, bien documentado, generador de código
- **Cons**: Más pesado, más overhead por generación, menos flexible para uso dinámico
- **Rendimiento**: ~50k entity updates/ms

### 3. Leo ECS
- **Pros**: Muy rápido, good performance
- **Cons**: Menos estable, documentación limitada
- **Rendimiento**: ~150k entity updates/ms

### 4. ECS propio
- **Pros**: Control total, sin dependencias externas
- **Cons**: Mucho trabajo de implementación, riesgo de bugs, mantenimiento propio
- **Rendimiento**: Variable, depende de la implementación

## Decisión

Usar **Arch ECS** como librería ECS.

## Consecuencias

### Positivas
- Rendimiento suficiente para cientos de entidades activas
- API limpia y data-oriented
- Fácil de aprender y mantener
- Open-source con licencia permisiva

### Negativas
- Comunidad más pequeña (menos ejemplos y tutoriales)
- Puede necesitar extensiones propias para funcionalidades avanzadas
- Dependencia de una tercera parte

### Riesgos
- Si Arch deja de mantenerse, migrar a otro ECS será más fácil que migrar de OOP a ECS
- El rendimiento puede no ser suficiente para miles de entidades (pero se optimizará después de medir, no antes)

## Follow-up
- Investigar APIs de Arch en Sprint 1
- Crear ADR-0001b si se necesitan extensiones significativas
