using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a system-wide configuration setting
/// Database Table: system_settings
/// </summary>
public partial class Model_SystemSetting : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string? _subCategory;

    [ObservableProperty]
    private string _settingKey = string.Empty;

    [ObservableProperty]
    private string _settingName = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _settingValue;

    [ObservableProperty]
    private string? _defaultValue;

    [ObservableProperty]
    private string _dataType = "string";

    [ObservableProperty]
    private string _scope = "system";

    [ObservableProperty]
    private string _permissionLevel = "admin";

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private bool _isSensitive;

    [ObservableProperty]
    private string? _validationRules; // JSON

    [ObservableProperty]
    private string _uiControlType = "textbox";

    [ObservableProperty]
    private int _uiOrder;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _updatedAt;

    [ObservableProperty]
    private int? _updatedBy;
}
