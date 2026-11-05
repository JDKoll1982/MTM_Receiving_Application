using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels;

/// <summary>
/// ViewModel for the label overlay mode: choose which pre-printed tags to run through the printer, choose
/// the printer and the print setup, then print the lot marking onto each tag or a sheet of cut-out cards.
/// </summary>
/// <remarks>
/// The print setup is remembered per PC, in a local file, because it describes the printer standing next
/// to the operator rather than the operator themselves. Every change saves immediately, so a setting only
/// has to be made once.
/// </remarks>
public partial class ViewModel_Receiving_LabelOverlay : ViewModel_Shared_Base
{
    private readonly IService_ReceivingLabelOverlay _overlayService;
    private readonly IService_ReceivingLabelOverlaySettings _settingsService;

    /// <summary>
    /// True while saved settings are being put back into the screen. Their own change notifications must
    /// not save, or opening the screen would overwrite what it is still loading.
    /// </summary>
    private bool _isApplyingPrintSettings;

    /// <summary>Keeps the fire-and-forget saves to the setup file in the order they were started.</summary>
    private readonly SemaphoreSlim _printSettingsSaveGate = new(1, 1);

    private string _savedPaperName = string.Empty;
    private float _savedPaperWidth = Model_LabelOverlayPrintSettings.DefaultPaperWidthInches;
    private float _savedPaperHeight = Model_LabelOverlayPrintSettings.DefaultPaperHeightInches;
    private string _savedPaperSourceName = string.Empty;
    private short _savedPaperSourceRawKind;

    public ViewModel_Receiving_LabelOverlay(
        IService_ReceivingLabelOverlay overlayService,
        IService_ReceivingLabelOverlaySettings settingsService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _overlayService = overlayService ?? throw new ArgumentNullException(nameof(overlayService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        _profiles[Enum_LabelOverlayPrintFormat.TagOverlay] = new()
        {
            Format = Enum_LabelOverlayPrintFormat.TagOverlay,
        };
        _profiles[Enum_LabelOverlayPrintFormat.CardSheet] = new()
        {
            Format = Enum_LabelOverlayPrintFormat.CardSheet,
        };

        ApplyGeometry(Model_LabelOverlayGeometry.Default);
        UpdateSelectionCounts();
    }

    /// <summary>
    /// Each output's printer setup, held apart so the overlay can print from one tray while the cut-out
    /// cards print from another. Both are kept per user in the settings database.
    /// </summary>
    private readonly Dictionary<
        Enum_LabelOverlayPrintFormat,
        Model_LabelOverlayPrintSettings
    > _profiles = [];

    /// <summary>The setup that belongs to the output currently being configured.</summary>
    private Model_LabelOverlayPrintSettings ActiveProfile => _profiles[ActiveFormat];

    /// <summary>The output whose setup the print controls are showing.</summary>
    private Enum_LabelOverlayPrintFormat ActiveFormat =>
        SettingsTargetIndex == 1
            ? Enum_LabelOverlayPrintFormat.CardSheet
            : Enum_LabelOverlayPrintFormat.TagOverlay;

    // ─── Labels available to overlay ────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLabelListEmpty))]
    private ObservableCollection<Model_ReceivingLabelOverlayItem> _labels = [];

    /// <summary>True when the label queue holds nothing to overlay.</summary>
    public bool IsLabelListEmpty => Labels.Count == 0;

    /// <summary>
    /// Every tag in the queue, before the tab's filter is applied. Kept so narrowing the list by date or
    /// part never throws away what was loaded.
    /// </summary>
    private List<Model_ReceivingLabelOverlayItem> _allLabels = [];

    // ─── Per-tab filters ────────────────────────────────────────────────────

    private readonly Dictionary<
        Enum_LabelOverlayPrintFormat,
        Model_LabelOverlayListFilter
    > _filters = new()
    {
        [Enum_LabelOverlayPrintFormat.TagOverlay] = Model_LabelOverlayListFilter.Default,
        [Enum_LabelOverlayPrintFormat.CardSheet] = Model_LabelOverlayListFilter.Default,
    };

    private bool _isApplyingFilter;

    /// <summary>The filter belonging to the output currently on screen.</summary>
    private Model_LabelOverlayListFilter ActiveFilter => _filters[ActiveFormat];

    [ObservableProperty]
    private DateTimeOffset? _filterFromDate;

    [ObservableProperty]
    private DateTimeOffset? _filterToDate;

    [ObservableProperty]
    private Model_Tool_LotMarkingSortOption _selectedSortOption = Model_Tool_LotMarkingSortOption.All[0];

    [ObservableProperty]
    private bool _isSortDescending = true;

    /// <summary>The sort choices, the same list the Heat / Lot Management tool offers.</summary>
    public IReadOnlyList<Model_Tool_LotMarkingSortOption> SortOptions =>
        Model_Tool_LotMarkingSortOption.All;

    /// <summary>The columns that can be shown for this tab, with the chosen ones ticked.</summary>
    public ObservableCollection<Model_Tool_LotMarkingColumnOption> FilterColumns { get; } = [];

    /// <summary>True when this tab's list is narrowed in any way, so the button can say so.</summary>
    public bool HasActiveFilter => ActiveFilter.HasDateRange;

    /// <summary>
    /// Whether each list header shows, following this tab's column choice. The header sits outside the row
    /// template, so it reads them here rather than from a row.
    /// </summary>
    public bool ShowShapeColumn =>
        ActiveFilter.IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Shape);

    public bool ShowColorColumn =>
        ActiveFilter.IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Color);

