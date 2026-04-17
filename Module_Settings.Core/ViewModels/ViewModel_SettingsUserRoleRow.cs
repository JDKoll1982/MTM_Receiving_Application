using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// Row state for the Users and Privileges settings page.
/// </summary>
public partial class ViewModel_SettingsUserRoleRow : ObservableObject
{
    public required Model_User User { get; init; }

    [ObservableProperty]
    private string _currentRoleName = "User";

    [ObservableProperty]
    private string _selectedRoleName = "User";

    [ObservableProperty]
    private bool _canEditRole;

    public ObservableCollection<string> AvailableRoleNames { get; } = new();

    public string CurrentRoleDisplayName => $"Privilege: {CurrentRoleName}";

    public bool HasPendingRoleChange =>
        string.Equals(CurrentRoleName, SelectedRoleName, System.StringComparison.OrdinalIgnoreCase)
            is false;

    partial void OnSelectedRoleNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasPendingRoleChange));
    }

    partial void OnCurrentRoleNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasPendingRoleChange));
        OnPropertyChanged(nameof(CurrentRoleDisplayName));
    }
}
