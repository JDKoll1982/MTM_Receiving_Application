# Module Receiving Permission Review

Last Updated: 2026-03-29

## Purpose

This review list captures the Module_Receiving features that are already sensitive enough to
deserve privilege-aware review. It is intended to support follow-up approval work before broader
role enforcement is expanded inside the module.

## Current State

- Module_Receiving does not currently contain a centralized role-enforcement layer.
- Session privilege data is now initialized during login and manual switch-user flows.
- Receiving Edit Mode now enforces ownership-based access for standard users: they can only
  view, change, and remove rows they created, while `Admin` and `Developer` retain unrestricted
  access.
- Broader Receiving actions still rely on workflow/state guards rather than explicit role checks.

## Features To Review As Permission-Gated Candidates

| Feature                                                      | Why It Is Sensitive                                                              | Primary Files                                                                                                                                                                                                    |
| ------------------------------------------------------------ | -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Enter `Edit Mode` from Mode Selection                        | Allows direct modification of previously entered receiving loads                 | `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`, `Module_Receiving/Services/Service_ReceivingWorkflow.cs`, `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-edit-mode.json` |
| Set `Edit Mode` as the user default receiving mode           | Can make an edit-focused workflow the default startup experience                 | `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`, `Module_Core/Models/Systems/Model_User.cs`                                                                                                   |
| Start Receiving directly in `Edit Mode` from a saved default | Bypasses mode-selection review if the user profile defaults to edit              | `Module_Receiving/Services/Service_ReceivingWorkflow.cs`                                                                                                                                                         |
| Remove selected rows in Receiving Edit Mode                  | Removes active loads from the current working set                                | `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`                                                                                                                                                    |
| Save edits made in Receiving Edit Mode                       | Persists edits and deletions from the edit workflow                              | `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`, `Module_Receiving/Services/Service_MySQL_Receiving.cs`                                                                                            |
| Clear current label data to history                          | Moves active label data into history and changes downstream edit source behavior | `Module_Receiving/Services/Service_MySQL_Receiving.cs`, `Module_Receiving/Services/Service_ReceivingLabelData.cs`                                                                                                |
| Delete current label data                                    | Removes active label rows from the current label-data store                      | `Module_Receiving/Services/Service_MySQL_Receiving.cs`                                                                                                                                                           |
| Delete receiving loads                                       | Removes persisted receiving data records                                         | `Module_Receiving/Services/Service_MySQL_Receiving.cs`, `Module_Receiving/Data/Dao_ReceivingLoad.cs`                                                                                                             |
| Delete package preferences                                   | Removes saved user packaging defaults                                            | `Module_Receiving/Services/Service_MySQL_PackagePreferences.cs`, `Module_Receiving/Data/Dao_PackageTypePreference.cs`                                                                                            |

## Existing Non-Privilege Guards

- `ViewModel_Receiving_Workflow` uses workflow-step visibility guards.
- `ViewModel_Receiving_EditMode` uses selection and busy-state guards.
- `Service_ReceivingWorkflow` validates step progression and draft state.

These are valuable, but they are not substitutes for role-based permission gates.

## Recommended Follow-Up Order

1. Gate entry into `Edit Mode` and prevent `edit` from being set as a default mode for unauthorized users.
2. Extend ownership or role-aware rules to any remaining Receiving destructive actions outside Edit Mode.
3. Decide whether `Clear current label data to history` should remain broader than Edit Mode ownership rules.
4. Keep CopilotForms metadata aligned with approved Receiving permission gates.
