using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// One row on the Scanner access page: an application user plus that user's allow-list state.
/// </summary>
public partial class ViewModel_SettingsScannerAccessRow : ObservableObject
{
    public ViewModel_SettingsScannerAccessRow(Model_User user, bool isAllowed)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        _isAllowed = isAllowed;
    }

    public Model_User User { get; }

    public int EmployeeNumber => User.EmployeeNumber;

    public string FullName => User.FullName;

    public string WindowsUsername => User.WindowsUsername;

    public string Department => User.Department;

    public bool IsActive => User.IsActive;

    public string StatusText => IsActive ? "Active" : "Inactive";

    /// <summary>True when this user is on the plant-wide Scanner allow-list.</summary>
    [ObservableProperty]
    private bool _isAllowed;
}
