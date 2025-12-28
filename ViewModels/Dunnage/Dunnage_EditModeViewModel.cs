using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Edit Mode (historical data editing)
/// </summary>
public partial class Dunnage_EditModeViewModel : Shared_BaseViewModel
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_DunnageCSVWriter _csvWriter;

    private const int PAGE_SIZE = 50;

    public Dunnage_EditModeViewModel(
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_DunnageCSVWriter csvWriter,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _dunnageService = dunnageService;
        _paginationService = paginationService;
        _csvWriter = csvWriter;

        // Set default date range (last 7 days)
        ToDate = DateTime.Now;
        FromDate = DateTime.Now.AddDays(-7);
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _filteredLoads = new();

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _selectedLoads = new();

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalRecords = 0;

    [ObservableProperty]
    private bool _canSave = false;

    [ObservableProperty]
    private bool _canNavigate = false;

    private List<Model_DunnageLoad> _allLoads = new();

    #endregion

    #region Load Data Commands

    [RelayCommand]
    private async Task LoadFromHistoryAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading historical data...";

            var result = await _dunnageService.GetLoadsByDateRangeAsync(FromDate, ToDate);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "LoadFromHistoryAsync", true);
                return;
            }

            _allLoads = result.Data;
            TotalRecords = _allLoads.Count;

            // Set pagination source
            _paginationService.SetSource(_allLoads);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            // Load first page
            LoadPage(1);

            CanNavigate = TotalPages > 1;
            StatusMessage = $"Loaded {TotalRecords} records";

            _logger.LogInfo($"Loaded {TotalRecords} historical loads from {FromDate:d} to {ToDate:d}", "EditMode");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading historical data",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SetDateRangeToday()
    {
        FromDate = DateTime.Now.Date;
        ToDate = DateTime.Now.Date;
    }

    [RelayCommand]
    private void SetDateRangeLastWeek()
    {
        ToDate = DateTime.Now.Date;
        FromDate = DateTime.Now.Date.AddDays(-7);
    }

    [RelayCommand]
    private void SetDateRangeLastMonth()
    {
        ToDate = DateTime.Now.Date;
        FromDate = DateTime.Now.Date.AddMonths(-1);
    }

    #endregion

    #region Pagination Commands

    [RelayCommand]
    private void FirstPage()
    {
        if (CurrentPage == 1) return;
        LoadPage(1);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage <= 1) return;
        LoadPage(CurrentPage - 1);
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= TotalPages) return;
        LoadPage(CurrentPage + 1);
    }

    [RelayCommand]
    private void LastPage()
    {
        if (CurrentPage == TotalPages) return;
        LoadPage(TotalPages);
    }

    private void LoadPage(int pageNumber)
    {
        _paginationService.GoToPage(pageNumber);
        CurrentPage = _paginationService.CurrentPage;

        var pageLoads = _paginationService.GetCurrentPageItems<Model_DunnageLoad>();

        FilteredLoads.Clear();
        foreach (var load in pageLoads)
        {
            FilteredLoads.Add(load);
        }

        StatusMessage = $"Page {CurrentPage} of {TotalPages}";
        _logger.LogInfo($"Loaded page {CurrentPage} of {TotalPages}", "EditMode");
    }

    #endregion

    #region Edit Commands

    [RelayCommand]
    private void SelectAll()
    {
        SelectedLoads.Clear();
        foreach (var load in FilteredLoads)
        {
            SelectedLoads.Add(load);
        }

        UpdateCanSave();
        _logger.LogInfo($"Selected all {FilteredLoads.Count} loads on page", "EditMode");
    }

    [RelayCommand]
    private void RemoveSelectedRows()
    {
        if (SelectedLoads.Count == 0) return;

        var loadsToRemove = SelectedLoads.ToList();
        
        foreach (var load in loadsToRemove)
        {
            FilteredLoads.Remove(load);
            _allLoads.Remove(load);
        }

        SelectedLoads.Clear();
        TotalRecords = _allLoads.Count;
        UpdateCanSave();

        _logger.LogInfo($"Removed {loadsToRemove.Count} loads", "EditMode");
    }

    #endregion

    #region Save Commands

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (_allLoads.Count == 0) return;

        try
        {
            IsBusy = true;
            CanSave = false;
            StatusMessage = "Saving changes...";

            // Validate all loads
            foreach (var load in _allLoads)
            {
                if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
                {
                    StatusMessage = "All loads must have Type and Part ID";
                    return;
                }
            }

            // Save to database
            var saveResult = await _dunnageService.SaveLoadsAsync(_allLoads);

            if (!saveResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            // Export to CSV
            var csvResult = await _csvWriter.WriteToCSVAsync(_allLoads);

            if (!csvResult.LocalSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "CSV export failed",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true);
            }

            StatusMessage = $"Successfully saved {_allLoads.Count} loads";
            _logger.LogInfo($"Saved {_allLoads.Count} loads", "EditMode");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving loads",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            CanSave = true;
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateCanSave()
    {
        CanSave = _allLoads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
    }

    #endregion
}
