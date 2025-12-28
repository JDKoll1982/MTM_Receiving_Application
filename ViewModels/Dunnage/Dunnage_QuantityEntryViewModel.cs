using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public Dunnage_QuantityEntryViewModel(
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _quantity = 1;

    [ObservableProperty]
    private string _selectedTypeName = string.Empty;

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
            SelectedPartName = _workflowService.CurrentSession.SelectedPart?.PartId ?? string.Empty;

            _logger.LogInfo($"Loaded context: Type={SelectedTypeName}, Part={SelectedPartName}", "QuantityEntry");
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
}
