# Checks and Health

Last Updated: 2026-11-11

## Unit tests to run

```powershell
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Module_VisualAutomation"
```

What they actually cover:

| Test class | Proves |
| ---------- | ------ |
| `Helper_VisualAutomation_FieldMapTests` | 87 macro variables, the 16 labels, label normalisation, writability, and that the probe catalogue only reports known variables |
| `Helper_VisualAutomation_VariableBindingTests` | 87 bindings, table/column for 18 named variables, `PO_USER_n` -> `USER_n`, runtime and macro-output classes, all 45 udf slots as `Scratch`, writable set strictly inside `ReceiptOnly` |
| `Model_VisualProbeInputsTests` | blank start state, completeness, missing-field names, trimming, clone independence |

They do not cover `Service_VisualAutomation_WindowDriver`, `Service_VisualAutomation_ProbeRunner`,
the view, the view model, or the dialogs. Those need a live VISUAL window.

## A clean probe run looks like this

- Target line: `Purchase Receipt Entry - Infor VISUAL - MTMFG  |  PID nnnn  |  N elements  |  (x,y) w x h`.
- Snapshot line: order id, receiver id, `16 of 16 mapped variables readable`, `Save control present: yes`.
- Ten cards, each with a green **Pass** pill.
- Instruction text: `Run finished - 10 passed, 0 failed. Nothing was saved; the VISUAL Save button was never pressed.`
- Details dialog: one block per probe with a verdict and captured values; the log line count is shown at
  the bottom.

## What P09_NoSaveGuard proves

- It reports `SAVE_ACTIONS_PERFORMED = 0`, `SAVE_CONTROL_PRESENT = yes|no`, and the receipt identity
  (`ORDER_ID|SITE_ID|RECEIVER_ID`) before and after two snapshots.
- It passes when those two identity strings are identical.
- It is a guard, not a measurement: the "0" is a literal in the code, and the real evidence is that no
  save path exists in the module plus the unchanged identity. `HasSaveControl` only looks for an element
  named `Save` or `Save transaction`.

## What P10_RestoreIntegrity proves

- Compares every field that had a control in the baseline snapshot with a fresh snapshot taken now,
  using trimmed string comparison, and reports any field whose control disappeared.
- The baseline is captured by `RunAllAsync` immediately before the first selected probe, so P10's
  strength depends on the write probes having run in the same run. Run P10 alone and its baseline is only
  moments older than its comparison.

## Health signals outside the page

- `dotnet build MTM_Receiving_Application.slnx` succeeds with no new warnings from the module.
- The application log contains `Typed '...' into VISUAL field KEY (was '...'); read back '...', restore
  verified|FAILED|not requested.` for every write.
- Log lines carry the context `ViewModel_VisualAutomation_MacroTester`,
  `Service_VisualAutomation_WindowDriver`, or `Service_VisualAutomation_ProbeRunner`.

## Quick manual smoke test without a receipt loaded

Two expectations follow from the code paths; both are unverified against a live empty form and should be
confirmed before you rely on them.

- P01 and P02 depend only on the window being readable, so they should still pass, with a lower element
  count than a loaded receipt.
- P03 and P04 fail unless every required key has a control and a non-empty value (P03: `ORDER_ID`,
  `SITE_ID`; P04: `DESIRED_RECV_DATE`, `BUYER`), so they should fail on an empty form with
  `These variables had no value on screen: ...`.

P05, P07, and P08 depend on whether the reference boxes resolve on an empty form; if they do not, the
failure is `These reference fields have no control on the window: ...` or
`Variable 'KEY' has no control on the visible window.`.
