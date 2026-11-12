using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reconciliation.ViewModels;

/// <summary>
/// The Receiving Validator's settings page. It reads and writes the module's two
/// windows through the settings core facade and rejects a negative window
/// (FR-059, R-SET-01, R-SET-03).
/// </summary>
public sealed partial class ViewModel_Settings_Reconciliation_Main : ViewModel_Shared_Base
{
    private readonly IService_ReconciliationSettings _settings;

    /// <summary>Creates the settings page over the module's settings service.</summary>
    public ViewModel_Settings_Reconciliation_Main(
        IService_ReconciliationSettings settings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settings = settings;
        Title = "Receiving Validator Settings";

        _ = LoadAsync();
    }

    /// <summary>Days either side of the receipt date used to discover movements.</summary>
    [ObservableProperty]
    private int _discoveryWindowDays;

    /// <summary>Days after the receipt date used for strict validation.</summary>
    [ObservableProperty]
    private int _strictWindowDays;

    /// <summary>Shown when a window is negative.</summary>
    [ObservableProperty]
    private string _validationMessage = string.Empty;

    /// <summary>Reads both windows.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            DiscoveryWindowDays = await _settings.GetDiscoveryWindowDaysAsync();
            StrictWindowDays = await _settings.GetStrictWindowDaysAsync();
            ValidationMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Writes both windows, refusing a negative one.</summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (DiscoveryWindowDays < 0 || StrictWindowDays < 0)
        {
            ValidationMessage = "A window must be zero or more days.";
            ShowStatus(ValidationMessage, InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;

        try
        {
            await _settings.SetDiscoveryWindowDaysAsync(DiscoveryWindowDays);
            await _settings.SetStrictWindowDaysAsync(StrictWindowDays);
            ValidationMessage = string.Empty;
            ShowStatus("The window settings were saved.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
