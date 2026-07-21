# High-Speed Centralized Batch Scanner Emulation Module

Technical project memorandum for the MTM Receiving Application.

- To: Core Development Team
- From: Lead Solutions Architect
- Date: 2026-07-21
- Status: Approved for Architecture Alignment

## Executive Summary And Core Objective

The target ERP terminal, VMINVEN, can desynchronize when automation sends values through slow,
character-at-a-time loops. The objective of this feature is to stage transfer records inside the
MTM Receiving Application and emit record-level input sequences with scanner-like speed while
still respecting the target application's pacing and Windows desktop focus rules.

In this repository, that capability must fit the existing WinUI 3 desktop architecture rather than
introducing a parallel framework stack. The feature should remain inside the established MVVM flow:
View to ViewModel to Service to DAO to Database. UI concerns stay in views and view-models,
business orchestration stays in services, shared persistence stays in DAOs, and any native window or
input interop stays behind service abstractions.

The feature may persist batch drafts, execution settings, and run history in the shared MySQL
application database, but only through the repository's existing DAO and stored-procedure patterns.
It must not assume Entity Framework Core, ad hoc DbContext usage, or direct database writes from
view-models.

## Repository Alignment

The scanner memorandum is constrained by the current project architecture and existing platform
patterns.

- WinUI 3 windowing and interop are HWND-based. Window handle access should align with the
  Windows App SDK interop pattern already used throughout the project.
- UI-thread updates should align with the existing DispatcherQueue-based approach, ideally through
  `IService_Dispatcher` or another shared abstraction instead of view-local threading logic.
- Shared native automation belongs in `Module_Core` as reusable service contracts and
  implementations.
- Feature-specific sequence assembly, validation, and batch execution orchestration belong in the
  owning functional module, not in generic core helpers.
- MySQL application writes must use stored procedures and return `Model_Dao_Result` or
  `Model_Dao_Result<T>`.
- Infor Visual SQL Server remains read-only and can only be used for supporting lookups or
  validation. It is not a persistence target for this feature.

## Structural Module Breakdown

## Domain Model Layer

The domain layer should stay as plain C# models that describe batch state without embedding UI,
database, or Win32 behavior.

- Batch Session Model: tracks batch identity, operator context, creation timestamps, execution
  status, and ordered child items.
- Batch Item Model: stores one record's field payload, sequence order, target-field navigation
  semantics, validation state, execution outcome, and retry metadata.
- Automation Profile Model: stores execution settings such as target window identity, per-record
  settle delay, send-shortcut behavior, and operator safety flags.

These models should remain serializable and persistence-friendly, but they should not assume a
specific ORM or direct relationship-tracking framework.

## Application And Service Layer

The application layer should be split between orchestration services and reusable native
automation services.

- Batch Orchestration Service: validates staged items, manages execution order, coordinates focus
  acquisition, calls the native input service, and records results.
- Native Input Service: encapsulates Win32 input emission and window targeting details. If the
  existing `IService_UIAutomation` contract is extended, it should remain generic. If Visual- or
  scanner-specific behavior becomes substantial, it should move into a dedicated service rather
  than overloading the generic automation abstraction.
- Settings Service Integration: user-scoped or shared defaults should flow through the existing
  settings subsystem patterns instead of a standalone settings store.
- Logging And Error Handling: expected failures should surface through the repository's existing
  result and error-handling patterns rather than raw exceptions escaping into the UI.

## Persistence Layer

Persistence for scanner drafts, saved batches, execution profiles, or audit history must align with
 the repository's current MySQL infrastructure.

- Use DAO classes for persistence operations.
- Use stored procedures for all MySQL writes.
- Use `Model_Dao_Result` and `Model_Dao_Result<T>` for expected failure paths.
- Reuse the repository's retry and command-execution patterns, such as
  `Helper_Database_StoredProcedure`, instead of introducing Entity Framework Core.
- Keep persistence asynchronous and outside the active input emission path so database latency does
  not stall the automation stream.

If a future phase needs relational tables for batch sessions and items, that schema should be added
through the repository's normal database deployment workflow rather than implied here as an ORM
first design.

## Presentation And ViewModel Layer

The feature should use standard MTM WinUI 3 presentation patterns.

- Views should use `x:Bind` and stay focused on layout, status display, and operator interaction.
- View-models should expose batch composition, validation, execution commands, progress state, and
  result summaries.
- Long-running execution state should be marshaled back to the UI through DispatcherQueue-friendly
  abstractions already present in the project.
- Business logic, native input composition, and persistence rules should not move into XAML
  code-behind.

## Native Engine Integration

The automation engine should be described in terms that match current Windows platform behavior.

