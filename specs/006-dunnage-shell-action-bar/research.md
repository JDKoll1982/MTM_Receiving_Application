# Research: Shell Action Bar for Non-Inventoried Labels

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Each entry records what was chosen, why, and what was rejected. Only the choices this feature actually
had to make appear here; everything else follows the pattern 005 already established.

## 1. How a third Dunnage mode travels through a strip built for Receiving's four

**Decision**: add a fourth entry-mode slot to the snapshot. `Model_ShellActionBarState` gains an
`ImageSearch` property beside `Guided`, `Manual`, `Edit`, and `Overlay`, and an `IsImageSearchDefault`
flag beside the three existing default flags. `ViewModel_Shared_ShellActionBar` projects both, and
`MainWindow.xaml` gets one more chip block in the left group. Dunnage fills `Guided`, `Edit`, and
`ImageSearch` and leaves `Manual` and `Overlay` hidden; Receiving keeps filling its four and leaves
`ImageSearch` hidden.

**Rationale**: the spec forbids a second strip and requires the mode group to show the owning module's
own modes, which a slot each module fills or hides already delivers. The change touches one property
per layer and one chip block, leaves Receiving's snapshot byte-for-byte equivalent, and keeps the
existing default-checkbox wiring intact.

**Alternatives considered**: replacing the fixed slots with an ordered `IReadOnlyList<Model_ShellModeAction>`
rendered by a repeater. It is the cleaner long-term shape, but it rewrites the strip's XAML and the
projection, and invalidates the 005 tests, for two modules that between them need five slots. Reusing the
`Overlay` slot for Image Search would weld two modules' meanings to one property and cannot carry a
default marker, because `Overlay` is deliberately not defaultable.

## 2. Where the Dunnage workflow's verbs live now that the shell owns the buttons

**Decision**: move Back, Next, Review, Save, and the mode-entry commands into
`ViewModel_Dunnage_WorkFlowViewModel`, and have `View_Dunnage_WorkflowView.xaml.cs` hand the hosted
child view models to it through a new `AttachShellActionHosts(...)` call, mirroring
`ViewModel_Receiving_Workflow`.

**Rationale**: the shell can only reach `IViewModel_ShellActionProvider.Command`, and today every one of
these verbs is a code-behind handler that reaches into child view models through XAML element names. The
gates the strip must honour (`PartSelectionView.ViewModel.IsPartSelected`,
`QuantityEntryView.ViewModel.IsValid`, `DetailsEntryView.ViewModel.CanProceedToNextStep`,
`ManualEntryView`/`EditModeView` unsaved state) are only visible from the page, so they have to be handed
over rather than discovered. This is also what FR-034 asks for.

**Alternatives considered**: keeping the handlers and letting `MainWindow` call into the page. The shell
sees the page's provider, not its children, and reaching through a view to its named elements from
`MainWindow` would put page knowledge in the shell, which FR-026 forbids.

## 3. What Next does on Select Type

**Decision**: Select Type offers Next, enabled once a type is selected. The type cards keep
auto-advancing as they do today.

**Rationale**: the per-page matrix in FR-017 lists Next on Select Type, and FR-014 requires it to be
unavailable until the page's own condition holds, which is a chosen type. Keeping the card's
auto-advance means no operator loses a step they rely on. Next is not dead: after Back from Select Part
the type is still selected, so Next is the way forward again.

**Alternatives considered**: (a) omit Next from Select Type and record a deviation from FR-017, because
auto-advance makes it rarely reachable; (b) stop the card auto-advancing so Next is the only way
forward. Both change behaviour the request did not ask to change. The first breaks a stated contract,
the second costs every operator an extra click on every part they enter.

## 4. What Back does on Image Search

**Decision**: Back returns to the guided step the operator was on when they entered Image Search, and to
Select Type when Image Search was the mode the module opened in.

**Rationale**: Image Search has no predecessor inside the guided walk, so a fixed destination is wrong in
one of the two ways it can be reached. Remembering where the operator came from costs one field and
matches what Back means everywhere else on the strip. Choosing a part continues to hand off to Select
Part, which is unchanged.

**Alternatives considered**: always returning to Select Type, which throws away guided progress when the
operator pressed Image Search mid-walk; and offering the deleted "Mode Selection" return, which FR-011
forbids.

## 5. Where the strip's words come from

**Decision**: a new `DunnageSettingsKeys.UiText` group holding `Workflow.Back`, `Workflow.Next`,
`Workflow.Help`, `Workflow.ClearAllLabels`, `Workflow.Review`, `Workflow.Save`, `Shell.Guided`,
`Shell.Edit`, `Shell.ImageSearch`, `Shell.Default`, and one `Shell.DefaultTooltip` per mode, with the
wording already on screen as the defaults in `DunnageSettingsDefaults` and one editable entry each in
`settings.manifest.json`.

**Rationale**: this is the shape Receiving already uses, so both modules are edited the same way, which
is US6's whole point. The manifest is where a manager edits the text, and the defaults dictionary is
what an unedited install shows.

