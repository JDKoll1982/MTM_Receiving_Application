# Common Issues

Last Updated: 2026-11-11

Symptom -> likely cause -> what to check. The quoted text is what the code actually reports.

## "No VMRCVENT.EXE window is open"

- Likely cause: Purchase Receipt Entry is closed, or VISUAL is on the main menu only.
- Check: the target line on the page; Task Manager for `VMRCVENT`.
- Fix: open Inventory > Purchase Receipt Entry, then press **Check VISUAL window**.

## "Could not enumerate VMRCVENT processes: ..."

- Likely cause: the process list could not be read at all.
- Check: the exception text in the target line. Run the app on the same machine, not over a remote session
  that blocks process enumeration.

## "The window was found but its executable path could not be read"

- Likely cause: the app and VISUAL run at different elevation levels.
- Check: `ExecutablePath` is blank but `ElementCount` is not.
- Fix: run both at the same elevation. Reads usually still work; do not rely on path-based checks.

## "The window was found but UI Automation could not read it: ..."

- Likely cause: the window closed mid-read, or the handle is stale.
- Check: press **Check VISUAL window** again. `ElementCount` tells you whether a read succeeded.

## "The Purchase Receipt Entry window could not be activated, so no keystrokes were sent"

- Likely cause: Windows refused `SetForegroundWindow` for the VISUAL window.
- Check: is VISUAL minimised, or is another window modal over it? The app never types blind.
- Fix: click the VISUAL window once, then rerun the write probe.

## "Another window took focus before typing. No keystrokes were sent."

- Likely cause: something took focus between activation and the focus re-check.
- Check: other applications with topmost windows or focus-stealing notifications.

## "Variable 'KEY' is not in the writable field list"

- Likely cause: a probe asked the driver to write a field outside `_writable`.
- Check: `Helper_VisualAutomation_FieldMap.IsWritable`. Only `BOL_ID`, `VEND_PACK_ID`, and
  `VEND_FREIGHT_ID` are writable. The guard runs before the window is touched.

## "Variable 'KEY' has no control on the visible window"

- Likely cause: the label did not resolve to a value control, so the write has nowhere to click.
- Check: `P05_ReferenceFields` and `P06_VariableCoverage` verdicts, then the row layout on screen.

## P02 fails with "expected a stable count of at least 30"

- Likely cause: the receipt window is still painting, or a modal dialog is open inside VISUAL.
- Check: close any VISUAL popup and rerun. Two reads of an idle window return the same count.

## P07 or P08 fails with "VISUAL did not keep the typed value"

- Likely cause: the field rejected the text or the click missed the box.
- Check: the verdict names the field and both strings. Try a shorter value with letters, digits, and a
  hyphen, which is what P08 is designed to exercise.

## P09 fails with "The receipt identity changed while probes were running"

- Likely cause: the order, site, or receiver id changed between the two snapshots - someone typed in
  VISUAL during the run, or the window switched receipts.
- Check: `POSTING_STATE_BEFORE` and `POSTING_STATE_AFTER` in the details dialog.

## P10 fails with "These fields differ from the starting snapshot"

- Likely cause: a write probe did not restore a value, or you edited VISUAL while the run was going.
- Check: `DIFFERENCES` names the field and both values. `BASELINE_AT` and `COMPARED_AT` give the times.

## "This probe has no implementation."

- Likely cause: a probe exists in `VisualProbeCatalog` but has no arm in the `RunAsync` switch.
- Check: [Daily Tasks](../How-To-Guides/Daily-Tasks.md), task 1, step 3.

## The Run button appears to do nothing

- Likely cause: the inputs gate. `RequiresInputs()` blocks the run and raises `ProbeInputsRequested`.
- Check: the inputs summary line under the buttons says which values are still blank. The dialog refuses
  to close with **Use these values** until all three have text.
