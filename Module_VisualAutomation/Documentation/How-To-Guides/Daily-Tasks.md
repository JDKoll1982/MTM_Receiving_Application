# Daily Tasks

Last Updated: 2026-11-11

Five things you will actually do in this module. All file paths are relative to `Module_VisualAutomation/`.

## 1. Add a probe to the catalogue

1. Add a constant to `Probes/VisualProbeCatalog.cs`, for example
   `public const string MyProbe = "P11_MyProbe";`.
2. Add a `Model_VisualProbeDefinition` to `_allProbes` with `Id`, `Title`, `Category`, `Purpose`,
   `UiSteps`, `Expected`, and, when it types into the window, `WritesToWindow = true` and
   `ReportedKeys`.
3. Add a `case` arm to the `switch` in `Service_VisualAutomation_ProbeRunner.RunAsync`. Without it the
   probe returns `This probe has no implementation.` at run time.
4. Update `Helper_VisualAutomation_FieldMapTests.Probes_ShouldHaveUniqueIdentifiers_WhenTheCatalogueIsRead`,
   which asserts `HaveCount(10)`.
5. Run the tests, then a live run of the new probe on its own.

Rules the tests enforce: ids are unique; every `ReportedKeys` entry must exist in
`Helper_VisualAutomation_FieldMap.MacroVariables` (except `POSTING_STATE_BEFORE` and
`POSTING_STATE_AFTER`); a probe with `WritesToWindow = true` must have a non-empty `UiSteps` and may only
report writable keys.

## 2. Add a macro variable to the field map

1. Add the name to `_macroVariables` in `Helpers/Helper_VisualAutomation_FieldMap.cs`, alphabetical.
2. If the window paints a caption for it, add `["KEY"] = "On-screen label"` to `_labels`.
3. Add a binding with the local `Add(...)` helper: key, meaning, MTMFG table, column, type, and one
   `Enum_VisualVariableSafety` value. Every macro variable must have a binding or
   `Helper_VisualAutomation_VariableBindingTests` fails.
4. Update the counts that are asserted: 87 in two tests, and 45 udf slots if you add a `udf*` variable.

Meaning and backing column are research, not guesswork. Check
[InforVisual-Macro-Variable-Reference.md](../../../docs/development/InforVisual-Macro-Variable-Reference.md).

## 3. Add or change a writable field

- Add the key to the `_writable` set in `Helper_VisualAutomation_FieldMap`.
- Its binding must be `Enum_VisualVariableSafety.ReceiptOnly`; a test asserts the writable set never
  exceeds that class, and `IsSafeToWrite` is true only for `ReceiptOnly`.
- It must have an entry in `_labels`; a test asserts every writable field has a visible label.
- A write probe that targets it must be in `VisualProbeCatalog` with `WritesToWindow = true`.
- `Service_VisualAutomation_WindowDriver.WriteValueAsync` refuses any key outside `IsWritable` before it
  touches the window, so the field map is the only place to widen the set.

Do not widen this set to a `Consequential` field. See [Guardrails](../AI-Handoff/Guardrails.md).

## 4. Run the page against a live VISUAL window

1. `dotnet build MTM_Receiving_Application.slnx`, then start the app.
2. In Infor VISUAL open Inventory > Purchase Receipt Entry and load a PO.
3. In the app open **Visual Macro Tester**, press **Check VISUAL window**. The target line must show a
   title, PID, element count, and rectangle.
4. Press the pencil icon and fill in `BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID`. All three are required
   before any run, because the gate applies to every run.
5. Select cards and press **Run selected probes**, or expand one card and press **Run this probe**.
6. A write probe steals focus for a moment. Do not click or type until the run finishes.

## 5. Run the unit tests

```powershell
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Module_VisualAutomation"
```

The three test classes need no VISUAL window and no database: they cover the field map, the bindings, and
the inputs model only. Runner and driver have no tests.
