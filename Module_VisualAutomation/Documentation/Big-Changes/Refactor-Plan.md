# Refactor Plan

Last Updated: 2026-11-11

There is **no approved refactor mandate** for this module. What follows is a proposal that a developer or
architect can accept, reject, or reshape. Nothing here is scheduled.

## Proposed scope: make the module testable without a live VISUAL window

Today the only automated tests cover static data (field map, bindings) and the inputs model. Window
discovery, label-to-control resolution, probe dispatch, and the inputs gate are untested because they are
buried inside a class that talks to `user32.dll` and a live process.

Proposed change, behaviour-preserving:

1. Extract the row-matching rules from `Service_VisualAutomation_WindowDriver.ResolveValueControls` into a
   pure function over a list of element records. The three constants (`LabelMatchTolerance`,
   `MinimumValueHeight`, `MinimumValueWidth`) move with it.
2. Extract the `probe.Id` dispatch from `Service_VisualAutomation_ProbeRunner.RunAsync` into a small
   table or method that is reachable from a test without a driver.
3. Extract `RequiresInputs` decision logic into a pure predicate on `Model_VisualProbeInputs` so the gate
   can be tested directly.
4. Add xUnit tests for all three, named `MethodName_ShouldResult_WhenCondition`.

## Goals

- Raise confidence in the two riskiest areas: positional field matching and probe dispatch.
- Keep every user-visible verdict string unchanged.
- No new NuGet packages, no DI changes, no new production dependency.

## Risks

| Risk | Why it matters | Mitigation |
| ---- | -------------- | ---------- |
| Geometry constants drift during extraction | Field matching is positional; a changed threshold silently mis-targets fields | Move constants unchanged; assert the same resolved set for a captured element list |
| Verdict text changes | The details dialog and support runbook quote verdicts | Treat verdict strings as contract; compare before and after |
| Dispatch table diverges from the catalogue | New probe returns `This probe has no implementation.` | Test that every catalogue id dispatches to a non-null implementation |
| Refactor is mistaken for approval to change behaviour | The inputs gate is a known rough edge | Keep gate behaviour identical; log any behaviour change as a separate decision |

## Rollback

- Each step is a separate commit; revert the commit to restore the original class.
- No schema, stored procedure, DI registration, or file layout change is involved, so rollback has no data
  or startup consequences.
- A live run of P01 to P10 before and after is the acceptance evidence; keep both details-dialog exports.

## Out of scope

- Changing which probes exist, their ids, or their wording.
- Changing the inputs gate, the writable set, or the safety classes.
- Any change to DI registration, package references, or the navigation route.

## Success criteria

- `dotnet build MTM_Receiving_Application.slnx` clean.
- `dotnet test ... --filter "FullyQualifiedName~Module_VisualAutomation"` green, with new tests for the
  extraction points.
- A full live run on a loaded receipt returns the same ten verdicts as before the refactor.
- No file outside `Module_VisualAutomation/` and the VisualAutomation test folder is touched.
