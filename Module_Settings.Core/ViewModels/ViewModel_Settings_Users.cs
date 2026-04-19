using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for the Settings ▸ Users page.
/// Provides user list, add/edit form, deactivation, and Visual credential management.
/// </summary>
public partial class ViewModel_Settings_Users : ViewModel_Shared_Base
{
    private readonly Dao_User _daoUser;
    private readonly Dao_SettingsCoreRoles _daoSettingsRoles;
    private readonly Dao_SettingsCoreUserRoles _daoSettingsUserRoles;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;
    private Dictionary<string, Model_SettingsRole> _rolesByName = new(
        StringComparer.OrdinalIgnoreCase
    );

    // ====================================================================
    // Observable Properties
    // ====================================================================

    [ObservableProperty]
    private ObservableCollection<Model_User> _users = new();

    [ObservableProperty]
    private ObservableCollection<ViewModel_SettingsUserRoleRow> _userRows = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedUser))]
    [NotifyPropertyChangedFor(nameof(SelectedUserIsActive))]
    private ViewModel_SettingsUserRoleRow? _selectedUserRow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisualPasswordMask))]
    private Model_User? _editingUser;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isAddingNew;

    [ObservableProperty]
    private List<string> _departments = new();

    [ObservableProperty]
    private string _pinPlaceholderText = "Leave blank to keep the current PIN when editing.";

    /// <summary>Required to host ContentDialogs from the ViewModel.</summary>
    public XamlRoot? XamlRoot { get; set; }

    /// <summary>True when a user row is selected in the list.</summary>
    public bool HasSelectedUser => SelectedUser is not null;

    /// <summary>True when the selected user is currently active.</summary>
    public bool SelectedUserIsActive => SelectedUser?.IsActive ?? false;

    public Model_User? SelectedUser => SelectedUserRow?.User;

    /// <summary>Masked indicator — never exposes the real password string.</summary>
    public string VisualPasswordMask =>
        string.IsNullOrEmpty(EditingUser?.VisualPassword) ? string.Empty : "••••••••";

    // ====================================================================
    // Constructor
    // ====================================================================

    public ViewModel_Settings_Users(
        Dao_User daoUser,
        Dao_SettingsCoreRoles daoSettingsRoles,
        Dao_SettingsCoreUserRoles daoSettingsUserRoles,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges
    )
        : base(errorHandler, logger, notificationService)
    {
        _daoUser = daoUser;
        _daoSettingsRoles = daoSettingsRoles;
        _daoSettingsUserRoles = daoSettingsUserRoles;
        _sessionManager = sessionManager;
        _userPrivileges = userPrivileges;
    }

    // ====================================================================
    // Commands
    // ====================================================================

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var result = await _daoUser.GetAllAsync();
            if (result.IsSuccess)
            {
                var loadedUsers = result.Data ?? new List<Model_User>();
                Users = new ObservableCollection<Model_User>(loadedUsers);
                await LoadUserRolesAsync(loadedUsers);
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? "Failed to load users.", InfoBarSeverity.Error);
            }

            var deptResult = await _daoUser.GetActiveDepartmentsAsync();
            if (deptResult.IsSuccess)
            {
                Departments = deptResult.Data ?? new List<string>();
            }

            EditingUser = null;
            IsEditing = false;
            IsAddingNew = false;
            SelectedUserRow = null;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadUsersAsync),
                nameof(ViewModel_Settings_Users)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddNewUserAsync()
    {
        EditingUser = new Model_User { IsActive = true };
        IsAddingNew = true;
        IsEditing = true;
        PinPlaceholderText = "Enter a new 4-digit PIN";

        var deptResult = await _daoUser.GetActiveDepartmentsAsync();
        if (deptResult.IsSuccess)
        {
            Departments = deptResult.Data ?? new List<string>();
        }
    }

    [RelayCommand]
    private void EditUser(Model_User user)
    {
        EditingUser = new Model_User
        {
            EmployeeNumber = user.EmployeeNumber,
            WindowsUsername = user.WindowsUsername,
            FullName = user.FullName,
            Pin = string.Empty,
            Department = user.Department,
            Shift = user.Shift,
            IsActive = user.IsActive,
            VisualUsername = user.VisualUsername,
            VisualPassword = user.VisualPassword,
            DefaultReceivingMode = user.DefaultReceivingMode,
            DefaultDunnageMode = user.DefaultDunnageMode,
            CreatedDate = user.CreatedDate,
            CreatedBy = user.CreatedBy,
            ModifiedDate = user.ModifiedDate,
        };
        IsAddingNew = false;
        IsEditing = true;
        PinPlaceholderText = "Leave blank to keep the current PIN";
    }

    [RelayCommand]
    private async Task SaveUserRoleAsync(ViewModel_SettingsUserRoleRow? row)
    {
        if (row is null || row.CanEditRole is false)
        {
            return;
        }

        if (IsBusy)
        {
            return;
        }

        var selectedRoleName = row.SelectedRoleName;
        if (
            string.IsNullOrWhiteSpace(selectedRoleName)
            || _rolesByName.TryGetValue(selectedRoleName, out var targetRole) is false
        )
        {
            await _errorHandler.ShowUserErrorAsync(
                "The selected role is not available for assignment.",
                "Privilege Change Blocked",
                nameof(SaveUserRoleAsync)
            );
            return;
        }

        if (CanGrantRole(selectedRoleName) is false)
        {
            await _errorHandler.ShowUserErrorAsync(
                $"Your current privilege level cannot grant '{selectedRoleName}'.",
                "Privilege Change Blocked",
                nameof(SaveUserRoleAsync)
            );
            row.SelectedRoleName = row.CurrentRoleName;
            return;
        }

        if (
            string.Equals(row.CurrentRoleName, selectedRoleName, StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _daoSettingsUserRoles.AssignRoleAsync(
                row.User.EmployeeNumber,
                targetRole.Id
            );
            if (result.IsSuccess)
            {
                row.CurrentRoleName = selectedRoleName;
                ShowStatus(
                    $"Updated {row.User.FullName} to {selectedRoleName}.",
                    InfoBarSeverity.Success
                );
            }
            else
            {
                row.SelectedRoleName = row.CurrentRoleName;
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to save the updated privilege level.",
                    "Privilege Change Failed",
                    nameof(SaveUserRoleAsync)
                );
            }
        }
        catch (Exception ex)
        {
            row.SelectedRoleName = row.CurrentRoleName;
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveUserRoleAsync),
                nameof(ViewModel_Settings_Users)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (EditingUser is null || IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingUser.FullName))
        {
            ShowStatus("Full name is required.", InfoBarSeverity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingUser.WindowsUsername))
        {
            ShowStatus("Windows username is required.", InfoBarSeverity.Warning);
            return;
        }

        if (IsAddingNew && string.IsNullOrWhiteSpace(EditingUser.Pin))
        {
            ShowStatus("PIN is required for new users.", InfoBarSeverity.Warning);
            return;
        }

        if (
            !string.IsNullOrWhiteSpace(EditingUser.Pin)
            && (EditingUser.Pin.Length != 4 || !int.TryParse(EditingUser.Pin, out _))
        )
        {
            ShowStatus("PIN must be exactly 4 numeric digits.", InfoBarSeverity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingUser.Department))
        {
            ShowStatus("Department is required.", InfoBarSeverity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingUser.Shift))
        {
            ShowStatus("Shift is required.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            var updatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "SYSTEM";

            Model_Dao_Result opResult;
            if (IsAddingNew)
            {
                var createResult = await _daoUser.CreateNewUserAsync(EditingUser, updatedBy);
                opResult = createResult.IsSuccess
                    ? Model_Dao_Result_Factory.Success()
                    : Model_Dao_Result_Factory.Failure(
                        createResult.ErrorMessage ?? "Create failed."
                    );
            }
            else
            {
                opResult = await _daoUser.UpdateAsync(EditingUser, updatedBy);
            }

            if (opResult.IsSuccess)
            {
                ShowStatus(
                    IsAddingNew ? "User created successfully." : "User updated successfully.",
                    InfoBarSeverity.Success
                );
                await LoadUsersAsync();
            }
            else
            {
                ShowStatus(opResult.ErrorMessage ?? "Save failed.", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveUserAsync),
                nameof(ViewModel_Settings_Users)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveVisualCredentialsAsync()
    {
        if (EditingUser is null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var updatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "SYSTEM";

            var result = await _daoUser.UpdateVisualCredentialsAsync(
                EditingUser.EmployeeNumber,
                EditingUser.VisualUsername,
                EditingUser.VisualPassword,
                updatedBy
            );

            ShowStatus(
                result.IsSuccess
                    ? "Visual credentials saved."
                    : (result.ErrorMessage ?? "Failed to save credentials."),
                result.IsSuccess ? InfoBarSeverity.Success : InfoBarSeverity.Error
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveVisualCredentialsAsync),
                nameof(ViewModel_Settings_Users)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeactivateUserAsync(Model_User user)
    {
        if (user is null || IsBusy)
        {
            return;
        }

        var currentEmp = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        if (currentEmp == user.EmployeeNumber)
        {
            ShowStatus("Cannot deactivate the currently logged-in user.", InfoBarSeverity.Warning);
            return;
        }

        if (XamlRoot is not null)
        {
            var dialog = new ContentDialog
            {
                Title = "Deactivate User",
                Content = $"Deactivate '{user.FullName}'? They will no longer be able to log in.",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                dialog,
                XamlRoot
            );

            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary)
            {
                return;
            }
        }

        try
        {
            IsBusy = true;
            var updatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "SYSTEM";

            var result = await _daoUser.DeactivateAsync(user.EmployeeNumber, updatedBy);
            if (result.IsSuccess)
            {
                ShowStatus($"'{user.FullName}' has been deactivated.", InfoBarSeverity.Success);
                await LoadUsersAsync();
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? "Deactivation failed.", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(DeactivateUserAsync),
                nameof(ViewModel_Settings_Users)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingUser = null;
        IsEditing = false;
        IsAddingNew = false;
        PinPlaceholderText = "Leave blank to keep the current PIN when editing.";
    }

    // ====================================================================
    // Property Change Handlers
    // ====================================================================

    partial void OnEditingUserChanged(Model_User? value)
    {
        OnPropertyChanged(nameof(VisualPasswordMask));
    }

    private async Task LoadUserRolesAsync(List<Model_User> loadedUsers)
    {
        var allRolesResult = await _daoSettingsRoles.GetAllAsync();
        var allRoles = allRolesResult.IsSuccess
            ? allRolesResult.Data ?? new List<Model_SettingsRole>()
            : new List<Model_SettingsRole>();
        _rolesByName = allRoles.ToDictionary(
            role => role.RoleName,
            StringComparer.OrdinalIgnoreCase
        );

        var rows = new List<ViewModel_SettingsUserRoleRow>();
        var currentEmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;

        foreach (var user in loadedUsers)
        {
            var mappingResult = await _daoSettingsUserRoles.GetByUserAsync(user.EmployeeNumber);
            var mappedRoles = mappingResult.IsSuccess
                ? mappingResult
                    .Data?.Select(mapping =>
                        allRoles.FirstOrDefault(role => role.Id == mapping.RoleId)?.RoleName
                    )
                    .Where(static roleName => string.IsNullOrWhiteSpace(roleName) is false)
                    .Cast<string>()
                    .ToList()
                    ?? new List<string>()
                : new List<string>();

            var currentRoleName = ResolveDisplayRole(mappedRoles);
            var row = new ViewModel_SettingsUserRoleRow
            {
                User = user,
                CurrentRoleName = currentRoleName,
                SelectedRoleName = currentRoleName,
                CanEditRole =
                    currentEmployeeNumber != user.EmployeeNumber
                    && GetAssignableRoleNames().Count > 0,
            };

            foreach (var roleName in GetAvailableRoleNames(currentRoleName))
            {
                row.AvailableRoleNames.Add(roleName);
            }

            rows.Add(row);
        }

        UserRows = new ObservableCollection<ViewModel_SettingsUserRoleRow>(rows);
    }

    private string ResolveDisplayRole(List<string> mappedRoles)
    {
        if (
            mappedRoles.Exists(static role =>
                string.Equals(role, "Developer", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return "Developer";
        }

        if (
            mappedRoles.Exists(static role =>
                string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return "Admin";
        }

        return mappedRoles.Count > 0 ? mappedRoles[0] : "User";
    }

    private List<string> GetAvailableRoleNames(string currentRoleName)
    {
        var roleNames = new List<string>(GetAssignableRoleNames());
        if (
            string.IsNullOrWhiteSpace(currentRoleName) is false
            && roleNames.Contains(currentRoleName, StringComparer.OrdinalIgnoreCase) is false
        )
        {
            roleNames.Insert(0, currentRoleName);
        }

        return roleNames;
    }

    private List<string> GetAssignableRoleNames()
    {
        if (_userPrivileges.HasRole("Developer"))
        {
            return new List<string> { "Developer", "Admin" };
        }

        if (_userPrivileges.HasRole("Admin"))
        {
            return new List<string> { "Admin" };
        }

        return new List<string>();
    }

    private bool CanGrantRole(string roleName)
    {
        return GetAssignableRoleNames().Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}
