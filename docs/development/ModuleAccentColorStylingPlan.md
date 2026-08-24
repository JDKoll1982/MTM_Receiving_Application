# Module Accent Color Styling Plan

Last Updated: 2026-08-24

## Context

The Reporting module and the Reprint module already define a set of brand accent
colors that identify the three primary working modules:

| Module    | Accent Color | Hex       | Existing references                                            |
| --------- | ------------ | --------- | -------------------------------------------------------------- |
| Receiving | Dark teal    | `#0B6157` | `Service_Reporting.SummaryAccentBackground`, `ReceivingAccentBrush` |
| Dunnage   | Dark amber   | `#8A5E00` | `Service_Reporting.DunnageAccentBackground`, `DunnageAccentBrush`   |
| Volvo     | Dark blue    | `#1E4F7A` | `Service_Reporting.DetailAccentBackground`, `VolvoAccentBrush`      |

The Receiving, Dunnage, and Volvo modules themselves currently use only the system
accent color (`AccentFillColorDefaultBrush`, `AccentTextFillColorPrimaryBrush`,
`SystemAccentColor`, `AccentButtonStyle`) and do not carry their brand identity.

Goal: incorporate each module's accent color into its own UI so the three modules are
visually branded and consistent with the Reporting and Reprint modules.

> **Revision (2026-08-24):** The initial approach added an in-page "Module Brand Header"
> band to each module's landing page. Per follow-up feedback, those in-page header bands
> were **removed** and replaced with a single mechanism: the **MainWindow shell header
> changes its accent** to the active module's color. See `MainWindow Shell Header` below.

> **Revision (2026-08-24):** Extended the brand palette from three to **eight** modules
> (added Reporting, Ship/Rec Tools, Reprint, Scanner, Settings) and added **accent border
> coloring** to the shell header: the header card border uses a fixed `*AccentHighlightBrush`
> tint on top of the accent fill, while the user card and the initials circle use theme-aware
> `*AccentBorderBrush` values so they stay visible in light and dark mode.

## Research Summary

WinUI 3 theming guidance (Microsoft Learn + WinUI 3 Gallery):

- Define reusable brand brushes in a `ResourceDictionary` and reference them by key.
- Use `StaticResource` for fixed brand colors that should not change with theme; use
  `ThemeResource` only for theme-dependent values.
- The three accent colors are dark, so a white foreground provides strong contrast.
  White text on these accents is accessible.
- Use slightly lighter and darker variants of the accent for hover and pressed states
  instead of relying on default control overlays.
- Avoid retemplating controls where possible; brand through surfaces that already use
  a solid fill or accent text.

## Shared Resources (all modules)

1. `Module_Core/Themes/ModuleAccentBrushes.xaml` is the single source of truth:
   - Fill brushes: `ReceivingAccentBrush` (`#0B6157`), `DunnageAccentBrush`
     (`#8A5E00`), `VolvoAccentBrush` (`#1E4F7A`), `ReportingAccentBrush`
     (`#6A1B9A`), `ShipRecAccentBrush` (`#37474F`), `ReprintAccentBrush`
     (`#2E7D32`), `ScannerAccentBrush` (`#B71C1C`), `SettingsAccentBrush`
     (`#424242`).
   - A shared `OnAccentForegroundBrush` (White) for text/icons on accent fills.
   - Fixed `*AccentHighlightBrush` values (lighter tints of each fill) for the
     header card border on top of the accent fill.
   - Theme-aware `*AccentBorderBrush` values (Light = dark brand accent, Dark =
     lighter tint) for the user card and initials circle borders.
   - Status: complete. No hover/pressed variants were needed because this pass brands
     static surfaces (headers, bars, icons), not accent-filled interactive buttons.
2. Merge the dictionary into `App.xaml` so every view can resolve the brushes.
   Status: complete.
3. Update `Module_Reprint/Views/View_Reprint_Main.xaml` to reference the shared brushes
   and remove its local brush definitions (single source of truth; no visual change).
   Status: complete.

## Receiving Module

Target: `Module_Receiving/Views/` — Status: complete.

- The in-page header band was **removed** (accent now lives in the shell header).
- `View_Receiving_ModeSelection.xaml`
  - Change the mode-card and reconcile-action `FontIcon` foregrounds from
    `AccentFillColorDefaultBrush` to `ReceivingAccentBrush`.
- `View_Receiving_Review.xaml`
  - Change the "Entry Counter" bar background from `AccentFillColorDefaultBrush` to
    `ReceivingAccentBrush` (bar already uses white text).
- `View_Receiving_POEntry.xaml`
  - Change the auto-detected package type `SystemAccentColor` text to
    `ReceivingAccentBrush`.
