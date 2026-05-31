---
applyTo: "**"
description: >
  Advanced Serena usage — prompting strategies, context management, custom agents (Agno),
  git worktrees, comparison with other agents, and MTM-specific coding workflows.
---

# Serena Advanced Usage

Sources:

- <https://oraios.github.io/serena/02-usage/999_additional-usage.html>
- <https://oraios.github.io/serena/03-special-guides/custom_agent.html>
- <https://oraios.github.io/serena/01-about/040_comparison-to-other-agents.html>

---

## Prompting Strategies

Effective prompting is critical to getting quality output from Serena. These strategies
apply specifically to the MTM project.

### Start Broad, Then Narrow

```
# Step 1: Architecture overview
"Get the symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs"

# Step 2: Read specific method
"Find the InsertReceivingLineAsync method with body"

# Step 3: Understand callers
"Find all symbols referencing InsertReceivingLineAsync"

# Step 4: Make the change
"Replace the body of InsertReceivingLineAsync with: [new implementation]"
```

### Tell Serena What to Read First

Always prime with relevant memories:

```
"Before making any changes, read the 'forbidden_practices' and 'dao_best_practices' memories"
```

### Scope Your Task

Smaller, focused tasks produce better results than large open-ended requests:

```
# Good - scoped
"Add a new method GetByStatusAsync to Dao_ReceivingLine that calls sp_ReceivingLine_GetByStatus.
 Use the same parameters-and-stored-procedure pattern as the existing GetByLoadAsync method."

# Risk - too broad
"Refactor all DAOs to use the new connection pooling strategy"
```

### Validate Architecture Before Ending

After each coding task:

```
"Search for any pattern: 'Dao_' in Module_Receiving/ViewModels/ - this would be a forbidden pattern"
"Verify the ViewModel only calls IService_ interfaces, not Dao_ classes directly"
```

---

## Context Window Management

Serena tasks can run long and fill the context window. Strategies to avoid this:

### Read Only What You Need

- Use `get_symbols_overview` before `find_symbol` with body
- Read one method at a time, not entire classes
- Use `find_referencing_symbols` to understand impact without reading all callers in full

### Split Long Sessions

After onboarding or a major exploration phase, start a fresh conversation:

```
"We've completed the analysis. Please write a summary to the memory 'task_summary'
and we'll continue in a new conversation."
```

### Summarize to Memory

If important discoveries were made mid-session, save them:

```
"Write to memory 'receive_workflow_constraints': [summary of what we discovered]"
```

### Use One-Shot Mode for Analysis

For analysis tasks where you want a complete answer without back-and-forth:

```bash
serena start-mcp-server --mode planning --mode one-shot
```

---

## Custom Agents with Serena (Agno Integration)

Official docs: <https://oraios.github.io/serena/03-special-guides/custom_agent.html>

