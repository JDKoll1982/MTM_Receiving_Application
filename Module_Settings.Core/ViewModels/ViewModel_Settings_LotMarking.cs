using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// One palette entry on the Heat / Lot Management settings page: a shape glyph or a color code,
/// with the switch that decides whether the palette may hand it out.
/// </summary>
public sealed partial class Model_Settings_LotMarkingPaletteOption : ObservableObject
{
    /// <summary>The persisted value: a shape glyph, or a color code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>The name the operator reads.</summary>
    public string DisplayName { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;
}

/// <summary>
/// Settings page ViewModel for Heat / Lot Management: the feature switch, what happens when two
/// lots collide, how a marking reads, which palette entries are offered, and the part list.
/// </summary>
public sealed partial class ViewModel_Settings_LotMarking : ViewModel_Shared_Base
{
    private const int PartCandidateLimit = 50;

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_ShipRecToolsSettings _shipRecToolsSettings;
    private readonly IService_Tool_LotMarking _lotMarkingService;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_SettingsErrorHandler _settingsErrorHandler;

    public ObservableCollection<Model_Settings_LotMarkingPaletteOption> ShapeOptions { get; } = [];

    public ObservableCollection<Model_Settings_LotMarkingPaletteOption> ColorOptions { get; } = [];

    public ObservableCollection<Model_Tool_LotMarkingScope> Parts { get; } = [];

    public IReadOnlyList<string> CollisionPolicyOptions { get; } =
        Enum.GetNames<Enum_LotMarkingCollisionPolicy>();

    [ObservableProperty]
    private bool _isFeatureEnabled = true;

    [ObservableProperty]
    private string _collisionPolicy = nameof(Enum_LotMarkingCollisionPolicy.Probe);

    [ObservableProperty]
    private bool _isShapeFirst = true;

