# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:39:49
- Module: Module_Receiving
- Metadata status: MetadataPresent
- Source file count: 72
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Receiving

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 19
- Unreferenced services/contracts: 0

## Index Findings

- Index file list matches the metadata files on disk.

## Feature Findings

### Edit Mode (receiving-edit-mode.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_MySQL_Receiving.cs
  - Module_Receiving/Contracts/Service_MySQL_ReceivingLine.cs
  - Module_Receiving/Contracts/Service_ReceivingLabelData.cs
  - Module_Receiving/Contracts/Service_ReceivingSettings.cs
  - Module_Receiving/Contracts/Service_ReceivingValidation.cs
  - Module_Receiving/Contracts/Service_ReceivingWorkflow.cs

### Label Data Export (receiving-label-export.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_ReceivingLabelData.cs

### Package Preferences (receiving-package-preferences.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_MySQL_PackagePreferences.cs

### Quality Hold (receiving-quality-hold.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_MySQL_QualityHold.cs
  - Module_Receiving/Contracts/Service_QualityHoldWarning.cs

### Receiving Workflow (receiving-workflow.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_MySQL_Receiving.cs
  - Module_Receiving/Contracts/Service_ReceivingLabelData.cs
  - Module_Receiving/Contracts/Service_ReceivingSettings.cs
  - Module_Receiving/Contracts/Service_ReceivingValidation.cs
  - Module_Receiving/Contracts/Service_ReceivingWorkflow.cs
  - Module_Receiving/Contracts/Service_SessionManager.cs

## Live Source Files Not Referenced By Metadata

- Module_Receiving/Contracts/IService_MySQL_QualityHold.cs
- Module_Receiving/Contracts/IService_MySQL_Receiving.cs
- Module_Receiving/Contracts/IService_MySQL_ReceivingLine.cs
- Module_Receiving/Models/Model_Application_Variables.cs
- Module_Receiving/Models/Model_ReceivingLine.cs
- Module_Receiving/Models/Model_ReceivingValidationResult.cs
- Module_Receiving/Models/Model_UserPreference.cs
- Module_Receiving/Models/Model_WorkflowStepResult.cs
- Module_Receiving/Settings/ReceivingSettingsDefaults.cs
- Module_Receiving/Views/View_Receiving_EditMode.xaml.cs
- Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs
- Module_Receiving/Views/View_Receiving_PackageType.xaml.cs
- Module_Receiving/Views/View_Receiving_POEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_Review.xaml.cs
- Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml.cs

## Unreferenced Source Files By Area

### Contracts (3)

- Module_Receiving/Contracts/IService_MySQL_QualityHold.cs
- Module_Receiving/Contracts/IService_MySQL_Receiving.cs
- Module_Receiving/Contracts/IService_MySQL_ReceivingLine.cs

### Models (5)

- Module_Receiving/Models/Model_Application_Variables.cs
- Module_Receiving/Models/Model_ReceivingLine.cs
- Module_Receiving/Models/Model_ReceivingValidationResult.cs
- Module_Receiving/Models/Model_UserPreference.cs
- Module_Receiving/Models/Model_WorkflowStepResult.cs

### Settings (1)

- Module_Receiving/Settings/ReceivingSettingsDefaults.cs

### Views (10)

- Module_Receiving/Views/View_Receiving_EditMode.xaml.cs
- Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs
- Module_Receiving/Views/View_Receiving_PackageType.xaml.cs
- Module_Receiving/Views/View_Receiving_POEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_Review.xaml.cs
- Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml.cs

## Live Services Or Contracts Not Referenced By Metadata

- Every scanned service or contract name appears in metadata.

## Unreferenced Services By Type

- No unreferenced service groups to report.
## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
