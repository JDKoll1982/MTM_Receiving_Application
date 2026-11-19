using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels;

/// <summary>
/// The guided "Select part" step: the part on the loaded PO is chosen here, on a page of its own,
/// instead of on the same page as the PO box.
/// </summary>
/// <remarks>
/// The parts come from <see cref="ViewModel_Receiving_POEntry"/>, which owns the PO lookup and the
/// part list. Choosing a part here writes straight back to that view model, so the rest of its work
/// - the PO due date, the part's default location, the package type guess, and the quality-hold
/// warning - all still runs exactly once and in one place. Only the non-PO flow skips this step,
/// because there the part number is typed on the PO page itself.
/// </remarks>
public partial class ViewModel_Receiving_PartSelection
    : ViewModel_Receiving_StepBase,
        IResettableViewModel
{
    private const string NoPoLoadedText =
        "Load the PO number first, then come back to choose the part.";

    private const string NoPartsLoadedPrefix = "The parts for ";

    private const string NoPartsLoadedSuffix =
        " have not been loaded on this page yet. Go back to the PO step, load the PO, then come forward again.";

    private const string AllLocationsOption = "All locations";

    private readonly IService_ReceivingWorkflow _workflowService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private readonly IService_ReceivingSettings? _receivingSettings;

    /// <summary>
    /// The PO page that owns the part list. Resolved on demand rather than captured once, because a
    /// page built before the PO page (or kept while a newer one replaced it) would otherwise hold a
    /// reference that never has parts.
    /// </summary>
    private ViewModel_Receiving_POEntry? _poEntry;

    /// <summary>
    /// True while the view state is being set from code (initial load or a sort click) so the
    /// per-property handlers do not each rebuild and re-save as they fire.
    /// </summary>
    private bool _isApplyingViewState;

    /// <summary>
    /// True while the double-click handler is moving the walk on, so a second click cannot start the
    /// same move twice.
    /// </summary>
    private bool _isAdvancing;

    /// <summary>
    /// The parts offered for the loaded PO, already narrowed and ordered by the search text, the
    /// filters, and the sort. Replaced whole on every rebuild rather than cleared and refilled, so
    /// the grid never redraws row by row.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParts))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private ObservableCollection<Model_InforVisualPart> _parts = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>The column the list is ordered by. One of the <see cref="ReceivingPartSortKeys"/> values.</summary>
    [ObservableProperty]
    private string _sortColumn = ReceivingPartSortKeys.ReceiveDate;

    /// <summary>False for the default newest-first order, true once the header has been clicked twice.</summary>
    [ObservableProperty]
    private bool _sortAscending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    [NotifyPropertyChangedFor(nameof(ActiveFilterCount))]
    private bool _onHandOnly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    [NotifyPropertyChangedFor(nameof(ActiveFilterCount))]
    private bool _qualityHoldOnly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    [NotifyPropertyChangedFor(nameof(ActiveFilterCount))]
    private string _selectedLocation = AllLocationsOption;

    /// <summary>The location list the filter offers: every location on this PO's parts, plus "all".</summary>
    [ObservableProperty]
    private ObservableCollection<string> _availableLocations = [AllLocationsOption];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPart))]
    [NotifyPropertyChangedFor(nameof(SelectedPartSummaryText))]
    [NotifyPropertyChangedFor(nameof(HasQualityHold))]
    [NotifyPropertyChangedFor(nameof(QualityHoldText))]
    private Model_InforVisualPart? _selectedPart;

    public ViewModel_Receiving_PartSelection(
        IService_ReceivingWorkflow workflowService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ReceivingSettings receivingSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _viewModelRegistry = viewModelRegistry;
        _receivingSettings = receivingSettings;

        // Resolve the PO page now so its parts arrive here from the start, and resolve it again if
        // it has since been replaced (see the PoEntry property).
        _ = PoEntry;

        _workflowService.StepChanged += OnStepChanged;
        _viewModelRegistry.Register(this);
        Refresh();

        // The saved sort and filters are per user, so they arrive after the page is already usable.
        ViewPreferencesLoaded = LoadViewPreferencesAsync();
    }

    /// <summary>
    /// Completes once the saved sort and filters have been applied, so a caller that needs the final
    /// ordering (a test, or a page that must not flash) can wait for it instead of guessing.
    /// </summary>
    public Task ViewPreferencesLoaded { get; }

    /// <summary>
    /// The PO page that owns the part list, resolved on each use. The registry keeps the most
    /// recently built view model last, and that is the one on screen; an earlier capture could point
    /// at a page from a previous visit whose part list was never loaded.
    /// </summary>
    private ViewModel_Receiving_POEntry? PoEntry
    {
        get
        {
            var resolved = _viewModelRegistry
                .GetViewModels<ViewModel_Receiving_POEntry>()
                .LastOrDefault();

            if (ReferenceEquals(resolved, _poEntry))
            {
                return _poEntry;
            }

            if (_poEntry is not null)
            {
                _poEntry.PropertyChanged -= OnPoEntryPropertyChanged;
            }

            _poEntry = resolved;

            if (_poEntry is not null)
            {
                _poEntry.PropertyChanged += OnPoEntryPropertyChanged;
            }

            return _poEntry;
        }
    }

    /// <summary>True when the PO's part list produced at least one row to choose from.</summary>
    public bool HasParts => Parts.Count > 0;

    /// <summary>True when the grid has nothing to show, whether the PO is unloaded or the search missed.</summary>
    public bool ShowEmptyState => Parts.Count == 0;

    /// <summary>Why the grid is empty, in wording that tells the clerk what to do next.</summary>
    public string EmptyStateText
    {
        get
        {
            var poEntry = PoEntry;

            if (poEntry is null || poEntry.Parts.Count == 0)
            {
                var poNumber = _workflowService.CurrentPONumber?.Trim();

                // A PO number without a part list is the page having been reached before the PO was
                // loaded. Saying "load the PO number first" there would be wrong - it is already in.
                return string.IsNullOrWhiteSpace(poNumber)
                    ? NoPoLoadedText
                    : $"{NoPartsLoadedPrefix}{poNumber}{NoPartsLoadedSuffix}";
            }

            var term = SearchText.Trim();

            if (term.Length == 0 && HasActiveFilters)
            {
                return "No part on this PO meets the filters you picked. Clear the filters to see every part.";
            }

            return term.Length > 0 && HasActiveFilters
                ? $"No part on this PO matches \"{term}\" with the filters you picked."
                : $"No part on this PO matches \"{term}\".";
        }
    }

    /// <summary>True when anything other than the search box is narrowing the list.</summary>
    public bool HasActiveFilters =>
        OnHandOnly || QualityHoldOnly || IsLocationFilterActive;

    /// <summary>How many filters are on, so the filter button can say so at a glance.</summary>
    public int ActiveFilterCount =>
        (OnHandOnly ? 1 : 0) + (QualityHoldOnly ? 1 : 0) + (IsLocationFilterActive ? 1 : 0);

    private bool IsLocationFilterActive =>
        !string.IsNullOrWhiteSpace(SelectedLocation)
        && !string.Equals(SelectedLocation, AllLocationsOption, StringComparison.OrdinalIgnoreCase);

    /// <summary>True once a part has been chosen, which is what the step needs to move on.</summary>
    public bool HasSelectedPart => SelectedPart is not null;

    /// <summary>The one-line identity of the chosen part for the summary card.</summary>
    public string SelectedPartSummaryText =>
        SelectedPart is null
            ? "No part chosen yet."
            : string.IsNullOrWhiteSpace(SelectedPart.Description)
                ? SelectedPart.PartID ?? string.Empty
                : $"{SelectedPart.PartID} - {SelectedPart.Description}";

    /// <summary>True when the chosen part needs the quality-hold conversation before it can be received.</summary>
    public bool HasQualityHold => SelectedPart?.RequiresQualityHold == true;

    /// <summary>The reason shown with the quality-hold warning.</summary>
    public string QualityHoldText =>
        SelectedPart?.QualityHoldRestrictionType is { Length: > 0 } restriction
            ? restriction
            : "This part needs a quality hold acknowledgement before it can be received.";

    /// <summary>
    /// The PO the list belongs to, so the page cannot be mistaken for an unprompted search. The PO
    /// number already carries its own prefix ("PO-073026"), so no label is added in front of it.
    /// </summary>
    public string PoContextText =>
        string.IsNullOrWhiteSpace(_workflowService.CurrentPONumber)
            ? "No PO loaded"
            : _workflowService.CurrentPONumber.Trim();

    /// <summary>
    /// Re-reads the part list from the PO page and re-applies the current search.
    /// </summary>
    public void Refresh()
    {
        if (PoEntry?.SelectedPart is not null && SelectedPart is null)
        {
            SelectedPart = PoEntry.SelectedPart;
        }

        RebuildParts();
        OnPropertyChanged(nameof(PoContextText));
        OnPropertyChanged(nameof(EmptyStateText));
    }

    /// <summary>
    /// Clears the part chosen on this page. Called when the workflow is reset, so a part from a
    /// finished receipt cannot leak into the next one.
    /// </summary>
    public void ResetToDefaults()
    {
        SearchText = string.Empty;
        SelectedPart = null;
    }

    partial void OnSearchTextChanged(string value)
    {
        RebuildParts();
        OnPropertyChanged(nameof(EmptyStateText));
    }

    partial void OnSortColumnChanged(string value) => ApplyViewStateChange();

    partial void OnSortAscendingChanged(bool value) => ApplyViewStateChange();

    partial void OnOnHandOnlyChanged(bool value) => ApplyViewStateChange();

    partial void OnQualityHoldOnlyChanged(bool value) => ApplyViewStateChange();

    partial void OnSelectedLocationChanged(string value) => ApplyViewStateChange();

    partial void OnSelectedPartChanged(Model_InforVisualPart? value)
    {
        // The PO page owns the rest of the selection work: PO due date, the part's default
        // location, the package type guess, and the quality-hold warning.
        if (PoEntry is not null && !ReferenceEquals(PoEntry.SelectedPart, value))
        {
            PoEntry.SelectedPart = value;
        }
        else
        {
            _workflowService.CurrentPart = value;
        }

        NotifyWorkflowNextButtonStateChanged();
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    /// <summary>
    /// Chooses the part the clerk picked and moves straight on to the next step. This is what a
    /// double-click on a row does, so a list of options can be worked without reaching for Next.
    /// </summary>
    /// <remarks>
    /// The part is already chosen by the row selection itself; what is left is the quality-hold
    /// conversation that choosing it may have started. Moving on while that is still open would
    /// carry a part the clerk has not agreed to receive.
    /// </remarks>
    [RelayCommand]
    private async Task SelectPartAndAdvanceAsync()
    {
        if (_isAdvancing || SelectedPart is null)
        {
            return;
        }

        _isAdvancing = true;
        try
        {
            var poEntry = PoEntry;
            if (poEntry is not null && !await poEntry.IsSelectedPartUsableAsync())
            {
                return;
            }

            if (SelectedPart is null)
            {
                return;
            }

            // The workflow page owns the walk, including the messages a failed step shows, so ask it
            // to move rather than moving the service behind its back.
            foreach (
                var workflowViewModel in _viewModelRegistry.GetViewModels<ViewModel_Receiving_Workflow>()
            )
            {
                if (workflowViewModel.NextStepCommand.CanExecute(null))
                {
                    await workflowViewModel.NextStepCommand.ExecuteAsync(null);
                    return;
                }
            }

            // No workflow page is registered: move the service directly so the click still works.
            var result = await _workflowService.AdvanceToNextStepAsync();
            if (!result.Success)
            {
                ShowStatus(
                    string.Join(" ", result.ValidationErrors),
                    InfoBarSeverity.Warning
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error moving on from the part page: {ex.Message}",
                ex,
                nameof(ViewModel_Receiving_PartSelection)
            );
        }
        finally
        {
            _isAdvancing = false;
        }
    }

    /// <summary>
    /// Puts every filter back to "show everything". The sort order is left alone, because it is a
    /// reading preference rather than something that hides parts.
    /// </summary>
    [RelayCommand]
    private void ResetFilters()
    {
        _isApplyingViewState = true;
        try
        {
            OnHandOnly = false;
            QualityHoldOnly = false;
            SelectedLocation = AllLocationsOption;
        }
        finally
        {
            _isApplyingViewState = false;
        }

        ApplyViewStateChange();
    }

    /// <summary>
    /// Called from the View's Sorting event handler to order the list by a column.
    /// </summary>
    /// <param name="columnKey">One of the <see cref="ReceivingPartSortKeys"/> values.</param>
    /// <param name="ascending">True to order smallest/oldest first.</param>
    public void SortBy(string columnKey, bool ascending)
    {
        if (string.IsNullOrWhiteSpace(columnKey))
        {
            return;
        }

        _isApplyingViewState = true;
        try
        {
            SortAscending = ascending;
            SortColumn = columnKey;
        }
        finally
        {
            _isApplyingViewState = false;
        }

        ApplyViewStateChange();
    }

    private void ApplyViewStateChange()
    {
        if (_isApplyingViewState)
        {
            return;
        }

        RebuildParts();
        OnPropertyChanged(nameof(EmptyStateText));
        _ = SaveViewPreferencesAsync();
    }

    private void RebuildParts()
    {
        RefreshAvailableLocations();

        var source = PoEntry?.Parts ?? [];
        var term = SearchText.Trim();

        IEnumerable<Model_InforVisualPart> matches = source;

        if (!string.IsNullOrWhiteSpace(term))
        {
            matches = matches.Where(part => Matches(part, term));
        }

        if (OnHandOnly)
        {
            matches = matches.Where(static part => part.OnHandQty > 0);
        }

        if (QualityHoldOnly)
        {
            matches = matches.Where(static part => part.RequiresQualityHold);
        }

        if (IsLocationFilterActive)
        {
            matches = matches.Where(part =>
                string.Equals(
                    part.Location?.Trim(),
                    SelectedLocation.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        Parts = new ObservableCollection<Model_InforVisualPart>(ApplySort(matches));
    }

    private void RefreshAvailableLocations()
    {
        var locations = new List<string> { AllLocationsOption };

        foreach (var part in PoEntry?.Parts ?? [])
        {
            var location = part.Location?.Trim();
            if (string.IsNullOrWhiteSpace(location))
            {
                continue;
            }

            if (
                locations.Any(existing =>
                    string.Equals(existing, location, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                continue;
            }

            locations.Add(location);
        }

        // A saved location has to stay selectable even before the PO that carries it is loaded.
        if (
            IsLocationFilterActive
            && !locations.Contains(SelectedLocation, StringComparer.OrdinalIgnoreCase)
        )
        {
            locations.Add(SelectedLocation);
        }

        if (locations.SequenceEqual(AvailableLocations, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        AvailableLocations = new ObservableCollection<string>(locations);
    }

    private List<Model_InforVisualPart> ApplySort(IEnumerable<Model_InforVisualPart> parts) =>
        SortColumn switch
        {
            ReceivingPartSortKeys.PartId => SortByKey(
                parts,
                static part => part.PartID,
                StringComparer.OrdinalIgnoreCase
            ),
            ReceivingPartSortKeys.Description => SortByKey(
                parts,
                static part => part.Description,
                StringComparer.OrdinalIgnoreCase
            ),
            ReceivingPartSortKeys.Location => SortByKey(
                parts,
                static part => part.Location,
                StringComparer.OrdinalIgnoreCase
            ),
            ReceivingPartSortKeys.OnHand => SortByKey(
                parts,
                static part => part.OnHandQty,
                Comparer<decimal>.Default
            ),
            _ => SortByReceiveDate(parts),
        };

    private List<Model_InforVisualPart> SortByKey<TKey>(
        IEnumerable<Model_InforVisualPart> parts,
        Func<Model_InforVisualPart, TKey> keySelector,
        IComparer<TKey> comparer
    )
    {
        var ordered = SortAscending
            ? parts.OrderBy(keySelector, comparer)
            : parts.OrderByDescending(keySelector, comparer);

        return ordered.ThenBy(static part => part.PartID, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Most recently received first, which is the default. A part that has never been received has
    /// no date at all, so it always sorts after the received ones; a missing date never floats a
    /// part to the top of the list.
    /// </summary>
    private List<Model_InforVisualPart> SortByReceiveDate(
        IEnumerable<Model_InforVisualPart> parts
    )
    {
        var materialized = parts.ToList();
        var received = materialized.Where(static part => part.LastReceivedDate.HasValue);
        var neverReceived = materialized
            .Where(static part => !part.LastReceivedDate.HasValue)
            .OrderBy(static part => part.PartID, StringComparer.OrdinalIgnoreCase);

        var orderedReceived = SortAscending
            ? received.OrderBy(static part => part.LastReceivedDate)
            : received.OrderByDescending(static part => part.LastReceivedDate);

        return orderedReceived
            .ThenBy(static part => part.PartID, StringComparer.OrdinalIgnoreCase)
            .Concat(neverReceived)
            .ToList();
    }

    private async Task LoadViewPreferencesAsync()
    {
        if (_receivingSettings is null)
        {
            return;
        }

        _isApplyingViewState = true;
        try
        {
            var sortColumn = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionSortColumn
            );
            var sortAscending = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionSortAscending
            );
            var onHandOnly = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionOnHandOnly
            );
            var qualityHoldOnly = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionQualityHoldOnly
            );
            var location = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionLocation
            );

            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                SortColumn = sortColumn.Trim();
            }

            SortAscending = string.Equals(sortAscending, "true", StringComparison.OrdinalIgnoreCase);
            OnHandOnly = string.Equals(onHandOnly, "true", StringComparison.OrdinalIgnoreCase);
            QualityHoldOnly = string.Equals(
                qualityHoldOnly,
                "true",
                StringComparison.OrdinalIgnoreCase
            );
            SelectedLocation = string.IsNullOrWhiteSpace(location)
                ? AllLocationsOption
                : location.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error loading the Select Part sort and filters: {ex.Message}",
                ex,
                nameof(ViewModel_Receiving_PartSelection)
            );
        }
        finally
        {
            _isApplyingViewState = false;
        }

        RebuildParts();
        OnPropertyChanged(nameof(EmptyStateText));
    }

    private async Task SaveViewPreferencesAsync()
    {
        if (_receivingSettings is null)
        {
            return;
        }

        try
        {
            await _receivingSettings.SaveStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionSortColumn,
                SortColumn ?? ReceivingPartSortKeys.ReceiveDate
            );
            await _receivingSettings.SaveStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionSortAscending,
                SortAscending ? "true" : "false"
            );
            await _receivingSettings.SaveStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionOnHandOnly,
                OnHandOnly ? "true" : "false"
            );
            await _receivingSettings.SaveStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionQualityHoldOnly,
                QualityHoldOnly ? "true" : "false"
            );
            await _receivingSettings.SaveStringAsync(
                ReceivingSettingsKeys.UiText.PartSelectionLocation,
                IsLocationFilterActive ? SelectedLocation : string.Empty
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error saving the Select Part sort and filters: {ex.Message}",
                ex,
                nameof(ViewModel_Receiving_PartSelection)
            );
        }
    }

    private static bool Matches(Model_InforVisualPart part, string term) =>
        Contains(part.PartID, term) || Contains(part.Description, term);

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

    private void OnPoEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel_Receiving_POEntry.Parts))
        {
            RebuildParts();
            OnPropertyChanged(nameof(EmptyStateText));
            return;
        }

        // The PO page drops a part the clerk will not acknowledge a quality hold for. Following it
        // keeps this page from showing a row as chosen when nothing is actually chosen.
        if (
            e.PropertyName is nameof(ViewModel_Receiving_POEntry.SelectedPart)
            && sender is ViewModel_Receiving_POEntry poEntry
            && !ReferenceEquals(poEntry.SelectedPart, SelectedPart)
        )
        {
            SelectedPart = poEntry.SelectedPart;
        }
    }

    private void OnStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.PartSelection)
        {
            Refresh();
        }
    }

    private void NotifyWorkflowNextButtonStateChanged()
    {
        foreach (var workflowViewModel in _viewModelRegistry.GetViewModels<ViewModel_Receiving_Workflow>())
        {
            workflowViewModel.RefreshNextButtonEnabled();
        }
    }
}

/// <summary>
/// The column keys the Select Part grid tags its columns with. They double as the persisted sort
/// column values, so they must not change without a settings migration.
/// </summary>
public static class ReceivingPartSortKeys
{
    public const string ReceiveDate = "RecvDate";

    public const string PartId = "PartID";

    public const string Description = "Description";

    public const string OnHand = "OnHand";

    public const string Location = "Location";
}
