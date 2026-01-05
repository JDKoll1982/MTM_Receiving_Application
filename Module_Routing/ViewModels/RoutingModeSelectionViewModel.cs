using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Mode Selection screen - choose Wizard, Manual Entry, or Edit Mode
/// </summary>
public partial class RoutingModeSelectionViewModel : BaseViewModel
{
    private readonly IRoutingUserPreferenceService _userPreferenceService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _setAsDefaultMode;

    public RoutingModeSelectionViewModel(
        IRoutingUserPreferenceService userPreferenceService,
        INavigationService navigationService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
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
                if (prefsResult.Data.DefaultMode != Enum_RoutingMode.WIZARD)
                {
                    await NavigateToModeAsync(prefsResult.Data.DefaultMode);
                }
            }

            StatusMessage = "Select a mode to continue";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Low,
                callerName: nameof(InitializeAsync), controlName: nameof(RoutingModeSelectionViewModel));
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
        await SavePreferenceIfChecked(Enum_RoutingMode.WIZARD);
        await NavigateToModeAsync(Enum_RoutingMode.WIZARD);
    }

    /// <summary>
    /// Navigate to Manual Entry mode
    /// </summary>
    [RelayCommand]
    private async Task SelectManualEntryModeAsync()
    {
        await SavePreferenceIfChecked(Enum_RoutingMode.MANUAL);
        await NavigateToModeAsync(Enum_RoutingMode.MANUAL);
    }

    /// <summary>
    /// Navigate to Edit mode
    /// </summary>
    [RelayCommand]
    private async Task SelectEditModeAsync()
    {
        await SavePreferenceIfChecked(Enum_RoutingMode.EDIT);
        await NavigateToModeAsync(Enum_RoutingMode.EDIT);
    }

    /// <summary>
    /// Save default mode preference if checkbox is checked
    /// </summary>
    private async Task SavePreferenceIfChecked(Enum_RoutingMode mode)
    {
        if (!SetAsDefaultMode) return;

        try
        {
            // TODO: Get current employee number from session
            int employeeNumber = 6229; // Placeholder

            var result = await _userPreferenceService.SaveUserPreferenceAsync(
                employeeNumber,
                mode,
                validatePOBeforeSave: true
            );

            if (result.IsSuccess)
            {
                await _logger.LogInformationAsync($"Default mode set to {mode} for employee {employeeNumber}");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Low,
                callerName: nameof(SavePreferenceIfChecked), controlName: nameof(RoutingModeSelectionViewModel));
        }
    }

    /// <summary>
    /// Navigate to selected mode
    /// </summary>
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

            _navigationService.NavigateTo(viewName);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Medium,
                callerName: nameof(NavigateToModeAsync), controlName: nameof(RoutingModeSelectionViewModel));
            await Task.CompletedTask;
        }
    }
}
