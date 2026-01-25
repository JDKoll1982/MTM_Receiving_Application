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

            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePoNumber, RequirePoNumber.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePartId, RequirePartId.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireQuantity, RequireQuantity.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireHeatLot, RequireHeatLot.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.AllowNegativeQuantity, AllowNegativeQuantity.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePoExists, ValidatePoExists.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePartExists, ValidatePartExists.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnQuantityExceedsPo, WarnOnQuantityExceedsPo.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnSameDayReceiving, WarnOnSameDayReceiving.ToString());

            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinLoadCount, MinLoadCount);
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxLoadCount, MaxLoadCount);
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinQuantity, MinQuantity);
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxQuantity, MaxQuantity);

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

            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePoNumber);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePartId);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireQuantity);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireHeatLot);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.AllowNegativeQuantity);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePoExists);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePartExists);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnQuantityExceedsPo);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnSameDayReceiving);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinLoadCount);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxLoadCount);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinQuantity);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxQuantity);

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
            RequirePoNumber = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePoNumber, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePoNumber]);
            RequirePartId = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePartId, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePartId]);
            RequireQuantity = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireQuantity, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireQuantity]);
            RequireHeatLot = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireHeatLot, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireHeatLot]);
            AllowNegativeQuantity = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.AllowNegativeQuantity, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.AllowNegativeQuantity]);
            ValidatePoExists = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePoExists, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePoExists]);
            ValidatePartExists = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePartExists, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePartExists]);
            WarnOnQuantityExceedsPo = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnQuantityExceedsPo, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnQuantityExceedsPo]);
            WarnOnSameDayReceiving = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnSameDayReceiving, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnSameDayReceiving]);

            MinLoadCount = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinLoadCount);
            MaxLoadCount = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxLoadCount);
            MinQuantity = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinQuantity);
            MaxQuantity = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxQuantity);
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

        if (Helper_Receiving_Infrastructure_SettingsDefaults.StringDefaults.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        if (Helper_Receiving_Infrastructure_SettingsDefaults.IntDefaults.TryGetValue(key, out var intFallback))
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
