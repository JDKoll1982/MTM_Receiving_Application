# Things To Change

---

## Module_Receiving → Enter Load Information

**View:** `Module_Receiving/Views/View_Receiving_LoadEntry.xaml`
**ViewModel:** `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`

### 1. Location Entry — Add LostFocus Validation

The live-mode `TextBox` for Location (View lines 66–81) is editable but has no validation on focus-lost.

- Add a `LostFocus` code-behind handler (pattern: `View_Receiving_ManualEntry.xaml.cs`
  `LocationTextBox_LostFocus()` at line 457).
- On lost focus: call `IService_InforVisual.LocationExistsAsync(location, warehouseCode)` via
  `IService_ReceivingValidation.ValidateLocationAsync()` (`Module_Receiving/Services/Service_ReceivingValidation.cs` line 130).
- If validation fails: call `IService_InforVisual.FuzzySearchLocationsAsync(term, warehouseCode)`
  (`Module_Core/Contracts/Services/IService_InforVisual.cs` line 107) and surface the closest matches
  to the user (suggestion list or info bar).
- Both `LocationExistsAsync` (line 127) and `FuzzySearchLocationsAsync` (line 107) already exist in
  `IService_InforVisual.cs`; the concrete implementations are in
  `Module_Core/Services/Database/Service_InforVisualConnect.cs` (lines 609 and 575 respectively).

### 2. Default Location "RECV" When Left Blank

- If `Location` is blank when the user advances, apply the configured default location (target value: `"RECV"`).
- Add `DefaultLocation` to `ReceivingSettingsKeys.Defaults` in
  `Module_Receiving/Settings/ReceivingSettingsKeys.cs` (inside the `Defaults` static class, after line 281).
  Key string: `"Receiving.Defaults.DefaultLocation"`.
- Add the default value `"RECV"` to `ReceivingSettingsDefaults.StringDefaults` in
  `Module_Receiving/Settings/ReceivingSettingsDefaults.cs` (near line 241).
- Expose as a settable field in **Settings → Receiving Navigation → Defaults**
  (`Module_Settings.Receiving/Views/View_Settings_Receiving_Defaults.xaml` /
  `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Defaults.cs`).
- The `Location` observable property is `_location` (ViewModel line 35). The partial method
  `OnLocationChanged(string value)` (ViewModel line 149) already syncs to `_workflowService.CurrentLocation`.

---

## Module_Settings.Core → Settings Window Back Button

**View:** `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml`
**Code-behind:** `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`

- `BackToHubButton` already exists at View lines 61–64 (`x:Name="BackToHubButton"`, `Content="Back to Hub"`,
  `Click="OnBackToHubClicked"`) but is set to `Visibility="Collapsed"`.
- `OnBackToHubClicked()` handler is already implemented at code-behind line 324.
- What is missing: toggling visibility.  Show the button when the `SettingsFrame` navigates to any
  non-hub child page; hide it when on a hub page (e.g., `View_Settings_Receiving_WorkflowHub`,
  `View_Settings_CoreNavigationHub`).
- `UpdateHeaderActions()` (called from `OnBackToHubClicked`) is the correct hook for resetting header
  state after navigation.

---

## Module_Settings.Receiving → Overview: Remove This Page

**View:** `Module_Settings.Receiving/Views/View_Settings_Receiving_SettingsOverview.xaml`
**ViewModel:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_SettingsOverview.cs`

Remove:

1. Step registration at `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_NavigationHub.cs`
   line 27:
   `new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Receiving_SettingsOverview))`
2. DI registrations in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`:
   - `ViewModel_Settings_Receiving_SettingsOverview` (line 361)
   - `View_Settings_Receiving_SettingsOverview` (line ~438)
3. Delete both `View_Settings_Receiving_SettingsOverview.xaml` and
   `ViewModel_Settings_Receiving_SettingsOverview.cs`.

---

## Module_Settings.Receiving → Receiving Validation

