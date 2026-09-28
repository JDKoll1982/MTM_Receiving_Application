---
description: 'WinUI 3 converter guidance for MTM covering naming, null handling, resource registration, and converter scope boundaries.'
applyTo: 'Module_Core/Converters/**/*.cs,Module_*/Views/**/*.xaml'
---

<!-- 
[DOC-META-START]
- File Name: winui3-xaml-converters.instructions.md
- Description: WinUI 3 converter guidance for MTM covering naming, null handling, resource registration, and converter scope boundaries.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # WinUI 3 XAML Converters
  - Line 10: ## Rules
  - Line 18: ## Preferred Uses
  - Line 24: ## Avoid
- Critical Notes: Keep converters single-purpose and free of business logic and service/data-access injection.
[DOC-META-END]
-->

# WinUI 3 XAML Converters

Use this file when adding or changing value converters.

## Rules

- Keep converters single-purpose.
- Name converters with the `Converter_` prefix used in the repository.
- Handle null and unexpected input safely.
- Do not inject services or data access into converters.
- Register converters at the narrowest reasonable XAML resource scope.

## Preferred Uses

- simple value-to-UI transforms
- visibility toggles
- enum-to-display or enum-to-visibility mapping

## Avoid

- business logic in converters
- multi-step workflow decisions in converter code
- converter code that depends on mutable external state