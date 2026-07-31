---
description: 'Token-friendly Context7 MCP usage guide for resolving libraries, pulling targeted docs, and applying advanced retrieval patterns safely.'
applyTo: '**'
---

# Context7 MCP Token-Friendly Guide

Use this file when you need fast, up-to-date external library docs with minimal token usage.

## Core Workflow

1. Resolve library ID once with `resolve-library-id`.
2. Fetch docs with `get-library-docs`.
3. Narrow by `topic` and `page` instead of broad retrieval.

## Mandatory Rule

- Call `resolve-library-id` before `get-library-docs` unless the user already provided a valid
  Context7 ID like `/org/project` or `/org/project/version`.

## High-Value Patterns

- Set `mode=code` for API references and implementation snippets.
- Set `mode=info` for architecture or concept questions.
- Provide explicit library IDs in prompts when known to skip resolution and save tokens.
- Page through docs (`page=1..n`) only when prior page is insufficient.

## Advanced Usage

- Prefer top-scoring, high-reputation sources when multiple IDs match.
- For ambiguous names, include framework, language, and runtime in the resolve query.
- Use one focused topic per call; avoid multi-topic requests in one fetch.

## Reliability and Limits

- Context7 content can evolve; re-resolve library IDs if docs look stale.
- If rate limits appear, use API-key-backed configuration in the MCP client.
- For Windows stdio setups, many clients require `cmd /c npx` wrapping.

## Security and Governance

- Do not paste secrets into prompts.
- Treat third-party docs as guidance; validate critical behavior in project code/tests.

## Use With Other MCP Servers

- Pair with Serena when mapping external API docs into local symbols.
- Pair with Microsoft Learn MCP for Microsoft/Azure-specific guidance and official samples.
