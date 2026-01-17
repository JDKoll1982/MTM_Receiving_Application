# Module Agents - Installation & Usage Guide

**Version:** 1.0.0 | **Date:** January 15, 2026

This document explains how to activate and use the custom module development agents in GitHub Copilot.

---

## 📦 Installed Agents

Ten specialized agents are now available in your workspace:

### 1. **Task Router** 🧭

- **Command:** `@task-router`
- **Purpose:** Intelligent agent that routes you to the correct specialist based on project state and intent
- **When to Use:** Starting point when unsure which agent to use
- **Example:** `@task-router I want to improve my project`

### 2. **Module Core Creator** 🏗️

- **Command:** `@module-core-creator`
- **Purpose:** Create Module_Core infrastructure for new WinUI 3 projects
- **When to Use:** Starting a brand new project from scratch
- **Example:** `@module-core-creator Create Module_Core for a new manufacturing application`

### 3. **Module Creator** 🎨

- **Command:** `@module-creator`
- **Purpose:** Generate new feature modules from specification documents
- **When to Use:** Adding a new feature to an existing project
- **Example:** `@module-creator Create Module_Inventory from specs/Module_Inventory_Specification.md`

### 4. **Module Rebuilder** 🔧

- **Command:** `@module-rebuilder`
- **Purpose:** Modernize existing modules to CQRS architecture
- **When to Use:** Refactoring legacy code to use MediatR/FluentValidation
- **Example:** `@module-rebuilder Rebuild Module_Receiving using CQRS patterns`

### 5. **Core Maintainer** 🛡️

- **Command:** `@core-maintainer`
- **Purpose:** Safely maintain and evolve Module_Core infrastructure
- **When to Use:** Adding/modifying/removing Core services
- **Example:** `@core-maintainer Add ShowWarning method to IService_ErrorHandler`

### 6. **Brownfield Health Check** 🔍

- **Command:** `@brownfield-health-check`
- **Purpose:** Analyze existing project state and generate modernization roadmap
- **When to Use:** Assessing project health and planning modernization
- **Example:** `@brownfield-health-check Analyze current project and create roadmap`

### 7. **Compliance Auditor** ✅

- **Command:** `@compliance-auditor`
- **Purpose:** Validate constitution compliance after module changes
- **When to Use:** After modernization to ensure architectural standards are met
- **Example:** `@compliance-auditor Validate Module_Receiving compliance`

### 8. **Doc Generator** 📝

- **Command:** `@doc-generator`
- **Purpose:** Auto-generate and update module documentation from codebase analysis
- **When to Use:** Creating or updating QUICK_REF.md, SETTABLE_OBJECTS.md, or PRIVILEGES.md
- **Example:** `@doc-generator Generate docs for Module_Receiving`

### 9. **Test Generator** 🧪

- **Command:** `@test-generator`
- **Purpose:** Generate unit and integration test scaffolds for modules
- **When to Use:** Adding test coverage to new or existing modules
- **Example:** `@test-generator Create tests for Module_Receiving`

### 10. **Privilege Code Generator** 🔐

- **Command:** `@privilege-code-generator`
- **Purpose:** Generate authorization code from PRIVILEGES.md specifications
- **When to Use:** Implementing RBAC from documented privilege rules
- **Example:** `@privilege-code-generator Generate auth code for Module_Receiving`

---

## 🚀 How to Activate Agents

### In GitHub Copilot Chat

1. **Open Copilot Chat** (Ctrl+Alt+I or Cmd+Shift+I)
2. **Type `@` to see available agents**
3. **Select the agent** you want to use
4. **Type your request** after the agent name

### Example Activation

```
You: @module-core-creator Create Module_Core for a new WinUI 3 project

Agent: 🏗️ Module Core Creator Activated
       I'll scaffold a complete Module_Core foundation for your WinUI 3 project.
       
       Let me gather some information first:
       1. Project Name?
       2. Company/Organization?
       3. Database(s)? ...
```

---

## 📋 Usage Examples

### Example 1: Starting a New Project

```
@module-core-creator
Create Module_Core for "Manufacturing Execution System"
- Company: Acme Manufacturing
- MySQL: Yes
- SQL Server: No
- Window: 1400x900
- Auth: Windows Authentication
```

