---
description: 'New Chat Frame Initialization Prompt - Use this when starting a new chat session'
applyTo: 'Agent Initialization'
---

# MTM Receiving Application - New Chat Frame Initialization

**Use this prompt when opening a new chat frame to familiarize the agent with all project instructions, BMAD commands, and project context.**

---

## Phase 1: Self-Initialization (Agent Must Execute Using Serena Tools)

You are a specialized development agent for the MTM Receiving Application. Your first action MUST be to initialize yourself using Serena tools (to conserve tokens). Follow this sequence exactly:

### Step 1: Check Onboarding Status
```
oraios_serena_check_onboarding_performed()
```
If onboarding not done, call:
```
oraios_serena_onboarding()
```

### Step 2: Read Core Memory Bank Files (in order)
Use `oraios_serena_read_memory()` to read these files:
1. `projectbrief.md` - Project scope and goals
2. `techContext.md` - Technology stack and constraints
3. `systemPatterns.md` - Architecture patterns and decisions
4. `productContext.md` - Why the project exists and user goals
5. `activeContext.md` - Current work focus and decisions
6. `progress.md` - What's complete, what remains
7. `MTM_Naming_Convention_Refactoring_Progress` - Naming standards (CRITICAL - Module_Receiving is law)

### Step 3: Read Constitution and Agent Files
Read these critical files using `get_file()`:
1. `.github/copilot-instructions.md` - Core development guidelines (contains naming conventions, MVVM patterns, code quality standards)
2. `.github/AGENTS.md` - Specialized AI agent definitions
3. `AGENTS.md` - MTM Receiving Specialist agent role and responsibilities

### Step 4: Scan Instruction Files Directory
Use `oraios_serena_find_file()` to list all files in `.github/instructions/`:
```
oraios_serena_find_file("*.instructions.md", ".github/instructions")
```

Then read the most relevant instruction files for your task:
- `csharp.instructions.md` - C# best practices
- `testing-strategy.instructions.md` - Testing guidelines
- `dotnet-architecture-good-practices.instructions.md` - Architecture patterns
- `memory-bank.instructions.md` - Memory bank workflow
- `serena-tools.instructions.md` - Serena tooling reference
- `taming-copilot.instructions.md` - Copilot interaction patterns

**Read others on-demand based on your specific task.**

### Step 5: Understand BMAD Command Structure
BMAD is the Build Master Architecture Docker system available in the `_bmad/` directory. Key command categories:
- **Agent Building**: `./_bmad/bmb/workflows/agent/` - AI agent creation and validation
- **Module Building**: `./_bmad/bmb/workflows/create-module/` - Creating new modules
- **Workflow Building**: `./_bmad/bmb/workflows/create-workflow/` - Workflow creation
- **Architecture**: `./_bmad/bmm/workflows/` - Full system architecture workflows

When you need to reference BMAD:
1. Use `oraios_serena_find_file()` to discover relevant BMAD files in `_bmad/`
2. Read the appropriate workflow documentation
3. Reference the data folder for standards and templates

### Step 6: Verify Module_Receiving Standards
This is CRITICAL - Module_Receiving is the gold standard. Read the naming convention memory:
```
oraios_serena_read_memory("MTM_Naming_Convention_Refactoring_Progress")
```

