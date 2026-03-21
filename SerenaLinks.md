https://github.com/oraios/serena/blob/main/README.md

# Serena Configuration & Memory Update Tasks

## Task 1: Update Project Configuration (serena-01-configure-project.md)

**Agent:** MTM Receiving Specialist
**File Path:** `.serena/project.yml`
**Priority:** High
**Context:** The MTM Receiving Application project configuration needs optimization for development workflow efficiency.

### Objective

Review and update the `.serena/project.yml` configuration file to enhance Serena's effectiveness for this specific project.

### Key Configuration Areas to Address

1. **Initial Prompt (`initial_prompt` field)**
   - Current state: Empty or generic
   - Required: Add project-specific prompt that orients Serena to:
     - MVVM architecture enforcement
     - Database constraints (MySQL READ/WRITE, SQL Server READ ONLY)
     - Reference to memory locations for patterns (architecture, forbidden practices, etc.)
     - Link to `.github/copilot-instructions.md` as source of truth
   - Example structure: "You are working on the MTM Receiving Application (WinUI 3 MVVM). Before making changes: (1) Read architectural_patterns memory, (2) Read forbidden_practices memory, (3) Consult .github/copilot-instructions.md"

2. **Base Modes (`base_modes` field)**
   - Current state: Empty list
   - Purpose: Modes that are ALWAYS active for this project, cannot be disabled by CLI
   - Recommended settings:
     - `editing` - Enable code modification capabilities
     - `interactive` - Enable conversational back-and-forth mode
   - Rationale: These modes support typical development workflow for feature implementation and bug fixes
   - Reference: See `serena-08-configuration.instructions.md` modes table

3. **Default Modes (`default_modes` field)**
   - Current state: Empty list
   - Purpose: Modes activated by default but can be overridden via CLI `--mode` parameter
   - Recommended settings:
     - Leave empty to use global configuration defaults, OR
     - Set to specific modes if project has unique requirements
   - Note: CLI parameters can override this setting; base_modes cannot be overridden
   - Reference: See `serena-08-configuration.instructions.md`

4. **Fixed Tools (`fixed_tools` field)**
   - Current state: Empty array
   - Purpose: Replace Serena's default tool set entirely (if non-empty)
   - Recommendation: Leave empty to use Serena's optimized default toolset
   - Warning: Cannot be combined with `excluded_tools` or `included_optional_tools`
   - Only use if project has very specific, limited tool requirements

5. **Symbol Info Budget (`symbol_info_budget` field)**
   - Current state: Null/unset
   - Purpose: Time budget (seconds) for retrieving additional symbol information per tool call
   - Recommendation: Leave unset to use global configuration default
   - Adjust only if experiencing timeout issues or need faster/slower responses
   - Typical range: 5-30 seconds depending on codebase size (MTM ~300 files)

6. **Language Backend (`language_backend` field)**
   - Current state: Unset (uses global default)
   - Valid values: `language_server` (default LSP) or `jetbrains`
   - MTM Recommendation: Set to `language_server` explicitly for clarity
   - Rationale: Roslyn LSP is standard for .NET 8 C# 12 projects
   - Note: Backend is fixed at startup; cannot change mid-session

7. **Read-Only Memory Patterns (`read_only_memory_patterns` field)**
   - Current state: Empty array
   - Purpose: Regex patterns to mark specific memories as read-only (prevents AI edits)
   - Recommended settings:
     - `"constitution_summary"` - Protect core rules from accidental modification
     - `"forbidden_practices"` - Protect critical restrictions from being altered
   - Example: `['constitution_summary', 'forbidden_practices']`
   - Rationale: Prevents AI from mistakenly editing architectural constraints

8. **Line Ending Convention (`line_ending` field)**
   - Current state: Unset (uses platform default)
   - Valid values: `unset` (global), `"lf"` (Unix), `"crlf"` (Windows), `"native"` (platform default)
   - MTM Recommendation: Set to `"crlf"` for Windows development consistency
   - Rationale: MTM team develops on Windows; prevents git diff pollution from line ending changes
   - Note: Does not affect Serena's own files (memories, config)

9. **Optional Tools (`included_optional_tools` field - if applicable)**
   - Evaluate which optional Serena tools would benefit this project
   - Consider: `search_for_pattern` for architecture validation, `execute_command` for build verification
   - Document rationale for each included tool

10. **Project Context Settings**
    - Verify `read_only: false` is set appropriately
    - Confirm language is set to `csharp` (Roslyn backend)
    - Review any language-specific settings needed for .NET 8 and C# 12

### Deliverable

Updated `.serena/project.yml` with:

- Non-empty, project-specific `initial_prompt`
- Populated `base_modes` with recommended modes for MTM development
- Optimized `default_modes` configuration (or explicitly left empty)
- `language_backend: language_server` set explicitly
- `read_only_memory_patterns` protecting critical memories
- `line_ending: "crlf"` for Windows consistency
- Any additional context settings beneficial for MTM development

### Validation Checklist

After configuration update:

