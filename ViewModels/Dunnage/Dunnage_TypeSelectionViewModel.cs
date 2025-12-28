using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Type Selection with 3x3 paginated grid
/// </summary>
public partial class Dunnage_TypeSelectionViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;

    public Dunnage_TypeSelectionViewModel(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _paginationService = paginationService;

        // Subscribe to pagination events
        _paginationService.PageChanged += OnPageChanged;
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _displayedTypes = new();

    [ObservableProperty]
    private Model_DunnageType? _selectedType;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private bool _canGoPrevious;

    [ObservableProperty]
    private string _pageInfo = "Page 1 of 1";

    #endregion

    #region Initialization

    /// <summary>
    /// Load dunnage types and initialize pagination
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInfo("TypeSelection: InitializeAsync called", "Dunnage_TypeSelectionViewModel");

        if (IsBusy)
        {
            _logger.LogInfo("TypeSelection: Already busy, returning", "Dunnage_TypeSelectionViewModel");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading dunnage types...";
            _logger.LogInfo("TypeSelection: Starting to load types", "Dunnage_TypeSelectionViewModel");

            await LoadTypesAsync();

            StatusMessage = $"Loaded {_paginationService.TotalItems} dunnage types";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleErrorAsync(
                "Failed to load dunnage types",
                Enum_ErrorSeverity.Error,
                ex,
                true
            ).Wait();
            StatusMessage = "Error loading types";
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Load Data Commands

    [RelayCommand]
    private async Task LoadTypesAsync()
    {
        _logger.LogInfo("TypeSelection: LoadTypesAsync called", "Dunnage_TypeSelectionViewModel");

        try
        {
            _logger.LogInfo("TypeSelection: Calling service.GetAllTypesAsync()", "Dunnage_TypeSelectionViewModel");

            var result = await _dunnageService.GetAllTypesAsync();

            _logger.LogInfo($"TypeSelection: Service returned - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}, Count: {result.Data?.Count ?? 0}", "Dunnage_TypeSelectionViewModel");

            if (result.IsSuccess && result.Data != null)
            {
                // Configure pagination for 3x3 grid (9 items per page)
                _paginationService.PageSize = 9;
                _paginationService.SetSource(result.Data);
                _logger.LogInfo($"TypeSelection: Pagination configured with PageSize=9, TotalItems={result.Data.Count}", "Dunnage_TypeSelectionViewModel");

                UpdatePaginationProperties();
                UpdatePageDisplay();

                _logger.LogInfo($"TypeSelection: Successfully loaded {result.Data.Count} dunnage types with {TotalPages} pages, DisplayedTypes.Count={DisplayedTypes.Count}", "Dunnage_TypeSelectionViewModel");
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(
                    result,
                    nameof(LoadTypesAsync),
                    true
                );
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading dunnage types",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    #endregion

    #region Pagination Commands

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextPage()
    {
        _paginationService.NextPage();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousPage()
    {
        _paginationService.PreviousPage();
    }

    [RelayCommand]
    private void FirstPage()
    {
        _paginationService.FirstPage();
    }

    [RelayCommand]
    private void LastPage()
    {
        _paginationService.LastPage();
    }

    #endregion

    #region Type Selection Commands

    [RelayCommand]
    private async Task SelectTypeAsync(Model_DunnageType? type)
    {
        if (type == null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SelectedType = type;

            _logger.LogInfo($"Selected dunnage type: {type.TypeName} (ID: {type.Id})");

            // Set in workflow session
            _workflowService.CurrentSession.SelectedTypeId = type.Id;
            _workflowService.CurrentSession.SelectedTypeName = type.TypeName;

            // Navigate to part selection
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error selecting dunnage type",
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
    private async Task QuickAddTypeAsync()
    {
        try
        {
            _logger.LogInfo("Quick Add Type requested", "TypeSelection");

            // Show dialog
            var dialog = new Views.Dunnage.Dunnage_QuickAddTypeDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            if (dialog.XamlRoot == null)
            {
                _logger.LogInfo("Cannot show dialog: XamlRoot is null", "TypeSelection");
                return;
            }

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var typeName = dialog.TypeName;
                var iconGlyph = dialog.SelectedIconGlyph;

                _logger.LogInfo($"Adding new type: {typeName} with icon {iconGlyph}", "TypeSelection");

                // Insert new type
                var newType = new Model_DunnageType
                {
                    TypeName = typeName
                    // Note: Icon storage would need to be added to database schema
                };

                var insertResult = await _dunnageService.InsertTypeAsync(newType);

                if (insertResult.IsSuccess)
                {
                    _logger.LogInfo($"Successfully added type: {typeName}", "TypeSelection");

                    // Reload types to show new type
                    await LoadTypesAsync();

                    StatusMessage = $"Added new type: {typeName}";
                }
                else
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        insertResult,
                        nameof(QuickAddTypeAsync),
                        true
                    );
                }
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error adding new dunnage type",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    #endregion

    #region Helper Methods

    private void OnPageChanged(object? sender, EventArgs e)
    {
        UpdatePaginationProperties();
        UpdatePageDisplay();
    }

    private void UpdatePaginationProperties()
    {
        CurrentPage = _paginationService.CurrentPage;
        TotalPages = _paginationService.TotalPages;
        CanGoNext = _paginationService.HasNextPage;
        CanGoPrevious = _paginationService.HasPreviousPage;
        PageInfo = $"Page {CurrentPage} of {TotalPages}";

        // Notify commands to update their CanExecute state
        NextPageCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePageDisplay()
    {
        var currentItems = _paginationService.GetCurrentPageItems<Model_DunnageType>();
        _logger.LogInfo($"TypeSelection: UpdatePageDisplay - Got {currentItems.Count()} items from pagination service", "Dunnage_TypeSelectionViewModel");

        DisplayedTypes.Clear();
        foreach (var type in currentItems)
        {
            DisplayedTypes.Add(type);
            _logger.LogInfo($"TypeSelection: Added type to DisplayedTypes - ID: {type.Id}, Name: {type.TypeName}", "Dunnage_TypeSelectionViewModel");
        }

        _logger.LogInfo($"TypeSelection: DisplayedTypes.Count after update: {DisplayedTypes.Count}", "Dunnage_TypeSelectionViewModel");
    }

    #endregion
}
