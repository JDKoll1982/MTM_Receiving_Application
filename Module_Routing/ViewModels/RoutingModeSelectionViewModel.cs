using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    private bool _setAsDefaultMode;

    public RoutingModeSelectionViewModel(
        IRoutingUserPreferenceService userPreferenceService,
        IService_Navigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _userPreferenceService = userPreferenceService;
        _navigationService = navigationService;
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

            // TODO: Get current employee number from session
            int employeeNumber = 6229; // Placeholder

            var prefsResult = await _userPreferenceService.GetUserPreferenceAsync(employeeNumber);
            if (prefsResult.IsSuccess && prefsResult.Data != null)
            {
                // If default mode is set, navigate directly to that mode
                if (Enum.TryParse<Enum_RoutingMode>(prefsResult.Data.DefaultMode, out var mode) && mode != Enum_RoutingMode.WIZARD)
                {
                    await NavigateToModeAsync(mode);
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

    /// <summary>
    /// Navigate to Wizard mode
    /// </summary>
    [RelayCommand]
    private async Task SelectWizardModeAsync()
    {
        await SavePreferenceIfCheckedAsync(Enum_RoutingMode.WIZARD);
        await NavigateToModeAsync(Enum_RoutingMode.WIZARD);
    }

    /// <summary>
    /// Navigate to Manual Entry mode
    /// </summary>
    [RelayCommand]
    private async Task SelectManualEntryModeAsync()
    {
        await SavePreferenceIfCheckedAsync(Enum_RoutingMode.MANUAL);
        await NavigateToModeAsync(Enum_RoutingMode.MANUAL);
    }

    /// <summary>
    /// Navigate to Edit mode
    /// </summary>
    [RelayCommand]
    private async Task SelectEditModeAsync()
    {
        await SavePreferenceIfCheckedAsync(Enum_RoutingMode.EDIT);
        await NavigateToModeAsync(Enum_RoutingMode.EDIT);
    }

    /// <summary>
    /// Save default mode preference if checkbox is checked
    /// </summary>
    /// <param name="mode"></param>
    private async Task SavePreferenceIfCheckedAsync(Enum_RoutingMode mode)
    {
        if (!SetAsDefaultMode)
        {
            return;
        }

        try
        {
            // TODO: Get current employee number from session
            int employeeNumber = 6229; // Placeholder

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
                nameof(SavePreferenceIfCheckedAsync), nameof(RoutingModeSelectionViewModel));
        }
    }

    /// <summary>
    /// Navigate to selected mode
    /// </summary>
    /// <param name="mode"></param>
    private async Task NavigateToModeAsync(Enum_RoutingMode mode)
    {
        try
        {
            string viewName = mode switch
            {
                Enum_RoutingMode.WIZARD => typeof(Views.RoutingWizardContainerView).FullName!,
                Enum_RoutingMode.MANUAL => typeof(Views.RoutingManualEntryView).FullName!,
                Enum_RoutingMode.EDIT => typeof(Views.RoutingEditModeView).FullName!,
                _ => throw new ArgumentException($"Unknown mode: {mode}")
            };

            // TODO: Need Frame reference for navigation
            // _navigationService.NavigateTo(frame, viewName);
            await _logger.LogInfoAsync($"Navigation to {viewName} requested");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(NavigateToModeAsync), nameof(RoutingModeSelectionViewModel));
            await Task.CompletedTask;
        }
    }
}
