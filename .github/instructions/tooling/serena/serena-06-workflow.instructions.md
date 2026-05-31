---
applyTo: "**"
description: >
  Serena project workflow — creation, indexing, activation, onboarding,
  preparing your codebase, and working with multiple projects.
---

# Serena Project Workflow

Official docs: <https://oraios.github.io/serena/02-usage/040_workflow.html>

The project workflow proceeds through these phases:

```
1. Project Creation  →  2. Project Activation  →  3. Onboarding  →  4. Coding Tasks
```

---

## Phase 1: Project Creation & Indexing

A **project** is simply a directory on the filesystem. Project creation sets up Serena
metadata in a `.serena/` subfolder inside that directory.

### Explicit Project Creation (Recommended)

```bash
cd C:\Users\johnk\source\repos\MTM_Receiving_Application

uvx --from git+https://github.com/oraios/serena serena project create \
  --language csharp \
  --name "MTM_Receiving_Application" \
  --index
```

- `--language csharp` — tell Serena which language server to spawn
- `--name "MTM_Receiving_Application"` — friendly name for activation later
- `--index` — immediately index the project for fast tool execution

**Creates:**

```
.serena/
  project.yml          ← Project config (commit to git)
  project.local.yml    ← Local overrides (gitignored by default)
  cache/               ← Symbol index (gitignored, rebuilt with 'project index')
```

### Implicit Project Creation

Tell the AI to activate a directory without prior setup:

```
"Activate the project C:\Users\johnk\source\repos\MTM_Receiving_Application"
```

Serena will auto-detect languages and create the project with defaults. You can refine
settings in `project.yml` afterward.

---

## Project Configuration (`project.yml`)

After creation, edit `.serena/project.yml` to configure the project:

```yaml
name: MTM_Receiving_Application
languages:
  - csharp
read_only: false # Set true to allow analysis-only (no file edits)
encoding: utf-8

# Initial prompt passed to LLM when project activates
initial_prompt: >
  This is the MTM Receiving Application — a WinUI 3 MVVM desktop application.
  Always follow the MVVM architecture: View → ViewModel → Service → DAO → DB.
  Read the architectural_patterns memory before making any changes.

# Tools to disable for this project (use names from tools reference)
excluded_tools: []

# Additional ignore rules (beyond .gitignore)
ignore_patterns:
  - "bin/**"
  - "obj/**"
  - ".vs/**"
```

**Local overrides** that won't be committed go in `project.local.yml`:

```yaml
# project.local.yml (gitignored)
read_only: false # My personal override
```

---

## Indexing

Indexing pre-caches symbol information from the language server, dramatically speeding up
tool execution on large projects like MTM (300+ files).

```bash
# Run once after project creation:
uvx --from git+https://github.com/oraios/serena serena project index
```

**When to re-index:**

- After major refactoring (50+ files changed simultaneously)
- If `find_symbol` or `get_symbols_overview` become noticeably slow
- After long development sessions with many changes

**Note:** During normal coding sessions, Serena automatically updates the index as files change.
Manual re-indexing is only needed in the cases above.

---

## Phase 2: Project Activation

Activation makes Serena aware of which project to work on.

### Option A: At MCP Server Startup (VSCode/Claude Code)

```bash
serena start-mcp-server --project "C:\Users\johnk\source\repos\MTM_Receiving_Application"
```

This is automatic when using the `.vscode/mcp.json` configuration with `${workspaceFolder}`.

### Option B: During Conversation

Tell the AI:

```
"Activate the project MTM_Receiving_Application"
# or
"Activate the project C:\Users\johnk\source\repos\MTM_Receiving_Application"
```

**Note:** The `activate_project` tool is disabled in single-project contexts (`ide`,
`claude-code`). Since a project is already set at startup, there's no need to switch.

---

## Phase 3: Onboarding

On first project activation (when no memories exist), Serena automatically runs onboarding:

1. Serena **reads key project files** — entry points, config files, main modules
2. It **analyzes project structure** — build system, test setup, architectural patterns
3. It **writes memory files** to `.serena/memories/` documenting what it learned

**MTM existing memories** (already created):

```
.serena/memories/
  architectural_patterns.md
  coding_standards.md
  constitution_summary.md
  dao_best_practices.md
  dialog_patterns.md
  error_handling_guide.md
  forbidden_practices.md
  help_system_architecture.md
  infor_visual_constraints.md
  mvvm_guide.md
  project_overview.md
  suggested_commands.md
  task_completion_workflow.md
  tech_stack.md
  xaml_binding_patterns.md
```

**Tips:**

- After onboarding, start a **fresh conversation** — onboarding fills up the context window
- Review the generated memories and edit them to add MTM-specific rules
- Commit memories to git so the next developer (or next session) starts with context

**To re-run onboarding** (if memories are wrong/stale):

```
"Please re-run onboarding for the MTM project"
# or delete .serena/memories/ and re-activate the project
```

---

## Phase 4: Working on Coding Tasks

See [serena-02-tools-reference.instructions.md](serena-02-tools-reference.instructions.md) for
tool usage during coding tasks.

---

## Preparing Your Project for Serena

### Clean Git State

Start tasks from a clean git state. This enables:

```bash
git status                # Should show nothing uncommitted
git add -A && git commit -m "checkpoint before refactoring"
```

Benefits:

- Easy inspection of Serena's changes via `git diff`
- Serena can itself call `git diff` to validate its own changes
- Clean rollback if needed

### Line Endings (Windows)

On Windows, set git to use CRLF so `git diff` isn't polluted by line ending changes:

```bash
git config --global core.autocrlf true
```

### Build and Test Before Starting

Ensure a green baseline before asking Serena to make changes:

```powershell
dotnet build MTM_Receiving_Application.slnx
dotnet test MTM_Receiving_Application.slnx
```

---

## Multiple Projects

### Single Agent, Multiple Projects → Monorepo

Create a folder containing all projects as subfolders and open it as a single Serena project.

### Multiple Agents Reading a Shared Project

Start Serena in HTTP mode so multiple agents share one instance:

```bash
serena start-mcp-server --transport streamable-http --port 8765
```

Each agent connects to `http://localhost:8765`.

### Reading from an External Project (Dependency Inspection)

Enable the `query-projects` mode to read code from another Serena project without switching
the active project:

```
"Switch to query-projects mode"
# then:
"Query the project InforVisual_Integration to find the Purchase Order model"
```

This requires the external project to also have been indexed by Serena.

### Git Worktrees + Serena

When using `git worktree` for parallel work, copy the symbol cache to each worktree:

```bash
cp -r $ORIG_PROJECT/.serena/cache $GIT_WORKTREE/.serena/cache
```

This avoids re-indexing for each worktree.
