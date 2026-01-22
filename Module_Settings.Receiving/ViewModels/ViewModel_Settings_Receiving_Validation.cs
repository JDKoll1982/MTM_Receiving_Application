using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_Validation : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private bool _requirePoNumber;

    [ObservableProperty]
    private bool _requirePartId;

    [ObservableProperty]
    private bool _requireQuantity;

    [ObservableProperty]
    private bool _requireHeatLot;

    [ObservableProperty]
    private bool _allowNegativeQuantity;

    [ObservableProperty]
    private bool _validatePoExists;

    [ObservableProperty]
    private bool _validatePartExists;

    [ObservableProperty]
    private bool _warnOnQuantityExceedsPo;

    [ObservableProperty]
    private bool _warnOnSameDayReceiving;

    [ObservableProperty]
    private string _minLoadCount = string.Empty;

    [ObservableProperty]
    private string _maxLoadCount = string.Empty;

    [ObservableProperty]
    private string _minQuantity = string.Empty;

    [ObservableProperty]
    private string _maxQuantity = string.Empty;

    public ViewModel_Settings_Receiving_Validation(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        Title = "Receiving Validation";

        _ = LoadSettingsAsync();
    }

    public bool IsBackVisible => false;
    public bool IsNextVisible => false;
    public bool IsCancelVisible => true;
    public bool IsSaveVisible => true;
    public bool IsResetVisible => true;

    public async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            await SaveSettingAsync(ReceivingSettingsKeys.Validation.RequirePoNumber, RequirePoNumber.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.RequirePartId, RequirePartId.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.RequireQuantity, RequireQuantity.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.RequireHeatLot, RequireHeatLot.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.AllowNegativeQuantity, AllowNegativeQuantity.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.ValidatePoExists, ValidatePoExists.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.ValidatePartExists, ValidatePartExists.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo, WarnOnQuantityExceedsPo.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving, WarnOnSameDayReceiving.ToString());

            await SaveSettingAsync(ReceivingSettingsKeys.Validation.MinLoadCount, MinLoadCount);
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.MaxLoadCount, MaxLoadCount);
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.MinQuantity, MinQuantity);
            await SaveSettingAsync(ReceivingSettingsKeys.Validation.MaxQuantity, MaxQuantity);

            ShowStatus("Receiving validation settings saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to save receiving validation settings.", Enum_ErrorSeverity.Error, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ResetAsync()
    {
        try
        {
            IsBusy = true;

            await ResetSettingAsync(ReceivingSettingsKeys.Validation.RequirePoNumber);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.RequirePartId);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.RequireQuantity);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.RequireHeatLot);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.AllowNegativeQuantity);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.ValidatePoExists);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.ValidatePartExists);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.MinLoadCount);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.MaxLoadCount);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.MinQuantity);
            await ResetSettingAsync(ReceivingSettingsKeys.Validation.MaxQuantity);

            await LoadSettingsAsync();
            ShowStatus("Receiving validation settings reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to reset receiving validation settings.", Enum_ErrorSeverity.Error, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CancelAsync()
    {
        await LoadSettingsAsync();
    }

    public Task BackAsync() => Task.CompletedTask;

    public Task NextAsync() => Task.CompletedTask;

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task LoadSettingsAsync()
    {
        try
        {
            RequirePoNumber = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.RequirePoNumber, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequirePoNumber]);
            RequirePartId = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.RequirePartId, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequirePartId]);
            RequireQuantity = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.RequireQuantity, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequireQuantity]);
            RequireHeatLot = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.RequireHeatLot, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequireHeatLot]);
            AllowNegativeQuantity = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.AllowNegativeQuantity, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.AllowNegativeQuantity]);
            ValidatePoExists = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.ValidatePoExists, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.ValidatePoExists]);
            ValidatePartExists = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.ValidatePartExists, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.ValidatePartExists]);
            WarnOnQuantityExceedsPo = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo]);
            WarnOnSameDayReceiving = await GetBoolSettingAsync(ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving]);

            MinLoadCount = await GetStringSettingAsync(ReceivingSettingsKeys.Validation.MinLoadCount);
            MaxLoadCount = await GetStringSettingAsync(ReceivingSettingsKeys.Validation.MaxLoadCount);
            MinQuantity = await GetStringSettingAsync(ReceivingSettingsKeys.Validation.MinQuantity);
            MaxQuantity = await GetStringSettingAsync(ReceivingSettingsKeys.Validation.MaxQuantity);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to load receiving validation settings.", Enum_ErrorSeverity.Warning, ex, false);
        }
    }

    private async Task<bool> GetBoolSettingAsync(string key, bool fallback)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (result.IsSuccess && result.Data != null && bool.TryParse(result.Data.Value, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private async Task<string> GetStringSettingAsync(string key)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data.Value;
        }

        if (ReceivingSettingsDefaults.StringDefaults.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        if (ReceivingSettingsDefaults.IntDefaults.TryGetValue(key, out var intFallback))
        {
            return intFallback.ToString();
        }

        return string.Empty;
    }

    private async Task SaveSettingAsync(string key, string value)
    {
        var result = await _settingsCore.SetSettingAsync(SettingsCategory, key, value ?? string.Empty, CurrentUserId);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Save {key}");
        }
    }

    private async Task ResetSettingAsync(string key)
    {
        var result = await _settingsCore.ResetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Reset {key}");
        }
    }
}
