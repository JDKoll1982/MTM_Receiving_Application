using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Part Selection with inventory notification
/// </summary>
public partial class ViewModel_Dunnage_PartSelection : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Help _helpService;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IService_UserPrivileges _userPrivileges;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private readonly IService_DunnageSettings _dunnageSettings;

    private bool _isRestoringDisplayFormatPreference;
    private bool _persistedImageDisplayPreference = true;

    public ViewModel_Dunnage_PartSelection(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_UserPrivileges userPrivileges,
        IService_UserSessionManager sessionManager,
        IService_DunnageSettings dunnageSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _dispatcher = dispatcher;
        _helpService = helpService;
        _viewModelRegistry = viewModelRegistry;
        _userPrivileges = userPrivileges;
        _sessionManager = sessionManager;
        _dunnageSettings = dunnageSettings;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _viewModelRegistry.Register(this);
        _logger.LogInfo(
            "PartSelection: ViewModel constructed and subscribed to StepChanged",
            "PartSelection"
        );
    }

    public void ResetToDefaults()
    {
        SelectedPart = null;
        AvailableParts = new ObservableCollection<Model_DunnagePart>();
        SelectedTypeName = string.Empty;
        SelectedTypeIcon = "Help";
        SelectedTypeImagePath = null;
        SelectedTypeId = 0;
        SelectedPartSpecSummaries = new ObservableCollection<string>();
        HasSelectedPartSpecs = false;
        IsInventoryNotificationVisible = false;
        InventoryMethod = "Adjust In";
        StatusMessage = string.Empty;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        _logger.LogInfo(
            $"PartSelection: Workflow step changed to {_workflowService.CurrentStep}",
            "PartSelection"
        );
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.PartSelection)
        {
            _logger.LogInfo(
                "PartSelection: Step is PartSelection, calling InitializeAsync via dispatcher",
                "PartSelection"
            );
            _dispatcher.TryEnqueue(async () =>
            {
                await InitializeAsync();
            });
        }
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnagePart> _availableParts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPartSelected))]
    [NotifyPropertyChangedFor(nameof(CanDeleteSelectedPart))]
    private Model_DunnagePart? _selectedPart;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeletePartCommand))]
    [NotifyPropertyChangedFor(nameof(CanDeleteSelectedPart))]
    private bool _canManageDefinitions;

    [ObservableProperty]
    private bool _isInventoryNotificationVisible;

    [ObservableProperty]
    private string _inventoryMethod = "Adjust In";

    [ObservableProperty]
    private string _selectedTypeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTypeIconKind))]
    private string _selectedTypeIcon = "Help";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTypeImage))]
    [NotifyPropertyChangedFor(nameof(SelectedTypeImageSource))]
    private string? _selectedTypeImagePath;

    /// <summary>
    /// Gets the MaterialIconKind for the selected type
    /// </summary>
    public MaterialIconKind SelectedTypeIconKind
    {
        get
        {
            if (
                !string.IsNullOrEmpty(SelectedTypeIcon)
                && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind)
            )
            {
                return kind;
            }
            return MaterialIconKind.PackageVariantClosed;
        }
    }

    [ObservableProperty]
    private int _selectedTypeId;

    [ObservableProperty]
    private ObservableCollection<string> _selectedPartSpecSummaries = new();

    [ObservableProperty]
    private bool _hasSelectedPartSpecs;

    [ObservableProperty]
    private bool _isImageDisplayPreferred;

    /// <summary>
    /// Helper property for UI binding
    /// </summary>
    public bool IsPartSelected => SelectedPart != null;

    public bool CanDeleteSelectedPart => CanManageDefinitions && IsPartSelected;

    public bool HasSelectedTypeImage => SelectedTypeImageSource is not null;

    public ImageSource? SelectedTypeImageSource =>
        Helper_DunnageImagePaths.CreateImageSource(SelectedTypeImagePath);

    public bool HasSelectedPartVisual => SelectedPartVisualSource is not null;

    public ImageSource? SelectedPartVisualSource =>
        SelectedPart?.ImageSource
        ?? SelectedPart?.DunnageTypeImageSource
        ?? SelectedTypeImageSource;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize and load parts for selected type
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInfo("PartSelection: InitializeAsync called", "PartSelection");
        if (IsBusy)
        {
            _logger.LogInfo(
                "PartSelection: InitializeAsync returning because IsBusy is true",
                "PartSelection"
            );
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading parts...";

            await EnsurePrivilegeStateAsync();
            await LoadDisplayPreferenceAsync();

            // Get selected type from workflow
            SelectedTypeId = _workflowService.CurrentSession.SelectedTypeId;
            SelectedTypeName = _workflowService.CurrentSession.SelectedTypeName ?? string.Empty;
            SelectedTypeIcon = _workflowService.CurrentSession.SelectedType?.Icon ?? "Help";
            SelectedTypeImagePath = _workflowService.CurrentSession.SelectedType?.ImagePath;

            _logger.LogInfo(
                $"PartSelection: SelectedTypeId={SelectedTypeId}, SelectedTypeName={SelectedTypeName}, SelectedTypeIcon={SelectedTypeIcon}",
                "PartSelection"
            );

            await LoadPartsAsync();

            RestoreWorkflowSelectedPart();

            StatusMessage = $"Loaded {AvailableParts.Count} parts for {SelectedTypeName}";
            _logger.LogInfo($"PartSelection: {StatusMessage}", "PartSelection");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"PartSelection: Failed to initialize: {ex.Message}",
                ex,
                "PartSelection"
            );
            await _errorHandler.HandleErrorAsync(
                "Failed to initialize part selection",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
            StatusMessage = "Error loading parts";
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Display Format Preference

    public async Task HandleDisplayFormatChangedAsync(bool isImageDisplayPreferred)
    {
        if (_isRestoringDisplayFormatPreference)
        {
            return;
        }

        if (IsImageDisplayPreferred != isImageDisplayPreferred)
        {
            _isRestoringDisplayFormatPreference = true;
            IsImageDisplayPreferred = isImageDisplayPreferred;
            _isRestoringDisplayFormatPreference = false;
        }

        if (isImageDisplayPreferred == _persistedImageDisplayPreference)
        {
            return;
        }

        await SetDisplayFormatPreferenceAsync(isImageDisplayPreferred);
    }

    private async Task LoadDisplayPreferenceAsync()
    {
        _isRestoringDisplayFormatPreference = true;

        try
        {
            var savedPreference = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.UserPreferences.PreferPartImages,
                GetCurrentUserId()
            );

            _persistedImageDisplayPreference = savedPreference;
            IsImageDisplayPreferred = savedPreference;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"PartSelection: Failed to load saved display preference. Falling back to image cards. Error: {ex.Message}",
                "PartSelection"
            );

            _persistedImageDisplayPreference = true;
            IsImageDisplayPreferred = true;
        }
        finally
        {
            _isRestoringDisplayFormatPreference = false;
        }
    }

    private async Task SetDisplayFormatPreferenceAsync(bool isImageDisplayPreferred)
    {
        try
        {
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UserPreferences.PreferPartImages,
                isImageDisplayPreferred ? "true" : "false",
                GetCurrentUserId()
            );

            _persistedImageDisplayPreference = isImageDisplayPreferred;
            StatusMessage = isImageDisplayPreferred
                ? "Part selection display set to image cards"
                : "Part selection display set to drop-down";

            _logger.LogInfo(
                $"PartSelection: Display preference saved as {(isImageDisplayPreferred ? "image-cards" : "drop-down")}",
                "PartSelection"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"PartSelection: Failed to save display preference. Error: {ex.Message}",
                "PartSelection"
            );

            _isRestoringDisplayFormatPreference = true;
            IsImageDisplayPreferred = _persistedImageDisplayPreference;
            _isRestoringDisplayFormatPreference = false;

            await _errorHandler.HandleErrorAsync(
                "Failed to save part selection display preference",
                Enum_ErrorSeverity.Warning,
                ex,
                true
            );
        }
    }

    private int? GetCurrentUserId()
    {
        int? employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        return employeeNumber.HasValue && employeeNumber.Value > 0 ? employeeNumber : null;
    }

    #endregion

    #region Load Parts Command

    [RelayCommand]
    private async Task LoadPartsAsync()
    {
        _logger.LogInfo(
            $"PartSelection: LoadPartsAsync called with SelectedTypeId={SelectedTypeId}",
            "PartSelection"
        );
        if (SelectedTypeId == 0)
        {
            _logger.LogInfo(
                "PartSelection: SelectedTypeId is 0, returning from LoadPartsAsync",
                "PartSelection"
            );
            return;
        }

        try
        {
            IsBusy = true;

            _logger.LogInfo(
                $"PartSelection: Calling _dunnageService.GetPartsByTypeAsync({SelectedTypeId})",
                "PartSelection"
            );
            var result = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);

            if (result.IsSuccess && result.Data != null)
            {
                AvailableParts = new ObservableCollection<Model_DunnagePart>(result.Data);

                _logger.LogInfo(
                    $"PartSelection: Successfully loaded {AvailableParts.Count} parts",
                    "PartSelection"
                );
            }
            else
            {
                _logger.LogWarning(
                    $"PartSelection: Failed to load parts: {result.ErrorMessage}",
                    "PartSelection"
                );
                await _errorHandler.HandleDaoErrorAsync(result, nameof(LoadPartsAsync), true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"PartSelection: Error in LoadPartsAsync: {ex.Message}",
                ex,
                "PartSelection"
            );
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

    #region Part Selection

    private void RestoreWorkflowSelectedPart()
    {
        var workflowSelectedPart = _workflowService.CurrentSession.SelectedPart;
        if (workflowSelectedPart is null)
        {
            SelectedPart = null;
            return;
        }

        var matchingPart = AvailableParts.FirstOrDefault(part =>
            part.Id == workflowSelectedPart.Id
            || part.PartId.Equals(workflowSelectedPart.PartId, StringComparison.OrdinalIgnoreCase)
        );

        SelectedPart = matchingPart;
    }

    partial void OnSelectedPartChanged(Model_DunnagePart? oldValue, Model_DunnagePart? newValue)
    {
        if (newValue != null)
        {
            _logger.LogInfo($"Part selected via ComboBox: {newValue.PartId}", "PartSelection");

            // Update workflow session immediately when part is selected
            _workflowService.CurrentSession.SelectedPart = newValue;
            _logger.LogInfo(
                $"Updated workflow session with selected part: {newValue.PartId}",
                "PartSelection"
            );

            UpdateSelectedPartSpecs(newValue);
            _ = CheckInventoryStatusAsync(newValue);
        }
        else
        {
            _workflowService.CurrentSession.SelectedPart = null;
            IsInventoryNotificationVisible = false;
            ReplaceSelectedPartSpecSummaries(Array.Empty<string>());
            HasSelectedPartSpecs = false;
        }

        // Notify that command can execute state changed
        OnPropertyChanged(nameof(SelectedPartVisualSource));
        OnPropertyChanged(nameof(HasSelectedPartVisual));
        SelectPartCommand.NotifyCanExecuteChanged();
        EditPartCommand.NotifyCanExecuteChanged();
        DeletePartCommand.NotifyCanExecuteChanged();
    }

    partial void OnCanManageDefinitionsChanged(bool value)
    {
        OnPropertyChanged(nameof(CanDeleteSelectedPart));
        DeletePartCommand.NotifyCanExecuteChanged();
    }

    private async Task CheckInventoryStatusAsync(Model_DunnagePart part)
    {
        try
        {
            var inventoryDetails = await _dunnageService.GetInventoryDetailsAsync(part.PartId);

            if (inventoryDetails.IsSuccess && inventoryDetails.Data is not null)
            {
                IsInventoryNotificationVisible = true;
                InventoryMethod = string.IsNullOrWhiteSpace(inventoryDetails.Data.InventoryMethod)
                    ? "Adjust In"
                    : inventoryDetails.Data.InventoryMethod;
                _workflowService.CurrentSession.InventoryMethod = InventoryMethod;

                _logger.LogInfo(
                    $"Part {part.PartId} is inventoried with method {InventoryMethod}",
                    "PartSelection"
                );
            }
            else
            {
                IsInventoryNotificationVisible = false;
                InventoryMethod = string.Empty;
                _workflowService.CurrentSession.InventoryMethod = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking inventory status: {ex.Message}", ex, "PartSelection");
            // Don't show error to user - just hide notification
            IsInventoryNotificationVisible = false;
            InventoryMethod = string.Empty;
            _workflowService.CurrentSession.InventoryMethod = string.Empty;
        }
    }

    private void UpdateSelectedPartSpecs(Model_DunnagePart part)
    {
        var specSummaries = new List<string>();

        foreach (var pair in part.SpecValuesDict.OrderBy(item => item.Key))
        {
            var formattedValue = FormatSpecValue(pair.Value);
            if (string.IsNullOrWhiteSpace(formattedValue))
            {
                continue;
            }

            specSummaries.Add($"{pair.Key}: {formattedValue}");
        }

        ReplaceSelectedPartSpecSummaries(specSummaries);
        HasSelectedPartSpecs = SelectedPartSpecSummaries.Count > 0;
    }

    private void ReplaceSelectedPartSpecSummaries(IEnumerable<string> specSummaries)
    {
        SelectedPartSpecSummaries = new ObservableCollection<string>(specSummaries);
    }

    private static string FormatSpecValue(object? rawValue)
    {
        if (rawValue is null)
        {
            return string.Empty;
        }

        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.True => "Yes",
                JsonValueKind.False => "No",
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.Array => string.Join(
                    ", ",
                    element.EnumerateArray().Select(item => item.ToString())
                ),
                _ => element.ToString(),
            };
        }

        return rawValue.ToString() ?? string.Empty;
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInfo("Returning to Type Selection", "PartSelection");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
    }

    [RelayCommand(CanExecute = nameof(IsPartSelected))]
    private async Task SelectPartAsync()
    {
        if (SelectedPart == null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            // Set selected part in workflow
            _workflowService.CurrentSession.SelectedPart = SelectedPart;
            await CheckInventoryStatusAsync(SelectedPart);

            _logger.LogInfo($"Selected part: {SelectedPart.PartId}", "PartSelection");

            // Navigate to quantity entry
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error selecting part",
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
    private async Task QuickAddPartAsync()
    {
        try
        {
            _logger.LogInfo(
                $"Quick Add Part requested for type {SelectedTypeName}",
                "PartSelection"
            );

            // Fetch specs for the selected type
            var specsResult = await _dunnageService.GetSpecsForTypeAsync(SelectedTypeId);
            var specs =
                (specsResult.IsSuccess && specsResult.Data != null)
                    ? specsResult.Data
                    : new List<Model_DunnageSpec>();

            var existingPartsResult = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);
            var existingParts =
                existingPartsResult.IsSuccess && existingPartsResult.Data != null
                    ? existingPartsResult.Data
                    : new List<Model_DunnagePart>();
            var quantityTypesResult = await _dunnageService.GetQuantityTypesAsync();
            var quantityTypes =
                quantityTypesResult.IsSuccess && quantityTypesResult.Data != null
                    ? quantityTypesResult.Data
                    : new List<Model_DunnageQuantityType>();

            Model_DunnagePartDialogDraft? dialogDraft = null;

            while (true)
            {
                var dialog = new Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog(
                    SelectedTypeId,
                    SelectedTypeName,
                    specs,
                    quantityTypes,
                    dialogDraft
                )
                {
                    XamlRoot = App.MainWindow?.Content?.XamlRoot,
                };

                if (dialog.XamlRoot == null)
                {
                    _logger.LogInfo("Cannot show dialog: XamlRoot is null", "PartSelection");
                    return;
                }

                dialog.PrepareDialogSize();
                await dialog.ShowAsync();

                if (dialog.RequestChooseExistingSpecs)
                {
                    dialogDraft = await SelectExistingSpecsDraftAsync(
                        existingParts,
                        dialog.GetDraft()
                    );
                    continue;
                }

                if (!dialog.WasAccepted)
                {
                    return;
                }

                var partId = dialog.PartId;
                var specValuesJson = dialog.SpecValuesJson;

                _logger.LogInfo(
                    $"Adding new part: {partId} for type {SelectedTypeName}",
                    "PartSelection"
                );

                // Create new part model
                var newPart = new Model_DunnagePart
                {
                    PartId = partId,
                    TypeId = SelectedTypeId,
                    SpecValues = specValuesJson,
                    ImagePath = dialog.SelectedImagePath,
                    DunnageTypeName = SelectedTypeName,
                    QuantityType = dialog.ResolvedQuantityType,
                    HomeLocation = dialog.HomeLocation,
                };

                var insertResult = await _dunnageService.InsertPartWithInventoryAsync(
                    newPart,
                    dialog.SelectedInventoryMethod
                );

                if (insertResult.IsSuccess)
                {
                    _logger.LogInfo($"Successfully added part: {partId}", "PartSelection");

                    // Reload parts to show new part
                    await LoadPartsAsync();

                    // Auto-select the new part
                    var addedPart = AvailableParts.FirstOrDefault(p => p.PartId == partId);
                    if (addedPart != null)
                    {
                        SelectedPart = addedPart;
                    }

                    if (dialog.RequestSaveQuantityTypeForFutureUse)
                    {
                        await PromptToSaveCustomQuantityTypeAsync(dialog.ResolvedQuantityType);
                    }

                    StatusMessage = $"Added new part: {partId}";
                }
                else
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        insertResult,
                        nameof(QuickAddPartAsync),
                        true
                    );

                    if (IsDuplicatePartError(insertResult))
                    {
                        dialogDraft = dialog.GetDraft();
                        continue;
                    }
                }

                break;
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error adding new dunnage part",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    [RelayCommand(CanExecute = nameof(IsPartSelected))]
    private async Task EditPartAsync()
    {
        if (SelectedPart == null || IsBusy)
        {
            return;
        }

        try
        {
            _logger.LogInfo($"Edit Part requested for: {SelectedPart.PartId}", "PartSelection");

            var specsResult = await _dunnageService.GetSpecsForTypeAsync(SelectedTypeId);
            var specs =
                (specsResult.IsSuccess && specsResult.Data != null)
                    ? specsResult.Data
                    : new List<Model_DunnageSpec>();

            var existingPartsResult = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);
            var existingParts =
                existingPartsResult.IsSuccess && existingPartsResult.Data != null
                    ? existingPartsResult.Data
                    : new List<Model_DunnagePart>();
            var quantityTypesResult = await _dunnageService.GetQuantityTypesAsync();
            var quantityTypes =
                quantityTypesResult.IsSuccess && quantityTypesResult.Data != null
                    ? quantityTypesResult.Data
                    : new List<Model_DunnageQuantityType>();

            var inventoryDetailsResult = await _dunnageService.GetInventoryDetailsAsync(
                SelectedPart.PartId
            );
            var currentInventoryMethod =
                inventoryDetailsResult.IsSuccess && inventoryDetailsResult.Data != null
                    ? inventoryDetailsResult.Data.InventoryMethod ?? "Not Inventoried"
                    : "Not Inventoried";
            var currentInventoryNotes =
                inventoryDetailsResult.IsSuccess && inventoryDetailsResult.Data != null
                    ? inventoryDetailsResult.Data.Notes ?? string.Empty
                    : string.Empty;

            Model_DunnagePartDialogDraft? dialogDraft = null;

            while (true)
            {
                var dialog = new Module_Dunnage.Views.View_Dunnage_EditPartDialog(
                    SelectedPart,
                    specs,
                    quantityTypes,
                    SelectedTypeName,
                    currentInventoryMethod,
                    dialogDraft,
                    CanManageDefinitions
                )
                {
                    XamlRoot = App.MainWindow?.Content?.XamlRoot,
                };

                if (dialog.XamlRoot == null)
                {
                    _logger.LogInfo("Cannot show dialog: XamlRoot is null", "PartSelection");
                    return;
                }

                dialog.PrepareDialogSize();
                await dialog.ShowAsync();

                if (dialog.RequestChooseExistingSpecs)
                {
                    dialogDraft = await SelectExistingSpecsDraftAsync(
                        existingParts,
                        dialog.GetDraft()
                    );
                    continue;
                }

                if (dialog.RequestDelete)
                {
                    await DeletePartInternalAsync(SelectedPart);
                    return;
                }

                if (!dialog.WasAccepted)
                {
                    return;
                }

                var partIdToReselect = SelectedPart.PartId;
                var updatedPartId = dialog.UpdatedPartId;

                var updatedPart = new Model_DunnagePart
                {
                    Id = SelectedPart.Id,
                    PartId = updatedPartId,
                    TypeId = SelectedPart.TypeId,
                    DunnageTypeName = SelectedTypeName,
                    SpecValues = dialog.UpdatedSpecValuesJson,
                    ImagePath = dialog.SelectedImagePath,
                    QuantityType = dialog.ResolvedQuantityType,
                    HomeLocation = dialog.UpdatedHomeLocation,
                };

                var updateResult = await _dunnageService.UpdatePartWithInventoryAndReferencesAsync(
                    updatedPart,
                    partIdToReselect,
                    dialog.SelectedInventoryMethod,
                    currentInventoryNotes
                );

                if (updateResult.IsSuccess)
                {
                    _logger.LogInfo(
                        $"Successfully updated part: {partIdToReselect} -> {updatedPartId}",
                        "PartSelection"
                    );

                    await LoadPartsAsync();

                    var refreshed = AvailableParts.FirstOrDefault(p => p.PartId == updatedPartId);
                    if (refreshed != null)
                    {
                        SelectedPart = refreshed;
                    }

                    if (dialog.RequestSaveQuantityTypeForFutureUse)
                    {
                        await PromptToSaveCustomQuantityTypeAsync(dialog.ResolvedQuantityType);
                    }

                    StatusMessage = $"Updated part: {updatedPartId}";
                }
                else
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        updateResult,
                        nameof(EditPartAsync),
                        true
                    );

                    if (IsDuplicatePartError(updateResult))
                    {
                        dialogDraft = dialog.GetDraft();
                        continue;
                    }
                }

                break;
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error editing dunnage part",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedPart))]
    private async Task DeletePartAsync()
    {
        await DeletePartInternalAsync(SelectedPart);
    }

    private static bool IsDuplicatePartError(Model_Dao_Result result)
    {
        return !string.IsNullOrWhiteSpace(result.ErrorMessage)
            && (
                result.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                || result.ErrorMessage.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
            );
    }

    /// <summary>
    /// Shows contextual help for part selection.
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.PartSelection");
    }

    private async Task EnsurePrivilegeStateAsync()
    {
        Model_UserSession? currentSession = _sessionManager.CurrentSession;
        int? employeeNumber = currentSession?.User?.EmployeeNumber;

        if (
            employeeNumber.HasValue
            && employeeNumber.Value > 0
            && (!_userPrivileges.IsInitialized || _userPrivileges.CurrentUserId != employeeNumber)
        )
        {
            var initializeResult = await _userPrivileges.InitializeAsync(employeeNumber.Value);
            if (!initializeResult.IsSuccess)
            {
                _logger.LogWarning(
                    $"PartSelection: Failed to initialize privileges: {initializeResult.ErrorMessage}",
                    "PartSelection"
                );
                CanManageDefinitions = false;
                return;
            }
        }

        CanManageDefinitions = _userPrivileges.HasAnyRole("Admin", "Developer");
    }

    private async Task DeletePartInternalAsync(Model_DunnagePart? part)
    {
        if (part is null || !CanManageDefinitions || IsBusy)
        {
            return;
        }

        try
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot,
                Title = "Delete Dunnage Part",
                Content =
                    $"Are you sure you want to permanently delete '{part.PartId}'? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                dialog,
                dialog.XamlRoot
            );

            var result = await dialog.ShowAsync();
            if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            IsBusy = true;

            var deleteResult = await _dunnageService.DeletePartAsync(part.PartId);
            if (deleteResult.IsSuccess)
            {
                _workflowService.CurrentSession.SelectedPart = null;
                SelectedPart = null;
                ReplaceSelectedPartSpecSummaries(Array.Empty<string>());
                HasSelectedPartSpecs = false;
                IsInventoryNotificationVisible = false;
                InventoryMethod = string.Empty;
                await LoadPartsAsync();
                StatusMessage = $"Deleted part: {part.PartId}";
                _logger.LogInfo($"Deleted part: {part.PartId}", "PartSelection");
                return;
            }

            await _errorHandler.HandleDaoErrorAsync(deleteResult, nameof(DeletePartAsync), true);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error deleting dunnage part",
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

    private async Task<Model_DunnagePartDialogDraft> SelectExistingSpecsDraftAsync(
        List<Model_DunnagePart> existingParts,
        Model_DunnagePartDialogDraft currentDraft
    )
    {
        var templateOptions = existingParts
            .Select(CreateSpecTemplateOption)
            .Where(option =>
                option.SpecValues.Count > 0 || !string.IsNullOrWhiteSpace(option.Notes)
            )
            .OrderBy(option => option.PartId)
            .ToList();

        if (templateOptions.Count == 0)
        {
            await _errorHandler.HandleErrorAsync(
                "No saved specs were found for this dunnage type.",
                Enum_ErrorSeverity.Info,
                null,
                true
            );
            return currentDraft;
        }

        var dialog = new Module_Dunnage.Views.View_Dunnage_SelectExistingSpecsDialog(
            templateOptions
        )
        {
            XamlRoot = App.MainWindow?.Content?.XamlRoot,
        };

        if (dialog.XamlRoot == null)
        {
            return currentDraft;
        }

        var result = await dialog.ShowAsync();
        if (
            result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
            || dialog.SelectedTemplate is null
        )
        {
            return currentDraft;
        }

        var nextDraft = currentDraft.Clone();
        nextDraft.SpecValues = new Dictionary<string, object?>(dialog.SelectedTemplate.SpecValues);
        nextDraft.Notes = dialog.SelectedTemplate.Notes;
        nextDraft.QuantityType = dialog.SelectedTemplate.QuantityType;

        if (string.IsNullOrWhiteSpace(nextDraft.PartId))
        {
            nextDraft.PartId = Helper_Dunnage_PartIdSuggestion.BuildSuggestedPartId(
                SelectedTypeName,
                nextDraft.SpecValues
            );
        }

        return nextDraft;
    }

    private static Model_DunnageSpecTemplateOption CreateSpecTemplateOption(Model_DunnagePart part)
    {
        var specValues = part.SpecValuesDict.ToDictionary(
            pair => pair.Key,
            pair => NormalizeSpecValue(pair.Value)
        );

        foreach (var definition in part.PartSpecificSpecDefinitions)
        {
            specValues[definition.Key] = definition.Value;
        }

        var notes = specValues.TryGetValue("Notes", out var notesValue)
            ? notesValue?.ToString() ?? string.Empty
            : string.Empty;
        specValues.Remove("Notes");

        return new Model_DunnageSpecTemplateOption
        {
            PartId = part.PartId,
            HomeLocation = part.HomeLocation ?? string.Empty,
            QuantityType = part.QuantityType,
            Notes = notes,
            SpecValues = specValues,
            SpecSummary = BuildSpecSummary(specValues),
        };
    }

    private static object? NormalizeSpecValue(object? rawValue)
    {
        if (rawValue is JsonElement element)
        {
            if (Helper_Dunnage_PartSpecs.TryGetSpecDefinition(element, out var definition))
            {
                return definition;
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number when element.TryGetInt64(out var integerValue) => integerValue,
                JsonValueKind.Number => element.GetDouble(),
                _ => element.ToString(),
            };
        }

        return rawValue;
    }

    private static string BuildSpecSummary(Dictionary<string, object?> specValues)
    {
        if (specValues.Count == 0)
        {
            return "No saved spec values";
        }

        return string.Join(
            " | ",
            specValues
                .OrderBy(pair => pair.Key)
                .Select(pair =>
                    Helper_Dunnage_PartSpecs.TryGetSpecDefinition(pair.Value, out var definition)
                        ? $"{pair.Key}: {definition.DataType} field"
                        : $"{pair.Key}: {pair.Value}"
                )
        );
    }

    private async Task PromptToSaveCustomQuantityTypeAsync(string quantityType)
    {
        if (string.IsNullOrWhiteSpace(quantityType))
        {
            return;
        }

        var xamlRoot = App.MainWindow?.Content?.XamlRoot;
        if (xamlRoot is null)
        {
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Save Quantity Type",
            Content = $"Would you like to save '{quantityType}' for future use?",
            PrimaryButtonText = "Save",
            CloseButtonText = "Not Now",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        if (await dialog.ShowAsync() != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        var saveResult = await _dunnageService.SaveQuantityTypeIfMissingAsync(quantityType);
        if (!saveResult.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(
                saveResult,
                nameof(PromptToSaveCustomQuantityTypeAsync),
                true
            );
        }
    }

    #endregion
}
