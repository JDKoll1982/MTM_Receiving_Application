---
description: Serena semantic coding tools - comprehensive guide for efficient code navigation, symbol-level editing, and intelligent codebase exploration
applyTo: '**/*.cs'
---

# Serena Semantic Coding Tools - Complete Reference

## Table of Contents

- [Overview](#overview)
- [Quick Reference](#quick-reference)
- [Project Workflow](#project-workflow)
- [Core Tools](#core-tools)
- [File & Pattern Search](#file--pattern-search)
- [Memory Management](#memory-management)
- [Shell & Utilities](#shell--utilities)
- [Thinking Tools](#thinking-tools)
- [Contexts & Modes](#contexts--modes)
- [MTM-Specific Workflows](#mtm-specific-workflows)
- [Token Efficiency](#token-efficiency)
- [Performance Optimization](#performance-optimization)
- [Troubleshooting](#troubleshooting)
- [Security Considerations](#security-considerations)

---

## Overview

**Serena** is a semantic coding agent toolkit providing IDE-like capabilities through the Language Server Protocol (LSP). It enables symbol-level operations (classes, methods, properties) rather than file-based text manipulation, significantly reducing token usage and improving accuracy.

**Key Benefits:**
- **80-90% token reduction** for large codebase exploration
- **Symbol-level precision** - Edit specific methods without reading entire files
- **Relationship discovery** - Find all usages before refactoring
- **Architectural validation** - Search for anti-patterns across codebase
- **C# language support** - Full LSP integration with OmniSharp/Roslyn

**Official Documentation**: https://oraios.github.io/serena/  
**Dashboard**: http://127.0.0.1:24282/dashboard/ (when MCP server running)

---

## Quick Reference

### Most-Used Tools for MTM Project

| Tool | Purpose | Token Savings | MTM Use Case |
|------|---------|---------------|--------------|
| `get_symbols_overview` | Get file structure | ~95% | Explore new DAO/View without reading all code |
| `find_symbol` | Find specific symbol | ~90% | Read one method from 500-line class |
| `find_referencing_symbols` | Find all usages | N/A | Check impact before refactoring method |
| `replace_symbol_body` | Replace entire symbol | N/A | Update DAO method implementation |
| `search_for_pattern` | Regex search | Variable | Find `MessageBox.Show` violations |
| `read_memory` | Access project knowledge | ~100% | Load architectural patterns once |

### Decision Tree: When to Use Serena

```
Is the task about code navigation/editing?
├─ YES: Is it 3+ files OR finding usages OR symbol-level?
│  ├─ YES: ✅ Use Serena (80-90% token savings)
│  └─ NO: Is it a single line edit?
│     ├─ YES: ❌ Use standard tools
│     └─ NO: ✅ Use Serena anyway (precision)
└─ NO: Is it file creation from scratch?
   └─ YES: ❌ Use standard file creation
```

### When to Use Serena

#### ✅ Use Serena For:
- **Large codebases** - Navigate 300+ files efficiently
- **Symbol discovery** - Find classes, methods, properties by name or pattern
- **Relationship analysis** - Discover where symbols are referenced
- **Precise editing** - Replace entire methods/classes accurately
- **Pattern searches** - Regex-based searches across specific file types
- **Architecture exploration** - Understand code structure without reading everything
- **Multi-file tasks** - Refactoring across 3+ files
- **Architectural validation** - Finding anti-patterns (MessageBox.Show, direct SQL)
- **Token efficiency** - When context window is limited

#### ❌ Don't Use Serena For:
- **Single-line edits** - Use standard editing tools
- **Non-code files** - Serena is optimized for code (C#, Java, Python, etc.)
- **Simple file reading** - Use `read_file` for small, non-code files (<100 lines)
- **Creating new files from scratch** - Use standard file creation tools
- **Binary files** - Images, executables, databases
- **Configuration files** - JSON, XML, YAML (unless very large)

---

## Project Workflow

Serena uses a project-based workflow. Understanding this is **critical** for effective usage.

### 1. Project Creation

**When:** First time using Serena in a codebase.

```powershell
# Explicit creation (recommended for MTM project)
serena project create --language csharp --name "MTM_Receiving_Application" --index

# Or let Serena auto-create on first activation
# (AI will call activate_project, which creates if needed)
```

**What it does:**
- Creates `.serena/project.yml` in project root
- Configures C# language server (OmniSharp/Roslyn)
- Sets up ignore patterns (respects .gitignore)
- Optionally indexes project (recommended for large codebases)

**MTM Project Configuration** (`.serena/project.yml`):
```yaml
name: MTM_Receiving_Application
languages:
  - csharp
read_only: false  # Set true for QA agents
excluded_tools: []  # Add tools to disable (e.g., execute_shell_command)
```

### 2. Project Indexing

**When:** After creation OR when many files change.

```powershell
# Index project for faster tool execution
serena project index

# Check index health
serena project health-check
```

**Why it matters:**
- **300+ C# files** in MTM project
- Indexing takes 30-60 seconds upfront
- Saves 5-10 seconds per tool call during session
- Auto-updates incrementally as files change

**MTM Note:** For git worktrees (parallel development), copy `.serena/cache/` to avoid re-indexing.

### 3. Project Activation

**When:** Start of every conversation.

```
# AI calls this tool
activate_project("/path/to/MTM_Receiving_Application")

# Or by project name (if previously registered)
activate_project("MTM_Receiving_Application")
```

**What it does:**
- Loads project configuration
- Starts language server
- Makes all tools available for this project
- Loads list of available memories

**Important:** Some contexts (like `ide-assistant`) disable this tool if project is provided at startup.

### 4. Onboarding (First-Time Setup)

**When:** First conversation in a new project.

```
# AI should check if onboarding was performed
check_onboarding_performed()

# If not, run onboarding
onboarding()
```

**What onboarding does:**
1. Explores project structure (directories, key files)
2. Identifies main components (DAOs, Services, Views, Helpers)
3. Reads architectural patterns (DAO pattern, error handling, MVVM)
4. Creates memories in `.serena/memories/`:
   - `architectural_patterns.md`
   - `coding_standards.md`
   - `project_structure.md`
   - `common_tasks.md`

**MTM Onboarding Focus:**
- DAO structure and `Model_Dao_Result` pattern
- `Service_ErrorHandler` usage
- `Helper_Database_StoredProcedure` for all database access
- MVVM pattern (`ViewModel`, `View`, `Model`)
- #region organization requirements

**After Onboarding:** Review and edit memories in `.serena/memories/`. Add custom memories as needed.

---

## Core Tools

### Symbol Discovery

#### `find_symbol` - Find Classes, Methods, Properties

**Purpose:** Locate and optionally read specific symbols by name or pattern.

**Parameters:**
- `name_path_pattern` (required, string): Symbol name or path (see Name Path Syntax)
- `relative_path` (optional, string): Restrict search to file/directory
- `include_body` (optional, boolean): Include source code (default: false)
- `depth` (optional, integer): Include descendants (0 = symbol only, 1 = immediate children)
- `substring_matching` (optional, boolean): Match partial names (default: false)
- `include_kinds` (optional, list[int]): Only return specific symbol types
- `exclude_kinds` (optional, list[int]): Exclude symbol types

**Returns:** List of symbols with name, kind, location, optionally body.

**Symbol Kinds Reference:**
```
5  = Class         | 12 = Function    | 22 = Enum Member
6  = Method        | 13 = Variable    | 23 = Struct
7  = Property      | 14 = Constant    | 24 = Event
8  = Field         | 15 = String      | 25 = Operator
9  = Constructor   | 16 = Number      | 26 = Type Parameter
10 = Enum          | 17 = Boolean     |
11 = Interface     |                  |
```

**MTM Examples:**

```
# 1. Find a DAO method implementation
find_symbol(
    name_path_pattern="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    include_body=true
)

# 2. Get all methods in a DAO (without reading implementations)
find_symbol(
    name_path_pattern="Dao_ReceivingLine",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    depth=1,
    include_body=false
)

# 3. Find all "GetAll*" methods across all DAOs
find_symbol(
    name_path_pattern="GetAll",
    relative_path="Data",
    substring_matching=true,
    include_kinds=[6]  # Only methods
)

# 4. Find Service_ErrorHandler methods
find_symbol(
    name_path_pattern="Service_ErrorHandler/*",
    relative_path="Services/Service_ErrorHandler.cs",
    depth=1
)

# 5. Find specific overload
find_symbol(
    name_path_pattern="Helper_Database_StoredProcedure/ExecuteDataTableWithStatusAsync[0]",
    include_body=true
)

# 6. Find all View implementations
find_symbol(
    name_path_pattern="*View",
    relative_path="Views",
    substring_matching=true,
    include_kinds=[5]  # Only classes
)
```

**Token Efficiency:**
- Reading one method: **~100-500 tokens**
- Reading entire 500-line file: **~5,000 tokens**
- **Savings: 90%**

**Name Path Syntax:**
- **Simple name**: `"GetAllAsync"` → Matches any symbol with that name
- **Relative path**: `"Dao_ReceivingLine/GetAllAsync"` → Matches this path suffix
- **Absolute path**: `"/Dao_ReceivingLine/GetAllAsync"` → Exact match from file root
- **Wildcard**: `"Dao_ReceivingLine/*"` → All direct children
- **Overload**: `"MyMethod[0]"` → First overload (0-based index)

**Anti-Patterns:**
- ❌ Reading body when you only need the signature
- ❌ Not restricting `relative_path` when you know the file
- ❌ Using without `substring_matching` when name is uncertain

---

#### `get_symbols_overview` - High-Level File Structure

**Purpose:** Get overview of top-level symbols in a file WITHOUT reading full code.

**Parameters:**
- `relative_path` (required, string): File path
- `depth` (optional, integer): Hierarchy depth (0 = top-level only, 1 = include immediate children)
- `max_answer_chars` (optional, integer): Limit output size (-1 = default limit)

**Returns:** JSON with symbol names, kinds, signatures (no implementations).

**MTM Examples:**

```
# 1. Explore a new DAO file
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)
# Returns: Class name + all method signatures (no bodies)

# 2. Check View structure
get_symbols_overview("Views/Receiving/Receiving_WorkflowView.xaml.cs", depth=1)
# Returns: Class + properties + methods (useful before editing)

# 3. Quick scan of Service methods
get_symbols_overview("Services/Service_ErrorHandler.cs", depth=1)

# 4. Understand Helper structure
get_symbols_overview("Helpers/Database/Helper_Database_StoredProcedure.cs", depth=0)
# Returns: Just the class structure, no methods

# 5. Review ViewModel layout
get_symbols_overview("ViewModels/Receiving/Receiving_WorkflowViewModel.cs", depth=1)
```

**Best Practice:** **ALWAYS** call this first when exploring a new file. It's like reading a table of contents before diving into chapters.

**Token Efficiency:**
- Overview of 20-method class: **~200-300 tokens**
- Reading full class: **~3,000-5,000 tokens**
- **Savings: 93-95%**

---

#### `find_referencing_symbols` - Find All Usages

**Purpose:** Discover where a symbol is referenced (calls, instantiations, etc.).

**Parameters:**
- `name_path` (required, string): Symbol to find references for
- `relative_path` (required, string): File containing the symbol
- `include_kinds` (optional, list[int]): Filter referencing symbols by type
- `exclude_kinds` (optional, list[int]): Exclude specific symbol types
- `max_answer_chars` (optional, integer): Limit output size

**Returns:** List of referencing symbols with metadata + code snippets.

**MTM Examples:**

```
# 1. Find where GetAllAsync is called before modifying it
find_referencing_symbols(
    name_path="GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs"
)

# 2. Find all usages of Service_ErrorHandler.ShowUserError
find_referencing_symbols(
    name_path="ShowUserError",
    relative_path="Services/Service_ErrorHandler.cs"
)

# 3. Find where Helper_Database_StoredProcedure is used
find_referencing_symbols(
    name_path="ExecuteDataTableWithStatusAsync",
    relative_path="Helpers/Database/Helper_Database_StoredProcedure.cs"
)

# 4. Find View instantiations
find_referencing_symbols(
    name_path="ReceivingView",  # Constructor
    relative_path="Views/Receiving/Receiving_WorkflowView.xaml.cs"
)

# 5. Find Model_Dao_Result usage
find_referencing_symbols(
    name_path="Model_Dao_Result",
    relative_path="Models/Model_Dao_Result.cs"
)
```

**Use Cases:**
- **Before refactoring** - Check impact radius
- **Breaking changes** - Find all callers to update
- **Usage validation** - Verify pattern compliance
- **Documentation** - Understand how something is used

**Important:** This is a **MUST** before modifying method signatures or renaming symbols!

---

### Code Editing

#### `replace_symbol_body` - Replace Entire Symbol

**Purpose:** Replace complete symbol definition (method, class, property).

**Parameters:**
- `name_path` (required, string): Symbol to replace
- `relative_path` (required, string): File containing symbol
- `body` (required, string): New complete symbol definition

**Returns:** Success/failure status.

**Important:** `body` must include the **COMPLETE** definition:
- For methods: Signature + implementation
- For classes: Class declaration + all members
- For properties: Full property definition

**MTM Examples:**
```
# 1. Replace DAO method implementation
replace_symbol_body(
    name_path="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body='''    /// <summary>
    /// Gets all receiving lines from database.
    /// </summary>
    /// <returns>Model_Dao_Result with DataTable of receiving lines.</returns>
    public async Task<Model_Dao_Result<DataTable>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure
            .ExecuteDataTableWithStatusAsync(
                "md_receiving_GetAll",
                null);
    }'''
)

# 2. Update Service_ErrorHandler method
replace_symbol_body(
    name_path="Service_ErrorHandler/ShowUserError",
    relative_path="Services/Service_ErrorHandler.cs",
    body=new_method_implementation
)
```

**Workflow:**
1. Read existing symbol: `find_symbol("Class/Method", include_body=true)`
2. Modify the code
3. Replace: `replace_symbol_body("Class/Method", file, modified_code)`

**Anti-Patterns:**
- ❌ Using for single-line edits (use standard tools)
- ❌ Forgetting XML documentation in the new body
- ❌ Not including correct indentation
- ❌ Omitting method signature/class declaration

---

#### `insert_after_symbol` - Insert Code After Symbol

**Purpose:** Add new code immediately after a symbol's definition ends.

**Parameters:**
- `name_path` (required, string): Symbol after which to insert
- `relative_path` (required, string): File path
- `body` (required, string): Code to insert

**MTM Examples:**

```
# 1. Add new method after existing method in DAO
insert_after_symbol(
    name_path="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body='''
    /// <summary>
    /// Gets receiving line by ID.
    /// </summary>
    /// <param name="id">ID to search for.</param>
    /// <returns>Model_Dao_Result with DataTable of matching line.</returns>
    public async Task<Model_Dao_Result<DataTable>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id }
        };
        
        return await Helper_Database_StoredProcedure
            .ExecuteDataTableWithStatusAsync(
                "md_receiving_GetById",
                parameters);
    }
'''
)

# 2. Add code at end of file (after last top-level symbol)
insert_after_symbol(
    name_path="Dao_ReceivingLine",  # Last class in file
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body="\n// End of Dao_ReceivingLine.cs\n"
)
```

**Use Cases:**
- Adding new methods to classes
- Inserting code at end of file
- Adding properties to models
- Appending helper methods

---

#### `insert_before_symbol` - Insert Code Before Symbol

**Purpose:** Add new code immediately before a symbol's definition starts.

**Parameters:**
- `name_path` (required, string): Symbol before which to insert
- `relative_path` (required, string): File path
- `body` (required, string): Code to insert

**MTM Examples:**

```
# 1. Add using statements at start of file
insert_before_symbol(
    name_path="Dao_ReceivingLine",  # First class in file
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body="using System.Collections.Generic;\n"
)

# 2. Add XML comment before class
insert_before_symbol(
    name_path="Dao_ReceivingLine",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body='''/// <summary>
/// Data access object for receiving line management.
/// </summary>
'''
)
```

**Common Use:** Adding imports/using statements at file start.

---

#### `rename_symbol` - Refactor Symbol Name

**Purpose:** Rename a symbol throughout the **entire codebase** using LSP refactoring.

**Parameters:**
- `name_path` (required, string): Symbol to rename
- `relative_path` (required, string): File containing symbol
- `new_name` (required, string): New symbol name

**Returns:** Summary of changes made.

**MTM Examples:**

```
# 1. Rename DAO method everywhere
rename_symbol(
    name_path="GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    new_name="GetAllReceivingLinesAsync"
)
# Updates: All call sites, XML docs, comments

# 2. Rename class and all references
rename_symbol(
    name_path="Dao_DunnageLine",
    relative_path="Data/Receiving/Dao_DunnageLine.cs",
    new_name="Dao_Dunnage"
)

# 3. Rename property
rename_symbol(
    name_path="Model_Dao_Result/IsSuccess",
    relative_path="Models/Model_Dao_Result.cs",
    new_name="Success"
)
```

**Important:**
- LSP handles **all references** automatically
- Safer than find-and-replace
- Works across **multiple files**
- Updates comments and strings (usually)

**Caution:** For methods with overloads, may need to specify index: `"MyMethod[0]"`

---

## File & Pattern Search

#### `find_file` - Locate Files by Name/Pattern

**Purpose:** Find files matching a glob pattern.

**Parameters:**
- `file_mask` (required, string): Filename or glob pattern (* and ? wildcards)
- `relative_path` (required, string): Directory to search in ("." for root)

**Returns:** List of matching file paths.

**MTM Examples:**

```
# 1. Find all DAO files
find_file("Dao_*.cs", "Data")

# 2. Find specific file
find_file("Helper_Database_StoredProcedure.cs", ".")

# 3. Find all View code-behind files
find_file("*.xaml.cs", "Views")

# 4. Find Model files
find_file("Model_*.cs", "Models")

# 5. Find stored procedure SQL files
find_file("md_receiving_*.sql", "Database/StoredProcedures")
```

**Use Cases:**
- Locating files before reading
- Finding related files (e.g., all DAOs)
- Discovering file organization

---

#### `search_for_pattern` - Regex Pattern Search

**Purpose:** Search for regex patterns across codebase with context.

**Parameters:**
- `substring_pattern` (required, string): Regex pattern (Python `re` module syntax)
- `relative_path` (optional, string): Restrict to file/directory
- `restrict_search_to_code_files` (optional, boolean): Only search LSP-indexed code files
- `context_lines_before` (optional, integer): Lines of context before match
- `context_lines_after` (optional, integer): Lines of context after match
- `paths_include_glob` (optional, string): Include files matching glob
- `paths_exclude_glob` (optional, string): Exclude files matching glob
- `max_answer_chars` (optional, integer): Limit output size

**Returns:** Mapping of file paths to matched lines with context.

**Important:** Use DOTALL mode (`.` matches newlines). Prefer non-greedy quantifiers (`.*?`).

**MTM Examples - Architectural Validation:**

```
# 1. Find MessageBox.Show violations (anti-pattern)
search_for_pattern(
    substring_pattern="MessageBox\\.Show",
    restrict_search_to_code_files=true,
    context_lines_before=2,
    context_lines_after=2
)

# 2. Find direct MySqlConnection usage (anti-pattern)
search_for_pattern(
    substring_pattern="new MySqlConnection",
    relative_path="Data",
    restrict_search_to_code_files=true
)

# 3. Find methods missing XML documentation
search_for_pattern(
    substring_pattern="^\\s*public.*\\(",
    restrict_search_to_code_files=true,
    relative_path="Data"
)
# Then verify preceding line doesn't have ///

# 4. Find Helper_Database_StoredProcedure usage
search_for_pattern(
    substring_pattern="Helper_Database_StoredProcedure\\.Execute",
    relative_path="Data"
)

# 5. Find Service_ErrorHandler usage
search_for_pattern(
    substring_pattern="Service_ErrorHandler\\.(ShowUserError|HandleException)",
    restrict_search_to_code_files=true
)

# 6. Find Model_Dao_Result returns
search_for_pattern(
    substring_pattern="Task<Model_Dao_Result",
    relative_path="Data"
)

# 7. Find View inheritance
search_for_pattern(
    substring_pattern="class \\w+View : Page",
    relative_path="Views"
)

# 8. Find stored procedure calls
search_for_pattern(
    substring_pattern='"md_receiving_\\w+"',
    relative_path="Data/Receiving/Dao_ReceivingLine.cs"
)
```

**Performance Tips:**
- Use `relative_path` to limit search scope
- Use `restrict_search_to_code_files=true` to skip binaries/logs
- Use `paths_exclude_glob` to skip large directories (e.g., "bin/**")
- Use `max_answer_chars` to prevent huge outputs

---

#### `list_dir` - List Directory Contents

**Purpose:** List files and subdirectories.

**Parameters:**
- `relative_path` (required, string): Directory path ("." for root)
- `recursive` (required, boolean): Include subdirectories
- `skip_ignored_files` (optional, boolean): Respect .gitignore
- `max_answer_chars` (optional, integer): Limit output size

**Returns:** JSON with directories and files.

**MTM Examples:**

```
# 1. List all top-level directories
list_dir(".", recursive=false, skip_ignored_files=true)

# 2. List all DAO files
list_dir("Data", recursive=false)

# 3. List Views directory structure
list_dir("Views", recursive=true, skip_ignored_files=true)

# 4. List stored procedures
list_dir("Database/StoredProcedures", recursive=false)
```

**Use Cases:**
- Understanding project structure
- Finding related files
- Verifying file organization

---

## Memory Management

Serena maintains project memories in `.serena/memories/`. These are markdown files containing project knowledge that can be read on-demand.

#### `list_memories` - List Available Memories

**Purpose:** See what memories exist.

**Parameters:** None

**Returns:** List of memory filenames.

**MTM Example:**

```
list_memories()

# Typical MTM memories after onboarding:
# - architectural_patterns.md
# - coding_standards.md
# - dao_best_practices.md
# - service_error_handler_usage.md
# - mvvm_guide.md
# - common_tasks.md
```

---

#### `read_memory` - Read Memory Content

**Purpose:** Load project knowledge into context.

**Parameters:**
- `memory_file_name` (required, string): Memory filename
- `max_answer_chars` (optional, integer): Limit output size

**Returns:** Memory content.

**MTM Examples:**

```
# Read architectural patterns before working
read_memory("architectural_patterns.md")

# Load DAO best practices
read_memory("dao_best_practices.md")

# Check MVVM guide
read_memory("mvvm_guide.md")

# Review common tasks
read_memory("common_tasks.md")
```

**Best Practices:**
- Read relevant memories at **start of conversation**
- Only read memories relevant to current task
- Don't re-read same memory multiple times in one conversation

---

#### `write_memory` - Create/Update Memory

**Purpose:** Store project knowledge for future sessions.

**Parameters:**
- `memory_file_name` (required, string): Memory filename
- `content` (required, string): Markdown content

**Returns:** Success confirmation.

**When to Write:**
- After discovering important patterns
- When completing complex refactoring (save summary)
- When user provides new architectural guidance
- End of successful task (save what was learned)

---

## Shell & Utilities

#### `execute_shell_command` - Run Terminal Commands

**Purpose:** Execute commands for builds, tests, database operations.

**Parameters:**
- `command` (required, string): Shell command to run
- `cwd` (optional, string): Working directory

**Returns:** stdout, stderr, exit code.

**MTM Examples:**

```
# 1. Build project
execute_shell_command(
    command="dotnet build MTM_Receiving_Application.csproj --configuration Debug"
)

# 3. Execute stored procedure migration
execute_shell_command(
    command='& "C:\\MAMP\\bin\\mysql\\bin\\mysql.exe" -h localhost -P 3306 -u root -proot mtm_receiving_application < Database/Schemas/01_create_receiving_tables.sql'
)

# 4. Git diff before committing
execute_shell_command(command="git diff")
```

**Security Warning:**
- ⚠️ Can execute **arbitrary commands**
- ⚠️ Risk of data loss if used incorrectly
- ⚠️ Consider `excluded_tools: [execute_shell_command]` for read-only agents

---

#### `restart_language_server` - Recover from LSP Issues

**Purpose:** Restart C# language server if it becomes unresponsive.

**When to Use:**
- Symbol search returns no results (but files exist)
- \"Language server not responding\" errors
- After major refactoring (LSP loses sync)
- After external file modifications (outside Serena)

---

## Thinking Tools

These tools help organize workflows and ensure quality. Use them strategically.

#### `think_about_collected_information`

**When:** After gathering data via multiple reads/searches.

**Purpose:** Reflect on completeness and relevance of collected information.

**Best Practice:** Call after 3+ consecutive read/search operations.

---

#### `think_about_task_adherence`

**When:** Before making code changes (especially large ones).

**Purpose:** Verify alignment with original task goals.

**Best Practice:** Call after conversation has gone 10+ turns.

---

#### `think_about_whether_you_are_done`

**When:** When you believe task is complete.

**Purpose:** Final verification before reporting completion.

**Best Practice:** ALWAYS call before telling user \"task complete\".

---

## Contexts & Modes

Contexts and modes configure Serena's behavior and available tools.

### Contexts (Environment-Specific)

**What:** Defines operating environment and tool set. **Set at startup, cannot change during session.**

**Available Contexts:**

| Context | Purpose | Use With |
|---------|---------|----------|
| `desktop-app` | Full standalone agent | Claude Desktop, standalone MCP clients |
| `claude-code` | Optimized for Claude Code | Claude Code CLI |
| `ide` | IDE assistant mode | VS Code, Cursor, Cline |
| `agent` | Autonomous agent | Agno, custom frameworks |

**MTM Recommendation:** Use `ide` context when working in VS Code with Serena MCP server.

**Setting Context:**

```powershell
# In MCP server launch command
serena start-mcp-server --context ide --project /path/to/MTM_Receiving_Application
```

---

### Modes (Task-Specific Behaviors)

**What:** Refines behavior for specific workflows. **Can be changed mid-session** with `switch_modes` tool.

**Available Modes:**

| Mode | Purpose |
|------|---------|
| `editing` | Code modification - prioritizes editing tools |
| `interactive` | Conversational - asks questions before acting |
| `planning` | Analysis/design - focuses on understanding |
| `one-shot` | Single response - completes task without follow-up |
| `no-onboarding` | Skip onboarding - use after first time |

**MTM Recommendations:**
- **Daily work:** `editing` + `interactive` (default)
- **Initial exploration:** `planning` + `interactive` + `onboarding`
- **Quick fixes:** `editing` + `one-shot` + `no-onboarding`
- **Code review:** `planning` + `no-onboarding`

**Setting Modes:**

```powershell
# At startup
serena start-mcp-server --mode editing --mode interactive

# During session (AI calls)
switch_modes(["planning", "one-shot"])
```

---

## MTM-Specific Workflows

### Workflow 1: Exploring a New DAO

**Scenario:** Need to understand `Dao_ReceivingLine` without reading all code.

**Steps:**

```
# 1. Get file structure (200-300 tokens)
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)

# 2. Read specific methods of interest (100-200 tokens each)
find_symbol("Dao_ReceivingLine/GetAllAsync", include_body=true)
find_symbol("Dao_ReceivingLine/InsertAsync", include_body=true)

# 3. Check how methods are used
find_referencing_symbols("GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs")

# 4. Verify architectural compliance
search_for_pattern(
    substring_pattern="Helper_Database_StoredProcedure",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs"
)
```

**Token Efficiency:** 500-800 tokens (vs 5,000+ for reading entire file)

---

### Workflow 2: Validating Service_ErrorHandler Usage

**Scenario:** Ensure no `MessageBox.Show` exists; all errors use `Service_ErrorHandler`.

**Steps:**

```
# 1. Find MessageBox.Show violations
search_for_pattern(
    substring_pattern="MessageBox\\.Show",
    restrict_search_to_code_files=true,
    context_lines_before=3,
    context_lines_after=3
)

# 2. For each violation, read context and fix
find_symbol("ViolatingClass/ViolatingMethod", include_body=true)

# 3. Replace with Service_ErrorHandler
replace_symbol_body(
    name_path="ViolatingClass/ViolatingMethod",
    relative_path="path/to/file.cs",
    body=corrected_method_with_service_error_handler
)

# 4. Verify fix
search_for_pattern(
    substring_pattern="MessageBox\\.Show",
    relative_path="path/to/file.cs"
)
```

---

### Workflow 3: Adding New DAO Method

**Scenario:** Add `GetByIdAsync` to `Dao_ReceivingLine`.

**Steps:**

```
# 1. Read existing method as template
find_symbol("Dao_ReceivingLine/GetAllAsync", include_body=true)

# 2. Check where to insert (after GetAllAsync)
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)

# 3. Insert new method
insert_after_symbol(
    name_path="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body=new_method_code_with_xml_docs
)

# 4. Verify insertion
find_symbol("Dao_ReceivingLine/GetByIdAsync", include_body=true)

# 5. Build to check compilation
execute_shell_command("dotnet build MTM_Receiving_Application.csproj")
```

---

### Workflow 4: Refactoring Method Signature

**Scenario:** Change `GetAllAsync()` to `GetAllAsync(bool includeInactive)`.

**Steps:**

```
# 1. Find all usages BEFORE changing
find_referencing_symbols("GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs")

# 2. Update method signature and implementation
replace_symbol_body(
    name_path="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    body=updated_method_with_new_parameter
)

# 3. Fix all call sites (from step 1 results)
# For each caller:
find_symbol("CallerClass/CallerMethod", include_body=true)
replace_symbol_body(
    name_path="CallerClass/CallerMethod",
    relative_path="path/to/caller.cs",
    body=updated_caller_code
)

# 4. Build and test
execute_shell_command("dotnet build MTM_Receiving_Application.csproj")
```

---

## Workflow Patterns

### Exploring a New Component

1. **Get file structure**:
   ```
   get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)
   ```

2. **Read specific methods**:
   ```
   find_symbol("Dao_ReceivingLine/GetAllAsync", include_body=true)
   find_symbol("Dao_ReceivingLine/InsertAsync", include_body=true)
   ```

3. **Find dependencies**:
   ```
   find_referencing_symbols("GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs")
   ```

### Modifying a Method

1. **Find the method**:
   ```
   find_symbol("Dao_ReceivingLine/GetAllAsync", include_body=true)
   ```

2. **Check where it's used**:
   ```
   find_referencing_symbols("GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs")
   ```

3. **Replace the method**:
   ```
   replace_symbol_body("Dao_ReceivingLine/GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs", new_code)
   ```

### Adding a New Method

1. **Find where to insert**:
   ```
   get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)
   ```

2. **Insert after related method**:
   ```
   insert_after_symbol("Dao_ReceivingLine/GetAllAsync", "Data/Receiving/Dao_ReceivingLine.cs", new_method)
   ```

### Finding Anti-Patterns

```
# Find direct MessageBox.Show usage (anti-pattern)
search_for_pattern("MessageBox\\.Show", restrict_search_to_code_files=true)

# Find direct MySqlConnection usage (anti-pattern)
search_for_pattern("new MySqlConnection", restrict_search_to_code_files=true)

# Find missing async/await
search_for_pattern("public.*Task.*\\(", restrict_search_to_code_files=true)
```

## Name Path Syntax

Symbols are identified by their **name path** within a file:

- **Class**: `MyClass`
- **Method**: `MyClass/MyMethod`
- **Nested class**: `OuterClass/InnerClass`
- **Property**: `MyClass/MyProperty`
- **Overloaded method**: `MyClass/MyMethod[0]` (0-based index)

### Name Path Patterns

- **Simple name**: `"GetAllAsync"` - Matches any symbol with that name
- **Relative path**: `"Dao_ReceivingLine/GetAllAsync"` - Matches this path suffix
- **Absolute path**: `"/Dao_ReceivingLine/GetAllAsync"` - Exact match only
- **Substring matching**: `"GetAll"` with `substring_matching=true` - Matches `GetAllAsync`, `GetAllData`, etc.

---

## Token Efficiency

### Benchmark: Exploring 10 DAO Files

| Approach | Token Usage | Time |
|----------|-------------|------|
| **Read all files completely** | ~50,000 tokens | 2-3 min |
| **Serena: Overviews + selective reads** | ~3,000 tokens | 30-45 sec |
| **Token savings** | **94%** | **75% faster** |

### ❌ Inefficient Approach
```
# Reading entire files repeatedly
read_file("Data/Receiving/Dao_ReceivingLine.cs", 1, 500)
read_file("Data/Authentication/Dao_User.cs", 1, 500)
read_file("Data/Receiving/Dao_DunnageLine.cs", 1, 500)
# Result: 10,000+ tokens consumed
```

### ✅ Efficient Approach with Serena
```
# Get overview first
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)

# Read only what you need
find_symbol("Dao_ReceivingLine/GetAllAsync", include_body=true)
find_symbol("Dao_ReceivingLine/InsertAsync", include_body=true)
# Result: 500-1000 tokens consumed (90% savings)
```

---

## Performance Optimization

### 1. Pre-Index Large Projects

```powershell
# Run once before starting work
serena project index

# Saves 5-10 seconds per tool call
```

**MTM Project:** Indexing 300+ files takes ~60 seconds. Do this once per worktree.

### 2. Restrict Search Scope

```
# ❌ Slow: Search entire project
search_for_pattern("GetAll")

# ✅ Fast: Search specific directory
search_for_pattern("GetAll", relative_path="Data")
```

### 3. Use Depth Wisely

```
# ❌ Too much: depth=2 for large class
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=2)

# ✅ Optimal: depth=1 (class + immediate children)
get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)
```

### 4. Don't Include Body Unless Needed

```
# ❌ Wasteful: include_body=true when exploring
find_symbol("Dao_*", relative_path="Data", include_body=true)

# ✅ Efficient: Get signatures first
find_symbol("Dao_*", relative_path="Data", include_body=false)
```

### 5. Use Memories to Cache Knowledge

```
# ❌ Re-reading architectural patterns every conversation
read_file(".github/copilot-instructions.md")

# ✅ Read once, save as memory, reuse
write_memory("architectural_patterns.md", extracted_patterns)
# Future conversations:
read_memory("architectural_patterns.md")
```

### 6. Git Worktrees: Copy Cache

```powershell
# Creating new worktree
git worktree add ../MTM_feature_branch

# Copy Serena cache (avoid re-indexing)
Copy-Item -Recurse .serena/cache ../MTM_feature_branch/.serena/cache
```

---

## Troubleshooting

### Issue: find_symbol Returns Empty

**Symptoms:** `find_symbol` finds nothing, but file exists.

**Solutions:**

```
# 1. Check language server status
get_current_config()

# 2. Restart language server
restart_language_server()

# 3. Re-index project
execute_shell_command("serena project index")

# 4. Try broader search
find_symbol("ReceivingLine", substring_matching=true)

# 5. Try reading file directly (fallback)
read_file("Data/Receiving/Dao_ReceivingLine.cs", 1, 50)
```

---

### Issue: Slow Tool Execution

**Symptoms:** Tools take 10+ seconds to complete.

**Solutions:**

```
# 1. Index project
serena project index

# 2. Restrict search scope
search_for_pattern(
    "pattern",
    relative_path="Data",  # Not entire project
    max_answer_chars=10000  # Limit output
)

# 3. Use include_body=false when exploring
find_symbol("Class", include_body=false, depth=1)

# 4. Check dashboard for bottlenecks
# Visit: http://127.0.0.1:24282/dashboard/
```

---

### Issue: Context Window Filling Up

**Symptoms:** AI warns about context limit.

**Solutions:**

```
# 1. Use Serena tools (reduce tokens)
# Instead of read_file, use:
get_symbols_overview(file, depth=1)

# 2. Save progress and start new conversation
prepare_for_new_conversation()
write_memory("current_progress.md", summary)

# 3. In new conversation:
activate_project("MTM_Receiving_Application")
read_memory("current_progress.md")
# Continue work
```

---

## Security Considerations

### Read-Only Mode

**Use Case:** Code review, architectural analysis, QA agents.

**Configuration:**

```yaml
# .serena/project.yml
read_only: true
```

**Effect:**
- Disables: `replace_symbol_body`, `insert_*`, `create_text_file`, `execute_shell_command`
- Enables: All read/search tools

---

### Disable Dangerous Tools

**Use Case:** Untrusted environments, shared agents.

**Configuration:**

```yaml
# .serena/project.yml
excluded_tools:
  - execute_shell_command  # No arbitrary commands
  - delete_memory          # Prevent memory deletion
  - write_memory           # Prevent memory creation (optional)
```

---

### MTM-Specific Security

**Database:**
- **Never** expose MySQL root password in commands
- Use `Helper_Database_Variables.cs` for connection strings
- Don't commit database credentials to git

**Sensitive Data:**
- Search for API keys before committing:
  ```
  search_for_pattern("API[_-]?KEY|SECRET|PASSWORD", restrict_search_to_code_files=true)
  ```

**Production Safety:**
- Use `read_only: true` for production environment exploration
- Test changes in `mtm_receiving_application_test` database first
- Never run `DROP` or `TRUNCATE` commands via `execute_shell_command`

---

## Additional Resources

### Serena Documentation
- **Homepage**: https://oraios.github.io/serena/
- **Tool Reference**: https://oraios.github.io/serena/03-tools/000_intro.html
- **Configuration**: https://oraios.github.io/serena/02-usage/050_configuration.html
- **GitHub Repository**: https://github.com/oraios/serena

### MTM Project Documentation
- **Architecture Guide**: `.github/copilot-instructions.md`
- **Development Workflow**: `AGENTS.md`
- **Constitution**: `.specify/memory/constitution.md`
- **Instruction Files**: `.github/instructions/` directory

---

## Summary

**Key Principle**: Use Serena to navigate code at the symbol level (classes, methods) rather than reading entire files. This dramatically reduces token consumption and improves accuracy for large codebases.

**Workflow**: Overview → Find → Read (only what you need) → Edit (precisely) → Verify (references)

**MTM Receiving Application**: Serena is especially valuable for navigating the DAO files, finding Service_ErrorHandler usages, and verifying architectural compliance across the codebase.
