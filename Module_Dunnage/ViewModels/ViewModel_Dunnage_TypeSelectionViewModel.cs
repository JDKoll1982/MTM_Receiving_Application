using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Type Selection with 3x3 paginated grid
/// </summary>
public partial class ViewModel_dunnage_typeselection : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_Help _helpService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;

    public ViewModel_dunnage_typeselection(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_ViewModelRegistry viewModelRegistry,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _paginationService = paginationService;
        _helpService = helpService;
        _viewModelRegistry = viewModelRegistry;

        // Subscribe to pagination events
        _paginationService.PageChanged += OnPageChanged;

        _viewModelRegistry.Register(this);
    }

    public void ResetToDefaults()
    {
        SelectedType = null;
        _paginationService?.FirstPage();
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
        _logger.LogInfo("TypeSelection: InitializeAsync called", "ViewModel_dunnage_typeselection");

        if (IsBusy)
        {
            _logger.LogInfo("TypeSelection: Already busy, returning", "ViewModel_dunnage_typeselection");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading dunnage types...";
            _logger.LogInfo("TypeSelection: Starting to load types", "ViewModel_dunnage_typeselection");

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
        _logger.LogInfo("TypeSelection: LoadTypesAsync called", "ViewModel_dunnage_typeselection");

        try
        {
            _logger.LogInfo("TypeSelection: Calling service.GetAllTypesAsync()", "ViewModel_dunnage_typeselection");

            var result = await _dunnageService.GetAllTypesAsync();

            _logger.LogInfo($"TypeSelection: Service returned - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}, Count: {result.Data?.Count ?? 0}", "ViewModel_dunnage_typeselection");

            if (result.IsSuccess && result.Data != null)
            {
                // Configure pagination for 3x3 grid (9 items per page)
                _paginationService.PageSize = 9;
                _paginationService.SetSource(result.Data);
                _logger.LogInfo($"TypeSelection: Pagination configured with PageSize=9, TotalItems={result.Data.Count}", "ViewModel_dunnage_typeselection");

                UpdatePaginationProperties();
                UpdatePageDisplay();

                _logger.LogInfo($"TypeSelection: Successfully loaded {result.Data.Count} dunnage types with {TotalPages} pages, DisplayedTypes.Count={DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
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

            _logger.LogInfo($"TypeSelection: Selected dunnage type: {type.TypeName} (ID: {type.Id})", "TypeSelection");

            // Set in workflow session
            _workflowService.CurrentSession.SelectedType = type;
            _workflowService.CurrentSession.SelectedTypeId = type.Id;
            _workflowService.CurrentSession.SelectedTypeName = type.TypeName;

            _logger.LogInfo($"TypeSelection: Session updated - SelectedTypeId={_workflowService.CurrentSession.SelectedTypeId}", "TypeSelection");

            // Navigate to part selection
            _logger.LogInfo("TypeSelection: Navigating to PartSelection step", "TypeSelection");
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
        }
        catch (Exception ex)
        {
            _logger.LogError($"TypeSelection: Error selecting type: {ex.Message}", ex, "TypeSelection");
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
            var dialog = new Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog
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
                var iconName = dialog.SelectedIconKind.ToString();

                _logger.LogInfo($"Adding new type: {typeName} with icon {iconName}", "TypeSelection");

                // Insert new type
                var newType = new Model_DunnageType
                {
                    TypeName = typeName,
                    Icon = iconName
                    // Note: Icon storage would need to be added to database schema
                };

                var insertResult = await _dunnageService.InsertTypeAsync(newType);

                if (insertResult.IsSuccess)
                {
                    _logger.LogInfo($"Successfully added type: {typeName}", "TypeSelection");

                    // Insert specs
                    foreach (var specItem in dialog.Specs)
                    {
                        var specDef = new SpecDefinition
                        {
                            DataType = specItem.DataType,
                            Required = specItem.IsRequired,
                            Unit = specItem.Unit,
                            MinValue = specItem.MinValue,
                            MaxValue = specItem.MaxValue
                        };

                        var specModel = new Model_DunnageSpec
                        {
                            TypeId = newType.Id,
                            SpecKey = specItem.Name,
                            SpecValue = JsonSerializer.Serialize(specDef)
                        };
                        await _dunnageService.InsertSpecAsync(specModel);
                    }

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

    [RelayCommand]
    private async Task EditTypeAsync(Model_DunnageType type)
    {
        if (type == null)
        {
            return;
        }

        try
        {
            _logger.LogInfo($"Edit Type requested for {type.TypeName}", "TypeSelection");

            var dialog = new Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            if (dialog.XamlRoot == null)
            {
                return;
            }

            // Load existing specs
            var specsResult = await _dunnageService.GetSpecsForTypeAsync(type.Id);
            var existingSpecsDict = new Dictionary<string, SpecDefinition>();

            if (specsResult.IsSuccess && specsResult.Data != null)
            {
                foreach (var s in specsResult.Data)
                {
                    try
                    {
                        var def = JsonSerializer.Deserialize<SpecDefinition>(s.SpecValue);
                        if (def != null)
                        {
                            existingSpecsDict[s.SpecKey] = def;
                        }
                        else
                        {
                            existingSpecsDict[s.SpecKey] = new SpecDefinition(); // Fallback
                        }
                    }
                    catch
                    {
                        existingSpecsDict[s.SpecKey] = new SpecDefinition(); // Fallback for empty/invalid JSON
                    }
                }
            }

            dialog.InitializeForEdit(type.TypeName, type.Icon, existingSpecsDict);

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var newName = dialog.TypeName;
                var newIcon = dialog.SelectedIconKind.ToString();
                var newSpecs = dialog.Specs; // Collection of SpecItem

                // Update Type info
                if (newName != type.TypeName || newIcon != type.Icon)
                {
                    type.TypeName = newName;
                    type.Icon = newIcon;

                    var updateResult = await _dunnageService.UpdateTypeAsync(type);
                    if (!updateResult.IsSuccess)
                    {
                        await _errorHandler.HandleDaoErrorAsync(updateResult, nameof(EditTypeAsync), true);
                        return;
                    }
                }

                // Update Specs
                // 1. Find removed specs
                var newSpecNames = newSpecs.Select(s => s.Name).ToList();
                var removedSpecKeys = existingSpecsDict.Keys.Except(newSpecNames).ToList();

                foreach (var specKey in removedSpecKeys)
                {
                    var specToDelete = specsResult.Data?.FirstOrDefault(s => s.SpecKey == specKey);
                    if (specToDelete != null)
                    {
                        await _dunnageService.DeleteSpecAsync(specToDelete.Id);
                    }
                }

                // 2. Find added or updated specs
                foreach (var specItem in newSpecs)
                {
                    var specDef = new SpecDefinition
                    {
                        DataType = specItem.DataType,
                        Required = specItem.IsRequired,
                        Unit = specItem.Unit,
                        MinValue = specItem.MinValue,
                        MaxValue = specItem.MaxValue
                    };
                    var json = JsonSerializer.Serialize(specDef);

                    if (existingSpecsDict.ContainsKey(specItem.Name))
                    {
                        // Update existing?
                        // We need to check if definition changed.
                        // For simplicity, we can just update the value if it's different.
                        var existingModel = specsResult.Data?.FirstOrDefault(s => s.SpecKey == specItem.Name);
                        if (existingModel != null && existingModel.SpecValue != json)
                        {
                            existingModel.SpecValue = json;
                            await _dunnageService.UpdateSpecAsync(existingModel);
                        }
                    }
                    else
                    {
                        // Insert new
                        var specModel = new Model_DunnageSpec
                        {
                            TypeId = type.Id,
                            SpecKey = specItem.Name,
                            SpecValue = json
                        };
                        await _dunnageService.InsertSpecAsync(specModel);
                    }
                }

                _logger.LogInfo($"Successfully updated type: {type.TypeName}", "TypeSelection");
                await LoadTypesAsync();
                StatusMessage = $"Updated type: {type.TypeName}";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error updating dunnage type", Enum_ErrorSeverity.Error, ex, true);
        }
    }

    [RelayCommand]
    private async Task DeleteTypeAsync(Model_DunnageType type)
    {
        if (type == null)
        {
            return;
        }

        try
        {
            _logger.LogInfo($"Delete Type requested for {type.TypeName}", "TypeSelection");

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot,
                Title = "Delete Dunnage Type",
                Content = $"Are you sure you want to delete '{type.TypeName}'? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var deleteResult = await _dunnageService.DeleteTypeAsync(type.Id);

                if (deleteResult.IsSuccess)
                {
                    _logger.LogInfo($"Successfully deleted type: {type.TypeName}", "TypeSelection");
                    await LoadTypesAsync();
                    StatusMessage = $"Deleted type: {type.TypeName}";
                }
                else
                {
                    await _errorHandler.HandleDaoErrorAsync(deleteResult, nameof(DeleteTypeAsync), true);
                }
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error deleting dunnage type", Enum_ErrorSeverity.Error, ex, true);
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
        _logger.LogInfo($"TypeSelection: UpdatePageDisplay - Got {currentItems.Count()} items from pagination service", "ViewModel_dunnage_typeselection");

        DisplayedTypes.Clear();
        foreach (var type in currentItems)
        {
            DisplayedTypes.Add(type);
            _logger.LogInfo($"TypeSelection: Added type to DisplayedTypes - ID: {type.Id}, Name: {type.TypeName}", "ViewModel_dunnage_typeselection");
        }

        _logger.LogInfo($"TypeSelection: DisplayedTypes.Count after update: {DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
    }

    #endregion

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    /// <summary>
    /// Gets a placeholder by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    /// <summary>
    /// Gets a tip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTip(string key) => _helpService.GetTip(key);

    #endregion
}

