using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.Services.Navigation;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;


namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Mode Selection - launching Wizard, Manual Entry, or Edit Mode
/// </summary>
public partial class RoutingModeSelectionViewModel : ViewModel_Shared_Base
{
    private readonly IRoutingUserPreferenceService _userPreferenceService;
    private readonly IService_Navigation _navigationService;
    private readonly IService_UserSessionManager _sessionManager;

    private Frame? _frame;

    // Separate default flags for each mode
    [ObservableProperty] private bool _isWizardDefault;
    [ObservableProperty] private bool _isManualDefault;
    [ObservableProperty] private bool _isEditDefault;

    public RoutingModeSelectionViewModel(
        IRoutingUserPreferenceService userPreferenceService,
        IService_Navigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _sessionManager = sessionManager;
        _userPreferenceService = userPreferenceService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Sets the navigation frame
    /// </summary>
    /// <param name="frame"></param>
    public void SetNavigationFrame(Frame frame)
    {
        _frame = frame;
    }

    /// <summary>
    /// Initialize - load user preferences
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading preferences...";

            // Issue #7: Get current user from session
            var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;

            var prefsResult = await _userPreferenceService.GetUserPreferenceAsync(employeeNumber);
            if (prefsResult.IsSuccess && prefsResult.Data != null)
            {
                if (Enum.TryParse<Enum_RoutingMode>(prefsResult.Data.DefaultMode, out var mode))
                {
                    // Set checkbox state based on preference
                    IsWizardDefault = mode == Enum_RoutingMode.WIZARD;
                    IsManualDefault = mode == Enum_RoutingMode.MANUAL;
                    IsEditDefault = mode == Enum_RoutingMode.EDIT;

                    // If default mode is set and not Wizard (which might be the landing page), auto-nav
                    // Revisit this logic if Wizard SHOULD technically be skipped to Step 1 directly. 
                    // For now keeping existing logic but fixing navigation.
                    if (mode != Enum_RoutingMode.WIZARD)
                    {
                        await NavigateToModeAsync(mode);
                    }
                }
            }

            StatusMessage = "Select a mode to continue";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
                nameof(InitializeAsync), nameof(RoutingModeSelectionViewModel));
            StatusMessage = "Error loading preferences";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectWizardModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.WIZARD);

    [RelayCommand]
    private async Task SelectManualEntryModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.MANUAL);

    [RelayCommand]
    private async Task SelectEditModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.EDIT);

    [RelayCommand]
    private async Task SetWizardAsDefaultAsync(bool isChecked)
    {
        if (isChecked)
        {
            IsManualDefault = false;
            IsEditDefault = false;
            await SavePreferenceAsync(Enum_RoutingMode.WIZARD);
        }
    }

    [RelayCommand]
    private async Task SetManualAsDefaultAsync(bool isChecked)
    {
        if (isChecked)
        {
            IsWizardDefault = false;
            IsEditDefault = false;
            await SavePreferenceAsync(Enum_RoutingMode.MANUAL);
        }
    }

    [RelayCommand]
    private async Task SetEditAsDefaultAsync(bool isChecked)
    {
        if (isChecked)
        {
            IsWizardDefault = false;
            IsManualDefault = false;
            await SavePreferenceAsync(Enum_RoutingMode.EDIT);
        }
    }

    private async Task SavePreferenceAsync(Enum_RoutingMode mode)
    {
        try
        {
            // Issue #7: Get current user from session
            var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;

            var preference = new Model_RoutingUserPreference
            {
                EmployeeNumber = employeeNumber,
                DefaultMode = mode.ToString(),
                EnableValidation = true
            };

            var result = await _userPreferenceService.SaveUserPreferenceAsync(preference);

            if (result.IsSuccess)
            {
                await _logger.LogInfoAsync($"Default mode set to {mode} for employee {employeeNumber}");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
                nameof(SavePreferenceAsync), nameof(RoutingModeSelectionViewModel));
        }
    }

    private async Task NavigateToModeAsync(Enum_RoutingMode mode)
    {
        try
        {
            if (_frame == null)
            {
                // Fallback attempt to find frame if not explicitly set? 
                // For now just log usage error
                _errorHandler.HandleException(new InvalidOperationException("Navigation Frame not set"),
                    Module_Core.Models.Enums.Enum_ErrorSeverity.Error,
                    nameof(NavigateToModeAsync), nameof(RoutingModeSelectionViewModel));
                return;
            }

            string viewName = mode switch
            {
                Enum_RoutingMode.WIZARD => typeof(Views.RoutingWizardContainerView).FullName!,
                Enum_RoutingMode.MANUAL => typeof(Views.RoutingManualEntryView).FullName!,
                Enum_RoutingMode.EDIT => typeof(Views.RoutingEditModeView).FullName!,
                _ => throw new ArgumentException($"Unknown mode: {mode}")
            };

            _navigationService.NavigateTo(_frame, viewName);
            await _logger.LogInfoAsync($"Navigated to {viewName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(NavigateToModeAsync), nameof(RoutingModeSelectionViewModel));
        }
    }
}