**View:** `Module_Settings.Receiving/Views/View_Settings_Receiving_Validation.xaml`
**ViewModel:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Validation.cs`

### 1. Wire These Settings Into Module_Receiving

All toggles and limit fields below already have UI elements and ViewModel properties.  Wire each to its
corresponding check in `Module_Receiving/Services/Service_ReceivingValidation.cs` via
`IService_ReceivingSettings` (`ReceivingSettingsKeys.Validation.*`):

| ViewModel Property | XAML Line | Settings Key | Where to Apply |
|---|---|---|---|
| `RequirePoNumber` | 33 | `ReceivingSettingsKeys.Validation.RequirePoNumber` | `Service_ReceivingValidation.cs` |
| `RequireQuantity` | 38 | `ReceivingSettingsKeys.Validation.RequireQuantity` | `Service_ReceivingValidation.cs` |
| `AllowNegativeQuantity` | 43 | `ReceivingSettingsKeys.Validation.AllowNegativeQuantity` | `Service_ReceivingValidation.cs` |
| `ValidatePartExists` | 48 | `ReceivingSettingsKeys.Validation.ValidatePartExists` | `Service_ReceivingValidation.cs` |
| `WarnOnSameDayReceiving` | 53 | `ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving` | `Service_ReceivingValidation.cs` |
| `RequireHeatLot` | 40 | `ReceivingSettingsKeys.Validation.RequireHeatLot` | `Service_ReceivingValidation.cs` |
| `ValidatePoExists` | 45 | `ReceivingSettingsKeys.Validation.ValidatePoExists` | `Service_ReceivingValidation.cs` |
| `WarnOnQuantityExceedsPo` | 50 | `ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo` | `Service_ReceivingValidation.cs` |
| `MinLoadCount` / `MaxLoadCount` | 64–65 | `ReceivingSettingsKeys.Validation.Min/MaxLoadCount` | `Service_ReceivingValidation.cs` |
| `MinQuantity` / `MaxQuantity` | 68–69 | `ReceivingSettingsKeys.Validation.Min/MaxQuantity` | `Service_ReceivingValidation.cs` |

### 2. Remove "Require Part ID"

Part ID is always required; this setting should not exist.

- Remove `ToggleSwitch` for `RequirePartId` at `View_Settings_Receiving_Validation.xaml` line 35.
- Remove the `RequirePartId` `[ObservableProperty]` from `ViewModel_Settings_Receiving_Validation.cs`.
- Remove `ReceivingSettingsKeys.Validation.RequirePartId` from `ReceivingSettingsKeys.cs`.

---

## Module_Settings.Receiving → Preferences: Rename Nav Card

**File:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_NavigationHub.cs` line 30

Current:
```csharp
new Model_SettingsNavigationStep("Preferences", typeof(Views.View_Settings_Receiving_UserPreferences))
```

Change to:
```csharp
new Model_SettingsNavigationStep("Part Number Auto Padding", typeof(Views.View_Settings_Receiving_UserPreferences))
```

Also update the corresponding header mapping in `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`
`GetPageHeader()` if `View_Settings_Receiving_UserPreferences` has an entry there.

---

## Module_Settings.Receiving → Receiving Defaults: Remove Label Database Save Location

