using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
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

    private readonly IService_ReceivingWorkflow _workflowService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private readonly ViewModel_Receiving_POEntry? _poEntry;

    /// <summary>
    /// The parts offered for the loaded PO, already narrowed by <see cref="SearchText"/>. Replaced
    /// whole on every rebuild rather than cleared and refilled, so the grid never redraws row by row.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParts))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    [NotifyPropertyChangedFor(nameof(PartsCountText))]
    private ObservableCollection<Model_InforVisualPart> _parts = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPart))]
    [NotifyPropertyChangedFor(nameof(SelectedPartSummaryText))]
    [NotifyPropertyChangedFor(nameof(HasQualityHold))]
    [NotifyPropertyChangedFor(nameof(QualityHoldText))]
    private Model_InforVisualPart? _selectedPart;

    public ViewModel_Receiving_PartSelection(
        IService_ReceivingWorkflow workflowService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _viewModelRegistry = viewModelRegistry;

        _poEntry = _viewModelRegistry.GetViewModels<ViewModel_Receiving_POEntry>().FirstOrDefault();
        if (_poEntry is not null)
        {
            _poEntry.PropertyChanged += OnPoEntryPropertyChanged;
        }

        _workflowService.StepChanged += OnStepChanged;
        _viewModelRegistry.Register(this);
        Refresh();
    }

    /// <summary>True when the PO's part list produced at least one row to choose from.</summary>
    public bool HasParts => Parts.Count > 0;

    /// <summary>True when the grid has nothing to show, whether the PO is unloaded or the search missed.</summary>
    public bool ShowEmptyState => Parts.Count == 0;

    /// <summary>Why the grid is empty, in wording that tells the clerk what to do next.</summary>
    public string EmptyStateText =>
        _poEntry is null || _poEntry.Parts.Count == 0
            ? NoPoLoadedText
            : $"No part on this PO matches \"{SearchText.Trim()}\".";

    /// <summary>How many parts the search is offering, so the count is visible without counting rows.</summary>
    public string PartsCountText =>
        Parts.Count == 1 ? "1 part" : $"{Parts.Count} parts";

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
        if (_poEntry?.SelectedPart is not null && SelectedPart is null)
        {
            SelectedPart = _poEntry.SelectedPart;
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

    partial void OnSelectedPartChanged(Model_InforVisualPart? value)
    {
        // The PO page owns the rest of the selection work: PO due date, the part's default
        // location, the package type guess, and the quality-hold warning.
        if (_poEntry is not null && !ReferenceEquals(_poEntry.SelectedPart, value))
        {
            _poEntry.SelectedPart = value;
        }
        else
        {
            _workflowService.CurrentPart = value;
        }

        NotifyWorkflowNextButtonStateChanged();
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    private void RebuildParts()
    {
        var source = _poEntry?.Parts ?? [];
        var term = SearchText.Trim();

        Parts = new ObservableCollection<Model_InforVisualPart>(
            string.IsNullOrWhiteSpace(term)
                ? source
                : source.Where(part => Matches(part, term))
        );
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
