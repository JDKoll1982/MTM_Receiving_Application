---
description: 'MTM main-window header workflow covering the shell header anatomy, title resolution (IViewModel_HeaderTitleProvider + fallback titles), the shared back action (IService_HeaderBackNavigation), per-module accent coloring, settings drill-down back state, and the sync rules.'
applyTo: 'MainWindow.xaml,MainWindow.xaml.cs,Module_Core/Contracts/ViewModels/IViewModel_HeaderTitleProvider.cs,Module_Core/Contracts/Services/IService_HeaderBackNavigation.cs,Module_Core/Services/Service_HeaderBackNavigation.cs,Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs,Module_Core/Themes/ModuleAccentBrushes.xaml,Module_*/ViewModels/**/*.cs'
---

<!-- 
[DOC-META-START]
- File Name: winui3-header-workflow.instructions.md
- Description: MTM main-window header workflow covering the shell header anatomy, title resolution (IViewModel_HeaderTitleProvider + fallback titles), the shared back action (IService_HeaderBackNavigation), per-module accent coloring, settings drill-down back state, and the sync rules.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6: # WinUI 3 Main-Window Header Workflow
  - Line 14: ## Header Anatomy (MainWindow.xaml)
  - Line 29: ## The Sync Chokepoint
  - Line 51: ## Title Resolution
  - Line 103: ## Module Accent Coloring
  - Line 151: ## Shared Back Action
  - Line 177: ## Settings Drill-Down Back State
  - Line 193: ## Rules And Guardrails
  - Line 223: ## Validation
  - Line 235: ## See Also
- Critical Notes: Every header refresh flows through MainWindow.SyncPageHeader — views and viewmodels never set header elements directly.
[DOC-META-END]
-->

# WinUI 3 Main-Window Header Workflow

The shell header is the persistent bar rendered inside `MainWindow`'s
`NavigationView.Header`. It shows the current page title, the active user, a
shared back button, and the active module's accent color. This file documents
how the header is populated and styled on every navigation so new pages,
viewmodels, and modules stay consistent.

## Header Anatomy (MainWindow.xaml)

Inside `NavigationView.Header` there is a two-row Grid:

- Row 0: `HeaderBarBorder` (rounded `Border`) containing:
  - `HeaderBackButton` + `HeaderBackButtonIcon` (shared back button, Collapsed by default).
  - `PageTitleTextBlock` (the page title, uses `SubtitleTextBlockStyle`).
  - `UserMenuButton` with `UserPictureBorder` (circular border around the
    initials) + `UserPicture` + `UserDisplayTextBlock` (user card).
- Row 1: `StatusInfoBar` (status notifications).

Only `MainWindow` owns these elements. Views and viewmodels never set
`PageTitleTextBlock.Text`, `HeaderBarBorder.Background`, or the back button
directly.

## The Sync Chokepoint

Every page swap flows through `MainWindow.SetContentPage(Page, fallbackTitle)`,
which calls `SyncPageHeader(content, fallbackTitle)`. This is the single place
the header is refreshed, so all of the following paths converge here:

- Nav-item clicks (`NavView_SelectionChanged` → `NavigateWithDI`).
- `NavigateToPage(pageType)` (used by settings pages and programmatic nav).
- `NavigateToSettingsTag(tag)` (settings hub drill-down).
- `ContentFrame_Navigated` and `ContentFrame_NavigationFailed` (frame events).

`SyncPageHeader` runs in this order:

1. `ClearHeaderSubscription()` — unsubscribes the previous header-provider
   ViewModel and drops `_currentWorkflowViewModel`.
2. `_headerBackNavigation.ClearBackAction()` — hides the shared back button.
3. `ApplyHeaderAccent(content?.GetType())` — applies/clears the module accent.
4. `ResolveHeaderProvider(content)` — looks up a title provider:
   - If found, `TrackHeaderProvider(provider)` (dynamic title).
   - Else, if a non-empty `fallbackTitle` exists, `UpdateHeaderText(fallbackTitle)`.
   - Else, `ResetHeaderContext()` (no title change).

## Title Resolution

### Dynamic titles (IViewModel_HeaderTitleProvider)

A ViewModel provides a live title by implementing
`Module_Core/Contracts/ViewModels/IViewModel_HeaderTitleProvider`:

```csharp
public interface IViewModel_HeaderTitleProvider : INotifyPropertyChanged
{
    string CurrentHeaderTitle { get; }
    string? CurrentHeaderContextTitle => null;
    string? CurrentHeaderContextSubtitle => null;
    ImageSource? CurrentHeaderContextImageSource => null;
    MaterialIconKind? CurrentHeaderContextIconKind => null;
}
```

Current implementers:

