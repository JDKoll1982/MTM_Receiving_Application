using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Heat / Lot Management tool: searches the marking ledger, lets a user correct
/// and protect a marking, and keeps the list of part numbers that receive markings.
/// </summary>
/// <remarks>
/// The markings shown here are the ones already recorded. Nothing on this screen recomputes a
/// marking, so a lot keeps the shape and color a handler has already seen.
/// </remarks>
public partial class ViewModel_Tool_LotMarking : ViewModel_Tool_Base
{
    private const int PartCandidateLimit = 50;

    private readonly IService_Tool_LotMarking _service;
    private readonly IService_ShipRecToolsSettings _settings;
    private readonly IService_InforVisual _inforVisual;
    private bool _isLoaded;

    public ViewModel_Tool_LotMarking(
        IService_Tool_LotMarking service,
        IService_ShipRecToolsSettings settings,
        IService_InforVisual inforVisual,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));

        ToolTitle = "Heat / Lot Management";
        ToolDescription =
            "Find a lot's shape and color, correct it when it is wrong, and choose which parts are marked.";
    }

    // ─── Feature switch and palette legend ──────────────────────────────────

    [ObservableProperty]
    private bool _isFeatureEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaletteSummary))]
    private ObservableCollection<Model_LotMarkingShape> _legendShapes = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaletteSummary))]
    private ObservableCollection<Model_LotMarkingColor> _legendColors = [];

    /// <summary>
    /// True when a marking reads shape first, then color. Only changes how a marking is written
    /// out; the stored shape and color are unaffected.
    /// </summary>
    [ObservableProperty]
    private bool _isShapeFirst = true;

    /// <summary>How many shape and color combinations the palette can hand out.</summary>
    public string PaletteSummary => $"{LegendShapes.Count} shape(s) × {LegendColors.Count} color(s)";

    // ─── Heat search and the marking ledger ─────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MarkingListEmptyText))]
    private string _heatSearchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMarkingListEmpty))]
    private ObservableCollection<Model_Tool_LotMarking> _markings = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeMarking))]
    [NotifyPropertyChangedFor(nameof(SelectedMarkingSummary))]
    private Model_Tool_LotMarking? _selectedMarking;

    /// <summary>True when no marking row is shown, whether from no data or no match.</summary>
    public bool IsMarkingListEmpty => Markings.Count == 0;

    /// <summary>
    /// The wording shown in place of the list. An empty ledger and a search that matched nothing
    /// are both normal, but they are not the same thing, so they do not read the same.
    /// </summary>
    public string MarkingListEmptyText =>
        string.IsNullOrWhiteSpace(HeatSearchText)
            ? "No lots have been marked yet."
            : "No marking found for that heat number.";

    /// <summary>
    /// A one-line reading of the selected lot, so a user can confirm they picked the right one
    /// before changing it.
    /// </summary>
    public string SelectedMarkingSummary =>
        SelectedMarking is null
            ? "Select a lot to change or protect its marking."
            : $"{SelectedMarking.ShapeCode} {SelectedMarking.ColorCode} · "
                + $"{SelectedMarking.HeatNumber} · supplier {SelectedMarking.SupplierId}"
                + (SelectedMarking.IsLocked ? " · locked" : string.Empty);

    // ─── Manual override ────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeMarking))]
    private string _overrideShape = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeMarking))]
    private string _overrideColor = string.Empty;

    /// <summary>
    /// Why the marking is being replaced. The procedure requires it, so Save stays disabled
    /// until it is filled in.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeMarking))]
    [NotifyCanExecuteChangedFor(nameof(SaveOverrideCommand))]
    private string _overrideReason = string.Empty;

    /// <summary>
    /// True when a lot is selected and a shape, a color and a reason have all been supplied.
    /// </summary>
    public bool CanChangeMarking =>
        SelectedMarking is not null
        && !string.IsNullOrWhiteSpace(OverrideShape)
        && !string.IsNullOrWhiteSpace(OverrideColor)
        && !string.IsNullOrWhiteSpace(OverrideReason)
        && !IsBusy;

    // ─── Enabled-part list ──────────────────────────────────────────────────

    [ObservableProperty]
    private string _partSearchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPartListEmpty))]
    private ObservableCollection<Model_Tool_LotMarkingScope> _parts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemovePart))]
    [NotifyPropertyChangedFor(nameof(CanReEnablePart))]
    private Model_Tool_LotMarkingScope? _selectedPart;

    /// <summary>True when the list has no rows at all.</summary>
    public bool IsPartListEmpty => Parts.Count == 0;

    /// <summary>True when the selected row is still an enabled part, so removing it has an effect.</summary>
    public bool CanRemovePart => SelectedPart is { IsEnabled: true } && !IsBusy;

    public bool CanReEnablePart => SelectedPart is { IsEnabled: false } && !IsBusy;

    // ─── Fuzzy picker bridge ────────────────────────────────────────────────

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can ask for a part to be confirmed without
    /// taking a dependency on UI types.
    /// Parameters: (candidates, dialogTitle) → the confirmed candidate, or null if cancelled.
    /// </summary>
    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    // ─── Property change plumbing ───────────────────────────────────────────

    partial void OnSelectedMarkingChanged(Model_Tool_LotMarking? value)
    {
        OverrideShape = value?.ShapeCode ?? string.Empty;
        OverrideColor = value?.ColorCode ?? string.Empty;
        OverrideReason = string.Empty;
        SaveOverrideCommand.NotifyCanExecuteChanged();
        ToggleLockCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPartChanged(Model_Tool_LotMarkingScope? value)
    {
        RemovePartCommand.NotifyCanExecuteChanged();
        ReEnablePartCommand.NotifyCanExecuteChanged();
    }

    partial void OnPartSearchTextChanged(string value)
    {
        AddPartCommand.NotifyCanExecuteChanged();
    }

    partial void OnHeatSearchTextChanged(string value)
    {
        SearchMarkingsCommand.NotifyCanExecuteChanged();
    }

    // ─── Activation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the palette, the ledger and the enabled-part list the first time the tool opens,
    /// and refreshes them on every later visit.
    /// </summary>
    public async void ActivateView()
    {
        if (_isLoaded)
        {
            await ReloadAsync();
            return;
        }

        _isLoaded = true;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadPaletteAsync();
        await LoadMarkingsAsync(heatSearch: null);
        await LoadPartsAsync(partSearch: null);
    }

    // ─── Palette legend ─────────────────────────────────────────────────────

    private async Task LoadPaletteAsync()
    {
        try
        {
            IsFeatureEnabled = await _settings.GetLotMarkingEnabledAsync();
            IsShapeFirst = await _settings.GetLotMarkingShapeFirstAsync();

            var enabledShapes = await _settings.GetLotMarkingEnabledShapesAsync();
            var enabledColors = await _settings.GetLotMarkingEnabledColorsAsync();

            var shapes = Model_LotMarkingPalette
                .Shapes.Where(shape => enabledShapes.Contains(shape.Glyph))
                .ToList();
            var colors = Model_LotMarkingPalette
                .Colors.Where(color => enabledColors.Contains(color.Code))
                .ToList();

            // An emptied palette would leave nothing to pick from, so fall back to the full set.
            LegendShapes = new ObservableCollection<Model_LotMarkingShape>(
                shapes.Count > 0 ? shapes : Model_LotMarkingPalette.Shapes
            );
            LegendColors = new ObservableCollection<Model_LotMarkingColor>(
                colors.Count > 0 ? colors : Model_LotMarkingPalette.Colors
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, nameof(LoadPaletteAsync), nameof(ViewModel_Tool_LotMarking));
            LegendShapes = new ObservableCollection<Model_LotMarkingShape>(Model_LotMarkingPalette.Shapes);
            LegendColors = new ObservableCollection<Model_LotMarkingColor>(Model_LotMarkingPalette.Colors);
        }
    }

    // ─── Marking ledger ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SearchMarkingsAsync()
    {
        await LoadMarkingsAsync(HeatSearchText);
    }

    private async Task LoadMarkingsAsync(string? heatSearch)
    {
        IsBusy = true;
        try
        {
            var result = string.IsNullOrWhiteSpace(heatSearch)
                ? await _service.GetAllMarkingsAsync()
                : await _service.SearchMarkingsByHeatAsync(heatSearch.Trim());

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);

                // A failed search leaves the rows already on screen, so a query that could not run
                // never looks like "this lot has no marking". Only the unfiltered load clears.
                if (string.IsNullOrWhiteSpace(heatSearch))
                {
                    Markings = [];
                }

                return;
            }

            var loaded = result.Data ?? [];
            Markings = new ObservableCollection<Model_Tool_LotMarking>(loaded);
            SelectedMarking = null;

            if (loaded.Count == 0)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(heatSearch)
                        ? "No lots have been marked yet."
                        : $"No lot matching '{heatSearch}' has a marking.",
                    InfoBarSeverity.Informational
                );
                return;
            }

            ShowStatus($"{loaded.Count} marking(s) found.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(LoadMarkingsAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The marking list could not be loaded.", InfoBarSeverity.Error);
            Markings = [];
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ─── Override and lock ──────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanChangeMarking))]
    private async Task SaveOverrideAsync()
    {
        if (SelectedMarking is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.OverrideMarkingAsync(
                SelectedMarking.Id,
                OverrideShape,
                OverrideColor,
                OverrideReason.Trim(),
                Environment.UserName
            );

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            var heat = SelectedMarking.HeatNumber;
            await LoadMarkingsAsync(HeatSearchText);
            ShowStatus($"The marking for {heat} was changed.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(SaveOverrideAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The marking could not be changed.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsMarkingSelected))]
    private async Task ToggleLockAsync()
    {
        if (SelectedMarking is null)
        {
            return;
        }

        var target = !SelectedMarking.IsLocked;
        IsBusy = true;
        try
        {
            var result = await _service.SetMarkingLockedAsync(
                SelectedMarking.Id,
                target,
                Environment.UserName
            );

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            SelectedMarking.IsLocked = target;
            ShowStatus(
                target
                    ? "The marking is locked and will not be recomputed."
                    : "The marking is unlocked.",
                InfoBarSeverity.Success
            );
            OnPropertyChanged(nameof(SelectedMarkingSummary));
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(ToggleLockAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The lock could not be changed.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool IsMarkingSelected() => SelectedMarking is not null && !IsBusy;

    // ─── Enabled-part list ──────────────────────────────────────────────────

    [RelayCommand]
    private async Task SearchPartsAsync()
    {
        await LoadPartsAsync(PartSearchText);
    }

    [RelayCommand(CanExecute = nameof(CanAddPart))]
    private async Task AddPartAsync()
    {
        var partId = await ConfirmPartAsync(PartSearchText);
        if (string.IsNullOrWhiteSpace(partId))
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.AddPartAsync(partId, Environment.UserName);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            PartSearchText = string.Empty;
            await LoadPartsAsync(partSearch: null);
            ShowStatus($"{partId} now receives markings.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(AddPartAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The part could not be added.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAddPart() => string.IsNullOrWhiteSpace(PartSearchText) is false && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRemovePart))]
    private async Task RemovePartAsync() => await RemovePartRowAsync(SelectedPart);

    /// <summary>
    /// Takes one part off the enabled list. The row comes straight from the list, so the view can
    /// offer removal on each row without the selection having to change first.
    /// </summary>
    /// <remarks>
    /// The part is deactivated, never deleted. A marking already recorded against it keeps its
    /// shape and color, which is what the handler on the floor is still looking at.
    /// </remarks>
    public async Task RemovePartRowAsync(Model_Tool_LotMarkingScope? row)
    {
        if (row is null || row.Id <= 0)
        {
            return;
        }

        var partId = row.PartId;
        IsBusy = true;
        try
        {
            var result = await _service.SetPartEnabledAsync(row.Id, false);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            await LoadPartsAsync(PartSearchText);
            ShowStatus(
                $"{partId} no longer receives markings. Markings already recorded are unchanged.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(RemovePartRowAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The part could not be removed.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReEnablePart))]
    private async Task ReEnablePartAsync()
    {
        if (SelectedPart is null)
        {
            return;
        }

        var partId = SelectedPart.PartId;
        IsBusy = true;
        try
        {
            var result = await _service.SetPartEnabledAsync(SelectedPart.Id, true);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            await LoadPartsAsync(PartSearchText);
            ShowStatus($"{partId} receives markings again.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(ReEnablePartAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The part could not be re-enabled.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPartsAsync(string? partSearch)
    {
        IsBusy = true;
        try
        {
            var result = string.IsNullOrWhiteSpace(partSearch)
                ? await _service.GetEnabledPartsAsync()
                : await _service.SearchPartsAsync(partSearch.Trim());

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                Parts = [];
                return;
            }

            Parts = new ObservableCollection<Model_Tool_LotMarkingScope>(result.Data ?? []);
            SelectedPart = null;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(LoadPartsAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The part list could not be loaded.", InfoBarSeverity.Error);
            Parts = [];
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Turns the typed search text into a confirmed part number, using the same fuzzy picker the
    /// other tools use. Returns an empty string when the user cancels.
    /// </summary>
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
            ShowStatus(candidates.ErrorMessage, InfoBarSeverity.Warning);
            return string.Empty;
        }

        var list = candidates.Data.Take(PartCandidateLimit).ToList();
        if (list.Count == 0)
        {
            ShowStatus($"No part numbers found matching '{term}'.", InfoBarSeverity.Warning);
            return string.Empty;
        }

        if (list.Count == 1)
        {
            return list[0].Key;
        }

        if (ShowFuzzyPickerAsync is null)
        {
            return list[0].Key;
        }

        var picked = await ShowFuzzyPickerAsync(list, "Select Part Number");
        if (picked is null)
        {
            ShowStatus("Search cancelled.", InfoBarSeverity.Informational);
            return string.Empty;
        }

        return picked.Key;
    }
}