**Result:** Complete Module_Core infrastructure scaffolded (28 files)

---

### Example 2: Creating a New Feature Module

**First, prepare a specification document:**

```markdown
# specs/Module_Inventory_Specification.md

## 1. Module Purpose
Track inventory levels and locations in real-time.

## 2. User Stories
- As a warehouse worker, I want to search for inventory by SKU
- As a manager, I want to adjust inventory quantities
- As a warehouse worker, I want to move inventory between locations

## 3. Data Model
[PlantUML ERD]

## 4. Workflows
[PlantUML state diagram]

## 5. Validation Rules
- SKU: Required, max 50 characters
- Quantity: Greater than 0
...
```

**Then activate the agent:**

```
@module-creator
Create Module_Inventory from specs/Module_Inventory_Specification.md
```

**Result:** Complete module scaffolded (45+ files)

---

### Example 3: Modernizing an Existing Module

```
@module-rebuilder
Rebuild Module_Receiving to use CQRS architecture
```

**Agent will:**

1. Extract current workflows for your validation
2. Classify services (Core vs Feature)
3. Present analysis report for approval
4. Execute 7-phase migration
5. Achieve 100% module independence

**Result:** Modernized module with MediatR, FluentValidation, global pipeline behaviors

---

### Example 4: Maintaining Module_Core

```
@core-maintainer
Add a new method ShowUserWarning to IService_ErrorHandler
```

**Agent will:**

1. Analyze impact on all modules
2. Classify as SAFE/MINOR/BREAKING
3. If safe, implement immediately
4. If breaking, request redesign approach

**Result:** Safe change to Core with zero module breakage

---

### Example 5: Assessing Project Health

```
@brownfield-health-check
Analyze my project and create a modernization roadmap
```

**Agent will:**

1. Scan all Module_* folders
2. Classify each module (Modern CQRS vs Legacy Service pattern)
3. Detect constitution violations
4. Analyze module dependencies (DAG)
5. Calculate safe rebuild order
6. Estimate effort for each module
7. Write `.github/.project-state.json`

**Result:** Comprehensive roadmap showing what to modernize and in what order

---

### Example 6: Generating Documentation

```
@doc-generator
Generate documentation for Module_Receiving
```

**Agent will:**

1. Scan for configuration reads → `SETTABLE_OBJECTS.md`
2. Scan for ViewModels, Commands, Queries, Handlers → `QUICK_REF.md`
3. Scan for authorization requirements → `PRIVILEGES.md`
4. Create `docs/copilot/` folder structure
5. Generate Copilot-optimized tables and PlantUML diagrams

**Result:** Complete, auto-generated documentation in sync with code

---

### Example 7: Creating Test Scaffolds

```
@test-generator
Create tests for Module_Receiving
```

**Agent will:**

1. Discover all handlers, DAOs, and ViewModels
2. Create test classes following AAA pattern
3. Place files in `Tests/Unit/Module_Receiving/` and `Tests/Integration/Module_Receiving/`
4. Include FluentAssertions syntax
5. Add TODO markers for implementation

**Result:** Compilable test scaffolds ready for implementation

---

### Example 8: Implementing Authorization

**First, document privileges in `Module_Receiving/PRIVILEGES.md`:**

```yaml
authorize:
  - handler: CreateReceivingLineCommand
    roles: [Receiving.Operator, Receiving.Manager]
  - handler: DeleteReceivingLineCommand
    roles: [Receiving.Manager]
```

**Then activate the agent:**

```
@privilege-code-generator
Generate auth code for Module_Receiving
```

**Agent will:**

1. Parse PRIVILEGES.md
2. Add `[Authorize(Roles = "Receiving.Operator,Receiving.Manager")]` to CreateReceivingLineCommandHandler
3. Add role check helpers
4. Suggest XAML visibility bindings

**Result:** Working RBAC implementation from documentation

---

### Example 9: Validating Compliance

```
@compliance-auditor
Validate Module_Receiving compliance
```

**Agent will:**

