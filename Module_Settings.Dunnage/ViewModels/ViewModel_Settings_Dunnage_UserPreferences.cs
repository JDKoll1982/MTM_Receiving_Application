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
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_UserPreferences : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Dunnage";
    private const string WarehouseCode = "002";
    private const string FallbackDefaultLocation = "RECV";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ReceivingValidation _receivingValidation;
    private readonly IService_InforVisual _inforVisualService;

    [ObservableProperty]
    private string _defaultLocation = FallbackDefaultLocation;

    public ViewModel_Settings_Dunnage_UserPreferences(
        IService_SettingsCoreFacade settingsCore,
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
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        Title = "Dunnage User Preferences";

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

            var result = await _settingsCore.SetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                DefaultLocation.Trim(),
                CurrentUserId
            );

            if (!result.IsSuccess)
            {
                ShowStatus(
                    result.ErrorMessage ?? "Failed to save the default Dunnage location.",
                    InfoBarSeverity.Error
                );
                return;
            }

            ShowStatus("Default Dunnage location saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save the default Dunnage location.",
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

            var result = await _settingsCore.ResetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                CurrentUserId
            );

            if (!result.IsSuccess)
            {
                ShowStatus(
                    result.ErrorMessage ?? "Failed to reset the default Dunnage location.",
                    InfoBarSeverity.Error
                );
                return;
            }

            await LoadSettingsAsync();
            ShowStatus("Default Dunnage location reset.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset the default Dunnage location.",
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
            var result = await _settingsCore.GetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.UserPreferences.DefaultLocation,
                CurrentUserId
            );

            DefaultLocation =
                result.IsSuccess && result.Data is not null
                    ? string.IsNullOrWhiteSpace(result.Data.Value)
                        ? FallbackDefaultLocation
                        : result.Data.Value.Trim()
                    : FallbackDefaultLocation;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading Dunnage user preferences: {ex.Message}", ex);
            DefaultLocation = FallbackDefaultLocation;
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
