using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Settings;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

public partial class ViewModel_Settings_Volvo_LabelPaths : ViewModel_Shared_Base
{
    private readonly IService_LabelViewLauncher _labelViewLauncher;
    private readonly IService_VolvoSettings _volvoSettings;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _volvoLabelPath = string.Empty;

    public ViewModel_Settings_Volvo_LabelPaths(
        IService_VolvoSettings volvoSettings,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _volvoSettings = volvoSettings ?? throw new ArgumentNullException(nameof(volvoSettings));
        _labelViewLauncher =
            labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
        Title = "Volvo Label Paths";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateLabelPath(VolvoLabelPath, "Volvo Label"))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _volvoSettings.SaveStringAsync(
                VolvoSettingsKeys.Labels.VolvoLabelPath,
                VolvoLabelPath ?? string.Empty
            );
            StatusMessage = "Volvo label path saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_Volvo_LabelPaths)
            );
            StatusMessage = "Failed to save Volvo label path.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            VolvoLabelPath = await _volvoSettings.GetStringAsync(
                VolvoSettingsKeys.Labels.VolvoLabelPath
            );
            StatusMessage = "Volvo label path loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_Volvo_LabelPaths)
            );
            StatusMessage = "Failed to load Volvo label path.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateLabelPath(string labelPath, string labelName)
    {
        if (string.IsNullOrWhiteSpace(labelPath))
        {
            return true;
        }

        if (_labelViewLauncher.IsLabelFilePathValid(labelPath))
        {
            return true;
        }

        StatusMessage = $"{labelName} must point to an existing .lbl file.";
        return false;
    }
}