1. Check MVVM purity (x:Bind usage, no Binding)
2. Check ViewModels are partial and inherit from ViewModel_Shared_Base
3. Check DAOs are instance-based and return Model_Dao_Result
4. Check CQRS patterns (handlers use IMediator, validators exist)
5. Check stored procedure usage (MySQL only)
6. Generate pass/fail report

**Result:** Detailed compliance report with specific violations and fix recommendations

---

### Example 10: Finding the Right Agent

```
@task-router
I want to improve my project's architecture
```

**Agent will:**

1. Check if Module_Core exists
2. Count Module_* folders
3. Read `.github/.project-state.json` if available
4. Ask clarifying questions
5. Route you to: Brownfield Health Check (for assessment) or Module Rebuilder (for specific module)

**Result:** Connected to the best agent for your needs

---

## 🎯 Agent Selection Guide

| Your Goal | Use This Agent | Reason |
|-----------|----------------|--------|
| **Not sure where to start** | **Task Router** | Analyzes project and recommends correct agent |
| **Assess project health** | **Brownfield Health Check** | Scans modules and creates modernization roadmap |
| Start a new WinUI 3 project | **Module Core Creator** | Creates foundational infrastructure |
| Add a new feature to existing project | **Module Creator** | Scaffolds complete feature module |
| Modernize old code to CQRS | **Module Rebuilder** | Migrates to MediatR/FluentValidation |
| **Verify constitution compliance** | **Compliance Auditor** | Validates MVVM, data access, CQRS patterns |
| Add method to IService_ErrorHandler | **Core Maintainer** | Impact analysis + safe changes |
| Change LoggingBehavior | **Core Maintainer** | Affects all modules globally |
| Create Module_Reporting | **Module Creator** | New feature module |
| Migrate Module_Routing to handlers | **Module Rebuilder** | Legacy → modern architecture |
| **Generate or update documentation** | **Doc Generator** | Auto-creates QUICK_REF, SETTABLE_OBJECTS, PRIVILEGES |
| **Create test scaffolds** | **Test Generator** | Generates xUnit tests for handlers/DAOs/ViewModels |
| **Implement authorization from specs** | **Privilege Code Generator** | Converts PRIVILEGES.md to RBAC code |

---

## 🔄 Agent Collaboration Example

**Scenario:** Building a complete manufacturing application from scratch

```
Step 1: @task-router
        Help me set up a new manufacturing application
        → Routes you to Module Core Creator
        
Step 2: @module-core-creator
        Create Module_Core for "MTM Manufacturing System"
        
Step 3: @module-creator
        Create Module_Receiving from specs/Module_Receiving_Specification.md
        
Step 4: @module-creator
        Create Module_Routing from specs/Module_Routing_Specification.md
        
Step 5: @module-creator
        Create Module_Dunnage from specs/Module_Dunnage_Specification.md
        
Step 6: @doc-generator
        Generate documentation for all modules
        
Step 7: @test-generator
        Create tests for Module_Receiving
        
Step 8: @privilege-code-generator
        Generate auth code for Module_Receiving
        
Step 9: @core-maintainer
        Add IService_EventAggregator for module communication
        
Step 10: @compliance-auditor
         Validate all modules for constitution compliance
         
Step 11: @brownfield-health-check
         Analyze final project state and document architecture
```

**Result:** Production-ready application with clean architecture, full tests, and compliance validation!

---

**Scenario 2:** Modernizing an existing legacy project

```
Step 1: @brownfield-health-check
        Analyze project and create modernization roadmap
        → Generates dependency-aware rebuild order
        
Step 2: @module-rebuilder
        Rebuild Module_Receiving (first in safe order)
        
Step 3: @compliance-auditor
        Validate Module_Receiving compliance
        → Auto-triggers after rebuilder completes
        
Step 4: @test-generator
        Create tests for modernized Module_Receiving
        
Step 5: @module-rebuilder
        Rebuild Module_Routing (next in order)
        
Step 6: @compliance-auditor
        Validate Module_Routing compliance
        
Step 7: @doc-generator
        Update documentation for all modernized modules
```

**Result:** Systematically modernized codebase with validated compliance!

---

## 📚 Agent Reference Documents

Each agent automatically loads these documents:

### All Agents Load

