using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step3;

/// <summary>
/// Manages the completion screen display for Wizard Step 3.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_CompletionScreen : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private string _successMessage = "Transaction Saved Successfully!";

    [ObservableProperty]
    private string _transactionId = string.Empty;

    [ObservableProperty]
    private string _csvFilePath = string.Empty;

    [ObservableProperty]
    private string _networkCsvPath = string.Empty;

    [ObservableProperty]
    private bool _canPrintLabels = true;

    [ObservableProperty]
    private int _totalLoads = 0;

    [ObservableProperty]
    private decimal _totalWeight = 0;

    [ObservableProperty]
    private DateTime _completedAt = DateTime.Now;

    [ObservableProperty]
    private string _completedAtFormatted = string.Empty;

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    #endregion

    #region Constructor

    public ViewModel_Receiving_Wizard_Display_CompletionScreen(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Completion Screen ViewModel initialized");
        
        UpdateCompletedAtFormatted();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void PrintLabels()
    {
        if (!CanPrintLabels)
        {
            ShowStatus("Print labels functionality is not available");
            return;
        }

        try
        {
            _logger.LogInfo($"Print labels requested for transaction {TransactionId}");
            StatusMessage = "Opening print labels...";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Print labels error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task StartNewTransactionAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Starting new transaction...";
            
            _logger.LogInfo("User starting new transaction from completion screen");
            
            await Task.CompletedTask;
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
    private void ReturnToHub()
    {
        try
        {
            _logger.LogInfo("User returning to hub from completion screen");
            StatusMessage = "Returning to hub...";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Return to hub error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenCSVFile()
    {
        if (string.IsNullOrWhiteSpace(CsvFilePath))
        {
            ShowStatus("CSV file path is not available");
            return;
        }

        try
        {
            _logger.LogInfo($"Opening CSV file: {CsvFilePath}");
            
            if (File.Exists(CsvFilePath))
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = CsvFilePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
                
                StatusMessage = "CSV file opened";
            }
            else
            {
                ShowStatus($"CSV file not found at: {CsvFilePath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Open CSV file error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CopyLocalPathToClipboard()
    {
        if (string.IsNullOrWhiteSpace(CsvFilePath)) return;

        try
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(CsvFilePath);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            
            _logger.LogInfo("Local CSV path copied to clipboard");
            StatusMessage = "Path copied to clipboard";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Copy to clipboard error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CopyNetworkPathToClipboard()
    {
        if (string.IsNullOrWhiteSpace(NetworkCsvPath)) return;

        try
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(NetworkCsvPath);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            
            _logger.LogInfo("Network CSV path copied to clipboard");
            StatusMessage = "Path copied to clipboard";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Copy to clipboard error: {ex.Message}");
        }
    }

    #endregion

    #region Public Methods

    public void SetCompletionData(
        string transactionId,
        string csvFilePath,
        string networkCsvPath,
        string poNumber,
        string partNumber,
        int totalLoads,
        decimal totalWeight)
    {
        _transactionId = transactionId ?? string.Empty;
        _csvFilePath = csvFilePath ?? string.Empty;
        _networkCsvPath = networkCsvPath ?? string.Empty;
        _poNumber = poNumber ?? string.Empty;
        _partNumber = partNumber ?? string.Empty;
        TotalLoads = totalLoads;
        TotalWeight = totalWeight;
        CompletedAt = DateTime.Now;
        
        UpdateCompletedAtFormatted();

        _logger.LogInfo($"Completion data set: TX={_transactionId}, PO={_poNumber}, Part={_partNumber}");
    }

    #endregion

    #region Helper Methods

    private void UpdateCompletedAtFormatted()
    {
        CompletedAtFormatted = CompletedAt.ToString("MMMM dd, yyyy - h:mm tt");
    }

    #endregion
}