- Input Emission: the engine should construct ordered arrays of Win32 `INPUT` events and inject
  them through `SendInput`. The platform guarantees that those injected events are inserted
  serially into the input stream, but this is still event-based input injection, not a direct raw
  string drop into an operating-system ring buffer.
- Foreground Window Targeting: the engine should resolve and verify the target window handle before
  execution. The operator should first bring the intended Inventory window to the foreground, then
  press the global scanner shortcut. Window acquisition and ownership should align with HWND-based
  WinUI 3 interop.
- Focus Settlement: execution should include configurable settle delays after window activation and
  between record submissions because the target ERP may validate or repaint more slowly than input
  can be injected.
- Navigation Semantics: field movement should be modeled as explicit key events such as Tab, Enter,
  or other required control keys instead of assuming direct control-level bindings inside the ERP.
- Commit Behavior: automatic save or finalize shortcuts, including `Alt+S`, are out of scope for
  this workflow. A record or staged send cycle is advanced only when the user presses the scanner
  send shortcut again.

The engine must also acknowledge standard Windows limitations.

- `SendInput` is subject to desktop integrity rules and can fail when the target application is at
  a higher integrity level.
- Existing pressed keys can interfere with injected input unless keyboard state is checked and the
  execution path is controlled carefully.
- Fast input injection does not remove the need for application-aware pacing.

## Global Shortcut Integration

Global batch-start or batch-cancel shortcuts should be described with the standard Win32 hotkey
model rather than a vague subclass-only design.

- Register the hotkey against the application's window or owning thread using the normal
  `RegisterHotKey` pattern.
- Handle the resulting `WM_HOTKEY` message on the app side and route it into the appropriate
  command or service entry point.
- Use `Ctrl+Alt+M` as the operator start or send shortcut and `Ctrl+Alt+N` as the stop-processing
  shortcut unless a later settings phase makes the bindings configurable.
- Treat the shortcut as an operator-driven send action after the Inventory window is already in the
  foreground, not as a background auto-targeting trigger.
- Unregister hotkeys during shutdown or module teardown.
- Keep hotkey registration isolated from business execution logic so that testing and alternate
  invocation paths remain straightforward.

If message-hooking or subclassing is later required for a specific implementation detail, that
should be documented as an implementation choice, not the baseline architecture.

## Operator Safety And Input Isolation

The original memorandum treated hardware input blocking as a default capability. That should be
reframed as an optional, high-risk safeguard rather than a baseline requirement.

- Default behavior should rely on foreground verification, short execution windows, and operator
  guidance.
- Any use of `BlockInput` must be explicit, narrowly timed, and recoverable because only the thread
  that blocked input can normally unblock it.
- Input blocking should never be required for ordinary execution correctness.
- If the feature ever introduces blocking mode, it should include clear operator notification,
  timeout safeguards, and recovery behavior.

## Recommended Runtime Flow

1. The operator assembles or loads a batch inside the WinUI 3 screen.
2. The operator brings the intended Inventory window to the foreground.
3. The operator presses the global scanner shortcut to send the next staged record or send cycle.
4. The view-model validates the staged data and passes execution to the orchestration service.
5. The orchestration service verifies the target window and emits the configured input sequence
  through the native input service.
6. The orchestration service applies configurable settle delays and records success or failure for
  each item.
7. If an item fails after earlier items were already sent, processing stops immediately, previously
  sent rows remain marked as sent, the failed row is marked as failed, and all remaining rows stay
  pending for operator review or rerun.
8. A stop request from the global shortcut is honored between send cycles rather than interrupting
  an active input sequence mid-cycle.
9. Completed rows remain visible until the user manually removes them.
10. The UI is updated through the dispatcher abstraction.
11. Drafts, profiles, and run history are persisted asynchronously through DAOs and stored
  procedures when enabled.

## Risk Matrix And Quality Guardrails

- Target Application Desynchronization: the ERP can repaint or validate more slowly than the input
  stream. Mitigation: keep per-record or per-phase settle delays configurable and observable.
- Focus Loss Or Wrong-Window Injection: injected input is only safe when the intended foreground
  window is verified. Mitigation: verify target handle, title, and foreground ownership before each
  execution segment.
- Integrity-Level Restrictions: Windows can block injected input across privilege boundaries.
  Mitigation: validate execution environment and fail fast with operator-visible guidance.
- Human Input Interference: user key presses can interfere with injected sequences. Mitigation: use
  short execution bursts, explicit foreground control, optional guarded mode, and clear operator
  instructions.
- No Auto-Save Safety Net: removing automatic finalize shortcuts reduces accidental commits but also
  means completion depends on explicit operator send actions. Mitigation: keep send behavior
  deliberate, visible, and repeatable.
