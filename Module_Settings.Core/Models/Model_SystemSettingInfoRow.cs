namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Read-only row displayed in the System Settings page for non-editable values.
/// </summary>
public sealed record Model_SystemSettingInfoRow(
    string DisplayName,
    string Key,
    string Value,
    string Notes
);