    [ObservableProperty]
    private string _partSearchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemovePart))]
    [NotifyPropertyChangedFor(nameof(CanReEnablePart))]
    private Model_Tool_LotMarkingScope? _selectedPart;

    public bool CanRemovePart => SelectedPart is { IsEnabled: true } && !IsBusy;

    public bool CanReEnablePart => SelectedPart is { IsEnabled: false } && !IsBusy;

    /// <summary>
    /// Set by the view's code-behind so the page can confirm a new part number without taking a
    /// dependency on UI types. Parameters: (candidates, dialogTitle) → the confirmed candidate.
    /// </summary>
    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    public ViewModel_Settings_LotMarking(
        IService_SettingsCoreFacade settingsCore,
        IService_ShipRecToolsSettings shipRecToolsSettings,
        IService_Tool_LotMarking lotMarkingService,
        IService_InforVisual inforVisual,
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
        _lotMarkingService =
            lotMarkingService ?? throw new ArgumentNullException(nameof(lotMarkingService));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _settingsErrorHandler =
            settingsErrorHandler ?? throw new ArgumentNullException(nameof(settingsErrorHandler));

        foreach (var shape in Model_LotMarkingPalette.Shapes.OrderBy(shape => shape.SortOrder))
        {
            ShapeOptions.Add(
                new Model_Settings_LotMarkingPaletteOption
                {
                    Code = shape.Glyph,
                    DisplayName = shape.DisplayName,
                }
            );
        }

        foreach (var color in Model_LotMarkingPalette.Colors.OrderBy(color => color.SortOrder))
        {
            ColorOptions.Add(
                new Model_Settings_LotMarkingPaletteOption
                {
                    Code = color.Code,
                    DisplayName = color.DisplayName,
                }
            );
        }

        _ = LoadAsync();
    }

    partial void OnSelectedPartChanged(Model_Tool_LotMarkingScope? value)
    {
        RemovePartCommand.NotifyCanExecuteChanged();
        ReEnablePartCommand.NotifyCanExecuteChanged();
    }

    // ─── Save and reset ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.LotMarking.Enabled,
                IsFeatureEnabled.ToString().ToLowerInvariant()
            );
            await SaveSettingAsync(ShipRecToolsSettingsKeys.LotMarking.CollisionPolicy, CollisionPolicy);
            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.LotMarking.DisplayShapeFirst,
                IsShapeFirst.ToString().ToLowerInvariant()
            );
            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledShapes,
                JsonSerializer.Serialize(EnabledCodes(ShapeOptions))
            );
            await SaveSettingAsync(
                ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledColors,
                JsonSerializer.Serialize(EnabledCodes(ColorOptions))
            );

            await _settingsErrorHandler.ShowSuccessAsync(
                "Heat / Lot Management settings saved successfully.",
                "Save Successful"
            );
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to save Heat / Lot Management settings.",
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

            await ResetSettingAsync(ShipRecToolsSettingsKeys.LotMarking.Enabled);
            await ResetSettingAsync(ShipRecToolsSettingsKeys.LotMarking.CollisionPolicy);
            await ResetSettingAsync(ShipRecToolsSettingsKeys.LotMarking.DisplayShapeFirst);
            await ResetSettingAsync(ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledShapes);
            await ResetSettingAsync(ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledColors);

            await LoadAsync();

            await _settingsErrorHandler.ShowSuccessAsync(
                "Heat / Lot Management settings were put back to their defaults.",
                "Reset Successful"
            );
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to reset Heat / Lot Management settings.",
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

    // ─── Part list ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AddPartAsync()
    {
        var partId = await ConfirmPartAsync(PartSearchText);
        if (string.IsNullOrWhiteSpace(partId))
        {
            return;
        }

        try
        {
            IsBusy = true;

            var result = await _lotMarkingService.AddPartAsync(partId, Environment.UserName);
            if (!result.IsSuccess)
            {
                await _settingsErrorHandler.HandleErrorAsync(
                    result.ErrorMessage,
                    "Could Not Add Part",
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            PartSearchText = string.Empty;
            await LoadPartsAsync();
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to add the part to the Heat / Lot Management list.",
                "Add Part Error",
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemovePart))]
    private async Task RemovePartAsync()
    {
        if (SelectedPart is null)
        {
            return;
        }

        await SetSelectedPartEnabledAsync(isEnabled: false);
    }

    [RelayCommand(CanExecute = nameof(CanReEnablePart))]
    private async Task ReEnablePartAsync()
    {
        if (SelectedPart is null)
        {
            return;
        }

        await SetSelectedPartEnabledAsync(isEnabled: true);
    }

    private async Task SetSelectedPartEnabledAsync(bool isEnabled)
    {
        var partId = SelectedPart?.PartId ?? string.Empty;
        var id = SelectedPart?.Id ?? 0;

        try
        {
            IsBusy = true;

            var result = await _lotMarkingService.SetPartEnabledAsync(id, isEnabled);
            if (!result.IsSuccess)
            {
                await _settingsErrorHandler.HandleErrorAsync(
                    result.ErrorMessage,
                    "Could Not Change The Part",
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            await LoadPartsAsync();

            await _settingsErrorHandler.ShowSuccessAsync(
                isEnabled
                    ? $"{partId} receives markings again."
                    : $"{partId} no longer receives markings. Markings already recorded are unchanged.",
                "Part List Updated"
            );
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to change the Heat / Lot Management part list.",
                "Part List Error",
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string> ConfirmPartAsync(string searchText)
    {
        var term = searchText?.Trim() ?? string.Empty;
        if (term.Length == 0)
        {
            return string.Empty;
        }

        var candidates = await _inforVisual.FuzzySearchPartsAsync(term);
        if (!candidates.IsSuccess || candidates.Data is null)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                candidates.ErrorMessage,
                "Part Search Failed",
                Enum_ErrorSeverity.Warning
            );
            return string.Empty;
        }

        var list = candidates.Data.Take(PartCandidateLimit).ToList();
        if (list.Count == 0)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                $"No part numbers found matching '{term}'.",
                "No Matches",
                Enum_ErrorSeverity.Warning
            );
            return string.Empty;
        }

        if (list.Count == 1 || ShowFuzzyPickerAsync is null)
        {
            return list[0].Key;
        }

        var picked = await ShowFuzzyPickerAsync(list, "Select Part Number");
        return picked?.Key ?? string.Empty;
    }

    // ─── Loading ────────────────────────────────────────────────────────────

    private async Task LoadAsync()
    {
        IsFeatureEnabled = await _shipRecToolsSettings.GetLotMarkingEnabledAsync();
        CollisionPolicy = (await _shipRecToolsSettings.GetLotMarkingCollisionPolicyAsync())
            .ToString();
        IsShapeFirst = await _shipRecToolsSettings.GetLotMarkingShapeFirstAsync();

        var enabledShapes = await _shipRecToolsSettings.GetLotMarkingEnabledShapesAsync();
        var enabledColors = await _shipRecToolsSettings.GetLotMarkingEnabledColorsAsync();

        ApplySelection(ShapeOptions, enabledShapes);
        ApplySelection(ColorOptions, enabledColors);

        await LoadPartsAsync();
    }

    private async Task LoadPartsAsync()
    {
        var result = await _lotMarkingService.GetEnabledPartsAsync();
        if (!result.IsSuccess)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                result.ErrorMessage,
                "Could Not Load The Part List",
                Enum_ErrorSeverity.Warning
            );
            Parts.Clear();
            return;
        }

        Parts.Clear();
        foreach (var part in result.Data ?? [])
        {
            Parts.Add(part);
        }

        SelectedPart = null;
    }

    /// <summary>
    /// Applies the stored selection. An empty stored selection means the setting was never
    /// written, so every entry is offered rather than none.
    /// </summary>
    private void ApplySelection(
        IEnumerable<Model_Settings_LotMarkingPaletteOption> options,
        IReadOnlyCollection<string> enabledCodes
    )
    {
        var offered = enabledCodes.Count == 0;
        foreach (var option in options)
        {
            option.IsEnabled = offered || enabledCodes.Contains(option.Code);
        }
    }

    private static List<string> EnabledCodes(
        IEnumerable<Model_Settings_LotMarkingPaletteOption> options
    ) => options.Where(option => option.IsEnabled).Select(option => option.Code).ToList();

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
