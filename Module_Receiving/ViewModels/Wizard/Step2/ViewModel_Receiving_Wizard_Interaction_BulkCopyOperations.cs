using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step2;

/// <summary>
/// Manages bulk copy operations for load details in Wizard Step 2.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private int _sourceLoadNumber = 1;

    [ObservableProperty]
    private int _maxLoadNumber = 1;

    [ObservableProperty]
    private bool _copyHeatLot = true;

    [ObservableProperty]
    private bool _copyPackageType = true;

    [ObservableProperty]
    private bool _copyPackagesPerLoad = true;

    [ObservableProperty]
    private bool _copyReceivingLocation = false;

    [ObservableProperty]
    private int _affectedLoadCount = 0;

    [ObservableProperty]
    private string _transactionId = string.Empty;

    [ObservableProperty]
    private string _currentUser = Environment.UserName;

    [ObservableProperty]
    private bool _isPreviewDialogOpen = false;

    #endregion

    #region Constructor

    public ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Bulk Copy Operations ViewModel initialized");
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task PreviewCopyAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Generating copy preview...";

            if (SourceLoadNumber < 1 || SourceLoadNumber > MaxLoadNumber)
            {
                await _errorHandler.ShowErrorDialogAsync(
                    "Invalid Source Load",
                    $"Source load number must be between 1 and {MaxLoadNumber}",
                    Enum_ErrorSeverity.Warning);
                return;
            }

            if (!CopyHeatLot && !CopyPackageType && !CopyPackagesPerLoad && !CopyReceivingLocation)
            {
                await _errorHandler.ShowErrorDialogAsync(
                    "No Fields Selected",
                    "Please select at least one field to copy",
                    Enum_ErrorSeverity.Warning);
                return;
            }

            IsPreviewDialogOpen = true;
            _logger.LogInfo($"Copy preview opened for source load {SourceLoadNumber}");
            StatusMessage = "Preview ready";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex.Message, Enum_ErrorSeverity.Low, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteCopyAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Executing bulk copy...";

            var selectedFieldsList = new List<Enum_Receiving_CopyType_FieldSelection>();
            if (CopyHeatLot) selectedFieldsList.Add(Enum_Receiving_CopyType_FieldSelection.HeatLot);
            if (CopyPackageType) selectedFieldsList.Add(Enum_Receiving_CopyType_FieldSelection.PackageType);
            if (CopyPackagesPerLoad) selectedFieldsList.Add(Enum_Receiving_CopyType_FieldSelection.PackagesPerLoad);
            if (CopyReceivingLocation) selectedFieldsList.Add(Enum_Receiving_CopyType_FieldSelection.ReceivingLocation);

            var command = new BulkCopyFieldsCommand
            {
                TransactionId = TransactionId,
                SourceLoadNumber = SourceLoadNumber,
                FieldsToCopy = selectedFieldsList,
                ModifiedBy = CurrentUser
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                AffectedLoadCount = result.Data;
                _logger.LogInfo($"Bulk copy successful: {AffectedLoadCount} loads affected");
                StatusMessage = $"Copied to {AffectedLoadCount} loads successfully";
            }
            else
            {
                await _errorHandler.ShowErrorDialogAsync("Copy Failed", result.ErrorMessage ?? "Copy operation failed", Enum_ErrorSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex.Message, Enum_ErrorSeverity.Medium, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearAutoFilledFieldsAsync(int? targetLoadNumber = null)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Clearing auto-filled fields...";

            var command = new ClearAutoFilledFieldsCommand
            {
                TransactionId = TransactionId,
                TargetLoadNumber = targetLoadNumber,
                ModifiedBy = CurrentUser
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var message = targetLoadNumber.HasValue
                    ? $"Auto-filled fields cleared for load {targetLoadNumber}"
                    : "Auto-filled fields cleared for all loads";
                
                _logger.LogInfo(message);
                StatusMessage = message;
            }
            else
            {
                await _errorHandler.ShowErrorDialogAsync("Clear Failed", result.ErrorMessage ?? "Clear operation failed", Enum_ErrorSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex.Message, Enum_ErrorSeverity.Low, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAllFields()
    {
        CopyHeatLot = true;
        CopyPackageType = true;
        CopyPackagesPerLoad = true;
        CopyReceivingLocation = true;
        _logger.LogInfo("All fields selected for copy");
        StatusMessage = "All fields selected";
    }

    [RelayCommand]
    private void DeselectAllFields()
    {
        CopyHeatLot = false;
        CopyPackageType = false;
        CopyPackagesPerLoad = false;
        CopyReceivingLocation = false;
        _logger.LogInfo("All fields deselected");
        StatusMessage = "All fields deselected";
    }

    #endregion

    #region Public Methods

    public void SetTransactionId(string transactionId)
    {
        TransactionId = transactionId ?? string.Empty;
        _logger.LogInfo($"Transaction ID set: {TransactionId}");
    }

    public void SetMaxLoadNumber(int maxLoadNumber)
    {
        MaxLoadNumber = maxLoadNumber;
        _logger.LogInfo($"Max load number set: {MaxLoadNumber}");
    }

    #endregion
}
