using System;
using System.Collections.Generic;
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
using MTM_Receiving_Application.Module_Receiving.Contracts;
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
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_ReceivingValidation _receivingValidation;
    private readonly IService_Window _windowService;

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
    private ObservableCollection<string> _statusOptions = new()
    {
        VolvoShipmentStatus.AllDisplayName,
        VolvoShipmentStatus.PendingPoDisplayName,
        VolvoShipmentStatus.CompletedDisplayName,
    };

    #endregion

    #region Constructor

    public ViewModel_Volvo_History(
        IMediator mediator,
        IService_InforVisual inforVisualService,
        IService_ReceivingValidation receivingValidation,
        IService_Window windowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void GoBack()
    {
#pragma warning disable CS0618
        var view = App.GetService<Views.View_Volvo_ShipmentEntry>();
#pragma warning restore CS0618
        if (view != null && App.MainWindow is MainWindow mainWindow)
        {
            mainWindow.SetContentPage(view, "Volvo Dunnage Requisition");
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
                ReplaceHistory(result.Data);
                StatusMessage = $"Loaded {History.Count} shipment(s)";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load recent shipments",
                    "Load Error",
                    nameof(LoadRecentShipmentsAsync)
                );
                StatusMessage = "Failed to load recent shipments";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadRecentShipmentsAsync),
                nameof(ViewModel_Volvo_History)
            );
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

            var result = await _mediator.Send(
                new GetShipmentHistoryQuery
                {
                    StartDate = StartDate,
                    EndDate = EndDate,
                    StatusFilter = StatusFilter,
                }
            );

            if (result.IsSuccess && result.Data != null)
            {
                ReplaceHistory(result.Data);
                StatusMessage = $"Loaded {History.Count} shipment(s)";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load history",
                    "Load Error",
                    nameof(FilterAsync)
                );
                StatusMessage = "Failed to load history";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(FilterAsync),
                nameof(ViewModel_Volvo_History)
            );
            StatusMessage = "Error loading history";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplaceHistory(IEnumerable<Model_VolvoShipment> shipments)
    {
        var selectedShipmentId = SelectedShipment?.Id;
        History = new ObservableCollection<Model_VolvoShipment>(shipments);
        SelectedShipment = selectedShipmentId is null
            ? null
            : History.FirstOrDefault(shipment => shipment.Id == selectedShipmentId.Value);
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

            var result = await _mediator.Send(
                new GetShipmentDetailQuery
                {
                    ShipmentId = SelectedShipment.Id,
                    IsArchived = SelectedShipment.IsArchived,
                }
            );

            if (result.IsSuccess && result.Data != null)
            {
                var shipment = result.Data.Shipment;
                var lines = result.Data.Lines;

                var dialogModel = BuildShipmentHistoryDetailDialogModel(shipment, lines);
                var detailDialogShown = await ShowShipmentHistoryDetailDialogAsync(dialogModel);
                if (!detailDialogShown)
                {
                    await _errorHandler.ShowUserErrorAsync(
                        "Cannot show shipment details because the window host is unavailable.",
                        "Shipment Details",
                        nameof(ViewDetailAsync)
                    );
                    return;
                }

                StatusMessage = $"Viewed details for shipment #{shipment.ShipmentNumber}";
                await _logger.LogInfoAsync($"Successfully loaded {lines.Count} lines");
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load shipment details",
                    "Load Error",
                    nameof(ViewDetailAsync)
                );
                StatusMessage = "Failed to load details";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ViewDetailAsync),
                nameof(ViewModel_Volvo_History)
            );
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

            var detailResult = await _mediator.Send(
                new GetShipmentDetailQuery
                {
                    ShipmentId = SelectedShipment.Id,
                    IsArchived = SelectedShipment.IsArchived,
                }
            );

            if (!detailResult.IsSuccess || detailResult.Data == null)
            {
                await _errorHandler.ShowUserErrorAsync(
                    detailResult.ErrorMessage ?? "Failed to load shipment lines",
                    "Load Error",
                    nameof(EditAsync)
                );
                return;
            }

            // Get available parts for the add part dialog
            var partsResult = await _mediator.Send(
                new GetAllVolvoPartsQuery { IncludeInactive = false }
            );

            var availableParts =
                partsResult.IsSuccess && partsResult.Data != null
                    ? new ObservableCollection<Model_VolvoPart>(partsResult.Data)
                    : new ObservableCollection<Model_VolvoPart>();

            // Convert List to ObservableCollection for binding
            var linesCollection = new ObservableCollection<Model_VolvoShipmentLine>(
                detailResult.Data.Lines
            );
            var dialogOutcome = await ShowShipmentEditDialogAsync(
                detailResult.Data.Shipment,
                linesCollection,
                availableParts
            );

            if (dialogOutcome is null)
            {
                await _errorHandler.ShowUserErrorAsync(
                    "Cannot show the shipment edit dialog because the window host is unavailable.",
                    "Edit Shipment",
                    nameof(EditAsync)
                );
                return;
            }

            if (dialogOutcome.Result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                StatusMessage = "Saving changes...";

                // Get updated data from dialog
                var updatedShipment = dialogOutcome.UpdatedShipment;
                var updatedLines = dialogOutcome.UpdatedLines;

                if (updatedShipment is null || updatedLines is null)
                {
                    await _errorHandler.ShowUserErrorAsync(
                        "Shipment changes could not be read from the edit dialog.",
                        "Edit Shipment",
                        nameof(EditAsync)
                    );
                    return;
                }

                // Call service to update shipment
                var updateCommand = new UpdateShipmentCommand
                {
                    ShipmentId = updatedShipment.Id,
                    ShipmentDate = new DateTimeOffset(updatedShipment.ShipmentDate),
                    Notes = updatedShipment.Notes ?? string.Empty,
                    PONumber = updatedShipment.PONumber ?? string.Empty,
                    ReceiverNumber = updatedShipment.ReceiverNumber ?? string.Empty,
                    Parts = updatedLines
                        .Select(line => new ShipmentLineDto
                        {
                            PartNumber = line.PartNumber,
                            Location = line.Location,
                            QuantityPerSkid = line.QuantityPerSkid,
                            ReceivedSkidCount = line.ReceivedSkidCount,
                            PoStatus = VolvoLinePoStatus.NormalizeStorageValue(line.PoStatus),
                            ExpectedSkidCount = line.ExpectedSkidCount.HasValue
                                ? Convert.ToInt32(line.ExpectedSkidCount.Value)
                                : null,
                            HasDiscrepancy = line.HasDiscrepancy,
                            DiscrepancyNote = line.DiscrepancyNote ?? string.Empty,
                        })
                        .ToList(),
                };

                var updateResult = await _mediator.Send(updateCommand);

                if (updateResult.IsSuccess)
                {
                    StatusMessage =
                        $"Shipment #{SelectedShipment.ShipmentNumber} updated successfully";

                    // Refresh history
                    await FilterAsync();
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
                        updateResult.ErrorMessage ?? "Failed to update shipment",
                        "Update Error",
                        nameof(EditAsync)
                    );
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
                nameof(ViewModel_Volvo_History)
            );
            StatusMessage = "Error editing shipment";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanEdit() => SelectedShipment?.IsArchived == false && !IsBusy;

    private async Task<bool> ShowShipmentHistoryDetailDialogAsync(
        Model_VolvoShipmentHistoryDetailDialog dialogModel
    )
    {
        var dispatcherQueue = App.MainWindow?.DispatcherQueue;
        if (dispatcherQueue is null)
        {
            return false;
        }

        var completionSource = new TaskCompletionSource<bool>();
        if (
            !dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var xamlRoot = _windowService.GetXamlRoot();
                    if (xamlRoot is null)
                    {
                        completionSource.SetResult(false);
                        return;
                    }

                    var dialog = new Views.View_Volvo_ShipmentHistoryDetailDialog
                    {
                        XamlRoot = xamlRoot,
                    };
                    dialog.Initialize(dialogModel);
                    await dialog.ShowAsync();
                    completionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            })
        )
        {
            return false;
        }

        return await completionSource.Task;
    }

    private async Task<ShipmentEditDialogOutcome?> ShowShipmentEditDialogAsync(
        Model_VolvoShipment shipment,
        ObservableCollection<Model_VolvoShipmentLine> lines,
        ObservableCollection<Model_VolvoPart> availableParts
    )
    {
        var dispatcherQueue = App.MainWindow?.DispatcherQueue;
        if (dispatcherQueue is null)
        {
            return null;
        }

        var completionSource = new TaskCompletionSource<ShipmentEditDialogOutcome?>();
        if (
            !dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var xamlRoot = _windowService.GetXamlRoot();
                    if (xamlRoot is null)
                    {
                        completionSource.SetResult(null);
                        return;
                    }

                    var dialog = new Views.VolvoShipmentEditDialog(ResolvePartLocationAsync)
                    {
                        XamlRoot = xamlRoot,
                    };
                    dialog.PrepareDialogSize();
                    dialog.LoadShipment(shipment, lines, availableParts);

                    var result = await dialog.ShowAsync();
                    completionSource.SetResult(
                        new ShipmentEditDialogOutcome(
                            result,
                            result == ContentDialogResult.Primary
                                ? dialog.GetUpdatedShipment()
                                : null,
                            result == ContentDialogResult.Primary ? dialog.GetUpdatedLines() : null
                        )
                    );
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            })
        )
        {
            return null;
        }

        return await completionSource.Task;
    }

    private static Model_VolvoShipmentHistoryDetailDialog BuildShipmentHistoryDetailDialogModel(
        Model_VolvoShipment shipment,
        IReadOnlyCollection<Model_VolvoShipmentLine> lines
    )
    {
        var details = new System.Text.StringBuilder();
        details.AppendLine($"Shipment #{shipment.ShipmentNumber}");
        details.AppendLine($"Date: {shipment.ShipmentDate:d}");
        details.AppendLine($"PO Number: {shipment.PONumber ?? "N/A"}");
        details.AppendLine($"Receiver: {shipment.ReceiverNumber ?? "N/A"}");
        details.AppendLine($"Status: {shipment.StatusDisplay}");
        details.AppendLine();
        details.AppendLine($"Parts ({lines.Count}):");

        foreach (var line in lines)
        {
            details.AppendLine(
                $"  • {line.PartNumber}: {line.ReceivedSkidCount} skids ({line.CalculatedPieceCount} pieces)"
            );
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

        return new Model_VolvoShipmentHistoryDetailDialog
        {
            DialogTitle = $"Shipment #{shipment.ShipmentNumber} Details",
            DetailText = details.ToString(),
        };
    }

    private async Task<string> ResolvePartLocationAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return string.Empty;
        }

        if (_receivingValidation.UseMockLocationList)
        {
            var presetLocations = _receivingValidation.PresetLocations;
            if (presetLocations.Count == 0)
            {
                return string.Empty;
            }

            var index =
                Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(partNumber))
                % presetLocations.Count;
            return presetLocations[index];
        }

        var partResult = await _inforVisualService.GetPartByIDAsync(partNumber.Trim());
        if (!partResult.IsSuccess || partResult.Data is null)
        {
            return string.Empty;
        }

        return partResult.Data.DefaultLocationId?.Trim() ?? string.Empty;
    }

    #endregion

    #region Property Changed Handlers

    partial void OnSelectedShipmentChanged(Model_VolvoShipment? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        EditCommand.NotifyCanExecuteChanged();
    }

    private sealed record ShipmentEditDialogOutcome(
        ContentDialogResult Result,
        Model_VolvoShipment? UpdatedShipment,
        ObservableCollection<Model_VolvoShipmentLine>? UpdatedLines
    );

    #endregion
}
