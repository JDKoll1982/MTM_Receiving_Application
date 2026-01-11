# Prompt for VSCode GitHub Copilot Chat

Copy and paste this entire prompt into GitHub Copilot Chat in VSCode:

---

You are helping me set up optimal GitHub Copilot configuration for the **JDKoll1982/MTM_Receiving_Application** repository based on best practices.

## Context

This is a WinUI 3 C# application using:

- . NET 8, MVVM architecture with CommunityToolkit.Mvvm
- MySQL database (READ/WRITE) and SQL Server/Infor Visual (READ ONLY)
- Modular structure:  Module_Core, Module_Receiving, Module_Dunnage, Module_Routing, Module_Reporting, Module_Settings, Module_Shared, Module_Volvo
- xUnit testing, strict MVVM separation with x:Bind in XAML

## MCP Tools Available

This workspace has **Serena MCP** and **Filesystem MCP** servers configured. Use these tools to gather accurate context before creating documentation files.

### Serena MCP (Code Navigation & Symbol Analysis)

**Purpose:** Understand the codebase structure at the symbol level before documenting it.

**Key Tools:**

- `mcp_oraios_serena_onboarding` - Generate onboarding information for a project area
- `mcp_oraios_serena_find_symbol` - Search for classes, methods, properties by name pattern
- `mcp_oraios_serena_get_symbols_overview` - Get overview of symbols in a file
- `mcp_oraios_serena_find_referencing_symbols` - Find where a symbol is used
- `mcp_oraios_serena_think_about_collected_information` - Reflection checkpoint after exploration

**When to Use:**

- Before creating `.instructions.md` - explore ViewModel patterns, DAO implementations
- Before creating `ARCHITECTURE.md` - map module dependencies, understand service layers
- Before creating `PROMPT_LIBRARY.md` - find real examples of patterns to document
- Before creating `COPILOT_TROUBLESHOOTING.md` - understand common code structures

**Example Workflow:**

```bash
1. Use mcp_oraios_serena_onboarding to understand Module_Receiving
2. Use mcp_oraios_serena_find_symbol to find all ViewModels (pattern: "*ViewModel")
3. Use mcp_oraios_serena_get_symbols_overview to understand ViewModel structure
4. Use mcp_oraios_serena_think_about_collected_information before documenting
```

### Filesystem MCP (File Operations)

**Purpose:** Safely read, write, and manage documentation files.

**Key Tools:**

- `mcp_filesystem_list_directory` - List files in directories
- `mcp_filesystem_directory_tree` - Get recursive directory structure
- `mcp_filesystem_read_text_file` - Read existing files for context
- `mcp_filesystem_read_multiple_files` - Batch read related files
- `mcp_filesystem_write_file` - Create new documentation files
- `mcp_filesystem_search_files` - Find files by glob pattern

**When to Use:**

- Check which instruction files already exist in `.github/instructions/`
- Read existing constitution, AGENTS.md, or copilot-instructions.md for consistency
- Verify module structure before documenting architecture
- Search for existing examples to reference in documentation

**Example Workflow:**

```bash
1. Use mcp_filesystem_directory_tree to understand .github/ structure
2. Use mcp_filesystem_read_multiple_files to read all .instructions.md files
3. Use mcp_filesystem_search_files to find all *ViewModel.cs files
4. Use mcp_filesystem_write_file to create new documentation
```

### Recommended MCP Usage Pattern

For each documentation file you create:

1. **Explore Context** (Serena + Filesystem)
   - Use Serena to understand code patterns and symbols
   - Use Filesystem to read existing documentation
   - Gather real examples from the codebase

2. **Reflect** (Serena)
   - Call `mcp_oraios_serena_think_about_collected_information`
   - Ensure you have sufficient context

3. **Create Documentation** (Filesystem)
   - Use `mcp_filesystem_write_file` to create the new file
   - Reference actual code patterns discovered via Serena

4. **Validate** (Filesystem)
   - Use `mcp_filesystem_read_text_file` to review created content
   - Ensure accuracy and completeness

## Your Task

This is a **RESET** operation. You will DELETE existing configuration files and recreate them from scratch based on best practices.

### STEP 1: Delete Existing Files (DO THIS FIRST)

Before creating any new files, DELETE the following files if they exist:

