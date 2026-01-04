using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for routing label history view with date grouping and alternating colors.
/// </summary>
public partial class ViewModel_Routing_History : ViewModel_Shared_Base
{
    private readonly IService_Routing_History _historyService;

    [ObservableProperty]
    private Dictionary<DateTime, List<Model_Routing_Label>> _groupedHistory = new();

    [ObservableProperty]
    private ObservableCollection<DateGroupViewModel> _dateGroups = new();

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTime.Today;

    [ObservableProperty]
    private int _totalLabelCount;

    public ViewModel_Routing_History(
        IService_Routing_History historyService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
    }

    /// <summary>
    /// Initializes the ViewModel - loads last 30 days of history.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading history...";
            _logger.LogInfo($"Loading history from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

            var result = await _historyService.GetHistoryAsync(StartDate.DateTime, EndDate.DateTime);
            if (result.IsSuccess)
            {
                var groupedData = result.Data
                    .GroupBy(l => l.CreatedDate.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                GroupedHistory = groupedData;
                TotalLabelCount = result.Data.Count;

                // Create view models for each date group
                DateGroups.Clear();
                foreach (var kvp in groupedData.OrderByDescending(x => x.Key))
                {
                    var dateGroup = new DateGroupViewModel
                    {
                        Date = kvp.Key,
                        Labels = new ObservableCollection<Model_Routing_Label>(kvp.Value),
                        IsExpanded = kvp.Key == DateTime.Today
                    };
                    DateGroups.Add(dateGroup);
                }

                StatusMessage = $"Loaded {TotalLabelCount} labels across {DateGroups.Count} days";
                _logger.LogInfo($"Loaded {TotalLabelCount} labels grouped by {DateGroups.Count} days");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(result.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error loading history";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error loading history", Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error loading history";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        await LoadHistoryAsync();
    }
}

/// <summary>
/// Helper class for date-grouped labels in the UI.
/// </summary>
public partial class DateGroupViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private ObservableCollection<Model_Routing_Label> _labels = new();

    [ObservableProperty]
    private bool _isExpanded = false;

    public string DateHeader => $"{Date:dddd, MMMM dd, yyyy} ({Labels.Count} labels)";
}
