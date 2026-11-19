# Shell Action Provider Contract

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

This is the interface the shell codes against. It is the whole surface that crosses the shell /
module boundary: the shell resolves a provider, reads one snapshot, and calls one method. Nothing
here names a `Module_Receiving` type, so `MainWindow` stays module-agnostic (FR-017).

## Resolution

The shell resolves the provider from the active page with the same three-step lookup it already
uses for `IViewModel_HeaderTitleProvider` and `IViewModel_HeaderStepBarProvider`: the
`ContentFrame` content itself, then its `DataContext`, then its `ViewModel` property. The
resolve-and-track pattern to copy is `MainWindow.UpdateHeader` and `MainWindow.TrackHeaderStepBar`.
A page that resolves to no provider is a valid state: the bar row collapses to `Collapsed` and the
page content takes the full remaining height.

## IViewModel_ShellActionProvider

A page view model implements or supplies this contract. `Module_Receiving` implements it on
`ViewModel_Receiving_Workflow`.

```csharp
public interface IViewModel_ShellActionProvider : INotifyPropertyChanged
{
    Model_ShellActionBarState ShellActions { get; }
    void SetDefaultMode(string? storedModeValue);
}
```

| Member | Type | Behaviour |
| --- | --- | --- |
| `ShellActions` | `Model_ShellActionBarState` | an immutable snapshot of every slot the bar renders, replaced wholesale whenever the active step or page changes |
| `SetDefaultMode` | `void SetDefaultMode(string? storedModeValue)` | persists the stored default. The argument is the stored string the database holds: `guided`, `manual`, `edit`, or `null` |

`PropertyChanged` is raised for `ShellActions` alone; the shell re-reads the whole snapshot rather than any
loose per-control property. A page that never raises it simply keeps the snapshot it handed over first.

## Model_ShellActionBarState

The snapshot the shell binds to. Every position carries its own visibility, enablement, label,
tooltip, accessibility name, and `ICommand`. The three default checkboxes carry their ticked state on
three settable booleans the shell projection owns; the setter is the only path a tick takes.

| Position | What the snapshot holds |
| --- | --- |
| `Guided` | visible, enabled, label `Guided`, command that enters guided mode, default checkbox with tooltip `make Guided my default` |
| `Manual` | visible, enabled, label `Manual`, command that enters manual mode, default checkbox with tooltip `make Manual my default` |
| `Edit` | visible, enabled, label `Edit`, command that enters edit mode, default checkbox with tooltip `make Edit my default` |
| `Overlay` | visible, enabled, label `Overlay`, command that opens `View_Receiving_LabelOverlayWindow` |
| `Back` | visible and enabled flags, label `Back`, command that invokes `PreviousStepCommand`, or the page-specific verb that occupies the Back slot |
| `Next` | visible and enabled flags, label `Next`, command that invokes `NextStepCommand`, or the page-specific verb that occupies the Next slot |
| `Clear Label Data` | visible and enabled flags, label `Clear Label Data`, command gated by `CanClearLabelData` that calls `ResetLabelDataAsync` |
| `Help` | visible and enabled flags, label `Help`, command that calls `ShowHelpAsync` |

The three default checkboxes are mutually exclusive: ticking one clears the other two, and unticking
the last one leaves no stored default. Ticking a checkbox persists the default through
`IService_UserPreferences.UpdateDefaultReceivingModeAsync` and must not change the active page or
mode (FR-006, FR-007). Because a `CheckBox` has no command, the tick travels through the projection's
`IsGuidedDefault`, `IsManualDefault`, or `IsEditDefault` setter, which calls `SetDefaultMode`. The
snapshot never carries a ticked state of its own.

## Shell Projection

`MainWindow` exposes a `ViewModel_Shared_ShellActionBar` in `Module_Shared` and binds the bar to it
with `x:Bind` only. The projection subscribes to the provider's `PropertyChanged`, re-reads
`ShellActions`, and holds no domain logic; it forwards whatever the attached provider reports. When
the shell detaches on navigation, the projection must collapse the bar row rather than wait for a
later provider.

## Rules

- Labels and tooltips come from `ReceivingSettingsKeys` through `IService_ReceivingSettings`; no
  user-visible string is hard-coded where a settings key already covers it (FR-025).
- New keys, all under `Receiving.UiText.Shell`, with the verbatim defaults as their default values:

| Key | Default |
| --- | --- |
| `Receiving.UiText.Shell.Guided` | `Guided` |
| `Receiving.UiText.Shell.Manual` | `Manual` |
| `Receiving.UiText.Shell.Edit` | `Edit` |
| `Receiving.UiText.Shell.Overlay` | `Overlay` |
| `Receiving.UiText.Shell.GuidedDefaultTooltip` | `make Guided my default` |
| `Receiving.UiText.Shell.ManualDefaultTooltip` | `make Manual my default` |
| `Receiving.UiText.Shell.EditDefaultTooltip` | `make Edit my default` |

Each new key needs all three of: the constant in `ReceivingSettingsKeys`, the entry in
`ReceivingSettingsDefaults.StringDefaults`, and the entry in
`Module_Settings.Core/Defaults/settings.manifest.json`. The existing `WorkflowBack`, `WorkflowNext`,
`WorkflowResetLabelData`, `WorkflowHelp`, `ManualEntrySaveAndFinish`, `EditModeSaveAndFinish`,
`ReviewSaveToDatabase`, and `CompletionStartNewEntry` keys cover the remaining positions.
- The strip invokes existing actions rather than re-implementing them: `PreviousStepCommand`,
  `NextStepCommand`, `ResetLabelDataAsync` gated by `CanClearLabelData`, and `ShowHelpAsync`
  (FR-024).
- Back is shown only on pages that support going back, and Next is disabled until the page's own
  conditions are met, including `IService_ReceivingWorkflow.IsPoPartListLoaded` on the PO step
  (FR-009).
- `Ctrl+Left` and `Ctrl+Right` keep working and gate on the same members the bar reads, so a
  keyboard shortcut can never reach an action the bar has hidden or disabled (FR-022).
- `IService_HeaderBackNavigation` stays registered and wired, and `HeaderBackButton` stays collapsed
  on every page; Back is offered only by the strip (FR-021).
- The new provider registration lives in `Infrastructure/DependencyInjection`, never in
  `App.xaml.cs`.
