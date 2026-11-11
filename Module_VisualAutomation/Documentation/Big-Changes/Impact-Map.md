# Impact Map

Last Updated: 2026-11-11

What breaks, or must be updated, when you change one of the three load-bearing pieces. Every reference
below was traced in source.

## A. Probe contract (`Model_VisualProbeDefinition`, `VisualProbeCatalog`)

| Kind | Location |
| ---- | -------- |
| Definition | `Models/Model_VisualProbeDefinition.cs` |
| Catalogue | `Probes/VisualProbeCatalog.cs` (`_allProbes`, `All`, `Find`) |
| Dispatch | `Services/Service_VisualAutomation_ProbeRunner.cs`, `RunAsync` switch on `probe.Id` |
| Card projection | `Models/Model_VisualProbeCard.cs` (reads `Definition` properties, `WritesToWindow`) |
| Page | `ViewModels/ViewModel_VisualAutomation_MacroTester.cs` builds `Probes` from `All`; `Views/View_VisualAutomation_MacroTester.xaml` binds card properties |
| Details modal | `Model_VisualProbeDetail`, `Dialog_VisualProbeDetails`, `BuildDetails()` |
| Tests | `Helper_VisualAutomation_FieldMapTests`: unique ids, `HaveCount(10)`, reported keys must exist, writers may only report writable keys |

Adding a property to the definition also means deciding whether the card exposes it and whether the XAML
shows it. Renaming an id breaks the switch arm and any saved expectations that quote verdicts.

## B. Field map (`Helper_VisualAutomation_FieldMap`)

| Kind | Location |
| ---- | -------- |
| Source | `Helpers/Helper_VisualAutomation_FieldMap.cs` (`_labels`, `_writable`, `_macroVariables`, `CreateBindings`) |
| Reads | `Service_VisualAutomation_WindowDriver.CaptureSnapshot` iterates `VisibleVariables`; `ResolveValueControls` calls `KeyForLabel` |
| Writes | `Service_VisualAutomation_WindowDriver.WriteValueAsync` guards with `IsWritable` |
| Reporting | `Service_VisualAutomation_ProbeRunner`: `RunVariableCoverage` uses `MacroVariables` and `VisibleVariables` |
| Page | `RefreshSnapshotSummary` reads `ORDER_ID` and `RECEIVER_ID` from the snapshot |
| Tests | `Helper_VisualAutomation_FieldMapTests` (87 variables, labels, writability, normalisation) and `Helper_VisualAutomation_VariableBindingTests` (87 bindings, 45 udf slots) |

Anything that changes the count 87, the count 45, or a label string fails a test by design.

## C. Inputs gate (`Model_VisualProbeInputs` and `RequiresInputs`)

| Kind | Location |
| ---- | -------- |
| Model | `Models/Model_VisualProbeInputs.cs` (`IsComplete`, `MissingFields`, `DescribeMissing`, `ToWriteValues`, `Clone`) |
| Gate | `ViewModel_VisualAutomation_MacroTester.RunProbesAsync` -> `RequiresInputs()` before window discovery |
| Dialog | `Dialogs/Dialog_VisualProbeInputs.xaml` and `.xaml.cs`; primary button disabled until complete |
| View wiring | `Views/View_VisualAutomation_MacroTester.xaml.cs`: `ProbeInputsRequested` subscription, `ShowInputsDialogAsync`, `RefreshInputsSummary` |
| Consumers | `Service_VisualAutomation_ProbeRunner.RunFieldWriteAsync` reads `inputs.BolId`, `inputs.VendPackId`, `inputs.VendFreightId` |
| Tests | `Model_VisualProbeInputsTests` |

Note: `ToWriteValues()` is used only by tests. If you change how inputs reach the runner, update the tests
and either wire that method in or remove it.

## D. Interfaces and wiring

- `Contracts/IService_VisualAutomation_WindowDriver.cs` and
  `Contracts/IService_VisualAutomation_ProbeRunner.cs`.
- Implementations: `Services/Service_VisualAutomation_WindowDriver.cs`,
  `Services/Service_VisualAutomation_ProbeRunner.cs`.
- Registration: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`, inside
  `AddVisualAutomationModule()`; called from the aggregate registration method at line 69.
- Construction: `View_VisualAutomation_MacroTester` resolves the view model with `App.GetService`.
- Navigation: `MainWindow.xaml` (`Tag="VisualMacroTester"`) and the route table in `MainWindow.xaml.cs`.

Changing a constructor signature ripples into DI only; changing a method signature ripples into the view
model and, for the driver, into every probe implementation in the runner.
