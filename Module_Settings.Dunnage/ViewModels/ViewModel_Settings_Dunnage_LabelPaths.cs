using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public partial class ViewModel_Settings_Dunnage_LabelPaths : ViewModel_Shared_Base
{
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly IService_LabelViewLauncher _labelViewLauncher;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _dunnageLabelPath = string.Empty;

    public ViewModel_Settings_Dunnage_LabelPaths(
        IService_DunnageSettings dunnageSettings,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        _labelViewLauncher =
            labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
        Title = "Dunnage Label Paths";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task OpenFolderAsync(string? configuredPath)
    {
        var result = await _labelViewLauncher.OpenFolderForPathAsync(configuredPath);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, nameof(OpenFolderAsync));
            return;
        }

        StatusMessage = "Opened File Explorer for the current path.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateLabelPath(DunnageLabelPath, "Dunnage Label"))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.Labels.DunnageLabelPath,
                DunnageLabelPath ?? string.Empty
            );
            StatusMessage = "Dunnage label path saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_Dunnage_LabelPaths)
            );
            StatusMessage = "Failed to save Dunnage label path.";
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
            DunnageLabelPath = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.Labels.DunnageLabelPath
            );
            StatusMessage = "Dunnage label path loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_Dunnage_LabelPaths)
            );
            StatusMessage = "Failed to load Dunnage label path.";
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
