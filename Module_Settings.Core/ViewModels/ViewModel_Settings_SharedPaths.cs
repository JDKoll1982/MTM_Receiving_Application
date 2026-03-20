using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for shared path settings: export root, Infor Visual executable path,
/// default warehouse code, and default lot number.
/// </summary>
public partial class ViewModel_Settings_SharedPaths : ViewModel_Shared_Base
{
    private const string SystemCategory = "System";
    private const string BulkInventoryCategory = "BulkInventory";
    private const string KeyExportRoot = "Core.SharedPaths.ExportRoot";
    private const string KeyVisualExePath = "Core.SharedPaths.InforVisualExePath";
    private const string KeyWarehouseCode = "BulkInventory.Defaults.WarehouseCode";
    private const string KeyLotNo = "BulkInventory.Defaults.LotNo";

    private readonly IService_SettingsCoreFacade _settingsCore;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _exportRootPath = string.Empty;

    [ObservableProperty]
    private string _inforVisualExePath = string.Empty;

    [ObservableProperty]
    private string _defaultWarehouseCode = string.Empty;

    [ObservableProperty]
    private string _defaultLotNo = string.Empty;

    public ViewModel_Settings_SharedPaths(
        IService_SettingsCoreFacade settingsCore,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading settings...";

            ExportRootPath = await GetStringSettingAsync(SystemCategory, KeyExportRoot);
            InforVisualExePath = await GetStringSettingAsync(SystemCategory, KeyVisualExePath);
            DefaultWarehouseCode = await GetStringSettingAsync(
                BulkInventoryCategory,
                KeyWarehouseCode
            );
            DefaultLotNo = await GetStringSettingAsync(BulkInventoryCategory, KeyLotNo);

            StatusMessage = "Settings loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_SharedPaths)
            );
            StatusMessage = "Failed to load settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            StatusMessage = "Saving settings...";

            await SaveSettingAsync(SystemCategory, KeyExportRoot, ExportRootPath);
            await SaveSettingAsync(SystemCategory, KeyVisualExePath, InforVisualExePath);
            await SaveSettingAsync(BulkInventoryCategory, KeyWarehouseCode, DefaultWarehouseCode);
            await SaveSettingAsync(BulkInventoryCategory, KeyLotNo, DefaultLotNo);

            StatusMessage = "Settings saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_SharedPaths)
            );
            StatusMessage = "Failed to save settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string> GetStringSettingAsync(string category, string key)
    {
        var result = await _settingsCore.GetSettingAsync(category, key);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data.Value;
        }
        return string.Empty;
    }

    private async Task SaveSettingAsync(string category, string key, string value)
    {
        var result = await _settingsCore.SetSettingAsync(category, key, value ?? string.Empty);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Save {key}");
        }
    }
}
