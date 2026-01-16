# Speckit Prepare - AI Agent Onboarding & Documentation Review

## Purpose

This prompt guides an AI agent through a comprehensive familiarization process with the MTM Receiving Application codebase, ensuring full understanding of architectural patterns, constitutional rules, and development standards before undertaking any implementation tasks.

## Core Directive

**Before proceeding with ANY implementation task, you must systematically read and internalize ALL project documentation, architectural guidelines, and constitutional principles.**

---

## Step 0: Activate Serena Project (REQUIRED FIRST)

**Before reading any documentation, activate the Serena MCP server project to enable efficient semantic code navigation.**

**Action Required:**

```
# Activate the MTM Receiving Application project
mcp_oraios_serena_activate_project("MTM_Receiving_Application")

# Check if onboarding was already performed
mcp_oraios_serena_check_onboarding_performed()

# If not performed, run onboarding to create project memories
mcp_oraios_serena_onboarding()
```

**Why Serena First:**

- **80-90% token savings** when exploring codebase
- **Semantic navigation** - Read specific symbols instead of entire files
- **Project memories** - Access architectural patterns without re-reading docs
- **Symbol-level precision** - Understand code structure efficiently

---

## Step 1: Read Serena Memories (MANDATORY - Most Token-Efficient)

**Serena creates project memories that contain distilled knowledge from core documents and instruction files. Read these FIRST to avoid redundant file reading.**

**Available Memories:**

- `project_overview` - Project purpose, features, architecture overview
- `constitution_summary` - Constitutional principles v1.2.0, forbidden practices
- `architectural_patterns` - MVVM layers, Service→DAO delegation, instance-based DAOs
- `dao_best_practices` - DAO patterns, Model_Dao_Result usage, DI registration
- `mvvm_guide` - ViewModel/View patterns, x:Bind usage, CommunityToolkit.Mvvm
- `coding_standards` - Naming conventions, file organization
- `forbidden_practices` - Critical violations to avoid (ViewModels calling DAOs, etc.)
- `infor_visual_constraints` - READ-ONLY database rules
- `error_handling_guide` - IService_ErrorHandler usage, logging standards
- `tech_stack` - .NET 8, WinUI 3, MySQL, libraries
- `task_completion_workflow` - Build/test/documentation workflow
- `suggested_commands` - Common PowerShell commands

**Action Required:**

```
# List all available memories
mcp_oraios_serena_list_memories()

# Read ALL memories (80-90% token savings vs reading source files)
mcp_oraios_serena_read_memory("project_overview")
mcp_oraios_serena_read_memory("constitution_summary")
mcp_oraios_serena_read_memory("architectural_patterns")
mcp_oraios_serena_read_memory("dao_best_practices")
mcp_oraios_serena_read_memory("mvvm_guide")
mcp_oraios_serena_read_memory("coding_standards")
mcp_oraios_serena_read_memory("forbidden_practices")
mcp_oraios_serena_read_memory("infor_visual_constraints")
mcp_oraios_serena_read_memory("error_handling_guide")
mcp_oraios_serena_read_memory("tech_stack")
mcp_oraios_serena_read_memory("task_completion_workflow")
mcp_oraios_serena_read_memory("suggested_commands")
```

**Token Savings:** Reading 12 memories (~3,000 tokens total) vs reading 40+ instruction files and core docs (~50,000+ tokens) = **94% reduction**

---

## Step 2: Read Instruction Files NOT Covered by Memories (Selective)

**Most architectural patterns are already in Serena memories. Only read instruction files for:**

- Task-specific guidance not in memories (e.g., window-sizing, XAML troubleshooting)
- Detailed implementation examples needed for your specific task
- Files referenced by user attachments or task requirements

**Task-specific guidance not in memories (e.g., window-sizing, XAML troubleshooting)**

- Detailed implementation examples needed for your specific task
- Files referenced by user attachments or task requirements

**Files Covered by Memories (DO NOT re-read):**

