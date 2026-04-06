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

public sealed partial class ViewModel_Settings_Dunnage_Workflow : ViewModel_Shared_Base
{
    private readonly IService_DunnageSettings _dunnageSettings;

    [ObservableProperty]
    private bool _showTypeImagesOnTypeSelection = true;

    [ObservableProperty]
    private bool _showPartImagesOnPartSelection = true;

    [ObservableProperty]
    private bool _showImagesOnReview = true;

    [ObservableProperty]
    private bool _fallbackToTypeImageWhenPartMissing = true;

    public ViewModel_Settings_Dunnage_Workflow(
        IService_DunnageSettings dunnageSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        Title = "Dunnage Workflow";
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection,
                ShowTypeImagesOnTypeSelection.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection,
                ShowPartImagesOnPartSelection.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.Workflow.ShowImagesOnReview,
                ShowImagesOnReview.ToString()
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing,
                FallbackToTypeImageWhenPartMissing.ToString()
            );
            ShowStatus("Dunnage workflow settings saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage workflow settings.",
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
        ShowStatus("Dunnage workflow settings reset to current defaults.", InfoBarSeverity.Success);
    }

    private async Task LoadAsync()
    {
        try
        {
            ShowTypeImagesOnTypeSelection = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection
            );
            ShowPartImagesOnPartSelection = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection
            );
            ShowImagesOnReview = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.Workflow.ShowImagesOnReview
            );
            FallbackToTypeImageWhenPartMissing = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load Dunnage workflow settings.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }
}
