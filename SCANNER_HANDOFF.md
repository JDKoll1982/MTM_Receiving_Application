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
  WM_HOTKEY via SetWindowSubclass; default send chord Ctrl+Alt+M (stop hotkey removed 2026-08-24);
  public `TryParseChord` (requires ≥1 modifier).
- `Contracts/IService_ScannerExecution.cs`, `Services/Service_ScannerExecution.cs` —
  `SendNextItemAsync`/`SendSpecificItemAsync`/`ClearTargetFormAsync` (Send All and Stop After This removed 2026-08-24);
  verify foreground by process name; send field order Part→Qty→FromWH→FromLoc→ToWH→ToLoc with tab counts
  1→2→1→5→1 (matching the visible VMINVENT "Inventory Transfers" screen tab path — see
  `Helper_ScannerSequence.BuildFieldSequence`);
  stop-on-first-failure; stop between cycles; persistence off hot path.
- `Models/Model_ScannerExecutionOutcome.cs`
- `Helpers/Helper_ScannerSequence.cs` (field order, eligibility, process/title match)
- `Helpers/Helper_ScannerAccess.cs` (dev gate: FullName "John Koll" | usernames jkoll/johnk | Department "Developer")

## Modified files
- `Module_Scanner/ViewModels/ViewModel_Scanner_Workbench.cs` — Send now calls execution
  service; `ResolveActiveProfileAsync` (default profile, null-safe fallback); hotkey subscribe via
  `Activate()`/`Deactivate()`; `RefreshSessionAfterExecution` (Send All / Stop After This removed 2026-08-24).
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
1. **Configurable send hotkey chord per profile** — read `Model_ScannerProfile.SendShortcutChord`
   in Settings; re-register hotkeys when profile changes (currently hardcoded in `MainWindow.InitializeScannerHotkeys`).
2. **Populate `SessionItemId`** — make `sp_Receiving_ScannerItem_Upsert` return the inserted id so run-history
   recording works (currently best-effort/skipped when null).
3. **Enable Scanner nav for all users** after verifying against real VMINVENT (currently dev-gated).
4. Optional per-item validation-on-send.

## From-location inventory picker (2026-08-20)
- From-location lost-focus workflow (in order):
  1. **Autocomplete** — applies the shared location dash-formatting rule
     (`Strategy_SharedLocationLookup.ApplyFormatting`, e.g. `VA101` → `V-A1-01`, `R5` → `R-05`)
     and writes the canonical value back into the field. Exposed as
     `IService_ScannerValidation.FormatLocation` → `ViewModel_Scanner_Workbench.FormatLocation`.
     Applied to both the From and To location fields (the To field goes through the same
     shared `HandleLocationTextBoxLostFocusAsync`).
  2. **Existence check** — `ValidateFromLocationAsync`.
  3. **Not found** → standard location fuzzy picker (like the To field); on selection it writes
     back and then validates the quantity.
  4. **Quantity validation** — if the resolved source can't fulfill the requested qty, the
     inventory picker (locations with qty > 0) opens so the operator can pick a source with stock.
- **Canonical vs display location (2026-08-20 fix)** — the dash formatting (e.g. `VA101` →
  `V-A1-01`) is a display-only convenience; Infor Visual stores locations WITHOUT dashes
  (`VA101`). `ValidateFromLocationAsync` already resolves and stores the canonical DB form in
  `NewFromLocation` (verified by
  `ValidateNewItemAsync_ShouldResolveLocationsWithoutDashes_WhenCanonicalVisualIdsExist`). The
  quantity/stock check in the view must therefore match against the **canonical** value
  (`ViewModel.NewFromLocation` after validation), NOT the pre-validation display-formatted value
  (`V-A1-01`) — matching the display form yields a false "No stock was found" and leaves Qty
  disabled. Guardrail test:
  `ValidateFromLocationAsync_ShouldStoreCanonicalLocation_WhenResolved`.
- The Add flow also triggers the picker: `AddDraftItemAsync` raises `FromLocationInventoryPickerRequested`
  (ViewModel → view) on source issues, updates `NewFromLocation`, and re-validates before staging.
- Backed by `IService_ScannerValidation.GetLocationsWithStockAsync` →
  `IService_InforVisual.GetMaterialAvailabilityCurrentStockAsync(null, partId, warehouse)`, exposed on the
  workbench ViewModel as `GetFromInventoryLocationsAsync`. To location keeps fuzzy search.
  If the part has no on-hand stock anywhere, the workbench shows a "no stock found" message instead of
  the fuzzy picker.

## Send confirmation + quantity gating (2026-08-20)

Four-part change on top of the inventory picker:

1. **LostFocus fix** — Part/From/To/Qty bindings now use `UpdateSourceTrigger=PropertyChanged` and each
   LostFocus handler syncs the ViewModel from `textBox.Text` first, so validation runs on the first
   focus-out instead of requiring exit/re-enter/leave.
2. **Quantity gating** — the Qty box is disabled (`IsQuantityEnabled`, bound to `IsEnabled`) until the
   From location is validated and resolves to a source with stock. `ValidateFromQuantityAsync` then sets
   `SetFromQuantityLimit(source.Quantity)` (max on-hand). `BeforeTextChanging` blocks any entry above that
   max (also clamps pastes). Clearing/invalidating From calls `ClearFromQuantityLimit()` (Qty re-disables).
3. **Send tab map** — `Helper_ScannerSequence.BuildFieldSequence` returns
   `[(PartId,1),(Qty,2),(FromWH,1),(FromLoc,5),(ToWH,1),(ToLoc,0)]`; `EmitFieldSequenceAsync` types each
   non-empty value then presses Tab `n` times (Alt+L to clear the VMINVENT form via
   `IService_ScannerExecution.ClearTargetFormAsync` = `SendChord(Alt, VK_L)`).
4. **Saved? confirmation** — after a line is emitted, Send Next/Send All are disabled and a
   `Saved? [Yes] [No]` prompt appears (bound to `IsSendPromptVisible`, hidden via
   `Converter_BooleanToVisibility`):
   - **Yes** → `ConfirmSavedCommand` removes the line from the batch, renumbers the rest, persists via
     `ReplaceSessionItemsAsync`, shows "Line N saved to history", and (in Send All) continues with the
     next eligible line.
   - **No** → `ConfirmNotSavedCommand` resets the item to `Waiting`, sends Alt+L to clear VMINVENT,
     raises `PartIdFocusRequested` (view focuses the Part box), and shows "Form cleared. Re-enter the
     part and try again.". Manual save only — no autosave.

Tests updated/added: `BuildFieldValues` (new 6-value order), `BuildFieldSequence` tab map,
`ClearTargetFormAsync` (Alt+L success/failure), Saved? prompt + Yes/No confirmation flow, and
quantity-limit enable/disable, plus the canonical-location guardrail. Module_Scanner suite now **114/114 pass**.

## Tooling notes (this machine)
- Use the 3 MCP servers: **Serena** (symbol-level C#), **Microsoft Learn MCP** (Win32/WinUI docs),
  **Context7** (library docs).
- Input engine uses direct P/Invoke; hotkeys use `WindowNative.GetWindowHandle` on the WinUI 3 window.
- Full implementation details: `/memories/session/scanner-engine-phase.md`.
