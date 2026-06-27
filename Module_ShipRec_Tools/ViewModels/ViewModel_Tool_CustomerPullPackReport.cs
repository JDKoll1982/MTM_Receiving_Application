using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Customer Pull n' Pack report page.
/// </summary>
public partial class ViewModel_Tool_CustomerPullPackReport : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_Notification _notificationService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_CustomerPullPackDataSourceResolver _dataSourceResolver;
    private readonly IService_CustomerPullPackMockDataCatalog _mockDataCatalog;
    private readonly IService_InforVisual _inforVisual;

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _dateFrom = new(DateTime.Today);

    [ObservableProperty]
    private DateTimeOffset _dateTo = new(DateTime.Today.AddDays(7));

    [ObservableProperty]
    private bool _showShortagesOnly;

    [ObservableProperty]
    private bool _showUnpulledOnly;

    [ObservableProperty]
    private bool _showLateOrdersOnly;

    [ObservableProperty]
    private string _partOrOrderSearchText = string.Empty;

    [ObservableProperty]
    private Enum_CustomerPullPackSortMode _selectedSortMode =
        Enum_CustomerPullPackSortMode.PullDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDemandLines))]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    [NotifyPropertyChangedFor(nameof(IsSelectionVisible))]
    private ObservableCollection<Model_CustomerPullPack_DemandLine> _demandLines = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectionVisible))]
    private Model_CustomerPullPack_DemandLine? _selectedDemandLine;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectionVisible))]
    private ObservableCollection<Model_CustomerPullPack_DemandLine> _selectedDemandLines = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    private bool _hasAttemptedLoad;

    [ObservableProperty]
    private string _activeCustomerId = string.Empty;

    [ObservableProperty]
    private string _activeCustomerName = string.Empty;

    [ObservableProperty]
    private int _shortageLineCount;

    [ObservableProperty]
    private int _lateOrderLineCount;

    [ObservableProperty]
    private int _linkedWaitlistLineCount;

    [ObservableProperty]
    private ObservableCollection<string> _favoriteCustomerIds = [];

    [ObservableProperty]
    private ObservableCollection<Model_CustomerPullPack_CrystalReportGroup> _crystalReportGroups =
    [];

    [ObservableProperty]
    private Model_CustomerPullPack_UserDefaults _currentUserDefaults = new();

    [ObservableProperty]
    private Enum_CustomerPullPackPrintMode _selectedPrintMode =
        Enum_CustomerPullPackPrintMode.CurrentView;

    [ObservableProperty]
    private string _selectedFavoriteCustomerId = string.Empty;

    private string _duplicateWaitlistId = string.Empty;

    public Func<
        ViewModel_Dialog_CustomerPullPackWaitlistEditor,
        Task<bool>
    >? ShowWaitlistEditorAsync { get; set; }

    public Func<
        decimal,
        decimal,
        Task<bool>
    >? ShowOverSelectedQuantityConfirmationAsync { get; set; }

    public Func<Task>? ShowWaitlistQueueAsync { get; set; }

    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    public Func<
        Model_CustomerPullPack_UserDefaults,
        Task<Model_CustomerPullPack_UserDefaults?>
    >? ShowDefaultsAsync { get; set; }

    public Func<Model_CustomerPullPack_PrintContext, Task>? ShowPrintPreviewAsync { get; set; }

    public IReadOnlyList<Enum_CustomerPullPackSortMode> SortModes { get; } =
        Enum.GetValues<Enum_CustomerPullPackSortMode>();

    public IReadOnlyList<Enum_CustomerPullPackPrintMode> PrintModes { get; } =
        Enum.GetValues<Enum_CustomerPullPackPrintMode>();

    public bool HasDemandLines => DemandLines.Count > 0;

    public bool IsEmptyStateVisible =>
        HasAttemptedLoad && IsBusy is false && HasDemandLines is false;

    public bool IsSelectionVisible => SelectedDemandLines.Count > 0;

    public Model_CustomerPullPack_DemandLine? PrimarySelectedDemandLine =>
        SelectedDemandLines.FirstOrDefault();

    public string CurrentCustomerDisplay =>
        string.IsNullOrWhiteSpace(ActiveCustomerId) ? "Select a customer and refresh the report."
        : string.IsNullOrWhiteSpace(ActiveCustomerName) ? ActiveCustomerId
        : $"{ActiveCustomerId} - {ActiveCustomerName}";

    public string EmptyStateTitle =>
        string.IsNullOrWhiteSpace(ActiveCustomerId)
            ? "No customer selected yet."
            : $"No open demand found for {ActiveCustomerId} in the selected range.";

    public string EmptyStateMessage =>
        string.IsNullOrWhiteSpace(ActiveCustomerId)
            ? "Enter a customer ID or customer display string, set the date range, and refresh the report."
            : "The customer selection is valid, but there are no rows to display for the current date range and filters. Choose another customer, widen the date range, or refresh if data was expected.";

    public string LinkedWaitlistSummary =>
        LinkedWaitlistLineCount == 0
            ? "No rows linked to waitlist"
            : $"{LinkedWaitlistLineCount} rows linked to waitlist";

    public string SelectionSummary
    {
        get
        {
            var selectedCount = DemandLines.Count(static line =>
            {
                return line.IsSelected;
            });
            return selectedCount switch
            {
                0 => "No request lines selected",
                1 => "1 request line selected",
                _ => $"{selectedCount} request lines selected",
            };
        }
    }

    public string CrystalReportDateDisplay => DateTime.Today.ToString("M/d/yyyy");

    public string CrystalReportTimeDisplay => DateTime.Now.ToString("h:mm:ss tt");

    public string SelectedParentPartId =>
        string.Join(
            ", ",
            SelectedDemandLines
                .Select(static line =>
                {
                    return line.ParentPartId;
                })
                .Where(static partId =>
                {
                    return string.IsNullOrWhiteSpace(partId) is false;
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

    public string SelectedCustomerOrderId =>
        string.Join(
            ", ",
            SelectedDemandLines
                .Select(static line =>
                {
                    return line.CustomerOrderId;
                })
                .Where(static customerOrderId =>
                {
                    return string.IsNullOrWhiteSpace(customerOrderId) is false;
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

    public string SelectedCustomerName => PrimarySelectedDemandLine?.CustomerName ?? string.Empty;

    public string SelectedPullDateDisplay =>
        PrimarySelectedDemandLine is null
            ? string.Empty
            : PrimarySelectedDemandLine.PullDate.ToString("MM/dd/yyyy");

    public string SelectedFgLocationId => PrimarySelectedDemandLine?.FgLocationId ?? string.Empty;

    public string SelectedSubPartAvailabilitySummary =>
        PrimarySelectedDemandLine?.SubPartAvailabilitySummary ?? string.Empty;

    public string SelectedRequesterNote => PrimarySelectedDemandLine?.RequesterNote ?? string.Empty;

    public IReadOnlyList<Model_CustomerPullPack_LocationOption> SelectedDemandLineLocationOptions =>
        PrimarySelectedDemandLine?.LocationOptions ?? [];

    public bool SelectedDemandLineHasSelectableLocations =>
        SelectedDemandLineLocationOptions.Count > 0;

    public string SelectedDemandLineLocationSummary =>
        PrimarySelectedDemandLine is null
            ? string.Empty
            : string.Join(
                ", ",
                SelectedDemandLineLocationOptions
                    .Where(static option =>
                    {
                        return option.Selected;
                    })
                    .Select(static option =>
                    {
                        return option.LocationId;
                    })
            );

    public ViewModel_Tool_CustomerPullPackReport(
        IMediator mediator,
        IService_CustomerPullPackDataSourceResolver dataSourceResolver,
        IService_CustomerPullPackMockDataCatalog mockDataCatalog,
        IService_InforVisual inforVisual,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(dataSourceResolver);
        ArgumentNullException.ThrowIfNull(mockDataCatalog);
        ArgumentNullException.ThrowIfNull(inforVisual);
        ArgumentNullException.ThrowIfNull(sessionManager);
        _mediator = mediator;
        _dataSourceResolver = dataSourceResolver;
        _mockDataCatalog = mockDataCatalog;
        _inforVisual = inforVisual;
        _notificationService = notificationService;
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Shows the shared guidance message for the Customer Pull n' Pack tool when it becomes active.
    /// </summary>
    public void ActivateView()
    {
        ShowStatus(
            "Choose a customer, adjust filters if needed, and refresh the report.",
            InfoBarSeverity.Informational
        );
    }

    public async Task LoadUserDefaultsAsync()
    {
        var result = await _mediator.Send(new Query_CustomerPullPackDefaults(CurrentUserId));
        if (!result.IsSuccess || result.Data is null)
        {
            ApplyMockCustomerDiscovery(CurrentUserDefaults);
            return;
        }

        ApplyUserDefaults(result.Data);
    }

    public async Task ResolveCustomerSearchTextOnBlurAsync(string textAtFocusGain)
    {
        var originalText = NormalizeCustomerSearchInput(textAtFocusGain);
        var currentText = NormalizeCustomerSearchInput(CustomerSearchText);

        if (string.IsNullOrWhiteSpace(currentText))
        {
            return;
        }

        if (string.Equals(originalText, currentText, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var lookupTerm = BuildCustomerLookupTerm(currentText);
        if (string.IsNullOrWhiteSpace(lookupTerm))
        {
            return;
        }

        IReadOnlyList<Model_FuzzySearchResult> candidates;
        if (_dataSourceResolver.IsMockMode)
        {
            candidates = BuildMockCustomerCandidates(lookupTerm);
        }
        else
        {
            var fuzzyResult = await _inforVisual.FuzzySearchCustomersAsync(lookupTerm);
            if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
            {
                ShowStatus(fuzzyResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            candidates = fuzzyResult.Data;
        }

        if (candidates.Count == 0)
        {
            ShowStatus($"No customers found matching '{currentText}'.", InfoBarSeverity.Warning);
            return;
        }

        var confirmedCustomer = await ConfirmCustomerCandidateAsync(candidates);
        if (confirmedCustomer is null)
        {
            ShowStatus("Customer search cancelled.", InfoBarSeverity.Informational);
            return;
        }

        CustomerSearchText = confirmedCustomer.Label;
    }

    [RelayCommand]
    private async Task RefreshReportAsync()
    {
        var customerId = ParseCustomerId(CustomerSearchText);
        var customerName = ParseCustomerName(CustomerSearchText);
        var hadActiveSelections = HasActiveSelections();

        if (string.IsNullOrWhiteSpace(customerId))
        {
            ShowStatus(
                "Customer ID is required before loading the report.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var startDate = DateFrom.Date;
        var endDate = DateTo.Date;

        if (startDate > endDate)
        {
            ShowStatus(
                "The start date cannot be later than the end date.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus(
                $"Loading Customer Pull n' Pack demand for {customerId}...",
                InfoBarSeverity.Informational
            );

            var filter = new Model_CustomerPullPack_DemandFilter
            {
                CustomerId = customerId,
                CustomerName = customerName,
                DateFrom = startDate,
                DateTo = endDate,
                ShortagesOnly = ShowShortagesOnly,
                UnpulledOnly = ShowUnpulledOnly,
                LateOrdersOnly = ShowLateOrdersOnly,
                SortMode = SelectedSortMode,
                SearchText = PartOrOrderSearchText.Trim(),
            };

            var result = await _mediator.Send(new Query_CustomerPullPackReport(filter));
            HasAttemptedLoad = true;

            if (!result.IsSuccess || result.Data is null)
            {
                ReplaceDemandLines(Array.Empty<Model_CustomerPullPack_DemandLine>());
                ClearDuplicateNotice();
                ActiveCustomerId = customerId;
                ActiveCustomerName = customerName;
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ReplaceDemandLines(result.Data);
            ClearDuplicateNotice();
            ActiveCustomerId = customerId;
            ActiveCustomerName = result.Data.FirstOrDefault()?.CustomerName ?? customerName;

            if (DemandLines.Count == 0)
            {
                ShowStatus(
                    hadActiveSelections
                        ? $"No open demand found for {ActiveCustomerId} in the selected range. Previous line and location selections were cleared."
                        : $"No open demand found for {ActiveCustomerId} in the selected range.",
                    hadActiveSelections ? InfoBarSeverity.Warning : InfoBarSeverity.Informational
                );
                return;
            }

            ShowStatus(
                hadActiveSelections
                    ? $"Loaded {DemandLines.Count} demand lines for {ActiveCustomerId}. Previous line and location selections were cleared."
                    : $"Loaded {DemandLines.Count} demand lines for {ActiveCustomerId}.",
                hadActiveSelections ? InfoBarSeverity.Warning : InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RefreshReportAsync),
                nameof(ViewModel_Tool_CustomerPullPackReport)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearReportFilters()
    {
        PartOrOrderSearchText = string.Empty;
        ShowShortagesOnly = false;
        ShowUnpulledOnly = false;
        ShowLateOrdersOnly = false;
        SelectedSortMode = Enum_CustomerPullPackSortMode.PullDate;

        if (HasAttemptedLoad && string.IsNullOrWhiteSpace(CustomerSearchText) is false)
        {
            ShowStatus(
                "Report filters reset. Refresh to load the default view again.",
                InfoBarSeverity.Informational
            );
            return;
        }

        ShowStatus("Report filters reset.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private void ResetCustomerSelection()
    {
        CustomerSearchText = string.Empty;
        ActiveCustomerId = string.Empty;
        ActiveCustomerName = string.Empty;
        HasAttemptedLoad = false;
        ReplaceDemandLines(Array.Empty<Model_CustomerPullPack_DemandLine>());
        ClearDuplicateNotice();
        ShowStatus("Customer selection cleared.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task CreateOrUpdateWaitlistAsync()
    {
        var selectedLines = DemandLines
            .Where(static line =>
            {
                return line.IsSelected;
            })
            .ToList();
        if (selectedLines.Count == 0)
        {
            ShowStatus(
                "Select at least one compatible report row before opening the waitlist editor.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var existingLinkedSelections = selectedLines
            .Where(line =>
            {
                return string.IsNullOrWhiteSpace(line.LinkedWaitlistId) is false;
            })
            .ToList();
        if (existingLinkedSelections.Count > 1)
        {
            ShowStatus(
                "Select a single linked request line when reopening or updating an existing waitlist item.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (await ConfirmOverSelectedQuantityAsync(selectedLines[0]) is false)
        {
            ShowStatus(
                "Waitlist update canceled so you can review the selected quantities.",
                InfoBarSeverity.Informational
            );
            return;
        }

        await OpenWaitlistEditorForLinesAsync(selectedLines);
    }

    [RelayCommand]
    private async Task OpenWaitlistQueueAsync()
    {
        if (ShowWaitlistQueueAsync is null)
        {
            ShowStatus(
                "The waitlist queue is not available in the current report host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        await ShowWaitlistQueueAsync();
    }

    [RelayCommand]
    private async Task OpenDefaultsAsync()
    {
        if (ShowDefaultsAsync is null)
        {
            ShowStatus(
                "The defaults editor is not available in the current report host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var updatedDefaults = await ShowDefaultsAsync(BuildCurrentDefaults());
        if (updatedDefaults is null)
        {
            return;
        }

        ApplyUserDefaults(updatedDefaults);
        await SaveUserDefaultsAsync(updatedDefaults);
    }

    [RelayCommand]
    private async Task SaveCurrentDefaultsAsync()
    {
        await SaveUserDefaultsAsync(BuildCurrentDefaults());
    }

    [RelayCommand]
    private async Task PrintSelectedModeAsync()
    {
        if (ShowPrintPreviewAsync is null)
        {
            ShowStatus(
                "Print preview is not available in the current report host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (DemandLines.Count == 0)
        {
            ShowStatus(
                "Load report demand before opening a print preview.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var printResult = await _mediator.Send(
            new Query_CustomerPullPackPrintContext(
                SelectedPrintMode,
                ActiveCustomerId,
                ActiveCustomerName,
                BuildFiltersSummary(),
                DemandLines.ToList(),
                [],
                string.Empty
            )
        );

        if (!printResult.IsSuccess || printResult.Data is null)
        {
            ShowStatus(printResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        await ShowPrintPreviewAsync(printResult.Data);
    }

    [RelayCommand]
    private async Task ReopenDuplicateWaitlistAsync()
    {
        if (string.IsNullOrWhiteSpace(_duplicateWaitlistId))
        {
            return;
        }

        var linkedWaitlistResult = await _mediator.Send(
            new Query_CustomerPullPackLinkedWaitlist(_duplicateWaitlistId)
        );
        if (!linkedWaitlistResult.IsSuccess || linkedWaitlistResult.Data is null)
        {
            ShowStatus(linkedWaitlistResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        var matchingLines = DemandLines
            .Where(line =>
            {
                return string.Equals(
                        line.SourceLineKey,
                        linkedWaitlistResult.Data.SourceLineKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                    || (
                        string.Equals(
                            line.CustomerOrderId,
                            linkedWaitlistResult.Data.CustomerOrderId,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && string.Equals(
                            line.ParentPartId,
                            linkedWaitlistResult.Data.ParentPartId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
            })
            .ToList();
        if (matchingLines.Count == 0)
        {
            ShowStatus(
                "The existing waitlist item was found, but its report line is not visible in the current report filters.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var matchingLine = matchingLines[0];
        foreach (var line in DemandLines)
        {
            line.IsSelected = matchingLines.Any(match =>
            {
                return string.Equals(
                    match.SourceLineKey,
                    line.SourceLineKey,
                    StringComparison.OrdinalIgnoreCase
                );
            });
        }

        RefreshDemandLinesSnapshot();
        ClearDuplicateNotice();
        await OpenWaitlistEditorForLinesAsync(matchingLines);
    }

    [RelayCommand]
    private void SelectDemandLine(Model_CustomerPullPack_DemandLine? demandLine)
    {
        if (demandLine is null)
        {
            return;
        }

        SelectedDemandLine = demandLine;
    }

    private void ReplaceDemandLines(IEnumerable<Model_CustomerPullPack_DemandLine> lines)
    {
        var orderedLines = lines.ToList();
        DemandLines = new ObservableCollection<Model_CustomerPullPack_DemandLine>(orderedLines);
        SelectedDemandLines = [];
        SelectedDemandLine = null;
        ShortageLineCount = orderedLines.Count(static line =>
        {
            return line.ShortageFlag;
        });
        LateOrderLineCount = orderedLines.Count(static line =>
        {
            return line.LateOrderFlag;
        });
        LinkedWaitlistLineCount = orderedLines.Count(static line =>
        {
            return line.HasLinkedWaitlist;
        });
        OnPropertyChanged(nameof(CurrentCustomerDisplay));
        OnPropertyChanged(nameof(LinkedWaitlistSummary));
        OnPropertyChanged(nameof(SelectedParentPartId));
        OnPropertyChanged(nameof(SelectedCustomerOrderId));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedPullDateDisplay));
        OnPropertyChanged(nameof(SelectedFgLocationId));
        OnPropertyChanged(nameof(SelectedSubPartAvailabilitySummary));
        OnPropertyChanged(nameof(SelectedRequesterNote));
        OnPropertyChanged(nameof(SelectedDemandLineLocationOptions));
        OnPropertyChanged(nameof(SelectedDemandLineHasSelectableLocations));
        OnPropertyChanged(nameof(SelectedDemandLineLocationSummary));
        OnPropertyChanged(nameof(SelectionSummary));
        RefreshCrystalReportGroups();
    }

    partial void OnSelectedDemandLineChanged(Model_CustomerPullPack_DemandLine? value)
    {
        OnPropertyChanged(nameof(SelectedParentPartId));
        OnPropertyChanged(nameof(SelectedCustomerOrderId));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedPullDateDisplay));
        OnPropertyChanged(nameof(SelectedFgLocationId));
        OnPropertyChanged(nameof(SelectedSubPartAvailabilitySummary));
        OnPropertyChanged(nameof(SelectedRequesterNote));
        OnPropertyChanged(nameof(SelectedDemandLineLocationOptions));
        OnPropertyChanged(nameof(SelectedDemandLineHasSelectableLocations));
        OnPropertyChanged(nameof(SelectedDemandLineLocationSummary));
    }

    partial void OnSelectedDemandLinesChanged(
        ObservableCollection<Model_CustomerPullPack_DemandLine> value
    )
    {
        _ = value;
        OnPropertyChanged(nameof(PrimarySelectedDemandLine));
        OnPropertyChanged(nameof(SelectedParentPartId));
        OnPropertyChanged(nameof(SelectedCustomerOrderId));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedPullDateDisplay));
        OnPropertyChanged(nameof(SelectedFgLocationId));
        OnPropertyChanged(nameof(SelectedSubPartAvailabilitySummary));
        OnPropertyChanged(nameof(SelectedRequesterNote));
        OnPropertyChanged(nameof(SelectedDemandLineLocationOptions));
        OnPropertyChanged(nameof(SelectedDemandLineHasSelectableLocations));
        OnPropertyChanged(nameof(SelectedDemandLineLocationSummary));
        OnPropertyChanged(nameof(SelectionSummary));
    }

    partial void OnSelectedFavoriteCustomerIdChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        CustomerSearchText = value.Trim();
    }

    partial void OnCustomerSearchTextChanged(string value)
    {
        if (DemandLines.Count == 0)
        {
            return;
        }

        var candidateCustomerId = ParseCustomerId(value);
        if (string.IsNullOrWhiteSpace(candidateCustomerId))
        {
            ClearCurrentSelections();
            return;
        }

        if (
            string.Equals(candidateCustomerId, ActiveCustomerId, StringComparison.OrdinalIgnoreCase)
            is false
        )
        {
            ClearCurrentSelections();
        }
    }

    public void ApplyCrystalRequestLineSelection(IReadOnlyCollection<string> selectedSourceLineKeys)
    {
        var selectedSourceLineSet = selectedSourceLineKeys
            .Where(static key =>
            {
                return string.IsNullOrWhiteSpace(key) is false;
            })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var focusedLine = DemandLines.FirstOrDefault(line =>
        {
            return selectedSourceLineSet.Contains(line.SourceLineKey);
        });
        var selectedCustomerId = focusedLine?.CustomerId ?? string.Empty;

        foreach (var line in DemandLines)
        {
            var shouldSelectLine =
                selectedSourceLineSet.Contains(line.SourceLineKey)
                && string.IsNullOrWhiteSpace(selectedCustomerId) is false
                && string.Equals(
                    line.CustomerId,
                    selectedCustomerId,
                    StringComparison.OrdinalIgnoreCase
                );

            line.IsSelected = shouldSelectLine;

            if (!shouldSelectLine)
            {
                foreach (var locationOption in line.LocationOptions)
                {
                    locationOption.Selected = false;
                }
            }
        }

        if (focusedLine is not null)
        {
            SelectedDemandLine = focusedLine;
        }
        else
        {
            SelectedDemandLine = null;
        }

        RefreshDemandLinesSnapshot();
    }

    public void ApplyCrystalLocationSelection(
        string groupKey,
        IReadOnlyCollection<string> selectedLocationIds
    )
    {
        var hasSelectedLineForGroup = DemandLines.Any(line =>
        {
            return line.IsSelected
                && string.Equals(line.ParentPartId, groupKey, StringComparison.OrdinalIgnoreCase);
        });

        if (hasSelectedLineForGroup is false)
        {
            foreach (
                var line in DemandLines.Where(line =>
                {
                    return string.Equals(
                        line.ParentPartId,
                        groupKey,
                        StringComparison.OrdinalIgnoreCase
                    );
                })
            )
            {
                foreach (var locationOption in line.LocationOptions)
                {
                    locationOption.Selected = false;
                }
            }

            RefreshDemandLinesSnapshot();
            return;
        }

        var selectedSet = selectedLocationIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (
            var line in DemandLines.Where(line =>
            {
                return string.Equals(
                    line.ParentPartId,
                    groupKey,
                    StringComparison.OrdinalIgnoreCase
                );
            })
        )
        {
            foreach (var locationOption in line.LocationOptions)
            {
                locationOption.Selected =
                    line.IsSelected && selectedSet.Contains(locationOption.LocationId);
            }
        }

        var focusedLine = DemandLines.FirstOrDefault(line =>
        {
            return line.IsSelected
                && string.Equals(line.ParentPartId, groupKey, StringComparison.OrdinalIgnoreCase);
        });
        if (focusedLine is not null)
        {
            SelectedDemandLine = focusedLine;
        }

        RefreshDemandLinesSnapshot();
    }

    private async Task OpenWaitlistEditorForLinesAsync(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines
    )
    {
        if (ShowWaitlistEditorAsync is null)
        {
            ShowStatus(
                "The waitlist editor is not available in the current report host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var selectedLine = selectedLines[0];
        var existingEntry =
            selectedLines.Count == 1 ? await LoadExistingEntryAsync(selectedLine) : null;
        ApplySelectionContext(selectedLines, existingEntry);

        var editorLines = selectedLines.Select(CloneDemandLineForDialog).ToList();

        var editorViewModel = new ViewModel_Dialog_CustomerPullPackWaitlistEditor(
            editorLines,
            existingEntry,
            _errorHandler,
            _logger,
            _notificationService
        );

        var shouldSave = await ShowWaitlistEditorAsync(editorViewModel);
        if (!shouldSave)
        {
            return;
        }

        if (!editorViewModel.ValidateInputs(out var validationMessage))
        {
            ShowStatus(validationMessage, InfoBarSeverity.Warning);
            return;
        }

        var currentUserId =
            _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        var currentUserDisplayName =
            _sessionManager.CurrentSession?.User?.DisplayName ?? currentUserId;
        var batchEntries = editorViewModel.BuildBatchEntries(currentUserId, currentUserDisplayName);

        var saveResult = await _mediator.Send(
            new Command_CustomerPullPackBatchUpsert(batchEntries)
        );
        if (!saveResult.IsSuccess || saveResult.Data is null)
        {
            var duplicateWaitlistId = saveResult.ReturnValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(duplicateWaitlistId) is false)
            {
                _duplicateWaitlistId = duplicateWaitlistId;
                var duplicateNoticeMessage = string.IsNullOrWhiteSpace(saveResult.ErrorMessage)
                    ? "An open waitlist item already exists for the selected report row. Open the existing item instead of creating a duplicate."
                    : saveResult.ErrorMessage;

                ShowStatusWithAction(
                    duplicateNoticeMessage,
                    InfoBarSeverity.Warning,
                    "Open Existing Item",
                    async () =>
                    {
                        await ReopenDuplicateWaitlistCommand.ExecuteAsync(null);
                    }
                );
                return;
            }

            ShowStatus(saveResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        ApplySavedEntries(saveResult.Data);
        ClearDuplicateNotice();
        ShowStatus("Saved Customer Pull n' Pack waitlist item.", InfoBarSeverity.Success);
    }

    private async Task<bool> ConfirmOverSelectedQuantityAsync(
        Model_CustomerPullPack_DemandLine selectedLine
    )
    {
        if (ShowOverSelectedQuantityConfirmationAsync is null)
        {
            return true;
        }

        var matchingGroup = CrystalReportGroups.FirstOrDefault(group =>
        {
            return string.Equals(
                group.GroupKey,
                selectedLine.ParentPartId,
                StringComparison.OrdinalIgnoreCase
            );
        });

        if (matchingGroup is null || matchingGroup.QuantityToPack <= 0)
        {
            return true;
        }

        var overageThreshold = matchingGroup.QuantityToPack * 1.2m;
        if (matchingGroup.QuantitySelected <= overageThreshold)
        {
            return true;
        }

        return await ShowOverSelectedQuantityConfirmationAsync(
            matchingGroup.QuantitySelected,
            matchingGroup.QuantityToPack
        );
    }

    private async Task<Model_CustomerPullPack_WaitlistEntry?> LoadExistingEntryAsync(
        Model_CustomerPullPack_DemandLine selectedLine
    )
    {
        if (string.IsNullOrWhiteSpace(selectedLine.LinkedWaitlistId))
        {
            return null;
        }

        var linkedWaitlistResult = await _mediator.Send(
            new Query_CustomerPullPackLinkedWaitlist(selectedLine.LinkedWaitlistId)
        );

        return linkedWaitlistResult.IsSuccess ? linkedWaitlistResult.Data : null;
    }

    private void ApplySelectionContext(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines,
        Model_CustomerPullPack_WaitlistEntry? existingEntry
    )
    {
        if (existingEntry is null)
        {
            RefreshDemandLinesSnapshot();
            return;
        }

        foreach (var selectedLine in selectedLines)
        {
            selectedLine.RequesterNote = existingEntry.RequesterContextNote;
            selectedLine.HasLinkedWaitlist = true;
            selectedLine.LinkedWaitlistId = existingEntry.WaitlistId;
            selectedLine.WaitlistStateDisplay = string.IsNullOrWhiteSpace(existingEntry.WaitlistId)
                ? existingEntry.CurrentStatus.ToString()
                : $"{existingEntry.CurrentStatus} ({existingEntry.WaitlistId})";

            foreach (var locationOption in selectedLine.LocationOptions)
            {
                locationOption.Selected = existingEntry.SelectedLocations.Contains(
                    locationOption.LocationId,
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        RefreshDemandLinesSnapshot();
    }

    private void ApplySavedEntries(IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> savedEntries)
    {
        foreach (var savedEntry in savedEntries)
        {
            var matchingLines = DemandLines
                .Where(line =>
                {
                    return string.Equals(
                            line.SourceLineKey,
                            savedEntry.SourceLineKey,
                            StringComparison.OrdinalIgnoreCase
                        )
                        || (
                            string.Equals(
                                line.CustomerOrderId,
                                savedEntry.CustomerOrderId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && string.Equals(
                                line.ParentPartId,
                                savedEntry.ParentPartId,
                                StringComparison.OrdinalIgnoreCase
                            )
                        );
                })
                .ToList();
            if (matchingLines.Count == 0)
            {
                continue;
            }

            foreach (var matchingLine in matchingLines)
            {
                matchingLine.IsSelected = false;
                matchingLine.HasLinkedWaitlist = true;
                matchingLine.LinkedWaitlistId = savedEntry.WaitlistId;
                matchingLine.RequesterNote = savedEntry.RequesterContextNote;
                matchingLine.WaitlistStateDisplay = string.IsNullOrWhiteSpace(savedEntry.WaitlistId)
                    ? savedEntry.CurrentStatus.ToString()
                    : $"{savedEntry.CurrentStatus} ({savedEntry.WaitlistId})";

                foreach (var locationOption in matchingLine.LocationOptions)
                {
                    locationOption.Selected = savedEntry.SelectedLocations.Any(locationId =>
                    {
                        return string.Equals(
                            locationId,
                            locationOption.LocationId,
                            StringComparison.OrdinalIgnoreCase
                        );
                    });
                }
            }
        }

        RefreshDemandLinesSnapshot();
    }

    private void RefreshDemandLinesSnapshot()
    {
        SelectedDemandLines = new ObservableCollection<Model_CustomerPullPack_DemandLine>(
            DemandLines.Where(static line =>
            {
                return line.IsSelected;
            })
        );
        SelectedDemandLine = SelectedDemandLines.FirstOrDefault();
        LinkedWaitlistLineCount = DemandLines.Count(static line =>
        {
            return line.HasLinkedWaitlist;
        });
        OnPropertyChanged(nameof(LinkedWaitlistSummary));
        OnPropertyChanged(nameof(DemandLines));
        RefreshCrystalReportGroups();
    }

    // Implements Workflow 1.1 and Workflow 2.1 by projecting live demand and location state
    // into the crystal-style report surface used for report review and multi-row selection by parent part.
    private void RefreshCrystalReportGroups()
    {
        var groupedDemandLines = DemandLines
            .GroupBy(
                static line =>
                {
                    return line.ParentPartId;
                },
                StringComparer.OrdinalIgnoreCase
            )
            .Select(group =>
            {
                var groupLines = group
                    .OrderBy(static line =>
                    {
                        return line.PullDate;
                    })
                    .ThenBy(
                        static line =>
                        {
                            return line.CustomerOrderId;
                        },
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToList();
                var firstLine = groupLines[0];

                var subPartLocations = groupLines
                    .SelectMany(static line =>
                    {
                        return line.LocationOptions;
                    })
                    .GroupBy(
                        static option =>
                        {
                            return option.LocationId;
                        },
                        StringComparer.OrdinalIgnoreCase
                    )
                    .Select(locationGroup =>
                    {
                        var firstOption = locationGroup.First();
                        return new Model_CustomerPullPack_CrystalSubPartLocation
                        {
                            PartId = string.IsNullOrWhiteSpace(firstOption.ParentPartId)
                                ? firstLine.ParentPartId
                                : firstOption.ParentPartId,
                            LocationId = firstOption.LocationId,
                            PartLocationId = firstOption.LocationId,
                            OnHandQuantity = locationGroup.Max(static option =>
                            {
                                return option.OnHandQuantity;
                            }),
                            IsSelected = locationGroup.Any(static option =>
                            {
                                return option.Selected;
                            }),
                        };
                    })
                    .ToList();

                var requestLines = groupLines.ConvertAll(line =>
                {
                    return new Model_CustomerPullPack_CrystalRequestLine
                    {
                        SourceLineKey = line.SourceLineKey,
                        CustomerId = line.CustomerId,
                        CustomerOrderId = line.CustomerOrderId,
                        ParentPartId = line.ParentPartId,
                        LocationId = string.IsNullOrWhiteSpace(line.SourceLocationId)
                            ? "undefined"
                            : line.SourceLocationId,
                        CustomerLabel = string.IsNullOrWhiteSpace(line.CustomerName)
                            ? line.CustomerId
                            : $"{line.CustomerId} - {line.CustomerName}",
                        ShipQuantity = line.ShipQuantity,
                        PullDateDisplay = line.PullDateDisplay,
                        OldestAddedDisplay = line.OldestAddedDisplay,
                        QtySatisfied = line.QtySatisfied,
                        FulfillmentStatusText = line.FulfillmentStatusDisplay,
                        FulfillmentDetailText = BuildFulfillmentDetailText(line),
                        IsSelected = line.IsSelected,
                        StatusNoteText = GetStatusNoteText(line),
                    };
                });

                return new Model_CustomerPullPack_CrystalReportGroup
                {
                    GroupKey = group.Key,
                    CustomerOrderId = firstLine.CustomerOrderId,
                    PrimaryPartId = group.Key,
                    QuantityToPack = groupLines.Sum(static line =>
                    {
                        return line.ShipQuantity;
                    }),
                    QuantitySelected = subPartLocations
                        .Where(static location =>
                        {
                            return location.IsSelected;
                        })
                        .Sum(static location =>
                        {
                            return location.OnHandQuantity;
                        }),
                    FgLocationId = firstLine.FgLocationId,
                    FgOnHandQuantity = firstLine.FgOnHandQuantity,
                    ShortageFlag = groupLines.Any(static line =>
                    {
                        return line.ShortageFlag;
                    }),
                    LateOrderFlag = groupLines.Any(static line =>
                    {
                        return line.LateOrderFlag;
                    }),
                    ServiceNote =
                        groupLines
                            .Select(static line =>
                            {
                                return line.RequesterNote;
                            })
                            .FirstOrDefault(static note =>
                            {
                                return string.IsNullOrWhiteSpace(note) is false;
                            })
                        ?? string.Empty,
                    SubPartLocations = subPartLocations,
                    RequestLines = requestLines,
                };
            })
            .ToList();

        CrystalReportGroups = new ObservableCollection<Model_CustomerPullPack_CrystalReportGroup>(
            groupedDemandLines
        );
    }

    private static string GetStatusNoteText(Model_CustomerPullPack_DemandLine line)
    {
        if (string.IsNullOrWhiteSpace(line.FulfillmentStatusDisplay) is false)
        {
            return line.FulfillmentStatusDisplay;
        }

        if (line.ShortageFlag)
        {
            return "Shortage";
        }

        if (line.LateOrderFlag)
        {
            return "Late Order";
        }

        if (line.RecheckIndicator || line.HasLinkedWaitlist)
        {
            return "Waitlist";
        }

        return "Normal";
    }

    private static string BuildFulfillmentDetailText(Model_CustomerPullPack_DemandLine line)
    {
        if (string.IsNullOrWhiteSpace(line.FulfillmentStatusDisplay))
        {
            return string.Empty;
        }

        var oldestAdded = string.IsNullOrWhiteSpace(line.OldestAddedDisplay)
            ? "n/a"
            : line.OldestAddedDisplay;
        return $"Qty Satisfied: {line.QtySatisfiedDisplay} | Oldest Added: {oldestAdded}";
    }

    private bool HasActiveSelections()
    {
        return DemandLines.Any(line =>
        {
            return line.IsSelected
                || line.LocationOptions.Any(static option =>
                {
                    return option.Selected;
                });
        });
    }

    private void ClearCurrentSelections()
    {
        if (HasActiveSelections() is false)
        {
            return;
        }

        foreach (var line in DemandLines)
        {
            line.IsSelected = false;
            foreach (var locationOption in line.LocationOptions)
            {
                locationOption.Selected = false;
            }
        }

        RefreshDemandLinesSnapshot();
    }

    private void ClearDuplicateNotice()
    {
        _duplicateWaitlistId = string.Empty;
        _notificationService.ClearStatusAction();
    }

    private void ApplyUserDefaults(Model_CustomerPullPack_UserDefaults defaults)
    {
        CurrentUserDefaults = defaults;
        ApplyMockCustomerDiscovery(defaults);
        SelectedPrintMode = defaults.DefaultPrintPreset;
        SelectedSortMode = defaults.DefaultSortMode;
        ShowShortagesOnly = defaults.DefaultShortagesOnly;
        ShowUnpulledOnly = defaults.DefaultUnpulledOnly;
        ShowLateOrdersOnly = defaults.DefaultLateOrdersOnly;

        if (defaults.LastGoodDateFrom.HasValue)
        {
            DateFrom = new DateTimeOffset(defaults.LastGoodDateFrom.Value);
        }

        if (defaults.LastGoodDateTo.HasValue)
        {
            DateTo = new DateTimeOffset(defaults.LastGoodDateTo.Value);
        }

        if (
            string.IsNullOrWhiteSpace(CustomerSearchText)
            && string.IsNullOrWhiteSpace(defaults.DefaultCustomerId) is false
        )
        {
            CustomerSearchText = ResolveCustomerDisplay(defaults.DefaultCustomerId);
            return;
        }

        if (
            string.IsNullOrWhiteSpace(CustomerSearchText)
            && UseMockData
            && FavoriteCustomerIds.Count > 0
        )
        {
            CustomerSearchText = FavoriteCustomerIds[0];
        }
    }

    private static Model_CustomerPullPack_DemandLine CloneDemandLineForDialog(
        Model_CustomerPullPack_DemandLine source
    )
    {
        return new Model_CustomerPullPack_DemandLine
        {
            SourceLineKey = source.SourceLineKey,
            CustomerId = source.CustomerId,
            CustomerName = source.CustomerName,
            CustomerOrderId = source.CustomerOrderId,
            ParentPartId = source.ParentPartId,
            PullDate = source.PullDate,
            OldestAdded = source.OldestAdded,
            QuantityToPack = source.QuantityToPack,
            ShipQuantity = source.ShipQuantity,
            FgOnHandQuantity = source.FgOnHandQuantity,
            FgLocationId = source.FgLocationId,
            ShortageFlag = source.ShortageFlag,
            LateOrderFlag = source.LateOrderFlag,
            RecheckIndicator = source.RecheckIndicator,
            QtySatisfied = source.QtySatisfied,
            FulfillmentStatusDisplay = source.FulfillmentStatusDisplay,
            HasLinkedWaitlist = source.HasLinkedWaitlist,
            LinkedWaitlistId = source.LinkedWaitlistId,
            WaitlistStateDisplay = source.WaitlistStateDisplay,
            RequesterNote = source.RequesterNote,
            LocationOptions = source.LocationOptions.ConvertAll(option =>
            {
                return new Model_CustomerPullPack_LocationOption
                {
                    LocationKey = option.LocationKey,
                    LocationId = option.LocationId,
                    DisplayLabel = option.DisplayLabel,
                    OnHandQuantity = option.OnHandQuantity,
                    SourceType = option.SourceType,
                    Selected = option.Selected,
                };
            }),
        };
    }

    private bool UseMockData
    {
        get
        {
            _dataSourceResolver.ResolveForWorkflow();
            return _dataSourceResolver.IsMockMode;
        }
    }

    private void ApplyMockCustomerDiscovery(Model_CustomerPullPack_UserDefaults defaults)
    {
        var favoriteDisplays = defaults
            .FavoriteCustomerIds.Select(ResolveCustomerDisplay)
            .Where(static customer =>
            {
                return string.IsNullOrWhiteSpace(customer) is false;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (UseMockData)
        {
            favoriteDisplays = favoriteDisplays
                .Concat(GetMockCustomerDisplays())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        FavoriteCustomerIds = new ObservableCollection<string>(favoriteDisplays);

        if (string.IsNullOrWhiteSpace(CustomerSearchText) && FavoriteCustomerIds.Count > 0)
        {
            var preferredMockCustomerDisplay = GetPreferredMockCustomerDisplay();
            CustomerSearchText = string.IsNullOrWhiteSpace(preferredMockCustomerDisplay)
                ? FavoriteCustomerIds[0]
                : preferredMockCustomerDisplay;
        }
    }

    private List<string> GetMockCustomerDisplays()
    {
        return _mockDataCatalog
            .GetDemandRows()
            .GroupBy(
                static row =>
                {
                    return row.CustomerId;
                },
                StringComparer.OrdinalIgnoreCase
            )
            .Select(group =>
            {
                var row = group.First();
                return string.IsNullOrWhiteSpace(row.CustomerName)
                    ? row.CustomerId.Trim().ToUpperInvariant()
                    : $"{row.CustomerId.Trim().ToUpperInvariant()} - {row.CustomerName.Trim()}";
            })
            .Where(static customer =>
            {
                return string.IsNullOrWhiteSpace(customer) is false;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string GetPreferredMockCustomerDisplay()
    {
        var mockRows = _mockDataCatalog.GetDemandRows();
        if (mockRows.Count == 0)
        {
            return string.Empty;
        }

        var startDate = DateFrom.Date;
        var endDate = DateTo.Date;
        var inRangeRow = mockRows
            .Where(row =>
            {
                return row.PullDate.Date >= startDate && row.PullDate.Date <= endDate;
            })
            .OrderBy(row =>
            {
                return row.PullDate;
            })
            .FirstOrDefault();

        if (inRangeRow is not null)
        {
            return ResolveCustomerDisplay(inRangeRow.CustomerId);
        }

        var minPullDate = mockRows.Min(row =>
        {
            return row.PullDate.Date;
        });
        var maxPullDate = mockRows.Max(row =>
        {
            return row.PullDate.Date;
        });
        DateFrom = new DateTimeOffset(minPullDate);
        DateTo = new DateTimeOffset(maxPullDate);

        return ResolveCustomerDisplay(
            mockRows
                .OrderBy(row =>
                {
                    return row.PullDate;
                })
                .First()
                .CustomerId
        );
    }

    private string ResolveCustomerDisplay(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return string.Empty;
        }

        var normalizedCustomerId = customerId.Trim().ToUpperInvariant();
        var matchingMockCustomer = _mockDataCatalog
            .GetDemandRows()
            .FirstOrDefault(row =>
            {
                return string.Equals(
                    row.CustomerId,
                    normalizedCustomerId,
                    StringComparison.OrdinalIgnoreCase
                );
            });

        if (
            matchingMockCustomer is null
            || string.IsNullOrWhiteSpace(matchingMockCustomer.CustomerName)
        )
        {
            return normalizedCustomerId;
        }

        return $"{normalizedCustomerId} - {matchingMockCustomer.CustomerName.Trim()}";
    }

    private Model_CustomerPullPack_UserDefaults BuildCurrentDefaults()
    {
        return new Model_CustomerPullPack_UserDefaults
        {
            UserId = CurrentUserId,
            DefaultCustomerId = ParseCustomerId(CustomerSearchText),
            FavoriteCustomerIds = FavoriteCustomerIds.ToList(),
            LastGoodDateRangeType = "Custom",
            LastGoodDateFrom = DateFrom.Date,
            LastGoodDateTo = DateTo.Date,
            DefaultSortMode = SelectedSortMode,
            DefaultShortagesOnly = ShowShortagesOnly,
            DefaultUnpulledOnly = ShowUnpulledOnly,
            DefaultLateOrdersOnly = ShowLateOrdersOnly,
            DefaultWaitlistStatusSet =
                CurrentUserDefaults.DefaultWaitlistStatusSet.Count == 0
                    ?
                    [
                        Enum_CustomerPullPackWaitlistStatus.Requested,
                        Enum_CustomerPullPackWaitlistStatus.Accepted,
                        Enum_CustomerPullPackWaitlistStatus.Problem,
                    ]
                    : CurrentUserDefaults.DefaultWaitlistStatusSet.ToList(),
            DefaultPrintPreset = SelectedPrintMode,
        };
    }

    private async Task SaveUserDefaultsAsync(Model_CustomerPullPack_UserDefaults defaults)
    {
        var result = await _mediator.Send(
            new Command_CustomerPullPackSaveDefaults(defaults, CurrentUserId)
        );
        if (!result.IsSuccess || result.Data is null)
        {
            ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        ApplyUserDefaults(result.Data);
        ShowStatus("Customer Pull n' Pack defaults saved.", InfoBarSeverity.Success);
    }

    private string BuildFiltersSummary()
    {
        return $"Customer: {CurrentCustomerDisplay} | Date Range: {DateFrom:MM/dd/yyyy} - {DateTo:MM/dd/yyyy} | Sort: {SelectedSortMode} | Shortages: {ShowShortagesOnly} | Unpulled: {ShowUnpulledOnly} | Late: {ShowLateOrdersOnly}";
    }

    private IReadOnlyList<Model_FuzzySearchResult> BuildMockCustomerCandidates(string lookupTerm)
    {
        return _mockDataCatalog
            .GetDemandRows()
            .Where(row =>
            {
                return row.CustomerId.Contains(lookupTerm, StringComparison.OrdinalIgnoreCase)
                    || row.CustomerName.Contains(lookupTerm, StringComparison.OrdinalIgnoreCase);
            })
            .GroupBy(
                row =>
                {
                    return row.CustomerId;
                },
                StringComparer.OrdinalIgnoreCase
            )
            .OrderBy(
                group =>
                {
                    return group.Key;
                },
                StringComparer.OrdinalIgnoreCase
            )
            .Take(50)
            .Select(group =>
            {
                var customerId = group.Key.Trim().ToUpperInvariant();
                var customerName = group
                    .Select(row =>
                    {
                        return row.CustomerName?.Trim() ?? string.Empty;
                    })
                    .FirstOrDefault(static value =>
                    {
                        return string.IsNullOrWhiteSpace(value) is false;
                    });
                var label = string.IsNullOrWhiteSpace(customerName)
                    ? customerId
                    : $"{customerId} - {customerName}";

                return new Model_FuzzySearchResult
                {
                    Key = customerId,
                    Label = label,
                    Detail = string.IsNullOrWhiteSpace(customerName) ? null : customerName,
                };
            })
            .ToList();
    }

    private async Task<Model_FuzzySearchResult?> ConfirmCustomerCandidateAsync(
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        if (ShowFuzzyPickerAsync is null)
        {
            return candidates[0];
        }

        return await ShowFuzzyPickerAsync(candidates, "Select Customer");
    }

    private static string NormalizeCustomerSearchInput(string? rawInput)
    {
        return rawInput?.Trim() ?? string.Empty;
    }

    private static string BuildCustomerLookupTerm(string rawInput)
    {
        var customerName = ParseCustomerName(rawInput);
        return string.IsNullOrWhiteSpace(customerName)
            ? NormalizeCustomerSearchInput(rawInput)
            : ParseCustomerId(rawInput);
    }

    private static string ParseCustomerId(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return string.Empty;
        }

        var trimmedInput = rawInput.Trim();
        var separatorIndex = trimmedInput.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0)
        {
            return trimmedInput[..separatorIndex].Trim().ToUpperInvariant();
        }

        separatorIndex = trimmedInput.IndexOf('·');
        if (separatorIndex > 0)
        {
            return trimmedInput[..separatorIndex].Trim().ToUpperInvariant();
        }

        return trimmedInput.ToUpperInvariant();
    }

    private static string ParseCustomerName(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return string.Empty;
        }

        var trimmedInput = rawInput.Trim();
        var separatorIndex = trimmedInput.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0 && separatorIndex + 3 < trimmedInput.Length)
        {
            return trimmedInput[(separatorIndex + 3)..].Trim();
        }

        separatorIndex = trimmedInput.IndexOf('·');
        if (separatorIndex > 0 && separatorIndex + 1 < trimmedInput.Length)
        {
            return trimmedInput[(separatorIndex + 1)..].Trim();
        }

        return string.Empty;
    }

    private string CurrentUserId =>
        _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
}
