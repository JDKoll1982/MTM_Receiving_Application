# Icon Mapping Strategy Checklist: Material Icons Migration (Phase 3)

**Purpose**: Create comprehensive mapping from Segoe Fluent icons to Material Design equivalents
**Created**: 2025-12-29
**Feature**: [Icon Migration Plan](plan.md)
**Phase**: 3 of 9 - Icon Mapping Strategy

---

## Icon Mapping Document Creation

- [x] CHK013-01 Create specs/011-material-icons-migration/icon-mapping.md file
- [x] CHK013-02 Add header with mapping table format (Glyph, Usage, Material Kind, Notes columns)
- [x] CHK013-03 Add Material Design icon browser reference: https://pictogrammers.com/library/mdi/

---

## IconPickerControl Glyph Mapping (18 Icons)

Map all 18 hardcoded icons from IconPickerControl.xaml:

- [x] CHK014-01 &#xF133; (Box) → MaterialIconKind.Package or MaterialIconKind.PackageVariantClosed
- [x] CHK014-02 &#xE7B8; (Package) → MaterialIconKind.PackageVariant or MaterialIconKind.Package
- [x] CHK014-03 &#xF158; (Cube) → MaterialIconKind.CubeOutline or MaterialIconKind.Cube
- [x] CHK014-04 &#xE8B7; (Folder) → MaterialIconKind.Folder or MaterialIconKind.FolderOutline
- [x] CHK014-05 &#xE787; (Calendar) → MaterialIconKind.Calendar or MaterialIconKind.CalendarBlank
- [x] CHK014-06 &#xE715; (Mail) → MaterialIconKind.Email or MaterialIconKind.EmailOutline
- [x] CHK014-07 &#xE7C1; (Flag) → MaterialIconKind.Flag or MaterialIconKind.FlagOutline
- [x] CHK014-08 &#xE734; (Star) → MaterialIconKind.Star or MaterialIconKind.StarOutline
- [x] CHK014-09 &#xEB51; (Heart) → MaterialIconKind.Heart or MaterialIconKind.HeartOutline
- [x] CHK014-10 &#xE718; (Pin) → MaterialIconKind.Pin or MaterialIconKind.PinOutline
- [x] CHK014-11 &#xE8EC; (Tag) → MaterialIconKind.Tag or MaterialIconKind.TagOutline
- [x] CHK014-12 &#xE8C9; (Important) → MaterialIconKind.Alert or MaterialIconKind.AlertCircle
- [x] CHK014-13 &#xE7BA; (Warning) → MaterialIconKind.AlertCircle or MaterialIconKind.AlertCircleOutline
- [x] CHK014-14 &#xE946; (Info) → MaterialIconKind.Information or MaterialIconKind.InformationOutline
- [x] CHK014-15 &#xE8A5; (Document) → MaterialIconKind.FileDocument or MaterialIconKind.FileDocumentOutline
- [x] CHK014-16 &#xEB9F; (Image) → MaterialIconKind.Image or MaterialIconKind.ImageOutline
- [x] CHK014-17 &#xEC4F; (Music) → MaterialIconKind.Music or MaterialIconKind.MusicNote
- [x] CHK014-18 &#xE714; (Video) → MaterialIconKind.Video or MaterialIconKind.VideoOutline

---

## Admin UI Card Icons Mapping (4 Icons)

Map navigation card icons from Dunnage_AdminMainView.xaml:

- [x] CHK015-01 &#xE7C3; (Manage Types) → MaterialIconKind.ShapeOutline or MaterialIconKind.Shape
- [x] CHK015-02 &#xE8FD; (Manage Specs) → MaterialIconKind.FormatListBulleted or MaterialIconKind.ClipboardListOutline
- [x] CHK015-03 &#xE8F1; (Manage Parts) → MaterialIconKind.CubeOutline or MaterialIconKind.PackageVariant
- [x] CHK015-04 &#xE8EF; (Inventoried List) → MaterialIconKind.ClipboardList or MaterialIconKind.ViewList

---

## Toolbar Action Icons Mapping

Map toolbar button icons found across Edit/Manual Entry views:

