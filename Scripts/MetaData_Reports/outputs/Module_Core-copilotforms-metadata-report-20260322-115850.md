# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:58:50
- Module: Module_Core
- Metadata status: MetadataPresent
- Source file count: 114
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Core

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 0
- Unreferenced services/contracts: 1

## Index Findings

- Index file list matches the metadata files on disk.

## Feature Findings

### Authentication And Session (core-auth-session.json)

- Missing service or contract files referenced by name:
  - Module_Core/Contracts/IService_Authentication.cs
  - Module_Core/Contracts/IService_SessionManager.cs
  - Module_Core/Contracts/IService_UserSessionManager.cs
  - Module_Core/Contracts/ITimer.cs
  - Module_Core/Contracts/Service_Authentication.cs
  - Module_Core/Contracts/Service_UserSessionManager.cs

### Converters And Theme Assets (core-converters-theme.json)

- No broken references detected.

### CQRS And Shared Models (core-cqrs-reporting-models.json)

- No broken references detected.

### Database And Infor Visual (core-database-inforvisual.json)

- Missing service or contract files referenced by name:
  - Module_Core/Contracts/IService_InforVisual.cs
  - Module_Core/Contracts/IService_VisualCredentialValidator.cs
  - Module_Core/Contracts/Service_InforVisualConnect.cs
  - Module_Core/Contracts/Service_VisualCredentialValidator.cs

### Error Help And Logging (core-error-help-logging.json)

- Missing service or contract files referenced by name:
  - Module_Core/Contracts/IService_ErrorHandler.cs
  - Module_Core/Contracts/IService_Help.cs
  - Module_Core/Contracts/IService_LoggingUtility.cs
  - Module_Core/Contracts/IService_Notification.cs
  - Module_Core/Contracts/Service_ErrorHandler.cs
  - Module_Core/Contracts/Service_Help.cs
  - Module_Core/Contracts/Service_LoggingUtility.cs

### Navigation And UI Services (core-navigation-ui.json)

- Missing service or contract files referenced by name:
  - Module_Core/Contracts/IService_Dispatcher.cs
  - Module_Core/Contracts/IService_Focus.cs
  - Module_Core/Contracts/IService_Navigation.cs
  - Module_Core/Contracts/IService_Pagination.cs
  - Module_Core/Contracts/IService_UIAutomation.cs
  - Module_Core/Contracts/IService_ViewModelRegistry.cs
  - Module_Core/Contracts/IWindowService.cs
  - Module_Core/Contracts/Service_Navigation.cs
  - Module_Core/Contracts/Service_UIAutomation.cs
  - Module_Core/Contracts/Service_ViewModelRegistry.cs

### Startup And System Config (core-startup-system-config.json)

- Missing service or contract files referenced by name:
  - Module_Core/Contracts/IService_OnStartup_AppLifecycle.cs
  - Module_Core/Contracts/Service_OnStartup_AppLifecycle.cs

## Live Source Files Not Referenced By Metadata

- Every scanned source file is referenced somewhere in the current metadata.

## Unreferenced Source Files By Area

- No unreferenced source-file groups to report.
## Live Services Or Contracts Not Referenced By Metadata

- Service_DispatcherTimerWrapper

## Unreferenced Services By Type

### Services (1)

- Service_DispatcherTimerWrapper

## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
