# GitHub Copilot Coding Agent — Task Deep Dive

> **Task ID:** `af343bac-93aa-43fc-bf21-d7046019328f`
> **Source URL:** <https://www.github.com/JDKoll1982/MTM_Receiving_Application/tasks/af343bac-93aa-43fc-bf21-d7046019328f>
> **Repository:** `JDKoll1982/MTM_Receiving_Application`
> **Last Updated:** 2026-04-05

---

## Table of Contents

1. [What This Task Was](#1-what-this-task-was)
2. [GitHub Copilot Coding Agent — How It Works](#2-github-copilot-coding-agent--how-it-works)
3. [Session UUIDs and the Task Lifecycle](#3-session-uuids-and-the-task-lifecycle)
4. [MTM Receiving Application — Project Overview](#4-mtm-receiving-application--project-overview)
5. [How Copilot Integrates With This Project](#5-how-copilot-integrates-with-this-project)
6. [MVVM Architecture — Deep Dive](#6-mvvm-architecture--deep-dive)
7. [Database Integration Patterns](#7-database-integration-patterns)
8. [Security Model and Constraints](#8-security-model-and-constraints)
9. [Custom Agent Configuration (`.github/agents/`)](#9-custom-agent-configuration-githubagents)
10. [Copilot Instructions and Governance](#10-copilot-instructions-and-governance)
11. [Recommendations for Effective Copilot Use](#11-recommendations-for-effective-copilot-use)
12. [References and Further Reading](#12-references-and-further-reading)

---

## 1. What This Task Was

This task was initiated through the **GitHub Copilot Coding Agent** task interface. Its original
instruction was:

> *"Read the returned response from `https://www.github.com/JDKoll1982/MTM_Receiving_Application/tasks/af343bac-93aa-43fc-bf21-d7046019328f`
> and create an MD file in the root directory with the response."*

The document you are reading is the **improved, research-enriched version** of that initial
response. Rather than a one-line summary, this document captures a complete picture of:

- How the Copilot coding agent works
- What this specific repository is and how it is structured
- How Copilot is deeply embedded into the MTM Receiving Application's development workflow
- Architectural best practices relevant to this codebase
- Actionable recommendations for maximising Copilot's value on this project

---

## 2. GitHub Copilot Coding Agent — How It Works

### 2.1 What Is the Coding Agent?

GitHub Copilot Coding Agent is a **fully autonomous, AI-driven developer** that can be
assigned to GitHub Issues, pull request reviews, or bespoke tasks. Unlike Copilot Chat
(which answers questions in real time), the coding agent works *asynchronously* in an
isolated environment — it reads code, writes files, runs builds and tests, and opens a pull
request for human review.

The agent is powered by an advanced large language model (Claude Sonnet 4.5 is the default
model for the MTM project, as configured in `AGENTS.md`) and orchestrated via GitHub Actions
runners.

### 2.2 How a Task Is Initiated

You can delegate work to the coding agent in four ways:

| Method | How |
|---|---|
| **Assign issue to Copilot** | In a GitHub Issue, set the assignee to `@copilot` |
| **Agents panel (GitHub.com)** | Click *New Task* in the Agents tab |
| **VS Code** | Use the Agents view in Copilot Chat |
| **GitHub CLI** | `gh copilot-agent start --repo <owner/repo> --issue <number>` |

Once triggered, Copilot:
1. Reads the issue/task description and all linked context
2. Spins up a secure, sandboxed Actions runner
3. Clones the repository into a temporary workspace
4. Reads instructions, memories, and configuration files (`AGENTS.md`, `.github/copilot-instructions.md`, `.serena/memories/`, `.github/agents/`)
5. Plans, edits, builds, tests, and prepares a pull request
6. Opens a draft PR on a `copilot/*` branch for human review

### 2.3 Sandbox Security Model

Copilot's execution environment enforces strict isolation:

- **Network controls** — restricted outbound access; no arbitrary internet calls
- **Branch protections** — all changes land on `copilot/*` branches; no direct pushes to `main`
- **No auto-merge** — CI/CD does not run without a human approving the PR first
- **Co-authored commits** — every commit is tagged `Co-Authored-By: @copilot`, creating a full audit trail
- **Read-only token scope** — the GitHub token is scoped to the source repository only

---

## 3. Session UUIDs and the Task Lifecycle

### 3.1 What Is a Task UUID?

Every Copilot coding agent session is assigned a **UUID (Universally Unique Identifier)**.

For this task:

```
af343bac-93aa-43fc-bf21-d7046019328f
```

This UUID serves as the *session key* that ties together all agent actions, logs, file edits,
PR comments, sub-sessions, and metadata for the duration of the task. It is used to:

- Reconstruct the task state after an interruption
- Track token usage and cost attribution
- Link PR review comments to agent sub-sessions
- Provide an audit-friendly timeline of every step the agent took

### 3.2 Task Lifecycle Stages

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   GITHUB COPILOT TASK LIFECYCLE                         │
│                                                                         │
│  1. INITIATION                                                          │
│     └── Task assigned or created → UUID generated                      │
│                                                                         │
│  2. ENVIRONMENT SETUP                                                   │
│     └── Secure Actions runner spawned → repo cloned                    │
│                                                                         │
│  3. CONTEXT LOADING                                                     │
│     └── AGENTS.md + .github/copilot-instructions.md + memories read   │
│                                                                         │
│  4. PLANNING                                                            │
│     └── Agent analyses the task → selects files to read/edit           │
│                                                                         │
│  5. EXECUTION                                                           │
│     └── Code edits → builds → tests → iterative refinement             │
│                                                                         │
│  6. PR CREATION                                                         │
│     └── Draft pull request opened on copilot/* branch                  │
│                                                                         │
│  7. REVIEW & FEEDBACK                                                   │
│     └── Human reviews → comments → agent spins sub-session             │
│         (same UUID family, new sub-session UUID)                        │
│                                                                         │
│  8. CLOSURE                                                             │
│     └── PR merged or closed → session archived                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Inspecting a Session

Sessions can be inspected at any time from:

- **GitHub.com → Copilot → Agents tab** — live logs, timeline, and artifact links
- **Session URL** — `https://github.com/{owner}/{repo}/tasks/{uuid}`
- **VS Code Agents view** — live streaming of current session output

---

## 4. MTM Receiving Application — Project Overview

### 4.1 Purpose

The **MTM Receiving Application** is a **WinUI 3 desktop application** built for manufacturing
plant receiving operations. It is used daily to:

- Process and log incoming materials against purchase orders from Infor Visual ERP
- Generate AIAG MH10.8.2 compliant shipping/receiving labels
- Track receiving history, quantities, lot numbers, and employee activity
- Integrate in real time with the plant's Infor Visual SQL Server database (read-only)
- Write all application data to a local MySQL 5.7 database via stored procedures

### 4.2 Technology Stack

| Layer | Technology |
|---|---|
| **UI Framework** | WinUI 3 (Windows App SDK 1.8+) |
| **Language** | C# 13 |
| **Platform** | .NET 10 |
| **Architecture** | MVVM with CommunityToolkit.Mvvm |
| **Read/Write DB** | MySQL 5.7 (`mtm_receiving_application`) |
| **Read-Only DB** | SQL Server — Infor Visual ERP (`MTMFG`) |
| **Testing** | xUnit with FluentAssertions |
| **DI Container** | Microsoft.Extensions.DependencyInjection |
| **Logging** | Structured logging via `IService_LoggingUtility` |
| **Code Analysis** | Serena MCP Server (Roslyn LSP) |

### 4.3 Module Structure

| Module | Responsibility |
|---|---|
| `Module_Core` | Shared base classes, error handling, logging, DI, settings |
| `Module_Receiving` | Main receiving workflow — mode selection, PO lookup, label printing |
| `Module_Dunnage` | Dunnage (packaging material) tracking and management |
| `Module_Reporting` | Receiving history viewer, search, export |
| `Module_Settings.Core` | Application-level and user-level settings |
| `Infrastructure/` | DI registrations, configuration, logging infrastructure |
| `Database/` | MySQL stored procedures, SQL Server queries, deployment scripts |

### 4.4 Application Data Flow

```
┌───────────────────────────────────────────────────────────────────┐
│                    DATA FLOW DIAGRAM                              │
│                                                                   │
│  Infor Visual ERP (SQL Server)  ←──── READ ONLY ────────────┐   │
│           (MTMFG database)                                   │   │
│                                                              ↓   │
│  WinUI 3 View (XAML)                                         │   │
│       ↕ x:Bind (compile-time)                                │   │
│  ViewModel (partial, ObservableObject)           ┌───────────┘   │
│       ↕ IService interface                        │               │
│  Service Layer (business logic)         DAO queries via           │
│       ↕ Dao instance                     Helper_Database_         │
│  DAO Layer (instance-based)               SqlServer               │
│       ↕ Stored Procedures                                         │
│  MySQL 5.7 (mtm_receiving_application)                            │
│  ← READ / WRITE via stored procs only →                           │
└───────────────────────────────────────────────────────────────────┘
```

---

## 5. How Copilot Integrates With This Project

### 5.1 Custom Agent Definition (`AGENTS.md`)

The project's `AGENTS.md` file defines the **MTM Receiving Specialist** — a custom Copilot
coding agent configured specifically for this repository:

```yaml
name: "MTM Receiving Specialist"
tools: ["read", "edit", "search", "execute"]
model: "Claude Sonnet 4.5"
target: "vscode"
infer: true
```

The agent is instructed to enforce the project's MVVM architecture, naming conventions, and
database access rules *before it writes a single line of code*.

### 5.2 Copilot Instructions File

`.github/copilot-instructions.md` contains the **immutable architecture constitution** for
the project. Key non-negotiable rules enforced on every Copilot session:

| Rule | Constraint |
|---|---|
| **Layer flow** | View → ViewModel → Service → DAO → Database |
| **ViewModels** | Must be `partial`, must inherit `ViewModel_Shared_Base` |
| **DAOs** | Instance-based (never static), return `Model_Dao_Result`, never throw |
| **MySQL SQL** | Stored procedures only — no raw SQL strings in C# |
| **Infor Visual** | READ ONLY — `ApplicationIntent=ReadOnly` mandatory |
| **XAML bindings** | `{x:Bind}` compile-time only — no runtime `{Binding}` |
| **Async methods** | Must end with `Async` suffix |
| **Business logic** | Never in `.xaml.cs` code-behind |
| **DI registration** | Always in `Infrastructure/DependencyInjection/` extension methods |

### 5.3 Serena AI Assistant (`/.serena/memories/`)

The project uses the **Serena MCP server** (backed by the Roslyn language server) to give
Copilot *semantic* code understanding. Serena's memory files cache the project's architectural
patterns across sessions:

| Memory File | Contents |
|---|---|
| `architectural_patterns.md` | MVVM layer rules, layer flow diagrams |
| `forbidden_practices.md` | Static DAOs, raw SQL, runtime `{Binding}`, etc. |
| `dao_best_practices.md` | DAO pattern, `Model_Dao_Result`, stored procedures |
| `mvvm_guide.md` | Complete ViewModel → Service → DAO walkthrough |
| `tech_stack.md` | .NET 10, WinUI 3, MySQL 5.7, CommunityToolkit.Mvvm |
| `infor_visual_constraints.md` | SQL Server READ ONLY rules |
| `error_handling_guide.md` | `IService_ErrorHandler` usage |

### 5.4 SpecKit Agents (`/.github/agents/`)

Several specialised **SpecKit** workflow agents are configured for use in VS Code:

| Agent | Purpose |
|---|---|
| `speckit.specify` | Create/update feature specifications from natural language |
| `speckit.plan` | Generate implementation design artifacts |
| `speckit.tasks` | Produce actionable, dependency-ordered `tasks.md` |
| `speckit.implement` | Execute an implementation plan across all tasks |
| `speckit.analyze` | Cross-artifact consistency and quality checks |
| `speckit.clarify` | Identify underspecified areas and ask targeted questions |
| `speckit.checklist` | Generate a feature-specific checklist |
| `noob-mode` | Plain-English, jargon-free assistant for non-technical stakeholders |
| `mtm-mcp-operator` | MCP-first operator with filesystem + GitHub + Serena tools |

---

## 6. MVVM Architecture — Deep Dive

CommunityToolkit.Mvvm is the MVVM backbone of this application. Understanding how it works
is essential for working effectively with Copilot on this codebase.

### 6.1 ViewModel Pattern

```csharp
// Every ViewModel follows this exact pattern:
[ObservableProperty]              // ← Generates INotifyPropertyChanged
private string _searchText = string.Empty;

[RelayCommand]                    // ← Generates ICommand
private async Task SearchAsync()
{
    if (IsBusy) return;           // ← Guard: prevent concurrent execution
    try
    {
        IsBusy = true;            // ← Shows loading indicators
        StatusMessage = "Searching...";
        var result = await _service.SearchAsync(SearchText);
        if (result.IsSuccess)
        {
            Items.Clear();
            foreach (var item in result.Data) Items.Add(item);
        }
        else
        {
            await _errorHandler.ShowUserErrorAsync(
                result.ErrorMessage, "Search Error", nameof(SearchAsync));
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(
            ex, Enum_ErrorSeverity.Medium, nameof(SearchAsync), nameof(ViewModel_X));
    }
    finally
    {
        IsBusy = false;           // ← Always restore to non-busy state
    }
}
```

### 6.2 DAO Pattern

```csharp
// Every DAO follows this pattern:
public class Dao_ReceivingLine
{
    private readonly string _connectionString;      // ← Constructor injection

    public async Task<Model_Dao_Result<List<Model_ReceivingLine>>>
        GetLinesByLoadAsync(int loadId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new("@p_LoadId", loadId)
            };
            return await Helper_Database_StoredProcedure
                .ExecuteListAsync<Model_ReceivingLine>(
                    "sp_Receiving_Line_GetByLoad",
                    parameters,
                    _connectionString);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLine>>(
                $"Error retrieving lines: {ex.Message}");
        }
    }
}
```

Key invariants enforced by this pattern:
- **No exceptions escape a DAO** — errors become `Model_Dao_Result` failure values
- **No raw SQL** — always calls a MySQL stored procedure by name
- **No static state** — `_connectionString` is injected at construction time

### 6.3 x:Bind Compile-Time Bindings

WinUI 3 supports two binding modes:

| Mode | Syntax | When Resolved | Performance |
|---|---|---|---|
| **Compile-time** | `{x:Bind ViewModel.Property, Mode=TwoWay}` | At build time | ✅ Excellent |
| **Runtime** | `{Binding Property}` | At runtime via reflection | ❌ Slow, fragile |

This project **exclusively uses `{x:Bind}`**. The runtime `{Binding}` form is forbidden
because it defeats type safety and generates subtle, hard-to-debug errors.

---

## 7. Database Integration Patterns

### 7.1 MySQL (Read/Write)

The application database is `mtm_receiving_application` running on MySQL 5.7. **All write
and read operations use stored procedures** — no C# code contains inline SQL strings.

Stored procedure conventions:
- Named `sp_<Entity>_<Action>` (e.g., `sp_Receiving_Line_Insert`)
- All parameters prefixed with `@p_` (e.g., `@p_LoadId`, `@p_Quantity`)
- Results returned as a single result set with consistent column order
- Error output via an `@p_Error` OUT parameter pattern

### 7.2 Infor Visual — SQL Server (Read-Only)

The plant ERP database (`MTMFG`) on SQL Server is **read-only by architectural mandate**:

```
ApplicationIntent=ReadOnly
```

This connection string attribute is verified on every DAO that accesses SQL Server.
The application **never issues INSERT, UPDATE, DELETE, or DDL** against this database.

Important schema facts (verified 2026-03-06):
- Tables prefixed `V_` (e.g., `V_ACCOUNT`, `V_CONTRACT`) are **base tables**, not views
- Only two actual SQL Server views exist: `CR_PART_LOCATION` and `SYSUSERAUTH`
- No stored procedures or user-defined functions exist in `MTMFG`; all queries are plain
  `SELECT` statements passed as parameterised command text

### 7.3 Helper Classes

| Helper | Purpose |
|---|---|
| `Helper_Database_Variables` | Centralises connection string retrieval; no hardcoding |
| `Helper_Database_StoredProcedure` | Executes MySQL stored procedures with parameters |
| `Helper_Database_SqlServer` | Executes read-only SQL Server `SELECT` queries |

---

## 8. Security Model and Constraints

### 8.1 Credentials

- Connection strings are **never hardcoded** in source files
- Credentials are stored in `appsettings.json` / `appsettings.Development.json` (gitignored)
- `Helper_Database_Variables` is the single point of connection string access

### 8.2 Infor Visual Read-Only Guarantee

The application enforces a hard architectural boundary: **no data is ever written to the
plant ERP**. This protects production manufacturing data from accidental mutation. All writes
target the local `mtm_receiving_application` MySQL database only.

### 8.3 Input Validation

- All parameters passed to stored procedures use parameterised `MySqlParameter` objects
- SQL injection is structurally impossible — no string concatenation builds SQL
- User input is validated in the Service layer before reaching the DAO

### 8.4 Copilot Agent Security

When Copilot works on this repository:
- It operates in a sandboxed Actions runner with no production database access
- All changes are reviewed on a `copilot/*` branch before merge
- `.gitignore` ensures secrets in `appsettings.Development.json` are never committed

---

## 9. Custom Agent Configuration (`.github/agents/`)

The project uses a rich ecosystem of custom VS Code agents. These are defined in
`.github/agents/` using the `*.agent.md` format and are invoked through GitHub Copilot Chat.

### 9.1 Core Agent File Structure

```yaml
---
description: "Brief description of purpose and when to use"
name: "Display Name"
tools: ["read", "edit", "search", "execute"]
model: "Claude Sonnet 4.5"
target: "vscode"
infer: true
---

# Agent Instructions
...
```

### 9.2 The SpecKit Workflow

The SpecKit agents implement a **spec-driven development workflow** with these phases:

```
User Request
    │
    ▼
speckit.specify     ← Write feature spec from natural language
    │
    ▼
speckit.clarify     ← Identify ambiguities, ask questions
    │
    ▼
speckit.plan        ← Generate implementation design
    │
    ▼
speckit.tasks       ← Create ordered task list
    │
    ▼
speckit.implement   ← Execute tasks one by one
    │
    ▼
speckit.analyze     ← Verify spec ↔ plan ↔ tasks consistency
```

### 9.3 The `noob-mode` Agent

For non-technical stakeholders, the `noob-mode` agent translates every action into plain
English with colour-coded risk levels (🟢 Low, 🟡 Medium, 🔴 High). Risky operations pause
for confirmation before proceeding.

---

## 10. Copilot Instructions and Governance

### 10.1 The Assumption Documentation Rule

Before proceeding with any major implementation decision, Copilot must create an
**assumption file** for human review:

- **Location:** `.github/assumptions/`
- **Naming:** `MMDDYYYY-HHMMam/pm-Assumptions.md`
- **When required:** Inferring missing requirements, choosing between implementation
  approaches, assuming a stored procedure exists, guessing at intended behavior, etc.

Assumption files written to date are in `.github/assumptions/`.

### 10.2 CopilotForms Metadata

The project uses a `CopilotForms` system (`docs/CopilotForms/`) that provides structured
form-based inputs to Copilot for common request types:

| Form Type | Purpose |
|---|---|
| `debugging` | Structured bug report for diagnosis |
| `new-feature-request` | Structured feature request |
| `ui-change` | UI-only change request |
| `logic-correction` | Fix incorrect business logic |
| `test-generation` | Generate targeted test coverage |
| `performance-issue-optimization` | Performance investigation and fix |
| `database-issue` | SQL / stored procedure / schema issue |
| `code-review` | Risk-focused code review request |
| `naming-consistency-cleanup` | Safe semantic rename/cleanup |
| `improvement-refactor` | Maintainability refactor |
| `logging-refactor` | Adjust logging strategy |
| `documentation-change` | Create or update documentation |
| `feature-removal-request` | Safely retire a feature |

Each form type has a corresponding `copilotforms-*.instructions.md` file that guides how
Copilot should interpret the structured input.

---

## 11. Recommendations for Effective Copilot Use

### 11.1 Always Let Copilot Read the Memories First

Before starting any session on this project, invoke:

```
"Read the architectural_patterns and forbidden_practices memories before making any changes"
```

This ensures the agent never violates the core architecture rules.

### 11.2 Use SpecKit for Non-Trivial Features

For any feature larger than a bug fix, use the `speckit.specify` → `speckit.plan` →
`speckit.tasks` → `speckit.implement` pipeline. This produces:

- A formal feature spec (`specs/<feature>/spec.md`)
- A detailed implementation plan with risks and rollback strategy
- An ordered task list that can be resumed if interrupted
- Automated consistency verification between spec, plan, and implementation

### 11.3 Use CopilotForms for Structured Requests

Rather than free-form prompts, use the CopilotForms exports for common request types.
Structured inputs yield more focused, compliant, less error-prone agent outputs.

### 11.4 Verify Architecture After Every Session

After each Copilot session, run these validation searches (via `speckit.analyze` or manually):

```
# Forbidden: ViewModel calling DAO directly
Pattern: "Dao_" in Module_*/ViewModels/**/*.cs → expect 0 results

# Forbidden: Static DAO class
Pattern: "static.*Dao_" in **/*.cs → expect 0 results

# Forbidden: Raw SQL in C# for MySQL
Pattern: "INSERT|UPDATE|DELETE" in Module_*/**/*.cs → expect 0 results

# Forbidden: Runtime binding in XAML
Pattern: "\{Binding " in **/*.xaml → expect 0 results

# Forbidden: Hardcoded connection strings
Pattern: "password=|Server=" in **/*.cs → expect 0 results
```

### 11.5 Commit to a Clean State Before Each Session

```bash
git status                            # Ensure no uncommitted changes
git add -A && git commit -m "checkpoint before Copilot session"
```

This makes reviewing Copilot's diff trivial and allows clean rollback if needed.

### 11.6 Keep Stored Procedures in Sync

If Copilot adds a new DAO method calling a stored procedure, verify the procedure exists in
`Database/StoredProcedures/` before merging the PR. The pattern is:

1. Write the stored procedure SQL first
2. Let Copilot implement the DAO method that calls it
3. Verify parameter names match exactly (case-sensitive in MySQL `CALL` statements)

---

## 12. References and Further Reading

### GitHub Copilot Coding Agent

| Resource | URL |
|---|---|
| Official Docs: Coding Agent | <https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent> |
| Tracking Sessions | <https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/track-copilot-sessions> |
| Custom Agents Guide | <https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents> |
| Coding Agent 101 (Blog) | <https://github.blog/ai-and-ml/github-copilot/github-copilot-coding-agent-101-getting-started-with-agentic-workflows-on-github/> |
| Coding Agent Examples | <https://devopsjournal.io/blog/2025/12/20/Copilot-Agent-example> |

### WinUI 3 and CommunityToolkit.Mvvm

| Resource | URL |
|---|---|
| WinUI 3 + MVVM Toolkit Tutorial | <https://learn.microsoft.com/en-us/windows/apps/tutorials/winui-mvvm-toolkit/intro> |
| CommunityToolkit.Mvvm Docs | <https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/> |
| MVVM Samples Repository | <https://github.com/CommunityToolkit/MVVM-Samples> |
| Windows Community Toolkit | <https://github.com/CommunityToolkit/Windows> |

### Serena MCP Server

| Resource | URL |
|---|---|
| Serena GitHub | <https://github.com/oraios/serena> |
| Serena Documentation | <https://oraios.github.io/serena/> |
| Tools Reference | <https://oraios.github.io/serena/01-about/035_tools.html> |

### Project-Specific References

| Document | Location |
|---|---|
| Architecture Constitution | `.github/copilot-instructions.md` |
| Agent Definitions | `AGENTS.md` |
| Serena Memory Catalog | `.serena/memories/` |
| SpecKit Configuration | `.specify/memory/constitution.md` |
| Infor Visual Schema Reference | `docs/InforVisual/DatabaseCSVFiles/` |
| CopilotForms Configuration | `docs/CopilotForms/data/copilot-forms.config.json` |
| Assumption Files | `.github/assumptions/` |
| Architecture Audit | `.github/audits/` |

---

*This document was created by the GitHub Copilot Coding Agent (task `af343bac-93aa-43fc-bf21-d7046019328f`)
and improved with deep research on 2026-04-05.*
