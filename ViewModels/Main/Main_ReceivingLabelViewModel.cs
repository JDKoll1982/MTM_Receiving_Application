using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Main;

/// <summary>
/// ViewModel for Receiving Label entry page
/// </summary>
public partial class Main_ReceivingLabelViewModel : Shared_BaseViewModel
{
    private readonly IService_MySQL_ReceivingLine _receivingLineService;

    public Main_ReceivingLabelViewModel(
        IService_MySQL_ReceivingLine receivingLineService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _receivingLineService = receivingLineService;
        ReceivingLines = new ObservableCollection<Model_ReceivingLine>();
    }

    /// <summary>
    /// Collection of receiving lines entered in the current session
    /// </summary>
    public ObservableCollection<Model_ReceivingLine> ReceivingLines { get; }

    [ObservableProperty]
    private Model_ReceivingLine _currentLine = new Model_ReceivingLine();

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private int _employeeNumber = 6229; // From login - will be dynamic later

    /// <summary>
    /// Adds the current line to the receiving lines collection
    /// </summary>
    [RelayCommand]
    private async Task AddLineAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Adding receiving line...";

            // Validate
            if (string.IsNullOrEmpty(CurrentLine.PartID))
            {
                await _errorHandler.HandleErrorAsync(
                    "Part ID is required",
                    Models.Enums.Enum_ErrorSeverity.Warning,
                    showDialog: true
                );
                return;
            }

            // Set employee number
            CurrentLine.EmployeeNumber = EmployeeNumber;

            // Save to database
            var result = await _receivingLineService.InsertReceivingLineAsync(CurrentLine);

            if (result.IsSuccess)
            {
                ReceivingLines.Add(CurrentLine);
                TotalRows = ReceivingLines.Count;

                CurrentLine = new Model_ReceivingLine(); // Reset for next entry
                OnPropertyChanged(nameof(CurrentLine));
                StatusMessage = "Line added successfully";
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage,
                    result.Severity,
                    showDialog: true
                );
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error adding receiving line: {ex.Message}",
                Models.Enums.Enum_ErrorSeverity.Error,
                ex,
                showDialog: true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves all lines to history and clears the current session
    /// </summary>
    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving to history...";

            // TODO: Implement bulk save to history
            await Task.Delay(500); // Placeholder

            ReceivingLines.Clear();
            TotalRows = 0;
            StatusMessage = "Saved to history";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error saving to history: {ex.Message}",
                Models.Enums.Enum_ErrorSeverity.Error,
                ex,
                showDialog: true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Fills blank fields with values from the last entry
    /// </summary>
    [RelayCommand]
    private async Task FillBlankSpacesAsync()
    {
        try
        {
            // TODO: Implement auto-fill logic from last entry
            StatusMessage = "Filled blank spaces";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error filling blank spaces: {ex.Message}",
                Models.Enums.Enum_ErrorSeverity.Warning,
                ex,
                showDialog: true
            );
        }
    }

    /// <summary>
    /// Sorts the lines for optimal printing order
    /// </summary>
    [RelayCommand]
    private async Task SortForPrintingAsync()
    {
        try
        {
            // TODO: Implement sort by Part ID, PO, Heat
            StatusMessage = "Sorted for printing";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error sorting for printing: {ex.Message}",
                Models.Enums.Enum_ErrorSeverity.Warning,
                ex,
                showDialog: true
            );
        }
    }
}
