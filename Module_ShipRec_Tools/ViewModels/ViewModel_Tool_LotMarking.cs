using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
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
    private readonly IService_Help _helpService;
    private bool _isLoaded;

    public ViewModel_Tool_LotMarking(
        IService_Tool_LotMarking service,
        IService_ShipRecToolsSettings settings,
        IService_InforVisual inforVisual,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_Help helpService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _helpService = helpService ?? throw new ArgumentNullException(nameof(helpService));

        ToolTitle = "Heat / Lot Management";
        ToolDescription =
            "Find a lot's shape and color, correct it when it is wrong, and choose which parts are marked.";

        // Every column is shown until the saved choice is read, so the list is never briefly bound to
        // an empty column set on the way in.
        ApplyColumnSelection(Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys);
    }

    /// <summary>
    /// The tooltip a button shows, read from the shared help content so every icon button on this
    /// page is understood from the same place as the rest of the app's buttons.
    /// </summary>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

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

    // ─── Search and the marking ledger ──────────────────────────────────────

    /// <summary>
    /// What the list is narrowed to: a lot number, a part number, or part of either. One box answers
    /// both questions because a handler standing at a pile knows one of the two, not always the same one.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MarkingListEmptyText))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMarkingListEmpty))]
    [NotifyPropertyChangedFor(nameof(HasMarkings))]
    private ObservableCollection<Model_Tool_LotMarking> _markings = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedMarking))]
    [NotifyPropertyChangedFor(nameof(CanChangeSelectedMarking))]
    [NotifyPropertyChangedFor(nameof(CanChangeMarking))]
    [NotifyPropertyChangedFor(nameof(SelectedMarkingSummary))]
    private Model_Tool_LotMarking? _selectedMarking;

    /// <summary>True when no marking row is shown, whether from no data or no match.</summary>
    public bool IsMarkingListEmpty => Markings.Count == 0;

    /// <summary>True when the list holds rows the report can be printed from.</summary>
    public bool HasMarkings => Markings.Count > 0;

    /// <summary>
    /// True when a row is chosen, which is what enables the button that opens the marking window.
    /// </summary>
    public bool HasSelectedMarking => SelectedMarking is not null;

    /// <summary>
    /// True when the chosen lot has a recorded marking, so there is something to change.
    /// </summary>
    /// <remarks>
    /// A lot read straight off its receipt lines has no ledger row behind it, because its marking was
    /// never recorded. Nothing can be edited or protected on it until Update all lots records one, so
    /// the button that opens the editor stays off rather than opening a window whose save would fail.
    /// </remarks>
    public bool CanChangeSelectedMarking => SelectedMarking is { Id: > 0 };

    /// <summary>
    /// The wording shown in place of the list. An empty list and a search that matched nothing are
    /// both normal, but they are not the same thing, so they do not read the same. A search that
    /// matches nothing often means the lot is for material that is not on the part list, so the
    /// wording says what the list covers instead of implying the lot carries no marking.
    /// </summary>
    public string MarkingListEmptyText =>
        string.IsNullOrWhiteSpace(SearchText)
            ? "No lots came in for the parts on the list in these dates. Choose a part to see its lots."
            : "No lot in the list matches that heat, lot or part number. The list covers only material "
                + "received for the parts on the part list, so a lot whose material is not on that list "
                + "is not shown.";

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
    [NotifyPropertyChangedFor(nameof(CanFillUnmarkedLots))]
    private Model_Tool_LotMarkingScope? _selectedPart;

    /// <summary>True when the list has no rows at all.</summary>
    public bool IsPartListEmpty => Parts.Count == 0;

    /// <summary>True when the selected row is still an enabled part, so removing it has an effect.</summary>
    public bool CanRemovePart => SelectedPart is { IsEnabled: true } && !IsBusy;

    public bool CanReEnablePart => SelectedPart is { IsEnabled: false } && !IsBusy;

    /// <summary>
    /// True when the selected row is an enabled part, so its earlier unmarked lots can be filled in.
    /// A removed part is not offered, because adding a part back is a deliberate separate act.
    /// </summary>
    public bool CanFillUnmarkedLots => SelectedPart is { IsEnabled: true } && !IsBusy;

    // ─── Dialog bridges ─────────────────────────────────────────────────────

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

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can put the counted preview in front of a
    /// user before anything is written.
    /// Parameters: (partId, preview) → true when the user confirms. Null, or false, means nothing
    /// is written.
    /// </summary>
    public Func<string, Model_Tool_LotMarkingBackfillPreview, Task<bool>>? ConfirmBackfillAsync { get; set; }

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can put the plan for Update All Lots in front of
    /// a user before anything is written.
    /// Parameters: (preview) → true when the user confirms. Null, or false, means nothing is written.
    /// </summary>
    public Func<Model_Tool_LotMarkingRealignPreview, Task<bool>>? ConfirmRealignAsync { get; set; }

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can hand a finished report to the shell for
    /// printing without taking a dependency on UI types.
    /// </summary>
    public Func<Model_FormattedReportDocument, Task<Model_Dao_Result<bool>>>? RequestPrintAsync
    {
        get;
        set;
    }

    /// <summary>
    /// Set by the marking window so the ViewModel can ask to be closed once the change is saved and
    /// there is nothing left to edit on the lot it was opened for.
    /// </summary>
    public Action? RequestCloseChangeMarking { get; set; }

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
        FillUnmarkedLotsCommand.NotifyCanExecuteChanged();

        // Choosing a part is how the list is read for one material, so the list follows the choice
        // rather than waiting for the search button. Reading the part list also clears the selection,
        // and that clearing happens while the view model is busy, so it is not read as a user
        // choosing nothing and does not send the list back to the whole scope.
        // The read is started and not awaited because a property setter cannot be asynchronous.
        // LoadMarkingsAsync handles its own failures, so nothing here can escape. The read is also
        // short, and setting IsBusy while it runs keeps a second selection from overlapping it.
        if (!IsBusy)
        {
            _ = LoadMarkingsAsync(SearchText);
        }
    }

    partial void OnPartSearchTextChanged(string value)
    {
        AddPartCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTextChanged(string value)
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
        await LoadListPreferencesAsync();
        await LoadMarkingsAsync(search: null);
        await LoadPartsAsync(partSearch: null);
    }

    /// <summary>
    /// Reads the lot list again after the enabled-part list was edited in its own window. The parts a
    /// user can see on the lot list are decided by that list, so the lot list is stale the moment it
    /// changes and is read again rather than trusted.
    /// </summary>
    public async Task ReloadAfterPartScopeChangeAsync()
    {
        await LoadPartsAsync(PartSearchText);
        await LoadMarkingsAsync(SearchText);
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
        await LoadMarkingsAsync(SearchText);
    }

    private async Task LoadMarkingsAsync(string? search)
    {
        var partId = SelectedPart?.PartId;

        IsBusy = true;
        try
        {
            var result = await _service.GetLedgerAsync(search, partId, BuildFilter());

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);

                // A failed search leaves the rows already on screen, so a query that could not run
                // never looks like "this lot has no marking". Only the unfiltered load clears.
                if (string.IsNullOrWhiteSpace(search))
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
                ShowStatus(MarkingListEmptyText, InfoBarSeverity.Informational);
                return;
            }

            ShowStatus(
                string.IsNullOrWhiteSpace(partId)
                    ? $"{loaded.Count} marking(s) found."
                    : $"{loaded.Count} marking(s) for {partId}.",
                InfoBarSeverity.Success
            );
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

        // A lot read straight off its receipt lines has no ledger row behind it, so there is nothing
        // to change yet. Saying so beats letting the write fail and reporting the row as not found.
        if (SelectedMarking.Id <= 0)
        {
            ShowStatus(
                "This lot was received but its marking was never recorded. Use Update all lots to "
                    + "record one, then change it.",
                InfoBarSeverity.Informational
            );
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
            await LoadMarkingsAsync(SearchText);
            ShowStatus($"The marking for {heat} was changed.", InfoBarSeverity.Success);
            RequestCloseChangeMarking?.Invoke();
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

    // ─── Report ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends what the list is showing to the printer. The report covers the rows on screen, so a
    /// search or a chosen part is reflected in what is printed, and nothing is read or written beyond
    /// what is already displayed.
    /// </summary>
    [RelayCommand]
    public async Task PrintMarkingsAsync()
    {
        if (RequestPrintAsync is null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await RequestPrintAsync(BuildReportDocument());
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            ShowStatus(
                $"{Markings.Count} lot(s) sent to the print dialog.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(PrintMarkingsAsync),
                nameof(ViewModel_Tool_LotMarking)
            );
            ShowStatus("The report could not be printed.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Builds the printable report from the rows on screen, using the columns the grid shows so the
    /// printed sheet and the screen cannot disagree. The filter line records what the list was
    /// narrowed by, so a sheet printed from a filtered list cannot later be mistaken for a full one.
    /// </summary>
    private Model_FormattedReportDocument BuildReportDocument()
    {
        var visible = VisibleColumnKeys;
        var html = new StringBuilder();

        html.AppendLine("<h1>Heat / Lot Markings</h1>");
        html.AppendLine(
            $"<p><strong>Printed:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}"
                + $" &nbsp; <strong>By:</strong> {WebUtility.HtmlEncode(Environment.UserName)}</p>"
        );

        html.AppendLine(
            $"<p><strong>Dates:</strong> {WebUtility.HtmlEncode(DescribeDateRange())}"
                + $" &nbsp; <strong>Sorted by:</strong> {WebUtility.HtmlEncode(SelectedSortOption.DisplayName)}"
                + $" ({WebUtility.HtmlEncode(SortDirectionLabel.ToLowerInvariant())})</p>"
        );

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            html.AppendLine(
                $"<p><strong>Search:</strong> {WebUtility.HtmlEncode(SearchText.Trim())}</p>"
            );
        }

        if (SelectedPart is not null)
        {
            html.AppendLine(
                $"<p><strong>Part:</strong> {WebUtility.HtmlEncode(SelectedPart.PartId)}</p>"
            );
        }

        html.AppendLine($"<p><strong>Lots on this sheet:</strong> {Markings.Count}</p>");

        html.AppendLine("<table><thead><tr>");

        foreach (var column in Model_Tool_LotMarkingColumnCatalog.All)
        {
            if (!IsColumnVisible(column.Key))
            {
                continue;
            }

            var cssClass = column.Key == Model_Tool_LotMarkingColumnCatalog.Skids
                ? " class=\"number\""
                : string.Empty;

            html.AppendLine($"<th{cssClass}>{WebUtility.HtmlEncode(column.DisplayName)}</th>");
        }

        html.AppendLine("</tr></thead><tbody>");

        foreach (var row in Markings)
        {
            html.AppendLine("<tr>");

            foreach (var column in Model_Tool_LotMarkingColumnCatalog.All)
            {
                if (!IsColumnVisible(column.Key))
                {
                    continue;
                }

                html.AppendLine(BuildReportCell(row, column.Key));
            }

            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>");

        return new Model_FormattedReportDocument
        {
            DocumentTitle = "Heat / Lot Markings",
            HtmlFragment = html.ToString(),
            PageCss =
                "@page { size: Letter portrait; margin: 0.4in; } "
                + "body { font-family: 'Segoe UI', sans-serif; font-size: 11pt; color: #202124; } "
                + "h1 { font-size: 16pt; margin: 0 0 8pt 0; } "
                + "p { margin: 2pt 0; } "
                + "table { width: 100%; border-collapse: collapse; margin-top: 10pt; } "
                + "th, td { border: 1px solid #c8c8c8; padding: 4pt 6pt; text-align: left; vertical-align: top; } "
                + "th { background: #f0f0f0; } "
                + "th.number { text-align: right; } "
                // The shade glyph is an emoji codepoint, so a browser would paint its colour layers and the
                // report would show a red heart where the paper shows a black one. brightness(0) drops every
                // channel to black and keeps the outline, which is how the overlay and the cards print it.
                + "td.shape { font-family: 'Segoe UI Emoji', 'Segoe UI', sans-serif; font-size: 13pt; color: #000; filter: brightness(0); } "
                + "td.number { text-align: right; }",
        };
    }

    /// <summary>One table cell, chosen by column key so header and body can never drift apart.</summary>
    private static string BuildReportCell(Model_Tool_LotMarking row, string columnKey)
    {
        var (cssClass, value) = columnKey switch
        {
            Model_Tool_LotMarkingColumnCatalog.Shape => ("shape", row.ShapeCode),
            Model_Tool_LotMarkingColumnCatalog.Color => (string.Empty, row.ColorCode),
            Model_Tool_LotMarkingColumnCatalog.Heat => (string.Empty, row.HeatNumber),
            Model_Tool_LotMarkingColumnCatalog.Supplier => (string.Empty, row.SupplierId),
            Model_Tool_LotMarkingColumnCatalog.Parts => (string.Empty, row.PartsText),
            Model_Tool_LotMarkingColumnCatalog.Skids => ("number", row.LineCountText),
            Model_Tool_LotMarkingColumnCatalog.LastLine => (string.Empty, row.LastLineText),
            _ => (string.Empty, string.Empty),
        };

        var escaped = WebUtility.HtmlEncode(value);
        var attribute = string.IsNullOrEmpty(cssClass) ? string.Empty : $" class=\"{cssClass}\"";

        return $"<td{attribute}>{escaped}</td>";
    }

    /// <summary>The date range in plain words, for the report header.</summary>
    private string DescribeDateRange()
    {
        if (!HasActiveDateRange)
        {
            return "All recorded dates";
        }

        return $"{FormatDay(FromDate)} to {FormatDay(ToDate)}";
    }

    // ─── List filters, ordering, and visible columns ─────────────────────────

    /// <summary>The sort choices the Filter window offers.</summary>
    public IReadOnlyList<Model_Tool_LotMarkingSortOption> SortOptions =>
        Model_Tool_LotMarkingSortOption.All;

    /// <summary>The quick date ranges the Filter window offers.</summary>
    public IReadOnlyList<Model_DateRangePreset> DateRangePresets => Helper_DateRangePresets.All;

    /// <summary>
    /// The one column set the grid and the printed report both read, so a column turned off here is
    /// gone from the screen and the paper rather than from only one of them.
    /// </summary>
    public ObservableCollection<Model_Tool_LotMarkingColumnOption> ColumnOptions { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilterSummary))]
    private DateTimeOffset? _fromDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilterSummary))]
    private DateTimeOffset? _toDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilterSummary))]
    [NotifyPropertyChangedFor(nameof(SortDirectionLabel))]
    private Model_Tool_LotMarkingSortOption _selectedSortOption =
        Model_Tool_LotMarkingSortOption.All[0];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilterSummary))]
    [NotifyPropertyChangedFor(nameof(SortDirectionLabel))]
    private bool _isSortDescending = true;

    /// <summary>True when the list is limited by date rather than reading every recorded lot.</summary>
    public bool HasActiveDateRange => FromDate is not null || ToDate is not null;

    /// <summary>What the direction control reads, worded for the chosen column.</summary>
    public string SortDirectionLabel => IsSortDescending ? "Highest first" : "Lowest first";

    /// <summary>
    /// A one-line reading of what the list is showing, so the page says plainly when it is narrowed
    /// rather than leaving a user to wonder why lots are missing.
    /// </summary>
    public string FilterSummary
    {
        get
        {
            var datePart = HasActiveDateRange
                ? $"{FormatDay(FromDate)} to {FormatDay(ToDate)}"
                : "All dates";

            return $"{datePart} · {SelectedSortOption.DisplayName} · {SortDirectionLabel}";
        }
    }

    /// <summary>True when anything narrows the list, which is what the Filter button reflects.</summary>
    public bool HasAnyFilter => HasActiveDateRange;

    /// <summary>
    /// Reads the date range, order and column choice the signed-in user last used. A first run has
    /// nothing saved, so the defaults come back and the list opens showing every column, newest first.
    /// </summary>
    private async Task LoadListPreferencesAsync()
    {
        try
        {
            var filter = await _settings.GetLotMarkingListFilterAsync();
            var visibleKeys = await _settings.GetLotMarkingListVisibleColumnKeysAsync();

            FromDate = filter.FromDate is null ? null : new DateTimeOffset(filter.FromDate.Value);
            ToDate = filter.ToDate is null ? null : new DateTimeOffset(filter.ToDate.Value);
            SelectedSortOption = Model_Tool_LotMarkingSortOption.FromField(filter.SortField);
            IsSortDescending = filter.IsDescending;

            ApplyColumnSelection(visibleKeys);
            RefreshFilterSummary();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(LoadListPreferencesAsync),
                nameof(ViewModel_Tool_LotMarking)
            );
        }
    }

    /// <summary>Rebuilds the column list, marking the chosen columns as shown.</summary>
    private void ApplyColumnSelection(IReadOnlyCollection<string> visibleKeys)
    {
        var visible = Model_Tool_LotMarkingColumnCatalog.Normalize(visibleKeys);

        ColumnOptions.Clear();

        foreach (var column in Model_Tool_LotMarkingColumnCatalog.All)
        {
            ColumnOptions.Add(
                new Model_Tool_LotMarkingColumnOption(
                    column.Key,
                    column.DisplayName,
                    visible.Contains(column.Key)
                )
            );
        }
    }

    /// <summary>The column keys currently chosen to show, in display order.</summary>
    public IReadOnlyList<string> VisibleColumnKeys =>
        Model_Tool_LotMarkingColumnCatalog
            .All.Where(column => IsColumnVisible(column.Key))
            .Select(column => column.Key)
            .ToList();

    /// <summary>Whether one column is currently shown. Answers true before the list is built.</summary>
    public bool IsColumnVisible(string key)
    {
        foreach (var option in ColumnOptions)
        {
            if (string.Equals(option.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return option.IsVisible;
            }
        }

        return true;
    }

    /// <summary>Whether the Shape column is shown. Bound by the grid.</summary>
    public bool ShowShapeColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Shape);

    /// <summary>Whether the Color column is shown. Bound by the grid.</summary>
    public bool ShowColorColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Color);

    /// <summary>Whether the Heat column is shown. Bound by the grid.</summary>
    public bool ShowHeatColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Heat);

    /// <summary>Whether the Supplier column is shown. Bound by the grid.</summary>
    public bool ShowSupplierColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Supplier);

    /// <summary>Whether the Parts column is shown. Bound by the grid.</summary>
    public bool ShowPartsColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Parts);

    /// <summary>Whether the Skids column is shown. Bound by the grid.</summary>
    public bool ShowSkidsColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Skids);

    /// <summary>Whether the Last received column is shown. Bound by the grid.</summary>
    public bool ShowLastLineColumn => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.LastLine);

    /// <summary>Tells the grid that the chosen columns moved.</summary>
    private void RefreshVisibleColumns()
    {
        OnPropertyChanged(nameof(ShowShapeColumn));
        OnPropertyChanged(nameof(ShowColorColumn));
        OnPropertyChanged(nameof(ShowHeatColumn));
        OnPropertyChanged(nameof(ShowSupplierColumn));
        OnPropertyChanged(nameof(ShowPartsColumn));
        OnPropertyChanged(nameof(ShowSkidsColumn));
        OnPropertyChanged(nameof(ShowLastLineColumn));
        OnPropertyChanged(nameof(VisibleColumnKeys));
    }

    /// <summary>
    /// Saves the chosen range, order and columns for this user, then reads the list again. Nothing is
    /// written to the marking ledger by this: the choices are preferences, not records.
    /// </summary>
    public async Task ApplyFiltersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (FromDate is not null && ToDate is not null && FromDate > ToDate)
        {
            ShowStatus(
                "The start date is after the end date, so nothing can match. Swap them or clear one.",
                InfoBarSeverity.Warning
            );
            return;
        }

        IsBusy = true;
        try
        {
            var filter = BuildFilter();
            var visibleKeys = VisibleColumnKeys;

            var saved = await _settings.SaveLotMarkingListFilterAsync(filter);
            if (!saved.IsSuccess)
            {
                ShowStatus($"The filter could not be saved. {saved.ErrorMessage}", InfoBarSeverity.Warning);
            }

            var savedColumns = await _settings.SaveLotMarkingListVisibleColumnKeysAsync(visibleKeys);
            if (!savedColumns.IsSuccess)
            {
                ShowStatus(
                    $"The column choice could not be saved. {savedColumns.ErrorMessage}",
                    InfoBarSeverity.Warning
                );
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ApplyFiltersAsync),
                nameof(ViewModel_Tool_LotMarking)
            );
            ShowStatus("The filter could not be applied.", InfoBarSeverity.Error);
            return;
        }
        finally
        {
            IsBusy = false;
        }

        RefreshFilterSummary();
        await LoadMarkingsAsync(SearchText);
    }

    /// <summary>Puts the list back to every date, newest first, and every column shown.</summary>
    [RelayCommand]
    public void ResetFilters()
    {
        FromDate = null;
        ToDate = null;
        SelectedSortOption = Model_Tool_LotMarkingSortOption.All[0];
        IsSortDescending = true;

        ApplyColumnSelection(Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys);
        RefreshFilterSummary();
    }

    /// <summary>Applies a quick date range and leaves it on screen for the user to confirm.</summary>
    public void ApplyDateRangePreset(string? presetKey)
    {
        if (string.IsNullOrWhiteSpace(presetKey))
        {
            return;
        }

        var (from, to) = Helper_DateRangePresets.Resolve(presetKey);

        FromDate = new DateTimeOffset(from);
        ToDate = new DateTimeOffset(to);
        RefreshFilterSummary();
    }

    /// <summary>
    /// Recomputes the derived reading of the filter. Called after the column list is rebuilt, because
    /// those properties do not otherwise know that the underlying state moved.
    /// </summary>
    public void RefreshFilterSummary()
    {
        OnPropertyChanged(nameof(FilterSummary));
        OnPropertyChanged(nameof(HasActiveDateRange));
        OnPropertyChanged(nameof(HasAnyFilter));
        OnPropertyChanged(nameof(SortDirectionLabel));
        RefreshVisibleColumns();
    }

    /// <summary>The filter the service is asked to read the list by.</summary>
    private Model_Tool_LotMarkingFilter BuildFilter() =>
        new(
            FromDate?.Date,
            ToDate?.Date,
            SelectedSortOption.Field,
            IsSortDescending
        );

    private static string FormatDay(DateTimeOffset? value) =>
        value is null ? "any" : value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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

    // ─── Filling in the lots that were never marked ─────────────────────────

    /// <summary>
    /// Gives a marking to the earlier lots of the selected enabled part. The counts are gathered
    /// first and shown to the user, and nothing is written until that preview is confirmed.
    /// </summary>
    /// <remarks>
    /// Which rows qualify, how lots group, and which rows are skipped are all the service's
    /// decisions. This method only asks, confirms, and reports.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanFillUnmarkedLots))]
    private async Task FillUnmarkedLotsAsync()
    {
        var part = SelectedPart;
        if (part is null)
        {
            return;
        }

        var partId = part.PartId;
        IsBusy = true;
        try
        {
            ShowStatus($"Counting the unmarked lots of {partId}...", InfoBarSeverity.Informational);

            var preview = await _service.PreviewUnmarkedLotMarkingsAsync(partId);
            if (!preview.IsSuccess || preview.Data is null)
            {
                ShowStatus(preview.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            if (preview.Data.DistinctLots == 0)
            {
                ShowStatus($"Every lot of {partId} already has a marking.", InfoBarSeverity.Informational);
                return;
            }

            if (ConfirmBackfillAsync is null || !await ConfirmBackfillAsync(partId, preview.Data))
            {
                ShowStatus("Cancelled.", InfoBarSeverity.Informational);
                return;
            }

            var filled = await _service.FillUnmarkedLotMarkingsAsync(partId, Environment.UserName);
            if (!filled.IsSuccess)
            {
                ShowStatus(filled.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            await LoadMarkingsAsync(SearchText);
            await LoadPartsAsync(PartSearchText);

            ShowStatus(
                $"{partId}: {preview.Data.DistinctLots} lot(s) marked across {filled.Data} row(s).",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(FillUnmarkedLotsAsync), nameof(ViewModel_Tool_LotMarking));
            ShowStatus("The unmarked lots could not be filled in.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Puts the markings right after lot numbers have been corrected on the receiving rows: a row is
    /// given the marking its lot number now calls for, a lot number that had none becomes a lot, and a
    /// lot whose number no longer appears on any receiving row is removed from the list so its shape
    /// and color are free again.
    /// </summary>
    /// <remarks>
    /// The plan is worked out and shown first, and nothing is written until it is confirmed. This is
    /// never triggered by saving an edit, and it is safe to run again at any time.
    /// </remarks>
    [RelayCommand]
    private async Task UpdateAllLotsAsync()
    {
        // The command carries no CanExecute, because IsBusy lives on the shared base and cannot be
        // watched from here, so it guards itself instead.
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            ShowStatus("Working out what needs putting right...", InfoBarSeverity.Informational);

            var preview = await _service.PreviewUpdateAllLotsAsync();
            if (!preview.IsSuccess || preview.Data is null)
            {
                ShowStatus(preview.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            if (!preview.Data.HasWork)
            {
                ShowStatus(
                    $"Nothing to put right. {preview.Data.LotsAlreadyCorrect} lot(s) already agree with the rows that carry them.",
                    InfoBarSeverity.Informational
                );
                return;
            }

            if (ConfirmRealignAsync is null || !await ConfirmRealignAsync(preview.Data))
            {
                ShowStatus("Cancelled.", InfoBarSeverity.Informational);
                return;
            }

            var applied = await _service.UpdateAllLotsAsync(Environment.UserName);
            if (!applied.IsSuccess || applied.Data is null)
            {
                ShowStatus(applied.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            await LoadMarkingsAsync(SearchText);
            await LoadPartsAsync(PartSearchText);

            var result = applied.Data;
            var summary =
                $"{result.RowsChanged} row(s) changed, {result.RowsCleared} cleared, {result.LotsCreated} lot(s) created, {result.LotsRemoved} removed from the list.";

            if (result.LotsSkipped > 0)
            {
                summary += $" {result.LotsSkipped} protected lot(s) left alone.";
            }

            ShowStatus(summary, InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(UpdateAllLotsAsync),
                nameof(ViewModel_Tool_LotMarking)
            );
            ShowStatus("The markings could not be put right.", InfoBarSeverity.Error);
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
