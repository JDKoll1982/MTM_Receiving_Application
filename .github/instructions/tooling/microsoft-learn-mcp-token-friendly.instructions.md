---
description: 'Token-friendly Microsoft Learn MCP guide covering search-fetch-sample workflow, dynamic tools, and advanced endpoint usage.'
applyTo: '**'
---

<!-- 
[DOC-META-START]
- File Name: microsoft-learn-mcp-token-friendly.instructions.md
- Description: Token-friendly Microsoft Learn MCP guide for search-fetch-sample workflow.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 24-27: # Microsoft Learn MCP Token-Friendly Guide
  - Line 28-33: ## Core Workflow
  - Line 34-39: ## Mandatory Rules
  - Line 40-45: ## High-Value Features
  - Line 46-51: ## Advanced Usage
  - Line 52-57: ## Release-Aware Notes
  - Line 58-63: ## Security and Reliability
  - Line 64-67: ## Use With Other MCP Servers
- Critical Notes: Treat Learn MCP as a dynamic surface; do not hardcode tool schemas.
[DOC-META-END]
-->

# Microsoft Learn MCP Token-Friendly Guide

Use this file for Microsoft and Azure documentation retrieval grounded in official Learn sources.

## Core Workflow

1. Run `microsoft_docs_search` for broad discovery.
2. Run `microsoft_code_sample_search` when generating Microsoft/Azure code.
3. Run `microsoft_docs_fetch` for complete page details before final decisions.

## Mandatory Rules

- Treat Learn MCP as a dynamic MCP surface; do not hardcode tool schemas.
- Prefer the search-then-fetch pattern for depth and accuracy.
- Use code sample search whenever output includes Microsoft/Azure code snippets.

## High-Value Features

- Endpoint: `https://learn.microsoft.com/api/mcp` (streamable HTTP).
- Token control: append `?maxTokenBudget=...` to reduce search payload size.
- Search returns concise chunks; fetch returns full markdown page content.

## Advanced Usage

- Refresh tool metadata (`tools/list`) on stale 400/404 behavior.
- Use scoped queries (`"WinUI 3 x:Bind"`, `"Azure Functions isolated worker"`) to reduce noise.
- Prefer fetching high-value pages from search results rather than broad repeated searches.

## Release-Aware Notes

- Current server capabilities include docs search, docs fetch, and code sample search.
- OpenAI-compatible endpoint and Learn CLI support exist in release notes; use when your client
  architecture benefits from those integration styles.

## Security and Reliability

- Do not send secrets in prompts.
- Validate generated guidance against your project constraints and pinned runtime versions.
- If links redirect, re-fetch the destination page because Learn content updates frequently.

## Use With Other MCP Servers

- Pair with Serena for local-code impact mapping and safe symbol-level edits.
- Pair with Context7 for non-Microsoft dependencies referenced by Microsoft docs.
