---
applyTo: "**"
description: >
  Serena semantic coding tools index for MTM Receiving Application — start here.
  Links to all ten detail files covering installation, tools, workflow, memories, and configuration.
---

# Serena Semantic Coding Tools — Index

**Serena** is an IDE-like MCP server that provides symbol-level code navigation and editing
backed by a real language server (Roslyn for C#). It enables 80–90% token savings on large
codebases compared to reading full files.

> **Start here.** Use the links below to navigate to detail files for each topic.
> Read [serena-01-overview.instructions.md](serena-01-overview.instructions.md) first
> if you are new to Serena.

---

## Quick Reference — When to Use Serena

| Situation                                           | Use Serena?                         |
| --------------------------------------------------- | ----------------------------------- |
| Explore a DAO or Service you've never opened        | ✅ Yes — `get_symbols_overview`     |
| Find all callers before changing a method signature | ✅ Yes — `find_referencing_symbols` |
| Refactor a method body in a 500-line class          | ✅ Yes — `replace_symbol_body`      |
| Search for anti-patterns across 300+ files          | ✅ Yes — `search_for_pattern`       |
| Rename a symbol everywhere in the codebase          | ✅ Yes — `rename_symbol`            |
| Single-line fix in a file you already have open     | ❌ No — use replace_string_in_file  |
| Edit a YAML/JSON config file                        | ❌ No — use read_file + replace     |
| Create a new file from scratch                      | ❌ No — use create_file             |

---

## MTM Memory Catalog (Quick Reference)

| Memory                     | Contents                                              |
| -------------------------- | ----------------------------------------------------- |
| `architectural_patterns`   | MVVM layer rules, layer flow                          |
| `forbidden_practices`      | Static DAOs, raw SQL, runtime Binding — must not do   |
| `dao_best_practices`       | `Model_Dao_Result`, stored procedure pattern          |
| `coding_standards`         | Naming conventions, `_camelCase`, bracing             |
| `mvvm_guide`               | Complete ViewModel/Service/DAO walkthrough            |
| `xaml_binding_patterns`    | `x:Bind` modes, `UpdateSourceTrigger`                 |
| `error_handling_guide`     | `IService_ErrorHandler`, try-catch pattern            |
| `infor_visual_constraints` | SQL Server READ ONLY, `ApplicationIntent=ReadOnly`    |
| `tech_stack`               | .NET 8, WinUI 3, MySQL 8, CommunityToolkit.Mvvm       |
| `project_overview`         | Modules, DB inventory, team context                   |
| `constitution_summary`     | Distilled critical rules from copilot-instructions.md |
| `dialog_patterns`          | `ContentDialog`, window sizing                        |
| `help_system_architecture` | In-app help system design                             |
| `suggested_commands`       | Build, test, MySQL connection commands                |
| `task_completion_workflow` | Build → test → validate after each task               |

Read a memory: `read_memory("forbidden_practices")`

---

## Detail File Index

### [01 — Overview & Quick Start](serena-01-overview.instructions.md)

What Serena is, when to use it vs standard tools, quick-start commands, decision tree,
and the four-phase workflow summary.

---

### [02 — Tools Reference](serena-02-tools-reference.instructions.md)

Every Serena tool with full parameter descriptions and MTM-specific examples:

- `get_symbols_overview` — file structure without reading full content
- `find_symbol` — read a specific method/class/property
- `find_referencing_symbols` — all callers/usages of a symbol
- `replace_symbol_body` — replace an entire method or class
- `insert_before_symbol` / `insert_after_symbol` — add code at symbol boundaries
- `rename_symbol` — language-server rename across entire codebase
- `search_for_pattern` — regex search across files
- `replace_content` — regex replace within file content
- `read_memory` / `write_memory` / `list_memories` / `edit_memory` / `delete_memory`
- `execute_command` — run shell commands (build, test, git)

---

### [03 — Language Support](serena-03-language-support.instructions.md)

C# and Roslyn language server setup, supported capabilities (find references, rename,
hierarchy), language-specific `name_path_pattern` syntax, and JetBrains plugin alternative.

---

### [04 — Running Serena](serena-04-running.instructions.md)

Installation options (uvx, pip, Docker), startup command reference, transport modes
(stdio vs streamable-http), and Windows-specific notes.

---

### [05 — Client Configuration](serena-05-clients.instructions.md)

How to connect Serena as an MCP server to:

- **VSCode GitHub Copilot** (`.vscode/mcp.json` — primary for MTM)
- Claude Code (per-project and global setup)
- Claude Desktop (`claude_desktop_config.json`)
- Codex, JetBrains Junie, and other clients

Includes recommended **context** (`ide`, `claude-code`, `desktop-app`) per client.

---

### [06 — Project Workflow](serena-06-workflow.instructions.md)

Creating and indexing a project, `project.yml` configuration, activating a project,
the four-phase workflow (Create → Activate → Onboard → Code), and git worktree tips.

---

### [07 — Memories & Onboarding](serena-07-memories.instructions.md)

Project vs global memories, memory topics/namespaces, the full MTM memory catalog,
how onboarding works, triggering re-onboarding, managing memories via dashboard.

---

### [08 — Configuration](serena-08-configuration.instructions.md)

`serena_config.yml` global settings, `project.yml` full annotated example,
`project.local.yml` for personal overrides, **contexts** table, **modes** table,
and configuration priority order.

---

### [09 — Dashboard, Logs & Security](serena-09-dashboard-logs-security.instructions.md)

Accessing the web dashboard (`http://localhost:24282`), Tool Calls tab for monitoring,
log levels and log file locations, and security safeguards:
git checkpoints, read-only mode, `excluded_tools`, Docker isolation.

---

### [10 — Advanced Usage](serena-10-advanced-usage.instructions.md)

Prompting strategies for MTM, context window management, custom agents with Agno,
comparison with other coding agents, git worktrees, and five
MTM-specific step-by-step coding workflows (add DAO method, add service method,
refactor ViewModel command, validate architecture, understand unknown module).

---

## Official References

- Serena GitHub: <https://github.com/oraios/serena>
- Full documentation: <https://oraios.github.io/serena/>
- Tools list: <https://oraios.github.io/serena/01-about/035_tools.html>
- Language support: <https://oraios.github.io/serena/01-about/020_programming-languages.html>