- `View_Receiving_Workflow.xaml`
  - Change the "Return to Mode Selection" button `SystemAccentColor` border to
    `ReceivingAccentBrush`.

## Dunnage Module

Target: `Module_Dunnage/Views/` — Status: complete.

- The in-page header band was **removed** (accent now lives in the shell header).
- `View_Dunnage_ModeSelectionView.xaml`
  - Change the three mode-card `FontIcon` foregrounds from `AccentFillColorDefaultBrush`
    to `DunnageAccentBrush`.
- `View_Dunnage_ReviewView.xaml`
  - Change the "Entry" bar background from `AccentFillColorDefaultBrush` to
    `DunnageAccentBrush` (bar already uses white text).
- `View_Dunnage_QuickAddTypeDialog.xaml`
  - Change the header accent text/icon usages from `AccentTextFillColorPrimaryBrush` to
    `DunnageAccentBrush`.

## Volvo Module

Target: `Module_Volvo/Views/` — Status: complete.

- The in-page header bar was **removed** (accent now lives in the shell header).
- `View_Volvo_ShipmentEntry.xaml`
  - Add a `VolvoAccentBrush` left accent bar to the per-part `Expander` header so the
    module color appears on each shipment line card.
- `View_Volvo_History.xaml`
  - Change the "Volvo Shipment History" title foreground to `VolvoAccentBrush`.
- `View_Volvo_ShipmentHistoryDetailDialog.xaml`
  - Change the dialog title accent text to `VolvoAccentBrush` where it currently uses
    the system accent.

Note: The Volvo status/discrepancy converters keep their semantic state colors (pending
amber, received green, warning amber) because they convey state, not brand. No change.

## MainWindow Shell Header

Status: complete.

- `MainWindow.xaml`
  - Name the header `Border` `HeaderBarBorder` and the back-button icon
    `HeaderBackButtonIcon`.
  - Wrap the initials `PersonPicture` in a circular named `Border`
    (`UserPictureBorder`) so it can carry the module accent ring.
  - The `HeaderUserMenuButtonStyle` hover/pressed states keep
    `{TemplateBinding BorderBrush}` so the module accent border on the user card
    persists on hover.
- `MainWindow.xaml.cs`
  - `ApplyHeaderAccent(Type?)` is called from `SyncPageHeader` (the single chokepoint
    every page navigation flows through) and re-applied on `ActualThemeChanged`.
  - `GetModuleAccent(Type?)` maps the active page type to a fill brush, a
    theme-aware border key, and a fixed highlight key:
    - `Module_Receiving.*` / `Module_Settings.Receiving.*` → `ReceivingAccentBrush`
    - `Module_Dunnage.*` / `Module_Settings.Dunnage.*` → `DunnageAccentBrush`
    - `Module_Volvo.*` / `Module_Settings.Volvo.*` → `VolvoAccentBrush`
    - `Module_Reporting.*` / `Module_Settings.Reporting.*` → `ReportingAccentBrush`
    - `Module_ShipRec_Tools.*` → `ShipRecAccentBrush`
    - `Module_Reprint.*` → `ReprintAccentBrush`
    - `Module_Scanner.*` → `ScannerAccentBrush`
    - `Module_Settings.*` (core/settings hub, checked last) → `SettingsAccentBrush`
  - When an accent applies, the header card background becomes the module color, the
    page title and back-button icon turn white, the back-button border becomes a
    translucent white, the header card border becomes the fixed accent highlight, and
    the user card + initials circle borders become the theme-aware accent border.
  - Pages outside those modules (Dashboard, Documentation) reset the header to the
    neutral theme resources.
  - The reset restores the title/back-button via `ClearValue` (never a null
    assignment) so the page-title style's theme foreground applies and the header
    text stays visible on the neutral bar.

## Validation

- Build the x64 Debug configuration to confirm all XAML resolves the new brush keys.
  Status: complete — build succeeds with 0 warnings / 0 errors.
- Manually verify (or verify via WinApp if available) each module page shows its accent
  in the branded header and emphasis elements in both light and dark themes.
  Status: pending manual visual check.
- Toggle light/dark theme (Settings → Theme) on a module page and confirm the user-card
  and initials-circle accent borders stay visible in both modes.
  Status: pending manual visual check.

## Out of Scope

- Retemplating standard controls (e.g., changing `AccentButtonStyle`).
- Changing the Reporting HTML output colors or the Reprint card colors (already branded).
- Changing semantic/state colors in the Volvo converters.
- Changing the shared scroll/viewer or DataGrid styles in `App.xaml`.