- ✅ `.github/copilot-instructions.md` → In `project_overview`, `forbidden_practices`
- ✅ `.specify/memory/constitution.md` → In `constitution_summary`
- ✅ `AGENTS.md` → In `project_overview`, `tech_stack`
- ✅ `.github/instructions/dao-pattern.instructions.md` → In `dao_best_practices`
- ✅ `.github/instructions/mvvm-pattern.instructions.md` → In `mvvm_guide`
- ✅ `.github/instructions/mvvm-viewmodels.instructions.md` → In `mvvm_guide`
- ✅ `.github/instructions/mvvm-views.instructions.md` → In `mvvm_guide`
- ✅ `.github/instructions/error-handling.instructions.md` → In `error_handling_guide`
- ✅ `.github/instructions/architecture-refactoring-guide.instructions.md` → In `architectural_patterns`
- ✅ `.github/instructions/service-dao-pattern.instructions.md` → In `architectural_patterns`
- ✅ `.github/instructions/dao-instance-pattern.instructions.md` → In `dao_best_practices`
- ✅ `.github/instructions/constitution-compliance.instructions.md` → In `constitution_summary`
- ✅ `.github/instructions/infor-visual-integration.instructions.md` → In `infor_visual_constraints`

**Files to Read Only If Task-Relevant:**

```
# List all instruction files to see what's available
mcp_oraios_serena_list_dir(".github/instructions", recursive=false, skip_ignored_files=true)

# Read only task-specific files (examples)
read_file(".github/instructions/window-sizing.instructions.md", 1, -1)  # If creating UI
read_file(".github/instructions/xaml-troubleshooting.instructions.md", 1, -1)  # If XAML errors
read_file(".github/instructions/unit-testing-standards.instructions.md", 1, -1)  # If writing tests
read_file(".github/instructions/pagination-services.instructions.md", 1, -1)  # If implementing pagination
```

**Efficiency Principle:** Memories provide 80-90% of architectural knowledge. Only read instruction files when you need task-specific details not covered by memories.

---

## Step 3: Read Linked Documentation & Referenced Files

 (using Serena):**

```
# Example: If instruction file references Documentation/REUSABLE_SERVICES.md
read_file("Documentation/REUSABLE_SERVICES.md", 1, -1)

# Search for architectural pattern references
mcp_oraios_serena_search_for_pattern(
    substring_pattern=r"See (Documentation|specs)/[^\s]+",
    relative_path=".github/instructions",
    context_lines_before=1,
    context_lines_after=1
)
```

**Efficiency Steps:**

1. Track all file references encountered in instructions (e.g., "See Documentation/REUSABLE_SERVICES.md")
2. Read each referenced file completely using `read_file`
**Common Referenced Documentation Locations:**

- `Documentation/` - Business logic, workflows, API documentation, troubleshooting
- `specs/` - Feature specifications, implementation plans, task lists
- `Database/` - Schema documentation, stored procedure standards
- `.specify/memory/` - Project memory files, architecture decisions

**Action Required:**

1. Track all file references encountered in instructions (e.g., "See Documentation/REUSABLE_SERVICES.md")
2. Read each referenced file completely
3. Follow any additional references found in those files (recursive documentation reading)
4. Build a mental model of how different documentation pieces connect

---

## Step 4: Read Files Attached to the Prompt Command

When you receive a task via a `/{prompt}` command, the user may attach additional context files or folders.

**Action Required:**

1. Check foLeverage Serena Memories for Architectural Knowledge

**After onboarding, Serena creates project memories containing architectural patterns. Read these to quickly understand the codebase without re-reading source files.**

**Action Required:**

```
# List available memories
mcp_oraios_serena_list_memories()

# Read relevant memories
mcp_oraios_serena_read_memory("architectural_patterns.md")
mcp_oraios_serena_read_memory("dao_best_practices.md")
mcp_oraios_serena_read_memory("mvvm_guide.md")
mcp_oraios_serena_read_memory("coding_standards.md")
```

**Token Savings:** Reading memories (~500-1000 tokens) vs re-reading source files (~5000-10000 tokens) = **80-90% reduction**

---

## Step 6: Understand Architectural Patterns & Constraints

After reading all documentation and memoriesd ALL files within that folder
4. Understand how attached context relates to the task at hand
5. Identify any task-specific constraints or requirements from attachments

**Examples of Attachments:**

- `#file:spec.md` - Feature specification
- `#file:plan.md` - Implementation plan
- `#folder:specs/008-dunnage-wizard-ui/` - Entire feature specification folder
- `#file7: Verify Understanding Using Serena Semantic Tools

**Use Serena to validate your understanding by exploring code examples without reading entire files.**

**Validation Actions:**

```
# 1. Explore a DAO to understand structure (200-300 tokens vs 5000+)
mcp_oraios_serena_get_symbols_overview("Data/Receiving/Dao_ReceivingLine.cs", depth=1)

