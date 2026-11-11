# Change Log

Last Updated: 2026-11-11

## 2026-11-11 - Module documentation created

- Added this documentation tree under `Module_VisualAutomation/Documentation/`.
- Content was written from the current source: the two services, both helpers, the probe catalogue, the
  view model, the view and dialogs, the models, and the three unit test classes.
- No production file was modified.

## Behaviour notes recorded from the code (no change date available)

The module's git history was not used for this log, so these are recorded as facts about the current code
rather than as dated changes. Each one is visible in the listed file.

- The inputs gate blocks every run while any of `BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID` is blank,
  including runs made up only of read-only probes (`ViewModel_VisualAutomation_MacroTester.RequiresInputs`).
- Probe runtime is selected by a `switch` on `probe.Id`, and an unmatched id returns
  `This probe has no implementation.` (`Service_VisualAutomation_ProbeRunner.RunAsync`).
- The write path commits with Tab and waits 320 ms before reading the field back
  (`Service_VisualAutomation_WindowDriver.WriteValueAsync`).
- `Enum_VisualProbeStatus.Skipped` is defined but no code assigns it; `Reset` returns cards to `NotRun`
  (`Model_VisualProbeCard`, converter, view model).
- `Model_VisualProbeInputs.ToWriteValues()` exists and is tested but is not called by the runner, which
  reads `inputs.BolId`, `inputs.VendPackId`, and `inputs.VendFreightId` directly.
- The activity log on the page is capped at 500 entries (`ViewModel_VisualAutomation_MacroTester.AddLog`).
- `Reset` clears cards and log entries but leaves the entered field values in place.

## Where to record future changes

Add a dated bullet at the top of this file for any change that alters page behaviour, a probe verdict, the
field map, or the writable set. Reference the files you touched so the next reader can verify the claim.

What counts as a change worth logging here:

- a new, removed, or reworded probe, including its id or category;
- a change to a verdict string that support staff read;
- a change to a label, binding, safety class, or the writable set;
- a change to the inputs gate or to which probes need values;
- a change to the focus or restore behaviour of the write path.

What does not need an entry: comment typos, formatting, and test-only refactors that leave behaviour
identical.