**Alternatives considered**: reading the labels from resource files. That would give a second,
inconsistent place to change operator-visible text and would not satisfy FR-022's requirement that the
copy be settings-driven.

One conflict had to be settled here. The two replaced save controls read `Save All` on Review and
`Save Changes` on Edit Mode, while the spec's Verbatim Constraints pin the save verb as exactly `Save`.
The constraints win: `Dunnage.UiText.Workflow.Save` defaults to `Save`, and both slots use it. This is
the one operator-visible wording change in the feature, and it is what the spec asked for.

## 6. How manual mode is removed

**Decision**: delete `Enum_DunnageWorkflowStep.ModeSelection` and `Enum_DunnageWorkflowStep.ManualEntry`;
delete `ViewModel_Dunnage_ModeSelection`, `View_Dunnage_ModeSelectionView`,
`ViewModel_Dunnage_ManualEntry`, and `View_Dunnage_ManualEntryView`; drop `Dunnage.Shortcuts.ModeSelection`
from the settings keys, the defaults, the shortcut service, the settings page model and view model, and
the manifest; drop the `Dunnage.ModeSelection` and `Dunnage.ManualEntry` help topics; and remove the
corresponding registrations. `StartWorkflowAsync` sends `manual`, an unrecognised value, and an absent
value all to the guided entry, and writes `guided` back for the first two.

**Rationale**: FR-011 says no step or screen may implement manual mode, and leaving an enum member
behind leaves a route that compiles. Correcting the stored value in the same call that reads it is what
FR-012 asks for and is why no migration script is needed.

**Alternatives considered**: keeping the members and refusing to navigate to them. A refused route is
still a route, and the enum member would have to survive every later switch over the step, which is how
the mode stayed alive this long.

## 7. How much of the search catalog to rebuild

**Decision**: keep the hand-maintained catalog, and fix what the audit finds:
`NavigateToRouteTagAsync` is split so the navigation pane and the search box call one opener, which
carries the scanner allow-list check and the leave confirmation; the scanner entry is filtered out of
the offered results for a user the policy refuses; `ReconciliationLauncherPage` (Receiving Validator)
is added to the catalog, because it is reachable from the navigation pane and missing from search; and
`View_Settings_Dunnage_LabelPaths` is registered in dependency injection, because it is offered in
search but was never registered and so cannot open.

**Rationale**: FR-030 to FR-033 ask for completeness, accuracy, and the same access rules, not for a new
mechanism. The catalog is short, explicit, and already the single place a search result is declared; a
generated catalog would replace a readable list with machinery to solve a drift problem caused by four
specific omissions.

**Alternatives considered**: generating the catalog by reflecting over pages or navigation attributes.
That trades a list anyone can audit for a mechanism that must be kept in step with navigation anyway,
and it cannot express "this page is an inner guided step, do not offer it".

## 8. How the settings hubs resolve a card

**Decision**: give every Dunnage hub card a `Tag` holding the exact step title it opens and resolve the
step by that title, which is what `View_Settings_Receiving_CategoryHub` already does. Delete the
`OnStep0Clicked`..`OnStep5Clicked` index handlers. Add a Label Files card, because that settings page
exists, is offered in search, and had no way in from the hub. Then check every Receiving card's tag
against its step title.

**Rationale**: FR-028 and FR-029 require resolution by name, not by position. The Dunnage hub's
positional mapping happens to be correct today, but any inserted or reordered card silently moves the
rest, which is the fault reported on the Receiving hub. Adding the Label Files card closes the one page
that had no route from its own hub.

**Alternatives considered**: leaving Dunnage positional because it currently works. That leaves the two
hubs maintained by two different rules, which is how the Receiving hub drifted in the first place.

Two observations recorded rather than acted on: `View_Settings_Dunnage_WorkflowHub`,
`View_Settings_Receiving_WorkflowHub`, `View_Settings_Reporting_WorkflowHub`, and
`View_Settings_Volvo_WorkflowHub` still use index-based card handlers. FR-028 and FR-029 name the two
category hubs, so those pages are out of scope, and the search catalog does not offer them.

## 9. Which existing tests move

**Decision**: delete `ViewModel_Dunnage_ModeSelectionViewModelTests`; re-point
`ViewModel_Dunnage_WorkFlowViewModelTests` off `Enum_DunnageWorkflowStep.ModeSelection`, which it uses as a
neutral starting step, onto guarded steps; drop the mode-selection assertions from
`Service_DunnageShortcutsTests` and `SettingsManifestDunnageShortcutTests`; and add
`ViewModel_Dunnage_ShellActionTests` and `ViewModel_Dunnage_ShellDefaultModeTests` beside the three
Receiving shell test files.

**Rationale**: the deleted screen's tests assert a screen that no longer exists, and the workflow tests
would otherwise fail to compile. The new tests mirror the Receiving files that already assert a
per-page matrix and a default-mode round trip, so the two modules are verified the same way.

**Alternatives considered**: extending the Receiving shell tests to cover Dunnage. They are named for
and scoped to one module, and a shared test would need both modules' fixture graphs.
