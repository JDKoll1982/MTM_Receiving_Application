# Decisions

Last Updated: 2026-11-11

These decisions are already implemented. Rationale comes from the XML doc comments and inline comments in
the module, not from meeting notes.

## 1. Do not deploy or run a VISUAL macro

`IService_VisualAutomation_WindowDriver` is documented as reading and driving the window
"the way a macro would, without deploying or running any macro script". The page doc comment repeats it:
"Despite the page name, no macro is deployed or executed".

Reason recorded in [InforVisual-Macro-Automation.md](../../../docs/development/InforVisual-Macro-Automation.md):
a deployed macro lives on the shared VISUAL folder and is live for every user of the share.

## 2. Read with UI Automation

`Service_VisualAutomation_WindowDriver` walks the automation tree and matches a label to the value control
on the same row. This is the only read channel: nothing reads MTMFG at run time.

## 3. Write with real mouse and keyboard

Comment in `Helper_VisualAutomation_NativeInput`: the field controls "are Gupta panes that expose no UI
Automation patterns and are not keyboard focusable, so the only way to change a field is to click it and
type". Writes therefore use `SetCursorPos` plus `SendInput`.

Consequence the code enforces: the driver refuses to type unless VISUAL is the foreground window, and
re-checks focus after activating.

## 4. Save is never performed

Interface remarks: "Saving is never performed - no toolbar button is clicked and no save key sequence is
sent." No code path in the module clicks a toolbar control or sends `Ctrl+S`, `F9`, or any other save
sequence. The only key events the helper can send are `Ctrl+A`, Delete, Tab, and the characters of a value.

## 5. Only three fields may be written

`Helper_VisualAutomation_FieldMap._writable` holds `BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID`. Inline
comment: "Only free-text and non-validating fields are included so a probe can never disturb dates,
quantities, or the posting state."

`IsSafeToWrite` returns true only for `Enum_VisualVariableSafety.ReceiptOnly`, whose comment calls it the
"safest surface for automation". A unit test asserts the writable set never widens past that class.

## 6. Typed values are always restored

`WriteValueAsync` takes `restoreOriginal` and both write probes pass `true`. `Model_VisualProbeInputs` is
documented as: each value "is typed into a reference field, read back, and restored, so a blank or wrong
value cannot leave a lasting change on the receipt". The restore is verified by reading the field again.

## 7. The probe catalogue is data, not logic

`VisualProbeCatalog` holds one `Model_VisualProbeDefinition` per probe with its purpose, on-screen effect,
and expected result. The page renders cards straight from it, and the runner dispatches by id. Adding a
probe is a catalogue entry plus a `switch` arm, not a new class per probe.

## 8. The safety model is an explicit enum, not a boolean

`Enum_VisualVariableSafety` separates `Identity`, `Runtime`, `ReceiptOnly`, `Consequential`, `Scratch`, and
`MacroOutput`, so the reason a field is unwritable is recorded per variable and enforced per variable.

## 9. Conventions followed without a recorded rationale

These match repository rules rather than module-specific reasoning; the rationale is not written down
anywhere in the module.

- Expected failures return `Model_Dao_Result` (and its generic form) even though there is no database,
  because that is the repo's result type for expected failures.
- The view is `x:Bind` only; code-behind holds dialog plumbing, not logic.
- Services are singletons, the view model and view are transient, per
  `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.
