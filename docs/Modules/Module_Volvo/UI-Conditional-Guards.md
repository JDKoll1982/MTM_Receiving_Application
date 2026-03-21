# Module_Volvo — UI Conditional Guards

Last Updated: 2026-03-21

This note captures the confirmed guard logic for the Volvo history and settings screens.
The current analysis confirmed two main interactive views: shipment history and Volvo settings / part master management.

---

## History View (`ViewModel_Volvo_History`) — Command Guards

Both commands require a row to be selected in the shipment grid and no active background operation.

| Command             | Guard Method      | Enabled When                          | Blocked When                           |
| ------------------- | ----------------- | ------------------------------------- | -------------------------------------- |
| `ViewDetailCommand` | `CanViewDetail()` | `SelectedShipment != null && !IsBusy` | No selection, or operation in progress |
| `EditCommand`       | `CanEdit()`       | `SelectedShipment != null && !IsBusy` | No selection, or operation in progress |

`NotifyCanExecuteChanged()` is called for both commands from `OnSelectedShipmentChanged`.

**No status-based lock:** Volvo shipments carry no lock based on shipment status. Any historical shipment can be re-opened for edit regardless of its current state.

**Source:** `ViewModel_Volvo_History.cs`

---

## Settings / Part Master View (`ViewModel_Volvo_Settings`) — Command Guards

| Command           | Guard Method    | Enabled When                      | Blocked When                           |
| ----------------- | --------------- | --------------------------------- | -------------------------------------- |
| `EditPartCommand` | `CanEditPart()` | `SelectedPart != null && !IsBusy` | No selection, or operation in progress |

**Source:** `ViewModel_Volvo_Settings.cs`

---

## Screen Context

| View                       | Purpose                                           |
| -------------------------- | ------------------------------------------------- |
| `View_Volvo_History.xaml`  | Browse shipment history and open selected records |
| `View_Volvo_Settings.xaml` | Maintain Volvo settings and edit selected parts   |

The confirmed guards show that both screens depend on row selection before edit/detail actions become available.

---

## Universal Busy Guard

All commands in this module guard against concurrent execution using `IsBusy`.

| Pattern                                                  | Applied To         |
| -------------------------------------------------------- | ------------------ |
| `if (IsBusy) return;` at command entry                   | All async commands |
| `IsBusy = true` in `try` / `IsBusy = false` in `finally` | All async commands |
