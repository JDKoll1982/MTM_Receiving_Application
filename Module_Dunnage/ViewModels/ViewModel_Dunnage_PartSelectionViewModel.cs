using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Part Selection with inventory notification
/// </summary>
public partial class ViewModel_Dunnage_PartSelection : ViewModel_Shared_Base
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_Dispatcher _dispatcher;

    public ViewModel_Dunnage_PartSelection(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _dispatcher = dispatcher;
        _helpService = helpService;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _logger.LogInfo("PartSelection: ViewModel constructed and subscribed to StepChanged", "PartSelection");
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        _logger.LogInfo($"PartSelection: Workflow step changed to {_workflowService.CurrentStep}", "PartSelection");
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.PartSelection)
        {
            _logger.LogInfo("PartSelection: Step is PartSelection, calling InitializeAsync via dispatcher", "PartSelection");
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
    private Model_DunnagePart? _selectedPart;

    [ObservableProperty]
    private bool _isInventoryNotificationVisible;

    [ObservableProperty]
    private string _inventoryNotificationMessage = string.Empty;

    [ObservableProperty]
    private string _inventoryMethod = "Adjust In";

    [ObservableProperty]
    private string _selectedTypeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTypeIconKind))]
    private string _selectedTypeIcon = "Help";

    /// <summary>
    /// Gets the MaterialIconKind for the selected type
    /// </summary>
    public MaterialIconKind SelectedTypeIconKind
    {
        get
        {
            if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind))
            {
                return kind;
            }
            return MaterialIconKind.PackageVariantClosed;
        }
    }

    [ObservableProperty]
    private int _selectedTypeId;

    /// <summary>
    /// Helper property for UI binding
    /// </summary>
    public bool IsPartSelected => SelectedPart != null;

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
            _logger.LogInfo("PartSelection: InitializeAsync returning because IsBusy is true", "PartSelection");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading parts...";

            // Get selected type from workflow
            SelectedTypeId = _workflowService.CurrentSession.SelectedTypeId;
            SelectedTypeName = _workflowService.CurrentSession.SelectedTypeName ?? string.Empty;
            SelectedTypeIcon = _workflowService.CurrentSession.SelectedType?.Icon ?? "Help";

            _logger.LogInfo($"PartSelection: SelectedTypeId={SelectedTypeId}, SelectedTypeName={SelectedTypeName}, SelectedTypeIcon={SelectedTypeIcon}", "PartSelection");

            await LoadPartsAsync();

            StatusMessage = $"Loaded {AvailableParts.Count} parts for {SelectedTypeName}";
            _logger.LogInfo($"PartSelection: {StatusMessage}", "PartSelection");
        }
        catch (Exception ex)
        {
            _logger.LogError($"PartSelection: Failed to initialize: {ex.Message}", ex, "PartSelection");
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

    #region Load Parts Command

    [RelayCommand]
    private async Task LoadPartsAsync()
    {
        _logger.LogInfo($"PartSelection: LoadPartsAsync called with SelectedTypeId={SelectedTypeId}", "PartSelection");
        if (SelectedTypeId == 0)
        {
            _logger.LogInfo("PartSelection: SelectedTypeId is 0, returning from LoadPartsAsync", "PartSelection");
            return;
        }

        try
        {
            IsBusy = true;

            _logger.LogInfo($"PartSelection: Calling _dunnageService.GetPartsByTypeAsync({SelectedTypeId})", "PartSelection");
            var result = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);

            if (result.IsSuccess && result.Data != null)
            {
                AvailableParts.Clear();
                foreach (var part in result.Data)
                {
                    AvailableParts.Add(part);
                }

                _logger.LogInfo($"PartSelection: Successfully loaded {AvailableParts.Count} parts", "PartSelection");
            }
            else
            {
                _logger.LogWarning($"PartSelection: Failed to load parts: {result.ErrorMessage}", "PartSelection");
                await _errorHandler.HandleDaoErrorAsync(
                    result,
                    nameof(LoadPartsAsync),
                    true
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"PartSelection: Error in LoadPartsAsync: {ex.Message}", ex, "PartSelection");
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

    partial void OnSelectedPartChanged(Model_DunnagePart? oldValue, Model_DunnagePart? newValue)
    {
        if (newValue != null)
        {
            _logger.LogInfo($"Part selected via ComboBox: {newValue.PartId}", "PartSelection");

            // Update workflow session immediately when part is selected
            _workflowService.CurrentSession.SelectedPart = newValue;
            _logger.LogInfo($"Updated workflow session with selected part: {newValue.PartId}", "PartSelection");

            _ = CheckInventoryStatusAsync(newValue);
        }
        else
        {
            _workflowService.CurrentSession.SelectedPart = null;
            IsInventoryNotificationVisible = false;
        }

        // Notify that command can execute state changed
        SelectPartCommand.NotifyCanExecuteChanged();
    }

    private async Task CheckInventoryStatusAsync(Model_DunnagePart part)
    {
        try
        {
            var isInventoried = await _dunnageService.IsPartInventoriedAsync(part.PartId);

            if (isInventoried)
            {
                // Part is inventoried - show notification
                IsInventoryNotificationVisible = true;
                InventoryMethod = "Adjust In"; // No PO context yet
                UpdateInventoryMessage();

                _logger.LogInfo($"Part {part.PartId} is inventoried - showing notification", "PartSelection");
            }
            else
            {
                IsInventoryNotificationVisible = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking inventory status: {ex.Message}", ex, "PartSelection");
            // Don't show error to user - just hide notification
            IsInventoryNotificationVisible = false;
        }
    }

    private void UpdateInventoryMessage()
    {
        InventoryNotificationMessage = $"This part requires inventory in Visual. Method: {InventoryMethod}";
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
            _logger.LogInfo($"Quick Add Part requested for type {SelectedTypeName}", "PartSelection");

            // Fetch specs for the selected type
            var specsResult = await _dunnageService.GetSpecsForTypeAsync(SelectedTypeId);
            var specs = (specsResult.IsSuccess && specsResult.Data != null)
                ? specsResult.Data
                : new List<Model_DunnageSpec>();

            // Show dialog with type pre-selected and specs
            var dialog = new Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog(SelectedTypeId, SelectedTypeName, specs)
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            if (dialog.XamlRoot == null)
            {
                _logger.LogInfo("Cannot show dialog: XamlRoot is null", "PartSelection");
                return;
            }

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var partId = dialog.PartId;
                var specValuesJson = dialog.SpecValuesJson;

                _logger.LogInfo($"Adding new part: {partId} for type {SelectedTypeName}", "PartSelection");

                // Create new part model
                var newPart = new Model_DunnagePart
                {
                    PartId = partId,
                    TypeId = SelectedTypeId,
                    SpecValues = specValuesJson,
                    DunnageTypeName = SelectedTypeName
                };

                // Insert new part
                var insertResult = await _dunnageService.InsertPartAsync(newPart);

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

                    StatusMessage = $"Added new part: {partId}";
                }
                else
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        insertResult,
                        nameof(QuickAddPartAsync),
                        true
                    );
                }
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

    /// <summary>\n    /// Shows contextual help for part selection\n    /// </summary>
    /// <param name="key"></param>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.PartSelection\");\n    }

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

