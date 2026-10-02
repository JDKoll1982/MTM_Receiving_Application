# Batch Skid Editing and Label Reprints

Last Updated: 2026-10-02

This is a preview of a proposed change. None of it is in the app yet, so do not plan today's work
around it. It is written for receiving operators, warehouse staff, and anyone who retags skids, so
that the intended behaviour can be reviewed before it is built.

## Why This Is Being Proposed

Today the receiving app can only present and change one skid at a time. If nine skids need the same
part number corrected, someone has to open nine rows, type the new value nine times, and wait for the
lookup to check it each time. The app can select several rows, but only two actions use that
selection: removing rows and saving. There is no way to change a value across a group.

Here is the request that prompted this work:

- Purchasing asked for 154,540 lb of `MMC000659` to be moved to `MMC000767`.
- The receipts behind it had been booked under the wrong material, across four deliveries on two days.
- The warehouse locations report for `MMC000659` listed seven locations.
- The instruction was to change the labels on three of those locations, in person, by hand.
- The person doing the work wrote on the sheet: *"There is supposed to be 154,540 lbs. I can only
  find 136,500 lbs. Feel free to check my math."*

That last line is the problem. The operator was changing permanent material identities while holding
an unresolved arithmetic problem in their head, with nothing in the app to help them check it.

## What Changes For You

| Today | After this change |
| --- | --- |
| Change one skid at a time | Select many skids and change them in one action |
| No running total for a selection | A bar shows how many skids you picked and what they weigh |
| Nothing compares your selection to the paperwork | You enter the expected weight and the app tells you if it balances |
| Errors appear when you save | Errors appear while you are still deciding |
| Reprinting means searching again the next day | The batch you just changed is waiting for you, already filtered |
| No record of what was changed | A printable report of the whole batch |

## Operator Flow

```text
Open Edit Mode
   |
   +-- Load Current Labels or History
   +-- Filter or search for the skids you need
   +-- Tick the skids you want to change
   +-- The selection bar shows the count and the total weight
   |
   +-- Have the paperwork handy?
   |      yes -> type the expected total and read the difference
   |      no  -> carry on
   |
   +-- Open Batch Edit
   +-- Tick the fields to override and type the new values
   +-- Preview lists every before and after value
   |
   +-- Any problems found?
   |      must be fixed -> correct the value or untick a skid, then reopen Batch Edit
   |      looks right   -> carry on
   |
   +-- Apply to the selected skids
   +-- Save and Finish writes the change
   |
   +-- Print a report of what changed
   +-- Reprint just this batch: Print Batch queues the labels and opens them
```

## Selecting Several Skids At Once

- Tick the checkbox at the left of each row you want, or hold `Shift` and click to take a run of rows.
- Hold `Ctrl` and click to add or remove one row at a time.
- The checkbox in the column heading selects or clears the whole page.
- If you have filtered the list down, you are offered a second, explicit action to select every
  matching row, not just the ones on screen. The app never quietly selects rows you cannot see.
- Your selection survives paging to the next page and back.
- Rows belonging to another user stay hidden, exactly as they do today.

## Seeing What You Have Selected

As soon as you tick something, a bar appears above the list showing:

- How many skids you have selected.
- The combined weight or quantity.
- Which part numbers and locations are involved, so you can spot a stray row before you change it.
- The actions available for the selection.

A skid you have ticked is lightly shaded. A skid the app is about to change gets a coloured edge
marker as well, so a selection is never confused with a pending change.

## Checking That The Weights Balance

This is the part that answers "feel free to check my math".

- Type the expected total from the transfer request into the selection bar.
- The app shows your selected total, the expected total, and the difference, with the sign.
- A small tolerance can be configured, so trivial rounding does not raise a warning.
- If the difference is larger than the tolerance, you get a plain warning and a required
  acknowledgement before the change can be applied.

The app will never quietly adjust your expected figure to match what you found. If a batch does not
balance, it stays visibly out of balance, and that imbalance is carried onto the report.

## Changing Many Skids At Once

Batch Edit opens a single window listing every field you are allowed to change. Each field has a tick
box. A field you do not tick is left exactly as it is on every selected skid.

