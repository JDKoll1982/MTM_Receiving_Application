# GitHub Copilot Prompts - Memory Bank & Testing

This directory contains reusable GitHub Copilot prompt files for Memory Bank management and comprehensive test generation in the MTM Receiving Application.

## 📁 Prompt Files

### Memory Bank Commands

| Prompt File | Command | Purpose | Agent |
|------------|---------|---------|-------|
| `memory-bank-update.prompt.md` | `/memory-bank-update` | Review and update all Memory Bank files to capture current project state | `agent` |
| `memory-bank-create-task.prompt.md` | `/memory-bank-create-task` | Create new task with thought process, implementation plan, and tracking | `agent` |
| `memory-bank-update-task.prompt.md` | `/memory-bank-update-task` | Update existing task with progress, status changes, and decisions | `agent` |
| `memory-bank-show-tasks.prompt.md` | `/memory-bank-show-tasks` | Display filtered list of tasks with status and completion info | `ask` |

### Testing Commands

| Prompt File | Command | Purpose | Agent |
|------------|---------|---------|-------|
| `mtm-generate-all-tests.prompt.md` | `/mtm-generate-all-tests` | Generate comprehensive xUnit tests for single file or entire module | `agent` |

## 🎯 Quick Start

### Memory Bank Commands

**Update Memory Bank After Work:**
```
/memory-bank-update
```
Reviews ALL memory bank files and updates with current project state, recent changes, and next steps.

**Create a New Task:**
```
/memory-bank-create-task Implement LoggingBehaviorTests
```
Creates task file with unique ID, thought process, implementation plan, and subtask tracking.

**Update Task Progress:**
```
/memory-bank-update-task TASK001: completed subtasks 1.8 and 1.9
```
Adds progress log entry, updates subtask status, recalculates completion percentage.

**View Tasks:**
```
/memory-bank-show-tasks active
```
Displays filtered task list (filters: all, active, pending, completed, blocked, recent).

### Testing Commands

**Generate Tests for Single File:**
```
/mtm-generate-all-tests Module_Core/Behaviors/LoggingBehavior.cs
```

**Generate Tests for Entire Module:**
```
/mtm-generate-all-tests Module_Core
```

**Generate Tests with Focus:**
```
/mtm-generate-all-tests Module_Core focus:Behaviors
```

## 📖 Memory Bank System

The Memory Bank provides persistent project context across sessions with these core files:

### Core Files (Required)
1. **`projectbrief.md`** - Project scope, technology stack, success criteria
2. **`productContext.md`** - Why project exists, problems solved, user goals
3. **`systemPatterns.md`** - Architecture, design patterns, testing patterns
4. **`techContext.md`** - Technologies, dependencies, constraints
5. **`activeContext.md`** - Current work focus, recent changes, next steps
6. **`progress.md`** - What works, milestones, known issues
7. **`tasks/` folder** - Individual task files with complete history

### Memory Bank Hierarchy

```
projectbrief.md (foundation)
├── productContext.md
├── systemPatterns.md
└── techContext.md
    └── activeContext.md
        ├── progress.md
        └── tasks/
            ├── _index.md
            └── TASKXXX-taskname.md
```

## 🎨 Prompt File Standards

All prompt files in this directory follow these standards from `.github/instructions/prompt.instructions.md`:

### Required Frontmatter
```yaml
---
name: 'command-name'
description: 'Brief description (single sentence)'
agent: 'ask|edit|agent'
tools: ['tool1', 'tool2']
argument-hint: 'Hint for user input'
---
```

### Structure
- **Mission** - Clear objective
- **Scope & Preconditions** - When to use, prerequisites
- **Inputs** - Required and optional parameters
- **Workflow** - Step-by-step execution
- **Output Expectations** - Format, location, success criteria
- **Quality Assurance** - Validation checklist
- **Example Usage** - Practical examples

## 🔧 Tool Usage

### Memory Bank Prompts Use:
- `read_file` - Load existing memory bank files
- `replace_string_in_file` - Update specific sections
- `create_file` - Create new task files
- `list_dir` - Scan directory structure

### Testing Prompt Uses:
- `list_dir` - Enumerate module files
- `get_symbols_overview` - Analyze file structure
- `get_symbols_by_name` - Identify public API
- `read_file` - Load source code
- `create_file` - Generate test files
- `code_search` - Find existing patterns

## 📝 Task Management

### Task File Format

Each task is tracked in `memory-bank/tasks/TASKID-taskname.md`:

```markdown
# TASK001 - Fix AuditBehaviorTests

**Status:** Completed  
**Added:** 2025-01-19  
**Updated:** 2025-01-19

## Original Request
[User's original request]

## Thought Process
[Reasoning and approach]

## Implementation Plan
1. Step 1
2. Step 2
3. Step 3

## Progress Tracking
**Overall Status:** Completed - 100%

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | First step | Complete | 2025-01-19 | |

## Progress Log
### 2025-01-19
- Completed work
- Made decisions
- Encountered issues
```

