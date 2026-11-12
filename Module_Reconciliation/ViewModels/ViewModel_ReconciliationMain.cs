using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The working page. It depends only on the module's three published services,
/// never on a data access class and never on a collaborator (R-MVVM-01, R-MVVM-09).
/// It works one calendar day at a time and opens on today (FR-007, FR-008).
/// </summary>
public partial class ViewModel_ReconciliationMain : ViewModel_Shared_Base
{
    private readonly IService_ReconciliationPreview _preview;
    private readonly IService_ReconciliationDayReader _dayReader;

    /// <summary>Creates the working page view model.</summary>
    public ViewModel_ReconciliationMain(
        IService_ReconciliationPreview preview,
        IService_ReconciliationDayReader dayReader,
        IService_ReconciliationWriteService writeService,
        IService_ReconciliationSettings settings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _preview = preview;
        _dayReader = dayReader;
        WriteService = writeService;
        ReconciliationSettings = settings;

        SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        SweepStartDate = SelectedDate;
        SweepEndDate = SelectedDate;
    }

    /// <summary>The write service the review cycle submits every accepted line through.</summary>
    protected IService_ReconciliationWriteService WriteService { get; }

    /// <summary>The module's two windows, read once by a sweep and never written here.</summary>
    protected IService_ReconciliationSettings ReconciliationSettings { get; }

    /// <summary>The finding currently selected in the grid.</summary>
    [ObservableProperty]
    private Model_ReconciliationFinding? _selectedFinding;

    /// <summary>The working day.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDateOffset))]
    [NotifyPropertyChangedFor(nameof(DayHeaderText))]
    private DateOnly _selectedDate;

    /// <summary>Filters the grid to a part or purchase order fragment.</summary>
    [ObservableProperty]
    private string _textFilter = string.Empty;

    /// <summary>Filters the grid to one outcome, or null for every outcome.</summary>
    [ObservableProperty]
    private Enum_ReconciliationOutcome? _outcomeFilter;

    /// <summary>The day's counts.</summary>
    [ObservableProperty]
    private Model_ReconciliationDaySummary _summary = new();

    /// <summary>The day's lifecycle state.</summary>
    [ObservableProperty]
    private Enum_ReconciliationDayState _dayState = Enum_ReconciliationDayState.NotLoaded;

    /// <summary>A page-level reason for an empty day, or an empty string.</summary>
    [ObservableProperty]
    private string _emptyStateMessage = string.Empty;

    /// <summary>A page-level failure reason, or an empty string.</summary>
    [ObservableProperty]
    private string _failureMessage = string.Empty;

    /// <summary>True while the inline Infor Visual retry banner is shown.</summary>
    [ObservableProperty]
    private bool _isVisualUnavailable;

    /// <summary>The copyable run identifier.</summary>
    [ObservableProperty]
    private string _runIdentifier = string.Empty;

    /// <summary>The Infor Visual user attribution the summary strip shows.</summary>
    [ObservableProperty]
    private string _visualUserText = string.Empty;

    /// <summary>The run duration, formatted for display.</summary>
    [ObservableProperty]
    private string _runDurationText = string.Empty;

    /// <summary>The position of the selected line in the review cycle.</summary>
    [ObservableProperty]
    private int _currentLineIndex;

    /// <summary>The number of lines the review cycle covers.</summary>
    [ObservableProperty]
    private int _totalLineCount;

    /// <summary>Accepted lines for this session.</summary>
    [ObservableProperty]
    private int _acceptedCount;

    /// <summary>Skipped lines for this session.</summary>
    [ObservableProperty]
    private int _skippedCount;

    /// <summary>Every finding the loaded day produced.</summary>
    public ObservableCollection<Model_ReconciliationFinding> Findings { get; } = [];

    /// <summary>The findings that pass the current filters, which is what the grid shows.</summary>
    public ObservableCollection<Model_ReconciliationFinding> VisibleFindings { get; } = [];

    /// <summary>The outcome filter's choices, the line that clears the filter first.</summary>
    public IReadOnlyList<Model_ReconciliationOutcomeFilterChoice> OutcomeFilterChoices =>
        Model_ReconciliationOutcomeFilterChoice.All;

    /// <summary>The outcome filter as the selector shows it.</summary>
    public Model_ReconciliationOutcomeFilterChoice? OutcomeFilterChoice
    {
        get => OutcomeFilterChoices.FirstOrDefault(choice => choice.Outcome == OutcomeFilter);
        set => OutcomeFilter = value?.Outcome;
    }

    /// <summary>The visible findings grouped by part and received date, which is the grid's source.</summary>
    public ObservableCollection<Model_ReconciliationFindingGroup> GroupedFindings { get; } = [];

