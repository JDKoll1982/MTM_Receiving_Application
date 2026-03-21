---
applyTo: "**"
description: >
  Serena configuration — global config file, project.yml settings, contexts
  (ide, claude-code, desktop-app), modes (editing, planning, interactive), and advanced options.
---

# Serena Configuration

Official docs: <https://oraios.github.io/serena/02-usage/050_configuration.html>

Serena has two levels of configuration:

| Level       | File                                      | Scope                        |
| ----------- | ----------------------------------------- | ---------------------------- |
| **Global**  | `%USERPROFILE%\.serena\serena_config.yml` | All projects on this machine |
| **Project** | `.serena/project.yml` (in project root)   | This project only            |

Project settings override global settings. Local overrides go in `.serena/project.local.yml`
(gitignored by default).

---

## Global Configuration (`serena_config.yml`)

Auto-created on first run at:

- **Windows:** `%USERPROFILE%\.serena\serena_config.yml`
- **Linux/macOS:** `~/.serena/serena_config.yml`

**Access via:**

- Serena dashboard (Settings tab)
- Direct text editor
- Command: `serena config edit`

**Key global settings:**

```yaml
# Default context (override with --context at startup)
contexts:
  default: desktop-app

# Active base modes (always on, not override-able via CLI)
base_modes:
  - interactive
  - editing

# Active default modes (can be overridden via --mode at startup)
default_modes: []

# Dashboard settings
web_dashboard_open_on_launch: true # Set false to suppress auto-open

# Log level (DEBUG, INFO, WARNING, ERROR)
log_level: INFO

# Serena data directory override (default: ~/.serena)
# SERENA_HOME environment variable also works

# Per-project .serena folder location
project_serena_folder_location: "$projectDir/.serena" # Default

# Read-only memory patterns (regex)
read_only_memory_patterns: []

# GUI tool (Windows native log viewer — disabled by default)
# gui_tool: true
```

---

## Project Configuration (`project.yml`)

Created in `.serena/project.yml` when running `serena project create`.

**Full annotated example for MTM:**

```yaml
# Friendly name for referencing this project
name: MTM_Receiving_Application

# Language servers to spawn (required for symbol tools)
languages:
  - csharp

# Language backend: 'language_server' (default) or 'jetbrains'
language_backend: language_server

# Source file encoding
encoding: utf-8

# Prevent Serena from modifying files (analysis only)
read_only: false

# Paths to ignore (extends .gitignore)
ignore_patterns:
  - "bin/**"
  - "obj/**"
  - ".vs/**"
  - "TestResults/**"

# Initial prompt shown to LLM each time this project activates
initial_prompt: >
  This is the MTM Receiving Application — a WinUI 3 .NET 8 MVVM desktop application
  for manufacturing receiving operations. Before making any changes:
  1. Read the 'architectural_patterns' memory
  2. Read the 'forbidden_practices' memory
  3. Ensure all changes follow MVVM: View → ViewModel → Service → DAO → Database

# Override default modes for this project
default_modes:
  - editing
  - interactive

# Tools to disable for this project
excluded_tools: []
```

**Local overrides** (not committed to git):

```yaml
# .serena/project.local.yml
read_only: false # Personal setting that differs from team default
```

---

## Contexts

Contexts define a **preset toolset** optimized for a particular client/workflow.
Context is set at startup: `--context ide`.

| Context           | Best For                          | Toolset                                                                          |
| ----------------- | --------------------------------- | -------------------------------------------------------------------------------- |
| `ide`             | **VSCode, JetBrains IDEs**        | Symbol tools + memory tools only. File/shell tools disabled (IDE handles those). |
| `claude-code`     | **Claude Code CLI**               | Symbol tools + memory tools only.                                                |
| `codex`           | **OpenAI Codex**                  | Codex-optimized toolset                                                          |
| `desktop-app`     | Claude Desktop, standalone agents | Full toolset including file/shell operations                                     |
| `agent`           | Autonomous agents                 | Full toolset                                                                     |
| `oaicompat-agent` | OpenAI-compatible agents          | OpenAI-format tools                                                              |

**Recommendation for MTM in VSCode:** `ide` context restricts Serena to symbol and memory tools.
VS Code's built-in Copilot handles file reads, searches, and terminal commands.

---

## Modes

Modes fine-tune behavior **within** a context. Multiple modes can be active simultaneously.
They affect the system prompt and can additionally enable/disable specific tools.

### Built-in Modes

| Mode             | Purpose                                                    |
| ---------------- | ---------------------------------------------------------- |
| `editing`        | Optimizes Serena for direct code modification              |
| `planning`       | Focuses on analysis and planning tasks (read-heavy)        |
| `interactive`    | Back-and-forth conversational style                        |
| `one-shot`       | Complete a task in a single response (use with `planning`) |
| `no-onboarding`  | Skip onboarding but retain memory tools                    |
| `no-memories`    | Disable all memory tools and onboarding                    |
| `onboarding`     | Focus exclusively on the onboarding process                |
| `query-projects` | Enable tools for querying other Serena projects            |

### How Modes Are Composed

```
Final active modes = base_modes (from global) + base_modes (from project) + default_modes (from global + project)
                     (CLI --mode overrides default_modes but NOT base_modes)
```

### MTM Recommended Mode Setup

For typical coding sessions:

```bash
serena start-mcp-server --context ide --mode editing --mode interactive
```

For architecture analysis/planning (read-only, no edits):

```bash
serena start-mcp-server --context ide --mode planning --mode one-shot
```

---

## Advanced Configuration

### Custom Serena Data Directory

```bash
# Environment variable (PowerShell)
$env:SERENA_HOME = "D:\serena-data"
```

Or in `serena_config.yml`:

```yaml
# The SERENA_HOME env variable overrides this setting
```

### Per-Project Serena Folder Location

Store all project metadata in a central location (outside the project tree):

```yaml
# In serena_config.yml
project_serena_folder_location: "C:\serena-projects\$projectFolderName\.serena"
```

Variables:

- `$projectDir` — absolute path to project root
- `$projectFolderName` — just the folder name (e.g., `MTM_Receiving_Application`)

**Backward compatibility:** If a `.serena` folder already exists in the project root,
Serena uses it regardless of this setting.

### Language Server-Specific Settings

Override the Roslyn Language Server download for air-gapped environments:

```yaml
ls_specific_settings:
  csharp:
    runtime_dependencies:
      - id: "CSharpLanguageServer"
        platform_id: "win-x64"
        url: "https://your-mirror/roslyn.win-x64.nupkg"
        package_version: "5.5.0-2.26078.4"
```

### Custom System Prompts

Extend Serena's default instructions for your workflow. Add to `serena_config.yml` or
`project.yml`:

```yaml
custom_system_prompt_suffix: >
  When working on this project, always check the current git branch before making changes.
  Prefer small, incremental commits over large batches.
```

---

## Configuration Priority

From lowest to highest precedence:

```
Global serena_config.yml
  ↓
Project .serena/project.yml
  ↓
Local .serena/project.local.yml
  ↓
CLI arguments (--mode, --context, etc.)
```
