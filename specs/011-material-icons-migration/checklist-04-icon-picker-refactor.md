# IconPickerControl Refactor Checklist: Material Icons Migration (Phase 4)

**Purpose**: Refactor IconPickerControl to use Material.Icons.WinUI3, enabling search and selection of all available icons.
**Created**: 2025-12-29
**Feature**: [Icon Migration Plan](plan.md)
**Phase**: 4 of 9 - IconPickerControl Refactor

---

## Model Updates

- [x] CHK020-01 Analyze `Models/Dunnage/Model_IconDefinition.cs`
- [x] CHK020-02 Update `Model_IconDefinition` to use `MaterialIconKind` instead of string Glyph
- [x] CHK020-03 Add `MaterialIconKind` property to `Model_IconDefinition`
- [x] CHK020-04 Remove legacy `Glyph` property
- [x] CHK020-05 Create helper method/service to generate list of all `MaterialIconKind` values

---

## IconPickerControl Logic Refactor

- [x] CHK021-01 Open `Views/Dunnage/Controls/IconPickerControl.xaml.cs`
- [x] CHK021-02 Change `SelectedIcon` DependencyProperty type from `string` to `MaterialIconKind?` (or nullable enum)
- [x] CHK021-03 Update `RecentlyUsedIcons` collection type if necessary
- [x] CHK021-04 Implement `LoadIcons()` method to populate the grid with all available Material Icons
- [x] CHK021-05 Implement `OnSearchTextChanged` to filter the icon list based on name/alias
- [x] CHK021-06 Implement `OnIconClick` handler to update `SelectedIcon`

---

## IconPickerControl XAML Refactor

- [x] CHK022-01 Open `Views/Dunnage/Controls/IconPickerControl.xaml`
- [x] CHK022-02 Add `xmlns:materialIcons="using:Material.Icons.WinUI3"` namespace
- [x] CHK022-03 Remove **ALL** hardcoded `Border`/`FontIcon` elements (lines 47-98)
- [x] CHK022-04 Replace `ScrollViewer`/`StackPanel` with `GridView`
- [x] CHK022-05 Define `GridView.ItemTemplate` containing `materialIcons:MaterialIcon`
- [x] CHK022-06 Bind `GridView.ItemsSource` to the filtered icon list
- [x] CHK022-07 Bind `SelectedItem` to `SelectedIcon` (TwoWay)
- [x] CHK022-08 Style the `GridViewItem` for proper sizing and hover effects

---

## Parent Integration Updates

- [x] CHK023-01 Open `ViewModels/Dunnage/Dunnage_AddTypeDialogViewModel.cs`
- [x] CHK023-02 Update `SelectedIcon` property to use `MaterialIconKind`
- [x] CHK023-03 Update default value to `MaterialIconKind.PackageVariantClosed`
- [x] CHK023-04 Ensure `RecentlyUsedIcons` are loaded correctly (Model update handles legacy mapping)
- [x] CHK023-05 Update `SaveAsync` to convert `SelectedIcon` to string
- [x] CHK024-01 Open `Views/Shared/Shared_IconSelectorWindow.xaml` and `.cs`
- [x] CHK024-02 Refactor to use `MaterialIconKind` and `MaterialIcon` control
- [x] CHK024-03 Remove file-based icon loading and use `Enum.GetValues`
- [x] CHK024-04 Update `Dunnage_QuickAddTypeDialog.xaml` and `.cs` to use `MaterialIconKind`

---

## Persistence & Recent Icons

- [x] CHK024-01 Update logic for saving "Recently Used" icons
- [x] CHK024-02 Ensure recent icons are stored as `MaterialIconKind` names (strings) in settings/database if applicable
- [x] CHK024-03 Verify loading of recent icons works with new enum type
- [x] CHK024-05 Create and run migration script `011_migrate_icons_to_material.sql`
- [x] CHK024-06 Deploy `sp_user_preferences_get_recent_icons.sql`

---

## Constitution Compliance Review

- [x] CONST008 No hardcoded icon lists (use Enum.GetValues)
- [x] CONST009 Search is performant (virtualization if needed)
- [x] CONST010 UI matches existing styling (colors, spacing)

---

## Completion Criteria

- [x] COMPLETE IconPickerControl displays Material Icons
- [x] COMPLETE Search functionality works for all 5000+ icons
- [x] COMPLETE Selection updates the bound property
- [x] COMPLETE Parent dialogs can receive the selected icon