    public bool ShowHeatColumn =>
        ActiveFilter.IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Heat);

    public bool ShowPartColumn =>
        ActiveFilter.IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Parts);

    public bool ShowReceivedColumn =>
        ActiveFilter.IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.LastLine);

    private void NotifyColumnVisibility()
    {
        OnPropertyChanged(nameof(ShowShapeColumn));
        OnPropertyChanged(nameof(ShowColorColumn));
        OnPropertyChanged(nameof(ShowHeatColumn));
        OnPropertyChanged(nameof(ShowPartColumn));
        OnPropertyChanged(nameof(ShowReceivedColumn));
        OnPropertyChanged(nameof(HasActiveFilter));
        OnPropertyChanged(nameof(FilterSummary));
    }

    /// <summary>A plain reading of the filter, shown on the tab without opening anything.</summary>
    public string FilterSummary
    {
        get
        {
            var filter = ActiveFilter.Filter;
            var parts = new List<string>();

            if (filter.FromDate is not null && filter.ToDate is not null)
            {
                parts.Add($"{filter.FromDate:MMM d} – {filter.ToDate:MMM d}");
            }
            else if (filter.FromDate is not null)
            {
                parts.Add($"since {filter.FromDate:MMM d}");
            }
            else if (filter.ToDate is not null)
            {
                parts.Add($"up to {filter.ToDate:MMM d}");
            }
            else
            {
                parts.Add("all dates");
            }

            parts.Add(Model_Tool_LotMarkingSortOption.FromField(filter.SortField).DisplayName);
            parts.Add(filter.IsDescending ? "high to low" : "low to high");

            return string.Join(" · ", parts);
        }
    }

    // ─── Cut-out tab: one part at a time ────────────────────────────────────

    /// <summary>The parts that carry a marking, which is what the cut-out tab offers to work on.</summary>
    public ObservableCollection<string> Parts { get; } = [];

    [ObservableProperty]
    private string? _selectedPart;

    /// <summary>The lots of the chosen part, which is what a cut-out sheet is printed for.</summary>
    public ObservableCollection<Model_ReceivingLabelOverlayItem> CardSheetLabels { get; } = [];

    /// <summary>
    /// Which cut-out lots are ticked, held by <see cref="Model_ReceivingLabelOverlayItem.SelectionKey"/> so
    /// the ticks survive a filter change and stay separate from the overlay tab's ticks.
    /// </summary>
    private readonly HashSet<string> _cutOutSelection = new(StringComparer.Ordinal);

    /// <summary>
    /// The rows the Print button acts on: the cut-out lots while that tab is showing, otherwise the
    /// overlay tags.
    /// </summary>
    public IReadOnlyList<Model_ReceivingLabelOverlayItem> SelectableItems =>
        IsCardSheet ? CardSheetLabels : Labels;

    /// <summary>What the cut-out lot list says when the chosen part has no lots in range.</summary>
    public string CardSheetListEmptyText =>
        string.IsNullOrWhiteSpace(SelectedPart)
            ? "Choose a part number to see its lots."
            : $"No lots for {SelectedPart} in the dates this tab is set to.";

    /// <summary>True when the cut-out lot list has nothing to show.</summary>
    public bool IsCardSheetListEmpty => CardSheetLabels.Count == 0;

    /// <summary>What the list says when it is empty, so an empty queue is not mistaken for a failure.</summary>
    public string LabelListEmptyText =>
        "No receiving tags are waiting in the label queue. Save a receipt first, then come back.";

    [ObservableProperty]
    private string _printersSummary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrint))]
    [NotifyPropertyChangedFor(nameof(CanClickPrint))]
    private string _selectedPrinter = string.Empty;

    public ObservableCollection<string> Printers { get; } = [];

    // ─── What gets printed ──────────────────────────────────────────────────

    /// <summary>
    /// 0 for the Overlays tab, 1 for Cut-Out Cards, 2 for Printer Settings. Held as an index because that
    /// is what a tab strip binds to.
    /// </summary>
    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// Which output the print controls on the Printer Settings tab are editing: 0 for the overlay, 1 for
    /// the cut-out cards. Kept in step with the tab strip while one of the two output tabs is showing.
    /// </summary>
    [ObservableProperty]
    private int _settingsTargetIndex;

    /// <summary>True when the tag overlay tab is showing.</summary>
    public bool IsOverlayTab => SelectedTabIndex == 0;

    /// <summary>True when the cut-out card tab is showing.</summary>
    public bool IsCardSheetTab => SelectedTabIndex == 1;

    /// <summary>True when the printer settings tab is showing.</summary>
    public bool IsPrinterSettingsTab => SelectedTabIndex == 2;

    /// <summary>Whether the printer-settings controls are editing the cut-out output.</summary>
    public bool IsEditingCardSheetSettings => SettingsTargetIndex == 1;

    /// <summary>True when a cut-out card sheet is what the print controls and the button are about.</summary>
    public bool IsCardSheet => ActiveFormat == Enum_LabelOverlayPrintFormat.CardSheet;

    /// <summary>True when the tag geometry is what decides where the marking lands.</summary>
    public bool IsTagOverlay => !IsCardSheet;

    /// <summary>The footer button's words: one short verb, because the window title already says what prints.</summary>
    public string PrintButtonText => "Print";

    // ─── Printer setup ──────────────────────────────────────────────────────

    /// <summary>0 for landscape, 1 for portrait. Landscape is what the tag artwork is drawn for.</summary>
    [ObservableProperty]
    private int _selectedOrientationIndex;

    /// <summary>The media the printer will use. Chosen from what the driver reports, not invented here.</summary>
    [ObservableProperty]
    private Model_LabelOverlayPaperOption? _selectedPaper;

    /// <summary>
    /// The tray this output feeds from. Held by the app rather than only inside the driver settings,
    /// because several drivers ignore the tray in a saved block and the job then feeds from the
    /// printer's own default.
    /// </summary>
    [ObservableProperty]
    private Model_LabelOverlayPaperSourceOption? _selectedPaperSource;

    [ObservableProperty]
    private string _papersSummary = string.Empty;

    [ObservableProperty]
    private string _paperSourcesSummary = string.Empty;

    /// <summary>
    /// Room left around the cut-out cards, in inches, so a printer that cannot reach the edge of the paper
    /// still produces cards whose part number and lot survive.
    /// </summary>
    [ObservableProperty]
    private double _cardSheetMarginInches =
        Model_LabelOverlayPrintSettings.DefaultCardSheetMarginInches;

    /// <summary>The lower and upper bounds the duplicates box is allowed to take.</summary>
    public int MinimumCopies => Model_LabelOverlayPrintSettings.DefaultCopies;

    /// <inheritdoc cref="MinimumCopies" />
    public int MaximumCopies => Model_LabelOverlayPrintSettings.MaximumCopies;

    /// <summary>
    /// How many times every printed page is repeated, as the print bar shows it: the duplicates each
    /// selected lot gets on a cut-out sheet, or the copies of each tag page.
    /// </summary>
    public int PrintCopies
    {
        get => ActiveProfile.ResolveCopies();
        set
        {
            ActiveProfile.Copies = Math.Clamp(value, MinimumCopies, MaximumCopies);

            OnPropertyChanged();
            OnPropertyChanged(nameof(PrintCopiesSummary));
            OnPropertyChanged(nameof(PrintSetupSummary));

            if (!_isApplyingPrintSettings)
            {
                _ = PersistPrintSettingsAsync();
            }
        }
    }

    /// <summary>What the copies box is counting, so the same box reads correctly on both outputs.</summary>
    public string PrintCopiesSummary =>
        IsCardSheet
            ? "How many full pages each selected lot gets. Every page carries the one lot, eight identical cards."
            : "How many copies of each tag page to print.";

    public ObservableCollection<Model_LabelOverlayPaperOption> Papers { get; } = [];

    /// <summary>The trays the chosen printer offers, so the saved one can be chosen and checked.</summary>
    public ObservableCollection<Model_LabelOverlayPaperSourceOption> PaperSources { get; } = [];

    /// <summary>The orientation choices, in the order <see cref="SelectedOrientationIndex"/> expects.</summary>
    public IReadOnlyList<string> OrientationOptions { get; } =
        ["Landscape (11\" across)", "Portrait (8.5\" across)"];

    /// <summary>A plain reading of the print setup, shown even while the advanced panel is closed.</summary>
    public string PrintSetupSummary => BuildPrintSettings().Describe();

    // ─── Where the marking lands ────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrint))]
    [NotifyPropertyChangedFor(nameof(CanClickPrint))]
    private double _centerXInches = Model_LabelOverlayGeometry.Default.CenterX;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrint))]
    [NotifyPropertyChangedFor(nameof(CanClickPrint))]
    private double _centerYInches = Model_LabelOverlayGeometry.Default.CenterY;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrint))]
    [NotifyPropertyChangedFor(nameof(CanClickPrint))]
    private double _diameterInches = Model_LabelOverlayGeometry.Default.Diameter;

    [ObservableProperty]
    private string _headingText = Model_LabelOverlayGeometry.Default.Heading;

    /// <summary>
    /// A plain reading of the tag geometry, so the numbers can be checked against a printed sheet with a
    /// rule rather than guessed at.
    /// </summary>
    public string GeometrySummary
    {
        get
        {
            var page = BuildPrintSettings().ResolvePageSize();

            return $"{DiameterInches:0.##}\" circle centred at {CenterXInches:0.###}\" from the left and "
                + $"{CenterYInches:0.###}\" from the top of a {page.Width:0.##}\" x {page.Height:0.##}\" page.";
        }
    }

    // ─── Selection ──────────────────────────────────────────────────────────

    /// <summary>How many rows are ticked, and how many of those can actually print a marking.</summary>
    public string SelectionSummary
    {
        get
        {
            var rows = SelectableItems;
            var selected = rows.Count(item => item.IsSelected);
            var printable = rows.Count(item => item.IsSelected && item.HasMarking);

            if (selected == 0)
            {
                return IsCardSheet
                    ? "Choose a part number, then tick the lots to print cards for."
                    : "Nothing selected yet.";
            }

            return printable == selected
                ? $"{selected} {(IsCardSheet ? "lot(s)" : "tag(s)")} selected."
                : $"{selected} selected, {printable} with a marking to print; "
                    + $"{selected - printable} will be skipped.";
        }
    }

    /// <summary>
    /// True when a printer is chosen, something with a marking is selected, and — for tag overlays — the
    /// geometry fits the page the print setup gives it. A card sheet sizes its own cards, so it only needs
    /// something to print.
    /// </summary>
    public bool CanPrint
    {
        get
        {
            if (IsBusy || Printers.Count == 0 || string.IsNullOrWhiteSpace(SelectedPrinter))
            {
                return false;
            }

            if (!SelectableItems.Any(item => item.IsSelected && item.HasMarking))
            {
                return false;
            }

            if (IsCardSheet)
            {
                return true;
            }

            var page = BuildPrintSettings().ResolvePageSize();

            return BuildGeometry().IsInsideTag(page.Width, page.Height);
        }
    }

    partial void OnSelectedPrinterChanged(string value)
    {
        UpdateSelectionCounts();

        // Rebuilding the printer list clears the bound selection for a moment. That is not the operator
        // choosing a printer, so it must not reload the paper list or overwrite the saved setup.
        if (!_isApplyingPrintSettings)
        {
            _ = OnPrinterChangedAsync();
        }
    }

    partial void OnHeadingTextChanged(string value) => OnPropertyChanged(nameof(GeometrySummary));

    partial void OnSelectedTabIndexChanged(int value)
    {
        // While an output tab is showing, the print controls belong to that output. The Printer Settings
        // tab keeps whichever output was last being configured, so the two can be set up independently.
        if (value is 0 or 1)
        {
            SettingsTargetIndex = value;
            ApplyFilterToScreen(ActiveFilter);
        }

        OnPropertyChanged(nameof(IsOverlayTab));
        OnPropertyChanged(nameof(IsCardSheetTab));
        OnPropertyChanged(nameof(IsPrinterSettingsTab));
        OnPropertyChanged(nameof(SelectableItems));
        RefreshPrintSetupText();
        UpdateSelectionCounts();
    }

    partial void OnSettingsTargetIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsEditingCardSheetSettings));

        if (_isApplyingPrintSettings)
        {
            return;
        }

        ApplyPrintSettings(ActiveProfile);
        _ = LoadPapersAsync();
        _ = LoadPaperSourcesAsync();
    }

    partial void OnSelectedOrientationIndexChanged(int value) => OnPrintSetupChanged();

    partial void OnSelectedPaperChanged(Model_LabelOverlayPaperOption? value) =>
        OnPrintSetupChanged();

    partial void OnCardSheetMarginInchesChanged(double value) => OnPrintSetupChanged();

    partial void OnSelectedPaperSourceChanged(Model_LabelOverlayPaperSourceOption? value)
    {
        if (value is not null)
        {
            // Kept alongside the picker so a reload of the tray list does not fall back to an older choice.
            _savedPaperSourceName = value.Name;
            _savedPaperSourceRawKind = value.RawKind;
        }

        OnPrintSetupChanged();
    }

    // ─── Activation ─────────────────────────────────────────────────────────

    /// <summary>Loads the saved setup, the printers, the paper sizes and the label queue, then shows them.</summary>
    public Task ActivateAsync() => ReloadAsync();

    /// <summary>Reads everything again, keeping the saved print setup.</summary>
    [RelayCommand]
    public async Task ReloadAsync()
    {
        await LoadPrintSettingsAsync();
        await LoadPrintersAsync();
        await LoadPapersAsync();
        await LoadPaperSourcesAsync();
        await LoadLabelsAsync();
    }

    private async Task LoadPrintSettingsAsync()
    {
        try
        {
            foreach (var format in _profiles.Keys.ToList())
            {
                var result = await _settingsService.GetPrintProfileAsync(format);

                _profiles[format] =
                    result.IsSuccess && result.Data is not null
                        ? result.Data
                        : new Model_LabelOverlayPrintSettings { Format = format };
            }

            ApplyPrintSettings(ActiveProfile);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(LoadPrintSettingsAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
        }
    }

    private async Task LoadPrintersAsync()
    {
        var result = await _overlayService.GetPrinterNamesAsync();
        if (!result.IsSuccess)
        {
            Printers.Clear();
            PrintersSummary = result.ErrorMessage;
            OnPropertyChanged(nameof(CanPrint));
            OnPropertyChanged(nameof(CanClickPrint));
            return;
        }

        var names = result.Data ?? [];
        var previous = SelectedPrinter;

        // The list is rebuilt with the saved selection held aside, so the operator's printer survives both
        // a relaunch and a reload. The flag stops the clear-then-refill from looking like choices.
        _isApplyingPrintSettings = true;

        try
        {
            Printers.Clear();
            foreach (var name in names)
            {
                Printers.Add(name);
            }

            SelectedPrinter =
                names.FirstOrDefault(name =>
                    string.Equals(name, previous, StringComparison.CurrentCultureIgnoreCase)
                ) ?? names.FirstOrDefault() ?? string.Empty;
        }
        finally
        {
            _isApplyingPrintSettings = false;
        }

        PrintersSummary =
            names.Count == 0 ? "No printers are installed." : $"{names.Count} printer(s).";

        OnPropertyChanged(nameof(CanPrint));
            OnPropertyChanged(nameof(CanClickPrint));
    }

    private async Task LoadPapersAsync()
    {
        var result = await _overlayService.GetPaperSizesAsync(SelectedPrinter);
        var papers = result.IsSuccess ? result.Data ?? [] : [];

        _isApplyingPrintSettings = true;
        try
        {
            Papers.Clear();
            foreach (var paper in papers)
            {
                Papers.Add(paper);
            }

            SelectedPaper = ResolvePaper(papers);

            PapersSummary = Papers.Count > 0
                ? $"{Papers.Count} paper size(s) reported by the printer."
                : string.IsNullOrWhiteSpace(SelectedPrinter)
                    ? "Choose a printer to see the paper sizes it takes."
                    : result.ErrorMessage;
        }
        finally
        {
            _isApplyingPrintSettings = false;
        }

        RefreshPrintSetupText();
    }

    /// <summary>
    /// Reads the trays the chosen printer offers and puts the picker on the saved one, so a tray that was
    /// chosen earlier is what the app prints from rather than whatever the driver's own default is.
    /// </summary>
    private async Task LoadPaperSourcesAsync()
    {
        var result = await _overlayService.GetPaperSourcesAsync(SelectedPrinter);
        var sources = result.IsSuccess ? result.Data ?? [] : [];

        _isApplyingPrintSettings = true;
        try
        {
            PaperSources.Clear();
            foreach (var source in sources)
            {
                PaperSources.Add(source);
            }

            SelectedPaperSource = ResolvePaperSource(sources);

            PaperSourcesSummary = PaperSources.Count > 0
                ? $"{PaperSources.Count} tray(s) reported by the printer."
                : string.IsNullOrWhiteSpace(SelectedPrinter)
                    ? "Choose a printer to see the trays it takes."
                    : result.ErrorMessage;
        }
        finally
        {
            _isApplyingPrintSettings = false;
        }

        RefreshPrintSetupText();
    }

    /// <summary>
    /// The saved tray when the printer still offers it, matched on the driver's own number first and then
    /// on the name, otherwise nothing — so a printer swap feeds from the driver's default rather than from
    /// a tray number that no longer exists.
    /// </summary>
    private Model_LabelOverlayPaperSourceOption? ResolvePaperSource(
        IReadOnlyList<Model_LabelOverlayPaperSourceOption> sources
    )
    {
        if (sources.Count == 0)
        {
            return null;
        }

        return sources.FirstOrDefault(source => source.RawKind == _savedPaperSourceRawKind)
            ?? sources.FirstOrDefault(source =>
                !string.IsNullOrWhiteSpace(_savedPaperSourceName)
                && string.Equals(source.Name, _savedPaperSourceName, StringComparison.OrdinalIgnoreCase)
            );
    }

    /// <summary>
    /// The saved paper when the printer still offers it, otherwise Letter, otherwise whatever the printer
    /// lists first — so a printer swap lands on something printable instead of nothing.
    /// </summary>
    private Model_LabelOverlayPaperOption? ResolvePaper(
        IReadOnlyList<Model_LabelOverlayPaperOption> papers
    )
    {
        if (papers.Count == 0)
        {
            return null;
        }

        var saved =
            papers.FirstOrDefault(paper =>
                string.Equals(paper.Name, _savedPaperName, StringComparison.OrdinalIgnoreCase)
                && Math.Abs(paper.WidthInches - _savedPaperWidth) < 0.02f
                && Math.Abs(paper.HeightInches - _savedPaperHeight) < 0.02f
            )
            ?? papers.FirstOrDefault(paper =>
                string.Equals(paper.Name, _savedPaperName, StringComparison.OrdinalIgnoreCase)
            )
            ?? papers.FirstOrDefault(paper =>
                string.Equals(paper.Name, "Letter", StringComparison.OrdinalIgnoreCase)
            );

        return saved ?? Helper_LabelOverlayPrinter.FindDefaultPaper(papers);
    }

    private async Task LoadLabelsAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _overlayService.GetOverlayCandidatesAsync();

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                Labels = [];
                UpdateSelectionCounts();
                return;
            }

            var items = result.Data ?? [];
            _allLabels = items.ToList();

            await LoadFiltersAsync();
            RebuildLists();

            ShowStatus(
                items.Count == 0 ? LabelListEmptyText : $"{items.Count} tag(s) in the label queue.",
                items.Count == 0 ? InfoBarSeverity.Informational : InfoBarSeverity.Success
            );

            ApplyPendingPreselection();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadLabelsAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
            ShowStatus("The label queue could not be read.", InfoBarSeverity.Error);
            _allLabels = [];
            Labels = [];
            CardSheetLabels.Clear();
            UpdateSelectionCounts();
        }
        finally
        {
            IsBusy = false;

            // CanPrint reads IsBusy, so the tick the preselection made during the load only reaches the
            // Print button if the counts are read again once loading has ended.
            UpdateSelectionCounts();
        }
    }

    /// <summary>
    /// Keeps the running totals and the Print button in step with the checkboxes, which the list binds
    /// to directly.
    /// </summary>
    private void SubscribeToSelection(IEnumerable<Model_ReceivingLabelOverlayItem> items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= OnLabelPropertyChanged;
            item.PropertyChanged += OnLabelPropertyChanged;
        }
    }

    private void OnLabelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Model_ReceivingLabelOverlayItem.IsSelected))
        {
            UpdateSelectionCounts();
        }
    }

    private void UpdateSelectionCounts()
    {
        OnPropertyChanged(nameof(SelectionSummary));
        OnPropertyChanged(nameof(CanPrint));
        OnPropertyChanged(nameof(CanClickPrint));
        OnPropertyChanged(nameof(IsLabelListEmpty));
        PrintCommand.NotifyCanExecuteChanged();
    }

    // ─── Filter handling ────────────────────────────────────────────────────

    partial void OnFilterFromDateChanged(DateTimeOffset? value) => OnFilterChanged();

    partial void OnFilterToDateChanged(DateTimeOffset? value) => OnFilterChanged();

    partial void OnSelectedSortOptionChanged(Model_Tool_LotMarkingSortOption value)
    {
        if (!_isApplyingFilter && value is not null)
        {
            IsSortDescending = value.DefaultDescending;
        }

        OnFilterChanged();
    }

    partial void OnIsSortDescendingChanged(bool value) => OnFilterChanged();

    partial void OnSelectedPartChanged(string? value)
    {
        RebuildCutOutLots();
        _ = PersistCardSheetPartAsync();
    }

    /// <summary>Clears this tab's date range, leaving its sort and columns alone.</summary>
    [RelayCommand]
    private void ClearDateRange()
    {
        FilterFromDate = null;
        FilterToDate = null;
    }

    private void OnFilterChanged()
    {
        if (_isApplyingFilter)
        {
            return;
        }

        CommitFilterToActiveTab();
        RebuildLists();

        NotifyColumnVisibility();

        _ = PersistFilterAsync();
    }

    /// <summary>Reads the screen's filter controls into the active tab's saved filter.</summary>
    private void CommitFilterToActiveTab()
    {
        var columns = FilterColumns.Where(column => column.IsVisible).Select(column => column.Key);

        _filters[ActiveFormat] = new Model_LabelOverlayListFilter(
            new Model_Tool_LotMarkingFilter(
                FilterFromDate?.Date,
                FilterToDate?.Date,
                SelectedSortOption?.Field ?? Enum_LotMarkingSortField.LastReceiptDate,
                IsSortDescending
            ),
            columns.ToList()
        );
    }

    /// <summary>Puts a tab's saved filter back into the screen's controls without saving it again.</summary>
    private void ApplyFilterToScreen(Model_LabelOverlayListFilter filter)
    {
        _isApplyingFilter = true;

        try
        {
            FilterFromDate = filter.Filter.FromDate is null
                ? null
                : new DateTimeOffset(filter.Filter.FromDate.Value);
            FilterToDate = filter.Filter.ToDate is null
                ? null
                : new DateTimeOffset(filter.Filter.ToDate.Value);
            SelectedSortOption = Model_Tool_LotMarkingSortOption.FromField(filter.Filter.SortField);
            IsSortDescending = filter.Filter.IsDescending;

            FilterColumns.Clear();
            foreach (var column in Model_LabelOverlayListFilter.Columns)
            {
                var option = new Model_Tool_LotMarkingColumnOption(
                    column.Key,
                    column.DisplayName,
                    filter.IsColumnVisible(column.Key)
                );

                option.PropertyChanged += OnColumnOptionChanged;
                FilterColumns.Add(option);
            }
        }
        finally
        {
            _isApplyingFilter = false;
        }

        NotifyColumnVisibility();
    }

    private void OnColumnOptionChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        if (e.PropertyName == nameof(Model_Tool_LotMarkingColumnOption.IsVisible))
        {
            OnFilterChanged();
        }
    }

    /// <summary>Rebuilds whichever list the current tab shows from the queue and the tab's filter.</summary>
    private void RebuildLists()
    {
        _isRebuildingLists = true;

        try
        {
            RebuildOverlayLabels();
            RebuildParts();
            RebuildCutOutLots();
        }
        finally
        {
            _isRebuildingLists = false;
        }
    }

    private bool _isRebuildingLists;

    private void RebuildOverlayLabels()
    {
        var filter = _filters[Enum_LabelOverlayPrintFormat.TagOverlay];

        var visible = SortForDisplay(
            _allLabels.Where(item => filter.IncludesDate(item.ReceivedDate)),
            filter
        );

        foreach (var item in visible)
        {
            item.SetVisibleColumns(filter.VisibleColumnKeys);
        }

        Labels = new ObservableCollection<Model_ReceivingLabelOverlayItem>(visible);
        SubscribeToSelection(Labels);
        UpdateSelectionCounts();
    }

    /// <summary>
    /// The parts the cut-out tab offers: those whose tags carry a marking, since a card with no marking
    /// would say less than the blank card already does.
    /// </summary>
    private void RebuildParts()
    {
        var previous = _savedCardSheetPartId ?? SelectedPart;
        _savedCardSheetPartId = null;

        var parts = _allLabels
            .Where(item => item.HasMarking)
            .Select(item => item.PartId)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(part => part, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Parts.Clear();
        foreach (var part in parts)
        {
            Parts.Add(part);
        }

        SelectedPart =
            parts.FirstOrDefault(part =>
                string.Equals(part, previous, StringComparison.OrdinalIgnoreCase)
            ) ?? parts.FirstOrDefault();
    }

    private void RebuildCutOutLots()
    {
        var filter = _filters[Enum_LabelOverlayPrintFormat.CardSheet];

        var lots = string.IsNullOrWhiteSpace(SelectedPart)
            ? []
            : SortForDisplay(
                    _allLabels.Where(item =>
                        item.HasMarking
                        && string.Equals(item.PartId, SelectedPart, StringComparison.OrdinalIgnoreCase)
                        && filter.IncludesDate(item.ReceivedDate)
                    ),
                    filter
                )
                .ToList();

        CardSheetLabels.Clear();
        foreach (var lot in lots)
        {
            var row = lot.CopyForCutOut(_cutOutSelection.Contains(lot.SelectionKey));
            row.SetVisibleColumns(filter.VisibleColumnKeys);
            row.PropertyChanged += OnCutOutRowChanged;
            CardSheetLabels.Add(row);
        }

        OnPropertyChanged(nameof(IsCardSheetListEmpty));
        OnPropertyChanged(nameof(CardSheetListEmptyText));
        UpdateSelectionCounts();
    }

    private void OnCutOutRowChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        if (
            e.PropertyName != nameof(Model_ReceivingLabelOverlayItem.IsSelected)
            || sender is not Model_ReceivingLabelOverlayItem item
        )
        {
            return;
        }

        if (item.IsSelected)
        {
            _cutOutSelection.Add(item.SelectionKey);
        }
        else
        {
            _cutOutSelection.Remove(item.SelectionKey);
        }

        UpdateSelectionCounts();
    }

    /// <summary>
    /// Orders a list the way the filter asks. Fields this list has no column for fall back to the receipt
    /// date, so a saved sort from the other tool still produces a sensible order rather than an error.
    /// </summary>
    private static IEnumerable<Model_ReceivingLabelOverlayItem> SortForDisplay(
        IEnumerable<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayListFilter filter
    )
    {
        var descending = filter.Filter.IsDescending;

        var ordered = filter.Filter.SortField switch
        {
            Enum_LotMarkingSortField.HeatNumber => Order(items, item => item.HeatLotNumber, descending),
            Enum_LotMarkingSortField.PartNumber => Order(items, item => item.PartId, descending),
            Enum_LotMarkingSortField.ShapeAndColor => Order(items, item => item.MarkingText, descending),
            _ => Order(items, item => item.ReceivedDate, descending),
        };

        return ordered
            .ThenBy(item => item.PartId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.HeatLotNumber, StringComparer.OrdinalIgnoreCase);
    }

    private static IOrderedEnumerable<Model_ReceivingLabelOverlayItem> Order<TKey>(
        IEnumerable<Model_ReceivingLabelOverlayItem> items,
        Func<Model_ReceivingLabelOverlayItem, TKey> key,
        bool descending
    ) => descending ? items.OrderByDescending(key) : items.OrderBy(key);

    private async Task PersistFilterAsync()
    {
        try
        {
            var result = await _settingsService.SaveListFilterAsync(ActiveFormat, ActiveFilter);

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(PersistFilterAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
        }
    }

    private async Task PersistCardSheetPartAsync()
    {
        if (_isRebuildingLists)
        {
            return;
        }

        try
        {
            await _settingsService.SaveCardSheetPartIdAsync(SelectedPart ?? string.Empty);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(PersistCardSheetPartAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
        }
    }

    private async Task LoadFiltersAsync()
    {
        var overlay = await _settingsService.GetListFilterAsync(
            Enum_LabelOverlayPrintFormat.TagOverlay
        );
        var cardSheet = await _settingsService.GetListFilterAsync(
            Enum_LabelOverlayPrintFormat.CardSheet
        );

        _savedCardSheetPartId = await _settingsService.GetCardSheetPartIdAsync();

        _filters[Enum_LabelOverlayPrintFormat.TagOverlay] =
            overlay.IsSuccess && overlay.Data is not null
                ? overlay.Data
                : Model_LabelOverlayListFilter.Default;
        _filters[Enum_LabelOverlayPrintFormat.CardSheet] =
            cardSheet.IsSuccess && cardSheet.Data is not null
                ? cardSheet.Data
                : Model_LabelOverlayListFilter.Default;

        ApplyFilterToScreen(ActiveFilter);
    }

    /// <summary>The part the cut-out tab was left on last time, used once when the list is first built.</summary>
    private string? _savedCardSheetPartId;

    // ─── Selection helpers ──────────────────────────────────────────────────

    /// <summary>
    /// The lots the window should open with already ticked, set by the receiving results page so the tags
    /// it just saved are the ones the operator is taken straight to.
    /// </summary>
    private IReadOnlyList<(string PartId, string HeatLotNumber)> _pendingPreselection = [];

    /// <summary>
    /// Asks the window to open with these lots ticked. Set before the window is shown; the request is
    /// applied once the label queue has been read, because the rows do not exist until then.
    /// </summary>
    /// <param name="lots">The part and heat/lot of each tag to tick.</param>
    public void RequestPreselection(
        IEnumerable<(string PartId, string HeatLotNumber)> lots
    )
    {
        _pendingPreselection = lots?.ToList() ?? [];
    }

    /// <summary>
    /// Ticks every queued tag that matches a requested lot, once, and clears the request. A tag that is no
    /// longer in the queue is simply not found, which is why the count of what was ticked is reported
    /// rather than assumed.
    /// </summary>
    private void ApplyPendingPreselection()
    {
        if (_pendingPreselection.Count == 0)
        {
            return;
        }

        var requested = _pendingPreselection;
        _pendingPreselection = [];

        var ticked = 0;

        foreach (var item in _allLabels)
        {
            var matches = requested.Any(lot =>
                string.Equals(lot.PartId, item.PartId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    lot.HeatLotNumber?.Trim() ?? string.Empty,
                    item.HeatLotNumber?.Trim() ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            item.IsSelected = matches;

            if (matches)
            {
                ticked++;
            }
        }

        UpdateSelectionCounts();

        ShowStatus(
            ticked > 0
                ? $"{ticked} label(s) ticked from the receipt you just saved."
                : "The labels from that receipt are not in the print queue yet. Print them first, then "
                    + "reopen this window and tick them.",
            ticked > 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
        );
    }

    /// <summary>Ticks every tag that has a marking, which is the usual case.</summary>
    [RelayCommand]
    private void SelectAllMarked()
    {
        foreach (var item in Labels)
        {
            item.IsSelected = item.HasMarking;
        }

        UpdateSelectionCounts();
    }

    /// <summary>Unticks everything.</summary>
    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var item in Labels)
        {
            item.IsSelected = false;
        }

        UpdateSelectionCounts();
    }

    // ─── Print setup ────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the printer driver's own dialog for tray, media and quality, bound to this output's private
    /// settings. The printer's saved defaults are not touched: the choices are kept with this output and
    /// re-applied to each job, so the cut-out cards and the tags can use different trays on one machine.
    /// </summary>
    [RelayCommand]
    public async Task OpenPrinterSetupAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedPrinter))
        {
            ShowStatus("Choose a printer before opening its setup.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            var chosen = Helper_PrinterDevMode.EditForJob(
                SelectedPrinter,
                PrinterSetupOwnerHandle,
                ActiveProfile.GetDevMode()
            );

            if (chosen is null)
            {
                ShowStatus("The printer setup was closed without changes.", InfoBarSeverity.Informational);
                return;
            }

            ActiveProfile.SetDevMode(chosen);
            await PersistPrintSettingsAsync();
            RefreshPrintSetupText();

            ShowStatus(
                $"Saved for this output only: {Helper_PrinterDevMode.Describe(chosen)}.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(OpenPrinterSetupAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
            ShowStatus("The printer setup could not be opened.", InfoBarSeverity.Error);
        }
    }

    /// <summary>
    /// The window the printer's dialog is owned by. Set by the view, because the ViewModel does not hold
    /// window handles.
    /// </summary>
    public IntPtr PrinterSetupOwnerHandle { get; set; }

    /// <summary>Puts the setup back to what the tag artwork expects: Letter, landscape, a quarter-inch inset.</summary>
    [RelayCommand]
    private void ResetPrintSetup()
    {
        ApplyPrintSettings(Model_LabelOverlayPrintSettings.CreateDefault(), keepPrinter: true);
        _ = ReapplyDefaultPaperAsync();

        ShowStatus("The print setup is back to the tag default.", InfoBarSeverity.Informational);
    }

    /// <summary>Puts the paper picker back on the default media and keeps the result.</summary>
    private async Task ReapplyDefaultPaperAsync()
    {
        await LoadPapersAsync();
        await PersistPrintSettingsAsync();
    }

    private void OnPrintSetupChanged()
    {
        RefreshPrintSetupText();

        if (!_isApplyingPrintSettings)
        {
            _ = PersistPrintSettingsAsync();
        }
    }

    private async Task OnPrinterChangedAsync()
    {
        await LoadPapersAsync();
        await LoadPaperSourcesAsync();
        await PersistPrintSettingsAsync();
    }

    private void RefreshPrintSetupText()
    {
        OnPropertyChanged(nameof(IsCardSheet));
        OnPropertyChanged(nameof(IsTagOverlay));
        OnPropertyChanged(nameof(PrintButtonText));
        OnPropertyChanged(nameof(PrintCopies));
        OnPropertyChanged(nameof(PrintCopiesSummary));
        OnPropertyChanged(nameof(PrintSetupSummary));
        OnPropertyChanged(nameof(GeometrySummary));
        OnPropertyChanged(nameof(CanPrint));
            OnPropertyChanged(nameof(CanClickPrint));
        PrintCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Puts a setup back into the screen without saving it. The printer is left alone when
    /// <paramref name="keepPrinter"/> is set, because the list of installed printers owns that choice.
    /// </summary>
    private void ApplyPrintSettings(Model_LabelOverlayPrintSettings settings, bool keepPrinter = false)
    {
        _isApplyingPrintSettings = true;

        try
        {
            _savedPaperName = settings.PaperName;
            _savedPaperWidth = settings.PaperWidthInches;
            _savedPaperHeight = settings.PaperHeightInches;
            _savedPaperSourceName = settings.PaperSourceName;
            _savedPaperSourceRawKind = settings.PaperSourceRawKind;

            SelectedOrientationIndex = settings.Landscape ? 0 : 1;
            CardSheetMarginInches = settings.CardSheetMarginInches;
            settings.Copies = Math.Clamp(settings.Copies, MinimumCopies, MaximumCopies);

            if (!keepPrinter && !string.IsNullOrWhiteSpace(settings.PrinterName))
            {
                SelectedPrinter = settings.PrinterName;
            }
        }
        finally
        {
            _isApplyingPrintSettings = false;
        }

        RefreshPrintSetupText();
    }

    /// <summary>
    /// Keeps the setup on this PC. A save that fails is reported but never blocks printing: the operator
    /// still gets their labels, they just have to set the printer up again next time.
    /// </summary>
    /// <remarks>
    /// Saves are taken one at a time, because several of them are started without waiting. Letting two run
    /// together lets an older setup land after a newer one, which is how the chosen printer used to be
    /// forgotten: the list rebuild wrote a blank one and it finished last.
    /// </remarks>
    private async Task PersistPrintSettingsAsync()
    {
        // A blank printer while printers are installed is the transient state of the list being rebuilt,
        // never something the operator asked for, so it must not be written over a good setup.
        if (Printers.Count > 0 && string.IsNullOrWhiteSpace(SelectedPrinter))
        {
            return;
        }

        var settings = BuildPrintSettings();

        await _printSettingsSaveGate.WaitAsync();

        try
        {
            var result = await _settingsService.SavePrintProfileAsync(
                settings.Format,
                settings
            );

            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(PersistPrintSettingsAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
        }
        finally
        {
            _printSettingsSaveGate.Release();
        }
    }

    /// <summary>The setup as the printer reads it, built from what the screen currently shows.</summary>
    public Model_LabelOverlayPrintSettings BuildPrintSettings()
    {
        var paper = SelectedPaper;
        var profile = ActiveProfile;

        profile.Format = ActiveFormat;
        profile.PrinterName = SelectedPrinter?.Trim() ?? string.Empty;
        profile.Landscape = SelectedOrientationIndex == 0;
        profile.PaperName = paper?.Name ?? "Letter";
        profile.PaperWidthInches =
            paper?.WidthInches ?? Model_LabelOverlayPrintSettings.DefaultPaperWidthInches;
        profile.PaperHeightInches =
            paper?.HeightInches ?? Model_LabelOverlayPrintSettings.DefaultPaperHeightInches;
        profile.PaperSourceName = SelectedPaperSource?.Name ?? string.Empty;
        profile.PaperSourceRawKind = SelectedPaperSource?.RawKind ?? 0;
        profile.CardSheetMarginInches = (float)Math.Clamp(CardSheetMarginInches, 0d, 1d);
        profile.Copies = Math.Clamp(profile.Copies, MinimumCopies, MaximumCopies);

        return profile;
    }

    // ─── Printing ───────────────────────────────────────────────────────────

    /// <summary>
    /// True from the moment the print is asked for until the driver has taken the last page. Drives the
    /// button's progress state rather than a spinner, because a print is slow enough to look like a hang.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanClickPrint))]
    private bool _isPrinting;

    /// <summary>How far through the run the printer is, from 0 to 100.</summary>
    [ObservableProperty]
    private double _printProgress;

    /// <summary>What the button says while it is working: which page of how many.</summary>
    [ObservableProperty]
    private string _printingStatus = string.Empty;

    /// <summary>
    /// The button stays usable while it is printing so the progress it is showing does not grey out; a
    /// second press is ignored by <see cref="PrintAsync"/> itself.
    /// </summary>
    public bool CanClickPrint => IsPrinting || CanPrint;

    [RelayCommand(CanExecute = nameof(CanPrint))]
    public async Task PrintAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        UpdateSelectionCounts();

        var printSettings = BuildPrintSettings();
        var rows = SelectableItems.ToList();
        var printed = rows.Count(item => item.IsSelected && item.HasMarking);

        IsPrinting = true;
        PrintProgress = 0;
        PrintingStatus = IsCardSheet
            ? $"Preparing {printed} card(s)…"
            : $"Preparing {printed} tag(s)…";

        var progress = new Progress<Model_LabelOverlayPrintProgress>(report =>
        {
            PrintProgress = report.PercentComplete;
            PrintingStatus = IsCardSheet
                ? $"Printing sheet {report.Completed} of {report.Total}…"
                : $"Printing page {report.Completed} of {report.Total}…";
        });

        try
        {
            // The job is shown before it is sent. Printing happens from the preview, where the print
            // dialog opens on this output's own printer, tray, media and copies.
            var result = IsCardSheet
                ? _overlayService.PreviewCardSheet(
                    PrinterSetupOwnerHandle,
                    printSettings.PrinterName,
                    rows,
                    printSettings,
                    progress
                )
                : _overlayService.PreviewOverlay(
                    PrinterSetupOwnerHandle,
                    printSettings.PrinterName,
                    rows,
                    BuildGeometry(),
                    printSettings,
                    progress
                );

            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            if (!result.Data.WasPrinted || result.Data.Choice is null)
            {
                ShowStatus("Nothing was printed.", InfoBarSeverity.Informational);
                return;
            }

            await ApplyPrintDialogChoiceAsync(result.Data.Choice, printSettings);

            PrintProgress = 100;
            PrintingStatus = "Sent.";

            ShowStatus(
                IsCardSheet
                    ? CardSheetStatus(printed)
                    : TagStatus(printed, printSettings),
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(PrintAsync),
                nameof(ViewModel_Receiving_LabelOverlay)
            );
            ShowStatus("The overlay could not be printed.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            IsPrinting = false;
            PrintingStatus = string.Empty;
            UpdateSelectionCounts();
        }
    }

    /// <summary>
    /// Folds what the print dialog came back with into this output's own setup: which printer, that
    /// printer's driver settings, and how many copies to make.
    /// </summary>
    /// <remarks>
    /// The printer's stored defaults are untouched, exactly as for the Printer setup button, so a choice
    /// made here cannot leak into another program or another user's printing.
    /// </remarks>
    private async Task ApplyPrintDialogChoiceAsync(
        Model_LabelOverlayPrintDialogChoice choice,
        Model_LabelOverlayPrintSettings printSettings
    )
    {
        if (string.IsNullOrWhiteSpace(choice.PrinterName))
        {
            return;
        }

        printSettings.PrinterName = choice.PrinterName;
        printSettings.SetDevMode(choice.DevMode);
        printSettings.Copies = Math.Clamp(
            choice.Copies,
            Model_LabelOverlayPrintSettings.DefaultCopies,
            Model_LabelOverlayPrintSettings.MaximumCopies
        );

        // A tray the driver named in its own dialog is what this output now feeds from, so the picker and
        // the saved setup agree with the block that was just used.
        var tray = Helper_PrinterDevMode.ReadPaperSource(choice.DevMode);

        if (tray is not null)
        {
            printSettings.PaperSourceRawKind = tray.Value;

            var match = PaperSources.FirstOrDefault(source => source.RawKind == tray.Value);

            if (match is not null)
            {
                printSettings.PaperSourceName = match.Name;
                _savedPaperSourceName = match.Name;
                _savedPaperSourceRawKind = match.RawKind;
                SelectedPaperSource = match;
            }
        }

        if (!string.Equals(SelectedPrinter, choice.PrinterName, StringComparison.Ordinal))
        {
            SelectedPrinter = choice.PrinterName;
        }

        await PersistPrintSettingsAsync();
        RefreshPrintSetupText();
    }

    /// <summary>
    /// How a finished tag run reads back to the operator: how many tags, how many copies of each, and
    /// where they went.
    /// </summary>
    private string TagStatus(int tagCount, Model_LabelOverlayPrintSettings printSettings)
    {
        var copies = printSettings.ResolveCopies();

        return copies == 1
            ? $"{tagCount} tag(s) sent to {printSettings.PrinterName}."
            : $"{tagCount} tag(s), {copies} copies of each, sent to {printSettings.PrinterName}.";
    }

    /// <summary>
    /// How a finished card-sheet run reads back to the operator: how many lots, one full page per lot,
    /// how many duplicates of each, and where they went — so a stack coming off the printer can be
    /// checked against what was asked for.
    /// </summary>
    private string CardSheetStatus(int cardCount)
    {
        var duplicates = Math.Max(1, PrintCopies);
        var pages = Helper_LabelOverlayPrinter.CardSheetPageCount(cardCount, duplicates);

        return duplicates == 1
            ? $"{cardCount} lot(s) on {pages} page(s), one lot to a page, sent to {SelectedPrinter}."
            : $"{cardCount} lot(s) on {pages} page(s), {duplicates} page(s) of each lot, sent to {SelectedPrinter}.";
    }

    /// <summary>Puts the measured tag geometry back, in case a nudged offset went wrong.</summary>
    [RelayCommand]
    private void ResetGeometry() => ApplyGeometry(Model_LabelOverlayGeometry.Default);

    private void ApplyGeometry(Model_LabelOverlayGeometry geometry)
    {
        // Rounded because a number box shows a double's full precision, and 1.184999943 is not something
        // anybody wants to read off a screen.
        CenterXInches = Math.Round(geometry.CenterX, 3);
        CenterYInches = Math.Round(geometry.CenterY, 3);
        DiameterInches = Math.Round(geometry.Diameter, 3);
        HeadingText = geometry.Heading;

        OnPropertyChanged(nameof(GeometrySummary));
        UpdateSelectionCounts();
    }

    /// <summary>
    /// The geometry as the printer reads it. The values are held as doubles because a number box binds
    /// to doubles, and converted here so the arithmetic stays in one place.
    /// </summary>
    public Model_LabelOverlayGeometry BuildGeometry() =>
        new(
            (float)CenterXInches,
            (float)CenterYInches,
            (float)DiameterInches,
            HeadingText?.Trim() ?? string.Empty
        );
}
