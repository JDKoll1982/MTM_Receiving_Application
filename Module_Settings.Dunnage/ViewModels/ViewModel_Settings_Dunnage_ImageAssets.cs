using System;
using System.IO;
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

public sealed partial class ViewModel_Settings_Dunnage_ImageAssets : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Dunnage";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly IService_DunnageImageStorage _imageStorage;

    [ObservableProperty]
    private string _defaultImageLocation = string.Empty;

    [ObservableProperty]
    private int _maximumImageFileSizeKb = 512;

    public ViewModel_Settings_Dunnage_ImageAssets(
        IService_SettingsCoreFacade settingsCore,
        IService_DunnageSettings dunnageSettings,
        IService_DunnageImageStorage imageStorage,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        _imageStorage = imageStorage ?? throw new ArgumentNullException(nameof(imageStorage));
        Title = "Dunnage Image Assets";

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            if (TryValidateDefaultImageLocation(out var validationMessage) is false)
            {
                ShowStatus(validationMessage, InfoBarSeverity.Warning);
                return;
            }

            if (MaximumImageFileSizeKb <= 0)
            {
                ShowStatus(
                    "The maximum image file size must be greater than zero.",
                    InfoBarSeverity.Warning
                );
                return;
            }

            var imageLocationResult = await _settingsCore.SetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.Application.DefaultImageLocation,
                DefaultImageLocation.Trim(),
                null
            );

            if (!imageLocationResult.IsSuccess)
            {
                ShowStatus(
                    imageLocationResult.ErrorMessage
                        ?? "Failed to save the shared Dunnage image folder.",
                    InfoBarSeverity.Error
                );
                return;
            }

            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb,
                MaximumImageFileSizeKb.ToString()
            );

            await _imageStorage.RefreshConfiguredRootFolderAsync();
            ShowStatus("Dunnage image asset settings saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage image asset settings.",
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
                DunnageSettingsKeys.Application.DefaultImageLocation,
                null
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb,
                null
            );

            await LoadAsync();
            ShowStatus("Dunnage image asset settings reset.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset Dunnage image asset settings.",
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
            var imageLocationResult = await _settingsCore.GetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.Application.DefaultImageLocation,
                null
            );
            DefaultImageLocation =
                imageLocationResult.IsSuccess && imageLocationResult.Data is not null
                    ? imageLocationResult.Data.Value.Trim()
                    : string.Empty;

            MaximumImageFileSizeKb = await _dunnageSettings.GetIntAsync(
                DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb
            );
            if (MaximumImageFileSizeKb <= 0)
            {
                MaximumImageFileSizeKb = 512;
            }

            await _imageStorage.RefreshConfiguredRootFolderAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading Dunnage image asset settings: {ex.Message}", ex);
            DefaultImageLocation = string.Empty;
            MaximumImageFileSizeKb = 512;
        }
    }

    private bool TryValidateDefaultImageLocation(out string message)
    {
        if (string.IsNullOrWhiteSpace(DefaultImageLocation))
        {
            DefaultImageLocation = string.Empty;
            message = string.Empty;
            return true;
        }

        var normalizedPath = DefaultImageLocation.Trim();
        if (Directory.Exists(normalizedPath))
        {
            DefaultImageLocation = normalizedPath;
            message = string.Empty;
            return true;
        }

        message = "The shared Dunnage image folder must point to an existing directory.";
        return false;
    }
}
