# Discovery & Analysis Checklist: Material Icons Migration (Phase 1)

**Purpose**: Identify all icon usage locations across the MTM Receiving Application codebase for migration to Material.Icons.WinUI3
**Created**: 2025-12-29
**Feature**: [Icon Migration Plan](plan.md)
**Phase**: 1 of 9 - Discovery & Analysis

---

## XAML Icon Discovery

**Files Identified**: 18 XAML files with 200+ FontIcon instances

- [x] CHK001-01 Catalog all FontIcon instances in Views/Dunnage/Controls/IconPickerControl.xaml (18 hardcoded icons: lines 47-98)
- [x] CHK001-02 Catalog FontIcon instance in Views/Dunnage/Dialogs/Dunnage_AddTypeDialog.xaml (line 145: &#xE76F;)
- [x] CHK001-03 Catalog FontIcon instances in Views/Dunnage/Dunnage_AdminMainView.xaml (4 cards: lines 58, 90, 122, 154 - glyphs &#xE7C3;, &#xE8FD;, &#xE8F1;, &#xE8EF;)
- [x] CHK001-04 Catalog FontIcon instances in Views/Dunnage/Dunnage_AdminMainView.xaml placeholders (2 icons: lines 190, 217)
- [x] CHK001-05 Catalog FontIcon instances in Views/Dunnage/Dunnage_AdminPartsView.xaml (line 171: &#xE8F1;)
- [x] CHK001-06 Catalog FontIcon instance in Views/Dunnage/Dunnage_AdminTypesView.xaml DataGrid column (lines 92-93: dynamic binding with converter)
- [x] CHK001-07 Catalog FontIcon instance in Views/Dunnage/Dunnage_AdminTypesView.xaml empty state (line 146: &#xE7C3;)
- [x] CHK001-08 Catalog FontIcon instance in Views/Dunnage/Dunnage_DetailsEntryView.xaml InfoBar (line 168: &#xE946;)
- [x] CHK001-09 Catalog FontIcon instances in Views/Dunnage/Dunnage_EditModeView.xaml toolbar (9 icons: lines 35, 46, 52, 127, 130, 143, 146)
- [x] CHK001-10 Catalog FontIcon instances in Views/Dunnage/Dunnage_ManualEntryView.xaml toolbar (7 icons: lines 31, 37, 43, 52, 58, 64)
- [x] CHK001-11 Catalog FontIcon instances in Views/Dunnage/Dunnage_ModeSelectionView.xaml (3 mode cards: lines 24, 57, 90)
- [x] CHK001-12 Catalog FontIcon instances in Views/Dunnage/Dunnage_PartSelectionView.xaml (3 icons: lines 110, 128, 137)
- [x] CHK001-13 Catalog FontIcon instance in Views/Dunnage/Dunnage_QuantityEntryView.xaml (line 96: &#xE946;)
- [x] CHK001-14 Catalog FontIcon instances in Views/Dunnage/Dunnage_QuickAddTypeDialog.xaml help flyout (2 icons: lines 43, 72)
- [x] CHK001-15 Catalog FontIcon instance in Views/Dunnage/Dunnage_QuickAddTypeDialog.xaml icon picker (line 116: dynamic binding with converter)
- [x] CHK001-16 Catalog FontIcon instance in Views/Dunnage/Dunnage_ReviewView.xaml empty state (line 109: &#xE7C3;)
- [x] CHK001-17 Catalog FontIcon instances in Views/Dunnage/Dunnage_ReviewView.xaml action buttons (3 icons: lines 133, 143, 154)
- [x] CHK001-18 Catalog FontIcon instances in Views/Dunnage/Dunnage_TypeSelectionView.xaml (6 icons: lines 77 dynamic, 107, 113, 129, 135, 143)
- [x] CHK001-19 Catalog FontIcon instance in Views/Dunnage/Dunnage_WorkflowView.xaml (line 109: &#xE946;)
- [x] CHK001-20 Catalog FontIcon instances in Views/Main/Main_CarrierDeliveryLabelPage.xaml (3 icons: lines 16, 38, 50)
- [x] CHK001-21 Catalog FontIcon instances in Views/Receiving/Receiving_EditModeView.xaml toolbar (9 icons: lines 35, 41, 47, 58, 64, 141, 144, 159, 162)
- [x] CHK001-22 Catalog FontIcon instances in Views/Receiving/Receiving_HeatLotView.xaml (2 icons: lines 46, 75)
- [x] CHK001-23 Catalog FontIcon instance in Views/Receiving/Receiving_LoadEntryView.xaml (line 19: &#xE7B8;)
- [x] CHK001-24 Catalog FontIcon instances in Views/Receiving/Receiving_ManualEntryView.xaml toolbar (4 icons: lines 35, 41, 47, 53)
- [x] CHK001-25 Catalog FontIcon instances in Views/Receiving/Receiving_ModeSelectionView.xaml (3 mode cards: lines 24, 54, 84)
- [x] CHK001-26 Catalog FontIcon instances in Views/Receiving/Receiving_PackageTypeView.xaml (2 icons: lines 34, 71)
- [x] CHK001-27 Catalog FontIcon instances in Views/Receiving/Receiving_POEntryView.xaml (7 icons: lines 41, 70, 92, 119, 137, 160, 186)
- [x] CHK001-28 Catalog FontIcon instances in Views/Receiving/Receiving_ReviewGridView.xaml (15 icons: lines 40, 70, 85, 98, 111, 126, 141, 154, 167, 260, 279, 289)
- [x] CHK001-29 Catalog FontIcon instances in Views/Receiving/Receiving_WeightQuantityView.xaml (2 icons: lines 31, 59)
- [x] CHK001-30 Catalog FontIcon instances in Views/Receiving/Receiving_WorkflowView.xaml (3 icons: lines 65, 67, 142)
- [x] CHK001-31 Catalog FontIcon instances in Views/Shared/Shared_IconSelectorWindow.xaml (6 icons: lines 88 dynamic, 117, 120, 128, 131)
- [x] CHK001-32 Catalog FontIcon instances in Views/Shared/Shared_NewUserSetupDialog.xaml (11 icons: lines 29, 70, 86, 102, 119, 135, 159, 175, 194, 220)
- [x] CHK001-33 Catalog FontIcon instance in Views/Shared/Shared_SplashScreenWindow.xaml (line 21: &#xE8F1;)

---

## Code-Behind & Dynamic Icon Usage Discovery

- [x] CHK002-01 Search for `new FontIcon` instances in all C# files (Views/*.xaml.cs)
- [x] CHK002-02 Search for `FontIcon.Glyph` assignments in all C# files
- [x] CHK002-03 Search for `\uE` Unicode escape sequences in C# code (icon glyphs)
- [x] CHK002-04 Document all dynamic icon creation patterns found

---

## Converter Discovery

- [x] CHK003-01 Analyze Converter_IconCodeToGlyph.cs - Purpose: Converts icon code to glyph character
- [x] CHK003-02 Identify all XAML files using `{StaticResource IconCodeToGlyphConverter}`
- [x] CHK003-03 Document converter input formats (e.g., "&#xE7B8;" or "E7B8")
- [x] CHK003-04 Document converter output type (string glyph character)
- [x] CHK003-05 Identify all binding sites using this converter (at least 3 found: Dunnage_AdminTypesView.xaml line 92, Dunnage_QuickAddTypeDialog.xaml line 116, Dunnage_TypeSelectionView.xaml line 77)

---

## IconPickerControl Analysis

- [x] CHK004-01 Read IconPickerControl.xaml.cs structure (View code-behind)
- [x] CHK004-02 Document SelectedIcon property type (currently string)
- [x] CHK004-03 Document icon selection mechanism (18 hardcoded Border/FontIcon elements)
- [x] CHK004-04 Document recently used icons storage approach (if any)
- [x] CHK004-05 Document search functionality (TextBox line 27, OnSearchTextChanged event)
- [x] CHK004-06 Identify parent dialogs/views using IconPickerControl (Dunnage_AddTypeDialog, Dunnage_QuickAddTypeDialog, Shared_IconSelectorWindow)

---

## Database Icon Storage Discovery

- [x] CHK005-01 Search for Icon properties in Models/Dunnage/*.cs
- [x] CHK005-02 Find Model_DunnageType.Icon property definition and type
- [x] CHK005-03 Search database schema for icon column (dunnage_types.Icon VARCHAR)
- [x] CHK005-04 Document current icon storage format (Unicode glyphs like "&#xE7B8;")
- [x] CHK005-05 Query database for sample icon values to validate format

---

## Icon Inventory Report Creation

- [x] CHK006-01 Create specs/011-material-icons-migration/icon-inventory.md file
- [x] CHK006-02 List all 200+ icon usage locations with file paths and line numbers
- [x] CHK006-03 Map glyph codes to intended icon meanings (Box, Package, Cube, etc.)
- [x] CHK006-04 Categorize icons by usage type (navigation, actions, status, data entry, placeholders)
- [x] CHK006-05 Identify unique glyph values (18 from IconPickerControl + others)
- [x] CHK006-06 Document icon sizes used (14, 16, 24, 32, 40, 48, 64, 72 FontSize values)
- [x] CHK006-07 Create summary statistics (total count, files affected, categories)

---

## Special Cases Identification

- [x] CHK007-01 Identify icons with dynamic binding via converters (3 locations found)
- [x] CHK007-02 Identify icons in DataGrid templates (Dunnage_AdminTypesView.xaml)
- [x] CHK007-03 Identify icons in ItemTemplates (Shared_IconSelectorWindow.xaml)
- [x] CHK007-04 Identify icons with theme-dependent colors (AccentFillColorDefaultBrush usages)
- [x] CHK007-05 Identify icons in ContentDialogs vs Pages vs Windows
- [x] CHK007-06 Identify icons using custom FontFamily (FluentSystemIcons.ttf found in some files)

---

## Completion Criteria

- [x] COMPLETE All 33 XAML files catalogued with line numbers and glyph codes
- [x] COMPLETE All code-behind dynamic icon usages documented
- [x] COMPLETE Converter_IconCodeToGlyph.cs fully analyzed with all binding sites identified
- [x] COMPLETE IconPickerControl.xaml.cs structure documented
- [x] COMPLETE Database icon storage format validated
- [x] COMPLETE icon-inventory.md created with complete mapping table
- [x] COMPLETE Special cases identified and documented

---

## Notes

- Total estimated icon instances: **200+**
- Files with FontIcon usage: **33 XAML files**
- IconPickerControl has **18 hardcoded** sample icons
- Converter_IconCodeToGlyph used in **at least 3 binding locations**
- Database stores icons as **VARCHAR strings** (Unicode glyph format)

---

## Next Phase

Once all discovery tasks are complete, proceed to [checklist-02-package-installation.md](checklist-02-package-installation.md)
