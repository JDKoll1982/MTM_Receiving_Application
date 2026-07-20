using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Type Selection with a 3x3 paginated grid.
/// </summary>
public partial class ViewModel_dunnage_typeselection : ViewModel_Shared_Base, IResettableViewModel
{
    private const string SortByNameOption = "Name (A-Z)";
    private const string SortByTimesUsedOption = "Times Used";
    private const string SortByLastUsedOption = "Last Used";
    private const string SortByNewestAddedOption = "Newest Added";
    private const string SortByRecentlyUpdatedOption = "Recently Updated";

    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_Help _helpService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private readonly IService_UserPrivileges _userPrivileges;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly List<Model_DunnageType> _allTypes = new();
    private Dictionary<int, int> _typeUsageCounts = new();
    private Dictionary<int, DateTime> _typeLastUsedDates = new();
    private bool _isRestoringSortPreference;

    public ViewModel_dunnage_typeselection(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_Help helpService,
        IService_UserPrivileges userPrivileges,
        IService_UserSessionManager sessionManager,
        IService_DunnageSettings dunnageSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_ViewModelRegistry viewModelRegistry,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _paginationService = paginationService;
        _helpService = helpService;
        _viewModelRegistry = viewModelRegistry;
        _userPrivileges = userPrivileges;
        _sessionManager = sessionManager;
        _dunnageSettings = dunnageSettings;

        // Subscribe to pagination events
        _paginationService.PageChanged += OnPageChanged;

        _viewModelRegistry.Register(this);
    }

    public void ResetToDefaults()
    {
        SelectedType = null;
        _paginationService?.FirstPage();
    }

