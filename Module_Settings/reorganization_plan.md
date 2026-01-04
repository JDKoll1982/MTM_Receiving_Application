# Plan: Reorganize Settings Module

This plan outlines the steps to move Settings-related files to the new `Module_Settings` structure, following the pattern established by the Receiving module.

## 1. Directory Structure (Created)
- `Module_Settings/Models`
- `Module_Settings/ViewModels`
- `Module_Settings/Views`
- `Module_Settings/Services`
- `Module_Settings/Enums`
- `Module_Settings/Interfaces`

## 2. File Moves and Renames

### ViewModels
| Source | Destination | New Class Name |
|--------|-------------|----------------|
| `ViewModels/Settings/Settings_DunnageModeViewModel.cs` | `Module_Settings/ViewModels/ViewModel_Settings_DunnageMode.cs` | `ViewModel_Settings_DunnageMode` |
| `ViewModels/Settings/Settings_ModeSelectionViewModel.cs` | `Module_Settings/ViewModels/ViewModel_Settings_ModeSelection.cs` | `ViewModel_Settings_ModeSelection` |
| `ViewModels/Settings/Settings_PlaceholderViewModel.cs` | `Module_Settings/ViewModels/ViewModel_Settings_Placeholder.cs` | `ViewModel_Settings_Placeholder` |
| `ViewModels/Settings/Settings_WorkflowViewModel.cs` | `Module_Settings/ViewModels/ViewModel_Settings_Workflow.cs` | `ViewModel_Settings_Workflow` |

### Views
| Source | Destination | New Class Name |
|--------|-------------|----------------|
| `Views/Settings/Settings_DunnageModeView.xaml` | `Module_Settings/Views/View_Settings_DunnageMode.xaml` | `View_Settings_DunnageMode` |
| `Views/Settings/Settings_DunnageModeView.xaml.cs` | `Module_Settings/Views/View_Settings_DunnageMode.xaml.cs` | `View_Settings_DunnageMode` |
| `Views/Settings/Settings_ModeSelectionView.xaml` | `Module_Settings/Views/View_Settings_ModeSelection.xaml` | `View_Settings_ModeSelection` |
| `Views/Settings/Settings_ModeSelectionView.xaml.cs` | `Module_Settings/Views/View_Settings_ModeSelection.xaml.cs` | `View_Settings_ModeSelection` |
| `Views/Settings/Settings_PlaceholderView.xaml` | `Module_Settings/Views/View_Settings_Placeholder.xaml` | `View_Settings_Placeholder` |
| `Views/Settings/Settings_PlaceholderView.xaml.cs` | `Module_Settings/Views/View_Settings_Placeholder.xaml.cs` | `View_Settings_Placeholder` |
| `Views/Settings/Settings_WorkflowView.xaml` | `Module_Settings/Views/View_Settings_Workflow.xaml` | `View_Settings_Workflow` |
| `Views/Settings/Settings_WorkflowView.xaml.cs` | `Module_Settings/Views/View_Settings_Workflow.xaml.cs` | `View_Settings_Workflow` |

### Services
| Source | Destination | Namespace |
|--------|-------------|-----------|
| `Services/Service_SettingsWorkflow.cs` | `Module_Settings/Services/Service_SettingsWorkflow.cs` | `MTM_Receiving_Application.Module_Settings.Services` |
| `Services/Database/Service_UserPreferences.cs` | `Module_Settings/Services/Service_UserPreferences.cs` | `MTM_Receiving_Application.Module_Settings.Services` |

### Interfaces
| Source | Destination | Namespace |
|--------|-------------|-----------|
| `Contracts/Services/IService_SettingsWorkflow.cs` | `Module_Settings/Interfaces/IService_SettingsWorkflow.cs` | `MTM_Receiving_Application.Module_Settings.Interfaces` |
| `Contracts/Services/IService_UserPreferences.cs` | `Module_Settings/Interfaces/IService_UserPreferences.cs` | `MTM_Receiving_Application.Module_Settings.Interfaces` |

### Enums
| Source | Destination | Namespace |
|--------|-------------|-----------|
| `Models/Enums/Enum_SettingsWorkflowStep.cs` | `Module_Settings/Enums/Enum_SettingsWorkflowStep.cs` | `MTM_Receiving_Application.Module_Settings.Enums` |

## 3. Execution Steps

1.  **Move and Rename Files**: Execute file moves and renames.
2.  **Update Code Content**:
    *   Update namespaces to `MTM_Receiving_Application.Module_Settings.*`.
    *   Update class names in ViewModels and Views (partial classes, constructors).
    *   Update XAML `x:Class` and `xmlns:viewmodels`.
    *   Update `Enum_SettingsWorkflowStep` namespace.
3.  **Update References**:
    *   Update `App.xaml.cs` DI registration.
    *   Update usages of moved classes/interfaces in the entire solution.
4.  **Cleanup**: Remove empty source directories.
5.  **Verify**: Build and fix errors.
