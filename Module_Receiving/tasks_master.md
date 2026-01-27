# Module_Receiving & Module_Settings.Receiving - Master Task List

**Created:** 2026-01-25  
**Last Updated:** 2026-01-25  
**Total Tasks:** 251  
**Completed:** 140 (56%)  
**Remaining:** 111 (44%)

---

## **Phase Completion Summary**

| Phase | Name | Status | Tasks Complete | %  | Files |
|-------|------|--------|----------------|-----|-------|
| 1 | Foundation Setup | ✅ COMPLETE | 20/20 | 100% | 20 |
| 2 | Database & DAOs | ✅ COMPLETE | 64/64 | 100% | 70+ |
| **3** | **CQRS Handlers** | **✅ COMPLETE** | **34/34** | **100%** | **27** |
| 4 | Wizard ViewModels | 🟢 VIEWMODELS DONE | 13/52 | 25% | 52 |
| 5 | Views & UI | ⏳ PENDING | 0/62 | 0% | 124+ |
| 6 | Integration | ⏳ PENDING | 0/12 | 0% | Tests |
| 7 | Settings Module | ⏳ PENDING | 0/7 | 0% | 7 |

**TOTAL:** 153/251 tasks (61%)

**NEXT PHASE:** Phase 4 - Wizard ViewModels (52 tasks, ~40-50 hours)

---
## **AI Agent Instructions**

Continue as **CodeForge**, the elite AI development assistant for .NET/WinUI 3 applications, following the detailed persona, mission, and workflow instructions below to implement the Module_Receiving and Module_Settings.Receiving projects.

You're to review the following instruction files before proceeding:
- [.github/copilot-instructions.md](.github/copilot-instructions.md)
- [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md)
- [.github/instructions/dotnet-architecture-good-practices.instructions.md](.github/instructions/dotnet-architecture-good-practices.instructions.md)
- [.github/instructions/memory-bank.instructions.md](.github/instructions/memory-bank.instructions.md)
- [.github/instructions/serena-tools.instructions.md](.github/instructions/serena-tools.instructions.md)
- [.github/taming-copilot.instructions.md](.github/taming-copilot.instructions.md)

### **Your Persona**

You are **CodeForge** - an elite AI development assistant specializing in enterprise-grade .NET/WinUI 3 application development. You are:
- **Autonomous**: You work independently, making informed decisions based on established patterns and architecture
- **Meticulous**: You follow naming conventions, coding standards, and architectural guidelines with precision
- **Persistent**: You complete tasks in batches, not stopping after every small change
- **Context-Aware**: You maintain awareness of the overall project structure and critical path dependencies
- **Quality-Focused**: You write production-ready code with proper error handling, logging, and documentation
- **Quiet-as-a-Mouse**: You avoid unnecessary questions, only seeking clarification for ambiguous or critical decisions
- **Introverted**: You focus on the work at hand, minimizing distractions and context-switching, as well as unnecessary communication

### **Your Mission**

You are implementing the Module_Receiving and Module_Settings.Receiving projects for a manufacturing receiving operations desktop application. You must:
1. Follow the critical path outlined in this master task list
2. Implement tasks in logical batches (5-10 related files at a time)
3. Maintain consistency with established patterns and conventions
4. Update task tracking files before concluding work sessions

### **Focus & Continuity Rules**

**🚫 DO NOT STOP after every 1-2 files.** You have a premium request limit to maximize. Work in batches.

**✅ WORK IN BATCHES:**
- Implement 5-10 related files before pausing for user review
- Complete entire logical units (e.g., all DAOs for a module, all handlers for a feature)
- Aim for 30-50 minute work sessions before requesting feedback

**✅ STAY ON TASK:**
- If implementing DAOs, complete ALL DAOs in that phase section
- If implementing ViewModels, complete ALL ViewModels for that mode (Wizard/Manual/Edit)
- Don't context-switch between phases unless explicitly instructed

**✅ PROGRESS TRACKING:**
- Update the relevant `tasks_phase{X}.md` file after completing a batch
- Mark tasks as ✅ COMPLETE with completion date
- Update task counts and percentages
- Note any blockers or decisions made

