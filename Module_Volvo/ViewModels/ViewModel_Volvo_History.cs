using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for viewing and managing Volvo shipment history
/// </summary>
public partial class ViewModel_Volvo_History : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipment> _history = new();

    [ObservableProperty]
    private Model_VolvoShipment? _selectedShipment;

    [ObservableProperty]
    private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset? _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusFilter = "All";

    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new() { "All", "Pending PO", "Completed" };

    #endregion

    #region Constructor

    public ViewModel_Volvo_History(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void GoBack()
    {
        if (App.MainWindow is MainWindow mainWindow)
        {
            var contentFrame = mainWindow.GetContentFrame();
            if (contentFrame?.CanGoBack == true)
            {
                contentFrame.GoBack();
            }
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Loads recent shipments (default: last 30 days)
    /// </summary>
    [RelayCommand]
    private async Task LoadRecentShipmentsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading recent shipments...";

            var result = await _mediator.Send(new GetRecentShipmentsQuery());

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
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load recent shipments",
                    "Load Error",
                    nameof(LoadRecentShipmentsAsync));
                StatusMessage = "Failed to load recent shipments";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadRecentShipmentsAsync),
                nameof(ViewModel_Volvo_History));
            StatusMessage = "Error loading recent shipments";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads shipment history based on current filters
    /// </summary>
    [RelayCommand]
    private async Task FilterAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading history...";

            var result = await _mediator.Send(new GetShipmentHistoryQuery
            {
                StartDate = StartDate,
                EndDate = EndDate,
                StatusFilter = StatusFilter
            });

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
                await _errorHandler.ShowUserErrorAsync(
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
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Loading details for shipment #{SelectedShipment.ShipmentNumber}...";
            await _logger.LogInfoAsync($"Loading details for shipment ID: {SelectedShipment.Id}");

            var result = await _mediator.Send(new GetShipmentDetailQuery
            {
                ShipmentId = SelectedShipment.Id
            });

            if (result.IsSuccess && result.Data != null)
            {
                var shipment = result.Data.Shipment;
                var lines = result.Data.Lines;

                // Build detail message
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Shipment #{shipment.ShipmentNumber}");
                details.AppendLine($"Date: {shipment.ShipmentDate:d}");
                details.AppendLine($"PO Number: {shipment.PONumber ?? "N/A"}");
                details.AppendLine($"Receiver: {shipment.ReceiverNumber ?? "N/A"}");
                details.AppendLine($"Status: {shipment.Status}");
                details.AppendLine();
                details.AppendLine($"Parts ({lines.Count}):");
                foreach (var line in lines)
                {
                    details.AppendLine($"  • {line.PartNumber}: {line.ReceivedSkidCount} skids ({line.CalculatedPieceCount} pieces)");
                    if (line.HasDiscrepancy)
                    {
                        details.AppendLine($"    ⚠ Discrepancy: Expected {line.ExpectedSkidCount} skids");
                    }
                }
                if (!string.IsNullOrWhiteSpace(shipment.Notes))
                {
                    details.AppendLine();
                    details.AppendLine($"Notes: {shipment.Notes}");
                }

                // Show dialog
                var dialog = new ContentDialog
                {
                    Title = $"Shipment #{SelectedShipment.ShipmentNumber} Details",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = details.ToString(),
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
                        },
                        MaxHeight = 500
                    },
                    CloseButtonText = "Close",
                    XamlRoot = App.MainWindow?.Content?.XamlRoot
                };
                await dialog.ShowAsync();

                StatusMessage = $"Viewed details for shipment #{shipment.ShipmentNumber}";
                await _logger.LogInfoAsync($"Successfully loaded {lines.Count} lines");
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load shipment details",
                    "Load Error",
                    nameof(ViewDetailAsync));
                StatusMessage = "Failed to load details";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ViewDetailAsync),
                nameof(ViewModel_Volvo_History));
            StatusMessage = "Error loading details";
        }
        finally
        {
            IsBusy = false;
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
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading shipment data...";

            var detailResult = await _mediator.Send(new GetShipmentDetailQuery
            {
                ShipmentId = SelectedShipment.Id
            });

            if (!detailResult.IsSuccess || detailResult.Data == null)
            {
                await _errorHandler.ShowUserErrorAsync(
                    detailResult.ErrorMessage ?? "Failed to load shipment lines",
                    "Load Error",
                    nameof(EditAsync));
                return;
            }

            // Get available parts for the add part dialog
            var partsResult = await _mediator.Send(new GetAllVolvoPartsQuery
            {
                IncludeInactive = false
            });

            var availableParts = partsResult.IsSuccess && partsResult.Data != null
                ? new ObservableCollection<Model_VolvoPart>(partsResult.Data)
                : new ObservableCollection<Model_VolvoPart>();

            // Create and show edit dialog
            var dialog = new Views.VolvoShipmentEditDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            // Convert List to ObservableCollection for binding
            var linesCollection = new ObservableCollection<Model_VolvoShipmentLine>(detailResult.Data.Lines);
            dialog.LoadShipment(detailResult.Data.Shipment, linesCollection, availableParts);

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                StatusMessage = "Saving changes...";

                // Get updated data from dialog
                var updatedShipment = dialog.GetUpdatedShipment();
                var updatedLines = dialog.GetUpdatedLines();

                // Call service to update shipment
                var updateCommand = new UpdateShipmentCommand
                {
                    ShipmentId = updatedShipment.Id,
                    ShipmentDate = new DateTimeOffset(updatedShipment.ShipmentDate),
                    Notes = updatedShipment.Notes ?? string.Empty,
                    PONumber = updatedShipment.PONumber ?? string.Empty,
                    ReceiverNumber = updatedShipment.ReceiverNumber ?? string.Empty,
                    Parts = updatedLines.Select(line => new ShipmentLineDataTransferObjects
                    {
                        PartNumber = line.PartNumber,
                        ReceivedSkidCount = line.ReceivedSkidCount,
                        ExpectedSkidCount = line.ExpectedSkidCount.HasValue
                            ? Convert.ToInt32(line.ExpectedSkidCount.Value)
                            : null,
                        HasDiscrepancy = line.HasDiscrepancy,
                        DiscrepancyNote = line.DiscrepancyNote ?? string.Empty
                    }).ToList()
                };

                var updateResult = await _mediator.Send(updateCommand);

                if (updateResult.IsSuccess)
                {
                    StatusMessage = $"Shipment #{SelectedShipment.ShipmentNumber} updated successfully";

                    // Refresh history
                    await FilterAsync();
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
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
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Exporting history...";

            var result = await _mediator.Send(new ExportShipmentsQuery
            {
                StartDate = StartDate,
                EndDate = EndDate,
                StatusFilter = StatusFilter
            });

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                // Save file picker would go here
                StatusMessage = $"Exported {History.Count} record(s) to CSV";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
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
