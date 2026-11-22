# Shell Action Provider Contract

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

The shell owns the strip. A page tells it what to draw through one core contract, and the shell never
learns anything about the module behind the page (FR-026). This document is the contract as it stands
after this feature, so a test can assert it without reading the shell.

## `IViewModel_ShellActionProvider`

`MTM_Receiving_Application.Module_Core.Contracts.ViewModels`

```csharp
public interface IViewModel_ShellActionProvider : INotifyPropertyChanged
{
    Model_ShellActionBarState ShellActions { get; }
    void SetDefaultMode(string? storedModeValue);
}
```

**Implementors**: `ViewModel_Receiving_Workflow` and `ViewModel_Dunnage_WorkFlowViewModel`.

**Resolution**: `MainWindow.ResolveShellActionProvider` looks, in order, for the content implementing
the interface, the content's `DataContext` implementing it, and the content's `ViewModel` property
returning an implementation. A page that supplies none collapses the strip.

**Notification**: the provider raises `PropertyChanged` for `ShellActions` alone. The snapshot is
replaced whole on every step or page change; no loose per-control property is tracked.

**`SetDefaultMode`**: takes the stored value the shell wants kept, or `null` to clear the default. It
never changes the active page. The stored values are `guided`, `edit`, and `image-search` for
Non-Inventoried Labels, and `guided`, `manual`, and `edit` for Receiving. Any other value is treated as
absent.

## `Model_ShellActionBarState`

`MTM_Receiving_Application.Module_Core.Models.UI`

The immutable snapshot, replaced whole. All properties are `init`.

| Property | Type | Default | Meaning |
| --- | --- | --- | --- |
| `IsVisible` | `bool` | `false` | The page has something to show. |
| `ShowModeGroup` | `bool` | `false` | The page belongs to a module that has entry modes. |
| `Guided` | `Model_ShellModeAction` | `Hidden` | Guided mode chip. |
| `Manual` | `Model_ShellModeAction` | `Hidden` | Manual mode chip. Receiving only. |
| `Edit` | `Model_ShellModeAction` | `Hidden` | Edit mode chip. |
| `Overlay` | `Model_ShellModeAction` | `Hidden` | Overlay mode chip. Receiving only. |
| `ImageSearch` | `Model_ShellModeAction` | `Hidden` | **New.** Image Search mode chip. Non-Inventoried Labels only. |
| `Back` | `Model_ShellActionSlot` | `Hidden` | Back position, or the page's own verb. |
| `Next` | `Model_ShellActionSlot` | `Hidden` | Next position, or the page's own verb. |
| `ClearLabelData` | `Model_ShellActionSlot` | `Hidden` | Clear Label Data position. |
| `Help` | `Model_ShellActionSlot` | `Hidden` | Help position. |
| `IsGuidedDefault` | `bool` | `false` | Guided holds the stored default. |
| `IsManualDefault` | `bool` | `false` | Manual holds the stored default. |
| `IsEditDefault` | `bool` | `false` | Edit holds the stored default. |
| `IsImageSearchDefault` | `bool` | `false` | **New.** Image Search holds the stored default. |

`Model_ShellActionBarState.Empty` is the snapshot that offers nothing.

**Invariant**: at most one `Is*Default` flag is true. A module sets only the flags for the modes it owns,
so Receiving never sets `IsImageSearchDefault` and Non-Inventoried Labels never sets `IsManualDefault`.

## `Model_ShellModeAction`

`MTM_Receiving_Application.Module_Core.Models.UI`

| Property | Type | Notes |
| --- | --- | --- |
| `Mode` | `string` | The stored value: `guided`, `manual`, `edit`, `overlay`, or `image-search`. |
| `Label` | `string` | Settings-driven. `Guided`, `Edit`, `Image Search` for Non-Inventoried Labels. |
| `Tooltip` | `string` | Settings-driven. |
| `AccessibilityName` | `string` | Screen-reader name. Never empty. |
| `Command` | `ICommand?` | Enters the mode. |
| `IsActive` | `bool` | The mode in force. Draws the success-green text and border. |
| `CanBeDefault` | `bool` | False for `overlay`; true for the other four. |
| `DefaultBadge` | `string` | `Default` when the chip can hold a default, empty otherwise. |
| `DefaultTooltip` | `string` | How to make this mode the default. |
| `ShowDividerAfter` | `bool` | True between chips, false after the module's last chip. |

## `Model_ShellActionSlot`

`MTM_Receiving_Application.Module_Core.Models.UI`

| Property | Type | Notes |
| --- | --- | --- |
| `IsVisible` | `bool` | False removes the control instead of disabling it. |
| `IsEnabled` | `bool` | False renders it disabled but present. |
| `Label` | `string` | Settings-driven button text. |
| `Tooltip` | `string` | Settings-driven. |
| `AccessibilityName` | `string` | Screen-reader name. Never empty. |
| `Command` | `ICommand?` | The command the page already used. The strip supplies none. |
| `ShiftCommand` | `ICommand?` | The wider variant for Shift activation. Only Clear All Labels sets it. |
| `Kind` | `Enum_ShellActionKind` | `Plain`, `Back`, or `Save`; drives the colour. |
| `ShowDividerAfter` | `bool` | Drawn only when both Back and Next are shown. |

## Shell Behaviour the Provider May Rely On

- The strip's row collapses when the page supplies no provider or an empty snapshot, and the page takes
  the full remaining height.
- The strip never invents a command, a label, or a visibility. It draws exactly what the snapshot says.
- A mode press runs the chip's command; a Ctrl press calls `SetDefaultMode` instead and re-reads the
  snapshot, which is what clears the other default markers.
- A Clear All Labels activation runs `ShiftCommand` when Shift is held and it is set, and `Command`
  otherwise.
- The strip detaches from a page that has gone away, and a detached provider cannot push itself back in.

## Prohibited

- A module must not expose its own strip, its own navigation row, or a second binding of a command the
  strip owns (FR-018).
- The shell must not name a module type for these actions. The pre-existing
  `ContentFrame.Content is View_Receiving_Workflow` and `is View_Dunnage_WorkflowView` checks in the
  leave-confirmation methods are the only ones, and they must not grow.
- A module must not report a mode it does not own (FR-005).
