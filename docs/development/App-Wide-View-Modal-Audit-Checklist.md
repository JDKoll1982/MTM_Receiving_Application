# App-Wide View / Page / Modal Audit Checklist

Last Updated: 2026-09-01

Audit scope: every view, page, and modal in the application. Each item is checked for:

1. **Help button** — does the surface expose the `?` help button (wired to `IService_Help`)?
2. **Searchable** — should it appear in the Main Window's top search bar (`CreateSearchDestinations`)?

Legend: ✅ present / covered · ❌ missing · ➖ not applicable (contextual surface, not a standalone navigable page)

---

## 1. Top-Level Application Pages (Frame routes — searchable)

| View | Route tag | Help button | Searchable |
| --- | --- | --- | --- |
| `View_Receiving_Workflow` | `ReceivingWorkflowView` | ✅ footer `?` | ✅ |
| `View_Dunnage_WorkflowView` | `DunnageLabelPage` | ✅ footer `?` | ✅ |
| `View_Volvo_ShipmentEntry` | `VolvoShipmentEntry` | ✅ | ✅ |
| `View_Volvo_History` | `VolvoHistory` | ✅ | ✅ |
| `View_Reporting_Main` | `ReportingMainPage` | ✅ | ✅ |
| `View_ShipRecTools_Main` | `ShipRecToolsPage` | ✅ | ✅ |
| `View_Scanner_Main` | `ScannerMainPage` | ✅ | ✅ |
| `View_Reprint_Main` | `ReprintLabelsPage` | ✅ | ✅ |
| App documentation | `AppDocumentation` | ➖ external | ✅ |

## 2. Workflow Sub-Views (hosted inside a workflow shell — covered by the shell footer `?`)

### Receiving (`View_Receiving_Workflow` host)
`ModeSelection`, `POEntry`, `LoadEntry`, `WeightQuantity`, `HeatLot`, `PackageType`, `Review`, `ManualEntry`, `EditMode`, `LocationReconciliationReview` — all ✅ (footer button + step help key), ➖ not separately searchable.

### Dunnage (`View_Dunnage_WorkflowView` host)
`ModeSelection`, `TypeSelection`, `PartSelection`, `QuantityEntry`, `DetailsEntry`, `Review`, `ManualEntry`, `EditMode` — all ✅ (footer button), ➖ not separately searchable.

## 3. Module Sub-Pages (hosted inside a module frame — not top-level search targets)

| View | Help button | Searchable |
| --- | --- | --- |
| `View_Scanner_Workbench` | ❌ | ➖ |
| `View_Scanner_History` | ❌ | ➖ |
| `View_Scanner_Settings` | ❌ | ➖ |
| `View_ShipRecTools_ToolSelection` | ❌ | ➖ |
| `View_Tool_DeliverySchedule` | ❌ | ➖ |
| `View_Tool_DunnageBook` | ❌ | ➖ |
| `View_Tool_MaterialAvailabilityBoard` | ❌ | ➖ |
| `View_Tool_OutsideServiceHistory` | ❌ | ➖ |
| `View_Tool_POLineSpecSearch` | ❌ | ➖ |
| `View_Tool_ReceivingAnalytics` | ❌ | ➖ |
| `View_Tool_WeldedCoils` | ❌ | ➖ |
| `View_Reporting_PreviewPage` | ❌ | ➖ |
| `View_Reprint_ModulePage` | ❌ | ➖ |

## 4. Dialogs / Modals

| Dialog | Module | Help button |
| --- | --- | --- |
| `View_Shared_NewUserSetupDialog` | Shared | ❌ |
| `View_Shared_SharedTerminalLoginDialog` | Shared | ❌ |
| `View_Shared_HelpDialog` | Shared | ➖ (is the help surface) |
| `View_Shared_IconSelectorWindow` | Shared | ❌ |
| `View_Shared_SplashScreenWindow` | Shared | ➖ (splash) |
| `View_Receiving_Dialog_NonPOEntry` | Receiving | ✅ |
| `Dialog_Receiving_EditModeColumnChooser` | Receiving | ✅ |
| `View_Dunnage_Dialog_ImagePartSearch` | Dunnage | ❌ |
| `View_Dunnage_Dialog_NonPOEntry` | Dunnage | ❌ |
| `View_Dunnage_Dialog_PartInfoModal` | Dunnage | ❌ |
| `View_Dunnage_Dialog_AddMultipleRowsDialog` | Dunnage | ❌ |
| `View_Dunnage_EditModeColumnChooserDialog` | Dunnage | ❌ |
| `View_Dunnage_SelectExistingSpecsDialog` | Dunnage | ❌ |
| `View_Dunnage_EditPartDialog` | Dunnage | ✅ |
| `View_Dunnage_QuickAddPartDialog` | Dunnage | ✅ |
| `View_Dunnage_QuickAddTypeDialog` | Dunnage | ✅ |
| `View_Volvo_EmailPreviewDialog` | Volvo | ❌ |
| `View_Volvo_GeneratedLabelDataDialog` | Volvo | ❌ |
| `View_Volvo_PartNumberEditDialog` | Volvo | ❌ |
| `View_Volvo_ShipmentHistoryDetailDialog` | Volvo | ❌ |
| `View_Volvo_ShipmentHistoryDetailWindow` | Volvo | ❌ |
| `VolvoShipmentEditDialog` | Volvo | ❌ |
| `View_Reporting_PreviewDialog` | Reporting | ❌ |
| `View_Scanner_ManageItemsDialog` | Scanner | ❌ |
| `Dialog_POLineSpecTextViewer` | ShipRec | ❌ |
| `Dialog_POLineSpecSearchOptions` | ShipRec | ❌ |
| `Dialog_MaterialAvailabilityWorkOrderDetails` | ShipRec | ❌ |
| `Dialog_MaterialAvailabilityIncomingDetails` | ShipRec | ❌ |
| `Dialog_DunnageBookPreview` | ShipRec | ❌ |
| `Dialog_Reprint_ColumnChooser` | Reprint | ❌ |
| `View_Settings_Volvo_PartAddEditDialog` | Settings.Volvo | ❌ |
| `Dialog_FuzzySearchPicker` | Core | ➖ (search picker) |

