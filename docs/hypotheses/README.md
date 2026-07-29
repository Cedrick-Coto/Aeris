# Hipótesis de Investigación

Este directorio contiene hipótesis de investigación del proyecto Aeris.

---

## Diferencia entre ADR e Hipótesis

| | ADR | Hipótesis |
|---|---|---|
| **Propósito** | Documentar una decisión vigente | Documentar una creencia a validar |
| **Estado típico** | Accepted (vigente) | Proposed → Validated / Rejected |
| **Reversibilidad** | Baja (requiere nuevo ADR) | Alta (se espera experimentación) |
| **Impacto** | Afecta la arquitectura | Afecta la implementación |
| **Estabilidad** | Permanente hasta nuevo ADR | Temporal hasta validación |

Las hipótesis permiten explorar diseños sin comprometer la arquitectura. Son especialmente útiles en los sprints de investigación (Sprint 3 en adelante), donde muchas decisiones son experimentales.

---

## Ciclo de vida

```
Propuesta
    ↓
Diseño del experimento
    ↓
Implementación
    ↓
Validación (métricas observables)
    ├── Validada → puede convertirse en ADR
    └── Rechazada → se documenta el resultado y se archiva
```

---

## Formato

Cada hipótesis sigue esta estructura:

```markdown
# H-0001: Título de la hipótesis

**Estado**: Proposed | Validated | Rejected
**Fecha**: YYYY-MM-DD
**Autor**: Nombre

## Hipótesis

Enunciado claro de lo que se cree que sucederá.

## Fundamento

Por qué se cree que esto puede funcionar (literatura, intuición, experimentos previos).

## Experimento

Cómo se va a validar o refutar:
- Condiciones del experimento
- Métricas observables
- Criterio de éxito/fracaso

## Resultado

(Se completa después de la validación)
- Datos recogidos
- Conclusión: Validada / Rechazada
- Implicaciones para la arquitectura
```
