using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_ImagePresentation : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Dunnage";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_DunnageSettings _dunnageSettings;

    [ObservableProperty]
    private bool _enableTypeImages = true;

    [ObservableProperty]
    private bool _enablePartImages = true;

    [ObservableProperty]
    private int _defaultThumbnailSize = 96;

    [ObservableProperty]
    private string _defaultVisualSource = "PartThenTypeThenIcon";

    public ViewModel_Settings_Dunnage_ImagePresentation(
        IService_SettingsCoreFacade settingsCore,
        IService_DunnageSettings dunnageSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        Title = "Dunnage Image Presentation";
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.EnableTypeImages,
                EnableTypeImages.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.EnablePartImages,
                EnablePartImages.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.DefaultThumbnailSize,
                DefaultThumbnailSize.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.DefaultVisualSource,
                DefaultVisualSource
            );
            ShowStatus("Dunnage image presentation settings saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage image presentation settings.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            IsBusy = true;
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UiUx.EnableTypeImages,
                null
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UiUx.EnablePartImages,
                null
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UiUx.DefaultThumbnailSize,
                null
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UiUx.DefaultVisualSource,
                null
            );
            await LoadAsync();
            ShowStatus("Dunnage image presentation settings reset.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset Dunnage image presentation settings.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            EnableTypeImages = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.UiUx.EnableTypeImages
            );
            EnablePartImages = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.UiUx.EnablePartImages
            );
            DefaultThumbnailSize = await _dunnageSettings.GetIntAsync(
                DunnageSettingsKeys.UiUx.DefaultThumbnailSize
            );
            DefaultVisualSource = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiUx.DefaultVisualSource
            );

            if (DefaultThumbnailSize <= 0)
            {
                DefaultThumbnailSize = 96;
            }

            if (string.IsNullOrWhiteSpace(DefaultVisualSource))
            {
                DefaultVisualSource = "PartThenTypeThenIcon";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load Dunnage image presentation settings.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }
}