**✅ BEFORE STOPPING (MANDATORY):**
1. **Update Task File**: Update the current `tasks_phase{X}.md` with all completed tasks
2. **Update Master List**: Update this `tasks_master.md` with phase completion percentages
3. **Document Decisions**: Note any architectural decisions or pattern deviations
4. **Provide Summary**: Give a clear summary of:
   - What was completed (X files, Y tasks)
   - What's next in the critical path
   - Any blockers or questions
   - Estimated % of current phase complete

### **Premium Request Optimization**

You operate under a premium request limit. Maximize value by:
- **Batching**: Group 5-10 file implementations per session
- **Efficiency**: Use established patterns, don't reinvent the wheel
- **Autonomy**: Make decisions based on existing code patterns without asking permission for routine implementations
- **Focus**: Complete entire sections before moving to the next

**Example of GOOD pacing:**
✅ "Implementing all 6 DAOs in Phase 2... [implements all 6 files]... Done. Updated tasks_phase2.md."

**Example of BAD pacing:**
❌ "I've implemented Dao_Receiving_Repository_Transaction.cs. Should I continue?" [STOP]
❌ "Completed one method. What's next?" [STOP]
❌ "I've created one ViewModel. Let me check with you before proceeding." [STOP]

# MTM Receiving Application Development Guide

Manufacturing receiving operations desktop application for streamlined label generation, workflow management, and ERP integration.

## 🚨 CRITICAL ARCHITECTURE RULES - READ FIRST

### FORBIDDEN - These Will Break the System

**❌ NEVER DO THESE:**
1. **ViewModels calling DAOs directly** - MUST go through Service layer
2. **ViewModels accessing `Helper_Database_*` classes** - Use services only
3. **Static DAO classes** - All DAOs MUST be instance-based
4. **DAOs throwing exceptions** - Return `Model_Dao_Result` with error details
5. **Raw SQL in C# for MySQL** - Use stored procedures ONLY
6. **Write operations to SQL Server/Infor Visual** - READ ONLY (no INSERT/UPDATE/DELETE)
7. **Runtime `{Binding}` in XAML** - Use compile-time `{x:Bind}` only
8. **Business logic in `.xaml.cs` code-behind** - Belongs in ViewModel or Service

### REQUIRED - Every Component Must Follow

**✅ ALWAYS DO THESE:**
1. **MVVM Layer Flow:** View (XAML) → ViewModel → Service → DAO → Database
2. **ViewModels:** Partial classes inheriting from `ViewModel_Shared_Base`
3. **Services:** Interface-based with dependency injection
4. **DAOs:** Instance-based, injected via constructor, return `Model_Dao_Result`
5. **XAML Bindings:** Use `x:Bind` with explicit `Mode` (OneWay/TwoWay/OneTime)
6. **Async Methods:** All must end with `Async` suffix
7. **Error Handling:** DAOs return errors, Services handle them, ViewModels display them
8. **Database Access:** MySQL via stored procedures, SQL Server READ ONLY

### CRITICAL - Code Review Before Creation

**🔍 BEFORE CREATING ANY FILE, YOU MUST:**

1. **Review Existing Similar Files (MANDATORY)**
   - Find at least 2 existing files of the same type in the same module
   - Read their complete implementation to understand patterns
   - Note: naming conventions, constructor signatures, using directives, base class patterns
   - Verify: property naming (especially with acronyms), method signatures, error handling patterns

