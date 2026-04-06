# Dunnage Modals Refactoring

## Overview

Refactor all dunnage modals to align with Module_Receiving design patterns and architecture.

## Requirements

### 1. UI Design and Loading

- Update ALL dunnage modals to follow the same UI design patterns as Module_Receiving modals
- Implement the same loading mechanism as Module_Receiving modals

### 2. Dunnage Type Edit/Add Modal

- Add a toggle switch for selecting between icon or image picker
- Dynamically show/hide UI elements based on user selection (icon vs. image mode)

### 3. Modal Refactoring

- Convert ALL hardcoded modals (from parent XAML or C# code-behind) into independent components
- Create separate files for each complex modal:
  - `[ModalName].xaml` (UI definition)
  - `[ModalName].xaml.cs` (code-behind)
  - `[ModalName]ViewModel.cs` (view model)
  - `[ModalName]Model.cs` (data model)
- **Exclusion:** Simple confirmation modals do not require refactoring
