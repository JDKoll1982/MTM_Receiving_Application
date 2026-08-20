# Scanner Engine Handoff — 2026-08-20

Session summary for continuing Module_Scanner work on a new machine.

## Branch
`InventoryAssistantFeature` (working tree had uncommitted scanner-engine changes).

## What was done this session
Implemented the **Native Input Engine + Global Hotkeys + Batch Orchestration** phase of
Module_Scanner (per `specs/ScannerFeature/MainPM.md`). Prior phases (models, DAOs, persistence,
validation, workbench UI) were already done.

Key user decisions: new `IService_ScannerExecution` contract (NOT extending workflow);
direct Win32 `SendInput` (no NuGet dep); Scanner nav item enabled only for developers.

## New files (root: `Module_Scanner/`)
- `Contracts/IService_ScannerInputEngine.cs`, `Services/Service_ScannerInputEngine.cs` — SendInput,
  KEYEVENTF_UNICODE text, nav keys, chord, foreground helpers.
- `Contracts/IService_ScannerHotkey.cs`, `Services/Service_ScannerHotkey.cs` — RegisterHotKey +
  WM_HOTKEY via SetWindowSubclass; default chords Ctrl+Alt+M (send) / Ctrl+Alt+N (stop);
  public `TryParseChord` (requires ≥1 modifier).
- `Contracts/IService_ScannerExecution.cs`, `Services/Service_ScannerExecution.cs` —
  `SendNextItemAsync`/`SendAllAsync`/`SendSpecificItemAsync`/`RequestStopAsync`; verify foreground
  by process name; field order Part→FromWH→FromLoc→ToWH→ToLoc→Qty (Tab between, from
  `Transactions.vb` transfer contract); stop-on-first-failure; stop between cycles; persistence off hot path.
- `Models/Model_ScannerExecutionOutcome.cs`
- `Helpers/Helper_ScannerSequence.cs` (field order, eligibility, process/title match)
- `Helpers/Helper_ScannerAccess.cs` (dev gate: FullName "John Koll" | usernames jkoll/johnk | Department "Developer")

## Modified files
- `Module_Scanner/ViewModels/ViewModel_Scanner_Workbench.cs` — Send/SendAll/Stop now call execution
  service; `ResolveActiveProfileAsync` (default profile, null-safe fallback); hotkey subscribe via
  `Activate()`/`Deactivate()`; `RefreshSessionAfterExecution`.
- `Module_Scanner/Views/View_Scanner_Workbench.xaml.cs` — Loaded→`ViewModel.Activate()`, Unloaded→`Deactivate()`.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — registered the 3 new services (singletons).
- `MainWindow.xaml.cs` — `ApplyScannerNavigationGate()` (dev-only enable of Scanner item) +
  `InitializeScannerHotkeys()` (register on window, unregister on Closed).
- Tests: `Service_ScannerHotkeyTests`, `Helper_ScannerSequenceTests`, `Helper_ScannerAccessTests`,
  `Service_ScannerExecutionTests` (DB-free paths), updated `ServiceCollection_ScannerWiringTests` +
  `ViewModel_Scanner_WorkbenchHistoryTests`.

## Validation
- `dotnet build MTM_Receiving_Application.csproj -c Debug -p:Platform=x64 -p:TargetFramework=net10.0-windows10.0.22621.0` → succeeds.
- `dotnet test` filter `FullyQualifiedName~Module_Scanner` → **91/91 pass**.
- Full suite: 9 pre-existing failures in OTHER modules (Dunnage WinRT, Reporting, InforVisual, casing, CopilotForms) — unrelated.

## Next-phase candidates (do these next)
1. **Configurable hotkey chords per profile** — read `Model_ScannerProfile.SendShortcutChord`/`StopShortcutChord`
   in Settings; re-register hotkeys when profile changes (currently hardcoded in `MainWindow.InitializeScannerHotkeys`).
2. **Populate `SessionItemId`** — make `sp_Receiving_ScannerItem_Upsert` return the inserted id so run-history
   recording works (currently best-effort/skipped when null).
3. **Enable Scanner nav for all users** after verifying against real VMINVENT (currently dev-gated).
4. Optional per-item validation-on-send.

## Tooling notes (this machine)
- Use the 3 MCP servers: **Serena** (symbol-level C#), **Microsoft Learn MCP** (Win32/WinUI docs),
  **Context7** (library docs).
- Input engine uses direct P/Invoke; hotkeys use `WindowNative.GetWindowHandle` on the WinUI 3 window.
- Full implementation details: `/memories/session/scanner-engine-phase.md`.
