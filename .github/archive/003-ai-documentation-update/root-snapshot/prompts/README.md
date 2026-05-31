# GitHub Copilot Prompts - Testing

This directory contains reusable GitHub Copilot prompt files for comprehensive test generation in the MTM Receiving Application. Project memory and progress tracking are handled by Serena's memory system (`.serena/memories/`).

## 📁 Prompt Files

### Testing Commands

| Prompt File                        | Command                   | Purpose                                                                                                                          | Agent   |
| ---------------------------------- | ------------------------- | -------------------------------------------------------------------------------------------------------------------------------- | ------- |
| `mtm-generate-all-tests.prompt.md` | `/mtm-generate-all-tests` | Generate comprehensive xUnit tests for single file or entire module                                                              | `agent` |
| `noob-mode.prompt.md`              | `/noob-mode`              | Activate plain-English mode — every action, error, and result explained in jargon-free language with color-coded risk indicators | `ask`   |

## 🎯 Quick Start

### Testing Commands

**Activate Noob Mode (plain-English explanations):**

```
/noob-mode
```

**Turn off Noob Mode:**

```
turn off noob mode
```

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

## 📖 Serena Memory System

Persistent project context is stored in Serena's memory system at `.serena/memories/`. Use the `mcp_oraios_serena_*` tools to read and write memories within agent sessions.

### Key Memory Files

| Memory                     | Purpose                                         |
| -------------------------- | ----------------------------------------------- |
| `architectural_patterns`   | MVVM layer rules and architecture decisions     |
| `coding_standards`         | Naming conventions and C# style                 |
| `dao_best_practices`       | DAO pattern, `Model_Dao_Result` usage           |
| `forbidden_practices`      | What must never be done in this codebase        |
| `testing_patterns`         | Test patterns discovered during test generation |
| `test_generation_{module}` | Progress tracking for module test generation    |

### Serena Memory Tools

- `mcp_oraios_serena_read_memory` — Read a specific memory file
- `mcp_oraios_serena_write_memory` — Create or overwrite a memory
- `mcp_oraios_serena_edit_memory` — Edit an existing memory
- `mcp_oraios_serena_list_memories` — List available memories

See the [Serena instructions](..\instructions\serena-tools.instructions.md) for full documentation.

## 🎨 Prompt File Standards

All prompt files in this directory follow these standards from `.github/instructions/prompt.instructions.md`:

### Required Frontmatter

```yaml
---
name: "command-name"
description: "Brief description (single sentence)"
agent: "ask|edit|agent"
tools: ["tool1", "tool2"]
argument-hint: "Hint for user input"
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

### Testing Prompt Uses:

- `list_dir` - Enumerate module files
- `get_symbols_overview` - Analyze file structure (via Serena MCP)
- `read_file` - Load source code
- `create_file` - Generate test files
- `code_search` - Find existing patterns
- `mcp_oraios_serena_write_memory` - Write progress and patterns to Serena memory
- `mcp_oraios_serena_edit_memory` - Update existing Serena memories
- `mcp_oraios_serena_read_memory` - Read project patterns from memory

## 📝 Progress Tracking with Serena Memories

Progress and task tracking is handled through Serena's memory system rather than separate task files.

### Writing Progress Entries

Use `mcp_oraios_serena_write_memory` with a descriptive name (e.g., `test_generation_module_core`):

```markdown
# Generate Unit Tests for Module_Core

**Status:** In Progress
**Started:** YYYY-MM-DD

## Files to Test

- [ ] Behaviors/LoggingBehavior.cs
- [x] Behaviors/AuditBehavior.cs (8 tests)

## Notes

- Discovered MediatR 14.0 requires discard parameter for CancellationToken
```

Use `mcp_oraios_serena_list_memories` to see all active progress entries.

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
3. Writes progress entry to Serena memory for tracking
4. Generates tests file-by-file
5. Updates progress entry in Serena memory after each file
6. Documents new patterns in `testing_patterns` Serena memory

## 🔗 Related Documentation

- **Serena Memory Tools**: `.github/instructions/serena-07-memories.instructions.md`
- **Serena Tools Index**: `.github/instructions/serena-tools.instructions.md`
- **Prompt File Guidelines**: `.github/instructions/prompt.instructions.md`
- **Testing Strategy**: `.github/instructions/testing-strategy.instructions.md`
- **MTM Architecture**: `.github/copilot-instructions.md`
- **Serena Memories Folder**: `.serena/memories/`

## 📊 Examples

### Workflow: Fix Bug → Generate Tests

1. **Generate tests for fixed code:**

   ```
   /mtm-generate-all-tests Module_Receiving/Services/Service_ReceivingWorkflow.cs
   ```

2. **Run the generated tests:**
   ```
   dotnet test --filter "FullyQualifiedName~Service_ReceivingWorkflowTests"
   ```

### Complete Workflow: New Module Tests

1. **Generate tests for module:**

   ```
   /mtm-generate-all-tests Module_Core focus:Behaviors
   ```

   _(Writes Serena progress entry automatically)_

2. **Review generated tests:**
   - Check `MTM_Receiving_Application.Tests/Module_Core/Behaviors/`
   - Run: `dotnet test --filter "Module=Module_Core"`

3. **Serena memories auto-updated:**
   - `test_generation_module_core` marked complete
   - `testing_patterns` updated with discovered patterns

## 🎓 Best Practices

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

1. **Generate tests for a module:**

   ```
   /mtm-generate-all-tests Module_Core
   ```

2. **Generate tests for a specific file:**

   ```
   /mtm-generate-all-tests Module_Core/Behaviors/LoggingBehavior.cs
   ```

3. **View Serena memories (progress, patterns):**
   Ask Copilot in agent mode: list Serena memories, or use `mcp_oraios_serena_list_memories`.

## 📮 Feedback & Updates

These prompts follow the living documentation principle - they evolve as patterns emerge and needs change.

**To suggest improvements:**

1. Write a Serena memory note with the proposed change
2. Test changes with actual usage
3. Update prompt file and examples
4. Verify all examples still work

---

**Last Updated:** 2026-03-21  
**Version:** 2.0  
**Maintainer:** MTM Development Team
