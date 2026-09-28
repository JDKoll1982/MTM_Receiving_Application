---
description: 'Settings system guidance for MTM covering the CQRS-based settings core, facade usage, caching, encryption, and extension points.'
applyTo: 'Module_Settings.Core/**/*.cs,Module_Settings.*/**/*.{cs,xaml}'
---

<!-- 
[DOC-META-START]
- File Name: settings-management-system.instructions.md
- Description: Settings system guidance for MTM covering the CQRS-based settings core, facade usage, caching, encryption, and extension points.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # Settings Management System
  - Line 10: ## Rules
  - Line 17: ## Extension Guidance
- Critical Notes: Add new settings through the existing settings core flow — never bypass the facade or CQRS commands/queries.
[DOC-META-END]
-->

# Settings Management System

The repository has a centralized settings subsystem rather than ad hoc module-level setting logic.

## Rules

- Add new settings through the existing settings core flow instead of bypassing it.
- Prefer the facade and CQRS commands or queries over direct storage access.
- Keep validation in validators.
- Keep encryption, caching, and audit behavior inside the existing settings services.

## Extension Guidance

- Add new setting commands or queries in the established pattern.
- Add validators alongside command or query changes.
- Update the relevant settings UI only after the underlying contract is defined.