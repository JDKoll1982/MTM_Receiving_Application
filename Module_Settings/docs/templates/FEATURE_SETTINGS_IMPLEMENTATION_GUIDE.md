# Feature Settings Implementation Guide

This guide defines how feature modules integrate with Core Settings.

## Goals

- Feature modules own their settings UI and documentation.
- Core Settings provides storage, validation, caching, and auditing.
- No feature module should access DAOs directly.

## Required Structure

```bash
Module_Settings.{FeatureName}/
  Data/
  Enums/
  Interfaces/
  Models/
  Services/
  ViewModels/
  Views/
  docs/
    templates/
```

## Required Steps

1. Define settings metadata in code (register with the Core registry).
2. Add a settings inventory doc using the Settable Objects Inventory Template.
3. Build UI in the feature module (use x:Bind, no code-behind logic).
4. Access settings through IService_SettingsCoreFacade only.

## Registration Example

- Register feature settings definitions at module startup.
- Include category, key, data type, default value, scope, and permission level.

## Anti-Patterns

- ViewModels calling DAOs directly.
- Raw SQL in C# (use stored procedures only).
- Sharing MainWindow navigation context with the Settings window.
