# Module_Reporting — Workflow Inventory

Last Updated: 2026-03-21

This summary captures the reporting workflow information confirmed during the March 2026 analysis.

---

## Confirmed Screens

| View                                | Role                                                                      |
| ----------------------------------- | ------------------------------------------------------------------------- |
| `View_Reporting_Main.xaml`          | Main reporting screen where the user selects modules and generates output |
| `View_Reporting_PreviewDialog.xaml` | Preview surface shown after generation                                    |

---

## Confirmed User Actions

| Action            | Enabled When                                                      |
| ----------------- | ----------------------------------------------------------------- |
| Generate reports  | At least one report module is selected and the screen is not busy |
| Copy email format | Preview cards exist and the screen is not busy                    |

---

## Main Flow

1. The user selects one or more reporting modules.
2. The Generate action becomes available.
3. Successful generation populates preview cards.
4. The Copy Email Format action becomes available only after that preview content exists.

---

## Recovery And Blocking Conditions

| Condition                  | Result                                  |
| -------------------------- | --------------------------------------- |
| No module selected         | Generate button stays disabled          |
| Generation still running   | Primary actions are blocked by `IsBusy` |
| No preview cards exist yet | Copy Email Format stays disabled        |

---

## Related Detail Document

See `UI-Conditional-Guards.md` in this folder for the command-level guard breakdown.
