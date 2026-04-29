using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

public partial class ViewModel_Settings_LabelViewExecutable : ViewModel_Shared_Base
{
    private readonly IService_LabelViewLauncher _labelViewLauncher;
    private readonly IService_SettingsCoreFacade _settingsCore;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _executablePath = string.Empty;

    public string DefaultExecutablePath => _labelViewLauncher.DefaultExecutablePath;

    public ViewModel_Settings_LabelViewExecutable(
        IService_SettingsCoreFacade settingsCore,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _labelViewLauncher =
            labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
        Title = "LabelView Executable";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private void UseDefaultPath()
    {
        ExecutablePath = DefaultExecutablePath;
        StatusMessage = "Default LabelView path loaded into the editor.";
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

        if (!_labelViewLauncher.IsExecutablePathValid(ExecutablePath))
        {
            StatusMessage = "LabelView path must point to an existing LV.exe file.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _settingsCore.SetSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.LabelView.ExecutablePath,
                ExecutablePath
            );

            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    result,
                    $"Save {CoreSettingsKeys.LabelView.ExecutablePath}"
                );
                StatusMessage = "Failed to save LabelView path.";
                return;
            }

            StatusMessage = "LabelView executable path saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_LabelViewExecutable)
            );
            StatusMessage = "Failed to save LabelView path.";
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
            var result = await _settingsCore.GetSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.LabelView.ExecutablePath
            );

            ExecutablePath = string.IsNullOrWhiteSpace(result.Data?.Value)
                ? DefaultExecutablePath
                : result.Data!.Value;

            StatusMessage = _labelViewLauncher.IsExecutablePathValid(ExecutablePath)
                ? "LabelView executable found."
                : "LabelView executable was not found. Confirm the path below or contact IT.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_LabelViewExecutable)
            );
            ExecutablePath = DefaultExecutablePath;
            StatusMessage = "Failed to load LabelView settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
