# Bulk Inventory — User Guide

**Last Updated:** 2026-03-08
**Audience:** Warehouse staff and receiving personnel who process inventory transfers and
work-order receipts in Infor Visual.
**Module replaces:** The standalone *Visual Inventory Assistant* (MTM Receiving Manager WinForms app).

---

## Contents

- [Bulk Inventory — User Guide](#bulk-inventory--user-guide)
  - [Contents](#contents)
  - [1. What This Module Does](#1-what-this-module-does)
  - [2. Before You Start — Requirements](#2-before-you-start--requirements)
  - [3. Opening Bulk Inventory](#3-opening-bulk-inventory)
  - [4. Building Your Batch](#4-building-your-batch)
    - [4.1 Adding Rows](#41-adding-rows)
    - [4.2 Transaction Types](#42-transaction-types)
    - [4.3 Editing and Deleting Rows](#43-editing-and-deleting-rows)
    - [4.4 Clearing the Entire Batch](#44-clearing-the-entire-batch)
  - [5. Validating Your Batch](#5-validating-your-batch)
  - [6. Pushing the Batch to Infor Visual](#6-pushing-the-batch-to-infor-visual)
    - [6.1 What Happens Automatically (Consolidation)](#61-what-happens-automatically-consolidation)
    - [6.2 Progress Overlay](#62-progress-overlay)
    - [6.3 When Visual Requires Your Input](#63-when-visual-requires-your-input)
    - [6.4 Cancelling Mid-Push](#64-cancelling-mid-push)
  - [7. Reviewing Results (Summary Screen)](#7-reviewing-results-summary-screen)
  - [8. Re-Pushing Failed Rows](#8-re-pushing-failed-rows)
  - [9. Row Status Reference](#9-row-status-reference)
  - [10. Keyboard Shortcuts](#10-keyboard-shortcuts)
  - [11. Common Problems \& Fixes](#11-common-problems--fixes)
    - ["Bulk Inventory" is not visible in the navigation bar](#bulk-inventory-is-not-visible-in-the-navigation-bar)
    - ["Infor Visual is open under a different login"](#infor-visual-is-open-under-a-different-login)
    - [Push Batch button is greyed out (validation warnings present)](#push-batch-button-is-greyed-out-validation-warnings-present)
    - ["Interrupted Batch" banner appears on startup](#interrupted-batch-banner-appears-on-startup)
    - [Infor Visual does not start ("Exe not found — check Settings › Shared Paths")](#infor-visual-does-not-start-exe-not-found--check-settings--shared-paths)
    - [A row stays "WaitingForConfirmation" for several minutes](#a-row-stays-waitingforconfirmation-for-several-minutes)
    - [A row shows "Parts popup did not close" in the error column](#a-row-shows-parts-popup-did-not-close-in-the-error-column)

---

## 1. What This Module Does

Bulk Inventory lets you build a list of inventory transactions — either **warehouse transfers**
(moving stock from one location to another) or **work-order-based receipts** (receiving parts
against a work order) — and then send them all to Infor Visual in one go.

You no longer need the old standalone WinForms app or Google Sheets.  Everything happens inside
the MTM Receiving Application.

---

## 2. Before You Start — Requirements

| Requirement | Why |
|-------------|-----|
| Your MTM login has **personal** Infor Visual credentials (not `SHOP2` or `MTMDC`) | The module uses your credentials to log into Visual on your behalf. Shared accounts cannot perform inventory transactions. |
| Your Visual credentials are saved under **Settings › Users** (your own user row) | The app reads them at login time. An admin can set them for you if needed. |
| The Infor Visual executable path is set under **Settings › Shared Paths** | Default is `\\visual\visual908$\VMFG`. Only an admin needs to change this. |

> **Don't see "Bulk Inventory" in the navigation bar?**
> Your account is using a shared Visual login (`SHOP2` or `MTMDC`).  Ask an admin to open
> **Settings › Users**, find your user record, and update the Infor Visual username and
> password to your personal credentials.  The Bulk Inventory item will appear in the bottom
> navigation bar the next time you log in.

---

## 3. Opening Bulk Inventory

1. Look in the **bottom** of the left navigation bar (below "End of Day Reports" and
   "Ship/Rec Tools").
2. Click **Bulk Inventory**.
3. The data-entry grid opens.  If a previous session was interrupted (e.g., the app crashed
   mid-push), you will see an **"Interrupted Batch"** banner — see
   [Common Problems & Fixes](#11-common-problems--fixes) for what to do.

> **"Infor Visual is open under a different login" error?**
> If someone else is already logged into Visual on this machine (or you are logged in under a
> different account), the Push Batch button will be disabled.  Either close Visual manually or
> update your Visual credentials in Settings to match the currently-open session, then
> re-click Push Batch to continue.

---

## 4. Building Your Batch

### 4.1 Adding Rows

Click **Add Row** (toolbar, top of the grid) to append a blank row.
Fill in the columns directly in the grid:

| Column | Required | Notes |
|--------|----------|-------|
| **Part ID** | ✅ | The Infor Visual part number. |
| **From Location** | ✅ for Transfer | Leave blank for New Transaction rows. |
| **To Location** | ✅ | Destination location code. |
| **Quantity** | ✅ | Must be a positive number. |
| **Work Order** | ✅ for New Transaction | Format `WO-######` (e.g. `WO-123456`). Leave blank for Transfer rows. |

> **Warehouse** is always `002` and is filled in automatically — you do not enter it.
> **Lot No** defaults to `1` and is sent to Visual automatically.  Both can be changed by
> an admin under **Settings › Bulk Inventory Defaults**.

### 4.2 Transaction Types

The module determines the transaction type from the **Work Order** column:

| Work Order column | Transaction type sent to Visual |
|-------------------|---------------------------------|
| *blank* | **Transfer** — moves stock between locations. Uses the *Inventory Transfers* window in Visual. |
| `WO-######` filled in | **New Transaction** — receipt against a work order. Uses the *Inventory Transaction Entry* window in Visual. |

You can mix both types freely in the same batch.

### 4.3 Editing and Deleting Rows

- **Edit** any cell by clicking it directly in the grid.
- **Delete a row** by selecting it and clicking **Delete Row** (toolbar), or pressing
  `Delete` when the row is selected.  You will be asked to confirm.
- Rows can only be deleted **before the push starts**.  Once you click Push Batch and the
  automation overlay appears, no rows can be deleted until the batch finishes or is cancelled.

### 4.4 Clearing the Entire Batch

Click **Clear All** to remove every row.  A confirmation prompt will appear before anything
is deleted.

---

## 5. Validating Your Batch

Before pushing, click **Validate All** to check every row against Infor Visual:

- Each **Part ID** is verified to exist in the Visual `PART` table.
- Each **From Location** and **To Location** is verified to exist for warehouse `002`.

Results appear **inline** in the grid — a warning message appears in the row itself, not as a
pop-up dialog.  Warnings are highlighted in orange; rows without issues show nothing.

**You cannot push the batch while any row has an unresolved warning.**  The Push Batch button
stays disabled until:
1. You fix all flagged cells, then
2. Click **Validate All** again to confirm the issues are resolved.

> **Tip:** Validate All is optional but strongly recommended before large batches.  Catching
> bad Part IDs or missing locations now saves you from failed rows mid-push.

---

## 6. Pushing the Batch to Infor Visual

When your batch is ready and Validate All shows no warnings, click **Push Batch** (or press **F5**).

The app will prompt you once to confirm before the push begins.

### 6.1 What Happens Automatically (Consolidation)

Immediately before the push starts, the app **consolidates** duplicate rows:

> If two or more rows share the same **Part ID + From Location + To Location**, their
> quantities are added together and sent as a single Visual transaction.

*Example:*

| Part ID | From | To | Qty | After consolidation |
|---------|------|----|-----|---------------------|
| ABC-123 | DOCK | BIN-01 | 10 | **15** (one transaction) |
| ABC-123 | DOCK | BIN-01 | 5  | *(merged above)* |
| XYZ-789 | DOCK | BIN-02 | 3  | **3** (separate transaction) |

The original rows stay in the grid showing a **Consolidated** status badge so you have a
full audit trail.  Visual only receives the merged rows.

### 6.2 Progress Overlay

Once the push starts, a **dark overlay** covers the entire application window.  While the
overlay is visible:

- All mouse clicks and keyboard input to the MTM app are blocked.
- The overlay shows a live status message (e.g., *"Processing row 3 of 12 — Part: ABC-123"*).
- A progress indicator and row counter keep you informed of where the batch is.
- The only active control is the **Cancel** button.

**Do not close or click on the Infor Visual window that opens.**  The app is filling it in
automatically.  Moving focus away from Visual will not stop the push, but unnecessary
interaction could cause errors.

### 6.3 When Visual Requires Your Input

Occasionally Infor Visual displays a warning dialog (typically a **duplicate transaction
warning**) that requires a human decision before it can proceed.

When this happens:
- The overlay message changes to:
  *"⚠ Visual requires your input — please respond to the dialog in Infor Visual to continue."*
- The progress indicator continues to pulse.
- **Bring the Infor Visual window to the front** and read the dialog.
- Click whichever response is correct for that transaction.
- The MTM app automatically detects when the dialog is dismissed and continues with the
  next row.

> The app waits up to **5 minutes** for you to dismiss the Visual dialog.  If you do not
> respond in time, the row is marked as Failed and the batch moves on.

### 6.4 Cancelling Mid-Push

Click the **Cancel** button on the overlay at any time to stop the batch.

What happens when you cancel:
1. The current row being processed is marked **Skipped**.
2. The app sends an Escape key to close any partially-filled Visual window.
3. The overlay disappears.
4. The Summary screen opens showing counts up to the point of cancellation.

Rows that already completed successfully before the cancel keep their **Success** status and
do not need to be re-pushed.

---

## 7. Reviewing Results (Summary Screen)

After all rows are processed (or the batch is cancelled), the Summary screen appears
automatically showing three counts:

| Badge | Meaning |
|-------|---------|
| 🟢 **Success** | Transaction was sent to Visual and Visual accepted it. |
| 🔴 **Failed** | Something went wrong — see the inline error message on each failed row for details. |
| ⚫ **Skipped** | Row was skipped via F6 or the batch was cancelled while this row was active. |

Click **Done** to return to the data-entry grid and start a new batch.  Your completed rows
are cleared automatically.

---

## 8. Re-Pushing Failed Rows

On the Summary screen, failed rows are listed with checkboxes.

1. Check the rows you want to retry.
2. Fix the underlying issue first (e.g., update the Part ID or location, or wait for Visual
   to be available).
3. Click **Re-push Selected**.

> **Consolidation is not applied to re-push.**  Each selected row is sent to Visual exactly
> as it appears — no merging.  This is intentional: the original consolidation already happened
> and you are retrying specific failed rows only.

---

## 9. Row Status Reference

| Status | Badge colour | What it means |
|--------|-------------|---------------|
| **Pending** | Grey | Row is waiting to be pushed. |
| **InProgress** | Blue (pulsing) | Currently being filled in Visual. |
| **WaitingForConfirmation** | Amber | Visual showed a dialog that requires your response. |
| **Success** | Green | Sent to Visual successfully. |
| **Failed** | Red | An error occurred.  Check the error message column for details. |
| **Skipped** | Dark grey | Row was skipped (F6) or cancelled. |
| **Consolidated** | Light purple | Original row whose quantity was merged into another row before pushing. |

---

## 10. Keyboard Shortcuts

These shortcuts are also shown in the **legend strip at the bottom of every screen**.

| Key | Where | Action |
|-----|-------|--------|
| **F5** | Data Entry screen | Push Batch (same as clicking the Push Batch button) |
| **F6** | During a push (between rows) | Skip the next pending row |
| **Escape** | Data Entry screen | Prompt to Clear All |
| **Escape** | During a push (overlay visible) | Cancel the push |
| **Tab** | Grid cells | Move between cells in a row |
| **Enter** | Grid row | Confirm the current row and move to the next |

---

## 11. Common Problems & Fixes

### "Bulk Inventory" is not visible in the navigation bar

**Cause:** Your Visual credentials are set to a shared account (`SHOP2` or `MTMDC`).

**Fix:** Ask an admin to open **Settings › Users**, find your user record, and update the
Infor Visual username and password fields to your personal credentials.  The nav item
becomes visible the next time you log in.

---

### "Infor Visual is open under a different login"

**Cause:** Infor Visual is already running on this machine and the account logged into Visual
does not match your MTM Visual credentials.

**Fix — Option A:** Close Infor Visual completely, then click **Push Batch** again.

**Fix — Option B:** Ask an admin to open **Settings › Users**, find your user record, and
update the Visual credentials to match the account currently open in Visual.  Then manually
click **Push Batch** again.

---

### Push Batch button is greyed out (validation warnings present)

**Cause:** One or more rows have unresolved Validate All warnings (invalid Part ID or location).

**Fix:** Correct the flagged cells in the grid, then click **Validate All** again.  Once all
warnings are cleared the button automatically becomes active.

---

### "Interrupted Batch" banner appears on startup

**Cause:** The app was closed or crashed while a push was in progress.  Any rows that were
still showing *InProgress* at the time have been automatically marked **Failed**.

**What to do:**
- Click **View in Summary** to see which rows failed.
- Select any rows you want to retry and click **Re-push Selected**.
- Click **Done** when finished to dismiss the banner and start fresh.

---

### Infor Visual does not start ("Exe not found — check Settings › Shared Paths")

**Cause:** The path to the Infor Visual executable is wrong or the network share is not
accessible.

**Fix:** Ask an admin to verify **Settings › Shared Paths › Infor Visual Executable Root Path**
(default: `\\visual\visual908$\VMFG`).

---

### A row stays "WaitingForConfirmation" for several minutes

**Cause:** Infor Visual is showing a duplicate-transaction dialog that has not been dismissed.

**Fix:** Bring the Infor Visual window to the front and respond to the dialog.  The app will
resume automatically.  If you wait more than 5 minutes without responding, the row is marked
Failed and the batch continues.

---

### A row shows "Parts popup did not close" in the error column

**Cause:** Infor Visual showed a Parts lookup dialog, and the app could not dismiss it within
3 seconds (rare on slow networks).

**Fix:** Re-push the failed row from the Summary screen.  If it keeps failing, verify the Part
ID is correct with **Validate All** before re-pushing.

---

*For issues not covered here, contact your MTM application administrator.*
