---
description: 'Tool-selection guidance for MCP and workspace tools in MTM, including Serena, Context7, WinApp, terminal usage, and archive handling.'
applyTo: '**'
---

# MCP Tooling Guidance

## Preferred Tool Order

- Use workspace file, search, and error tools for local file work.
- Use Serena for symbol-level exploration and semantic navigation in C#.
- Use Context7 for current non-Microsoft library/framework documentation.
- Use Microsoft Learn MCP for Microsoft and Azure product documentation and official code samples.
- Use the VS Code MySQL Client 2 extension (`cweijan.vscode-mysql-client2`) for direct MySQL schema reads and fast read/write validation against `mtm_receiving_application_test`.
- Use WinApp only for live desktop UI inspection or automation tasks.
- Use terminal commands for build, test, archive, or bulk file operations that are impractical as
  manual patches.

## MCP Deep-Research Pattern

- For Microsoft or Azure topics:
  - Run `microsoft_docs_search` first for breadth.
  - Run `microsoft_code_sample_search` when producing Microsoft/Azure code.
  - Run `microsoft_docs_fetch` for full-page depth before final recommendations.
- For external libraries:
  - Resolve the library with Context7 `resolve-library-id`.
  - Pull targeted docs with `get-library-docs` using `mode=code` or `mode=info`.
  - Use `topic` and `page` to control depth without over-fetching.
- For local repository behavior and constraints:
  - Use Serena for symbol-level investigation and architecture-safe edits.
  - Index large projects once and then use symbol tools to avoid whole-file reads.

## High-Leverage MCP Features

- Microsoft Learn MCP:
  - Tool surface is dynamic; do not hardcode tool schemas.
  - `maxTokenBudget` can be appended to endpoint URL for tighter search payload control.
  - Search returns focused chunks; fetch returns full page markdown.
- Context7:
  - Passing an explicit Context7 library ID skips the resolve step.
  - `mode=code` and `mode=info` support different retrieval intents.
  - Use API keys for higher limits and private-doc access when available.
- Serena:
  - Single-project contexts (`ide`, `claude-code`, `grok`) intentionally restrict project switching.
  - `replace_in_files` with `dry_run=true` is the safest large-scale replacement workflow.
  - Memories should be concise, topical, and versioned with the project.

## Rules

- Keep tool usage scoped to the smallest surface that answers the question.
- Prefer targeted searches over broad repo scans.
- Do not treat `.github/archive/` as an active search target unless historical comparison is the
  task.
- Validate file moves and bulk rewrites after they run.
- Preserve SQL files under `Database/` as source of truth; extension-side SQL is for immediate, safe validation and should be reconciled back into repo scripts when finalized.
- Use non-destructive extension testing habits: explicit test schema selection and cleanup of temporary data/objects.

## Documentation Work

- When changing prompt or instruction layout, update the matching README and workspace settings.
- Use official docs before adding or changing VS Code Copilot settings.
- For quick MCP usage guidance, prefer token-friendly files:
  - `tooling/context7-mcp-token-friendly.instructions.md`
  - `tooling/microsoft-learn-mcp-token-friendly.instructions.md`
  - `tooling/serena/serena-token-friendly.instructions.md`