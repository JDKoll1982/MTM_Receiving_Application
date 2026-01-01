using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Quantity Entry
/// </summary>
public partial class Dunnage_QuantityEntryViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IService_Help _helpService;

    public Dunnage_QuantityEntryViewModel(
        IService_DunnageWorkflow workflowService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dispatcher = dispatcher;
        _helpService = helpService;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.QuantityEntry)
        {
            _dispatcher.TryEnqueue(LoadContextData);
        }
    }

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _quantity = 1;

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
    private string _selectedPartName = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    /// <summary>
    /// Indicates if the quantity is valid
    /// </summary>
    public bool IsValid => Quantity > 0;

    #endregion

    #region Initialization

    /// <summary>
    /// Load context data from workflow session
    /// </summary>
    public void LoadContextData()
    {
        try
        {
            SelectedTypeName = _workflowService.CurrentSession.SelectedTypeName ?? string.Empty;
            SelectedTypeIcon = _workflowService.CurrentSession.SelectedType?.Icon ?? "Help";
            SelectedPartName = _workflowService.CurrentSession.SelectedPart?.PartId ?? string.Empty;

            // Initialize workflow session quantity with default value if not set
            if (_workflowService.CurrentSession.Quantity <= 0)
            {
                _workflowService.CurrentSession.Quantity = Quantity;
                _logger.LogInfo($"Initialized workflow session quantity to {Quantity}", "QuantityEntry");
            }

            _logger.LogInfo($"Loaded context: Type={SelectedTypeName}, Part={SelectedPartName}, Quantity={_workflowService.CurrentSession.Quantity}", "QuantityEntry");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading context data: {ex.Message}", ex, "QuantityEntry");
        }
    }

    #endregion

    #region Validation

    partial void OnQuantityChanged(int value)
    {
        // Update workflow session immediately
        _workflowService.CurrentSession.Quantity = value;
        _logger.LogInfo($"Quantity changed to {value}, updated workflow session", "QuantityEntry");

        ValidateQuantity();
        GoNextCommand.NotifyCanExecuteChanged();
    }

    private void ValidateQuantity()
    {
        if (Quantity <= 0)
        {
            ValidationMessage = "Quantity must be greater than 0";
        }
        else
        {
            ValidationMessage = string.Empty;
        }
    }

    /// <summary>\n    /// Shows contextual help for quantity entry\n    /// </summary>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.QuantityEntry\");\n    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInfo("Navigating back to Part Selection", "QuantityEntry");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
    }

    [RelayCommand(CanExecute = nameof(IsValid))]
    private async Task GoNextAsync()
    {
        if (!IsValid || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving quantity...";

            // Set quantity in workflow session
            _workflowService.CurrentSession.Quantity = Quantity;

            _logger.LogInfo($"Quantity set to {Quantity}", "QuantityEntry");

            // Navigate to Details Entry
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving quantity",
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

    #endregion
}