# 2. Read specific DAO method to see Model_Dao_Result pattern (100-200 tokens)
mcp_oraios_serena_find_symbol(
    name_path_pattern="Dao_ReceivingLine/GetAllAsync",
    relative_path="Data/Receiving/Dao_ReceivingLine.cs",
    include_body=true
)

# 3. Check ViewModel structure (300-400 tokens)
mcp_oraios_serena_get_symbols_overview("ViewModels/Receiving/ReceivingViewModel.cs", depth=1)

# 4. Validate Service_ErrorHandler usage pattern
mcp_oraios_serena_find_symbol(
    name_path_pattern="Service_ErrorHandler/ShowUserError",
    relative_path="Services/Service_ErrorHandler.cs",
    include_body=true
)

# 5. Search for architectural violations
mcp_oraios_serena_search_for_pattern(
    substring_pattern=r"MessageBox\.Show",
    restrict_search_to_code_files=true,
    context_lines_before=2,
    cont9: Identify Task-Specific Documentation Needs

Based on your assigned task, identify which additional documentation you should prioritize using Serena

**Token Efficiency:** ~1000-2000 tokens to validate understanding vs ~20,000+ tokens reading full source files

---

## Step 8Model_ReceivingLine.cs` - Existing code for reference

---

## Step 5: Understand Architectural Patterns & Constraints

After reading all documentation, synthesize your understanding of:

### Critical Architecture Patterns
- **MVVM Architecture:** Strict layer separation (ViewModels → Services → DAOs)
- **Dependency Injection:** All services and DAOs registered in App.xaml.cs
- **Instance-Based DAOs:** No static DAOs, all must use DI with connection string injection
- **Model_Dao_Result Pattern:** All database operations return structured results
- **Error Handling:** Never throw from DAOs, always use IService_ErrorHandler in ViewModels

### Non-Negotiable Constraints
- **Infor Visual is READ ONLY:** No INSERT/UPDATE/DELETE on SQL Server database
```

# Read specs efficiently

read_file("specs/XXX-feature-name/spec.md", 1, -1)
read_file("specs/XXX-feature-name/plan.md", 1, -1)

# Explore related code using Serena

mcp_oraios_serena_get_symbols_overview("Services/Receiving/Service_MyFeature.cs", depth=1)
mcp_oraios_serena_find_symbol("Service_MyFeature/MyMethod", include_body=true)

```

### For Bug Fixes:
```

# Read troubleshooting docs

read_file("Documentation/Troubleshooting/COMMON_ISSUES.md", 1, -1)

# Find error patterns in code

mcp_oraios_serena_search_for_pattern(
    substring_pattern=r"Service_ErrorHandler\.HandleException",
    context_lines_before=3,
    context_lines_after=3
)

```
10: Confirm Readiness to Proceed

**BEFORE starting implementation, you MUST:**

✅ Confirm Serena project is activated and onboarding complete
✅ Confirm you have read ALL Serena memories (12 total)
✅ Confirm you understand files covered by memories (no need to re-read)
✅ Confirm you have read task-relevant instruction files NOT covered by memories
✅ Confirm you have read all documentation referenced in instruction files
✅ Confirm you have read all files attached to the prompt command
✅ Confirm you understand the architectural patterns and constraints (validated via Serena exploration)
✅ Confirm you can identify forbidden practices and avoid them
✅ Confirm you know which tools to use: **Serena for code exploration, standard tools for file creation**
    substring_pattern=r"static.*Dao_",
    restrict_search_to_code_files=true
)
```

## Step 7: Identify Task-Specific Documentation Needs

Based on your assigned task, identify which additional documentation you should prioritize:

### For New Feature Implementation

- Read the feature specification in `specs/XXX-feature-name/spec.md`
- Review the implementation plan in `specs/XXX-feature-name/plan.md`
- Check task status in `specs/XXX-feature-name/tasks.md`
- Review related service documentation in `Documentation/`

### For Bug Fixes

- Review troubleshooting guides in `Documentation/Troubleshooting/`
- Check error handling standards in `.github/instructions/error-handling.instructions.md`
- Review logging standards in `.github/instructions/logging-standards.instructions.md`

### For Refactoring

- Review architecture refactori (Using Serena MCP Server)

## Serena Setup

- ✅ Project activated: MTM_Receiving_Application
- ✅ Onboarding complete: [X] memories created
- ✅ Memories read: architectural_patterns.md, dao_best_practices.md, mvvm_guide.md

## Documentation Read

- ✅ Core project documents (copilot-instructions.md, constitution.md, AGENTS.md)
- ✅ All [X] instruction files in .github/instructions/
- ✅ [List any additional referenced documentation read]
- ✅ [List any attached files/folders read]

## Serena Code Exploration (Token-Efficient Validation)

- ✅ Explored [X] DAO files using get_symbols_overview (~300 tokens vs ~5000)
- ✅ Validated Service_ErrorHandler usage patterns (~200 tokens)
- ✅ Checked ViewModel structure (~400 tokens)
- ✅ Searched for architectural violations (MessageBox.Show, static DAOs)

## Key Constraints Understood

- [List 3-5 most relevant constraints for the task]

## Token EfficiSerena activation** - Must activate project FIRST to enable semantic tools

❌ **DON'T skip onboarding** - Creates critical project memories for efficient navigation
❌ **DON'T read entire files when exploring code** - Use `get_symbols_overview` and `find_symbol` instead
❌ **DON'T skip instruction files** - Even seemingly unrelated files may contain critical constraints
❌ **DON'T skim documentation** - Read completely to understand nuances and edge cases
❌ **DON'T assume patterns from other projects** - This project has strict, specific patterns
❌ **DON'T start coding before understanding** - Premature implementation leads to rework
❌ **DON'T ignore constitutional principles** - These are non-negotiable and must be followed
❌ **DON'T use forbidden patterns** - Check constitution.md forbidden practices list
❌ **DON'T forget to read Serena memories** - They contain distilled architectural knowledge

**BEFORE starting implementation, you MUST:**

✅ Confirm you have read `.github/copilot-instructions.md`, `.specify/memory/constitution.md`, and `AGENTS.md`
✅ C**Serena Setup:** Project activated, onboarding complete, memories read
2. **Architecture:** You can describe the MVVM architecture layers using Serena-explored examples
3. **Database:** You can explain MySQL vs Infor Visual patterns (validated via code exploration)
4. **Forbidden Practices:** You can identify at least 5 forbidden practices from constitution
5. **Code Structure:** You can describe ViewModel, Service, DAO, View creation (verified via `get_symbols_overview`)
6. **Tools:** You understand when to use Serena vs standard tools (code exploration vs file creation)
7. **Documentation:** You have read all instruction files relevant to your task
8. **Mental Model:** You have a clear understanding of project structure from Serena exploration
9. **Implementation Readiness:** You can confidently state what to implement and how to do it correctly
10. **Token Efficiency:** You achieved 80-90% token savings using Serena vs traditional file reading
**Once all confirmations are complete, respond to the user:**

```
✅ Documentation Review Complete

