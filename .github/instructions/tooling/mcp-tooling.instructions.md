---
description: 'Tool-selection guidance for MCP and workspace tools in MTM, including Serena, Context7, WinApp, terminal usage, and archive handling.'
applyTo: '**'
---

# MCP Tooling Guidance

## Preferred Tool Order

- Use workspace file, search, and error tools for local file work.
- Use Serena for symbol-level exploration and semantic navigation in C#.
- Use Context7 for current documentation about libraries, frameworks, SDKs, and editor settings.
- Use WinApp only for live desktop UI inspection or automation tasks.
- Use terminal commands for build, test, archive, or bulk file operations that are impractical as
  manual patches.

## Rules

- Keep tool usage scoped to the smallest surface that answers the question.
- Prefer targeted searches over broad repo scans.
- Do not treat `.github/archive/` as an active search target unless historical comparison is the
  task.
- Validate file moves and bulk rewrites after they run.

## Documentation Work

- When changing prompt or instruction layout, update the matching README and workspace settings.
- Use official docs before adding or changing VS Code Copilot settings.