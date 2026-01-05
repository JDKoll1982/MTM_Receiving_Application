using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for viewing and managing Volvo shipment history
/// </summary>
public partial class ViewModel_Volvo_History : ViewModel_Shared_Base
{
    private readonly IService_Volvo _volvoService;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipment> _history = new();

    [ObservableProperty]
    private Model_VolvoShipment? _selectedShipment;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Now.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Now;

    [ObservableProperty]
    private string _statusFilter = "All";

    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new() { "All", "Pending PO", "Completed" };

    #endregion

    #region Constructor

    public ViewModel_Volvo_History(
        IService_Volvo volvoService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _volvoService = volvoService ?? throw new ArgumentNullException(nameof(volvoService));
    }

    #endregion

    #region Commands

    /// <summary>
    /// Loads shipment history based on current filters
    /// </summary>
    [RelayCommand]
    private async Task FilterAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading history...";

            var result = await _volvoService.GetHistoryAsync(StartDate, EndDate, StatusFilter);

            if (result.IsSuccess && result.Data != null)
            {
                History.Clear();
                foreach (var shipment in result.Data)
                {
                    History.Add(shipment);
                }
                StatusMessage = $"Loaded {History.Count} shipment(s)";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage ?? "Failed to load history",
                    "Load Error",
                    nameof(FilterAsync));
                StatusMessage = "Failed to load history";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(FilterAsync),
                nameof(ViewModel_Volvo_History));
            StatusMessage = "Error loading history";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Views details for the selected shipment
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private async Task ViewDetailAsync()
    {
        if (SelectedShipment == null)
            return;

        try
        {
            StatusMessage = $"Loading details for shipment #{SelectedShipment.ShipmentNumber}...";

            // Get shipment lines
            var result = await _volvoService.GetShipmentLinesAsync(SelectedShipment.Id);

            if (result.IsSuccess && result.Data != null)
            {
                // Show detail flyout/dialog with shipment lines
                // For now, just update status message
                StatusMessage = $"Loaded {result.Data.Count} line(s) for shipment #{SelectedShipment.ShipmentNumber}";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage ?? "Failed to load shipment details",
                    "Load Error",
                    nameof(ViewDetailAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ViewDetailAsync),
                nameof(ViewModel_Volvo_History));
        }
    }

    private bool CanViewDetail() => SelectedShipment != null && !IsBusy;

    /// <summary>
    /// Opens edit dialog for the selected shipment
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditAsync()
    {
        if (SelectedShipment == null)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading shipment data...";

            // Get shipment lines
            var linesResult = await _volvoService.GetShipmentLinesAsync(SelectedShipment.Id);
            if (!linesResult.IsSuccess || linesResult.Data == null)
            {
                _errorHandler.ShowUserError(
                    linesResult.ErrorMessage ?? "Failed to load shipment lines",
                    "Load Error",
                    nameof(EditAsync));
                return;
            }

            // Get available parts for dropdown
            // Note: This would normally come from a master data service
            // For now, we'll create a placeholder collection
            var availableParts = new ObservableCollection<Model_VolvoPart>();

            // Create and show edit dialog
            var dialog = new Views.VolvoShipmentEditDialog();
            dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

            // Convert List to ObservableCollection for binding
            var linesCollection = new ObservableCollection<Model_VolvoShipmentLine>(linesResult.Data);
            dialog.LoadShipment(SelectedShipment, linesCollection, availableParts);

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                StatusMessage = "Saving changes...";

                // Get updated data from dialog
                var updatedShipment = dialog.GetUpdatedShipment();
                var updatedLines = dialog.GetUpdatedLines();

                // Call service to update shipment
                var updateResult = await _volvoService.UpdateShipmentAsync(
                    updatedShipment,
                    new System.Collections.Generic.List<Model_VolvoShipmentLine>(updatedLines));

                if (updateResult.IsSuccess)
                {
                    StatusMessage = $"Shipment #{SelectedShipment.ShipmentNumber} updated successfully";

                    // Refresh history
                    await FilterAsync();
                }
                else
                {
                    _errorHandler.ShowUserError(
                        updateResult.ErrorMessage ?? "Failed to update shipment",
                        "Update Error",
                        nameof(EditAsync));
                    StatusMessage = "Update failed";
                }
            }
            else
            {
                StatusMessage = "Edit cancelled";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(EditAsync),
                nameof(ViewModel_Volvo_History));
            StatusMessage = "Error editing shipment";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanEdit() => SelectedShipment != null && !IsBusy;

    /// <summary>
    /// Exports history to CSV file
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Exporting history...";

            var result = await _volvoService.ExportHistoryToCsvAsync(StartDate, EndDate, StatusFilter);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                // Save file picker would go here
                StatusMessage = $"Exported {History.Count} record(s) to CSV";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage ?? "Failed to export history",
                    "Export Error",
                    nameof(ExportAsync));
                StatusMessage = "Export failed";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ExportAsync),
                nameof(ViewModel_Volvo_History));
            StatusMessage = "Error exporting history";
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Property Changed Handlers

    partial void OnSelectedShipmentChanged(Model_VolvoShipment? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        EditCommand.NotifyCanExecuteChanged();
    }

    #endregion
}
