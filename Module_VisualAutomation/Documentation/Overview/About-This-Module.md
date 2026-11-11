# About This Module

Last Updated: 2026-11-11

`Module_VisualAutomation` is a proof-of-capability harness. It proves that this WinUI 3 application can
read and change fields on the **live Infor VISUAL Purchase Receipt Entry window** (`VMRCVENT.EXE`) the
same way a VISUAL macro would, **without deploying or running any macro**.

It is not part of the receiving workflow. Nothing in the module creates, posts, or saves a receipt.

## What it talks to

| Item | Value | Source |
| ---- | ----- | ------ |
| External program | `VMRCVENT.EXE` | `Service_VisualAutomation_WindowDriver.ModuleProcessName` |
| Window title must contain | `Purchase Receipt` | `ExpectedTitleFragment` |
| Reads | Windows UI Automation (`System.Windows.Automation`) | `Service_VisualAutomation_WindowDriver` |
| Writes | Injected mouse and keyboard via `SendInput` | `Helper_VisualAutomation_NativeInput` |
| Databases | none | no `Data/` folder, no DAO in the module |

The VISUAL field boxes are Gupta panes: they expose no UI Automation patterns and cannot take keyboard
focus. That is why reads are positional (label, then the value box on the same row) and writes are
"click, select all, type, Tab, read back".

## What it explicitly does not do

- It never saves. No code clicks a Save control and no save key sequence is sent anywhere in the module.
- It never writes to any database. There is no DAO, no SQL, and no connection string in the module.
- It never writes to Infor Visual SQL Server. The MTMFG table and column names in
  `Helper_VisualAutomation_FieldMap` are reference metadata only.
- It never deploys, edits, or executes a `.VMS` macro file.

## Where you find it in the app

- Navigation item: **Visual Macro Tester** (`MainWindow.xaml`, `Tag="VisualMacroTester"`).
- Route table entry: `MainWindow.xaml.cs` maps `VisualMacroTester` to
  `Module_VisualAutomation.Views.View_VisualAutomation_MacroTester`.
- Page title: `Visual Macro Tester`, set by the view model.
- DI: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` -> `AddVisualAutomationModule()`.

## Size and shape

23 `.cs` files and 3 `.xaml` files. Ten probes. One page, two dialogs, two services, two helpers.
The layering is View -> ViewModel -> Service -> external process; the "data access" step is replaced by
UI Automation against another application.

## Start here, in this order

1. [How It Works at a Glance](./How-It-Works-at-a-Glance.md) - the runtime path and the class at each step.
2. [Daily Tasks](../How-To-Guides/Daily-Tasks.md) - the five changes you will actually make.
3. `Probes/VisualProbeCatalog.cs` and `Helpers/Helper_VisualAutomation_FieldMap.cs` - the two data files
   that drive everything else.
4. [Decisions](../Changes-and-Decisions/Decisions.md) - why the design is shaped this way.
5. [Guardrails](../AI-Handoff/Guardrails.md) - the rules that must not be broken.

## Deeper background outside this folder

- [InforVisual-Macro-Automation.md](../../../docs/development/InforVisual-Macro-Automation.md) - how a
  field is located, read, and written; why macros were rejected; safety rules.
- [InforVisual-Macro-Variable-Reference.md](../../../docs/development/InforVisual-Macro-Variable-Reference.md) -
  the evidence mapping every macro variable to an MTMFG table and column.
- [UI_Interaction_Guide.md](../../../docs/development/InforVisual/Interaction_Guides/UI_Interaction_Guide.md) -
  the original VISUAL discovery session: process names, window titles, window class, install path.

## One honest limitation to know up front

A run is blocked unless the three field values (`BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID`) are filled
in, even when the selected probes only read the window. See
[How It Works at a Glance](./How-It-Works-at-a-Glance.md).
