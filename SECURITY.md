# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| main    | ✅ Active development |

## Reporting a Vulnerability

Aeris is a simulation engine, not a web service. Security vulnerabilities are unlikely but possible (e.g., malicious world data, prompt injection through the LLM adapter).

If you find a security issue:

1. **Do not** open a public issue
2. Email the maintainers or open a draft security advisory on GitHub
3. Describe the vulnerability, how to reproduce it, and its potential impact

We will acknowledge receipt within 48 hours and work on a fix.

## Scope

The following are in scope:

- The simulation engine (`src/Aeris.Engine/`)
- The build pipeline and CI/CD
- The LLM adapter interfaces

The following are out of scope:

- The LLM providers themselves (OpenAI, Claude, Ollama)
- The .NET runtime or NuGet packages
