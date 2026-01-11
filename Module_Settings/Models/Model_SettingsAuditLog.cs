using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents an audit log entry for settings changes
/// Database Table: settings_audit_log
/// </summary>
public partial class Model_SettingsAuditLog : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _settingId;

    [ObservableProperty]
    private int? _userSettingId;

    [ObservableProperty]
    private string? _oldValue;

    [ObservableProperty]
    private string? _newValue;

    [ObservableProperty]
    private string _changeType = "update";

    [ObservableProperty]
    private int _changedBy;

    [ObservableProperty]
    private DateTime _changedAt;

    [ObservableProperty]
    private string? _ipAddress;

    [ObservableProperty]
    private string? _workstationName;

    // Navigation properties
    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _settingKey = string.Empty;

    [ObservableProperty]
    private string _settingName = string.Empty;
}
