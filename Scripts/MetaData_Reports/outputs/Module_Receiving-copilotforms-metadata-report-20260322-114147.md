# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:41:47
- Module: Module_Receiving
- Metadata status: MetadataPresent
- Source file count: 72
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Receiving

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 0
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

### Supporting Files (receiving-supporting-files.json)

- No broken references detected.

### Receiving Workflow (receiving-workflow.json)

- Missing service or contract files referenced by name:
  - Module_Receiving/Contracts/Service_MySQL_Receiving.cs
  - Module_Receiving/Contracts/Service_ReceivingLabelData.cs
  - Module_Receiving/Contracts/Service_ReceivingSettings.cs
  - Module_Receiving/Contracts/Service_ReceivingValidation.cs
  - Module_Receiving/Contracts/Service_ReceivingWorkflow.cs
  - Module_Receiving/Contracts/Service_SessionManager.cs

## Live Source Files Not Referenced By Metadata

- Every scanned source file is referenced somewhere in the current metadata.

## Unreferenced Source Files By Area

- No unreferenced source-file groups to report.
## Live Services Or Contracts Not Referenced By Metadata

- Every scanned service or contract name appears in metadata.

## Unreferenced Services By Type

- No unreferenced service groups to report.
## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
