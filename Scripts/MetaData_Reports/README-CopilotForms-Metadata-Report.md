# CopilotForms Metadata Report Script

Last Updated: 2026-03-22

Use [Scripts/New-CopilotFormsModuleMetadataReport.ps1](c:\Users\johnk\source\repos\MTM_Receiving_Application\Scripts\New-CopilotFormsModuleMetadataReport.ps1) to generate a report comparing a live module to its split CopilotForms metadata.

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
.\Scripts\New-CopilotFormsModuleMetadataReport.ps1 -ModuleName Module_Receiving
```

Default output location:

- `Scripts/outputs/<ModuleName>-copilotforms-metadata-report-<timestamp>.md`
- `Scripts/outputs/<ModuleName>-copilotforms-metadata-report-<timestamp>.json`

Use the generated Markdown report as the review artifact when updating module metadata manually.
