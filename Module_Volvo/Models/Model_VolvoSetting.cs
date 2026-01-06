using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents a configurable setting for the Volvo module
/// Database Table: volvo_settings
/// </summary>
public partial class Model_VolvoSetting : ObservableObject
{
    [ObservableProperty]
    private string _settingKey = string.Empty;

    [ObservableProperty]
    private string _settingValue = string.Empty;

    [ObservableProperty]
    private string _settingType = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _defaultValue = string.Empty;

    [ObservableProperty]
    private int? _minValue;

    [ObservableProperty]
    private int? _maxValue;

    [ObservableProperty]
    private DateTime _modifiedDate = DateTime.Now;

    [ObservableProperty]
    private string? _modifiedBy;
}