- Partial-Send Failure: once some rows have already been emitted, replaying automatically risks
  duplicate ERP transactions. Mitigation: stop on first failure, preserve earlier sent rows, and
  require operator review before resuming remaining rows.
- Hotkey Conflicts: global shortcuts can collide with existing registrations. Mitigation: make the
  shortcut configurable and handle registration failure explicitly.
- Stop Shortcut Timing: interrupting a live input burst can leave the ERP screen in an unknown
  state. Mitigation: honor `Ctrl+Alt+N` between send cycles, not during an active send sequence.
- Database Latency: synchronous persistence during execution can stall the operator workflow.
  Mitigation: keep persistence off the hot path and rely on asynchronous DAO operations.

## Resolved Operational Decisions

- Failure Handling After Partial Send: recommended behavior is stop on first failure. If earlier
  rows were already sent, those rows remain marked as sent, the failed row is marked as failed, and
  the remaining unsent rows stay pending for manual review or rerun. This is the safest approach
  because it avoids automatic replay of rows that may already exist in the ERP.
- Draft Ownership Scope: saved drafts are user-scoped.
- Cancellation Timing: `Ctrl+Alt+M` starts or sends and `Ctrl+Alt+N` stops processing. The stop
  request takes effect between send cycles rather than interrupting a live input sequence.

## Candidate NuGet Packages For Review

Research was performed against NuGet package listings, Context7 package documentation, and
Microsoft Learn platform guidance. These packages can cover parts of the scanner feature, but none
of them replace the need for MTM-specific orchestration, state tracking, validation, and DAO-backed
persistence.

- `FlaUI.Core` plus `FlaUI.UIA3`: strong review candidate if the feature needs richer desktop UI
  Automation beyond the current generic service contract. Context7 documents FlaUI as a .NET
  Windows automation library for Win32, WinForms, WPF, and Store apps with UIA3 support. NuGet
  shows current packages targeting modern .NET and .NET Framework. This is the best off-the-shelf
  candidate for window discovery, element lookup, and popup handling, but it is primarily oriented
  toward automation and testing workflows, so it should be evaluated carefully against the simpler
  existing `IService_UIAutomation` abstraction.
- `Interop.UIAutomationClient`: good review candidate when the team wants to stay close to the
  native Windows UI Automation stack while keeping `System.Windows.Automation`-style usage.
  NuGet describes it as the UI Automation COM-to-.NET adapter for the newer Windows Automation API
  interfaces with the same `System.Windows.Automation` programming model. This is the lowest-risk
  package candidate if the current project needs better UI Automation interop without adopting a
  larger external automation framework.
- `InputSimulatorCore`: review candidate for wrapping `SendInput` instead of hand-writing all
  keyboard and mouse interop. NuGet describes it as a .NET Core update of Windows Input Simulator
  that provides a simple C# interface over the Win32 `SendInput` API. Microsoft Learn still makes
  clear that `SendInput` remains subject to UIPI and current keyboard-state interference, so this
  package would reduce boilerplate but would not remove the feature's core safety constraints.
  Because the package is older, it should be treated as a convenience wrapper review option, not an
  automatic default.
- `NHotkey`: not a strong fit for this repository as currently described. NuGet states that the
  package is aimed at Windows Forms and WPF and requires concrete framework-specific packages such
  as `NHotkey.Wpf`. Since this feature lives in a WinUI 3 application, the package is useful as a
  reference point for hotkey behavior but is not the preferred implementation candidate.

## Package Review Conclusion

- No single NuGet package covers the full feature defined in this memorandum.
- The most plausible review path is a mixed approach: keep batch orchestration, failure handling,
  user-scoped draft persistence, and runtime flow custom to MTM, while evaluating a package only
  for one narrow technical layer.
- Best UI Automation review candidate: `Interop.UIAutomationClient` for minimal-stack alignment, or
  `FlaUI.Core` plus `FlaUI.UIA3` if richer UIA3 convenience APIs are needed.
- Best input-wrapper review candidate: `InputSimulatorCore`, with the expectation that MTM still
  owns integrity-level checks, keyboard-state handling, pacing, and stop-cycle behavior.
- Global hotkey registration should likely remain a direct Win32 implementation in this WinUI 3
  app unless a WinUI-compatible package with clearer maintenance and framework fit is identified.

## Approved Direction

This feature is approved only as a repository-aligned WinUI 3 MVVM capability built on shared
services, DAO-backed persistence, and documented Win32 interop. It is not approved as an
Entity-Framework-based subsystem, a direct database writer from the UI layer, or a generic
"hardware blocker first" automation design.