<!-- 
[DOC-META-START]
- File Name: README.md
- Description: Index of the .github/instructions library: categories, recommended starting files, maintenance rules.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 15-18: # Instruction Library
  - Line 19-30: ## Categories
  - Line 31-42: ## Recommended Starting Files
  - Line 43-47: ## Maintenance Rules
- Critical Notes: Use category folders for new files; referenced instruction paths must exist.
[DOC-META-END]
-->

# Instruction Library

Instruction files are grouped by topic so agents can load only the guidance that matches the task.

## Categories

- `architecture/` — MVVM, DAO, CQRS, dialogs, settings, converters
- `copilotforms/` — CopilotForms export interpretation rules
- `database/` — stored procedures, Infor Visual query authoring, schema guidance
- `documentation/` — markdown, spec slices, and doc maintenance
- `languages/` — C#, PowerShell, Python, shell
- `quality/` — review, performance, security, and readability
- `testing/` — test strategy and validation expectations
- `tooling/` — MCP, WinApp, Serena, and supporting tools
- `workflow/` — prompt engineering, research, process, and customization authoring

## Recommended Starting Files

- `languages/csharp.instructions.md`
- `architecture/mvvm-pattern.instructions.md`
- `architecture/dao-pattern.instructions.md`
- `database/sql-sp-generation.instructions.md`
- `testing/testing-strategy.instructions.md`
- `tooling/mcp-tooling.instructions.md`
- `tooling/microsoft-learn-mcp-token-friendly.instructions.md`
- `tooling/context7-mcp-token-friendly.instructions.md`
- `tooling/serena/serena-token-friendly.instructions.md`

## Maintenance Rules

- Keep instruction files focused on one topic.
- Use category folders for new files instead of adding new flat files.
- If a prompt or document references an instruction path, that path must exist.