**View:** `Module_Settings.Receiving/Views/View_Settings_Receiving_Defaults.xaml`
**ViewModel:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Defaults.cs`

Remove from View (lines 38–56):

- `TextBlock` "Label Database Table Save Location" (line 39)
- Read-only `TextBox` bound to `ViewModel.LabelTableSaveLocation` (line 50)
- `Button` "Browse..." bound to `ViewModel.BrowseLabelTableLocationCommand` (lines 52–55)

Remove from ViewModel:

- `[ObservableProperty] private string _labelTableSaveLocation`
- `[RelayCommand] private void BrowseLabelTableLocation()`

> **Note:** Before removing `ReceivingSettingsKeys.Defaults.LabelTableSaveLocation`
> (`Module_Receiving/Settings/ReceivingSettingsKeys.cs` line 280) and its default entry in
> `ReceivingSettingsDefaults.cs` (line 242), verify no other code reads this key.

---

## Module_Settings.Receiving → Business Rules

**View:** `Module_Settings.Receiving/Views/View_Settings_Receiving_BusinessRules.xaml`
**ViewModel:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs`
**Nav Hub:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_NavigationHub.cs` line 30

### 1. Rename the Page to a User-Friendly Name

The Nav Hub step is currently `"Business Rules"` (NavigationHub.cs line 30).
The ViewModel sets `Title = "Receiving Business Rules"` (BusinessRules ViewModel line ~64).

- Rename to something meaningful to the end user, such as **"Workflow Behavior"** or **"Workflow Options"**.
- Update `Model_SettingsNavigationStep` title in `ViewModel_Settings_Receiving_NavigationHub.cs` line 30.
- Update `Title` assignment in `ViewModel_Settings_Receiving_BusinessRules.cs` (~line 64).
- Update any matching entry in `View_Settings_CoreWindow.xaml.cs` `GetPageHeader()`.
- Do **not** rename backing `ReceivingSettingsKeys.BusinessRules.*` constants — users never see those.

### 2. Remove the Storage Card

Remove the entire "Storage" `Border` card from the View.  It spans from where
`<TextBlock Text="Storage"` begins down through the closing `</Border>` — that section contains
these toggles (confirmed XAML):

| Property (ViewModel) | Header Text |
|---|---|
| `SaveToLabelTableEnabled` | "Save To Label Table" |
| `SaveToNetworkLabelTableEnabled` | "Save To Network Label Table" |
| `SaveToDatabaseEnabled` | "Save To Database" |

Remove the corresponding `[ObservableProperty]` declarations from
`ViewModel_Settings_Receiving_BusinessRules.cs`.

> **Note:** Verify no other code reads `ReceivingSettingsKeys.BusinessRules.SaveToLabelTableEnabled`,
> `SaveToNetworkLabelTableEnabled`, or `SaveToDatabaseEnabled` before removing those constants.

### 3. Default Mode — Change TextBox to ComboBox

The `TextBox` with `Header="Default Mode On Startup"` bound to `ViewModel.DefaultModeOnStartup` (View ~line 63)
should become a `ComboBox`.

- Current backing default: `"ModeSelection"` (`Module_Receiving/Settings/ReceivingSettingsDefaults.cs` line 259;
  settings key: `ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup` at
  `Module_Receiving/Settings/ReceivingSettingsKeys.cs` line 268).
- ComboBox options (display label → stored value from `Enum_ReceivingWorkflowStep`,
  `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`):

| Display Label (what user sees) | Stored Value |
|---|---|
| Mode Selection | `ModeSelection` |
| Guided Wizard | `Guided` |
| Manual Entry | `ManualEntry` |
| Edit Mode | `EditMode` |

- Add an `ObservableCollection<string>` of display labels and a mapping to stored values in the ViewModel,
  or use a converter.

### 4. Implement Workflow Toggle Settings Into Module_Receiving

Wire these existing ViewModel properties (already saved/loaded via `IService_ReceivingSettings`) into
Module_Receiving runtime behavior.  Read each via `IService_ReceivingSettings` using
`ReceivingSettingsKeys.BusinessRules.*`:

| ViewModel Property | Settings Key | Where to Apply in Module_Receiving |
|---|---|---|
| `RememberLastMode` | `ReceivingSettingsKeys.BusinessRules.RememberLastMode` | `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` |
| `ConfirmModeChange` | `ReceivingSettingsKeys.BusinessRules.ConfirmModeChange` | `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` |
| `AutoFillHeatLotEnabled` | `ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled` | `Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs` |
| `SavePackageTypeAsDefault` | `ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault` | `Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs` |
| `ShowReviewTableByDefault` | `ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault` | `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs` |
| `AllowEditAfterSave` | `ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave` | `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs` |

---

## Module_Settings.Receiving → ERP Integration: Remove This Page

**View:** `Module_Settings.Receiving/Views/View_Settings_Receiving_Integrations.xaml`
**ViewModel:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Integrations.cs`
**Nav Hub:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_NavigationHub.cs` line 31

Remove:

1. Step at NavigationHub.cs line 31:
   `new Model_SettingsNavigationStep("ERP Integration", typeof(Views.View_Settings_Receiving_Integrations))`
2. DI registrations in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`:
   - `ViewModel_Settings_Receiving_Integrations` (line 366)
   - `View_Settings_Receiving_Integrations` (line ~444)
3. Delete both `View_Settings_Receiving_Integrations.xaml` and
   `ViewModel_Settings_Receiving_Integrations.cs`.

> **Do NOT remove** any ERP service logic (e.g., `IService_InforVisual`,
> `Service_InforVisualConnect`) — other modules depend on those. Only remove what is exclusively
> wired to this settings page.

---

## Module_Receiving → Enter Weight & Quantity

**View:** `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`
**ViewModel:** `Module_Receiving/ViewModels/ViewModel_Receiving_WeightQuantity.cs`
**Model:** `Module_Receiving/Models/Model_ReceivingLoad.cs`

### 1. Weight/Quantity Fields Should Start Blank

- The `NumberBox` (View lines 115–122) currently has `Minimum="1"` causing `0` to display as `1` on load.
- `Model_ReceivingLoad._weightQuantity` defaults to `0` (Model line 32; type `decimal`).
- Change `Minimum="1"` → `Minimum="0"` (or remove it).
- Set the initial `Value` to `double.NaN` so WinUI3 `NumberBox` displays as blank.
  The `DoubleToDecimalConverter` bound at lines 115–122 will need to round-trip `NaN` correctly
  (return `0m` on conversion, skip write-back when source is `NaN`).

### 2. Add "Auto Fill" Button

Add a button that fills down the Weight/Quantity from the first populated row to all blank rows below
it — the same fill-down pattern used by the Heat/Lot Auto Fill.

- Reference button markup: `View_Receiving_HeatLot.xaml` line 20
  (`Command="{x:Bind ViewModel.AutoFillCommand}"`).
- Add `[RelayCommand] private void AutoFill()` to `ViewModel_Receiving_WeightQuantity.cs`,
  modeled on `ViewModel_Receiving_HeatLot.AutoFill()` (lines 118–129 in
  `Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs`):

  ```csharp
  for (int i = 1; i < Loads.Count; i++)
  {
      var currentLoad = Loads[i];
      var prevLoad    = Loads[i - 1];
      if (currentLoad.WeightQuantity == 0 && prevLoad.WeightQuantity != 0)
          currentLoad.WeightQuantity = prevLoad.WeightQuantity;
  }
  ```



