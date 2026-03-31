using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.ViewModels;

/// <summary>
/// Active waitlist ViewModel for Outside Service lines.
/// </summary>
public partial class ViewModel_OutsideService_Waitlist : ViewModel_Shared_Base
{
    private readonly IService_OutsideService _outsideService;
    private List<Model_OutsideServiceRequestLine> _allLines = new();

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceRequestLine> _filteredLines = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedLine))]
    [NotifyPropertyChangedFor(nameof(HasSelectedInitializeLine))]
    [NotifyPropertyChangedFor(nameof(HasSelectedSetupLine))]
    private Model_OutsideServiceRequestLine? _selectedLine;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedPhaseFilter = "All";

    [ObservableProperty]
    private string _selectedSortOption = "Oldest waiting";

    [ObservableProperty]
    private ObservableCollection<string> _phaseFilterOptions = new(
        new[] { "All", "Initialize", "Setup" }
    );

    [ObservableProperty]
    private ObservableCollection<string> _sortOptions = new(
        new[] { "Oldest waiting", "Newest first", "Part A-Z" }
    );

    public ViewModel_OutsideService_Waitlist(
        IService_OutsideService outsideService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _outsideService = outsideService ?? throw new ArgumentNullException(nameof(outsideService));
        Title = "Outside Service Active Waitlist";
    }

    public event Action<Model_OutsideServiceRequestLine>? SetupRequested;

    public event Action? LineCompleted;

    public event Action<Model_OutsideServiceRequestLine?>? SelectedLineChanged;

    /// <summary>
    /// Gets whether a waitlist line is selected.
    /// </summary>
    public bool HasSelectedLine => SelectedLine is not null;

    /// <summary>
    /// Gets whether an Initialize-phase line is selected.
    /// </summary>
    public bool HasSelectedInitializeLine => SelectedLine?.IsInitialize == true;

    /// <summary>
    /// Gets whether a Setup-phase line is selected.
    /// </summary>
    public bool HasSelectedSetupLine => SelectedLine?.IsSetup == true;

    partial void OnSelectedLineChanged(Model_OutsideServiceRequestLine? value)
    {
        SelectedLineChanged?.Invoke(value);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Loading active Outside Service lines…");
            var result = await _outsideService.GetOpenLinesAsync();
            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            _allLines = result.Data;
            ApplyFilters();
            ShowStatus($"Loaded {_allLines.Count} active line(s).", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_OutsideService_Waitlist)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private void OpenSelectedLine()
    {
        if (SelectedLine is null || !SelectedLine.IsInitialize && !SelectedLine.IsSetup)
        {
            return;
        }

        SetupRequested?.Invoke(SelectedLine);
    }

    [RelayCommand]
    private async Task MarkSelectedLineCompleteAsync()
    {
        if (SelectedLine is null || !SelectedLine.IsSetup || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var completionResult = await _outsideService.MarkCompleteAsync(
                SelectedLine.OutsideServiceRequestLineId,
                null
            );
            if (!completionResult.IsSuccess)
            {
                ShowStatus(completionResult.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            SelectedLine.LinePhase = Enum_OutsideServiceLinePhase.Complete;
            SelectedLine.CompletedUtc = DateTime.UtcNow;
            ShowStatus($"Marked {SelectedLine.QueueKey} complete.", InfoBarSeverity.Success);
            LineCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(MarkSelectedLineCompleteAsync),
                nameof(ViewModel_OutsideService_Waitlist)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedPhaseFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        IEnumerable<Model_OutsideServiceRequestLine> lines = _allLines;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            lines = lines.Where(line =>
                line.RequestNumber.Contains(term, StringComparison.OrdinalIgnoreCase)
                || line.PartId.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (
                    line.SetupVendorName?.Contains(term, StringComparison.OrdinalIgnoreCase)
                    ?? false
                )
                || (line.BOLNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        if (!string.Equals(SelectedPhaseFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            lines = lines.Where(line =>
                string.Equals(
                    line.LinePhase.ToString(),
                    SelectedPhaseFilter,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        lines = SelectedSortOption switch
        {
            "Newest first" => lines.OrderByDescending(line => line.CreatedUtc),
            "Part A-Z" => lines.OrderBy(line => line.PartId),
            _ => lines.OrderBy(line => line.CreatedUtc),
        };

        FilteredLines.Clear();
        foreach (var line in lines)
        {
            FilteredLines.Add(line);
        }

        if (SelectedLine is null || !FilteredLines.Contains(SelectedLine))
        {
            SelectedLine = FilteredLines.FirstOrDefault();
        }
    }
}
