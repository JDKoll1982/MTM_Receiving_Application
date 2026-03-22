# CopilotForms Metadata Report Script

Last Updated: 2026-03-22

Use [Scripts/MetaData_Reports/New-CopilotFormsModuleMetadataReport.ps1](c:\Users\johnk\source\repos\MTM_Receiving_Application\Scripts\MetaData_Reports\New-CopilotFormsModuleMetadataReport.ps1) to generate a report comparing a live module to its split CopilotForms metadata.

What it does:

- Scans the live module folders and files
- Reads `docs/CopilotForms/data/module-metadata/<ModuleName>/index.json`
- Reads each feature JSON file for that module
- Reports stale or suspicious metadata signals
- Writes a Markdown report and a JSON report

What it does not do:

- It does not edit metadata files
- It does not change `copilot-forms.config.json`
- It does not create new feature metadata automatically

Example:

```powershell
.\Scripts\MetaData_Reports\New-CopilotFormsModuleMetadataReport.ps1 -ModuleName Module_Receiving
```

Run all tracked repo paths:

```powershell
.\Scripts\MetaData_Reports\New-CopilotFormsModuleMetadataReport.ps1 -AllModules
```

Tracked paths used by `-AllModules`, and also valid to run individually with `-ModuleName`:

- `Infrastructure`
- `Module_Bulk_Inventory`
- `Module_Core`
- `Module_Dunnage`
- `Module_Receiving`
- `Module_Reporting`
- `Module_Settings.Core`
- `Module_Settings.DeveloperTools`
- `Module_Settings.Dunnage`
- `Module_Settings.Receiving`
- `Module_Settings.Volvo`
- `Module_Settings.Reporting`
- `Module_Shared`
- `Module_ShipRec_Tools`
- `Module_Volvo`
- `Database`
- `MTM_Receiving_Application.Tests`

If a tracked path does not yet have split CopilotForms metadata, the script writes a report with `Metadata status: MissingMetadataFolder` instead of failing.

Default output location:

- `Scripts/MetaData_Reports/outputs/<ModuleName>-copilotforms-metadata-report-<timestamp>.md`
- `Scripts/MetaData_Reports/outputs/<ModuleName>-copilotforms-metadata-report-<timestamp>.json`

When `-AllModules` is used, the script also writes:

- `Scripts/MetaData_Reports/outputs/AllModules-copilotforms-metadata-report-<timestamp>.md`
- `Scripts/MetaData_Reports/outputs/AllModules-copilotforms-metadata-report-<timestamp>.json`

Use the generated Markdown report as the review artifact when updating module metadata manually.
