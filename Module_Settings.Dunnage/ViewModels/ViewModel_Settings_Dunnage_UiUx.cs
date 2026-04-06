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

public sealed partial class ViewModel_Settings_Dunnage_UiUx : ViewModel_Shared_Base
{
    private readonly IService_DunnageSettings _dunnageSettings;

    [ObservableProperty]
    private bool _enableTypeImages = true;

    [ObservableProperty]
    private bool _enablePartImages = true;

    [ObservableProperty]
    private int _defaultThumbnailSize = 96;

    [ObservableProperty]
    private int _maximumImageFileSizeKb = 512;

    [ObservableProperty]
    private string _defaultVisualSource = "PartThenTypeThenIcon";

    public ViewModel_Settings_Dunnage_UiUx(
        IService_DunnageSettings dunnageSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        Title = "Dunnage UI/UX";
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
                DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb,
                MaximumImageFileSizeKb.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UiUx.DefaultVisualSource,
                DefaultVisualSource
            );
            ShowStatus("Dunnage UI/UX settings saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage UI/UX settings.",
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
        await LoadAsync();
        ShowStatus("Dunnage UI/UX settings reset to current defaults.", InfoBarSeverity.Success);
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
            MaximumImageFileSizeKb = await _dunnageSettings.GetIntAsync(
                DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb
            );
            DefaultVisualSource = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UiUx.DefaultVisualSource
            );

            if (DefaultThumbnailSize <= 0)
            {
                DefaultThumbnailSize = 96;
            }

            if (MaximumImageFileSizeKb <= 0)
            {
                MaximumImageFileSizeKb = 512;
            }

            if (string.IsNullOrWhiteSpace(DefaultVisualSource))
            {
                DefaultVisualSource = "PartThenTypeThenIcon";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load Dunnage UI/UX settings.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }
}