I have familiarized myself with:
- CSerena MCP Server Setup

### Project Activation
- ✅ Activated project: MTM_Receiving_Application
- ✅ Onboarding status: Complete
- ✅ Memories created: [X] files

### Memories Read (Token-Efficient Knowledge)
- ✅ architectural_patterns.md (~300 tokens)
- ✅ dao_best_practices.md (~250 tokens)
- ✅ mvvm_guide.md (~400 tokens)
- ✅ coding_standards.md (~200 tokens)
- ✅ [List other memories read]

## Documentation Read

### Core Documents (3/3)
- ✅ .github/copilot-instructions.md
- ✅ .specify/memory/constitution.md (Version X.X.X)
- ✅ AGENTS.md
- ✅ .github/instructions/serena-semantic-tools.instructions.md

### Instruction Files ([X]/[Total])
- ✅ [List all instruction files read]

### Referenced Documentation
- ✅ [List all referenced docs read]

### Attached Files (from prompt)
- ✅ [List all attached files/folders read]

## Serena Code Exploration (Validation)

### DAOs Explored (~300 tokens each vs ~5000)
- ✅ Dao_ReceivingLine.cs - Structure and patterns validated
- ✅ [Other DAOs explored]

### Services Explored (~400 tokens each)
- ✅ Service_ErrorHandler.cs - Error handling pattern confirmed
- ✅ [Other services explored]

### ViewModels Explored (~400 tokens each)
- ✅ ReceivingViewModel.cs - MVVM pattern validated
- ✅ [Other ViewModels explored]

