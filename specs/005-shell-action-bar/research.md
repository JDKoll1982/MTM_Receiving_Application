# Phase 0 Research: Persistent Shell Action Bar

Eleven choices the specification leaves open. Each one is recorded as what was chosen, why, and what else
was considered. Everything else the plan needs is already settled by the codebase.

## R1. Where the shell contract lives and how the shell finds it

**Decision**: a new `IViewModel_ShellActionProvider` in `Module_Core/Contracts/ViewModels/`, resolved from
the active page with the same three-step lookup `MainWindow.ResolveHeaderProvider` and
`ResolveStepBarProvider` already use: the content itself, then its `DataContext`, then its `ViewModel`
property.

**Rationale**: the header already proves this pattern for two other page-supplied capabilities, so the
shell gains no new mechanism and a third provider slots in beside the first two. It keeps `Module_Core`
free of module types: the contract speaks in booleans, strings, and `ICommand`.

**Alternatives considered**: a shell-resolved `IService_ShellActions` singleton. Rejected because it would
need a push channel from every page to report its state, and the shell would still have to know when a
page became active. Page-supplied provider with resolve-and-track already answers both.

## R2. Who implements the contract

**Decision**: `ViewModel_Receiving_Workflow` implements it. The step pages (PO entry, part selection,
loads, and the rest) stay plain hosted user controls and do not implement it.

**Rationale**: `View_Receiving_Workflow` is the only object in `ContentFrame.Content`; the guided steps,
Manual Entry, Edit Mode, Review, and the Result panel are user controls hosted inside it. The shell lookup
can therefore only ever reach the workflow view model. Putting the matrix there also puts every
visibility decision in one testable place that already knows `Enum_ReceivingWorkflowStep`.

**Alternatives considered**: making each step view model a provider and walking the visual tree to the
active host. Rejected as fragile, untestable, and it would spread the matrix across a dozen classes.

## R3. How the workflow view model learns the hosted pages' commands

**Decision**: the workflow page hands them over at construction, the same way it already assigns each
host's content and sets `RequestOpenLabelOverlay`. `View_Receiving_Workflow.xaml.cs` calls one new method
on the view model with the Manual Entry, Edit Mode, and Review view models it already receives by
injection.

**Rationale**: those view models are transient DI instances created for the hosted views, so asking the
container again would return different objects. The page is the only place that already holds the right
instances. The assignment is construction wiring, not business logic, and it mirrors
`ViewModel.RequestOpenLabelOverlay = ShowLabelOverlayAsync`.

**Alternatives considered**: resolving through `IService_ViewModelRegistry`. Rejected because only some
Receiving view models register themselves (Manual Entry and Edit Mode do, Review does not), so the lookup
would silently return nothing for the page that needs it most.

## R4. How the bar renders what the provider reports

**Decision**: a shell-owned projection view model, `ViewModel_Shared_ShellActionBar` in `Module_Shared`,
exposed by `MainWindow` and bound with `x:Bind`. `MainWindow` attaches the resolved provider to it and
detaches on navigation; the projection subscribes to `PropertyChanged` and re-reads the snapshot.

**Rationale**: `x:Bind` needs a compile-time type, and the resolved provider's type is only known at run
time. A fixed projection gives the bar a stable bindable surface while every actual value still comes from
the page. The alternative, setting control properties one by one from code behind, is what
`UpdateHeaderStepBar` does for the step bar, but that control has four properties; this bar has eight
buttons and three two-way checkboxes.

**Alternatives considered**: imperative control updates in `MainWindow.xaml.cs`. Rejected because the
checkboxes need two-way binding, which means event handlers and setter logic in code behind.

## R5. What the contract actually carries

**Decision**: one read-only snapshot, `Model_ShellActionBarState`, returned by `ShellActions`, plus a single
`SetDefaultMode(string? storedModeValue)` method. The snapshot carries visibility, enablement, label,
tooltip, accessibility name, and `ICommand` for each position, and the three default-checkbox states.

**Rationale**: twenty-odd loose properties on the interface would be unreadable and would have to be
re-announced one at a time. A snapshot is replaced wholesale on each step change, which is exactly how the
state changes. The stored mode arrives as the string the database already holds, so no Receiving enum
crosses into `Module_Core`.

**Alternatives considered**: an action descriptor list rendered through a template selector. Rejected
because the eight controls have different visuals (accent Next, red Clear Label Data, icon buttons for
Help and Overlay), so a selector would reintroduce per-action branching in XAML for no gain.

## R6. Where mode entry and default persistence live

**Decision**: mode entry stays in `ViewModel_Receiving_Workflow`, moved from the deleted mode page's view
model: confirm if there is unsaved data, reset the workflow, then `GoToStep` with
`RequestedEditDataSource = CurrentLabels` for Edit. Persistence goes through
`IService_UserPreferences.UpdateDefaultReceivingModeAsync` and updates the current session user's
`DefaultReceivingMode` so the checkbox state and the next launch agree.