- [x] CHK016-01 &#xE710; (Add) → MaterialIconKind.Plus or MaterialIconKind.PlusCircle
- [x] CHK016-02 &#xE74D; (Remove) → MaterialIconKind.Minus or MaterialIconKind.Delete
- [x] CHK016-03 &#xE74C; (History) → MaterialIconKind.History or MaterialIconKind.ClockOutline
- [x] CHK016-04 &#xE8B3; (Select All) → MaterialIconKind.SelectAll or MaterialIconKind.CheckboxMultipleMarked
- [x] CHK016-05 &#xE74E; (Save/AutoFill) → MaterialIconKind.ContentSave or MaterialIconKind.Check
- [x] CHK016-06 &#xE8B7; (Fill Spaces) → MaterialIconKind.FolderDownload or MaterialIconKind.FormatLineWeight
- [x] CHK016-07 &#xE8CB; (Sort) → MaterialIconKind.Sort or MaterialIconKind.SortAlphabeticalAscending
- [x] CHK016-08 &#xE72C; (Refresh/Reload) → MaterialIconKind.Refresh or MaterialIconKind.Reload
- [x] CHK016-09 &#xE711; (Cancel) → MaterialIconKind.Close or MaterialIconKind.Cancel

---

## Pagination Icons Mapping

Map pagination control icons:

- [x] CHK017-01 &#xE892; (First Page) → MaterialIconKind.ChevronDoubleLeft or MaterialIconKind.PageFirst
- [x] CHK017-02 &#xE76B; (Previous) → MaterialIconKind.ChevronLeft or MaterialIconKind.ArrowLeft
- [x] CHK017-03 &#xE76C; (Next) → MaterialIconKind.ChevronRight or MaterialIconKind.ArrowRight
- [x] CHK017-04 &#xE893; (Last Page) → MaterialIconKind.ChevronDoubleRight or MaterialIconKind.PageLast

---

## Mode Selection Icons Mapping

Map workflow mode selection icons:

- [x] CHK018-01 &#xE81E; (Guided Wizard - Dunnage) → MaterialIconKind.WizardHat or MaterialIconKind.Navigation
- [x] CHK018-02 &#xE771; (Guided Wizard - Receiving) → MaterialIconKind.WizardHat or MaterialIconKind.Navigation
- [x] CHK018-03 &#xE8F0; (Manual Entry - Dunnage) → MaterialIconKind.Grid or MaterialIconKind.TableLarge
- [x] CHK018-04 &#xE7F0; (Manual Entry - Receiving) → MaterialIconKind.Grid or MaterialIconKind.TableLarge
- [x] CHK018-05 &#xE70F; (Edit Mode) → MaterialIconKind.Pencil or MaterialIconKind.FileEdit

---

## Data Entry Field Icons Mapping

Map data entry helper icons:

- [x] CHK019-01 &#xE8A5; (PO Number) → MaterialIconKind.FileDocument or MaterialIconKind.Clipboard
- [x] CHK019-02 &#xE8B7; (Part ID) → MaterialIconKind.Identifier or MaterialIconKind.Barcode
- [x] CHK019-03 &#xE7B8; (Package/Load) → MaterialIconKind.Package or MaterialIconKind.PackageVariant
- [x] CHK019-04 &#xE9F9; (Quantity) → MaterialIconKind.Counter or MaterialIconKind.Numeric
- [ ] CHK019-05 &#xEA8B; (Weight) → MaterialIconKind.Weight or MaterialIconKind.ScaleBalance
- [ ] CHK019-06 &#xE8B4; (Heat/Lot) → MaterialIconKind.FireCircle or MaterialIconKind.NumericBox
- [ ] CHK019-07 &#xE8C9; (Status) → MaterialIconKind.InformationOutline or MaterialIconKind.Alert
- [ ] CHK019-08 &#xE896; (Load) → MaterialIconKind.Download or MaterialIconKind.CloudDownload
- [ ] CHK019-09 &#xE721; (Lookup) → MaterialIconKind.Magnify or MaterialIconKind.Search

---

## User Profile Icons Mapping

Map user setup dialog icons (Shared_NewUserSetupDialog.xaml):

- [ ] CHK020-01 &#xE77B; (User Profile) → MaterialIconKind.Account or MaterialIconKind.AccountCircle
- [ ] CHK020-02 &#xE8D4; (Employee Number) → MaterialIconKind.Badge or MaterialIconKind.Identifier
- [ ] CHK020-03 &#xE910; (Windows Username) → MaterialIconKind.Windows or MaterialIconKind.Laptop
- [ ] CHK020-04 &#xE716; (Department) → MaterialIconKind.Domain or MaterialIconKind.OfficeBuildingOutline
- [ ] CHK020-05 &#xE823; (Shift) → MaterialIconKind.ClockOutline or MaterialIconKind.CalendarClock
- [ ] CHK020-06 &#xE70F; (Custom Entry) → MaterialIconKind.Pencil or MaterialIconKind.FormTextbox
- [ ] CHK020-07 &#xE72E; (PIN) → MaterialIconKind.LockOutline or MaterialIconKind.KeyVariant
- [ ] CHK020-08 &#xE8C9; (Confirm) → MaterialIconKind.CheckCircle or MaterialIconKind.ShieldCheck
- [ ] CHK020-09 &#xE89F; (ERP Access) → MaterialIconKind.Database or MaterialIconKind.ServerNetwork

