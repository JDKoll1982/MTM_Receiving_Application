---
description: 'Token-friendly Serena MCP quick guide for symbol-first exploration, safe edits, context selection, and high-efficiency workflow.'
applyTo: '**'
---

<!-- 
[DOC-META-START]
- File Name: serena-token-friendly.instructions.md
- Description: Token-friendly Serena MCP quick guide for symbol-first exploration and safe edits.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 23-26: # Serena Token-Friendly Guide
  - Line 27-33: ## Core Workflow
  - Line 34-39: ## Mandatory Rules
  - Line 40-45: ## High-Value Features
  - Line 46-51: ## Advanced Usage
  - Line 52-57: ## Practical Guardrails
  - Line 58-62: ## Use With Other MCP Servers
- Critical Notes: Prefer symbol tools over whole-file reads for C# work.
[DOC-META-END]
-->

# Serena Token-Friendly Guide

Use this file when you need high-precision local code analysis and editing with low token cost.

## Core Workflow

1. Get symbol map first (`get_symbols_overview` or symbol search).
2. Read only target symbols (`find_symbol` with `include_body=true` only when needed).
3. Edit at symbol granularity (`replace_symbol_body`, insert before/after symbol).
4. Use file regex replacements only for small intra-symbol edits.

## Mandatory Rules

- Do not read whole files unless strictly necessary.
- Run impact checks before signature changes (`find_referencing_symbols`).
- Prefer Serena symbol tools over manual broad search for C# implementation tasks.

## High-Value Features

- Index once on large projects for fast symbol retrieval (`serena project index`).
- Use single-project contexts (`ide`, `claude-code`) for focused toolsets.
- Use `replace_in_files` with `dry_run=true` before bulk replacements.

## Advanced Usage

- Compose modes per task: `planning` for analysis, `editing` for implementation.
- Store durable project guidance in memories and keep them short and topic-specific.
- Use context-specific startup (`--context ide --project ${workspaceFolder}`) in VS Code.

## Practical Guardrails

- If a change spans multiple symbols, update callers in the same pass.
- Keep edits minimal and architecture-safe (MVVM boundaries in this repo).
- Validate targeted slices after edits, not the entire solution by default.

## Use With Other MCP Servers

- Use Microsoft Learn MCP for Microsoft/Azure authoritative docs.
- Use Context7 for third-party library API docs.
- Then apply changes locally with Serena symbol tools.
