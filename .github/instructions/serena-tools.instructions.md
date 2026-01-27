---
description: Serena semantic coding tools for efficient C# code navigation, symbol-level editing, and intelligent codebase exploration
applyTo: '**/*.cs'
---

# Serena Semantic Coding Tools for MTM Application

## Overview

**Serena** is an IDE-like coding toolkit providing semantic code retrieval and editing at the symbol level (classes, methods, properties). Use Serena to navigate the MTM Application's 300+ C# files efficiently without reading entire files.

### Key Benefits for MTM Project

- **80-90% token savings** - Navigate large codebases without reading full files
- **Symbol-level precision** - Edit specific methods in 500+ line classes accurately
- **Relationship discovery** - Find all usages of a method before refactoring
- **Architecture validation** - Verify MVVM patterns across all Views and ViewModels
- **Automated refactoring** - Rename symbols throughout codebase with language server support

### When to Use Serena

**✅ Use Serena For:**
- Exploring new DAO or Service implementations
- Finding all usages of a method before changing its signature
- Refactoring across 3+ files
- Validating architectural patterns (MVVM, DI, database access rules)
- Searching for anti-patterns (`MessageBox.Show` outside views, direct SQL, static DAOs)
- Multi-file symbol rename or replace operations
- Understanding large class structures without full context reads

**❌ Don't Use Serena For:**
- Single-line edits (use standard file editing)
- Creating new projects (use file creation tools)
- Reading configuration files or non-code files
- Binary files or compiled output

### Documentation Reference

- **Official Docs:** https://oraios.github.io/serena/
- **Tools List:** https://oraios.github.io/serena/01-about/035_tools.html
- **Project Workflow:** https://oraios.github.io/serena/02-usage/040_workflow.html
- **Configuration:** https://oraios.github.io/serena/02-usage/050_configuration.html

---

## Quick Start

### Start the Serena MCP Server

```bash
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --help
```

### Essential Serena Commands

```bash
# Create project (run in MTM_Receiving_Application directory)
serena project create --language csharp --name "MTM_Receiving_Application" --index

# Index project for faster operations (first time or after large changes)
serena project index

# View available tools
serena tools list --all

# Open dashboard for logs and statistics
serena open dashboard
```

### Most-Used Tools for MTM Development

| Tool | Purpose | MTM Use Case | Token Savings |
|------|---------|--------------|---------------|
| `get_symbols_overview` | View file structure | Explore new DAO without reading entire file | ~95% |
| `find_symbol` | Find/read specific symbol | Read one method from large class | ~90% |
| `find_referencing_symbols` | Find all usages | Check impact before changing method signature | ~85% |
| `replace_symbol_body` | Replace method/class body | Update DAO implementation accurately | ~80% |
| `search_for_pattern` | Regex search across codebase | Find all `MessageBox.Show()` in Views | ~70% |
| `rename_symbol` | Rename throughout codebase | Refactor property names in Services | ~75% |

### Decision Tree: Use Serena or Standard Tools?

```
Task involves exploring/editing code?
├─ YES: Affects 3+ files OR finding usages OR symbol-level precision?
│  ├─ YES: ✅ Use Serena (80-90% token savings)
│  └─ NO: Is it a single line edit in one file?
│     ├─ YES: Use standard file editing
│     └─ NO: ✅ Use Serena anyway (precision > token cost)
└─ NO: Creating/reading configuration?
   └─ Use standard file tools
```

---

## Project Workflow

**Serena operates on a project-based model.** Every conversation starts with a project context. Follow these four phases:

### Phase 1: Project Creation (First Time Only)

**Do this once** when first using Serena on MTM project.

```bash
cd C:\Users\johnk\source\repos\MTM_Receiving_Application
serena project create --language csharp --name "MTM_Receiving_Application" --index
```

**What this creates:**
- `.serena/project.yml` - Project configuration
- `.serena/cache/` - Symbol index (recommended for performance)

**Configure `.serena/project.yml`:**
```yaml
name: MTM_Receiving_Application
languages:
  - csharp
read_only: false
excluded_tools: []
```

### Phase 2: Project Activation (Each Conversation)