- `./.github/copilot-instructions.md`
- `./.vscode/settings.json` (backup first if it contains user-specific settings)
- `./ARCHITECTURE.md`
- `./.github/copilot-instructions.md`
- `./docs/COPILOT_GUIDE.md`
- `./docs/PROMPT_LIBRARY.md`
- `./docs/COPILOT_TROUBLESHOOTING.md`
- `./.vscode/tasks.json`
- `./.github/PULL_REQUEST_TEMPLATE.md`

**IMPORTANT:** `.editorconfig` should NOT be deleted as it's already properly configured.

**After deletion, confirm with me before proceeding to file creation.**

### STEP 2: Create Files (Priority Order)

Create the following files **one at a time**, in priority order. After creating each file, STOP and wait for my confirmation before proceeding to the next file.

### File Creation Rules

1. **Create files from scratch** - don't reference existing files, generate complete content
2. **Show me the full file content** in a code block with the correct file name and path
3. **Wait for my approval** before moving to the next file
4. **Make files repository-specific** - use actual module names, technologies, and patterns from this codebase
5. **Be comprehensive** - don't create placeholder content, make it production-ready
6. **Follow Copilot Best Practices** - align all content with docs/Copilot-BestPractices.md principles

## Files to Create (Priority Order)

### 0. Project Constitution (SPECIAL - USE CUSTOM AGENT)

**Location:** `./.specify/memory/constitution.md`

**Purpose:** Define non-negotiable project principles, governance, and architectural constraints

**⚠️ SPECIAL INSTRUCTIONS:**

This file is **NOT created manually**. Instead, use the custom agent:

1. Open VSCode Command Palette (`Ctrl+Shift+P`)
2. Select **"Chat: Use Agent"** or click the agent picker in Copilot Chat
3. Choose **"spec-constitution.agent"** from `.github/agents/spec-constitution.agent.md`
4. The agent will guide you through an interactive process to create or update the constitution
5. The agent ensures the constitution stays in sync with all dependent templates

**What the Constitution Contains:**

- Core principles (MVVM Architecture, Database Layer Consistency, Service Layer Architecture)
- Non-negotiable constraints (NO static DAOs, NO writing to SQL Server, MUST use x:Bind)
- Code quality standards (aligned with .editorconfig)
- Testing requirements
- Security policies
- Governance and amendment procedures
- Version tracking (semantic versioning for constitution changes)

**Why This Matters:**

The constitution is the foundation document that drives all instruction files, templates, and code generation patterns. It must be created FIRST using the agent to ensure consistency.

**After Creation:**

Once the constitution is created via the agent, review it and confirm before proceeding to `.instructions.md`.

---

### 1. `.instructions.md` (ROOT DIRECTORY - HIGHEST PRIORITY)

**Location:** `./. instructions.md`

**Purpose:** Personalize GitHub Copilot's behavior for this entire codebase

**Required Content:**

- Project context (WinUI 3, . NET 8, MVVM, CommunityToolkit.Mvvm)
- Technology stack (MySQL READ/WRITE, SQL Server READ ONLY, xUnit)
- Coding standards and naming conventions
- Architecture principles (strict MVVM, modular organization, dependency flow)
- MVVM patterns (ViewModels must be partial classes, use [ObservableProperty], [RelayCommand])
- Database access patterns (DAOs must be instance-based, use stored procedures for MySQL, NEVER write to SQL Server)
- Async/await best practices
- Error handling standards
- Code documentation requirements (XML docs for public APIs)
- Testing standards (xUnit, AAA pattern, FluentAssertions)
- Preferences for generating code, refactoring, explaining
- Project-specific business rules (authentication, receiving workflow, data persistence)
- "When I ask you to..." sections for common tasks (generate ViewModel, create View, implement Service, write DAO, add feature, debug, optimize)
- Things to avoid (static DAOs, raw SQL, writing to SQL Server, business logic in Views, etc.)
- Communication style preferences

**MCP-Enhanced Content Requirements:**

Before creating this file:

1. **Use Serena to explore patterns:**

   ```bash
   - mcp_oraios_serena_find_symbol with pattern "*ViewModel" to find all ViewModels
   - mcp_oraios_serena_get_symbols_overview on 2-3 representative ViewModels
   - mcp_oraios_serena_find_symbol with pattern "Dao_*" to find all DAOs
   - mcp_oraios_serena_find_symbol with pattern "*Service" to find all Services
   ```

