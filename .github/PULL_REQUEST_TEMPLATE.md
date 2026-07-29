## Descripción

¿Qué cambia este PR?

## Tipo de cambio

- [ ] Bug fix
- [ ] Nueva funcionalidad
- [ ] Cambio arquitectónico (requiere ADR)
- [ ] Refactor
- [ ] Documentación
- [ ] Tests

## Pruebas

- [ ] Todos los tests existentes pasan (`dotnet test`)
- [ ] Se añadieron tests nuevos cuando correspondía
- [ ] El build produce 0 errores y 0 warnings

## Determinismo

- [ ] El cambio no introduce aleatoriedad no controlada
- [ ] El cambio mantiene la reproducibilidad con misma seed

## Trazabilidad

- [ ] Las decisiones de implementación están documentadas en el código o en ADRs
- [ ] Si aplica, se actualizó la documentación afectada

## Checklist

- [ ] El código sigue los principios de `docs/16-agent-architecture.md`
- [ ] No se introdujo lógica del tipo `if (emotion == X)`
- [ ] Todo subsistema nuevo declara inputs, outputs e invariantes
