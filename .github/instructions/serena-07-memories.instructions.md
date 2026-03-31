---
applyTo: "**"
description: >
  Serena memory system — project vs global memories, organizing memories,
  the onboarding process, and MTM-specific memory catalog.
---

# Serena Memories & Onboarding

Official docs: <https://oraios.github.io/serena/02-usage/045_memories.html>

Memories are simple **Markdown files** that both you and the AI can create, read, and edit.
They persist project knowledge across sessions — solving the "AI forgets everything" problem.

---

## Memory Types

### Project Memories

Stored in `.serena/memories/` **inside the project root**. Scoped to this project only.
Commit these to git so all team members and future sessions benefit.

```
.serena/
  memories/
    architectural_patterns.md    ← MVVM rules, layer separation
    coding_standards.md          ← C# naming, formatting conventions
    forbidden_practices.md       ← What never to do (static DAOs, raw SQL, etc.)
    ...
```

### Global Memories

Stored in `~/.serena/memories/global/` (Windows: `%USERPROFILE%\.serena\memories\global\`).
Shared across **all projects**. Use for your personal preferences and cross-project patterns.

Memory name prefix `global/` → stored as global memory:

```
write_memory("global/my-preferences", "Always prefer x:Bind, never use Binding")
```

**Protecting global memories from accidental edits:**
Add regex patterns to `read_only_memory_patterns` in `serena_config.yml`:

```yaml
read_only_memory_patterns:
  - "global/.*" # All global memories are read-only
```

---

## Organizing Memories

Use `/` in memory names to create topics (mapped to filesystem subdirectories):

```
architecture/patterns       →  .serena/memories/architecture/patterns.md
architecture/constraints    →  .serena/memories/architecture/constraints.md
database/mysql              →  .serena/memories/database/mysql.md
database/infor-visual       →  .serena/memories/database/infor-visual.md
```

The `list_memories` tool can filter by topic prefix:

```
"List all memories under the architecture/ topic"
```

---

## MTM Memory Catalog

All 15 MTM project memories and their purpose:

| Memory File                   | Contents                                                                   |
| ----------------------------- | -------------------------------------------------------------------------- |
| `architectural_patterns.md`   | MVVM layer separation rules, layer flow diagrams                           |
| `coding_standards.md`         | Naming conventions, bracing, accessibility modifiers                       |
| `constitution_summary.md`     | Distilled version of critical rules from `.github/copilot-instructions.md` |
| `dao_best_practices.md`       | DAO pattern, `Model_Dao_Result`, stored procedure calling                  |
| `dialog_patterns.md`          | `ContentDialog`, window sizing, modal patterns                             |
| `error_handling_guide.md`     | `IService_ErrorHandler`, ViewModel try-catch pattern                       |
| `forbidden_practices.md`      | Things that must never happen (static DAOs, raw SQL, etc.)                 |
| `help_system_architecture.md` | In-app help system design                                                  |
| `infor_visual_constraints.md` | SQL Server READ ONLY rules, `ApplicationIntent=ReadOnly`                   |
| `mvvm_guide.md`               | Complete MVVM walkthrough with examples                                    |
| `project_overview.md`         | High-level: modules, database inventory, team context                      |
| `suggested_commands.md`       | `dotnet build`, `dotnet test`, MySQL connection commands                   |
| `task_completion_workflow.md` | How to complete a coding task: build → test → validate                     |
| `tech_stack.md`               | .NET 8, WinUI 3, MySQL 5.7, SQL Server, CommunityToolkit.Mvvm              |
| `xaml_binding_patterns.md`    | `x:Bind` patterns, Mode, UpdateSourceTrigger                               |

**Reading memories at session start:**

```
"Read the architectural_patterns and forbidden_practices memories before starting"
```

---

## Onboarding

### What Onboarding Does

On first project activation (when `.serena/memories/` is empty), Serena automatically:

1. **Reads key files** — entry point, config files, main module folders
2. **Analyzes structure** — build system, test setup, dependency injection, patterns
3. **Writes memory files** — one per discovered topic

### How Onboarding Is Triggered

- Automatic: when no project memories exist
- Manual: ask the AI to run onboarding, or delete memories and re-activate

```
"Please run onboarding for the MTM Receiving Application"
```

### After Onboarding

1. **Switch to a fresh conversation** — onboarding fills the context window
2. **Review the memories** in `.serena/memories/` and edit as needed
3. **Commit memories** to git:

```bash
git add .serena/memories/
git commit -m "Add Serena project memories from initial onboarding"
```

---

## Managing Memories via Dashboard

The Serena dashboard (`http://localhost:24282/dashboard`) provides a graphical UI
for viewing, creating, editing, and deleting memories while Serena is running.

To open:

```
"Open the Serena dashboard"
# or navigate directly to http://localhost:24282/dashboard
```

---

## Managing Memories in Code

**Create/overwrite a memory:**

```
write_memory("architecture/mvvm_layer_rules", "Content here...")
```

**Read a specific memory:**

```
read_memory("forbidden_practices")
```

**List all memories:**

```
list_memories()           # All memories
list_memories("database") # Only database/* memories
```

**Edit an existing memory:**

```
edit_memory("coding_standards", new_content)
```

---

## Disabling Memories and Onboarding

If memories are not wanted (e.g., for a quick one-off task):

```yaml
# In serena_config.yml or project.yml
base_modes:
  - no-memories # Disable all memory tools (including onboarding)
  # OR
  - no-onboarding # Disable only onboarding, keep memory tools
```

Or at startup:

```bash
serena start-mcp-server --mode no-onboarding
```

---

## Memory Best Practices for MTM

1. **Keep memories focused** — one topic per file, not a brain dump
2. **Update after refactoring** — if you rename a class, update the architectural_patterns memory
3. **Add new patterns** — when you establish a new coding pattern, write a memory for it
4. **Protect critical memories** — add `read_only_memory_patterns: ["constitution_summary"]` to
   prevent the AI from accidentally editing the core rules

**Recommended memory update triggers:**

- After completing a major feature
- After deciding on a new pattern or convention
- After discovering an important constraint (e.g., new Infor Visual table structure)