    public IReadOnlyList<string> SortOptions { get; } =
        new[]
        {
            SortByNameOption,
            SortByTimesUsedOption,
            SortByLastUsedOption,
            SortByNewestAddedOption,
            SortByRecentlyUpdatedOption,
        };

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteTypeCommand))]
    private bool _canManageDefinitions;

    [ObservableProperty]
    private string _selectedSortOption = SortByNameOption;

    #endregion

    partial void OnSelectedSortOptionChanged(string value)
    {
        if (_isRestoringSortPreference)
        {
            return;
        }

        if (_allTypes.Count > 0)
        {
            ApplySorting();
        }

        PersistSortPreference(value);
    }

    #region Initialization

    /// <summary>
    /// Load dunnage types and initialize pagination
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInfo("TypeSelection: InitializeAsync called", "ViewModel_dunnage_typeselection");

        if (IsBusy)
        {
            _logger.LogInfo(
                "TypeSelection: Already busy, returning",
                "ViewModel_dunnage_typeselection"
            );
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading dunnage types...";
            await EnsurePrivilegeStateAsync();
            await LoadSortPreferenceAsync();
            _logger.LogInfo(
                "TypeSelection: Starting to load types",
                "ViewModel_dunnage_typeselection"
            );

            await LoadTypesAsync();

            StatusMessage = $"Loaded {_paginationService.TotalItems} dunnage types";
        }
        catch (Exception ex)
        {
            _errorHandler
                .HandleErrorAsync(
                    "Failed to load dunnage types",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                )
                .Wait();
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
            _logger.LogInfo(
                "TypeSelection: Calling service.GetAllTypesAsync()",
                "ViewModel_dunnage_typeselection"
            );

            Task<Model_Dao_Result<List<Model_DunnageType>>> typesTask =
                _dunnageService.GetAllTypesAsync();
            Task<Model_Dao_Result<List<Model_DunnageLoad>>> loadsTask =
                _dunnageService.GetAllLoadsAsync();

            await Task.WhenAll(typesTask, loadsTask);

            Model_Dao_Result<List<Model_DunnageType>> result = await typesTask;
            Model_Dao_Result<List<Model_DunnageLoad>> loadsResult = await loadsTask;

            _logger.LogInfo(
                $"TypeSelection: Service returned - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}, Count: {result.Data?.Count ?? 0}",
                "ViewModel_dunnage_typeselection"
            );

            if (result.IsSuccess && result.Data != null)
            {
                _allTypes.Clear();
                _allTypes.AddRange(result.Data);
                UpdateSortMetrics(loadsResult);
                ApplySorting();

                _logger.LogInfo(
                    $"TypeSelection: Pagination configured with PageSize=9, TotalItems={result.Data.Count}, Sort={SelectedSortOption}",
                    "ViewModel_dunnage_typeselection"
                );

                _logger.LogInfo(
                    $"TypeSelection: Successfully loaded {result.Data.Count} dunnage types with {TotalPages} pages, DisplayedTypes.Count={DisplayedTypes.Count}",
                    "ViewModel_dunnage_typeselection"
                );
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, nameof(LoadTypesAsync), true);
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

            _logger.LogInfo(
                $"TypeSelection: Selected dunnage type: {type.TypeName} (ID: {type.Id})",
                "TypeSelection"
            );

            // Set in workflow session
            _workflowService.CurrentSession.SelectedType = type;
            _workflowService.CurrentSession.SelectedTypeId = type.Id;
            _workflowService.CurrentSession.SelectedTypeName = type.TypeName;

            _logger.LogInfo(
                $"TypeSelection: Session updated - SelectedTypeId={_workflowService.CurrentSession.SelectedTypeId}",
                "TypeSelection"
            );

            // Navigate to part selection
            _logger.LogInfo("TypeSelection: Navigating to PartSelection step", "TypeSelection");
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"TypeSelection: Error selecting type: {ex.Message}",
                ex,
                "TypeSelection"
            );
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
                XamlRoot = App.MainWindow?.Content?.XamlRoot,
            };

            if (dialog.XamlRoot == null)
            {
                _logger.LogInfo("Cannot show dialog: XamlRoot is null", "TypeSelection");
                return;
            }

            dialog.PrepareDialogSize();

            await dialog.ShowAsync();

            if (dialog.WasAccepted)
            {
                var typeName = dialog.TypeName;
                var iconName = dialog.SelectedIconKind.ToString();

                _logger.LogInfo(
                    $"Adding new type: {typeName} with icon {iconName}",
                    "TypeSelection"
                );

                // Insert new type
                var newType = new Model_DunnageType
                {
                    TypeName = typeName,
                    Icon = iconName,
                    ImagePath = dialog.SelectedImagePath,
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
                            MaxValue = specItem.MaxValue,
                            Choices = specItem.Choices,
                        };

                        var specModel = new Model_DunnageSpec
                        {
                            TypeId = newType.Id,
                            SpecKey = specItem.Name,
                            SpecValue = JsonSerializer.Serialize(specDef),
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
                XamlRoot = App.MainWindow?.Content?.XamlRoot,
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
                            def.DataType = string.IsNullOrWhiteSpace(def.DataType)
                                ? "Text"
                                : def.DataType.Trim();
                            def.Unit ??= string.Empty;
                            def.DefaultValue ??= string.Empty;
                            def.Choices ??= new List<string>();
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

            dialog.InitializeForEdit(
                type.TypeName,
                type.Icon,
                type.ImagePath,
                existingSpecsDict,
                CanManageDefinitions
            );
            dialog.PrepareDialogSize();

            await dialog.ShowAsync();

            if (dialog.RequestDelete)
            {
                await DeleteTypeAsync(type);
                return;
            }

            if (dialog.WasAccepted)
            {
                var originalName = type.TypeName;
                var originalIcon = type.Icon;
                var newName = dialog.TypeName;
                var newIcon = dialog.SelectedIconKind.ToString();
                var newSpecs = dialog.Specs; // Collection of SpecItem

                // Update Type info
                if (
                    newName != type.TypeName
                    || newIcon != type.Icon
                    || dialog.SelectedImagePath != type.ImagePath
                )
                {
                    if (
                        await ConfirmSavedRowRewriteAsync(
                            BuildSavedRowRewriteWarning(
                                originalName,
                                newName,
                                originalIcon,
                                newIcon
                            )
                        ) is false
                        && (newName != originalName || newIcon != originalIcon)
                    )
                    {
                        return;
                    }

                    type.TypeName = newName;
                    type.Icon = newIcon;
                    type.ImagePath = dialog.SelectedImagePath;

                    var updateResult = await _dunnageService.UpdateTypeAsync(type);
                    if (!updateResult.IsSuccess)
                    {
                        await _errorHandler.HandleDaoErrorAsync(
                            updateResult,
                            nameof(EditTypeAsync),
                            true
                        );
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
                        MaxValue = specItem.MaxValue,
                        Choices = specItem.Choices,
                    };
                    var json = JsonSerializer.Serialize(specDef);

                    if (existingSpecsDict.ContainsKey(specItem.Name))
                    {
                        // Update existing?
                        // We need to check if definition changed.
                        // For simplicity, we can just update the value if it's different.
                        var existingModel = specsResult.Data?.FirstOrDefault(s =>
                            s.SpecKey == specItem.Name
                        );
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
                            SpecValue = json,
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
            await _errorHandler.HandleErrorAsync(
                "Error updating dunnage type",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    [RelayCommand(CanExecute = nameof(CanManageDefinitions))]
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
                Content =
                    $"Are you sure you want to delete '{type.TypeName}'? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                dialog,
                dialog.XamlRoot
            );

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
                    await _errorHandler.HandleDaoErrorAsync(
                        deleteResult,
                        nameof(DeleteTypeAsync),
                        true
                    );
                }
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error deleting dunnage type",
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

    private async Task<bool> ConfirmSavedRowRewriteAsync(string? warningMessage)
    {
        if (string.IsNullOrWhiteSpace(warningMessage))
        {
            return true;
        }

        var xamlRoot = App.MainWindow?.Content?.XamlRoot;
        if (xamlRoot == null)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = "Update Saved Dunnage Rows",
            Content = warningMessage,
            PrimaryButtonText = "Continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            dialog.XamlRoot
        );

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    internal static string? BuildSavedRowRewriteWarning(
        string originalName,
        string newName,
        string originalIcon,
        string newIcon
    )
    {
        var changedValues = new List<string>();

        if (string.Equals(originalName, newName, StringComparison.Ordinal) is false)
        {
            changedValues.Add($"type name from '{originalName}' to '{newName}'");
        }

        if (string.Equals(originalIcon, newIcon, StringComparison.Ordinal) is false)
        {
            changedValues.Add($"type icon from '{originalIcon}' to '{newIcon}'");
        }

        if (changedValues.Count == 0)
        {
            return null;
        }

        var changeSummary = string.Join(" and ", changedValues);
        return $"This edit will also change all pre-existing Dunnage current label data and history rows that use this saved value. Continue updating the {changeSummary}?";
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
        _logger.LogInfo(
            $"TypeSelection: UpdatePageDisplay - Got {currentItems.Count()} items from pagination service",
            "ViewModel_dunnage_typeselection"
        );

        var currentItemList = currentItems.ToList();
        DisplayedTypes = new ObservableCollection<Model_DunnageType>(currentItemList);

        foreach (var type in currentItemList)
        {
            _logger.LogInfo(
                $"TypeSelection: Added type to DisplayedTypes - ID: {type.Id}, Name: {type.TypeName}",
                "ViewModel_dunnage_typeselection"
            );
        }

        _logger.LogInfo(
            $"TypeSelection: DisplayedTypes.Count after update: {DisplayedTypes.Count}",
            "ViewModel_dunnage_typeselection"
        );
    }

    private void ApplySorting()
    {
        IEnumerable<Model_DunnageType> sortedTypes = SelectedSortOption switch
        {
            SortByTimesUsedOption => _allTypes
                .OrderByDescending(GetUsageCount)
                .ThenByDescending(GetLastUsedDate)
                .ThenByDescending(GetRecentlyUpdatedDate)
                .ThenBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase),
            SortByLastUsedOption => _allTypes
                .OrderByDescending(GetLastUsedDate)
                .ThenByDescending(GetUsageCount)
                .ThenByDescending(GetRecentlyUpdatedDate)
                .ThenBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase),
            SortByNewestAddedOption => _allTypes
                .OrderByDescending(type => type.CreatedDate)
                .ThenBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase),
            SortByRecentlyUpdatedOption => _allTypes
                .OrderByDescending(GetRecentlyUpdatedDate)
                .ThenBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase),
            _ => _allTypes.OrderBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase),
        };

        _paginationService.PageSize = 9;
        _paginationService.SetSource(sortedTypes.ToList());
        _paginationService.FirstPage();
        UpdatePaginationProperties();
        UpdatePageDisplay();
    }

    private void UpdateSortMetrics(Model_Dao_Result<List<Model_DunnageLoad>> loadsResult)
    {
        _typeUsageCounts = new Dictionary<int, int>();
        _typeLastUsedDates = new Dictionary<int, DateTime>();

        if (loadsResult.IsSuccess is false || loadsResult.Data == null)
        {
            _logger.LogWarning(
                $"TypeSelection: Unable to load history metrics for sorting: {loadsResult.ErrorMessage}",
                "ViewModel_dunnage_typeselection"
            );
            return;
        }

        IEnumerable<IGrouping<int, Model_DunnageLoad>> loadsByType = loadsResult
            .Data.Where(load => load.TypeId.HasValue)
            .GroupBy(load => load.TypeId!.Value);

        foreach (IGrouping<int, Model_DunnageLoad> loadGroup in loadsByType)
        {
            _typeUsageCounts[loadGroup.Key] = loadGroup.Count();
            _typeLastUsedDates[loadGroup.Key] = loadGroup.Max(GetLoadUsedOn);
        }
    }

    private int GetUsageCount(Model_DunnageType type)
    {
        return _typeUsageCounts.GetValueOrDefault(type.Id);
    }

    private async Task LoadSortPreferenceAsync()
    {
        _isRestoringSortPreference = true;

        try
        {
            string persistedValue = await _dunnageSettings.GetStringAsync(
                DunnageSettingsKeys.UserPreferences.TypeSelectionSort,
                GetCurrentUserId()
            );

            SelectedSortOption = SortOptions.Contains(persistedValue, StringComparer.Ordinal)
                ? persistedValue
                : SortByNameOption;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"TypeSelection: Failed to load saved sort option. Falling back to {SortByNameOption}. Error: {ex.Message}",
                "ViewModel_dunnage_typeselection"
            );
            SelectedSortOption = SortByNameOption;
        }
        finally
        {
            _isRestoringSortPreference = false;
        }
    }

    private void PersistSortPreference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        try
        {
            Task.Run(() =>
                    _dunnageSettings.SaveStringAsync(
                        DunnageSettingsKeys.UserPreferences.TypeSelectionSort,
                        value,
                        GetCurrentUserId()
                    )
                )
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"TypeSelection: Failed to save sort option '{value}'. Error: {ex.Message}",
                "ViewModel_dunnage_typeselection"
            );
        }
    }

    private int? GetCurrentUserId()
    {
        int? employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        return employeeNumber.HasValue && employeeNumber.Value > 0 ? employeeNumber : null;
    }

    private DateTime GetLastUsedDate(Model_DunnageType type)
    {
        return _typeLastUsedDates.GetValueOrDefault(type.Id, DateTime.MinValue);
    }

    private static DateTime GetRecentlyUpdatedDate(Model_DunnageType type)
    {
        return type.ModifiedDate ?? type.CreatedDate;
    }

    private static DateTime GetLoadUsedOn(Model_DunnageLoad load)
    {
        return load.ReceivedDate == default ? load.CreatedDate : load.ReceivedDate;
    }

    private async Task EnsurePrivilegeStateAsync()
    {
        int? employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        if (
            employeeNumber.HasValue
            && employeeNumber.Value > 0
            && (!_userPrivileges.IsInitialized || _userPrivileges.CurrentUserId != employeeNumber)
        )
        {
            Model_Dao_Result initializeResult = await _userPrivileges.InitializeAsync(
                employeeNumber.Value
            );
            if (!initializeResult.IsSuccess)
            {
                _logger.LogWarning(
                    $"TypeSelection: Failed to initialize privileges: {initializeResult.ErrorMessage}",
                    "ViewModel_dunnage_typeselection"
                );
                CanManageDefinitions = false;
                return;
            }
        }

        CanManageDefinitions = _userPrivileges.HasAnyRole("Admin", "Developer");
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
