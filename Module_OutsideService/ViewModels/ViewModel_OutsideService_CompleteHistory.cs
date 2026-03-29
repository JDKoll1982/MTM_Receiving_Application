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
/// Completed-history ViewModel for Outside Service lines.
/// </summary>
public partial class ViewModel_OutsideService_CompleteHistory : ViewModel_Shared_Base
{
    private readonly IService_OutsideService _outsideService;
    private List<Model_OutsideServiceRequestLine> _allLines = new();

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceRequestLine> _filteredLines = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedLine))]
    private Model_OutsideServiceRequestLine? _selectedLine;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedVendorFilter = "All";

    [ObservableProperty]
    private string _selectedDateRange = "Last 30 days";

    [ObservableProperty]
    private ObservableCollection<string> _vendorFilterOptions = new(new[] { "All" });

    [ObservableProperty]
    private ObservableCollection<string> _dateRangeOptions = new(
        new[] { "Last 30 days", "Last 90 days", "All" }
    );

    public ViewModel_OutsideService_CompleteHistory(
        IService_OutsideService outsideService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _outsideService = outsideService ?? throw new ArgumentNullException(nameof(outsideService));
        Title = "Outside Service Complete History";
    }

    /// <summary>
    /// Gets whether a completed line is selected.
    /// </summary>
    public bool HasSelectedLine => SelectedLine is not null;

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
            ShowStatus("Loading completed Outside Service history…");
            var result = await _outsideService.GetCompletedLinesAsync();
            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            _allLines = result.Data;
            VendorFilterOptions.Clear();
            VendorFilterOptions.Add("All");
            foreach (
                var vendor in _allLines
                    .Select(line => line.SetupVendorName)
                    .Where(vendor => !string.IsNullOrWhiteSpace(vendor))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order()
            )
            {
                VendorFilterOptions.Add(vendor!);
            }

            ApplyFilters();
            ShowStatus($"Loaded {_allLines.Count} completed line(s).", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_OutsideService_CompleteHistory)
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

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedVendorFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedDateRangeChanged(string value)
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

        if (!string.Equals(SelectedVendorFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            lines = lines.Where(line =>
                string.Equals(
                    line.SetupVendorName,
                    SelectedVendorFilter,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        var cutoff = SelectedDateRange switch
        {
            "Last 30 days" => DateTime.UtcNow.AddDays(-30),
            "Last 90 days" => DateTime.UtcNow.AddDays(-90),
            _ => DateTime.MinValue,
        };

        if (cutoff != DateTime.MinValue)
        {
            lines = lines.Where(line => line.CompletedUtc is null || line.CompletedUtc >= cutoff);
        }

        lines = lines
            .OrderByDescending(line => line.CompletedUtc ?? DateTime.MinValue)
            .ThenByDescending(line => line.CreatedUtc);

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