- `ViewModel_Receiving_Workflow` — `CurrentHeaderTitle => CurrentStepTitle`.
- `ViewModel_Dunnage_WorkFlowViewModel` — `CurrentHeaderTitle => CurrentStepTitle`
  (also exposes rich header context properties).
- `ViewModel_Scanner_Main` — `CurrentHeaderTitle => CurrentPageTitle`.
- `ViewModel_ShipRecTools_Main` — `CurrentHeaderTitle => CurrentToolTitle`.

`ResolveHeaderProvider` finds the provider in this order:

1. `content` is itself an `IViewModel_HeaderTitleProvider`.
2. `content` is a `FrameworkElement` whose `DataContext` is a provider.
3. `content` has a `ViewModel` property whose value is a provider.

`TrackHeaderProvider` subscribes to `PropertyChanged` and re-runs
`UpdateHeader` when `CurrentHeaderTitle` changes, so the header follows
step changes, tool switches, etc. Use `[NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]`
on the source property that drives it.

The `CurrentHeaderContext*` properties are the reserved "rich header" extension
point (title + subtitle + image/icon below the main title). The shell currently
renders only `CurrentHeaderTitle`; `ResetHeaderContext()` is a stub. Keep the
contract shape stable even if the rich header is not yet rendered.

### Static / fallback titles

When no provider is found, the title comes from:

- The nav route table `MainWindow._navRoutes` (e.g. `Reprint Labels`,
  `Scanner`, `Volvo Dunnage Requisition`).
- `View_Settings_CoreWindow.GetPageHeader(pageType)` → `(Title, Description)`
  used by settings navigation (`NavigateToSettingsTag`, `NavigateToPage`).
- `GetFallbackTitle(pageType)` reverse-looks-up `_navRoutes` by page type.

## Module Accent Coloring

`ApplyHeaderAccent(Type? pageType)` colors the header to match the active
module. `GetModuleAccent` maps the page type namespace to a shared brush set
from `Module_Core/Themes/ModuleAccentBrushes.xaml` (fill + theme-aware border +
fixed highlight):

- `Module_Receiving.*` / `Module_Settings.Receiving.*` → `ReceivingAccentBrush` (`#0B6157`).
- `Module_Dunnage.*` / `Module_Settings.Dunnage.*` → `DunnageAccentBrush` (`#8A5E00`).
- `Module_Volvo.*` / `Module_Settings.Volvo.*` → `VolvoAccentBrush` (`#1E4F7A`).
- `Module_Reporting.*` / `Module_Settings.Reporting.*` → `ReportingAccentBrush` (`#6A1B9A`).
- `Module_ShipRec_Tools.*` → `ShipRecAccentBrush` (`#37474F`).
- `Module_Reprint.*` → `ReprintAccentBrush` (`#2E7D32`).
- `Module_Scanner.*` → `ScannerAccentBrush` (`#B71C1C`).
- `Module_Settings.*` (core/settings hub and any settings page not claimed by a
  specific module above) → `SettingsAccentBrush` (`#424242`).

The generic `Module_Settings` prefix is checked **last** so settings namespaces
that belong to a specific module (Receiving, Dunnage, Volvo, Reporting) resolve
to that module's own color.

Rules:

- Accent path: `HeaderBarBorder.Background` = accent brush and `PageTitleTextBlock`
  + `HeaderBackButtonIcon` turn white. The header card border becomes the fixed
  `*AccentHighlightBrush` (a lighter tint of the module color, visible on the
  accent fill in both themes) at 2px. The user card (`UserMenuButton`) and the
  initials circle (`UserPictureBorder`) receive the theme-aware
  `*AccentBorderBrush` at 2px. The accent brushes are fixed brand colors —
  reference them with `StaticResource` in XAML, never redefine them.
- Theme-aware borders: `*AccentBorderBrush` resolves to the dark brand accent in
  Light theme and a lighter tint in Dark theme, so the user card and initials
  circle borders stay visible on their neutral fill in both modes. Resolved in
  code via the app `ThemeDictionaries`; re-applied on `ActualThemeChanged`.
- Reset path (all other pages): restore `LayerFillColorDefaultBrush` and
  `CardStrokeColorDefaultBrush`, reset border thickness to 1px, and clear the
  title/icon foregrounds with `ClearValue`.
- **Never set `PageTitleTextBlock.Foreground = null`.** A null local value
  overrides the style's theme foreground and renders the title invisible.
  Use `PageTitleTextBlock.ClearValue(TextBlock.ForegroundProperty)` to restore
  the `SubtitleTextBlockStyle` foreground (and `IconElement.ForegroundProperty`
  for the back icon).
- The grey neutral bar is expected for non-module pages; the title must remain
  readable on it.