2. **Use Filesystem to gather context:**

   ```bash
   - mcp_filesystem_read_multiple_files on existing .github/instructions/*.md files
   - mcp_filesystem_directory_tree for Module_* structure
   - mcp_filesystem_read_text_file on .specify/memory/constitution.md
   ```

3. **Include real code examples from exploration:**
   - Show actual ViewModel structure discovered (e.g., ReceivingViewModel)
   - Reference actual DAO patterns found (e.g., Dao_ReceivingPackage)
   - Include actual service implementations as examples

**Specific Requirements:**

- Reference actual modules:  Module_Core, Module_Receiving, Module_Dunnage, Module_Routing, Module_Reporting, Module_Settings, Module_Shared, Module_Volvo
- Include code examples showing ViewModel structure, XAML binding patterns, DAO structure
- Emphasize:  ViewModels MUST be partial classes, use x:Bind NOT Binding, DAOs instance-based NOT static
- Database rules: MySQL stored procedures only, SQL Server READ ONLY
- **Align with constitution** (.specify/memory/constitution.md) - reference constitutional principles
- Include model selection guidance (Claude Sonnet for complex architecture, GPT-4o for routine tasks)
- Add prompt patterns for common tasks (as outlined in Copilot-BestPractices.md)

---

### 2. `.vscode/settings.json` (WORKSPACE SETTINGS)

**Location:** `./.vscode/settings.json`

**Purpose:** Workspace-specific VSCode and Copilot settings optimized for this project

**Required Content:**

- GitHub Copilot settings (enable, code actions, project templates, next edit suggestions)
- Copilot Chat settings (locale, follow-ups, terminal location)
- C# editor settings (format on save, IntelliSense, code lens)
- XAML editor settings
- File associations for .xaml, .cs, .csproj
- Search exclusions (bin, obj, . vs folders)
- Recommended extensions list in comments

**Specific Requirements:**

- Enable all Copilot features (per Copilot-BestPractices.md recommendations)
- Set appropriate context length for large projects (`"github.copilot.advanced.contextLength": "high"`)
- Configure C# and XAML formatters
- Add file watcher exclusions for build artifacts
- **Include settings from Copilot-BestPractices.md table:**
  - `github.copilot.enable: true`
  - `github.copilot.editor.enableCodeActions: true`
  - `github.copilot.chat.useProjectTemplates: true`
  - `github.copilot.nextEditSuggestions.enabled: true`
  - `github.copilot.chat.followUps: "always"`
  - `github.copilot.advanced.inlineSuggestCount: 3`
