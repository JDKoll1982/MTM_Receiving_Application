---
applyTo: "**"
description: >
  Complete Serena tool catalogue — symbol tools, file tools, memory tools,
  workflow tools, and shell tools — with MTM-specific usage examples.
---

# Serena Tools Reference

Full catalogue of Serena tools. In most configurations only a **subset** is active at a time,
controlled by the active [context and modes](serena-08-configuration.instructions.md).

Official reference: <https://oraios.github.io/serena/01-about/035_tools.html>

---

## Symbol Tools

These are the core tools used for code navigation and editing.

### `get_symbols_overview`

Gets top-level symbols (classes, methods, properties) defined in a file — **without reading
the full file content**. Use this first before `find_symbol`.

**MTM examples:**

```
Get symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs
Get symbols overview for Module_Core/Services/Service_ErrorHandler.cs
```

**Returns:** List of symbol names with line numbers and kinds (class, method, property, field).

---

### `find_symbol`

Performs a global or local search for a specific symbol by name path, optionally including its
full body. The most frequently used tool.

**Parameters:**

- `name_path_pattern` — symbol name or path (e.g., `Dao_ReceivingLine/InsertReceivingLineAsync`)
- `relative_path` — restrict search to a folder or file
- `include_body` — whether to include source code (default: `false`)
- `substring_matching` — match partial names (e.g., `Insert` matches `InsertAsync`)
- `depth` — how many nesting levels to show

**MTM examples:**

```
Find symbol "Dao_ReceivingLine" in Module_Receiving/Data/ with depth=1
Find symbol "InsertReceivingLineAsync" with body in Module_Receiving/Data/
Find symbol "ViewModel_Receiving_Workflow" with include_body=false, depth=2
Find symbol "LoadDataAsync" with substring_matching=true in Module_Receiving/ViewModels/
```

**Token savings:** ~90% vs reading a 400-line file to find one method.

---

### `find_referencing_symbols`

Finds all symbols that reference a given symbol — the "find all usages" operation.
**Always run this before changing a method signature, renaming a property, or deleting code.**

**MTM examples:**

```
Find referencing symbols for Dao_ReceivingLine/GetLinesByLoadAsync
Find referencing symbols for ViewModel_Shared_Base/IsBusy
Find referencing symbols for IService_MySQL_ReceivingLine/InsertLineAsync
```

**Returns:** List of methods/classes that call or use the symbol, with code snippets.

---

### `replace_symbol_body`

Replaces the **entire definition** of a symbol (method, class, property) with new content.
Use this instead of file-based edits when replacing a whole method or class body.

**When to use:**

- Updating a DAO method implementation
- Replacing an entire ViewModel command handler
- Refactoring a Service method

**Do NOT use for:**

- Small tweaks inside a method (use `replace_content` for line-level edits instead)

---

### `insert_after_symbol`

Inserts new code immediately after the end of a symbol's definition. Useful for adding:

- A new method at the end of a class
- A new property after the last property

**MTM example:**

```
Insert after the last method in Dao_ReceivingLine:
  public async Task<Model_Dao_Result<int>> CountByStatusAsync(string status) { ... }
```

---

### `insert_before_symbol`

Inserts new code immediately before the start of a symbol's definition.

---

### `rename_symbol`

Renames a symbol **throughout the entire codebase** using language server refactoring.
This is safe — it correctly handles XAML bindings, usages in other files, string references.

**MTM examples:**

```
Rename symbol ViewModel_Receiving_Workflow/CurrentLineID to CurrentLoadID
Rename symbol IService_MySQL_ReceivingLine to IService_ReceivingLine
```

---

## File Tools (optional — enabled in `desktop-app` context)

These tools are available when Serena is used as a standalone agent (not in `ide` context,
which assumes the IDE handles file operations).

| Tool                   | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| `read_file`            | Read a file's contents                                       |
| `create_file`          | Create a new file                                            |
| `delete_file`          | Delete a file                                                |
| `list_dir`             | List directory contents                                      |
| `search_files_by_name` | Find files by name pattern                                   |
| `replace_content`      | Regex or string replacement within a file (line-level edits) |

