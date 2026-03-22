# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:33:32
- Module: Module_Reporting
- Metadata status: MetadataPresent
- Source file count: 16
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Reporting

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 16
- Unreferenced services/contracts: 3

## Index Findings

- Index file list matches the metadata files on disk.

## Feature Findings

### Reporting Dashboard (reporting-dashboard.json)

- Missing service or contract files referenced by name:
  - Module_Reporting/Contracts/IService_ErrorHandler.cs

## Live Source Files Not Referenced By Metadata

- Module_Reporting/Contracts/IService_Reporting.cs
- Module_Reporting/Contracts/IService_ReportingClipboard.cs
- Module_Reporting/Data/Dao_Reporting.cs
- Module_Reporting/Models/Model_ReportingPreviewCell.cs
- Module_Reporting/Models/Model_ReportingPreviewColumnOption.cs
- Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs
- Module_Reporting/Models/Model_ReportingPreviewRow.cs
- Module_Reporting/README.md
- Module_Reporting/Services/Service_Reporting.cs
- Module_Reporting/Services/Service_ReportingClipboard.cs
- Module_Reporting/SETTABLE_OBJECTS_REPORT.md
- Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs
- Module_Reporting/Views/View_Reporting_Main.xaml
- Module_Reporting/Views/View_Reporting_Main.xaml.cs
- Module_Reporting/Views/View_Reporting_PreviewDialog.xaml
- Module_Reporting/Views/View_Reporting_PreviewDialog.xaml.cs

## Unreferenced Source Files By Area

### Contracts (2)

- Module_Reporting/Contracts/IService_Reporting.cs
- Module_Reporting/Contracts/IService_ReportingClipboard.cs

### Data (1)

- Module_Reporting/Data/Dao_Reporting.cs

### Models (4)

- Module_Reporting/Models/Model_ReportingPreviewCell.cs
- Module_Reporting/Models/Model_ReportingPreviewColumnOption.cs
- Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs
- Module_Reporting/Models/Model_ReportingPreviewRow.cs

### README.md (1)

- Module_Reporting/README.md

### Services (2)

- Module_Reporting/Services/Service_Reporting.cs
- Module_Reporting/Services/Service_ReportingClipboard.cs

### SETTABLE_OBJECTS_REPORT.md (1)

- Module_Reporting/SETTABLE_OBJECTS_REPORT.md

### ViewModels (1)

- Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs

### Views (4)

- Module_Reporting/Views/View_Reporting_Main.xaml
- Module_Reporting/Views/View_Reporting_Main.xaml.cs
- Module_Reporting/Views/View_Reporting_PreviewDialog.xaml
- Module_Reporting/Views/View_Reporting_PreviewDialog.xaml.cs

## Live Services Or Contracts Not Referenced By Metadata

- IService_ReportingClipboard
- Service_Reporting
- Service_ReportingClipboard

## Unreferenced Services By Type

### Contracts (1)

- IService_ReportingClipboard

### Services (2)

- Service_Reporting
- Service_ReportingClipboard

## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
