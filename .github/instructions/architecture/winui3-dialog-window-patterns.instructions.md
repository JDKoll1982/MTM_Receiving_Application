---
description: 'WinUI 3 dialog and window guidance for MTM covering ContentDialog usage, XamlRoot requirements, modal flow, and result handling.'
applyTo: 'Module_*/Dialogs/**/*.{cs,xaml},Module_*/Views/**/*Dialog*.{cs,xaml},**/*.xaml.cs'
---

<!-- 
[DOC-META-START]
- File Name: winui3-dialog-window-patterns.instructions.md
- Description: WinUI 3 dialog and window guidance for MTM covering ContentDialog usage, XamlRoot requirements, modal flow, and result handling.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # WinUI 3 Dialog And Window Patterns
  - Line 8: ## Use Dialogs For
  - Line 14: ## Use Windows For
  - Line 20: ## Rules
- Critical Notes: Set XamlRoot correctly for ContentDialog; keep validation and persistence outside the dialog UI layer.
[DOC-META-END]
-->

# WinUI 3 Dialog And Window Patterns

## Use Dialogs For

- short focused edits
- confirmations
- lightweight modal data entry

## Use Windows For

- larger workflows
- multi-region editing surfaces
- scenarios that require persistent independent UI state

## Rules

- Set `XamlRoot` correctly for `ContentDialog` usage.
- Keep dialog code-behind view-specific.
- Return clear results or models instead of sharing mutable state across layers.
- Keep validation and persistence outside the dialog UI layer.
- Use shared window-sizing helpers where the repo already standardizes them.