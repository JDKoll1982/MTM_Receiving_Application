using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_PersonalDefaults : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Dunnage";
    private const string WarehouseCode = "002";
    private const string FallbackDefaultLocation = "RECV";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ReceivingValidation _receivingValidation;
    private readonly IService_InforVisual _inforVisualService;

    [ObservableProperty]
    private string _defaultLocation = FallbackDefaultLocation;

    [ObservableProperty]
    private int _preferredThumbnailSize = 96;

    [ObservableProperty]
    private bool _preferPartImages = true;

    public ViewModel_Settings_Dunnage_PersonalDefaults(
        IService_SettingsCoreFacade settingsCore,
        IService_DunnageSettings dunnageSettings,
        IService_UserSessionManager sessionManager,
        IService_ReceivingValidation receivingValidation,
        IService_InforVisual inforVisualService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _dunnageSettings =
            dunnageSettings ?? throw new ArgumentNullException(nameof(dunnageSettings));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        Title = "Dunnage Personal Defaults";

        _ = LoadSettingsAsync();
    }

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            var validation = await ValidateLocationAsync();
            if (!validation.IsValid)
            {
                ShowStatus(validation.Message, InfoBarSeverity.Warning);
                return;
            }

            var locationResult = await _settingsCore.SetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                DefaultLocation.Trim(),
                CurrentUserId
            );
            if (!locationResult.IsSuccess)
            {
                ShowStatus(
                    locationResult.ErrorMessage ?? "Failed to save the default Dunnage location.",
                    InfoBarSeverity.Error
                );
                return;
            }

            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize,
                PreferredThumbnailSize.ToString(),
                CurrentUserId
            );
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UserPreferences.PreferPartImages,
                PreferPartImages.ToString(),
                CurrentUserId
            );

            ShowStatus("Dunnage personal defaults saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage personal defaults.",
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
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                CurrentUserId
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize,
                CurrentUserId
            );
            await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.PreferPartImages,
                CurrentUserId
            );

            await LoadSettingsAsync();
            ShowStatus("Dunnage personal defaults reset.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset Dunnage personal defaults.",
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

    public async Task<Model_ReceivingValidationResult> ValidateLocationAsync()
    {
        var locationToValidate = string.IsNullOrWhiteSpace(DefaultLocation)
            ? FallbackDefaultLocation
            : DefaultLocation.Trim();

        var validation = await _receivingValidation.ValidateLocationAsync(
            locationToValidate,
            WarehouseCode
        );

        if (validation.IsValid)
        {
            DefaultLocation = locationToValidate;
        }

        return validation;
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetLocationSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(DefaultLocation))
        {
            return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
        }

        if (_receivingValidation.UseMockLocationList)
        {
            var normalizedSearch = NormalizeLocationForMatch(DefaultLocation);
            var locationSuggestions = _receivingValidation
                .PresetLocations.Where(presetLocation =>
                {
                    var normalizedPreset = NormalizeLocationForMatch(presetLocation);
                    return normalizedPreset.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    );
                })
                .OrderByDescending(presetLocation =>
                    presetLocation.StartsWith(
                        DefaultLocation.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ThenBy(presetLocation => presetLocation, StringComparer.OrdinalIgnoreCase)
                .Select(presetLocation => new Model_FuzzySearchResult
                {
                    Key = presetLocation,
                    Label = presetLocation,
                    Detail = "Preset mock location",
                })
                .ToList();

            return Model_Dao_Result_Factory.Success(locationSuggestions);
        }

        var fuzzyResult = await _inforVisualService.FuzzySearchLocationsAsync(
            DefaultLocation.Trim(),
            WarehouseCode
        );
        if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
        {
            return fuzzyResult;
        }

        var suggestions = fuzzyResult
            .Data.Where(static result => string.IsNullOrWhiteSpace(result.Label) is false)
            .GroupBy(static result => result.Label.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();

        return Model_Dao_Result_Factory.Success(suggestions);
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var locationResult = await _settingsCore.GetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                CurrentUserId
            );

            DefaultLocation =
                locationResult.IsSuccess && locationResult.Data is not null
                    ? string.IsNullOrWhiteSpace(locationResult.Data.Value)
                        ? FallbackDefaultLocation
                        : locationResult.Data.Value.Trim()
                    : FallbackDefaultLocation;

            PreferredThumbnailSize = await _dunnageSettings.GetIntAsync(
                DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize,
                CurrentUserId
            );
            if (PreferredThumbnailSize <= 0)
            {
                PreferredThumbnailSize = 96;
            }

            PreferPartImages = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.UserPreferences.PreferPartImages,
                CurrentUserId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading Dunnage personal defaults: {ex.Message}", ex);
            DefaultLocation = FallbackDefaultLocation;
            PreferredThumbnailSize = 96;
            PreferPartImages = true;
        }
    }

    private static string NormalizeLocationForMatch(string? location)
    {
        return new string(
            (location ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray()
        );
    }
}