**Key Pattern:** `{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **ViewModels**: `ViewModel_Receiving_Wizard_Display_PONumberEntry` ✅
- **DAOs**: `Dao_Receiving_Repository_WorkflowSession` ✅
- **Services**: `Service_Receiving_Business_MySQL_ReceivingLine` ✅
- **Models**: `Model_Receiving_Entity_ReceivingLoad` ✅

---

## Phase 2: Project Context Familiarization

### Technology Stack
- **Framework**: WinUI 3 (Windows App SDK 1.6+)
- **Language**: C# 12
- **Platform**: .NET 8
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Database**: MySQL 8.0 (READ/WRITE via stored procedures), SQL Server/Infor Visual (READ ONLY)
- **Testing**: xUnit with FluentAssertions
- **DI**: Microsoft.Extensions.DependencyInjection

### Project Modules
- `Module_Core` - Shared infrastructure and base classes
- `Module_Receiving` - Receiving workflow (fully refactored to 5-part naming standard)
- `Module_Dunnage` - Dunnage management
- `Module_Routing` - Routing rules
- `Module_Reporting` - Report generation
- `Module_Settings` - Configuration UI
- `Module_Volvo` - Volvo-specific integration
- `Module_Shared` - Shared ViewModels and utilities

### Critical Architecture Rules
**FORBIDDEN:**
- ❌ ViewModels calling DAOs directly (MUST go through Service layer)
- ❌ ViewModels accessing `Helper_Database_*` classes
- ❌ Static DAOs (all instance-based)
- ❌ DAOs throwing exceptions (return `Model_Dao_Result`)
- ❌ Raw SQL in C# for MySQL (use stored procedures ONLY)
- ❌ Write operations to SQL Server/Infor Visual (READ ONLY)
- ❌ Runtime `{Binding}` in XAML (use `x:Bind` only)
- ❌ Business logic in `.xaml.cs` code-behind

**REQUIRED:**
- ✅ MVVM Layer Flow: View → ViewModel → Service → DAO → Database
- ✅ ViewModels: `partial` classes inheriting from `ViewModel_Shared_Base`
- ✅ Services: Interface-based with dependency injection
- ✅ DAOs: Instance-based, return `Model_Dao_Result`
- ✅ XAML: Use `x:Bind` with explicit `Mode`
- ✅ Async: All methods end with `Async` suffix
- ✅ Error Handling: DAOs return errors, Services handle, ViewModels display

---

## Phase 3: Ready to Work

After completing initialization, you are ready to:
1. ✅ Understand the full project scope and constraints
2. ✅ Follow all naming conventions (Module_Receiving is authoritative)
3. ✅ Implement features following MVVM patterns
4. ✅ Reference appropriate instruction files as needed
5. ✅ Use BMAD workflows when needed
6. ✅ Update memory bank as work progresses

---

## Quick Reference Commands for This Session

**Check Memory Bank:**
```
oraios_serena_list_memories()
```

**Read Specific Memory:**
```
oraios_serena_read_memory("{memory_name}")
```

**Find Instruction Files:**
```
oraios_serena_find_file("*.instructions.md", ".github/instructions")
```

**Find BMAD Resources:**
```
oraios_serena_find_file("*.md", "_bmad/bmb/workflows")
```

**Get Project Overview:**
```
get_projects_in_solution()
```

**Search Code Patterns:**
```
oraios_serena_search_for_pattern("{pattern}", "{relative_path}")
```

---

## Common Tasks & Where to Find Guidance

| Task | Documentation |
|------|---|
| Create new ViewModel | `.github/copilot-instructions.md` - ViewModel Pattern section |
| Create new Service | `.github/copilot-instructions.md` - Service Layer Pattern section |
| Create new DAO | `.github/copilot-instructions.md` - DAO Pattern section |
| Add new XAML View | `.github/copilot-instructions.md` - XAML Binding Pattern section |
| Write Unit Tests | `.github/copilot-instructions.md` - Unit Test Pattern section |
| Understand Architecture | `memory-bank/systemPatterns.md` + `.github/copilot-instructions.md` |
| Check Naming Standards | `memory-bank/MTM_Naming_Convention_Refactoring_Progress` |
| Database Operations | `.github/instructions/sql-sp-generation.instructions.md` |
| Code Quality | `.github/instructions/object-calisthenics.instructions.md` |
| Testing Strategy | `.github/instructions/testing-strategy.instructions.md` |
| Create New Agent | `_bmad/bmb/workflows/agent/workflow.md` |
| Create New Module | `_bmad/bmb/workflows/create-module/workflow.md` |

---

## Initialization Checklist

Before proceeding with any task, the agent should have:
- [ ] Run Phase 1 initialization using Serena tools
- [ ] Read all Phase 1 files (memory bank + constitutions)
- [ ] Understood the 5-part naming convention (Module_Receiving is law)
- [ ] Reviewed architecture rules (FORBIDDEN/REQUIRED lists)
- [ ] Confirmed understanding of technology stack
- [ ] Located relevant instruction files for the task
- [ ] Identified which BMAD workflows might apply

---

## Token Conservation Tips

1. **Use Serena tools** instead of web searches - much more token efficient
2. **Read memory files** instead of regenerating context
3. **Reference instruction files** rather than creating custom guidance
4. **Reuse existing patterns** from Module_Receiving rather than inventing new ones
5. **Batch multiple file reads** in single Serena calls when possible
6. **Check memory bank first** before asking for clarification
7. **Use task tracking** in `memory-bank/tasks/` for long-running work

---

**IMPORTANT:** After completing this initialization, you are fully context-aware. Do not re-read files unnecessarily. Reference the memory bank and instruction files as needed for specific tasks.
