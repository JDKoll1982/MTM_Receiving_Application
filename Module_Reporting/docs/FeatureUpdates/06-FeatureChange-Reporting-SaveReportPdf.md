# 06-FeatureChange-Reporting-SaveReportPdf

## Feature

The Reporting preview can save the current report as a PDF file, which replaces the earlier
draft that tried to send the report by email.

## Why Email Sending Was Removed

- The workstation runs **new Outlook for Windows**, which exposes no `Outlook.Application` COM
  server. COM automation only reaches classic Outlook.
- Real background sending (Microsoft Graph or SMTP) requires an Entra app registration
  (delegated `Mail.Send` / SMTP AUTH) or a relay credential. None is available, so no automated
  send can work.
- `mailto:` hand-off cannot carry the PDF attachment and degrades the HTML report, so it adds
  nothing over the existing **Copy Report Summary** action.

Decision: keep the PDF capability, remove all send code.

## Required Outcome

- A **Save PDF** button in the preview toolbar writes the report as a PDF with the included
  modules, summary table, detail rows, and a generated-by footer.
- The PDF goes into the folder configured in Settings > Reporting > **Report PDF Output**, which
  defaults to `\\mtmanu-fs01\expo drive\RECEIVING\EoD Reports`. No file dialog is shown.
- The output folder is created when it is missing, and after a successful save the folder opens
  with the new PDF selected.
- Suggested file name derives from the report title, e.g.
  `End of Day Report for 11-18-2026 - 11-24-2026.pdf`.
- No email, Outlook, or mail-credential code remains in the change set.

## Files

- `Module_Reporting/Contracts/IService_ReportingPdf.cs`
- `Module_Reporting/Contracts/IService_ReportingOutput.cs` (folder creation + reveal in Explorer)
- `Module_Reporting/Services/Service_ReportingOutput.cs`
- `Module_Reporting/Services/Service_ReportingPdf.cs` (QuestPDF, Community license)
- `Module_Reporting/Helpers/Helper_ReportingPdfFileName.cs`
- `Module_Reporting/Helpers/Helper_ReportingModuleAccent.cs` (shared module accent colors)
- `Module_Reporting/Settings/ReportingSettingsKeys.cs` (`Output.PdfSaveFolder` + its default)
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
  (`SaveReportPdfAsync`, `ReportPdfSuggestedFileName`, `ReportPdfSaveFailureAsync`)
- `Module_Reporting/Views/View_Reporting_PreviewPage.xaml` and `.xaml.cs` (Save PDF button)
- `Module_Settings.Reporting/Views/View_Settings_Reporting_PdfOutput.xaml` and its ViewModel
  (Report PDF Output settings page: folder, Use default, Open folder, Save)
- `Module_Settings.Core/Defaults/settings.manifest.json` (`Reporting.Output.PdfSaveFolder`)
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  (`IService_ReportingPdf`, `IService_ReportingOutput`, and the new settings page registrations)
- `MTM_Receiving_Application.csproj` (`QuestPDF`)

## Removed

- Outlook COM sender, email message/result models, recipient-address helper, STA automation
  helper, the Send Report Email dialog and its ViewModel, and all related tests and DI
  registrations.
- `Microsoft.Office.Interop.Outlook` package reference.

## Validation

- `dotnet build MTM_Receiving_Application.slnx`
- `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Module_Reporting"`
- Manual check: generate a preview, choose **Save PDF**, and confirm the PDF appears in the
  configured folder, opens with the summary table and detail rows, and that the containing folder
  is shown with the file selected.
- Manual check: change the folder in Settings > Reporting > **Report PDF Output**, save, and
  confirm the next **Save PDF** uses the new folder.

## Notes

- QuestPDF requires the Community license to apply to the organization (under $1M USD annual
  revenue); otherwise a paid license key and code change are required.
- Recipients are still shown on the preview page and can be copied for a manual paste into
  Outlook.