- [ ] `initial_prompt` references architectural patterns and forbidden practices memories
- [ ] `initial_prompt` directs to `.github/copilot-instructions.md` as source of truth
- [ ] `base_modes` includes `editing` and `interactive` (or project-specific alternatives)
- [ ] `language_backend` explicitly set to `language_server` for clarity
- [ ] `read_only_memory_patterns` protects `constitution_summary` and `forbidden_practices`
- [ ] `line_ending` set to `"crlf"` for Windows team consistency
- [ ] All empty/commented fields that need values have been populated with documented rationale
- [ ] Configuration aligns with guidance in `serena-08-configuration.instructions.md`

---

## Task 2: Audit and Update Serena Memories (serena-02-update-memories.md)

**Agent:** MTM Receiving Specialist
**File Path:** `.serena/memories/` (all files)
**Priority:** Critical
**Context:** Project memories must reflect current codebase state and serve as the authoritative knowledge base for all AI agents.

### Objective

Conduct a comprehensive audit of all memory files in `.serena/memories/`, remove outdated content, add missing patterns discovered in the codebase, and create any new memories needed.

### Memory Files to Review & Update

**Existing Memories (Review & Refresh):**

1. `architectural_patterns.md` - Verify against current codebase; update with any new patterns found
2. `coding_standards.md` - Ensure alignment with `.editorconfig` and current conventions
3. `constitution_summary.md` - Distill from `.github/copilot-instructions.md`; keep as authoritative
4. `dao_best_practices.md` - Verify `Model_Dao_Result` usage; ensure no stale patterns
5. `dialog_patterns.md` - Verify against current `ContentDialog`, `ContentWindow` usage
6. `error_handling_guide.md` - Verify `IService_ErrorHandler` integration patterns
7. `forbidden_practices.md` - Most critical; verify all rules still enforced
8. `help_system_architecture.md` - Update if help system has evolved
9. `infor_visual_constraints.md` - Verify `ApplicationIntent=ReadOnly`, table references
10. `mvvm_guide.md` - Comprehensive MVVM walkthrough; ensure examples are current
11. `project_overview.md` - High-level module and context summary; verify completeness
12. `suggested_commands.md` - Verify all commands are current and functional
13. `task_completion_workflow.md` - Update based on current build/test/validation process
14. `tech_stack.md` - Verify versions (.NET 8, WinUI 3, MySQL 5.7, etc.)
15. `xaml_binding_patterns.md` - Verify `x:Bind` patterns; remove any `Binding` references

**Potential New Memories to Create (if patterns found):**

- `dependency_injection_patterns.md` - If DI patterns warrant dedicated guide
- `stored_procedure_patterns.md` - MySQL stored procedure conventions
- `module_structure_guide.md` - Module folder organization and naming
- `viewmodel_best_practices.md` - ViewModel-specific patterns beyond MVVM basics
- `testing_patterns.md` - Existing or new test patterns discovered
- `common_gotchas.md` - Frequent mistakes or debugging issues

### Methodology

**For Each Memory File:**

1. **Read Current Content** - Understand what is documented
2. **Codebase Audit** - Use Serena to search for patterns mentioned:
   - Verify patterns still exist and are being followed
   - Find counter-examples or violations (these become warnings for inclusion)
   - Discover new patterns not yet documented
3. **Remove Dead Content** - Delete any references to:
   - Deprecated technologies or APIs
   - Obsolete folder structures
   - Outdated naming conventions
   - Removed classes or methods
4. **Add Missing Content** - Include:
   - Current best practices discovered during audit
   - Real code examples from the codebase (class names, method names)
   - Common patterns found in 3+ similar implementations
   - Architecture validation searches and expected results
5. **Validate Accuracy** - Cross-reference against:
   - `.github/copilot-instructions.md`
   - `.github/AGENTS.md`
   - Actual codebase implementations
6. **Update `Last Updated` Field** - Set to current date in YYYY-MM-DD format

### Serena Tool Usage Recommendations

Use the following Serena tools during the audit:

```
- get_symbols_overview() - Understand module structures
- find_symbol() - Locate pattern implementations
- search_for_pattern() - Verify forbidden patterns absent/present as expected
- find_referencing_symbols() - Understand usage patterns
- list_memories() - See current memory inventory
- read_memory() - Read each memory file for review
- write_memory() / edit_memory() - Update or create memories
```

### Validation Checklist

After memory update completion:

- [ ] All existing memory files reviewed and refreshed
- [ ] Dead/obsolete content removed
- [ ] All memory files have current `Last Updated: YYYY-MM-DD` line
- [ ] New memory files created for patterns discovered (if warranted)
- [ ] Memories align with `.github/copilot-instructions.md` guidance
- [ ] Architecture validation searches documented with expected results
- [ ] Real codebase examples used throughout (not generic pseudocode)
- [ ] Cross-references between related memory files are current
- [ ] Memory index/catalog is complete and accurate

### Deliverable

Updated/Created Memory Files (in `.serena/memories/`):

- All existing memories refreshed with current, relevant content
- Dead content removed
- New memories created as discovered
- Each file includes `Last Updated: YYYY-MM-DD`
- All memories align with live codebase state

---

## Supporting Documentation

**Reference Links:**

- https://oraios.github.io/serena/02-usage/045_memories.html
- https://oraios.github.io/serena/02-usage/050_configuration.html
- `.github/copilot-instructions.md` - Source of truth for project rules
- `.github/AGENTS.md` - Agent definitions and responsibilities
