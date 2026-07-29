# ADR-0007: Target .NET 10

**Estado**: Accepted  
**Fecha**: 2026-07-28  
**Decidido por**: Cedrick

---

## Contexto

El proyecto Aeris comenzó con .NET 8 LTS como framework base. Con la evolución de la plataforma .NET, el entorno de desarrollo disponible pasó a tener instalado el SDK 10.0.302, mientras que el runtime .NET 8.0 x64 no estaba presente. Esto obligaba a usar `RollForward` para ejecutar tests o requería instalar un runtime adicional.

La pregunta es: **¿qué versión de .NET debe usar el proyecto de forma estable?**

Esta decisión afecta:
- La reproducibilidad del entorno de desarrollo
- La infraestructura de CI/CD futura
- El acceso a mejoras del runtime y el JIT
- La compatibilidad con dependencias (Arch ECS, paquetes NuGet)

## Alternativas Consideradas

### 1. Mantener .NET 8 LTS

```xml
<TargetFramework>net8.0</TargetFramework>
```

**Pros**:
- Versión LTS consolidada (soporte hasta noviembre de 2026)
- Máxima compatibilidad con el ecosistema NuGet del momento
- Estabilidad probada en producción

**Cons**:
- No hay runtime x64 instalado localmente → requiere `RollForward` o instalación adicional
- `RollForward` introduce diferencias potenciales entre entornos
- Sin acceso a mejoras recientes del JIT (tiered PGO, AVX-512, etc.)

### 2. Usar .NET 9

```xml
<TargetFramework>net9.0</TargetFramework>
```

**Pros**:
- Versión intermedia con mejoras incrementales

**Cons**:
- STS (Standard Term Support), no LTS
- Tampoco está instalado en el entorno actual
- Ciclo de vida más corto

### 3. Usar .NET 10 (decidido)

```xml
<TargetFramework>net10.0</TargetFramework>
```

**Pros**:
- SDK y runtime instalados localmente (10.0.302 / 10.0.10)
- Sin necesidad de `RollForward`
- Entorno reproducible: todos los desarrolladores usan la misma versión TFM
- Últimas mejoras del runtime: JIT, GC, bibliotecas base
- Alineado con la versión LTS par (Microsoft designa LTS a las versiones pares: 6, 8, 10)
- C# 13/14 disponible según el SDK

**Cons**:
- Requiere que los desarrolladores tengan instalado .NET 10
- Menor tiempo en producción que .NET 8

## Decisión

**Adoptar `net10.0` como TFM único del proyecto.**

Todos los proyectos (engine, tests, benchmarks) heredan el TFM desde `Directory.Build.props`, eliminando la declaración redundante en cada `.csproj`.

No se utiliza `RollForward` para preservar la reproducibilidad entre entornos.

## Consecuencias

### Positivas
- El SDK y runtime .NET 10 están disponibles localmente → build y tests funcionan directamente
- Sin `RollForward` → el comportamiento es idéntico en cualquier máquina con .NET 10
- TFM centralizado en `Directory.Build.props` → una sola fuente de verdad
- Acceso a mejoras del JIT y runtime para el motor de simulación (crítico para rendimiento determinista)
- Actualización futura más simple: cambiar una línea en `Directory.Build.props`

### Negativas
- Quienes clonen el repositorio necesitarán instalar .NET 10 SDK
- Proyectos que referencien librerías antiguas solo para `net8.0` podrían requerir compatibilidad hacia adelante (funciona por diseño en .NET)

### Riesgos
- Dependencias NuGet que no tengan versiones para `net10.0` (mitigado: el sistema de targets de .NET permite usar librerías `net8.0` desde `net10.0` sin problema)
- Adopción temprana de .NET 10 si surgieran bugs de runtime (mitigado: .NET 10.0.10 es una versión madura dentro del ciclo de release)

## Follow-up
- Confirmar que todos los paquetes NuGet (Arch, xUnit, FsCheck, FluentAssertions, BenchmarkDotNet) funcionan correctamente — verificado con build y 31 tests pasando.
- Mantener `Directory.Build.props` como única fuente del TFM.
- Para futuras versiones LTS (net12.0, etc.), cambiar únicamente ese archivo.