    /// <summary>
    /// The working day as the date picker understands it. The offset must be the
    /// machine's local offset: a UTC offset moves a midnight date into the previous
    /// day west of Greenwich, so the picker would show the wrong day and every change
    /// would push it back another one.
    /// </summary>
    public DateTimeOffset SelectedDateOffset =>
        new(SelectedDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));

    /// <summary>The working day as the summary strip shows it.</summary>
    public string DayHeaderText =>
        SelectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// The lines the review cycle covers: writable lines only. A line that cannot be
    /// pre-filled never enters the cycle and is reached through Needs Attention instead
    /// (FR-046).
    /// </summary>
    protected IReadOnlyList<Model_ReconciliationFinding> ReviewableFindings =>
        Findings
            .Where(finding => finding.State is Enum_ReconciliationFindingState.Writable)
            .ToList();

    /// <summary>Loads the day the page is already positioned on.</summary>
    public Task LoadSelectedDateAsync() => LoadDayAsync(SelectedDate);

    /// <summary>Loads a day the operator picked.</summary>
    /// <param name="date">The day to load.</param>
    public Task SelectDateAsync(DateOnly date) => LoadDayAsync(date);

    /// <summary>Moves to the nearest earlier day that holds receiving rows.</summary>
    [RelayCommand]
    private async Task GoToPreviousDayAsync()
    {
        DateOnly? nearest = await FindNearestDayAsync(-1);

        if (nearest.HasValue is false)
        {
            ShowStatus("There is no earlier receiving day.", InfoBarSeverity.Informational);
            return;
        }

        await LoadDayAsync(nearest.Value);
    }

    /// <summary>Moves to the nearest later day that holds receiving rows.</summary>
    [RelayCommand]
    private async Task GoToNextDayAsync()
    {
        DateOnly? nearest = await FindNearestDayAsync(1);

        if (nearest.HasValue is false)
        {
            ShowStatus("There is no later receiving day.", InfoBarSeverity.Informational);
            return;
        }

        await LoadDayAsync(nearest.Value);
    }

    /// <summary>Returns to today in one action, from any position.</summary>
    [RelayCommand]
    private Task GoToTodayAsync() =>
        LoadDayAsync(DateOnly.FromDateTime(DateTime.Today));

    private async Task<DateOnly?> FindNearestDayAsync(int direction)
    {
        var result = await _dayReader.FindNearestActiveDayAsync(SelectedDate, direction);

        if (result.IsSuccess is false)
        {
            ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
            return null;
        }

        return result.Data;
    }

    private async Task LoadDayAsync(DateOnly date)
    {
        IsBusy = true;
        SelectedDate = date;
        FailureMessage = string.Empty;
        IsVisualUnavailable = false;

        try
        {
            var result = await _preview.PreviewDayAsync(date);

            if (result.IsSuccess is false || result.Data is null)
            {
                DayState = Enum_ReconciliationDayState.Failed;
                FailureMessage = result.ErrorMessage;
                ClearPage();
                return;
            }

            ApplyDay(result.Data);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Drops every value the page derived from the day that was showing, so a failed
    /// load cannot leave the earlier day's rows, selection or counts on screen under
    /// the new day's header (FR-048, FR-046). The day state and the failure reason are
    /// the caller's, because they describe the day that failed rather than the old one.
    /// The grid is emptied through <see cref="ApplyFilters"/>, the same path a loaded
    /// day uses, so the grouped source and the flat source cannot drift apart.
    /// </summary>
    private void ClearPage()
    {
        Findings.Clear();
        ApplyFilters();
        Summary = new Model_ReconciliationDaySummary();
        SelectedFinding = null;
        DestinationField = string.Empty;
        EmptyStateMessage = string.Empty;
        CurrentLineIndex = 0;
        TotalLineCount = 0;
        UpdateUnresolvedCount();
        RunIdentifier = string.Empty;
        VisualUserText = string.Empty;
        RunDurationText = string.Empty;
    }

    /// <summary>Replaces the page with a freshly previewed day.</summary>
    /// <param name="day">The day to show.</param>
    protected void ApplyDay(Model_ReconciliationDay day)
    {
        DayState = day.State;
        SelectedDate = day.Date;
        Summary = day.Summary;
        FailureMessage = day.FailureReason;
        IsVisualUnavailable =
            day.Findings.Count != 0
            && day.Findings.Any(finding => finding.Outcome is Enum_ReconciliationOutcome.Error);

        RunIdentifier =
            day.Run is null
                ? string.Empty
                : day.Run.RunId.ToString("D", CultureInfo.InvariantCulture);
        VisualUserText = day.Run?.VisualUserId ?? string.Empty;
        RunDurationText =
            day.Run?.Duration is null or { TotalSeconds: < 1 }
                ? string.Empty
                : day.Run.Duration.Value.ToString(
                    @"m\:ss",
                    CultureInfo.InvariantCulture
                );

        Findings.Clear();

        foreach (Model_ReconciliationFinding finding in day.Findings)
        {
            Findings.Add(finding);
        }

        ApplyFilters();
        UpdateProgress();
        UpdateEmptyState(day);
        UpdateUnresolvedCount();
        SelectFirstReviewableLine();
    }

    /// <summary>Rebuilds the visible grid from the current filters.</summary>
    protected void ApplyFilters()
    {
        IEnumerable<Model_ReconciliationFinding> filtered = Findings;

        if (OutcomeFilter.HasValue)
        {
            Enum_ReconciliationOutcome outcome = OutcomeFilter.Value;
            filtered = filtered.Where(finding => finding.Outcome == outcome);
        }

        string text = TextFilter.Trim();

        if (text.Length != 0)
        {
            filtered = filtered.Where(finding =>
                finding.Row.PartId.Contains(text, StringComparison.OrdinalIgnoreCase)
                || finding.Row.PurchaseOrderNumber.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        VisibleFindings.Clear();

        foreach (Model_ReconciliationFinding finding in filtered)
        {
            VisibleFindings.Add(finding);
        }

        RebuildGroups();
    }

    private void RebuildGroups()
    {
        GroupedFindings.Clear();

        foreach (
            IGrouping<string, Model_ReconciliationFinding> group in VisibleFindings.GroupBy(
                finding =>
                    $"{finding.Row.PartId} · "
                    + finding.Row.TransactionDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
            )
        )
        {
            var findingGroup = new Model_ReconciliationFindingGroup(group.Key);

            foreach (Model_ReconciliationFinding finding in group)
            {
                findingGroup.Add(finding);
            }

            GroupedFindings.Add(findingGroup);
        }
    }

    /// <summary>Recomputes the review cycle's position and size.</summary>
    protected void UpdateProgress()
    {
        IReadOnlyList<Model_ReconciliationFinding> reviewable = ReviewableFindings;
        TotalLineCount = reviewable.Count;

        int index = reviewable
            .Select((finding, position) => (finding, position))
            .Where(entry => ReferenceEquals(entry.finding, SelectedFinding))
            .Select(entry => entry.position)
            .DefaultIfEmpty(-1)
            .First();

        CurrentLineIndex = index + 1;
        OnPropertyChanged(nameof(CyclePositionText));
        OnPropertyChanged(nameof(RunProvenanceText));
    }

    partial void OnVisualUserTextChanged(string value) =>
        OnPropertyChanged(nameof(RunProvenanceText));

    partial void OnRunDurationTextChanged(string value) =>
        OnPropertyChanged(nameof(RunProvenanceText));

    private void UpdateEmptyState(Model_ReconciliationDay day)
    {
        if (Findings.Count == 0)
        {
            EmptyStateMessage = day.HasActivity
                ? $"Every row saved for {day.Date:yyyy-MM-dd} was a reprint, so there is nothing to reconcile."
                : $"No receiving rows were saved for {day.Date:yyyy-MM-dd}.";
            return;
        }

        bool everyLineAlreadyMatches =
            day.Findings.All(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.Unchanged
                        or Enum_ReconciliationOutcome.Skipped
            );

        EmptyStateMessage = everyLineAlreadyMatches
            ? $"Every line saved for {day.Date:yyyy-MM-dd} already matches where the material went."
            : string.Empty;
    }

    partial void OnTextFilterChanged(string value) => ApplyFilters();

    partial void OnOutcomeFilterChanged(Enum_ReconciliationOutcome? value)
    {
        OnPropertyChanged(nameof(OutcomeFilterChoice));
        ApplyFilters();
    }

    partial void OnSelectedFindingChanged(Model_ReconciliationFinding? value)
    {
        DestinationField = value?.ProposedLocation ?? string.Empty;
        OnPropertyChanged(nameof(HasSelectedFinding));
        OnPropertyChanged(nameof(HasNoSelectedFinding));
        UpdateProgress();
    }

    /// <summary>True when a line is selected, so the detail pane can show it.</summary>
    public bool HasSelectedFinding => SelectedFinding is not null;

    /// <summary>True when nothing is selected, so the pane can show its placeholder.</summary>
    public bool HasNoSelectedFinding => SelectedFinding is null;

    /// <summary>The cycle position as one readable phrase, for the summary line.</summary>
    public string CyclePositionText =>
        TotalLineCount == 0 ? string.Empty
        : CurrentLineIndex == 0 ? $"{TotalLineCount} to review"
        : $"Line {CurrentLineIndex} of {TotalLineCount}";

    /// <summary>
    /// The quiet provenance line: cycle position, who moved the material in Infor
    /// Visual, and how long the pass took. Empty parts are omitted, so no separator
    /// is left dangling. The run identifier is deliberately not shown — it belongs in
    /// the log, not on the page.
    /// </summary>
    public string RunProvenanceText =>
        string.Join(
            "   ·   ",
            new[]
            {
                CyclePositionText,
                string.IsNullOrWhiteSpace(VisualUserText)
                    ? string.Empty
                    : $"Moved in Infor Visual by {VisualUserText}",
                RunDurationText,
            }.Where(part => string.IsNullOrWhiteSpace(part) is false)
        );

    /// <summary>
    /// The day's counts on one readable line. FR-049 requires every count to be
    /// visible, so all of them are named rather than shown as a bare number strip.
    /// </summary>
    public string SummaryLineText =>
        Summary.Scanned == 0
            ? "No receiving lines for this day."
            : string.Join(
                "  ·  ",
                $"{Summary.Scanned} lines",
                $"{Summary.Written} updated",
                $"{Summary.Unchanged} already correct",
                $"{Summary.Unresolved} need attention",
                $"{Summary.NotFound} not found",
                $"{Summary.Skipped} skipped",
                $"{Summary.Error} errors"
            );
}