**Do this at the start of each conversation** to activate the project context.

**Option A:** Tell the AI to activate:
```
Activate the project MTM_Receiving_Application
```

**Option B:** Start Serena with project pre-loaded:
```bash
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --project "MTM_Receiving_Application"
```

### Phase 3: Onboarding (First Conversation Only)

**Serena automatically performs onboarding** on first activation. This:
- Explores project structure (DAOs, Services, Views, ViewModels)
- Identifies architectural patterns
- Creates memories in `.serena/memories/`

**After onboarding:**
1. Review generated memories in `.serena/memories/`
2. Edit memories to add MTM-specific patterns
3. Commit memories to git for persistence

**MTM Memories to Create/Review:**
- DAO pattern and `Model_Dao_Result` usage
- `Service_ErrorHandler` conventions
- MVVM layer separation rules
- `Helper_Database_StoredProcedure` for all database access
- Forbidden patterns (MessageBox.Show outside Views, direct SQL, static DAOs)

### Phase 4: Start Coding Tasks

Now use Serena tools for your coding work:
- Use `find_symbol` to locate DAO methods by name
- Use `get_symbols_overview` to explore file structure
- Use `find_referencing_symbols` to validate impact before changes
- Use `replace_symbol_body` to update method implementations
- Use `search_for_pattern` to find architectural violations

### Project Indexing (Performance Optimization)

For faster tool execution on large projects:

```bash
cd C:\Users\johnk\source\repos\MTM_Receiving_Application
serena project index
```

**When to re-index:**
- After major refactoring (50+ files changed)
- When tool execution slows down
- After long development sessions with many changes

---

## Core Tools Reference

### Symbol Discovery Tools

#### `get_symbols_overview`
**Purpose:** Get high-level structure of a file without reading full content.

**Use When:**
- Exploring a new DAO or Service for the first time
- Understanding file organization quickly
- Finding which method you need to edit

**Example:**
```
Get symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs
```

**Result:** List of top-level symbols (classes, methods, properties) with line numbers.

---

#### `find_symbol`
**Purpose:** Locate and optionally read specific classes, methods, or properties by name.

**Use When:**
- Finding a specific method in a large class
- Reading one method from a 500+ line file
- Searching for symbols matching a pattern

**Example:**
```
Find symbol "InsertReceivingLineAsync" with body in Module_Receiving/Data/
```

**Result:** Symbol with full source code (no need to read entire file).

**Parameters:**
- `name_path_pattern`: Symbol name or path (e.g., `Dao_ReceivingLine/InsertReceivingLineAsync`)
- `relative_path`: Restrict search to folder/file (e.g., `Module_Receiving/Data/`)
- `include_body`: Include source code (default: false)
- `substring_matching`: Match partial names (e.g., `Insert` matches `InsertAsync`)

---

#### `find_referencing_symbols`
**Purpose:** Find all usages of a symbol throughout codebase before making changes.

**Use When:**
- Validating impact before changing a method signature
- Checking if a property is still used
- Finding all callers of a service method

**Example:**
```
Find referencing symbols for Dao_ReceivingLine/GetLineAsync
```

**Result:** List of all methods calling `GetLineAsync` with code snippets.

---

### Code Editing Tools

#### `replace_symbol_body`
**Purpose:** Replace entire method/class implementation at symbol level.

**Use When:**
- Updating a DAO method implementation
- Refactoring a Service method
- Replacing entire class body

**Example:**
```
Replace body of Dao_ReceivingLine/InsertReceivingLineAsync with:
[new implementation]
```

**Advantages:**
- Precise - no line number guessing
- Safe - Serena finds exact symbol boundaries
- Token-efficient - works on symbol level

---

#### `rename_symbol`
**Purpose:** Rename a symbol throughout codebase using language server refactoring.

**Use When:**
- Renaming a property across ViewModel and XAML bindings
- Changing a method name in Service and all callers
- Renaming a class consistently

**Example:**
```
Rename symbol ViewModel_Receiving_Workflow/CurrentLineID to CurrentLoadID
```

---

### Search Tools

#### `search_for_pattern`
**Purpose:** Find code matching regex patterns across codebase.