---

## Status & Feedback Icons Mapping

Map status indicator icons:

- [ ] CHK021-01 &#xE930; (Success) → MaterialIconKind.CheckCircle or MaterialIconKind.Check
- [ ] CHK021-02 &#xE783; (Error/Failed) → MaterialIconKind.CloseCircle or MaterialIconKind.AlertCircle
- [ ] CHK021-03 &#xE707; (Placeholder/Coming Soon) → MaterialIconKind.ProgressClock or MaterialIconKind.ClockOutline
- [ ] CHK021-04 &#xE73A; (Quick Select) → MaterialIconKind.LightningBolt or MaterialIconKind.Flash
- [ ] CHK021-05 &#xE8A9; (Single View) → MaterialIconKind.FileOutline or MaterialIconKind.CardOutline

---

## Special Cases Mapping

Icons requiring design review or multiple options:

- [ ] CHK022-01 &#xE76F; (Dialog icon - unknown context) → Review usage, map to MaterialIconKind.HelpCircle or context-specific
- [ ] CHK022-02 &#xE8A7; (Memory - Receiving_EditModeView) → MaterialIconKind.Memory or MaterialIconKind.Chip
- [ ] CHK022-03 &#xE8E5; (Current Labels) → MaterialIconKind.Label or MaterialIconKind.TagMultiple
- [ ] CHK022-04 Custom FontFamily icons (FluentSystemIcons.ttf) → Verify Material equivalent exists

---

## Visual Comparison Documentation

- [ ] CHK023-01 Create screenshots folder: specs/011-material-icons-migration/screenshots/
- [ ] CHK023-02 Screenshot current Fluent icons in IconPickerControl (before)
- [ ] CHK023-03 Screenshot current admin navigation cards (before)
- [ ] CHK023-04 Screenshot current toolbar buttons (before)
- [ ] CHK023-05 Add screenshots to icon-mapping.md as visual reference

---

## Size Compatibility Check

Document standard sizes and Material equivalents:

- [ ] CHK024-01 Map FontSize="12" → Width="12" Height="12"
- [ ] CHK024-02 Map FontSize="14" → Width="14" Height="14"
- [ ] CHK024-03 Map FontSize="16" → Width="16" Height="16"
- [ ] CHK024-04 Map FontSize="20" → Width="20" Height="20"
- [ ] CHK024-05 Map FontSize="24" → Width="24" Height="24"
- [ ] CHK024-06 Map FontSize="32" → Width="32" Height="32"
- [ ] CHK024-07 Map FontSize="40" → Width="40" Height="40"
- [ ] CHK024-08 Map FontSize="48" → Width="48" Height="48"
- [ ] CHK024-09 Map FontSize="64" → Width="64" Height="64"
- [ ] CHK024-10 Map FontSize="72" → Width="72" Height="72"

---

## Color Compatibility Check

Document theme resource compatibility:

- [ ] CHK025-01 Verify AccentFillColorDefaultBrush works with MaterialIcon Foreground
- [ ] CHK025-02 Verify SystemFillColorCautionBrush works with MaterialIcon Foreground
- [ ] CHK025-03 Verify SystemFillColorCriticalBrush works with MaterialIcon Foreground
- [ ] CHK025-04 Verify SystemFillColorSuccessBrush works with MaterialIcon Foreground
- [ ] CHK025-05 Test Light/Dark theme compatibility with Material icons

---

## Completion Criteria

- [ ] COMPLETE All 60+ unique glyph codes mapped to Material equivalents
- [ ] COMPLETE icon-mapping.md created with full table
- [ ] COMPLETE Visual comparison screenshots captured
- [ ] COMPLETE Size compatibility documented
- [ ] COMPLETE Color compatibility verified
- [ ] COMPLETE Design review completed for ambiguous icons

---

## Estimated Time

**1 hour**

---

## Notes

- Total unique glyphs identified: **60+**
- Some Fluent icons have multiple Material equivalents (outlined vs filled)
- **Preference**: Use outlined variants for consistency with WinUI 3 design language
- Custom FluentSystemIcons.ttf usage found - requires verification Material equivalent exists

---

## Next Phase

Once icon mapping is complete, proceed to [checklist-04-icon-picker-refactor.md](checklist-04-icon-picker-refactor.md)
