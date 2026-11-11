# Editing Brief

Last Updated: 2026-11-11

Read this before you let an agent (or yourself) change anything here. It assumes you have read
[About This Module](../Overview/About-This-Module.md).

## Read order

1. `Contracts/IService_VisualAutomation_WindowDriver.cs` - the five operations the harness can perform.
2. `Contracts/IService_VisualAutomation_ProbeRunner.cs` - run all, or run one.
3. `Probes/VisualProbeCatalog.cs` - the ten probes and their promised behaviour.
4. `Helpers/Helper_VisualAutomation_FieldMap.cs` - labels, bindings, writable set.
5. `Services/Service_VisualAutomation_WindowDriver.cs` - discovery, snapshot, resolution, write.
6. `Services/Service_VisualAutomation_ProbeRunner.cs` - dispatch and verdicts.
7. `ViewModels/ViewModel_VisualAutomation_MacroTester.cs` - gate, orchestration, log.
8. `Views/View_VisualAutomation_MacroTester.xaml` and `.xaml.cs` - presentation only.

## Who owns what

| Concern | File |
| ------- | ---- |
| Probe list, ids, wording | `Probes/VisualProbeCatalog.cs` |
| Verdict rules | `Services/Service_VisualAutomation_ProbeRunner.cs` |
| Window discovery, element matching, typing | `Services/Service_VisualAutomation_WindowDriver.cs` |
| Field names, labels, bindings, writability | `Helpers/Helper_VisualAutomation_FieldMap.cs` |
| Raw input injection | `Helpers/Helper_VisualAutomation_NativeInput.cs` |
| Run gating, pass/fail counts, log | `ViewModels/ViewModel_VisualAutomation_MacroTester.cs` |
| Card status pill and text | `Models/Model_VisualProbeCard.cs`, `Converters/Converter_VisualMacroStatusToBrush.cs` |
| Dialogs | `Dialogs/Dialog_VisualProbeInputs.*`, `Dialogs/Dialog_VisualProbeDetails.*` |

Do not put a rule in two places. If a verdict needs a new rule, it belongs in the runner; if a label or
field changes, it belongs in the field map.

## Never touch

- No save path, ever: no toolbar click, no `Ctrl+S`, `F9`, or any save key sequence.
- No database access: no DAO, no SQL, no connection string, no MTMFG read or write.
- No `.VMS` macro file creation, deployment, or execution.
- Nothing outside `Module_VisualAutomation/`, except tests under
  `MTM_Receiving_Application.Tests/Unit/Module_VisualAutomation/`.
- Do not widen the writable set without checking `Enum_VisualVariableSafety`.
- No runtime `{Binding}` in XAML; no business logic in `.xaml.cs`.
- Do not change DI registration, base classes, or add packages without approval.

## How to validate a change

1. `dotnet build MTM_Receiving_Application.slnx`.
2. `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Module_VisualAutomation"`.
3. A live run on a loaded receipt: open the page, press **Check VISUAL window**, fill the three values,
   select all cards, press **Run selected probes**.
4. Open the details dialog and confirm the affected probes report what you expect. Quote the verdict text
   in your change description.

A change that cannot be exercised without VISUAL must still build and keep the unit tests green.

## Conventions already in force

- Async methods end in `Async`; UI-updating awaits use `ConfigureAwait(true)`.
- Expected failures return `Model_Dao_Result` rather than throwing; the runner converts unexpected
  exceptions into a failing result and logs them.
- Public members carry XML doc comments; keep that true for anything you add.
- Tests are xUnit plus FluentAssertions, named `MethodName_ShouldResult_WhenCondition`.
- Every public-facing string you add is a support surface: someone will read it in a verdict or log line.
  Write it as a sentence a shop-floor tester can act on.

## Traps

- `CaptureSnapshot` calls `DiscoverWindow` internally, and again inside `HasSaveControl`, so one snapshot
  is several window reads. Do not call it in a tight loop.
- Card `RunCommand` is assigned by the view model after construction so the item template can keep
  compiled bindings. A card created without that assignment has no working Run button.
- `Inputs` on the view model is a plain property, not observable; the summary line is refreshed by calling
  `RefreshInputsSummary()`.
- The runner's baseline snapshot is a field, so P10 depends on which probes ran in the same run.
