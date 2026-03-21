# Module_Dunnage — UI Conditional Guards & Workflow Step Activation

Last Updated: 2026-03-21

This note captures the command-level UI guards confirmed from the Dunnage module analysis.
The current evidence is strongest for three workflow steps: Type Selection, Part Selection,
and Quantity Entry.

---

## Known Step Sequence

The analyzed Dunnage flow exposes these guarded steps:

1. Type Selection
2. Part Selection
3. Quantity Entry

The command guards below determine whether the user can move forward or edit data within those steps.

---

## TypeSelection Step — Pagination Command Guards

Next and Previous buttons are enabled or disabled based on the pagination service state.
First and Last buttons do not currently expose matching guard logic.

| Command | Guard Property | Enabled When |
|---|---|---|
| `GoNextPageCommand` | `CanGoNext` | `_paginationService.HasNextPage = true` |
| `GoPreviousPageCommand` | `CanGoPrevious` | `_paginationService.HasPreviousPage = true` |
| `FirstPageCommand` | — | Always enabled |
| `LastPageCommand` | — | Always enabled |

Guard state is refreshed by `UpdatePaginationState()`, which updates the two boolean guard properties and then calls `NotifyCanExecuteChanged()` on the guarded commands.

**Source:** `ViewModel_Dunnage_TypeSelectionViewModel.cs`

---

## PartSelection Step — Command Guards

Both part actions depend on whether the user has actually selected a part.

| Command | Guard Method / Property | Enabled When |
|---|---|---|
| `SelectPartCommand` | `IsPartSelected` | `SelectedPart != null` |
| `EditPartCommand` | `IsPartSelected` | `SelectedPart != null` |

`IsPartSelected` is an expression property that resolves to `SelectedPart != null`.
When the selection changes, both commands are refreshed via `NotifyCanExecuteChanged()`.

**Source:** `ViewModel_Dunnage_PartSelectionViewModel.cs`

---

## QuantityEntry Step — Navigation Guard

The user cannot advance until the quantity is greater than zero.

| Command | Guard Method | Enabled When | Blocked When |
|---|---|---|---|
| `GoNextCommand` | `IsValid` | `Quantity > 0` | `Quantity <= 0` |

When blocked, the screen sets `ValidationMessage` to `"Quantity must be greater than 0"` and shows that feedback inline.
The button guard is re-evaluated on each quantity change.

**Source:** `ViewModel_Dunnage_QuantityEntryViewModel.cs`

---

## Universal Busy Guard

Async commands in the module follow the same concurrency guard pattern.

| Pattern | Applied To |
|---|---|
| `if (IsBusy) return;` at command entry | Async commands |
| `IsBusy = true` in `try` and `IsBusy = false` in `finally` | Async commands |

This prevents overlapping operations and keeps the UI from starting the same work twice.
