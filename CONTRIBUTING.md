# Contributing to Aeris

Aeris is a research-oriented open source project. We welcome contributions that align with its architectural principles.

## Before you start

Read these documents to understand the project's philosophy:

- `docs/00-overview.md` — Project overview
- `docs/14-development-roadmap.md` — Current sprint and roadmap
- `docs/adr/README.md` — Architecture Decision Records hierarchy
- `docs/16-agent-architecture.md` — Agent architecture and principles

## How to contribute

### 1. Report bugs

Open a [bug report](.github/ISSUE_TEMPLATE/bug_report.md). Include:

- Steps to reproduce
- Expected vs actual behavior
- Seed if the issue involves determinism

### 2. Propose features

Open a [feature request](.github/ISSUE_TEMPLATE/feature_request.md). Explain:

- What capability you want to add
- How it fits the architecture
- Why it cannot emerge from existing subsystems

### 3. Propose research hypotheses

Open a [research hypothesis](.github/ISSUE_TEMPLATE/research_hypothesis.md). Include:

- Clear, falsifiable statement
- Proposed experiment with metrics
- Success/failure criteria

### 4. Submit code

1. Fork the repository
2. Create a branch: `git checkout -b feature/your-feature`
3. Follow the architectural principles (see below)
4. Write tests
5. Run `dotnet build` and `dotnet test` (0 errors, 0 warnings)
6. Submit a pull request

## Architectural principles (must follow)

These are **non-negotiable**. PRs that violate them will be rejected.

| Principle | Rule |
|-----------|------|
| Determinism | Same seed + same input → same output |
| Causal pressure | Every behavior must trace to an internal state; no hardcoded reactions |
| Traceability | Every state transition must be explainable |
| Computational contract | Every concept must have formal inputs, processing, outputs, and invariants |
| Causal locality | A subsystem only modifies the state it declares |
| Affective modulation | Affect modulates weights/thresholds, never selects responses directly |

## Code style

- C# with nullable enabled
- No comments unless the code cannot express intent clearly
- Follow existing patterns in `src/`
- Zero allocations per tick (reuse buffers, avoid boxing)

## License

By contributing, you agree that your contributions will be licensed under GPL-3.0.
