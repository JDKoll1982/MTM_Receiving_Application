# Module_Reporting — UI Conditional Guards

Last Updated: 2026-03-21

This note captures the confirmed command guards for the reporting entry screen.
The analysis also confirmed that `View_Reporting_Main.xaml` is the primary interactive screen and `View_Reporting_PreviewDialog.xaml` is the preview surface that appears after report generation.

---

## Main View (`ViewModel_Reporting_Main`) — Command Guards

| Command                  | Guard Method           | Enabled When                                      | Blocked When                                                                       |
| ------------------------ | ---------------------- | ------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `GenerateReportsCommand` | `CanGenerateReports()` | `!IsBusy && GetSelectedModules().Any()`           | No report module checkbox is checked, or operation in progress                     |
| `CopyEmailFormatCommand` | `CanCopyEmail()`       | `IncludedPreviewModuleCards.Count > 0 && !IsBusy` | No reports have been generated yet (preview cards empty), or operation in progress |

### Guard Details

**`CanGenerateReports`** — requires at least one module selection before the Generate button activates.
`GetSelectedModules().Any()` returns `true` only when one or more report module toggle is checked.
`NotifyCanExecuteChanged()` is fired whenever the module selection state changes.

**`CanCopyEmail`** — the Copy Email Format action requires a prior generation run to populate preview cards.
`IncludedPreviewModuleCards` is an `ObservableCollection` that is empty until `GenerateReportsCommand` completes successfully.
`NotifyCanExecuteChanged()` is fired on collection changes and after generation completes or fails.

**Source:** `ViewModel_Reporting_Main.cs`

---

## Screen Context

| View | Purpose |
|---|---|
| `View_Reporting_Main.xaml` | Main reporting selection and generation screen |
| `View_Reporting_PreviewDialog.xaml` | Post-generation preview surface used to inspect generated output |

These command guards are important because they define when the user can move from module selection to report generation and then from generated preview content to copy-ready email output.

---

## Universal Busy Guard

All commands in this module guard against concurrent execution using `IsBusy`.

| Pattern                                                  | Applied To         |
| -------------------------------------------------------- | ------------------ |
| `if (IsBusy) return;` at command entry                   | All async commands |
| `IsBusy = true` in `try` / `IsBusy = false` in `finally` | All async commands |