- ✅ `_bmad/module-agents/config/module-development-guide.md` (constitutional rules)
- ✅ `_bmad/module-agents/config/stack-winui3-csharp.yaml` (tech stack)
- ✅ `_bmad/module-agents/diagrams/module-rebuild-diagrams.md` (20 diagrams)
- ✅ `.specify/memory/constitution.md` (non-negotiables)
- ✅ `.github/copilot-instructions.md` (project standards)

### Agent-Specific Documents

- **Task Router:** Uses project-state analysis patterns
- **Brownfield Health Check:** Uses dependency analysis and classification patterns
- **Module Core Creator:** Uses all setup/scaffolding patterns
- **Module Creator:** Uses Diagrams 16-17 (spec structure, scaffolding workflow)
- **Module Rebuilder:** Uses Diagrams 11-12 (workflow extraction, service classification)
- **Core Maintainer:** Uses Diagram 13 (impact analysis)
- **Compliance Auditor:** Uses constitution validation patterns
- **Doc Generator:** Uses symbol discovery and Copilot-optimized documentation patterns
- **Test Generator:** Uses xUnit + FluentAssertions patterns, AAA structure
- **Privilege Code Generator:** Uses RBAC attribute and policy patterns

---

## ⚙️ Agent Configuration

### Agent Configuration

Agents are configured in `.github/copilot-agents.json`:

```json
{
  "agents": [
    {
      "name": "task-router",
      "instructions": "file:_bmad/module-agents/agents/task-router.md",
      "model": "claude-sonnet-4.5",
      "tools": ["read_file", "list_files", ...]
    },
    {
      "name": "module-core-creator",
      "instructions": "file:_bmad/module-agents/agents/module-core-creator.md",
      "model": "claude-sonnet-4.5",
      "tools": ["create_file", "read_file", ...]
    },
    {
      "name": "module-creator",
      "instructions": "file:_bmad/module-agents/agents/module-creator.md",
      "model": "claude-sonnet-4.5",
      ...
    },
    {
      "name": "module-rebuilder",
      "instructions": "file:_bmad/module-agents/agents/module-rebuilder.md",
      ...
    },
    {
      "name": "core-maintainer",
      "instructions": "file:_bmad/module-agents/agents/core-maintainer.md",
      ...
    },
    {
      "name": "brownfield-health-check",
      "instructions": "file:_bmad/module-agents/agents/brownfield-health-check.md",
      ...
    },
    {
      "name": "compliance-auditor",
      "instructions": "file:_bmad/module-agents/agents/compliance-auditor.md",
      ...
    },
    {
      "name": "doc-generator",
      "instructions": "file:_bmad/module-agents/agents/doc-generator.md",
      ...
    },
    {
      "name": "test-generator",
      "instructions": "file:_bmad/module-agents/agents/test-generator.md",
      ...
    },
    {
      "name": "privilege-code-generator",
      "instructions": "file:_bmad/module-agents/agents/privilege-code-generator.md",
      ...
    }
  ]
}
```

---

## 🛠️ Troubleshooting

### Agent Not Appearing in `@` List

**Solution:**

1. Reload VS Code window (Ctrl+Shift+P → "Reload Window")
2. Verify `.github/copilot-agents.json` exists
3. Check agent definition files exist in `_bmad/module-agents/agents/`

### Agent Doesn't Follow Instructions

**Solution:**

1. Ensure all reference documents exist:
   - `_bmad/module-agents/config/module-development-guide.md`
   - `_bmad/module-agents/config/stack-winui3-csharp.yaml`
   - `_bmad/module-agents/diagrams/module-rebuild-diagrams.md`
2. Use Claude Sonnet 4.5 model for best results
3. Provide clear, specific requests

### Agent Creates Wrong Files

**Solution:**

1. Check your specification document has all 5 required sections
2. Validate PlantUML diagrams are correct
3. Review agent's scaffolding plan before approving

---

## 📖 Quick Reference

### Task Router

```
@task-router [describe your goal]
- Analyzes project state (greenfield vs brownfield)
- Scans Module_* folders
- Understands your intent
- Routes to appropriate specialist agent
- Result: Connected to the right agent with full context
```

### Brownfield Health Check