**Use When:**
- Finding architectural violations (e.g., `MessageBox.Show`)
- Locating hardcoded connection strings
- Finding direct SQL queries in C# files

**Example: Find all MessageBox usage outside Views**
```
Search for pattern: MessageBox\.Show\(
In files: Module_**/*.cs
Exclude: **/Views/**
```

**Result:** Line numbers and context for each match.

**Common MTM Patterns:**

Find potential direct SQL (forbidden):
```
`SELECT|INSERT|UPDATE|DELETE` in `Module_**/*.cs`
```

Find hardcoded connection strings (forbidden):
```
`Server=|localhost|password=` in `**/*.cs`
```

Find static DAOs (forbidden):
```
`static.*Dao_` in `Module_**/*.cs`
```

---

## MTM-Specific Workflows

### Explore a New DAO

**Task:** Understand `Dao_ReceivingLine` to add a new method.

**Workflow:**

1. **Get overview:**
   ```
   Get symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs
   ```
   Result: See all methods quickly without reading 400+ line file

2. **Read constructor:**
   ```
   Find symbol "Dao_ReceivingLine/__init__" with body in Module_Receiving/Data/
   ```
   Result: Understand dependency injection pattern

3. **Read one method:**
   ```
   Find symbol "Dao_ReceivingLine/GetLineAsync" with body
   ```
   Result: See actual implementation pattern

4. **Write new method** using same pattern

**Token Savings:** ~90% compared to reading entire file

---

### Refactor Service Method Signature

**Task:** Change `GetReceivingLinesAsync(int loadId)` to accept filter object instead.

**Workflow:**

1. **Check all callers:**
   ```
   Find referencing symbols for Service_ReceivingLine/GetReceivingLinesAsync
   ```
   Result: See all 7 places calling this method

2. **Update implementation:**
   ```
   Replace body of Service_ReceivingLine/GetReceivingLinesAsync with:
   [new implementation with filter object]
   ```

3. **Update all callers:**
   For each caller, use same workflow to update calls

**Token Savings:** ~85% compared to manual grep + file reading

---

### Validate MVVM Architecture

**Task:** Ensure no Views directly call DAOs (forbidden pattern).

**Workflow:**

1. **Search for violating pattern:**
   ```
   Search for "Dao_" in Module_**/*Views/*.cs
   ```
   Result: List any violations

2. **Check each violation:**
   ```
   Find symbol <ClassName> with body to verify
   ```

3. **Extract to Service layer:**
   ```
   Replace symbol body to remove DAO call
   ```

**Token Savings:** Quick validation without exploring 50+ View files

---

## Language Server Backend

### C# Language Server (OmniSharp)

Serena uses OmniSharp/Roslyn for C# symbol analysis and refactoring.

**Capabilities:**
- Symbol finding with type information
- Find all references with parameter types
- Rename refactoring across codebase
- Type hierarchy (supertypes/subtypes)
- Completion hints

**Requirements:**
- .NET SDK installed (already in MTM project)
- OmniSharp started automatically by Serena

### Alternative: JetBrains Plugin

For more robust analysis, use Serena JetBrains Plugin with Rider IDE:
- More accurate symbol analysis
- Better support for complex C# patterns
- XAML binding awareness
- No language server configuration needed

See: https://plugins.jetbrains.com/plugin/28946-serena/

---

## Best Practices

### 1. Start with Overview
Always get symbols overview before diving into a file:
```
Get symbols overview for [file]
```
This prevents unnecessary full file reads (90%+ token savings).

### 2. Use Memories for Context
Create project memories to preserve MTM architectural knowledge:
- DAO pattern requirements
- Service layer conventions
- MVVM layer separation rules
- Forbidden practices

Access with:
```
Read memory [name]
```

### 3. Validate Impact Before Changes
Use `find_referencing_symbols` before changing:
- Method signatures
- Property names
- Service interfaces

This prevents breaking changes.

### 4. Search for Violations
Use `search_for_pattern` to validate architecture:
- No hardcoded connection strings
- No direct SQL in C# files
- No MessageBox.Show outside Views
- No static DAOs

### 5. Start from Clean Git State
Before starting Serena tasks:
```bash
git status
git commit -am "checkpoint before refactoring"
```

This enables:
- Inspection of changes with `git diff`
- Rollback if needed
- Serena can use `git diff` to validate its changes

---

## Troubleshooting

### Language Server Not Responding
**Problem:** Tools return errors about language server connection.

**Solution:**
```bash
# Restart language server
serena restart-language-server

# Or re-activate project
serena activate-project "MTM_Receiving_Application"
```

### Slow Tool Execution
**Problem:** `find_symbol` and other tools take 10+ seconds.

**Solution:**
```bash
# Re-index project
serena project index

# This pre-caches symbol information
```

### Symbol Not Found
**Problem:** `find_symbol` returns no results for method you know exists.

**Causes:**
- Name is slightly different (typo or pluralization)
- Use `substring_matching: true` to find partial matches
- Symbol is in a different namespace than expected

**Solution:**
```
Find symbol "Insert" with substring_matching, in Module_Receiving/Data/
```

### Memory Not Updated
**Problem:** Changes made but memories still show old patterns.

**Solution:**
1. Edit memories manually in `.serena/memories/`
2. Or re-run onboarding:
   ```
   Check onboarding and re-run if needed
   ```

---

## Performance Optimization

### Token Efficiency Strategy

**For 300+ file codebase, token efficiency is critical:**

| Operation | Standard Approach | Serena Approach | Savings |
|-----------|-------------------|-----------------|---------|
| Read one method | Read entire 400-line file | Use `find_symbol` with body | ~90% |
| Find all usages | Read all files, grep search | Use `find_referencing_symbols` | ~85% |
| Update method | Read file, locate, replace | Use `replace_symbol_body` | ~80% |
| Refactor 5 files | Read all 5 files fully | Use symbolic tools on each | ~75% |
| Validate architecture | Read 50+ views | Use `search_for_pattern` | ~70% |

### Indexing Impact

**Without indexing:**
- First tool call: 5-10 seconds
- Subsequent calls: 2-5 seconds
- Total for 10-tool session: ~50 seconds

**With indexing:**
- First tool call: 1-2 seconds
- Subsequent calls: <1 second
- Total for 10-tool session: ~15 seconds

**Recommendation:** Run `serena project index` once after project creation.

---

## Integration with MTM Architecture

### Verify MVVM Compliance

Use Serena to ensure architecture follows MTM rules:

```bash
# Find Views calling Daos (forbidden)
search_for_pattern "Dao_" in Module_**/*Views/*.cs

# Find hardcoded connection strings (forbidden)
search_for_pattern "Server=|localhost" in **/*.cs