The fields you can override are:

- Part ID
- Heat / Lot number
- Location
- Weight or quantity
- Packages per load
- Package type
- PO number
- The user-set and customer variables

A few details that matter:

- If the selected skids disagree on a value, the field says so plainly, for example `(mixed — 3
  values)`, instead of showing the first skid's value as if it applied to all of them.
- Weight and quantity offer more than one way to change the number, because overwriting is often
  wrong: set to, add, subtract, scale by a factor, or distribute a target total across the rows.
- Changing the Part ID also updates the description, unit of measure, part type, and quality hold
  for each skid, taken from the new part rather than copied from the first row.
- Changing the PO number is optional and off by default, because correcting a material is not the
  same as re-receiving it.

## Reviewing Before You Save

Nothing is written until you press Save and Finish.

- The window shows a preview listing every change as before and after, skid by skid.
- A validation strip lists anything that needs attention, naming the affected skids.
- Applying writes the change into the rows you can see, and the window closes.
- Cancel, closing the app, or losing power leaves the database untouched.

## What The App Will Refuse To Do

- It will not apply a blocked change. If the part number does not exist exactly, the change cannot be
  applied until it is corrected.
- It will not accept an approximate part match without an explicit confirmation from you. An
  approximate match is fine when you are changing one row and are watching it closely; across forty
  rows it is a mistake waiting to happen.
- It will not guess or silently correct your expected total.
- It will not report labels as printed when they have only been queued.
- It will not quietly apply part of a batch. If some rows fail to save, you are told how many, and
  the rows that did not save stay marked as changed so you can retry.

## Printing A Report Of What Changed

Once a batch is saved you can print a report containing:

- The batch reference, the date and time, and the operator.
- What moved, for example `MMC000659` to `MMC000767`.
- Every skid that changed, with its location, heat or lot number, quantity, and both part numbers.
- A reconciliation section showing the selected total, the expected total, the difference, and an
  explicit statement when it does not balance.
- A signature line for whoever physically re-labels and verifies the skids.

That signature line is deliberate. The original note said *"I will fix the system when completed"*,
which means a person is reconciling afterwards. They should have something to sign.

## Reprinting The Labels For One Batch

Reprinting already exists, and this work extends it rather than replacing it.

- After you save a batch, the Reprint Labels module already knows which skids you just changed, and
  opens on them, already selected. You do not go looking for them again.
- New filters let you narrow to the current batch, or to rows modified in the last day or week. That
  second option matters: a batch retagged yesterday currently disappears from the default view.
- Location and Heat / Lot become available as columns and as search options, because "which skid was
  in V-F0-07" is the natural question afterwards.
- One button, **Print Batch**, does the whole run: it confirms what is about to be printed, queues
  the labels, and opens them. If some were already queued or some fail, you are told which, and can
  retry just those.

## Worked Example

What the attached transfer request turns into:

1. Edit Mode is filtered to the part number on the paperwork.
2. The three skids named on the locations report are ticked.
3. `154540` is typed as the expected total. The app immediately shows how the selection compares, and
   keeps showing the difference if it does not match.
4. Batch Edit changes Part ID from `MMC000659` to `MMC000767` for those three skids and nothing else.
5. The preview is checked, then applied, then saved once.
6. A batch report is printed for the transfer paperwork.
7. Print Batch re-queues those three labels and opens them.

## What Is Not Included

- No change to how labels are actually printed. The label file and the printing station stay as they
  are.
- No automatic printing on save. Someone still has to press the button.
- No change to who can edit what. Skids created by another user remain hidden and untouchable.
- No change to Infor Visual. That system stays read-only.

## For More Detail

The engineering detail lives in four documents:

- `docs/specs/BatchSkidEdit_UX_Specification.md` — the selection and batch edit design.
- `docs/specs/BatchReprint_Automation_Spec.md` — the handoff to Reprint Labels and the print flow.
- `Module_Receiving/Proposed_UI_Changes.xaml.md` — the exact controls that change.
- `Module_Shared/Proposed_Validation_Safeguards.md` — how errors and partial failures are surfaced.