```
@brownfield-health-check
- Scans all modules and classifies (Modern vs Legacy)
- Detects constitution violations
- Analyzes module dependencies (DAG)
- Calculates safe rebuild order
- Estimates modernization effort
- Result: `.github/.project-state.json` + roadmap
```

### Module Core Creator

```
@module-core-creator Create Module_Core for [project name]
- Gathers project requirements
- Scaffolds 28 files
- Configures Serilog, MediatR, FluentValidation
- Result: Ready for feature modules
```

### Module Creator

```
@module-creator Create Module_[Feature] from specs/[file]
- Validates specification (5 required sections)
- Generates scaffolding plan
- Creates 45+ files (Models, DAOs, Handlers, Validators, ViewModels, Views)
- Result: Complete module ready for implementation
```

### Module Rebuilder

```
@module-rebuilder Rebuild Module_[Feature]
- Extracts workflows → validates with user
- Classifies services (Core vs Feature)
- Executes 7-phase migration
- Result: Modernized CQRS architecture
```

### Core Maintainer

```
@core-maintainer [Add/Modify/Remove] [service/method] in Module_Core
- Analyzes impact on all modules
- Classifies: SAFE / MINOR / BREAKING
- Implements or requests redesign
- Result: Safe Core evolution
```

### Compliance Auditor

```
@compliance-auditor Validate Module_[Feature]
- Validates MVVM purity (x:Bind, partial ViewModels)
- Validates data access integrity (stored procs, instance DAOs)
- Validates CQRS patterns (handlers, validators, IMediator)
- Auto-triggers after Module Rebuilder completes
- Result: Pass/fail report with specific violations
```

### Doc Generator

```
@doc-generator [options]
- /generate-docs → Update ALL docs for ALL modules
- /generate-docs Module_Receiving → Specific module
- /generate-docs QUICK_REF → Specific doc type across all modules
- Result: Auto-generated QUICK_REF.md, SETTABLE_OBJECTS.md, PRIVILEGES.md
```

### Test Generator

```
@test-generator [Module_Name]
- Discovers handlers, DAOs, ViewModels
- Creates test classes in Unit/ and Integration/ folders
- AAA structure with TODO markers
- Respects constitution (no DB writes to SQL Server)
- Result: Compilable test scaffolds ready for implementation
```

### Privilege Code Generator

```
@privilege-code-generator Module_[Name]
- Parses PRIVILEGES.md YAML authorize: sections
- Generates [Authorize] attributes or policies
- Inserts role checks in handlers
- Suggests XAML visibility bindings
- Result: RBAC code scaffolding from documentation
```

---

## ✅ Success Indicators

**You're using agents correctly when:**

✅ Module Core Creator builds a foundation in one session  
✅ Module Creator scaffolds from specs without manual coding  
✅ Module Rebuilder preserves all existing functionality  
✅ Core Maintainer prevents breaking changes  
✅ All agents reference constitutional documents  
✅ All agents generate compilable code  
✅ All agents include complete documentation  

---

## 🚀 Getting Started

1. **Reload VS Code** to activate agents
2. **Open Copilot Chat** (Ctrl+Alt+I)
3. **Type `@`** to see available agents
4. **Select an agent** and describe your task
5. **Review and approve** agent's plan
6. **Let the agent work** - it will scaffold everything

**Happy building!** 🎊

---

**For detailed agent documentation, see:**

- Task Router: `_bmad/module-agents/agents/task-router.md`
- Brownfield Health Check: `_bmad/module-agents/agents/brownfield-health-check.md`
- Module Core Creator: `_bmad/module-agents/agents/module-core-creator.md`
- Module Creator: `_bmad/module-agents/agents/module-creator.md`
- Module Rebuilder: `_bmad/module-agents/agents/module-rebuilder.md`
- Core Maintainer: `_bmad/module-agents/agents/core-maintainer.md`
- Compliance Auditor: `_bmad/module-agents/agents/compliance-auditor.md`
- Doc Generator: `_bmad/module-agents/agents/doc-generator.md`
- Test Generator: `_bmad/module-agents/agents/test-generator.md`
- Privilege Code Generator: `_bmad/module-agents/agents/privilege-code-generator.md`
