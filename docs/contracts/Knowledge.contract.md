# CONTRACT-KNOWLEDGE

**Estado**: Draft
**Última actualización**: 2026-08-05
**Versión**: 0.1

---

# 1. Propósito del documento

Definir el contrato del subsistema de conocimiento declarado: cómo las regularidades observadas se transforman en conocimiento con afirmación, alcance, evidencia fuente, confianza epistemológica y revisión temporal, sin que una observación se promueva automáticamente a conocimiento.

Este contrato cubre exclusivamente **KNOWLEDGE_BASE**: el almacenamiento de patrones y generalizaciones. No cubre la transformación de ese conocimiento en criterios, pesos o decisiones, que pertenece a **DECISION_POLICY**.

Cualquier modelo de aprendizaje futuro debe satisfacer este contrato para ser intercambiable.

## No define

Este contrato no define:

- verdad ontológica del conocimiento;
- recomendaciones normativas o de acción;
- probabilidad exacta del mundo (salvo que exista un modelo explícito que la derive);
- procesos internos privados del mecanismo de extracción.

Una entrada de conocimiento representa una generalización derivada bajo un modelo determinado, no un hecho verdadero del mundo.

---

# 2. Modelo de conocimiento

## 2.1 Pipeline de conocimiento

```
Dato observado
    ↓
Evidencia estructurada
    ↓
Patrón identificado
    ↓
Conocimiento candidato
    ↓
Conocimiento aceptado
```

### Dato observado

Registro crudo de percepción o memoria. Incluye referencia temporal y de fuente. No es conocimiento.

### Evidencia estructurada

Dato normalizado con descripción, instante y fuente, usable como soporte de una regularidad.

### Patrón identificado

Regularidad detectada sobre un conjunto de evidencias estructuradas. Aún no es una afirmación declarada.

### Conocimiento candidato

Patrón formulado como afirmación con alcance y confianza preliminar, pendiente de evaluación contra los criterios de generalización.

### Conocimiento aceptado

Candidato que satisface los criterios de generalización y se registra en KNOWLEDGE_BASE.

Regla **K-003**: una regularidad observada nunca se promueve automáticamente a conocimiento aceptado. Cada transición es una transformación registrada y evaluada.

## 2.2 Conocimiento declarado

Todo conocimiento en KNOWLEDGE_BASE declara obligatoriamente:

- **Afirmación**: proposición general en forma declarativa.
- **Alcance**: dominio de aplicación explícito (entidades, contexto, condiciones).
- **Evidencia fuente**: referencias a las evidencias estructuradas que lo soportan.
- **Confianza**: propiedad epistemológica compuesta.
- **Revisión temporal**: instante de la última revisión.

Una entrada sin estos cinco campos no es conocimiento válido.

## 2.3 Confianza como propiedad epistemológica

La confianza es una propiedad epistemológica compuesta por:

- **Soporte** — cantidad y solidez de la evidencia fuente;
- **Consistencia** — coherencia con el conocimiento aceptado vigente;
- **Alcance** — delimitación explícita del dominio de aplicación.

La confianza no es una probabilidad exacta del mundo, salvo que exista un modelo explícito que la derive a partir de componentes registradas.

La confianza es propiedad de la entrada de conocimiento, no del proceso que la generó.

## 2.4 KNOWLEDGE_BASE y DECISION_POLICY

| | KNOWLEDGE_BASE | DECISION_POLICY |
|---|---|---|
| Almacena | Patrones y generalizaciones | Criterios, pesos o decisiones |
| Naturaleza | Descriptivo: qué se sostiene | Normativo: qué se hace |
| Escribe | Subsistema de conocimiento | — |
| Lee | — | Conocimiento de KNOWLEDGE_BASE |

Regla **K-005**: KNOWLEDGE_BASE no almacena recomendaciones normativas. Una entrada de conocimiento no implica ejecución ni preferencia de acción.

---

# 3. Extracción de conocimiento

## 3.1 Transiciones registradas

Cada transición del pipeline (§2.1) se registra con su regla de extracción (`RuleId`) y las referencias de origen, de modo que toda entrada de KNOWLEDGE_BASE es reconstruible desde sus datos observados.

## 3.2 Criterios de generalización

Un conocimiento candidato solo se acepta cuando satisface todos los criterios:

- **Evidencia trazable** — toda la evidencia fuente es localizable y verificable.
- **Patrón identificado** — existe una regularidad formalizada sobre evidencias.
- **Alcance declarado** — el dominio de aplicación es explícito.
- **Capacidad de transferencia** — la afirmación se sostiene más allá del contexto inmediato de su evidencia.
- **Ausencia de contradicciones relevantes sin resolver** — no existe conocimiento vigente que contradiga la afirmación sin una resolución registrada.

El resultado de la evaluación se registra como parte de la transición a Aceptado.

## 3.3 No hay conocimiento sin evidencia

Un candidato sin evidencia fuente no puede alcanzar la etapa de conocimiento aceptado.

---

# 4. Registro de conocimiento

## 4.1 Entrada de KNOWLEDGE_BASE

```csharp
KnowledgeEntry
{
    string          Id;              // identificador único
    string          Claim;           // afirmación
    string          Scope;           // alcance
    EvidenceRef[]   SourceEvidence;  // evidencia fuente
    Confidence      Confidence;      // { Soporte, Consistencia, Alcance }
    float           LastReviewedAt;  // revisión temporal
    KnowledgeState  State;           // Aceptado | Refutado | Limitado | Deprecado
    string          RuleId;          // regla de extracción
}
```

## 4.2 Reglas de registro

- **Determinismo**: mismo flujo de extracción y misma entrada producen el mismo registro.
- **No duplicación**: una misma afirmación con el mismo alcance y la misma evidencia fuente no se registra dos veces.
- **Inmutabilidad histórica**: un registro no se edita; evoluciona mediante cambios de estado (§6).
- **Escritura exclusiva**: solo el subsistema de conocimiento escribe en KNOWLEDGE_BASE.

---

# 5. Integración con consumidores

## 5.1 Consumidores

- **DECISION_POLICY** — transforma conocimiento en criterios, pesos o decisiones.
- **Reasoning** — usa conocimiento como patrones y reglas disponibles.
- **Planning** — usa conocimiento como restricciones de dominio.

*Nota*: el conocimiento aceptado puede servir como fuente para formular nuevas hipótesis, sin constituir una dependencia obligatoria del ciclo experimental.

## 5.2 Responsabilidades de acceso

- Los consumidores solo leen conocimiento en estado vigente (Aceptado o Limitado).
- Ningún consumidor escribe en KNOWLEDGE_BASE.
- Ningún consumidor promueve candidatos a aceptados.
- Ningún consumidor introduce recomendaciones normativas en KNOWLEDGE_BASE.

---

# 6. Evolución y revisión del conocimiento

## 6.1 Estados de evolución

- **Aceptado** — vigente; usable como soporte.
- **Refutado** — contradicho por evidencia; deja de usarse como soporte.
- **Limitado** — alcance reducido tras nueva evidencia; vigente solo dentro del alcance restringido.
- **Deprecado** — reemplazado por otra entrada; se conserva para historia.

El estado inicial de un conocimiento aceptado es Aceptado.

## 6.2 Revisión temporal

Toda transición de estado registra:

- instante de la transición;
- causa (regla o evidencia que la motiva);
- entrada afectada.

## 6.3 Invariantes

- **K-001 — Determinismo**: misma entrada produce el mismo conocimiento.
- **K-002 — Trazabilidad completa**: toda entrada conduce a evidencia fuente.
- **K-003 — No promoción automática**: una regularidad observada no pasa a conocimiento sin evaluación registrada.
- **K-004 — Confianza epistemológica**: soporte, consistencia y alcance; no probabilidad salvo modelo explícito.
- **K-005 — Separación KNOWLEDGE_BASE / DECISION_POLICY**: sin recomendaciones normativas en KNOWLEDGE_BASE.
- **K-006 — Evolución registrada**: todo cambio de estado es trazable.

---

# Dependencies

- CONTRACT-PERCEPTION — datos observados de origen
- CONTRACT-MR — evidencias recuperadas desde memoria
- CONTRACT-REASONING — patrones y reglas derivadas

---

# History

| Version | Date       | Change                                                                                   |
| ------- | ---------- | ---------------------------------------------------------------------------------------- |
| 0.1     | 2026-08-05 | Draft inicial: pipeline, confianza epistemológica, registro, frontera KB/DP, estados de evolución |
