# Research Notes

Este directorio contiene notas de literatura científica relevante para el diseño del agente. No son decisiones arquitectónicas (ADR) ni hipótesis pendientes de validar (Hypotheses). Son **resúmenes de evidencia externa** que informan el diseño sin comprometerlo.

---

## Diferencia entre ADR, Hipótesis y Research Note

| | ADR | Hipótesis | Research Note |
|---|---|---|---|
| **Propósito** | Documentar una decisión vigente | Documentar una creencia a validar | Resumir literatura relevante |
| **Contenido** | Decisión + alternativas + consecuencias | Enunciado + experimento + resultado | Resumen + fuentes + impacto potencial |
| **Estado típico** | Accepted | Proposed → Validated / Rejected | Reviewing → Referenced / Superseded |
| **Implica código** | Sí (decisiones activas) | Sí (si se acepta) | No (solo fundamento) |
| **Cambia con el tiempo** | No (se crea nuevo ADR) | Sí (se actualiza con resultado) | Sí (si aparece nueva evidencia) |

---

## Ciclo de vida

```
Lectura de literatura
    ↓
Resumen inicial (Reviewing)
    ↓
Evaluación de relevancia
    ├── Referenced → se cita en ADRs, docs de diseño o hipótesis
    └── Superseded → nueva evidencia reemplaza a esta nota
```

---

## Formato

```markdown
# RN-0001: Título del tema

**Estado**: Reviewing | Referenced | Superseded
**Última actualización**: YYYY-MM-DD

## Tema

Descripción del área o mecanismo cognitivo cubierto.

## Resumen de la literatura

Síntesis de los hallazgos principales, teorías y experimentos relevantes.

## Impacto potencial

Qué subsistemas del agente podrían verse afectados por esta literatura.

## Estado de decisión

Si esta nota ha influido en alguna decisión arquitectónica, se referencia aquí.
```

---

## RNs activos

| ID | Tema | Estado | Impacto | Hipótesis relacionada |
|----|------|--------|---------|----------------------|
| RN-0001 | Working Memory (Baddeley) | Referenced | WM, Attention | H-0001 |
| RN-0002 | Modelos de Afecto | Referenced | Affect, todos los subsistemas | H-0002, H-0005 |
| RN-0003 | Identidad y Reconstrucción del Self | Reviewing | Identity, SemanticExtractor | H-0003 |
| RN-0004 | Modelos de Atención | Referenced | Attention, WM | H-0002 |
| RN-0005 | Modelos Internos del Mundo | Reviewing | WorldModel, Planning, Reasoning | H-0004 |
| RN-0006 | Planificación en Agentes Cognitivos | Reviewing | Planning, Decision, WorldModel | H-0004, H-0006 |
