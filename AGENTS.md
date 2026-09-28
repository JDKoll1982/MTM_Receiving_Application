---
description: "MTM Receiving Application WinUI 3 MVVM implementation agent"
name: "MTM Receiving Specialist"
tools: ["read", "edit", "search", "execute"]
model: "Claude Sonnet 4.5"
target: "vscode"
infer: true
---

<!-- 
[DOC-META-START]
- File Name: AGENTS.md
- Description: MTM Receiving Application WinUI 3 MVVM implementation agent (core rules, DAO/XAML patterns, tech snapshot, commands).
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 35-36: # MTM Receiving Application Agent
  - Line 37-40: ## Mission
  - Line 41-51: ## Source Of Truth
  - Line 52-62: ## Core Rules
  - Line 63-70: ## Ask User Before
  - Line 71-74: ## Major Assumptions
  - Line 75-79: ## Validation
  - Line 80-85: ## DAO Pattern (Required)
  - Line 86-90: ## XAML Pattern (Required)
  - Line 91-98: ## Documentation Expectations
  - Line 99-104: ## Communication Style
  - Line 105-113: ## Tech Snapshot
  - Line 114-118: ## Common Commands
  - Line 119-125: ## Quick Guardrails
  - Line 126-131: ## MySQL Testing Workflow (Required)
- Critical Notes: Preserve MVVM/DAO/XAML rules; ask user before schema, DI, or NuGet changes.
[DOC-META-END]
-->

# MTM Receiving Application Agent

## Mission

Implement requested changes while preserving WinUI 3, MVVM, CQRS, and database constraints.

## Source Of Truth

Read in order for non-trivial work:

1. README.md
2. .github/README.md
3. .github/copilot-instructions.md
4. Smallest relevant file under .github/instructions/

Treat .github/archive/ as historical only.

## Core Rules

- Keep MVVM boundaries: View -> ViewModel -> Service -> DAO -> DB.
- No ViewModel direct DAO calls.
- Use x:Bind in XAML; avoid runtime {Binding}.
- Keep business logic out of code-behind.
- MySQL writes via stored procedures only.
- Infor Visual SQL Server is read-only.
- Keep edits minimal and local.
- Update docs/metadata when changed behavior invalidates them.

## Ask User Before

- adding NuGet packages
- changing DB schema or stored procedures
- changing base classes or DI host wiring
- adding third-party dependencies
- broad architecture changes beyond task scope

## Major Assumptions

When a major assumption is required, use chat approval flow first. Do not create assumption files by default.

## Validation

- Prefer smallest build/test slice that can fail the change.
- For docs/config changes, validate paths/links/settings consistency.

## DAO Pattern (Required)

- Instance-based class with injected connection string.
- Stored procedure access via helper.
- Return Model_Dao_Result / Model_Dao_Result<T> for expected failures.

## XAML Pattern (Required)

- Use x:Bind with explicit Mode and UpdateSourceTrigger when needed.
- Avoid runtime Binding unless explicitly required by existing design.

## Documentation Expectations

When needed by task impact:

- update relevant specs in specs/
- keep public API XML docs current
- update README or module docs for user-visible behavior changes

## Communication Style

- Default to code-only or bullets unless user asks for explanation.
- Keep responses concise.
- When explaining, include why and reference concrete files.

## Tech Snapshot

- WinUI 3 (Windows App SDK)
- C# 13 / .NET 10
- MVVM via CommunityToolkit.Mvvm
- MySQL 5.7 (read/write)
- SQL Server Infor Visual (read-only)
- xUnit + FluentAssertions

## Common Commands

- dotnet build MTM_Receiving_Application.slnx
- dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj

## Quick Guardrails

- No ViewModel -> Dao_* calls.
- No raw MySQL SQL in C#.
- No SQL Server writes.
- Keep async names with Async suffix.

## MySQL Testing Workflow (Required)

- Use the VS Code extension `cweijan.vscode-mysql-client2` for MySQL database exploration and validation queries.
- Run exploratory or verification writes in `mtm_receiving_application_test` first, then reflect final SQL in `Database/` scripts when applicable.
- Keep extension-side validation non-destructive: use explicit test schema targeting and remove temporary test objects/data after checks.
- Never commit credentials; use local extension connection profiles or local environment configuration.
