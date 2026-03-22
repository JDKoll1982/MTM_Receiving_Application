using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    private readonly IService_UserSessionManager _sessionManager;

    // ====================================================================
    // Observable Properties
    // ====================================================================

    [ObservableProperty]
    private ObservableCollection<Model_User> _users = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedUser))]
    [NotifyPropertyChangedFor(nameof(SelectedUserIsActive))]
    private Model_User? _selectedUser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisualPasswordMask))]
    private Model_User? _editingUser;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isAddingNew;

    [ObservableProperty]
    private List<string> _departments = new();

    /// <summary>Required to host ContentDialogs from the ViewModel.</summary>
    public XamlRoot? XamlRoot { get; set; }

    /// <summary>True when a user row is selected in the list.</summary>
    public bool HasSelectedUser => SelectedUser is not null;

    /// <summary>True when the selected user is currently active.</summary>
    public bool SelectedUserIsActive => SelectedUser?.IsActive ?? false;

    /// <summary>Masked indicator — never exposes the real password string.</summary>
    public string VisualPasswordMask =>
        string.IsNullOrEmpty(EditingUser?.VisualPassword) ? string.Empty : "••••••••";

    // ====================================================================
    // Constructor
    // ====================================================================

    public ViewModel_Settings_Users(
        Dao_User daoUser,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_UserSessionManager sessionManager
    )
        : base(errorHandler, logger, notificationService)
    {
        _daoUser = daoUser;
        _sessionManager = sessionManager;
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
                Users = new ObservableCollection<Model_User>(result.Data ?? new List<Model_User>());
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
            Pin = user.Pin,
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
    }

    // ====================================================================
    // Property Change Handlers
    // ====================================================================

    partial void OnEditingUserChanged(Model_User? value)
    {
        OnPropertyChanged(nameof(VisualPasswordMask));
    }
}
