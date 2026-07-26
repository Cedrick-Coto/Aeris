# ADR-0003: Uso de C#

**Estado**: Accepted  
**Fecha**: 2026-07-26  
**Decidido por**: Cedrick

---

## Contexto

El motor necesita un lenguaje que soporte:
- ECS data-oriented (structs, valores, rendimiento)
- Multiplataforma (celular y computadora)
- Persistencia (SQLite, JSON)
- Comunicación con LLMs (HTTP, WebSockets)
- Desarrollo rápido pero rendimiento acceptable

## Alternativas Consideradas

### 1. C#
- **Pros**: .NET multiplataforma, structs/valores para ECS, buena integración con SQLite, soporte MAUI/Avalonia
- **Cons**: No es tan rápido como Rust/C++ para DOD, garbage collector
- **Rendimiento**: Bueno para la mayoría de casos
- **Multiplataforma**: .NET 8+ soporta Windows, Linux, macOS, Android, iOS

### 2. Rust
- **Pros**: Muy rápido, sin GC, ownership system, excelente para DOD
- **Cons**: Curva de aprendizaje alta, menos librerías para UI, más tiempo de desarrollo
- **Rendimiento**: Excelente
- **Multiplataforma**: Bueno pero con más trabajo

### 3. TypeScript/JavaScript
- **Pros**: Fácil de aprender, buen ecosistema, Web como plataforma
- **Cons**: No es DOD friendly, rendimiento limitado, no hay buenos ECS libraries
- **Rendimiento**: Bajo para simulación
- **Multiplataforma**: Excelente (Web)

### 4. Python
- **Pros**: Fácil, rápido de desarrollar
- **Cons**: Muy lento para simulación, no es DOD, no hay buenos ECS
- **Rendimiento**: Muy bajo
- **Multiplataforma**: Bueno

## Decisión

Usar **C#** con .NET 8+.

## Consecuencias

### Positivas
- .NET 8+ es multiplataforma nativo
- C# soporta structs para ECS (data-oriented)
- Buen ecosistema de librerías (SQLite, JSON, HTTP)
- MAUI y Avalonia para UI multiplataforma
- Comunidad grande y documentación abundante

### Negativas
- Garbage collector puede causar micro-stutters (mitigado con pooling y diseño)
- No es tan rápido como Rust para DOD extremo
- MAUI tiene limitaciones conocidas

### Riesgos
- Si el rendimiento no es suficiente, se puede optimizar con unsafe code o migrar partes críticas a Rust
- MAUI puede no ser la mejor opción para UI (Avalonia es alternativa)

## Follow-up
- Decidir target framework (.NET 8 o .NET 9) en Sprint 1
- Investigar Avalonia vs MAUI para UI