- Add C# editor settings (format on save, organize imports)
- Include recommended extensions in comments (C# Dev Kit, XAML tools, Copilot extensions)

---

### 3. `ARCHITECTURE.md` (ROOT DIRECTORY)

**Location:** `./ARCHITECTURE.md`

**Purpose:** Comprehensive architecture documentation with visual diagrams

**Required Content:**

- High-level architecture overview
- Module breakdown (what each module does:  Module_Core, Module_Receiving, etc.)
- Dependency flow diagram (Mermaid)
- MVVM pattern implementation
- Database architecture (MySQL schema, SQL Server read-only integration)
- Data flow diagrams showing:  User → View → ViewModel → Service → DAO → Database
- Authentication flow (personal workstation vs shared terminal)
- Receiving workflow sequence diagram
- Key design patterns used (DI, Repository, MVVM, Command)
- Technology decisions and rationale
**MCP-Enhanced Content Requirements:**

Before creating this file:

1. **Use Serena to map architecture:**

   ```bash
   - mcp_oraios_serena_onboarding for each major module
   - mcp_oraios_serena_find_referencing_symbols to trace dependencies
   - Explore service layer → DAO relationships
   - Map ViewModel → Service → DAO chains for key features
   ```

2. **Use Filesystem to understand structure:**

   ```bash
   - mcp_filesystem_directory_tree to visualize module organization
   - mcp_filesystem_list_directory for each Module_* folder
   - mcp_filesystem_search_files with "*.csproj" to find project boundaries
   ```

3. **Create data-driven diagrams:**
   - Use actual class names discovered via Serena
   - Show real dependency chains (not theoretical examples)
   - Reference actual service registrations from App.xaml.cs
**Specific Requirements:**

- **Use PlantUML diagrams, NOT Mermaid** (per markdown-documentation.instructions.md standards)
- Include at least 3 PlantUML diagrams:
  - Architecture component diagram
  - Module dependency diagram (showing Module_Core, Module_Receiving, etc.)
  - Data flow sequence diagram (User → View → ViewModel → Service → DAO → Database)
- Reference actual modules and their responsibilities
- Show integration points between modules
- Document database access patterns (MySQL read/write, SQL Server read-only)
- Include constitutional principles in architecture decisions
- Add legend to all diagrams explaining symbols and colors

---

### 4. `.github/copilot-instructions.md` (TEAM-WIDE INSTRUCTIONS)

**Location:** `./.github/copilot-instructions.md`

**Purpose:** Team-wide Copilot instructions for all developers

**Required Content:**

- Team coding standards
- Review checklist for Copilot-generated code
- Security guidelines (SQL injection prevention, no secrets in code)
- Testing requirements (minimum coverage, test patterns)
- Code review process
- Common pitfalls to avoid
- Escalation guidelines (when NOT to use Copilot)

**Specific Requirements:**

- Focus on team collaboration aspects
- Include security and compliance rules (no SQL injection, no secrets in code, SQL Server read-only enforcement)
- Reference constitution principles
- Provide review checklist template for Copilot-generated code
- Add escalation guidelines (when NOT to blindly accept Copilot suggestions)
- Include code ownership and responsibility policies
- Reference Copilot-BestPractices.md for model selection in team context

---

### 5. `docs/COPILOT_GUIDE.md` (DEVELOPER GUIDE)

**Location:** `./docs/COPILOT_GUIDE.md`

**Purpose:** Complete guide for developers on using Copilot with this project

**Required Content:**

- Getting started with Copilot in this project
- Model selection guide (which AI model for what task)
- Prompt engineering examples specific to this codebase
- Real-world scenarios (creating a new ViewModel for a feature, adding a DAO, refactoring async code)
- Common prompts library (with examples using actual modules)
- Troubleshooting common issues (binding errors, async deadlocks, DI issues)
- Best practices specific to WinUI 3 and MVVM
- Workflow patterns (iterative refinement, context building, test-driven development)

**MCP-Enhanced Content Requirements:**

Before creating this file:

1. **Use Serena to find real examples:**

   ```bash
   - Find 3-5 actual ViewModels to use as examples
   - Find actual problematic patterns (e.g., async void, missing try-catch)
   - Identify common architectural patterns to document
   ```

2. **Create scenario-based prompts:**
   - "Create a ViewModel like ReceivingViewModel for [new feature]"
   - "Add a DAO following the pattern in Dao_ReceivingPackage"
   - Include actual file paths and class names

3. **Use MCP tools in example prompts:**
   - Show how to use Serena to explore before coding
   - Demonstrate Filesystem for reading related files
   - Include MCP-enhanced workflow examples

**Specific Requirements:**

- Include 5+ real-world examples using actual module names (Module_Receiving, Module_Dunnage, etc.)
- Provide prompt templates for:  creating ViewModels, implementing DAOs, debugging binding issues, optimizing queries
- **Directly reference and expand on Copilot-BestPractices.md sections:**
  - Model Comparison Matrix (when to use Claude Sonnet vs GPT-4o vs o1)
  - Optimal prompting strategies
  - Context management techniques
  - Iterative refinement workflows
- Include PlantUML decision tree diagrams (not Mermaid)
- Add troubleshooting section for common WinUI 3 / XAML issues
- Reference constitutional constraints in examples
- Include "What to avoid" section (anti-patterns with Copilot)

---

### 6. `.github/agents/README.md` (CUSTOM AGENTS DOCUMENTATION)

**Location:** `./.github/agents/README.md`

**Purpose:** Document all custom Copilot agents available in the project

**Required Content:**

- Overview of custom agents (what they are, how to use them)
- **spec-constitution.agent.md** - Constitution creation and amendment
- Instructions for creating new custom agents
- Agent file format and structure
- Handoff patterns between agents
- When to use agents vs direct prompts

**Specific Requirements:**

- Include example of invoking an agent via VSCode Command Palette
- Document the spec-constitution.agent workflow
- Provide template for creating new agents
- Reference .github/agents/ directory structure

---

### 7. `.editorconfig` (VERIFY/UPDATE EXISTING - DO NOT DELETE)

**Location:** `./.editorconfig`

**Purpose:** Enforce consistent code formatting (ALREADY EXISTS - verify alignment)

**Required Actions:**

- **DO NOT DELETE** this file
- Read existing `.editorconfig`
- Verify it aligns with:
  - Constitution Principle IX (Code Quality & Maintainability)
  - C# 12 and .NET 8 best practices
  - Naming conventions (PascalCase for public, _camelCase for private fields)
  - Bracing rules (`csharp_prefer_braces = true:error`)
  - Async method naming (`Async` suffix required)
- If gaps found, propose updates in separate step
- Ensure XAML formatting rules present

**Verification Checklist:**

- [ ] Bracing enforcement for all if statements
- [ ] Accessibility modifiers required
- [ ] Async method naming convention
- [ ] Null handling conventions
- [ ] LINQ optimization preferences
- [ ] File encoding (UTF-8)
- [ ] Line endings (CRLF for Windows)

---

### 8. `docs/PROMPT_LIBRARY.md` (REUSABLE PROMPTS)

**Location:** `./docs/PROMPT_LIBRARY. md`

**Purpose:** Library of tested, reusable prompts for common tasks

**Required Content:**
Categories:

- **Architecture & Design** (design a new module, refactor module dependencies)
- **ViewModel Creation** (create ViewModel with validation, add async commands)
- **View/XAML** (create DataGrid view, implement master-detail, fix binding)
- **Service Layer** (implement service with DI, add caching, error handling)
- **Data Access** (create DAO with stored proc, optimize query, add transaction support)
- **Testing** (generate unit tests, integration tests, test data builders)
- **Debugging** (diagnose binding issue, fix async deadlock, memory leak analysis)
- **Performance** (optimize database query, reduce UI lag, async optimization)
- **Documentation** (generate XML docs, create architecture diagram, update README)
- **MCP Workflows** (use Serena to explore before coding, use Filesystem to read context)

**MCP-Enhanced Content Requirements:**

Each prompt should include MCP preparation steps:

**Example Prompt Structure:**

```markdown
### Create New ViewModel Following Project Patterns

**Purpose:** Generate a new ViewModel that follows all architectural standards

**Model Recommendation:** Claude Sonnet (architectural understanding)

**MCP Preparation (do this first):**
1. Use Serena: mcp_oraios_serena_find_symbol with pattern "*ViewModel"
2. Use Serena: mcp_oraios_serena_get_symbols_overview on ReceivingViewModel.cs
3. Use Filesystem: mcp_filesystem_read_text_file on BaseViewModel.cs
4. Use Serena: mcp_oraios_serena_think_about_collected_information

**Complete Prompt:**
"Create a ViewModel for [feature] following the pattern in #file:ReceivingViewModel.cs..."

**Expected Outcome:** Partial class, inherits BaseViewModel, uses [ObservableProperty]...
```

**Specific Requirements:**

- Each prompt includes: Purpose, **Model Recommendation** (per Copilot-BestPractices.md), Complete Prompt Text, Expected Outcome
- Use actual module names and technologies from this project
- Provide before/after context examples
- Include success criteria and validation steps
- Add "Context Setup" section for each prompt (which files to reference with #file:)
- Include constitutional compliance checks in prompts

---

### 9. `.vscode/tasks.json` (BUILD AND TEST TASKS)

**Location:** `./.vscode/tasks.json`

**Purpose:** Automate common development tasks

**Required Content:**

- Build tasks (Debug, Release, specific projects)
- Test tasks (All tests, Unit tests only, Integration tests)
- Database tasks (run migrations, seed data)
- Clean tasks (clean bin/obj folders)
- Restore tasks (NuGet restore)

**Specific Requirements:**

- Include problem matchers for C# compiler errors and warnings
- Add tasks for running specific test categories (Unit, Integration)
- Configure output groups and presentation options
- Include database tasks if applicable (migrations, seed data)
- Add XAML compilation verification task
- Include clean/restore tasks

---

### 10. `docs/COPILOT_TROUBLESHOOTING.md`

**Location:** `./docs/COPILOT_TROUBLESHOOTING.md`

**Purpose:** Solutions to common Copilot-generated issues

**Required Content:**

- Common WinUI 3 / XAML issues (binding errors, x:Bind vs Binding, compiled bindings)
- MVVM issues (ViewModel not partial, missing source generators, command not executing)
- Database issues (connection string errors, SQL injection risks, transaction handling)
- Async/await issues (deadlocks, UI thread blocking, cancellation)
- DI issues (service not registered, circular dependencies, lifetime issues)
- Build issues (source generator errors, XAML compilation errors)

**Specific Requirements:**

- Problem/Solution format with clear diagnostic steps
- Include actual error messages from WinUI 3 / WPF / C# compiler
- Reference constitutional violations as source of errors
- Provide corrected code examples showing before/after
- Add prevention strategies (how to avoid generating these issues)
- Include links to relevant instruction files (.github/instructions/)
- Cross-reference with PROMPT_LIBRARY.md for proper prompt patterns

---

### 11. `.github/PULL_REQUEST_TEMPLATE.md`

**Location:** `./.github/PULL_REQUEST_TEMPLATE. md`

**Purpose:** PR template with Copilot usage disclosure

**Required Content:**

- Description section
- Changes made checklist
- **Copilot usage disclosure** (which parts used Copilot, how was code reviewed)
- Testing completed
- Screenshots (if UI changes)
- Breaking changes notification
- Reviewer checklist

**Specific Requirements:**

- Include specific section: "**GitHub Copilot Usage**"
  - [ ] GitHub Copilot was used for this PR
  - [ ] All Copilot-generated code was reviewed and tested
  - [ ] Code aligns with constitution principles
  - [ ] Security review completed (no SQL injection, no secrets, SQL Server read-only enforced)
- Add constitutional compliance checklist
- Include links to relevant documentation
- Add breaking changes notification section
- Include screenshots section for UI changes
- Add database migration checklist if applicable

---

### 12. `.vscode/extensions.json` (RECOMMENDED EXTENSIONS)

**Location:** `./.vscode/extensions.json`

**Purpose:** Recommend essential VSCode extensions for the team

**Required Content:**

- GitHub Copilot and Copilot Chat extensions
- C# Dev Kit and related C# tools
- XAML and WinUI extensions
- Database management extensions (MySQL)
- Testing extensions (Test Explorer, Coverage)
- PlantUML preview extension (for architecture diagrams)
- Markdown tools (for documentation)

**Specific Requirements:**

- Use official extension IDs
- Separate into "recommendations" and "unwantedRecommendations"
- Include comments explaining why each extension is recommended
- Align with tools mentioned in Copilot-BestPractices.md

---

## Execution Instructions

**STEP 1:** Confirm deletion of existing files listed above.

**STEP 2:** **Use MCP tools to explore the codebase** before creating documentation:

```bash
Recommended Initial Exploration:
1. mcp_filesystem_directory_tree - Understand overall structure
2. mcp_oraios_serena_onboarding - Get project overview
3. mcp_filesystem_read_text_file on constitution.md, AGENTS.md
4. mcp_oraios_serena_find_symbol to map key patterns (ViewModels, DAOs, Services)
5. mcp_oraios_serena_think_about_collected_information - Reflect on findings
```

**STEP 3:** Create the constitution using the spec-constitution.agent (File #0).

**STEP 4:** Start with File #1 (`.instructions.md`) and work through the list in order.

For each file:

1. **Use MCP tools first** - Gather real examples and patterns from the codebase
2. Generate the COMPLETE file content (not a summary or outline) based on actual code
3. Show it to me in a code block with proper syntax highlighting
4. Include the file path in the code block header
5. WAIT for my approval before proceeding
6. After I approve, move to the next file

**MCP Best Practices Throughout:**

- Use Serena for code exploration (symbols, references, patterns)
- Use Filesystem for reading existing documentation and structure
- Call `mcp_oraios_serena_think_about_collected_information` after multi-step exploration
- Reference actual file paths using #file: syntax when available
- Base examples on real code, not theoretical patterns

**Reference Documents:**

- Constitution: `.specify/memory/constitution.md` (create first via agent)
- Best Practices: `docs/Copilot-BestPractices.md`
- Markdown Standards: `.github/instructions/markdown-documentation.instructions.md`
- MVVM Patterns: `.github/instructions/mvvm-pattern.instructions.md`
- DAO Patterns: `.github/instructions/dao-pattern.instructions.md`

**Do you understand these instructions? If yes, please:**

1. First, confirm you will delete the listed files
2. Then guide me through using the spec-constitution.agent to create the constitution
3. Finally, start with File #1: `.instructions.md`