**MTM note:** In VSCode with the `ide` context, use VS Code's built-in tools for file
operations. Serena's file tools in this context are limited to `replace_content`, which is
useful for surgical edits inside a method.

---

## Memory Tools

| Tool            | Description                                               |
| --------------- | --------------------------------------------------------- |
| `list_memories` | List available memory files, optionally filtered by topic |
| `read_memory`   | Read a specific memory file                               |
| `write_memory`  | Create or overwrite a memory file                         |
| `edit_memory`   | Edit an existing memory file                              |
| `delete_memory` | Delete a memory file                                      |

**MTM project memories** (in `.serena/memories/`):

| Memory                     | Contents                              |
| -------------------------- | ------------------------------------- |
| `architectural_patterns`   | MVVM layer rules, forbidden patterns  |
| `coding_standards`         | Naming conventions, C# style          |
| `constitution_summary`     | Key non-negotiable rules              |
| `dao_best_practices`       | DAO pattern, `Model_Dao_Result` usage |
| `dialog_patterns`          | Dialog and window management          |
| `error_handling_guide`     | `IService_ErrorHandler` usage         |
| `forbidden_practices`      | What never to do                      |
| `help_system_architecture` | In-app help system                    |
| `infor_visual_constraints` | SQL Server READ ONLY rules            |
| `mvvm_guide`               | MVVM detailed guide                   |
| `project_overview`         | High-level project description        |
| `suggested_commands`       | Useful dev commands                   |
| `task_completion_workflow` | How to complete tasks                 |
| `tech_stack`               | Technology details                    |
| `xaml_binding_patterns`    | x:Bind patterns                       |

---

## Workflow Tools

| Tool                         | Description                                        |
| ---------------------------- | -------------------------------------------------- |
| `activate_project`           | Make Serena work with a specific project directory |
| `check_onboarding_performed` | Check if onboarding has run                        |
| `initial_instructions`       | Read Serena's initial instructions for the project |

---

## Shell / Command Tools (optional)

| Tool                 | Description                      | MTM Use                       |
| -------------------- | -------------------------------- | ----------------------------- |
| `execute_command`    | Run a shell command              | `dotnet build`, `dotnet test` |
| `get_current_config` | Show active Serena configuration | Debugging tool setup          |
| `switch_modes`       | Dynamically switch active modes  | Switch to planning mode       |
| `open_dashboard`     | Open Serena's web dashboard      | Monitor tool execution        |

---

## Search Tools

| Tool                 | Description                              |
| -------------------- | ---------------------------------------- |
| `search_for_pattern` | Regex-based search across codebase files |

**MTM architecture validation searches:**

```
# Find ViewModels calling DAOs directly (FORBIDDEN)
Pattern: "Dao_" in Module_**/ViewModels/**/*.cs

# Find static DAO classes (FORBIDDEN)
Pattern: "static.*Dao_" in Module_**/*.cs

# Find raw SQL in C# (FORBIDDEN for MySQL)
Pattern: "SELECT|INSERT|UPDATE|DELETE" in Module_**/*.cs

# Find hardcoded connection strings (FORBIDDEN)
Pattern: "Server=|password=" in **/*.cs

# Find MessageBox.Show outside Views (FORBIDDEN)
Pattern: "MessageBox\.Show" excluding **/Views/**

# Find runtime Binding in XAML (FORBIDDEN)
Pattern: "\{Binding " in **/*.xaml
```

---

## Tool Selection Quick Reference

| You Want To...                               | Tool                                   |
| -------------------------------------------- | -------------------------------------- |
| Understand a file's structure                | `get_symbols_overview`                 |
| Read one method from a large file            | `find_symbol` with `include_body=true` |
| Find all callers before changing a signature | `find_referencing_symbols`             |
| Replace/rewrite an entire method             | `replace_symbol_body`                  |
| Add a new method to a class                  | `insert_after_symbol`                  |
| Rename a property everywhere                 | `rename_symbol`                        |
| Search for an anti-pattern                   | `search_for_pattern`                   |
| Edit a few lines inside a method             | `replace_content`                      |
| Preserve project knowledge                   | `write_memory` / `edit_memory`         |
