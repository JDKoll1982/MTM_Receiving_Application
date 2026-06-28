# VendorVariableMappingsJson Workflow

## Purpose
`ReceivingSettingsKeys.UserPreferences.VendorVariableMappingsJson` stores the vendor-to-UI-variable mapping list used by the receiving Heat/Lot workflow.

## Write Path
- The authoritative editor is `ViewModel_Settings_Receiving_UserPreferences`.
- It loads the JSON setting with `GetStringSettingAsync(ReceivingSettingsKeys.UserPreferences.VendorVariableMappingsJson)`.
- `DeserializeVendorVariableMappings(...)` turns blank or invalid JSON into a default mapping:
  - `VendorName = "Skana"`
  - `VariableName = "DM #"`
- Before display, the editor trims vendor and variable names, drops blank entries, de-duplicates by vendor name case-insensitively, and sorts by vendor name.
- Saving serializes `VendorVariableMappings.ToArray()` back to JSON and persists it through `SettingsCoreFacade.SetSettingAsync("Receiving", key, value, CurrentUserId)`.

## Read Path
- `ViewModel_Receiving_HeatLot` reads the same setting through `IService_ReceivingSettings.GetStringAsync(ReceivingSettingsKeys.UserPreferences.VendorVariableMappingsJson)`.
- Its local `DeserializeVendorVariableMappings(...)` uses the same `Skana -> DM #` default when the JSON is blank or invalid.
- The active mapping is selected by matching the current PO vendor name with `Model_ReceivingVendorVariableMapping.MatchesVendorName(...)`.
- If multiple mappings match, the viewmodel prefers the longest vendor name and then uses case-insensitive alphabetical ordering.

## Heat/Lot UI Behavior
- When no mapping matches, Heat/Lot hides the user-set variable field and resets per-load vendor-variable UI state.
- When a mapping matches, the viewmodel:
  - shows the user-set variable field,
  - sets the header text to the mapped variable name,
  - sets the placeholder to `Enter <VariableName>`,
  - updates accessibility text to the variable name,
  - clears `UserSetVariable` on loads that switch to a different vendor,
  - stamps each load with the active vendor name and field metadata.

## Model Behavior
- `Model_ReceivingVendorVariableMapping.DisplayName` renders `VendorName -> VariableName`.
- `MatchesVendorName(...)` performs a case-insensitive `Contains(...)` check, so vendor aliases and partial names can activate the mapping.

## Important Notes
- The receiving consumer does not pass an explicit user id when reading the setting.
- The settings page is the only writer found in the workspace for this JSON value.
- If the JSON is missing or malformed, the app falls back to the built-in `Skana / DM #` mapping instead of failing.
