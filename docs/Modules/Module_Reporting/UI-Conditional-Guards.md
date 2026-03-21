# Module_Reporting — UI Conditional Guards

Last Updated: 2026-03-21

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

## Universal Busy Guard

All commands in this module guard against concurrent execution using `IsBusy`.

| Pattern                                                  | Applied To         |
| -------------------------------------------------------- | ------------------ |
| `if (IsBusy) return;` at command entry                   | All async commands |
| `IsBusy = true` in `try` / `IsBusy = false` in `finally` | All async commands |