### Task Index

Tasks are organized in `memory-bank/tasks/_index.md`:

```markdown
## In Progress
- [TASK003] Implement feature - 60% complete

## Pending
- [TASK004] Planned work

## Completed
- [TASK001] Fixed issue - Completed 2025-01-19

## Blocked
- [TASK005] Waiting on dependency
```

## 🧪 Test Generation

### Supported File Types

- **Models** - POCOs, data transfer objects
- **Services** - Classes implementing `IService_*`
- **DAOs** - Database access objects (`Dao_*`)
- **ViewModels** - MVVM pattern classes
- **Behaviors** - MediatR pipeline behaviors
- **Handlers** - CQRS handlers
- **Validators** - FluentValidation classes
- **Converters** - Value converters
- **Helpers** - Utility classes

### Test Coverage Standards

- **Minimum 80% code coverage** for business logic
- All public methods tested
- Error handling paths covered
- Edge cases and boundary values
- FluentAssertions for readability
- Moq for dependency mocking

### Module Test Generation

When targeting entire module:
1. Enumerates all testable C# files
2. Checks for existing test files
3. Creates Memory Bank task for tracking
4. Generates tests file-by-file
5. Updates task progress after each file
6. Documents patterns in `systemPatterns.md`
7. Updates `progress.md` with accomplishment

## 🔗 Related Documentation

- **Memory Bank Instructions**: `.github/instructions/memory-bank.instructions.md`
- **Prompt File Guidelines**: `.github/instructions/prompt.instructions.md`
- **Testing Strategy**: `.github/instructions/testing-strategy.instructions.md`
- **MTM Architecture**: `.github/copilot-instructions.md`
- **System Patterns**: `memory-bank/systemPatterns.md`

## 📊 Examples

### Complete Workflow: Fix Bug → Test → Document

1. **Create task for bug fix:**
   ```
   /memory-bank-create-task Fix validation issue in ReceivingWorkflow
   ```

2. **Work on fix, then update progress:**
   ```
   /memory-bank-update-task TASK005: fixed validation logic, added edge case handling
   ```

3. **Generate tests for fixed code:**
   ```
   /mtm-generate-all-tests Module_Receiving/Services/Service_ReceivingWorkflow.cs
   ```

4. **Update memory bank with patterns discovered:**
   ```
   /memory-bank-update
   ```

5. **Mark task complete:**
   ```
   /memory-bank-update-task TASK005 - completed!
   ```

6. **Review all recent work:**
   ```
   /memory-bank-show-tasks recent
   ```

### Complete Workflow: New Module Tests

1. **Generate tests for module:**
   ```
   /mtm-generate-all-tests Module_Core focus:Behaviors
   ```
   *(Creates task automatically)*

2. **Review generated tests:**
   - Check `MTM_Receiving_Application.Tests/Module_Core/Behaviors/`
   - Run: `dotnet test --filter "Module=Module_Core"`

3. **Memory bank auto-updated:**
   - Task marked complete
   - systemPatterns.md updated with patterns
   - progress.md shows accomplishment

## 🎓 Best Practices

### Memory Bank
- **Update frequently** - After each significant change
- **Be specific** - Document decisions and rationale
- **Stay current** - Keep activeContext.md reflective of NOW
- **Track tasks** - Use for any multi-step work

### Testing
- **Generate early** - Create tests as you write code
- **Module batches** - Use module command for comprehensive coverage
- **Review output** - Always verify generated tests make sense
- **Run tests** - Validate they compile and pass

### Prompts
- **Read documentation** - Understand prompt structure before creating new ones
- **Follow standards** - Use consistent formatting and structure
- **Test thoroughly** - Verify prompts work as expected
- **Document usage** - Add examples for future reference

## 🚀 Getting Started

1. **Initialize Memory Bank** (first time only):
   ```
   /memory-bank-update
   ```
   Creates full structure if missing.

2. **Create your first task:**
   ```
   /memory-bank-create-task My first tracked task
   ```

3. **Generate tests for a module:**
   ```
   /mtm-generate-all-tests Module_Core
   ```

4. **View progress:**
   ```
   /memory-bank-show-tasks all
   ```

## 📮 Feedback & Updates

These prompts follow the living documentation principle - they evolve as patterns emerge and needs change. 

**To suggest improvements:**
1. Document issue/enhancement in Memory Bank task
2. Test changes with actual usage
3. Update prompt file and examples
4. Verify all examples still work

---

**Last Updated:** 2025-01-19  
**Version:** 1.0  
**Maintainer:** MTM Development Team