Serena provides a reference integration with **Agno** (<https://docs.agno.com>), a
model-agnostic agent framework. This allows you to build custom AI agents that use
Serena's tools independently of the MCP protocol.

### What Agno + Serena provides

- Use any LLM (GPT-4, Claude, Gemini, local models)
- Fine-grained control over agent behavior
- Web-based agent UI
- Prefill conversation with system prompts

### Setup

```bash
# 1. Clone Agno's agent UI
npx create-agent-ui@latest
# or
git clone https://github.com/agno-agi/agent-ui.git
cd agent-ui && pnpm install && pnpm dev

# 2. Install Serena with optional Agno requirements
pip install "serena[agno]"
# or
uv tool install "git+https://github.com/oraios/serena[agno]"

# 3. Run the Serena agent
serena agno-playground --project /path/to/project
```

### Customizing the Agent

Provide a `serena_agent_config.yml`:

```yaml
# serena_agent_config.yml
project: C:\Users\johnk\source\repos\MTM_Receiving_Application
model_provider: "Claude"
model_id: "claude-sonnet-4-5"
instructions: |
  You are a WinUI 3 MVVM expert working on the MTM Receiving Application.
  Always follow the MVVM architecture rules.
  Never call DAOs directly from ViewModels.
```

---

## Comparison with Other Coding Agents

Official docs: <https://oraios.github.io/serena/01-about/040_comparison-to-other-agents.html>

### How Serena Differs

| Capability                    | Serena                  | grep_search / file_search        | Claude Code built-in |
| ----------------------------- | ----------------------- | -------------------------------- | -------------------- |
| Symbol-level navigation       | ✅ Language server      | ❌ Text search only              | ❌ Text search       |
| Find all usages               | ✅ True LSP references  | ⚠️ Regex-based (misses generics) | ⚠️ Text search       |
| Rename refactoring            | ✅ Safe, language-aware | ❌ Not available                 | ⚠️ Find & replace    |
| Memory/knowledge persistence  | ✅ Built-in             | ❌ Not built-in                  | ❌ Not built-in      |
| Token cost for symbol reading | ~90% savings            | Full file required               | Full file required   |

### When NOT to Use Serena

- **Single file, known location**: Standard `replace_string_in_file` is faster
- **Config / YAML files**: No symbol structure; use read_file
- **Quick grep**: `grep_search` is faster for simple text matches
- **Creating new files**: VS Code's file creation tools work fine

---

## MTM-Specific Coding Workflows

### Workflow 1: Add a New DAO Method

```
1. get_symbols_overview(Dao_ReceivingLine.cs)       → See current method list
2. find_symbol("GetByLoadAsync", include_body=true) → Read pattern to copy
3. insert_after_symbol(last_method)                 → Add new method using same pattern
4. search_for_pattern("sp_ReceivingLine_GetByStatus", "Database/StoredProcedures/")
   → Verify stored procedure exists before calling it
5. dotnet build                                     → Confirm it compiles
```

### Workflow 2: Add a New Service Method

```
1. find_symbol("IService_MySQL_ReceivingLine", depth=1) → See current interface
2. find_symbol("Service_MySQL_ReceivingLine/GetByLoadAsync", include_body=true) → Read pattern
3. insert_after_symbol(interface, new method signature)
4. insert_after_symbol(implementation class, new method body)
5. find_referencing_symbols("IService_MySQL_ReceivingLine") → Ensure no breaking changes
```

### Workflow 3: Refactor ViewModel Command

```
1. find_symbol("ViewModel_Receiving_Workflow/LoadDataAsync", include_body=true)
   → Read current implementation
2. read_memory("forbidden_practices") → Confirm what we can't do
3. replace_symbol_body("ViewModel_Receiving_Workflow/LoadDataAsync", new_impl)
4. dotnet build
5. search_for_pattern("Dao_", "Module_Receiving/ViewModels/") → Verify no DAO calls
```

### Workflow 4: Validate Architecture After Changes

```
# Run these searches after any module-level changes:

search_for_pattern("Dao_", "Module_*/ViewModels/")         → Should return 0 results
search_for_pattern("static.*Dao_", "**/*.cs")              → Should return 0 results
search_for_pattern("MessageBox\.Show", "Module_*/ViewModels/**") → Should return 0 results
search_for_pattern("INSERT|UPDATE|DELETE", "**/*.cs")
  excluding "Database/StoredProcedures/"                   → Should return 0 results
search_for_pattern("\{Binding ", "**/*.xaml")              → Should return 0 results
```

### Workflow 5: Understanding an Unknown Module

```
1. list_dir("Module_ShipRec_Tools/")                       → Understand folder structure
2. get_symbols_overview("Module_ShipRec_Tools/ViewModels/") → Class list
3. find_symbol("ViewModel_ShipRec_*", depth=1)             → All properties/methods
4. find_symbol("IService_ShipRec_*", depth=1)              → Service interfaces
5. read_memory("project_overview")                         → High-level module description
```

---

## Git Worktrees + Serena

When using `git worktree` to develop multiple branches simultaneously:

```bash
# Create a worktree
git worktree add ../MTM-feature-branch feature/new-receiving-mode

# Copy the symbol cache to the new worktree (avoid re-indexing)
Get-Item ".serena/cache" | Copy-Item -Destination "../MTM-feature-branch/.serena/cache" -Recurse

# Copy memories too (they're shared knowledge)
Get-Item ".serena/memories" | Copy-Item -Destination "../MTM-feature-branch/.serena/memories" -Recurse
```

Start a separate Serena instance for the worktree when needed:

```bash
cd ../MTM-feature-branch
serena start-mcp-server --context ide --project .
```

---

## Serena MCP SDK Note

Serena's MCP server is built on the **MCP Python SDK**
(<https://github.com/modelcontextprotocol/python-sdk>). If you experience MCP protocol
issues (e.g., with a custom client), verify MCP SDK compatibility:

```bash
uvx --from git+https://github.com/oraios/serena serena --version
```

The Serena version implies a specific MCP SDK version. Match this against your client's
supported MCP specification version.
