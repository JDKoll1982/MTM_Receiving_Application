# Conversation checklist - 2026-11-17

Everything worked on in this conversation, in the order it was requested, written as things to try and
things to see. Tick each box once you have checked it.

---

## 0. Before you check anything

- [ ] **Close the running app** before you rebuild. A running `MTM_Receiving_Application.exe` locks the output
      and the build fails with MSB3021/MSB3027.
- [ ] **Deploy the new stored procedure.** The Record Days tab cannot read anything until
      `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetTopDaysByItemType.sql`
      has been deployed (use the database deployment tool or run it by hand).
- [ ] **Rebuild:** `dotnet build MTM_Receiving_Application.slnx`.
- [ ] **Tests:** `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj`
      (last run: 1344 passed, 264 skipped, 0 failed).

---

## 1. Receiving Analytics - the period picker now changes the chart

Open **Extra Tools > Receiving Analytics > ERP Data**.

- [ ] Change the period from **Day** to **Week**. The chart re-renders on its own; points collapse into weeks.
- [ ] Change it to **Month**. Points collapse into months.
- [ ] Change it back to **Day**. The chart returns to one point per day without needing **Load** again.
- [ ] Change the chart type (bar / line / etc.) with a period selected. The shape and the bucketing stay consistent.
- [ ] Check the axis labels show the bucket start (week beginning, month) rather than a mid-bucket date.

**What changed underneath:** the range filter no longer widens itself, and the chart is rebuilt from the cached
points by bucketing them - it used to re-query and effectively always show days.

---

## 2. Receiving Analytics - "Trend vs Last Week" is gone

- [ ] The **Compare to previous period** checkbox is no longer on the page.
- [ ] There is no fourth stat card saying anything about last week.
- [ ] The three remaining stat cards still show current-period totals.
- [ ] Nothing errors in the log when the page loads (the trend code and its previous-period query are deleted,
      not just hidden).

---

## 3. Receiving Analytics - the new "Record Days" tab

Open **Extra Tools > Receiving Analytics > Record Days**.

- [ ] **First load:** the range defaults to roughly the last 90 days; press **Load** and the tables fill.
- [ ] **Three ranked lists side by side:** **Top Coil Days**, **Top Flat Stock Days**, **Top Other Item Days**.
      Each row shows rank, day, loads, and labels.
- [ ] **Item type rule:** coil = part contains `MMC`, flat stock = part contains `MMF`, everything else = other.
      Same rule the App Data tab uses, so a part in one tab shows up in the matching list here.
- [ ] **Loads are the ranking figure** (not label count), and the labels column is informational only.
- [ ] **Weekends are excluded**, matching every other app figure.
- [ ] **Show: Top 5 / Top 10 / Top 20** - changing it reloads immediately.
- [ ] **From / To** - changing the range waits for **Load**. **Clear** empties all four tables.
- [ ] **The 4th table, "Busiest Days", spans the full width** under the three lists and ranks the busiest days
      overall by that day's own total loads.
- [ ] **A load carrying two item types is counted once** in Busiest Days, but once in each type list it belongs to.
      Spot-check one day against the per-type tables to confirm the total is not the sum of the three lists.
- [ ] **Tidiness:** all four tables share one row layout, numeric columns are right-aligned under their headers,
      each card has a rule under its header, and the empty state reads sensibly when a range has no data.
- [ ] **Sort order** in every table is busiest day first.

---

## 4. Module Scanner - the Workbench watches the Infor Visual window

Open **Scanner > Workbench**.

- [ ] **On open:** with the Infor Visual Inventory Transactions window **closed**, a note appears immediately to the
      left of **Remove Selected** reading "Inventory Transactions window is closed - Open it to enable this button."
- [ ] **Button gating:** **Remove Selected** is greyed out even when a session line is selected.
- [ ] **Open the Infor Visual window.** Within about 5 seconds the note disappears and the button enables (with a line
      selected) without reloading the page.
- [ ] **Close the Infor Visual window again.** Within about 5 seconds the note returns and the button greys out.
- [ ] **Not focus-dependent:** clicking away from Infor Visual does not trip the notice - only closing the window does.
- [ ] **Wrong-window guard:** a same-titled window owned by a different process does not count as open.
- [ ] **Leave the page** (navigate away or close the Workbench), leave it a minute, and confirm nothing keeps polling -
      the watch loop stops on deactivate.

---

## 5. Material Availability Board - two new columns

Open **Extra Tools > Material Availability**, pick a date, and expand a card with associated parts. Check both the
board and the print/export.

- [ ] **New UI figures per associated-part row:** **Material required** and **Remaining after run**.
- [ ] **Rows are ordered by run date**, not by part or work order.
- [ ] **A past job** shows blanks in both columns and does not reduce the running balance.
- [ ] **Remaining after run** is a running balance: the second job's remaining equals the first job's remaining minus
      the second job's requirement.
- [ ] **The starting on-hand** is total positive locations minus any location named `SHIP` and any location whose id
      contains `NCM`. Sanity-check one part where you know the pile.
- [ ] **Negative balance is red.** Find or force a case where a job takes more than is left and confirm only the
      negative figure is tinted - normal rows stay the theme colour (dark theme included).
- [ ] **Print / export sheet:** the associated-parts table has the same two columns, in the same order, with the same
      values, and the text export shows "Material required" and "Remaining after run" headers.
- [ ] **Requirement maths:** a row's requirement is the estimated use (component requirement plus scrap), not the raw
      component quantity.

