---
applyTo: "**"
description: >
  Serena overview — what it is, key benefits for the MTM project,
  when to use it vs standard tools, and the quick-start command.
---

# Serena Overview

**Serena** is a free, open-source coding agent toolkit that provides IDE-like semantic code
retrieval and editing tools via the Model Context Protocol (MCP). It uses language servers
(LSP) or the Serena JetBrains Plugin to understand code at the **symbol level** — finding,
reading, and editing classes, methods, and properties without reading entire files.

- GitHub: <https://github.com/oraios/serena>
- Official docs: <https://oraios.github.io/serena/>

---

## Key Benefits for the MTM Project

The MTM Receiving Application has 300+ C# files across 10+ modules. Serena addresses this directly:

| Capability                       | Savings     | MTM Example                                                  |
| -------------------------------- | ----------- | ------------------------------------------------------------ |
| `get_symbols_overview` on a file | ~95% tokens | See all methods in a 400-line DAO                            |
| `find_symbol` with body          | ~90% tokens | Read one method from a large class                           |
| `find_referencing_symbols`       | ~85% tokens | Check impact before changing a Service signature             |
| `replace_symbol_body`            | ~80% tokens | Update DAO implementation without touching the file manually |
| `search_for_pattern`             | ~70% tokens | Find all `MessageBox.Show` calls outside Views               |

---

## When to Use Serena vs Standard Tools

```
Task involves code exploration or editing?
├─ YES: Affects 3+ files OR finding usages OR symbol-level precision needed?
│  ├─ YES → ✅ Use Serena (80-90% token savings)
│  └─ NO  → Single-line edit in one known file?
│           ├─ YES → Use standard replace_string_in_file
│           └─ NO  → ✅ Use Serena (precision matters)
└─ NO: Reading config files, non-code files, binary files?
      └─ Use standard read_file / grep_search tools
```

### Use Serena For

- Exploring a new DAO, Service, or ViewModel implementation
- Finding all callers of a method before changing its signature
- Refactoring across 3 or more files
- Validating MVVM architecture (View → ViewModel → Service → DAO)
- Searching for anti-patterns: `MessageBox.Show` outside Views, direct SQL, static DAOs
- Multi-file symbol rename or replace operations
- Understanding large class structure without full-file reads

### Do NOT Use Serena For

- Single-line edits in one known file
- Creating new solution/project files
- Reading JSON, YAML, XML, `.config`, `.csproj` files
- Binary files or compiled output

---

## Quick Start (MTM Project)

### Pre-requisite

Install `uv` (Serena's package manager):

```powershell
winget install astral-sh.uv
# or
powershell -c "irm https://astral.sh/uv/install.ps1 | iex"
```

### Launch MCP Server via uvx (Recommended)

```bash
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --help
```

### VSCode Integration (MTM .vscode/mcp.json)

Paste this into `<project>/.vscode/mcp.json`:

```json
{
  "servers": {
    "oraios/serena": {
      "type": "stdio",
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena",
        "start-mcp-server",
        "--context",
        "ide",
        "--project",
        "${workspaceFolder}"
      ]
    }
  },
  "inputs": []
}
```

### First-Time Project Setup

```bash
cd C:\Users\johnk\source\repos\MTM_Receiving_Application

# Create Serena project with C# language and immediately index
uvx --from git+https://github.com/oraios/serena serena project create --language csharp --name "MTM_Receiving_Application" --index
```

This creates `.serena/project.yml` and pre-caches symbol information for faster tool execution.

---

## How Serena Works

Serena provides tools to an LLM/AI agent via MCP. The AI orchestrates tool use:

1. **Language Server (LSP)** — for C#, Serena uses the **Roslyn Language Server**
   (auto-downloaded; requires .NET 10+ or auto-installs it)
2. **Symbol Index** — Serena pre-caches symbol information from the language server
   so tools respond in under 1 second (vs 5-10 seconds without indexing)
3. **Memories** — Serena stores project knowledge in `.serena/memories/` Markdown files
   that persist across sessions

---

## MTM C# Language Server Notes

Serena uses **Microsoft's Roslyn Language Server** for C# (default since replacing OmniSharp).

- Requires: .NET 10+ (auto-installs if absent)
- On Windows: requires `pwsh` (PowerShell 7+) — already present in MTM environment
- Set language to `csharp_omnisharp` in `project.yml` to use OmniSharp instead

```yaml
# .serena/project.yml
languages:
  - csharp
```

---

## Serena File Structure in MTM Project

After setup, the following files exist in the project root:

```
.serena/
  project.yml          ← Project configuration (commit this)
  project.local.yml    ← Local overrides (gitignored)
  memories/            ← Project knowledge (commit these)
    architectural_patterns.md
    coding_standards.md
    ...
  cache/               ← Symbol index (gitignored, rebuild with 'project index')
```

---

## Instruction File Index

This file is part of a set. See the TOC in
`.github/instructions/serena-tools.instructions.md` for the full index.

| File                                                | Topic                                                |
| --------------------------------------------------- | ---------------------------------------------------- |
| `serena-01-overview.instructions.md`                | **This file** — overview, when to use, quick start   |
| `serena-02-tools-reference.instructions.md`         | Full tool catalogue with MTM examples                |
| `serena-03-language-support.instructions.md`        | C#/Roslyn, JetBrains plugin, all supported languages |
| `serena-04-running.instructions.md`                 | uvx, Docker, CLI commands                            |
| `serena-05-clients.instructions.md`                 | VSCode, Claude Code, Claude Desktop configs          |
| `serena-06-workflow.instructions.md`                | Project creation, activation, indexing               |
| `serena-07-memories.instructions.md`                | Memory system and onboarding                         |
| `serena-08-configuration.instructions.md`           | Contexts, modes, global/project config               |
| `serena-09-dashboard-logs-security.instructions.md` | Dashboard, logs, security                            |
| `serena-10-advanced-usage.instructions.md`          | Prompting strategies, custom agents, MTM workflows   |
