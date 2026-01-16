# Module Agents - Installation & Usage Guide

**Version:** 1.0.0 | **Date:** January 15, 2026

This document explains how to activate and use the custom module development agents in GitHub Copilot.

---

## 📦 Installed Agents

Four specialized agents are now available in your workspace:

### 1. **Module Core Creator** 🏗️

- **Command:** `@module-core-creator`
- **Purpose:** Create Module_Core infrastructure for new WinUI 3 projects
- **When to Use:** Starting a brand new project from scratch
- **Example:** `@module-core-creator Create Module_Core for a new manufacturing application`

### 2. **Module Creator** 🎨

- **Command:** `@module-creator`
- **Purpose:** Generate new feature modules from specification documents
- **When to Use:** Adding a new feature to an existing project
- **Example:** `@module-creator Create Module_Inventory from specs/Module_Inventory_Specification.md`

### 3. **Module Rebuilder** 🔧

- **Command:** `@module-rebuilder`
- **Purpose:** Modernize existing modules to CQRS architecture
- **When to Use:** Refactoring legacy code to use MediatR/FluentValidation
- **Example:** `@module-rebuilder Rebuild Module_Receiving using CQRS patterns`

### 4. **Core Maintainer** 🛡️

- **Command:** `@core-maintainer`
- **Purpose:** Safely maintain and evolve Module_Core infrastructure
- **When to Use:** Adding/modifying/removing Core services
- **Example:** `@core-maintainer Add ShowWarning method to IService_ErrorHandler`

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

## 🎯 Agent Selection Guide

| Your Goal | Use This Agent | Reason |
|-----------|----------------|--------|
| Start a new WinUI 3 project | **Module Core Creator** | Creates foundational infrastructure |
| Add a new feature to existing project | **Module Creator** | Scaffolds complete feature module |
| Modernize old code to CQRS | **Module Rebuilder** | Migrates to MediatR/FluentValidation |
| Add method to IService_ErrorHandler | **Core Maintainer** | Impact analysis + safe changes |
| Change LoggingBehavior | **Core Maintainer** | Affects all modules globally |
| Create Module_Reporting | **Module Creator** | New feature module |
| Migrate Module_Routing to handlers | **Module Rebuilder** | Legacy → modern architecture |

---

## 🔄 Agent Collaboration Example

**Scenario:** Building a complete manufacturing application from scratch

```
Step 1: @module-core-creator
        Create Module_Core for "MTM Manufacturing System"
        
Step 2: @module-creator
        Create Module_Receiving from specs/Module_Receiving_Specification.md
        
Step 3: @module-creator
        Create Module_Routing from specs/Module_Routing_Specification.md
        
Step 4: @module-creator
        Create Module_Dunnage from specs/Module_Dunnage_Specification.md
        
Step 5: @core-maintainer
        Add IService_EventAggregator for module communication
        
Step 6: @module-rebuilder
        Modernize Module_Reporting (was added manually)
```

**Result:** Production-ready application with clean architecture!

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

- **Module Core Creator:** Uses all setup/scaffolding patterns
- **Module Creator:** Uses Diagrams 16-17 (spec structure, scaffolding workflow)
- **Module Rebuilder:** Uses Diagrams 11-12 (workflow extraction, service classification)
- **Core Maintainer:** Uses Diagram 13 (impact analysis)

---

## ⚙️ Agent Configuration

Agents are configured in `.github/copilot-agents.json`:

```json
{
  "agents": [
    {
      "name": "module-core-creator",
      "instructions": "file:_bmad/module-agents/agents/module-core-creator.md",
      "model": "claude-sonnet-4.5",
      "tools": ["create_file", "read_file", ...]
    },
    {
      "name": "module-creator",
      "instructions": "file:_bmad/module-agents/agents/module-creator.md",
      ...
    },
    ...
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

- Module Core Creator: `_bmad/module-agents/agents/module-core-creator.md`
- Module Creator: `_bmad/module-agents/agents/module-creator.md`
- Module Rebuilder: `_bmad/module-agents/agents/module-rebuilder.md`
- Core Maintainer: `_bmad/module-agents/agents/core-maintainer.md`
