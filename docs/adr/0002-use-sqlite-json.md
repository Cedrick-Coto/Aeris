# ADR-0002: Persistencia con SQLite + JSON

**Estado**: Accepted  
**Fecha**: 2026-07-26  
**Decidido por**: Cedrick

---

## Contexto

El motor necesita persistir:
1. Estado del mundo (Entitys, Components, Resources) — cambia frecuentemente, necesita consultas
2. Worldbuilding (regiones, especies, items) — cambia poco, necesita ser editable manualmente
3. Configuración — cambia muy poco, necesita ser legible

## Alternativas Consideradas

### 1. SQLite + JSON
- **Pros**: SQLite para estado (rápido, transaccional, consultas SQL), JSON para worldbuilding (flexible, legible, editable)
- **Cons**: Dos sistemas de persistencia, sincronización necesaria
- **Complejidad**: Media

### 2. Solo SQLite
- **Pros**: Un solo sistema, consistente
- **Cons**: JSON es mejor para worldbuilding (edición manual, legibilidad)
- **Complejidad**: Baja

### 3. Solo JSON
- **Pros**: Simple, legible
- **Cons**: Lento para consultas, sin transaccionalidad, difícil de mantener integridad
- **Complejidad**: Baja

### 4. PostgreSQL
- **Pros**: Muy potente, escalable
- **Cons**: Overkill para una aplicación local, requiere servidor
- **Complejidad**: Alta

## Decisión

Usar **SQLite** para estado persistente y **JSON** para worldbuilding y configuración.

## Consecuencias

### Positivas
- SQLite: consultas rápidas, transaccionalidad, integridad referencial
- JSON: legibilidad, edición manual, facilidad de debugging
- Separación clara entre datos dinámicos y estáticos

### Negativas
- Dos sistemas de persistencia que mantener
- Necesidad de sincronizar entre ambos (ej: worldbuilding cargado en Entitys)
- Más complejidad en el código de persistencia

### Riesgos
- Si el worldbuilding crece mucho, JSON puede volverse lento (pero es poco probable)
- La sincronización entre SQLite y JSON puede causar inconsistencias (mitigado con diseño claro)

## Follow-up
- Definir esquema SQLite en Sprint 1
- Definir estructura de archivos JSON en Sprint 1