### Architectural Searches
- ✅ MessageBox.Show violations: [X] found
- ✅ Static DAO violations: [X] found
- ✅ Model_Dao_Result usage: Verified across DAOs
---

## Common Pitfalls to Avoid

❌ **DON'T skip Serena activation** - Must activate project FIRST to enable semantic tools
❌ **DON'T skip onboarding** - Creates critical project memories for efficient navigation
❌ **DON'T skip reading memories** - They contain 80-90% of architectural knowledge
❌ **DON'T re-read files covered by memories** - Wastes tokens unnecessarily
❌ **DON'T read entire files when exploring code** - Use `get_symbols_overview` and `find_symbol` instead
❌ **DON'T skip task-relevant instruction files** - May contain critical constraints for your specific work
❌ **DON'T skim documentation** - Read completely to understand nuances and edge cases
❌ **DON'T assume patterns from other projects** - This project has strict, specific patterns
❌ **DON'T start coding before understanding** - Premature implementation leads to rework
❌ **DON'T ignore constitutional principles** - These are non-negotiable and must be followed
❌ **DON'T use forbidden patterns** - Check forbidden_practices memory

---

## Success Criteria

You have successfully prepared when:

### Serena Setup
- ✅ Project activated and onboarding complete
- ✅ All Serena memories read (architectural_patterns.md, etc.)
- ✅ Serena semantic tools understood (get_symbols_overview, find_symbol, etc.)

### Documentation
- ✅ All core documents read
- ✅ All instruction files read
- ✅ All referenced documentation read
- ✅ All attached files read

### Code Understanding (via Serena)
- ✅ DAO structure explored and validated (~80-90% token savings)
- ✅ Service patterns explored and validated
- ✅ ViewModel patterns explored and validated
- ✅ Architectural violations searched and identified
## Serena Workflow Summary

**Efficient Preparation Workflow Using Serena MCP Server:**

1. **Activate** → `mcp_oraios_serena_activate_project("MTM_Receiving_Application")`
2. **Onboard** → `mcp_oraios_serena_onboarding()` (first time only)
3. **Read Memories FIRST** → `mcp_oraios_serena_list_memories()` + `mcp_oraios_serena_read_memory()` (ALL 12 memories)
4. **Skip Redundant Files** → Do NOT re-read core docs or instruction files covered by memories
5. **Read Task-Specific Docs** → Only instruction files needed for your specific task
6. **Explore Code** → Use `get_symbols_overview` and `find_symbol` (80-90% token savings)
7. **Validate Patterns** → Use `search_for_pattern` for architectural compliance
8. **Ready to Code** → Proceed with implementation using proper tools

**Token Efficiency Comparison:**

| Task | Traditional | Serena Memories | Savings |
|------|------------|-----------------|---------|
| Read architectural docs | ~50,000 tokens | ~3,000 tokens | 94% |
| Explore 5 DAOs (500 lines each) | ~25,000 tokens | ~1,500 tokens | 94% |
| Find method usages | Manual search | `find_referencing_symbols` | 95% |
| Validate architecture | Read all files | `search_for_pattern` | 90% |
| Understand class structure | Read full file | `get_symbols_overview` | 95% |
| **TOTAL PREPARATION** | **~75,000 tokens** | **~5,000 tokens** | **93%** |

**Remember:** Time spent understanding the architecture now prevents hours of rework later. This project has strict patterns that MUST be followed - cutting corners on preparation leads to code that violates constitutional principles and must be refactored.

**Using Serena MCP server, you can achieve this understanding with 80-90% fewer tokens while gaining deeper semantic insights into the codebase.**

**Your job is to build it right the first time, following all established patterns and constraints - and Serena makes this efficient
- ✅ Forbidden practices identified (searched in codebase)
- ✅ Tool usage rules understood (Serena for exploration, standard for creation)
- ✅ Task requirements clear

### Efficiency Metrics
- ✅ Token usage: ~[X] tokens (vs ~[Y] traditional approach)
- ✅ Savings: ~[Z]% reduction using Serena

## Template Response (Use This Format)