## 5. Settings Pages (searchable targets)

| Settings page | Help button | Searchable |
| --- | --- | --- |
| `View_Settings_CoreNavigationHub` | ❌ | ✅ |
| `View_Settings_Users` | ❌ | ✅ |
| `View_Settings_Theme` | ❌ | ✅ |
| `View_Settings_System` | ❌ | ✅ |
| `View_Settings_SharedPaths` | ❌ | ✅ |
| `View_Settings_LabelViewExecutable` | ❌ | ✅ |
| `View_Settings_MaterialAvailabilityBoardFields` | ❌ | ✅ |
| `View_Settings_Database` / `Logging` | ❌ | ➖ stub (no XAML) |
| `View_Settings_Receiving_CategoryHub` | ❌ | ✅ |
| `View_Settings_Receiving_EntryDefaults` | ❌ | ✅ |
| `View_Settings_Receiving_ValidationRules` | ❌ | ✅ |
| `View_Settings_Receiving_PartFormatting` | ❌ | ✅ |
| `View_Settings_Receiving_Reconciliation` | ❌ | ✅ |
| `View_Settings_Receiving_WorkflowDefaults` | ❌ | ✅ |
| `View_Settings_Receiving_KeyboardShortcuts` | ❌ | ✅ |
| `View_Settings_Receiving_LabelPaths` | ❌ | ✅ |
| `View_Settings_Dunnage_CategoryHub` | ❌ | ✅ |
| `View_Settings_Dunnage_PersonalDefaults` | ❌ | ✅ |
| `View_Settings_Dunnage_ImageAssets` | ❌ | ✅ |
| `View_Settings_Dunnage_ImagePresentation` | ❌ | ✅ |
| `View_Settings_Dunnage_WorkflowVisuals` | ❌ | ✅ |
| `View_Settings_Dunnage_KeyboardShortcuts` | ❌ | ✅ |
| `View_Settings_Dunnage_LabelPaths` | ❌ | ✅ |
| `View_Settings_Reporting_NavigationHub` | ❌ | ✅ |
| `View_Settings_Reporting_EmailRecipients` | ❌ | ✅ |
| `View_Settings_Volvo_NavigationHub` | ❌ | ✅ |
| `View_Settings_Volvo_PartCatalog` | ❌ | ✅ |
| `View_Settings_Volvo_EmailRecipients` | ❌ | ✅ |
| `View_Settings_Volvo_LabelPaths` | ❌ | ✅ |

## Fix Summary

### Searchability (MainWindow.xaml.cs `CreateSearchDestinations`) — ✅ DONE
- Added `View_Settings_Receiving_LabelPaths` ("Receiving Label Files")
- Added `View_Settings_Dunnage_LabelPaths` ("Dunnage Label Files")
- Added `View_Settings_Volvo_LabelPaths` ("Volvo Label Files")
- Added `View_Settings_System` ("System Settings")

### Help buttons — top-level pages — ✅ DONE
- Added `?` button + help content (`Service_Help.InitializeModuleHelp`) to:
  `View_Reporting_Main`, `View_Reprint_Main`, `View_ShipRecTools_Main`, `View_Volvo_ShipmentEntry`, `View_Volvo_History`, `View_Scanner_Main`.

### Help buttons — dialogs (REMAINING, flagged for follow-up)
- Add `?` button + content to every dialog flagged ❌ in section 4 (Dunnage, Volvo, Reporting, Scanner, ShipRec, Reprint, Shared, Settings.Volvo dialogs).

### Help buttons — settings pages (REMAINING, flagged for follow-up)
- Settings pages have no `?` button. Recommended approach: add one help button in the settings shell (`View_Settings_CoreWindow`) that shows help for the current page, rather than per-page buttons.

### Workflow sub-views / module sub-pages
- Covered by their shell footer buttons (Receiving/Dunnage). Scanner/ShipRec/Reporting/Reprint sub-pages: considered ➖ (hosted); optional follow-up to add per-sub-page help.
