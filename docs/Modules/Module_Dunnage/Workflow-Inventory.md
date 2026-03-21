# Module_Dunnage — Workflow Inventory

Last Updated: 2026-03-21

This summary captures the Dunnage workflow information confirmed during the March 2026 module analysis.
At this time, the strongest confirmed details are the guarded user actions and the step progression points documented below.

---

## Confirmed Workflow Areas

| Area           | Confirmed Responsibility                                   |
| -------------- | ---------------------------------------------------------- |
| Type Selection | Browse or page through available types before choosing one |
| Part Selection | Highlight a part before selecting or editing it            |
| Quantity Entry | Enter a quantity greater than zero before moving forward   |

---

## User Actions Confirmed In Code

| Step           | User Action         | What Must Be True First       |
| -------------- | ------------------- | ----------------------------- |
| Type Selection | Go to next page     | Another page exists           |
| Type Selection | Go to previous page | A previous page exists        |
| Part Selection | Select part         | A part row is selected        |
| Part Selection | Edit part           | A part row is selected        |
| Quantity Entry | Move to next step   | Quantity is greater than zero |

---

## Validation And Recovery

| Step           | Blocking Condition                 | User Feedback                                     |
| -------------- | ---------------------------------- | ------------------------------------------------- |
| Quantity Entry | Quantity is blank or zero          | Inline message: `Quantity must be greater than 0` |
| Part Selection | No part selected                   | Action buttons remain disabled                    |
| Type Selection | No next or previous page available | Corresponding paging button remains disabled      |

---

## Busy-State Pattern

The module uses the standard `IsBusy` pattern on async commands:

- Commands return early when `IsBusy` is already `true`.
- Each async command sets `IsBusy = true` when it starts.
- Each async command resets `IsBusy = false` in `finally`.

This means the Dunnage workflow is protected against duplicate clicks during background work.

---

## Related Detail Document

See `UI-Conditional-Guards.md` in this folder for the command-level guard breakdown.
