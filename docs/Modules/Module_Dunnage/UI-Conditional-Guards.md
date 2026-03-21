# Module_Dunnage — UI Conditional Guards & Workflow Step Activation

Last Updated: 2026-03-21

---

## TypeSelection Step — Pagination Command Guards

Next/Previous buttons are enabled or disabled based on the pagination service state. First/Last buttons have no guard.

| Command | Guard Property | Enabled When |
|---|---|---|
| `GoNextPageCommand` | `CanGoNext` (bool) | `pagination.HasNextPage = true` |
| `GoPreviousPageCommand` | `CanGoPrevious` (bool) | `pagination.HasPreviousPage = true` |
| `FirstPageCommand` | — | Always enabled |
| `LastPageCommand` | — | Always enabled |

Guard properties are updated on every page change via `UpdatePaginationState()`, which calls:
- `CanGoNext = _paginationService.HasNextPage`
- `CanGoPrevious = _paginationService.HasPreviousPage`
- `NextPageCommand.NotifyCanExecuteChanged()`
- `PreviousPageCommand.NotifyCanExecuteChanged()`

**Source:** `ViewModel_Dunnage_TypeSelectionViewModel.cs` — pagination update method

---

## PartSelection Step — Command Guards

Both action commands share the same guard condition.

| Command | Guard Method | Enabled When |
|---|---|---|
| `SelectPartCommand` | `IsPartSelected` (expression property) | `SelectedPart != null` |
| `EditPartCommand` | `IsPartSelected` (expression property) | `SelectedPart != null` |

`IsPartSelected` is defined as: `public bool IsPartSelected => SelectedPart != null;`

`NotifyCanExecuteChanged()` is called for both commands from `OnSelectedPartChanged`.

**Source:** `ViewModel_Dunnage_PartSelectionViewModel.cs`

---

## QuantityEntry Step — Navigation Guard

The GoNext button is disabled until the quantity field holds a valid value.

| Command | Guard Method | Enabled When | Blocked When |
|---|---|---|---|
| `GoNextCommand` | `IsValid` | `Quantity > 0` | `Quantity <= 0` (zero or blank) |

`IsValid` is inline: `[RelayCommand(CanExecute = nameof(IsValid))]`

When blocked, `ValidationMessage` is populated with `"Quantity must be greater than 0"` and displayed in the view inline. `GoNextCommand.NotifyCanExecuteChanged()` is called from `OnQuantityChanged`.

**Source:** `ViewModel_Dunnage_QuantityEntryViewModel.cs`

---

## Universal Busy Guard

All commands in this module guard against concurrent execution using `IsBusy`.

| Pattern | Applied To |
|---|---|
| `if (IsBusy) return;` at command entry | All async commands |
| `IsBusy = true` in `try` / `IsBusy = false` in `finally` | All async commands |