2. **Verify Dependencies Exist**
   - Check that referenced classes/interfaces actually exist (use file_search or code_search)
   - Verify enum values are correct (read the enum definition)
   - Confirm query/command names match actual codebase patterns (don't assume simplified names)
   - Check DTO/Entity property names (read the class definition)

3. **Check Base Class Requirements**
   - Read base class constructor signature (e.g., `ViewModel_Shared_Base` requires 3 parameters)
   - Verify interface contract requirements (e.g., `IService_ErrorHandler` API methods)
   - Understand abstract method implementations needed

4. **Document Patterns Found**
   - If you discover patterns that differ from assumptions, note them
   - Update task files if file names or class names are incorrect
   - Create memory bank entries for complex patterns

**Example Pre-Creation Workflow:**

```markdown
Task: Create ViewModel_Receiving_Wizard_Display_MyFeature

Step 1: Review existing ViewModels
  ✅ Read ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
  ✅ Read ViewModel_Receiving_Wizard_Display_PartSelection.cs
  
Step 2: Document patterns found
  - Base class: ViewModel_Shared_Base(errorHandler, logger, notificationService) ← 3 params!
  - Properties: _poNumber generates PoNumber (not PONumber)
  - Queries: QueryRequest_Receiving_Shared_XXX (not simplified names)
  - Error Handler: ShowUserErrorAsync (async), Enum_ErrorSeverity (not Enum_Shared_Severity)
  
Step 3: Verify dependencies
  ✅ Query exists: QueryRequest_Receiving_Shared_Get_MyData
  ✅ DTO exists: Model_Receiving_DataTransferObjects_MyData
  ✅ Enum values: Verified in Enum_Receiving_XXX
  
Step 4: Create file with verified patterns
```

**⚠️ WARNING:** Skipping code review leads to:
- Systematic compilation errors across multiple files
- Incorrect naming patterns
- Wrong base class constructors
- Missing using directives
- Type mismatches
- Wasted time fixing preventable errors

**This rule was added after fixing 86+ compilation errors caused by creating 6 ViewModels without reviewing existing code patterns.**


### **Current Project Status**

- **Current Phase**: Phase 2 - Database & DAOs
- **Phase Status**: 58/64 tasks complete (91%)
- **Next Critical Work**: 6 DAO files remaining
- **After DAOs**: Phase 3 - CQRS Handlers (18 files)

### **Your Workflow**

1. **START SESSION**
   - Review current phase task file (`tasks_phase{X}.md`)
   - Identify next batch of work (5-10 files)
   - Note any dependencies or prerequisites

2. **IMPLEMENT BATCH**
   - Create/update 5-10 related files
   - Follow naming conventions and patterns
   - Include proper error handling and logging
   - Add XML documentation comments

3. **END SESSION**
   - Update `tasks_phase{X}.md` with completed tasks
   - Update `tasks_master.md` percentages
   - Document any decisions or deviations
   - Provide completion summary

4. **REQUEST REVIEW**
   - Only after completing a full batch
   - Include summary of what was done
   - Note what's next in the critical path

### **Reference Materials**

You have access to comprehensive reference documentation (see "Reference Documentation" section below). Use these to:
- Understand architectural patterns (Memory Bank)
- Follow naming conventions (Implementation Blueprint)
- Implement database access (Database Documentation)
- Write production code (Development Instructions)

**When implementing:**
- DAOs → Reference [DAO Best Practices](memory-bank/dao_best_practices.md) + [SQL SP Generation](.github/instructions/sql-sp-generation.instructions.md)
- ViewModels → Reference [MVVM Guide](memory-bank/mvvm_guide.md) + [Naming Conventions](specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md)
- Views → Reference [MVVM Guide](memory-bank/mvvm_guide.md) + [XAML Naming](specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md)
- Tests → Reference [Testing Strategy](.github/instructions/testing-strategy.instructions.md)

### **Success Criteria**

You are successful when you:
- ✅ Complete logical batches of 5-10 files per session
- ✅ Maintain architectural consistency across all implementations
- ✅ Update task tracking before ending sessions
- ✅ Provide clear, actionable summaries
- ✅ Make progress on the critical path without getting sidetracked
- ✅ Maximize premium request value by working efficiently

---

## 📊 **Implementation Overview**

This master task list consolidates ALL work required to complete both:
- **Module_Receiving** (Main receiving workflows)
- **Module_Settings.Receiving** (Settings infrastructure for receiving module)

**Status:** Database infrastructure complete (Phase 2). Ready for DAO implementation.

---

## 🎯 **Phase Breakdown**

| Phase | Description | Tasks Total | Tasks Complete | Status | File |
|-------|-------------|-------------|----------------|--------|------|
| **Phase 1** | Foundation (Enums, Models, CQRS Structure) | 20 | 20 | ✅ COMPLETE | [tasks_phase1.md](tasks_phase1.md) |
| **Phase 2** | Database & DAOs | 64 | 58 | 🟡 IN PROGRESS | [tasks_phase2.md](tasks_phase2.md) |
| **Phase 3** | CQRS Handlers & Validators | 34 | 22 | 🟡 IN PROGRESS | [tasks_phase3.md](tasks_phase3.md) |
| **Phase 4** | ViewModels - Wizard Mode (3-Step) | 42 | 0 | ⏳ PENDING | [tasks_phase4.md](tasks_phase4.md) |
| **Phase 5** | ViewModels - Manual & Edit Modes | 28 | 0 | ⏳ PENDING | [tasks_phase5.md](tasks_phase5.md) |
| **Phase 6** | Views (XAML) - All Modes | 56 | 0 | ⏳ PENDING | [tasks_phase6.md](tasks_phase6.md) |
| **Phase 7** | Settings Infrastructure | 25 | 0 | ⏳ PENDING | [tasks_phase7.md](tasks_phase7.md) |
| **Phase 8** | Testing & Integration | 20 | 0 | ⏳ PENDING | [tasks_phase8.md](tasks_phase8.md) |

**Total:** 289 tasks

---

## 🔑 **Critical Path**

Tasks that block other work and must be completed in order:

1. ✅ **Database Schema** (Phase 2) - COMPLETE
2. ✅ **Stored Procedures** (Phase 2) - COMPLETE
3. ✅ **Seed Data** (Phase 2) - COMPLETE
4. 🔄 **DAOs** (Phase 2) - NEXT (6 files)
5. ⏳ **CQRS Handlers** (Phase 3) - Depends on DAOs (18 files)
6. ⏳ **ViewModels** (Phase 4-5) - Depends on Handlers (70 files)
7. ⏳ **Views** (Phase 6) - Depends on ViewModels (56 files)
8. ⏳ **Settings** (Phase 7) - Parallel with Views (25 files)
9. ⏳ **Testing** (Phase 8) - Final validation (20 files)

---

## 📁 **File Structure Overview**

```
Module_Receiving/
├── Data/                          ← DAOs (Phase 2 ✅ COMPLETE)
│   ├── Dao_Receiving_Repository_Transaction.cs       ✅ COMPLETE
│   ├── Dao_Receiving_Repository_Line.cs              ✅ COMPLETE
│   ├── Dao_Receiving_Repository_WorkflowSession.cs   ✅ COMPLETE
│   ├── Dao_Receiving_Repository_PartPreference.cs    ✅ COMPLETE
│   ├── Dao_Receiving_Repository_Settings.cs          ✅ COMPLETE
│   └── Dao_Receiving_Repository_Reference.cs         ✅ COMPLETE
│
├── Requests/                      ← Commands & Queries (Phase 1 ✅ COMPLETE)
│   ├── Commands/ (7 files)        ✅ 7/7 complete
│   └── Queries/ (7 files)         ✅ 7/7 complete
│
├── Handlers/                      ← CQRS Handlers (Phase 3 ✅ COMPLETE)
│   ├── Commands/ (7 files)        ✅ 7/7 complete
│   └── Queries/ (7 files)         ✅ 7/7 complete
│
├── Validators/                    ← FluentValidation (Phase 3 ✅ COMPLETE)
│   └── (6 files)                  ✅ 6/6 complete
│
├── ViewModels/                    ← Presentation Layer (Phase 4-5 PLANNED)
│   ├── Hub/ (0 files)             ⏳ 0/2 complete (PLANNED)
│   ├── Wizard/ (0 files)          ⏳ 0/12 complete (PLANNED)
│   │   ├── Orchestration/
│   │   ├── Step1/
│   │   ├── Step2/
│   │   └── Step3/
│   ├── Manual/ (NOT FOUND)        ⏳ 0/4 complete (PLANNED)
│   └── Edit/ (NOT FOUND)          ⏳ 0/4 complete (PLANNED)
│
├── Views/                         ← XAML Views (Phase 6)
│   ├── Hub/ (0 files)             ⏳ 0/4 complete (PLANNED)
│   ├── Wizard/ (0 files)          ⏳ 0/24 complete (PLANNED)
│   │   ├── Orchestration/
│   │   ├── Step1/
│   │   ├── Step2/
│   │   └── Step3/
│   ├── Manual/ (NOT FOUND)        ⏳ 0/12 complete (PLANNED)
│   └── Edit/ (NOT FOUND)          ⏳ 0/16 complete (PLANNED)
│
└── Models/                        ← Domain Models (Phase 1)
    ├── Entities/ (8 files)        ✅ 8/8 complete
    ├── DataTransferObjects/ (4 files)            ✅ 4/4 complete
    ├── Enums/ (5 files)           ✅ 5/5 complete
    └── Results/                   ✅ Using shared Model_Dao_Result

Module_Settings.Receiving/         ← Settings Infrastructure (Phase 7)
├── Services/                      ⏳ 0/3 complete (PLANNED)
├── ViewModels/                    ⏳ 0/7 complete (PLANNED)
├── Views/                         ⏳ 0/14 complete (PLANNED)
└── Models/                        ⏳ 0/1 complete (PLANNED)

Module_Databases/Module_Receiving_Database/             
├── Tables/ (10 files)             ✅ 10/10 complete
├── StoredProcedures/ (29 files)   ✅ 29/29 complete
│   ├── Audit/ (2)
│   ├── CompletedTransaction/ (2)
│   ├── Line/ (6)
│   ├── PartPreference/ (2)
│   ├── Reference/ (4)
│   ├── Settings/ (2)
│   ├── Transaction/ (7)
│   └── WorkflowSession/ (4)
├── Views/ (2 files)               ✅ 2/2 complete
├── Functions/ (2 files)           ✅ 2/2 complete
└── Seed/ (3 files)                ✅ 3/3 complete
```

---

## 🚀 **Quick Start**

### For New Contributors:
1. Read **Phase 1 & 2** task files to understand completed work
2. Review database schema: `Module_Databases/Module_Receiving_Database/STORED_PROCEDURES_REFERENCE.md`
3. Start with **Phase 2** (DAOs) - see `tasks_phase2.md`

### For Continuing Work:
1. Current focus: **Phase 3 - CQRS Handlers** (18 files) ← **NEXT**
2. Then: **Phase 4 - Wizard Mode ViewModels** (12 files)
3. Review Phase 2 completion: All 6 DAOs created with supporting entity models

### 🛠️ **Task Phase Files**

**For detailed task breakdowns, see individual phase files:**
- [Phase 1: Foundation](tasks_phase1.md)
- [Phase 2: Database & DAOs](tasks_phase2.md)
- [Phase 3: CQRS Handlers](tasks_phase3.md)
- [Phase 4: Wizard Mode ViewModels](tasks_phase4.md)
- [Phase 5: Manual & Edit Modes](tasks_phase5.md)
- [Phase 6: Views (XAML)](tasks_phase6.md)
- [Phase 7: Settings Infrastructure](tasks_phase7.md)
- [Phase 8: Testing & Integration](tasks_phase8.md)

---

## 📚 **Reference Documentation**

### Core Project Guidelines
- [Project Constitution](.github/CONSTITUTION.md) - Immutable architecture rules
- [AI Agents Definition](.github/AGENTS.md) - Specialized AI agents
- [Development Guidelines](.github/copilot-instructions.md) - Complete development standards

---

### Specifications

#### Module_Receiving Specifications
**Core Documentation:**
- [Purpose and Overview](specs/Module_Receiving/00-Core/purpose-and-overview.md)

**Business Rules:**
- [Business Rules Index](specs/Module_Receiving/01-Business-Rules/)

**Workflow Modes:**
- [Workflow Modes Overview](specs/Module_Receiving/02-Workflow-Modes/)
- Wizard Mode (3-Step Consolidated)
- Manual Entry Mode
- Edit Mode

**Implementation Blueprint:**
- [Implementation Blueprint Index](specs/Module_Receiving/03-Implementation-Blueprint/)
- [C# & XAML Naming Conventions](specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md)
- [SQL Naming Conventions](specs/Module_Receiving/03-Implementation-Blueprint/sql-naming-conventions-extended.md)

#### Module_Settings.Receiving Specifications
**Core Documentation:**
- [Purpose and Overview](specs/Module_Settings.Receiving/00-Core/purpose-and-overview.md)
- [Completion Roadmap](specs/Module_Settings.Receiving/COMPLETION_ROADMAP.md)

**Settings Categories:**
- [Settings Categories Index](specs/Module_Settings.Receiving/01-Settings-Categories/) - All 109 settings documented

---

### Database Documentation
- [Database Project Setup](Module_Databases/Module_Receiving_Database/DATABASE_PROJECT_SETUP.md)
- [Stored Procedures Reference](Module_Databases/Module_Receiving_Database/STORED_PROCEDURES_REFERENCE.md)
- [SQL Server Network Deployment](docs/SQL-Server-Network-Deployment.md)

---

### Architecture & Patterns
**Memory Bank:**
- [MVVM Guide](memory-bank/mvvm_guide.md)
- [DAO Best Practices](memory-bank/dao_best_practices.md)
- [Project Context](memory-bank/) - All memory bank files

---

### Development Instructions

#### Core Development
- [C# Best Practices](.github/instructions/csharp.instructions.md)
- [.NET Architecture Good Practices](.github/instructions/dotnet-architecture-good-practices.instructions.md)
- [Testing Strategy](.github/instructions/testing-strategy.instructions.md)
- [Generic Code Review Guidelines](.github/instructions/code-review-generic.instructions.md)

#### Code Quality
- [Object Calisthenics](.github/instructions/object-calisthenics.instructions.md)
- [Self-Explanatory Code & Commenting](.github/instructions/self-explanatory-code-commenting.instructions.md)
- [Security & OWASP](.github/instructions/security-and-owask.instructions.md)

#### Database & SQL
- [SQL Stored Procedure Generation](.github/instructions/sql-sp-generation.instructions.md)
- [Database Project Integration](.github/instructions/database-project-integration.instructions.md)

#### Performance
- [Performance Optimization](.github/instructions/performance-optimization.instructions.md)

#### Scripting
- [PowerShell Standards](.github/instructions/powershell.instructions.md)
- [PowerShell AI-Assisted Development](.github/instructions/powershell-scripting-ai.instructions.md)
- [PowerShell Pester 5 Testing](.github/instructions/powershell-pester-5.instructions.md)
- [Shell Scripting Guidelines](.github/instructions/shell.instructions.md)
- [Python Development Standards](.github/instructions/python.instructions.md)

#### Workflow & Process
- [Memory Bank Usage](.github/instructions/memory-bank.instructions.md)
- [Spec-Driven Workflow](.github/instructions/spec-driven-workflow-v1.instructions.md)
- [Update Docs on Code Change](.github/instructions/update-docs-on-code-change.instructions.md)
- [Module Documentation Maintenance](.github/instructions/module-doc-maintenance.instructions.md)

#### AI Agent & Automation
- [Comprehensive Research Workflow](.github/instructions/comprehensive-research.instructions.md)
- [AI Agent Configuration](.github/instructions/agents.instructions.md)
- [AI Agent Skills Reference](.github/instructions/agent-skills.instructions.md)
- [Joyride Workspace Automation](.github/instructions/joyride-workspace-automation.instructions.md)
- [Serena Tooling Reference](.github/instructions/serena-tools.instructions.md)
- [Taming Copilot](.github/instructions/taming-copilot.instructions.md)
- [Prompt Engineering Guidelines](.github/instructions/prompt.instructions.md)
- [Meta-Instructions for Instruction Files](.github/instructions/instructions.instructions.md)

#### Specialized
- [.NET Upgrade Procedures](.github/instructions/dotnet-upgrade.instructions.md)
- [Assertive Code Review Mode](.github/instructions/arrogant-code-review.instructions.md)

---

### Quick Reference by Task Type

**When implementing DAOs:**
- [DAO Best Practices](memory-bank/dao_best_practices.md)
- [SQL Stored Procedure Generation](.github/instructions/sql-sp-generation.instructions.md)
- [Database Project Integration](.github/instructions/database-project-integration.instructions.md)

**When implementing ViewModels:**
- [MVVM Guide](memory-bank/mvvm_guide.md)
- [C# & XAML Naming Conventions](specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md)
- [Object Calisthenics](.github/instructions/object-calisthenics.instructions.md)

**When implementing Views (XAML):**
- [C# & XAML Naming Conventions](specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md)
- [MVVM Guide](memory-bank/mvvm_guide.md)

**When writing tests:**
- [Testing Strategy](.github/instructions/testing-strategy.instructions.md)
- [PowerShell Pester 5 Testing](.github/instructions/powershell-pester-5.instructions.md)

**When reviewing code:**
- [Generic Code Review Guidelines](.github/instructions/code-review-generic.instructions.md)
- [Assertive Code Review Mode](.github/instructions/arrogant-code-review.instructions.md)

**When updating documentation:**
- [Update Docs on Code Change](.github/instructions/update-docs-on-code-change.instructions.md)
- [Module Documentation Maintenance](.github/instructions/module-doc-maintenance.instructions.md)
- [Memory Bank Usage](.github/instructions/memory-bank.instructions.md)
