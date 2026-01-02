using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Part Management
/// Handles CRUD operations with pagination, filtering, and search
/// </summary>
public partial class Dunnage_AdminPartsViewModel : Shared_BaseViewModel
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageAdminWorkflow _adminWorkflow;
    private readonly IService_Pagination _paginationService;
    private readonly IService_Window _windowService;
    private readonly IService_Help _helpService;

    private List<Model_DunnagePart> _allParts = new();
    private const int PAGE_SIZE = 20;

    public Dunnage_AdminPartsViewModel(
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageAdminWorkflow adminWorkflow,
        IService_Pagination paginationService,
        IService_Window windowService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _dunnageService = dunnageService;
        _adminWorkflow = adminWorkflow;
        _paginationService = paginationService;
        _windowService = windowService;
        _helpService = helpService;

        // Configure pagination
        _paginationService.PageChanged += OnPageChanged;
    }

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    #endregion

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnagePart> _parts = new();

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _availableTypes = new();

    [ObservableProperty]
    private Model_DunnagePart? _selectedPart;

    [ObservableProperty]
    private Model_DunnageType? _selectedFilterType;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalRecords = 0;

    [ObservableProperty]
    private bool _canEdit = false;

    [ObservableProperty]
    private bool _canDelete = false;

    [ObservableProperty]
    private bool _canNavigatePrevious = false;

    [ObservableProperty]
    private bool _canNavigateNext = false;

    #endregion

    #region Load Data

    /// <summary>
    /// Load all parts and types
    /// </summary>
    [RelayCommand]
    private async Task LoadPartsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading parts...";

            // Load types for filter dropdown
            var typesResult = await _dunnageService.GetAllTypesAsync();
            if (typesResult.Success && typesResult.Data != null)
            {
                AvailableTypes.Clear();
                foreach (var type in typesResult.Data)
                {
                    AvailableTypes.Add(type);
                }
            }

            // Load all parts
            var partsResult = await _dunnageService.GetAllPartsAsync();

            if (!partsResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(partsResult, "LoadPartsAsync", true);
                return;
            }

            _allParts = partsResult.Data ?? new List<Model_DunnagePart>();
            TotalRecords = _allParts.Count;

            // Setup pagination
            _paginationService.SetSource(_allParts);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            // Load first page
            LoadPage();

            UpdateNavigationButtons();
            StatusMessage = $"Loaded {TotalRecords} parts";
            await _logger.LogInfoAsync($"Loaded {TotalRecords} dunnage parts", "PartManagement");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading parts",
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

    #endregion

    #region Filtering and Search

    /// <summary>
    /// Filter parts by selected type
    /// </summary>
    [RelayCommand]
    private async Task FilterByTypeAsync()
    {
        if (SelectedFilterType == null)
        {
            await LoadPartsAsync();
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Filtering by {SelectedFilterType.DunnageType}...";

            var result = await _dunnageService.GetPartsByTypeAsync(SelectedFilterType.Id);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "FilterByTypeAsync", true);
                return;
            }

            _allParts = result.Data ?? new List<Model_DunnagePart>();
            TotalRecords = _allParts.Count;

            _paginationService.SetSource(_allParts);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            LoadPage();
            UpdateNavigationButtons();

            StatusMessage = $"Filtered {TotalRecords} parts";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error filtering parts",
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

    /// <summary>
    /// Search parts by text
    /// </summary>
    [RelayCommand]
    private async Task SearchPartsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadPartsAsync();
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Searching...";

            var result = await _dunnageService.SearchPartsAsync(SearchText);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "SearchPartsAsync", true);
                return;
            }

            _allParts = result.Data ?? new List<Model_DunnagePart>();
            TotalRecords = _allParts.Count;

            _paginationService.SetSource(_allParts);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            LoadPage();
            UpdateNavigationButtons();

            StatusMessage = $"Found {TotalRecords} parts";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error searching parts",
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

    /// <summary>
    /// Clear all filters
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SelectedFilterType = null;
        SearchText = string.Empty;
        await LoadPartsAsync();
    }

    #endregion

    #region Pagination

    [RelayCommand]
    private void NextPage()
    {
        if (_paginationService.NextPage())
        {
            LoadPage();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (_paginationService.PreviousPage())
        {
            LoadPage();
        }
    }

    [RelayCommand]
    private void FirstPage()
    {
        if (_paginationService.FirstPage())
        {
            LoadPage();
        }
    }

    [RelayCommand]
    private void LastPage()
    {
        if (_paginationService.LastPage())
        {
            LoadPage();
        }
    }

    private void LoadPage()
    {
        var pageItems = _paginationService.GetCurrentPageItems<Model_DunnagePart>();

        Parts.Clear();
        foreach (var item in pageItems)
        {
            Parts.Add(item);
        }

        CurrentPage = _paginationService.CurrentPage;
        TotalPages = _paginationService.TotalPages;
        UpdateNavigationButtons();
    }

    private void OnPageChanged(object? sender, EventArgs e)
    {
        LoadPage();
    }

    private void UpdateNavigationButtons()
    {
        CanNavigatePrevious = _paginationService.HasPreviousPage;
        CanNavigateNext = _paginationService.HasNextPage;
    }

    #endregion

    #region Delete Part

    /// <summary>
    /// Show delete confirmation with transaction count
    /// </summary>
    [RelayCommand]
    private async Task ShowDeleteConfirmationAsync()
    {
        if (SelectedPart == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Checking usage...";

            var countResult = await _dunnageService.GetTransactionCountByPartAsync(SelectedPart.PartId);

            if (!countResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(countResult, "GetTransactionCountByPartAsync", true);
                return;
            }

            int transactionCount = countResult.Data;
            IsBusy = false;

            string message = $"Part ID: {SelectedPart.PartId}\n\n";
            message += $"Transactions using this part: {transactionCount}\n\n";

            if (transactionCount > 0)
            {
                message += "⚠️ WARNING: This part has transaction history and cannot be deleted.";

                var warningDialog = new ContentDialog
                {
                    Title = "Cannot Delete Part",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = _windowService.GetXamlRoot()
                };

                await warningDialog.ShowAsync();
                return;
            }

            message += "This part has no transaction history and can be deleted.\n\n";
            message += "Are you sure you want to delete this part?";

            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = message,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = _windowService.GetXamlRoot()
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DeletePartAsync();
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error checking part usage",
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

    private async Task DeletePartAsync()
    {
        if (SelectedPart == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Deleting part...";

            var result = await _dunnageService.DeletePartAsync(SelectedPart.PartId);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "DeletePartAsync", true);
                return;
            }

            StatusMessage = "Part deleted successfully";
            await _logger.LogInfoAsync($"Deleted part: {SelectedPart.PartId}", "PartManagement");

            // Reload parts
            await LoadPartsAsync();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error deleting part",
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

    #endregion

    #region Navigation

    [RelayCommand]
    private async Task ReturnToAdminHubAsync()
    {
        try
        {
            await _adminWorkflow.NavigateToHubAsync();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating back",
                Enum_ErrorSeverity.Low,
                ex,
                false
            );
        }
    }

    #endregion

    #region Property Changed

    partial void OnSelectedPartChanged(Model_DunnagePart? value)
    {
        CanEdit = value != null;
        CanDelete = value != null;
    }

    #endregion
}
