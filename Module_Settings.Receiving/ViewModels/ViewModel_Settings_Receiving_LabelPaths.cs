using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public partial class ViewModel_Settings_Receiving_LabelPaths : ViewModel_Shared_Base
{
    private readonly IService_LabelViewLauncher _labelViewLauncher;
    private readonly IService_ReceivingUserLabelSettings _userLabelSettings;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _receivingLabelPath = string.Empty;

    [ObservableProperty]
    private string _miniReceivingLabelPath = string.Empty;

    public ViewModel_Settings_Receiving_LabelPaths(
        IService_ReceivingUserLabelSettings userLabelSettings,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _userLabelSettings =
            userLabelSettings ?? throw new ArgumentNullException(nameof(userLabelSettings));
        _labelViewLauncher =
            labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
        Title = "Receiving Label Paths";
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

        if (!ValidateLabelPath(ReceivingLabelPath, "Receiving Label"))
        {
            return;
        }

        if (!ValidateLabelPath(MiniReceivingLabelPath, "Mini-Receiving Label"))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _userLabelSettings.SaveReceivingLabelPathAsync(
                ReceivingLabelPath ?? string.Empty
            );
            await _userLabelSettings.SaveMiniReceivingLabelPathAsync(
                MiniReceivingLabelPath ?? string.Empty
            );

            StatusMessage = "Receiving label paths saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_Receiving_LabelPaths)
            );
            StatusMessage = "Failed to save Receiving label paths.";
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
            ReceivingLabelPath = await _userLabelSettings.GetReceivingLabelPathAsync();
            MiniReceivingLabelPath = await _userLabelSettings.GetMiniReceivingLabelPathAsync();
            StatusMessage = "Receiving label paths loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_Receiving_LabelPaths)
            );
            StatusMessage = "Failed to load Receiving label paths.";
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
