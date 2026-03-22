# CopilotForms Module Metadata Report

- Generated: 2026-03-22T11:53:47
- Module: Module_Volvo
- Metadata status: MetadataPresent
- Source file count: 91
- Metadata folder: C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\CopilotForms\data\module-metadata\Module_Volvo

## Summary

- Unindexed metadata files: 0
- Missing metadata files from index: 0
- Unreferenced source files: 61
- Unreferenced services/contracts: 0

## Index Findings

- Index file list matches the metadata files on disk.

## Feature Findings

### Volvo Integration (volvo-integration.json)

- No broken references detected.

## Live Source Files Not Referenced By Metadata

- Module_Volvo/Data/IDao_VolvoLabelHistory.cs
- Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Commands/ClearLabelDataCommandHandler.cs
- Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Commands/DeletePendingShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs
- Module_Volvo/Handlers/Commands/RemovePartFromShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandler.cs
- Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetVolvoSettingQueryHandler.cs
- Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandler.cs
- Module_Volvo/Interfaces/IDao_VolvoPart.cs
- Module_Volvo/Interfaces/IDao_VolvoShipment.cs
- Module_Volvo/Interfaces/IDao_VolvoShipmentLine.cs
- Module_Volvo/Models/Model_EmailRecipient.cs
- Module_Volvo/Models/VolvoShipmentStatus.cs
- Module_Volvo/Requests/Commands/AddPartToShipmentCommand.cs
- Module_Volvo/Requests/Commands/AddVolvoPartCommand.cs
- Module_Volvo/Requests/Commands/ClearLabelDataCommand.cs
- Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs
- Module_Volvo/Requests/Commands/DeactivateVolvoPartCommand.cs
- Module_Volvo/Requests/Commands/DeletePendingShipmentCommand.cs
- Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs
- Module_Volvo/Requests/Commands/RemovePartFromShipmentCommand.cs
- Module_Volvo/Requests/Commands/SavePendingShipmentCommand.cs
- Module_Volvo/Requests/Commands/UpdateShipmentCommand.cs
- Module_Volvo/Requests/Commands/UpdateVolvoPartCommand.cs
- Module_Volvo/Requests/Queries/FormatEmailDataQuery.cs
- Module_Volvo/Requests/Queries/GenerateLabelCsvQuery.cs
- Module_Volvo/Requests/Queries/GetAllVolvoPartsQuery.cs
- Module_Volvo/Requests/Queries/GetPartComponentsQuery.cs
- Module_Volvo/Requests/Queries/GetRecentShipmentsQuery.cs
- Module_Volvo/Requests/Queries/GetShipmentHistoryQuery.cs
- Module_Volvo/Requests/Queries/GetVolvoSettingQuery.cs
- Module_Volvo/Requests/Queries/SearchVolvoPartsQuery.cs
- Module_Volvo/Requests/ShipmentLineDto.cs
- Module_Volvo/Validators/AddPartToShipmentCommandValidator.cs
- Module_Volvo/Validators/AddVolvoPartCommandValidator.cs
- Module_Volvo/Validators/CompleteShipmentCommandValidator.cs
- Module_Volvo/Validators/DeactivateVolvoPartCommandValidator.cs
- Module_Volvo/Validators/ImportPartsCsvCommandValidator.cs
- Module_Volvo/Validators/SavePendingShipmentCommandValidator.cs
- Module_Volvo/Validators/UpdateShipmentCommandValidator.cs
- Module_Volvo/Validators/UpdateVolvoPartCommandValidator.cs
- Module_Volvo/Views/View_Volvo_History.xaml.cs
- Module_Volvo/Views/View_Volvo_Settings.xaml.cs
- Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs
- Module_Volvo/Views/VolvoPartAddEditDialog.xaml.cs
- Module_Volvo/Views/VolvoShipmentEditDialog.xaml.cs

## Unreferenced Source Files By Area

### Data (1)

- Module_Volvo/Data/IDao_VolvoLabelHistory.cs

### Handlers (22)

- Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Commands/ClearLabelDataCommandHandler.cs
- Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Commands/DeletePendingShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs
- Module_Volvo/Handlers/Commands/RemovePartFromShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandler.cs
- Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandler.cs
- Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandler.cs
- Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandler.cs
- Module_Volvo/Handlers/Queries/GetVolvoSettingQueryHandler.cs
- Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandler.cs

### Interfaces (3)

- Module_Volvo/Interfaces/IDao_VolvoPart.cs
- Module_Volvo/Interfaces/IDao_VolvoShipment.cs
- Module_Volvo/Interfaces/IDao_VolvoShipmentLine.cs

### Models (2)

- Module_Volvo/Models/Model_EmailRecipient.cs
- Module_Volvo/Models/VolvoShipmentStatus.cs

### Requests (20)

- Module_Volvo/Requests/Commands/AddPartToShipmentCommand.cs
- Module_Volvo/Requests/Commands/AddVolvoPartCommand.cs
- Module_Volvo/Requests/Commands/ClearLabelDataCommand.cs
- Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs
- Module_Volvo/Requests/Commands/DeactivateVolvoPartCommand.cs
- Module_Volvo/Requests/Commands/DeletePendingShipmentCommand.cs
- Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs
- Module_Volvo/Requests/Commands/RemovePartFromShipmentCommand.cs
- Module_Volvo/Requests/Commands/SavePendingShipmentCommand.cs
- Module_Volvo/Requests/Commands/UpdateShipmentCommand.cs
- Module_Volvo/Requests/Commands/UpdateVolvoPartCommand.cs
- Module_Volvo/Requests/Queries/FormatEmailDataQuery.cs
- Module_Volvo/Requests/Queries/GenerateLabelCsvQuery.cs
- Module_Volvo/Requests/Queries/GetAllVolvoPartsQuery.cs
- Module_Volvo/Requests/Queries/GetPartComponentsQuery.cs
- Module_Volvo/Requests/Queries/GetRecentShipmentsQuery.cs
- Module_Volvo/Requests/Queries/GetShipmentHistoryQuery.cs
- Module_Volvo/Requests/Queries/GetVolvoSettingQuery.cs
- Module_Volvo/Requests/Queries/SearchVolvoPartsQuery.cs
- Module_Volvo/Requests/ShipmentLineDto.cs

### Validators (8)

- Module_Volvo/Validators/AddPartToShipmentCommandValidator.cs
- Module_Volvo/Validators/AddVolvoPartCommandValidator.cs
- Module_Volvo/Validators/CompleteShipmentCommandValidator.cs
- Module_Volvo/Validators/DeactivateVolvoPartCommandValidator.cs
- Module_Volvo/Validators/ImportPartsCsvCommandValidator.cs
- Module_Volvo/Validators/SavePendingShipmentCommandValidator.cs
- Module_Volvo/Validators/UpdateShipmentCommandValidator.cs
- Module_Volvo/Validators/UpdateVolvoPartCommandValidator.cs

### Views (5)

- Module_Volvo/Views/View_Volvo_History.xaml.cs
- Module_Volvo/Views/View_Volvo_Settings.xaml.cs
- Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs
- Module_Volvo/Views/VolvoPartAddEditDialog.xaml.cs
- Module_Volvo/Views/VolvoShipmentEditDialog.xaml.cs

## Live Services Or Contracts Not Referenced By Metadata

- Every scanned service or contract name appears in metadata.

## Unreferenced Services By Type

- No unreferenced service groups to report.
## Update Guidance

- Use this report to decide which feature JSON files or index entries are stale.
- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.
- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.