**Rationale**: this is view orchestration plus a service call, which is what the MVVM rule asks for. The
confirmation dialog already exists on the workflow view model for the same reason.

**Alternatives considered**: a new `EnterModeAsync` on `Service_ReceivingWorkflow`. Rejected because the
confirmation dialog is UI, and pushing it into a service would either duplicate it or drag dialog code
down a layer.

## R7. What happens to the settings-backed startup mode

**Decision**: `Service_ReceivingWorkflow.StartWorkflowAsync` resolves the mode from the user preference,
falls back to Guided when the value is missing, empty, or unrecognised, and writes Guided back through
`IService_UserPreferences` when it had to fall back. The `Receiving.BusinessRules.DefaultModeOnStartup`
setting is no longer consulted for routing, and `Mode Selection` is removed from its option lists on the
two settings pages that expose it.

**Rationale**: the deleted step removes every destination that setting could name. Keeping a second
resolver that can point at a deleted page is exactly the blank-page edge case the spec calls out.
Acceptance also requires that no mode-selection entry exists anywhere in settings.

**Alternatives considered**: deleting the setting outright. Rejected as a wider change than needed; the
key stays readable and harmless, it just stops deciding the landing mode.

## R8. How the overlay button opens the window

**Decision**: the Overlay command calls the existing `RequestOpenLabelOverlay` hook, which
`View_Receiving_Workflow` sets to its existing `ShowLabelOverlayAsync`.

**Rationale**: the window is constructed in page code behind today, from
`ViewModel_Receiving_LabelOverlay` resolved out of the container. Routing through the existing hook means
the overlay window and its contents do not move, which is what the spec asks for. The hook fires while the
workflow page is still the frame content, because Manual Entry and Edit Mode are hosted inside it.

**Alternatives considered**: a new shell-level window launcher contract. Rejected because it would move
the window's construction out of Receiving for no behavioural gain, and the spec explicitly scopes the
overlay window out of the change.

## R9. What happens to the mode-selection keyboard shortcut

**Decision**: delete it. Remove the `Receiving.Shortcuts.ModeSelection` key, its default, its settings row,
the `ModeSelection` property on `Model_Settings_ReceivingShortcuts`, the `ReturnToModeSelectionCommand`,
`CanReturnToModeSelection`, and `IsModeSelectionVisible`.

**Rationale**: the shortcut's only destination no longer exists. The spec forbids new shortcuts and
requires every shortcut path to respect the bar's rules, so a shortcut that can reach nothing must go.

**Alternatives considered**: rebinding it to open the bar's mode group. Rejected because the mode group is
always visible on Receiving pages, so the shortcut would do nothing worth reporting, and it would invent a
shortcut the spec did not ask for.

## R10. Where the header back button is suppressed

**Decision**: in `MainWindow.UpdateHeaderBackButton`, force the button collapsed regardless of
`IService_HeaderBackNavigation.IsBackButtonVisible`. Leave the service, its registration, and every
`RegisterBackAction` call untouched.

**Rationale**: that method is the single place the control's visibility is set, so one guard hides it
everywhere without unwiring anything. Re-enabling later means deleting the guard, exactly the "visibility
change, not a re-plumb" the spec asks for. `View_Reporting_PreviewPage` and `ViewModel_ShipRecTools_Main`
keep registering their actions; those become invisible for now.

**Alternatives considered**: making `Service_HeaderBackNavigation` never report visible. Rejected because
it changes a shared service's contract behaviour and makes the later re-enable a service edit rather than a
one-line view edit.

## R11. How keyboard parity is guaranteed

**Decision**: leave the page's `KeyDown` handler where it is and keep gating it on the same members the bar
reads: `NextStepCommand.CanExecute` (which reads `ShowWorkflowNextButton && IsNextButtonEnabled`),
`PreviousStepCommand.CanExecute` (which reads `ShowWorkflowBackButton`), `CanClearLabelData`, and the
existing Help shortcut repointed at the bar's help command.

**Rationale**: the gate already mirrors the buttons; once the buttons stop existing, those same members
describe the bar. The handler is registered on the page with `handledEventsToo`, so it still sees keys
while a hosted step control has focus.

**Alternatives considered**: moving the handler into `MainWindow`. Rejected because it would need the
shell to know Receiving shortcut keys and commands, breaking FR-017.

## R12. How the per-page matrix is tested

**Decision**: xUnit tests in
`MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellActionTests.cs`
that drive `Service_ReceivingWorkflow` step changes with mocks and assert the snapshot per page: which
positions are shown, which are enabled, and which label each slot carries.

**Rationale**: the matrix is the acceptance surface, and it is pure view-model state with mocked services,
which the repository's testing rules classify as a unit test. The existing workflow tests already use this
shape (`Raise` on the service event, `Task.Delay(25)` after construction).

**Alternatives considered**: asserting through the shell projection. Rejected because that would test the
shell's plumbing rather than the Receiving decision, and the projection is a straight copy of the snapshot.
