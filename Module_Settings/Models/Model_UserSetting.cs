using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a user-specific override for a system setting
/// Database Table: user_settings
/// </summary>
public partial class Model_UserSetting : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _userId;

    [ObservableProperty]
    private int _settingId;

    [ObservableProperty]
    private string? _settingValue;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _updatedAt;

    // Navigation property
    [ObservableProperty]
    private Model_SystemSetting? _systemSetting;
}