- All header text stays readable in both themes: white on the accent fill
  (title/back icon) and the theme `TextFillColorPrimaryBrush` on the neutral
  user-card fill (user name).

## Shared Back Action

The header back button is driven by
`IService_HeaderBackNavigation` (registered as a singleton in
`Infrastructure/DependencyInjection/CoreServiceExtensions.cs`):

- `RegisterBackAction(Func<Task> action, string toolTip = "Back")` — sets
  `IsBackButtonVisible = true` and the tooltip.
- `ClearBackAction()` — hides the button and clears the action.
- `ExecuteBackActionAsync()` — runs the registered action.

MainWindow subscribes to `PropertyChanged` and updates the button via
`UpdateHeaderBackButton` (visibility + tooltip). Clicking the button runs
`ExecuteBackActionAsync`.

Examples:

- `ViewModel_ShipRecTools_Main` registers a per-tool "Back to Tools" action and
  clears it when returning to tool selection.
- `View_Reporting_PreviewPage` registers "Back to Report Setup" in `Loaded` and
  clears it in `Unloaded`.

Because `SyncPageHeader` clears the back action on every page change, a page
must re-register its action after navigation (in `Loaded` or when its state
becomes active).

## Settings Drill-Down Back State

`UpdateSettingsBackButtonState` manages the `NavigationView` back button (not the
header button) for settings sub-pages. It enables back only when all of these
hold:

- The app is in settings mode (`_isSettingsMode`).
- The current settings tag maps to a module context
  (`View_Settings_CoreWindow.TryGetModuleContext`).
- The active page differs from the module hub page and lives under the module's
  namespace prefix.

`EnterSettingsModeAsync` / `ExitSettingsModeAsync` switch the nav pane between
application and settings items and restore the return route on exit
(`ResolveSettingsReturnRouteTag`).

## Rules And Guardrails

- Route all header title changes through `IViewModel_HeaderTitleProvider` or the
  fallback title path; never write to `PageTitleTextBlock` from a view or
  viewmodel.
- Keep `SyncPageHeader` as the only header-refresh entry point; add new
  navigation paths through it instead of duplicating title/accent logic.
- New module landing pages that change title by context should implement
  `IViewModel_HeaderTitleProvider` and notify `CurrentHeaderTitle`.
- Use the shared module accent brushes; do not hardcode hex values or duplicate
  the `*AccentBrush` / `*AccentBorderBrush` / `*AccentHighlightBrush` keys
  locally.
- Apply accent borders through `ApplyHeaderAccent` only (single chokepoint);
  never touch `HeaderBarBorder.BorderBrush`, `UserMenuButton.BorderBrush`, or
  `UserPictureBorder.BorderBrush` from a view or viewmodel.
- Keep the header borders theme-aware: the user card and initials circle sit on
  a neutral fill, so they must use the `*AccentBorderBrush` theme dictionaries
  (not a fixed brush) or they will disappear in dark mode. The header card
  border stays fixed (`*AccentHighlightBrush`) because the accent fill behind it
  does not change with theme.
- Re-apply the accent on theme changes: `ActualThemeChanged` already re-runs
  `ApplyHeaderAccent` so live light/dark toggles keep the borders correct.
- Never assign a null `Foreground` to header text elements; use `ClearValue` or a
  resolved theme brush.
- Register back actions symmetrically (register when a drill-down becomes active,
  clear when it ends); rely on `SyncPageHeader` clearing stale actions on
  navigation.
- Preserve the `CurrentHeaderContext*` contract for the future rich header even
  though the shell does not render it yet.

## Validation

- `dotnet build MTM_Receiving_Application.csproj -c Debug -p:Platform=x64`
- Manually verify each module page (Receiving, Dunnage, Volvo, Reporting,
  Ship/Rec Tools, Reprint, Scanner + their settings) shows the accent header
  with white text and the accent borders on the header card, user card, and
  initials circle, and that Dashboard / Documentation show the neutral bar with
  readable title text.
- Toggle light/dark theme (Settings → Theme) on a module page and confirm the
  user-card and initials-circle borders stay visible and the title/user text
  stays readable in both modes.

## See Also

- `MainWindow.xaml` / `MainWindow.xaml.cs` — header UI and sync logic.
- `Module_Core/Contracts/ViewModels/IViewModel_HeaderTitleProvider.cs` — dynamic title contract.
- `Module_Core/Contracts/Services/IService_HeaderBackNavigation.cs` and
  `Module_Core/Services/Service_HeaderBackNavigation.cs` — shared back action.
- `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs` — settings page headers
  and module contexts (`GetPageHeader`, `TryGetModuleContext`).
- `Module_Core/Themes/ModuleAccentBrushes.xaml` — the module accent brushes.
- `architecture/mvvm-pattern.instructions.md` — ViewModel layering that feeds the header.
