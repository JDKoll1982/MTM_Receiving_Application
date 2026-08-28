---
description: 'MVVM rules for MTM WinUI 3 views and viewmodels, including binding, layering, command, and dependency boundaries.'
applyTo: 'Module_*/ViewModels/**/*.cs,Module_*/Views/**/*.xaml,Module_Shared/**/*.{cs,xaml}'
---

<!-- 
[DOC-META-START]
- File Name: mvvm-pattern.instructions.md
- Description: MVVM rules for MTM WinUI 3 views and viewmodels, including binding, layering, command, and dependency boundaries.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # MVVM Pattern
  - Line 10: ## ViewModel Rules
  - Line 18: ## View Rules
  - Line 25: ## Forbidden Patterns
  - Line 32: ## Validation
- Critical Notes: Never call DAOs from ViewModels; use x:Bind only — no runtime {Binding} in new XAML.
[DOC-META-END]
-->

# MVVM Pattern

Use this file when changing WinUI views or ViewModels.

## ViewModel Rules

- Make ViewModels `partial` classes.
- Inherit from `ViewModel_Shared_Base` unless a narrower shared base is already established.
- Inject services, not DAOs, into ViewModels.
- Keep orchestration in the ViewModel and persistence in services and DAOs.
- Expose UI actions through commands instead of code-behind event logic.

## View Rules

- Use `x:Bind` with explicit binding mode.
- Keep code-behind limited to view-only concerns such as focus, visual state glue, and dialog host
  setup.
- Do not place business decisions, persistence, or validation orchestration in `.xaml.cs` files.

## Forbidden Patterns

- ViewModel direct DAO usage
- runtime `{Binding}` in new XAML
- service location from the view layer
- code-behind business logic

## Validation

- Check for ViewModel-to-DAO references before finishing.
- Check that new bindings use `x:Bind`.