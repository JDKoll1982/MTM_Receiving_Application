# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:02:39
- Module: Module_Receiving
- Source file count: 72
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Receiving

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 33
- Unreferenced services/contracts: 12

## Index Findings

- Index file list matches the metadata files on disk.

## Feature Findings

### Edit Mode (receiving-edit-mode.json)

- No broken references detected.

### Label Data Export (receiving-label-export.json)

- No broken references detected.

### Package Preferences (receiving-package-preferences.json)

- No broken references detected.

### Quality Hold (receiving-quality-hold.json)

- No broken references detected.

### Receiving Workflow (receiving-workflow.json)

- No broken references detected.

## Live Source Files Not Referenced By Metadata

- Module_Receiving/Contracts/IService_MySQL_QualityHold.cs
- Module_Receiving/Contracts/IService_MySQL_Receiving.cs
- Module_Receiving/Contracts/IService_MySQL_ReceivingLine.cs
- Module_Receiving/Contracts/IService_ReceivingSettings.cs
- Module_Receiving/Contracts/IService_ReceivingValidation.cs
- Module_Receiving/Contracts/IService_ReceivingWorkflow.cs
- Module_Receiving/Data/Dao_ReceivingLoad.cs
- Module_Receiving/Models/Model_Application_Variables.cs
- Module_Receiving/Models/Model_EditModeColumn.cs
- Module_Receiving/Models/Model_ReceivingLine.cs
- Module_Receiving/Models/Model_ReceivingLoad.cs
- Module_Receiving/Models/Model_ReceivingSession.cs
- Module_Receiving/Models/Model_ReceivingValidationResult.cs
- Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs
- Module_Receiving/Models/Model_SaveResult.cs
- Module_Receiving/Models/Model_UserPreference.cs
- Module_Receiving/Models/Model_WorkflowStepResult.cs
- Module_Receiving/Services/Service_MySQL_Receiving.cs
- Module_Receiving/Services/Service_ReceivingSettings.cs
- Module_Receiving/Services/Service_ReceivingValidation.cs
- Module_Receiving/Services/Service_SessionManager.cs
- Module_Receiving/Settings/ReceivingSettingsDefaults.cs
- Module_Receiving/Settings/ReceivingSettingsKeys.cs
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

- IService_ReceivingSettings
- IService_ReceivingValidation
- Service_MySQL_PackagePreferences
- Service_MySQL_QualityHold
- Service_MySQL_Receiving
- Service_MySQL_ReceivingLine
- Service_QualityHoldWarning
- Service_ReceivingLabelData
- Service_ReceivingSettings
- Service_ReceivingValidation
- Service_ReceivingWorkflow
- Service_SessionManager

## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