# Find direct SQL queries (forbidden)
search_for_pattern "SELECT|INSERT|UPDATE|DELETE" excluding StoredProcedures in Module_**/*.cs
```

### Validate DAO Pattern

Ensure all DAOs follow `Model_Dao_Result` pattern:

```bash
# Find methods not returning Model_Dao_Result
find_symbol "Dao_*" in Module_**/Data/ with body

# Check each for proper error handling
```

### Monitor DI Registration

Validate all services registered in DI:

```bash
# Find ServiceLocator usage (deprecated pattern)
search_for_pattern "App\.GetService" in **/*.cs
```

---

## Additional Resources

- **Serena User Guide:** https://oraios.github.io/serena/02-usage/000_intro.html
- **Configuration Guide:** https://oraios.github.io/serena/02-usage/050_configuration.html
- **MCP Server Setup:** https://oraios.github.io/serena/02-usage/030_clients.html
- **Troubleshooting:** https://oraios.github.io/serena/03-special-guides/000_overview.html
- **GitHub Repository:** https://github.com/oraios/serena
- **Dashboard (Local):** http://127.0.0.1:24282/ (when server running)

---

## Validation Checklist

Before committing Serena-assisted refactoring:

- [ ] Build succeeds: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Architecture validated: `search_for_pattern` for violations
- [ ] Git diff reviewed: `git diff` shows intended changes only
- [ ] Memories updated: Project knowledge preserved in `.serena/memories/`
- [ ] No regressions: Manual testing of affected workflows