```markdown
# MTM Receiving Application - Preparation Complete

## Serena MCP Server Setup

### Project Activation
- ✅ Activated project: MTM_Receiving_Application
- ✅ Onboarding status: Complete
- ✅ Memories created: [X] files

### Memories Read (Token-Efficient Knowledge)
- ✅ project_overview (~300 tokens)
- ✅ constitution_summary (~800 tokens)
- ✅ architectural_patterns (~400 tokens)
- ✅ dao_best_practices (~600 tokens)
- ✅ mvvm_guide (~500 tokens)
- ✅ coding_standards (~200 tokens)
- ✅ forbidden_practices (~400 tokens)
- ✅ infor_visual_constraints (~300 tokens)
- ✅ error_handling_guide (~400 tokens)
- ✅ tech_stack (~200 tokens)
- ✅ task_completion_workflow (~150 tokens)
- ✅ suggested_commands (~100 tokens)

## Documentation Read

### Files Covered by Memories (Skipped for Efficiency)
- ⏩ .github/copilot-instructions.md → Covered by `project_overview`, `forbidden_practices`
- ⏩ .specify/memory/constitution.md → Covered by `constitution_summary`
- ⏩ AGENTS.md → Covered by `project_overview`, `tech_stack`
- ⏩ .github/instructions/dao-pattern.instructions.md → Covered by `dao_best_practices`
- ⏩ .github/instructions/mvvm-pattern.instructions.md → Covered by `mvvm_guide`
- ⏩ .github/instructions/error-handling.instructions.md → Covered by `error_handling_guide`
- ⏩ [List other files skipped due to memory coverage]

### Task-Relevant Instruction Files ([X] files read)
- ✅ [List only instruction files NOT covered by memories that were read for this task]

### Referenced Documentation
- ✅ [List all referenced docs read]

### Attached Files (from prompt)
- ✅ [List all attached files/folders read]

## Serena Code Exploration (Validation)

### DAOs Explored (~300 tokens each vs ~5000)
- ✅ Dao_ReceivingLine.cs - Structure and patterns validated
- ✅ [Other DAOs explored]

### Services Explored (~400 tokens each)
- ✅ Service_ErrorHandler.cs - Error handling pattern confirmed
- ✅ [Other services explored]

### ViewModels Explored (~400 tokens each)
- ✅ ReceivingViewModel.cs - MVVM pattern validated
- ✅ [Other ViewModels explored]

### Architectural Searches
- ✅ MessageBox.Show violations: [X] found
- ✅ Static DAO violations: [X] found
- ✅ Model_Dao_Result usage: Verified across DAOs

## Key Constraints Identified

1. **[Constraint Category]:** [Brief description]
2. **[Constraint Category]:** [Brief description]
3. **[Constraint Category]:** [Brief description]
4. **[Constraint Category]:** [Brief description]
5. **[Constraint Category]:** [Brief description]

## Forbidden Practices (Will NOT Use)

- ❌ [Forbidden practice 1]
- ❌ [Forbidden practice 2]
- ❌ [Forbidden practice 3]

## Task Understanding

**Task:** [Brief description of the assigned task]

**Acceptance Criteria:**
- [ ] [Criterion 1]
- [ ] [Criterion 2]
- [ ] [Criterion 3]

**Implementation Approach:**
[2-3 sentence summary of how you plan to approach the task while following all constraints]

## Readiness Checklist

- ✅ Serena activated and onboarding complete
- ✅ All 12 Serena memories read (~3,000 tokens)
- ✅ Files covered by memories identified (skipped for efficiency)
- ✅ Task-relevant instruction files read
- ✅ Referenced documentation read
- ✅ Attached files read
- ✅ Architectural patterns validated via Serena exploration
- ✅ Forbidden practices identified
- ✅ Tool usage rules understood
- ✅ Task requirements clear

## Efficiency Metrics

- ✅ Token usage: ~[X] tokens (vs ~[Y] traditional approach)
- ✅ Savings: ~93% reduction using Serena memories
- ✅ Preparation time: [X] minutes (vs ~[Y] hours traditional)

**Status:** ✅ READY TO PROCEED

Awaiting user confirmation to begin implementation.
```

---

**Remember:** Time spent understanding the architecture now prevents hours of rework later. This project has strict patterns that MUST be followed - cutting corners on preparation leads to code that violates constitutional principles and must be refactored.

**Using Serena MCP server, you can achieve this understanding with 80-90% fewer tokens while gaining deeper semantic insights into the codebase.**

**Your job is to build it right the first time, following all established patterns and constraints - and Serena makes this efficient.**