---

## 6. Receiving - Non-PO Entry advances after a part is found

Open **Receiving > Enter PO Number**, choose **Non-PO Entry**.

- [ ] Type a valid part and press **Lookup Part**. When it is found, the walk **moves to the load step** on its own.
- [ ] A status message reads "Part {part} selected. Continue to the load step."
- [ ] **Fuzzy picker path:** type a partial part, pick one from the picker, and confirm the walk also advances.
- [ ] **Blur / Enter does not advance.** Tab out of the part box, or press Enter, with a part that exists - the part
      fills in but the page stays put.
- [ ] **A part that is not found** leaves you on the Enter PO page with the usual message.
- [ ] **A quality-hold part that is not acknowledged** leaves you on the Enter PO page - it does not advance past the
      hold.
- [ ] **Editable message:** the new text is a settings entry,
      `Receiving.Messages.Info.NonPoPartReady`, in Settings like every other receiving message.

---

## 7. Receiving - Remaining on PO no longer reads 0

Open a PO whose part card showed **Remaining on PO 0** before.

- [ ] **Remaining on PO** now shows ordered minus received for every part, not 0.
- [ ] **Arithmetic check:** 55,000 ordered with 6,570 received shows **48,430**.
- [ ] **The column is consistent** across a PO with one part and a PO with several parts.
- [ ] **Mock / offline mode** also shows a sensible remaining figure instead of 0.
- [ ] The figure is the same rule used elsewhere (`ORDER_QTY - TOTAL_RECEIVED_QTY`) so it agrees with the delivery
      schedule and the remaining-quantity lookups.

---

## 8. Label Overlay - the window explains its delays

Open a receipt's labels so the **Label Overlay** window appears.

- [ ] **On open:** a modal card covers the window while it loads - "Loading label overlay", a bar, and
      "Step 1 of 5" through "Step 5 of 5": saved setup, printers, paper sizes, trays, label queue.
- [ ] **Blocked input:** while the card is up, clicking behind it does nothing. It disappears on its own when loading
      finishes.
- [ ] **Switch the settings tab to another output:** the card appears again for paper sizes and trays.
- [ ] **Change the printer:** card appears, then goes away when the paper sizes and trays have been read.
- [ ] **Reset setup:** card appears and clears.
- [ ] **Printer setup:** the card appears while the printer's own dialog opens. If the dialog opens over it, the card
      is behind the dialog and disappears once you close it.
- [ ] **Print:** the card appears with "Printing tags" or "Printing card sheets", the progress bar, and a
      "Preparing N tag(s)…" / "Printing page n of m…" / "Sent to the printer." line. The button keeps showing the same
      progress, as before.
- [ ] **Second click while printing** does nothing (the card blocks it, and `PrintAsync` still guards itself).
- [ ] **The card never stays up.** Close the window during a load, or hit a printer error, and confirm it is gone -
      a stuck scrim would lock the window.
- [ ] **Remembered caveat:** while the WinForms print preview is open it owns the mouse and the card behind it does not
      repaint. That is expected - the preview is the feedback at that point.

---

## 9. Repo hygiene

- [ ] `CHANGELOG.md` **Unreleased** has entries for all seven items (Non-PO advance, Material Availability columns,
      Scanner watch, Record Days, Label Overlay progress card under **Added**; Remaining on PO and the period toggle
      under **Fixed**; Trend card under **Removed**).
- [ ] `Database/Database_Deployment/sql-object-catalog.json` and `StoredProcedure-Test-Checklist.md` include the new
      reporting procedure.
- [ ] Settings manifest (`Module_Settings.Core/Defaults/settings.manifest.json`) lists the new
      `Receiving.Messages.Info.NonPoPartReady` key.
- [ ] Help/docs touched: `docs/help/scanner/workbench.html`, `docs/features/receiving/index.html`,
      `Module_ShipRec_Tools/docs/ReceivingAnalyticsAndDeliverySchedule_FeatureUpdate.md`,
      `Module_ShipRec_Tools/docs/ReceivingAnalytics_TwoTab_AnalyticsCatalog.md`, new
      `Module_ShipRec_Tools/docs/MaterialAvailabilityAssociatedPartsBalance.md`.
- [ ] `git status` shows no stray build output and no temporary files.
- [ ] New test-infrastructure file: `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/DunnageImagePathCollection.cs`.

---

## 10. Things that are known and deliberate

- [ ] **Record Days naming:** you asked for a "Top Days" tab; it is labelled **Record Days** because that is the name
      chosen when the layout questions were asked.
- [ ] **One accidental first-request miss:** the period toggle fix and the Trend removal are both in, but they are
      separate checklist sections above only so each can be checked on its own.
- [ ] **Flaky test now fixed:** `Service_Tool_DunnageBookTests.BuildBookAsync_ShouldEmbedSharedRootImage_WhenCachedCopyIsStale`
      used to fail roughly once in twelve full parallel runs and always passed alone. Cause: it and
      `Service_DunnageImageStorageTests` both move `Helper_DunnageImagePaths`'s process-wide root folders, and xUnit
      ran the two classes in parallel, so one class repointed the roots while the other was reading an image through
      them. Both classes now share one non-parallel xUnit collection and put the roots back after every test. Verified
      by reproducing the failure 1 in 12 runs without the fix and 0 in 20 runs with it.
- [ ] **Nothing else was refactored** outside the seven items above.
