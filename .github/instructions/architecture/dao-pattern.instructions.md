---
description: 'DAO rules for MTM data access covering constructor injection, stored procedure use, read-only ERP access, and Model_Dao_Result handling.'
applyTo: 'Module_*/Data/**/*.cs'
---

<!-- 
[DOC-META-START]
- File Name: dao-pattern.instructions.md
- Description: DAO rules for MTM data access covering constructor injection, stored procedure use, read-only ERP access, and Model_Dao_Result handling.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # DAO Pattern
  - Line 10: ## Core Rules
  - Line 17: ## Database Rules
  - Line 24: ## Implementation Expectations
  - Line 31: ## Validation
- Critical Notes: DAOs are instance-based, return Model_Dao_Result, and never write raw MySQL SQL or write to Infor Visual.
[DOC-META-END]
-->

# DAO Pattern

Use this file for repository DAOs and data access helpers.

## Core Rules

- Keep DAOs instance based.
- Accept required connection material through constructor injection.
- Return `Model_Dao_Result` or `Model_Dao_Result<T>` for operational outcomes.
- Do not throw for expected database failures that should flow back to services.

## Database Rules

- MySQL writes must go through stored procedures.
- Do not add raw MySQL SQL strings in C# for write paths.
- Treat Infor Visual SQL Server access as read only.
- Use verified schema references before adding or changing Visual queries.

## Implementation Expectations

- Validate inputs early.
- Build parameters explicitly.
- Keep mapping logic local and readable.
- Keep cross-table business rules out of DAOs when services can own them.

## Validation

- Check for static DAO declarations.
- Check for raw SQL write statements in MySQL-facing C# code.