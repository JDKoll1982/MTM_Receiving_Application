# Module_Volvo — Workflow Inventory

Last Updated: 2026-03-21

This summary captures the Volvo workflow information confirmed during the March 2026 analysis.

---

## Confirmed Screens

| View                       | Role                                                 |
| -------------------------- | ---------------------------------------------------- |
| `View_Volvo_History.xaml`  | Shipment history screen with detail and edit actions |
| `View_Volvo_Settings.xaml` | Settings / part master screen with part editing      |

---

## Confirmed User Actions

| Screen   | User Action   | Enabled When                                      |
| -------- | ------------- | ------------------------------------------------- |
| History  | View detail   | A shipment is selected and the screen is not busy |
| History  | Edit shipment | A shipment is selected and the screen is not busy |
| Settings | Edit part     | A part is selected and the screen is not busy     |

---

## Flow Implications

- History actions are selection-driven.
- No shipment-status lock was confirmed in the history screen analysis.
- Settings editing is also selection-driven, not freeform.
- Busy-state guards prevent duplicate actions while background work is active.

---

## Related Detail Document

See `UI-Conditional-Guards.md` in this folder for the command-level guard breakdown.
