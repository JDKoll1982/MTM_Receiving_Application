using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// Settings page ViewModel for Material Availability work-order field visibility and print selection.
/// </summary>
public sealed partial class ViewModel_Settings_MaterialAvailabilityBoardFields
    : ViewModel_Shared_Base
{
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_ShipRecToolsSettings _shipRecToolsSettings;
    private readonly IService_SettingsErrorHandler _settingsErrorHandler;

    public ObservableCollection<Model_Settings_MaterialAvailabilityFieldGroup> FieldGroups { get; } =
    [];

    [ObservableProperty]
    private bool _isShowAllChipEnabled;

    public ViewModel_Settings_MaterialAvailabilityBoardFields(
        IService_SettingsCoreFacade settingsCore,
        IService_ShipRecToolsSettings shipRecToolsSettings,
        IService_SettingsErrorHandler settingsErrorHandler,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _shipRecToolsSettings =
            shipRecToolsSettings ?? throw new ArgumentNullException(nameof(shipRecToolsSettings));
        _settingsErrorHandler =
            settingsErrorHandler ?? throw new ArgumentNullException(nameof(settingsErrorHandler));

        foreach (
            var group in MaterialAvailabilityWorkOrderFieldCatalog
                .All.GroupBy(field => field.Category)
                .Select(group => new Model_Settings_MaterialAvailabilityFieldGroup
                {
                    Title = group.Key,
                    Fields =
                        new ObservableCollection<Model_Settings_MaterialAvailabilityFieldOption>(
                            group
                                .OrderBy(field => field.SortOrder)
                                .Select(field => new Model_Settings_MaterialAvailabilityFieldOption
                                {
                                    Id = field.Id,
                                    DisplayName = field.DisplayName,
                                    IsLogicOnly = field.IsLogicOnly,
                                })
                        ),
                })
        )
        {
            FieldGroups.Add(group);
        }

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            var uiFieldIds = FieldGroups
                .SelectMany(group => group.Fields)
                .Where(field => field.IsVisibleInUi)
                .Select(field => field.Id)
                .ToList();
            var printFieldIds = FieldGroups
                .SelectMany(group => group.Fields)
                .Where(field => field.IsVisibleInPrint)
                .Select(field => field.Id)
                .ToList();

            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderUiVisibleFieldIds,
                JsonSerializer.Serialize(uiFieldIds)
            );
            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderPrintVisibleFieldIds,
                JsonSerializer.Serialize(printFieldIds)
            );
            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderAllowShowAllChip,
                IsShowAllChipEnabled.ToString().ToLowerInvariant()
            );

            await _settingsErrorHandler.ShowSuccessAsync(
                "Material Availability work-order field settings saved successfully.",
                "Save Successful"
            );
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to save Material Availability field settings.",
                "Save Error",
                ex
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

            await ResetSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderUiVisibleFieldIds
            );
            await ResetSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderPrintVisibleFieldIds
            );
            await ResetSettingAsync(
                ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderAllowShowAllChip
            );

            await LoadAsync();
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to reset Material Availability field settings.",
                "Reset Error",
                ex,
                Enum_ErrorSeverity.Warning
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync()
    {
        var settings = await _shipRecToolsSettings.GetMaterialAvailabilityFieldSettingsAsync();
        await ApplySettingsAsync(settings);
    }

    private Task ApplySettingsAsync(
        Module_ShipRec_Tools.Models.Model_Tool_MaterialAvailabilityFieldSettings settings
    )
    {
        IsShowAllChipEnabled = settings.IsShowAllChipEnabled;
        foreach (var field in FieldGroups.SelectMany(group => group.Fields))
        {
            field.IsVisibleInUi = settings.UiVisibleFieldIds.Contains(field.Id);
            field.IsVisibleInPrint = settings.PrintVisibleFieldIds.Contains(field.Id);
        }

        return Task.CompletedTask;
    }

    private async Task SaveSettingAsync(string key, string value)
    {
        var result = await _settingsCore.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            key,
            value
        );
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }

    private async Task ResetSettingAsync(string key)
    {
        var result = await _settingsCore.ResetSettingAsync(ShipRecToolsSettingsKeys.Category, key);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}
