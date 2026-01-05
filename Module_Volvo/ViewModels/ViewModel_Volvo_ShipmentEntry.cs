using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo shipment entry
/// </summary>
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IService_Volvo _volvoService;
    private readonly IService_Window _windowService;

    public ViewModel_Volvo_ShipmentEntry(
        IService_Volvo volvoService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService) : base(errorHandler, logger)
    {
        _volvoService = volvoService;
        _windowService = windowService;
    }

    #region Observable Properties

    [ObservableProperty]
    private DateTimeOffset? _shipmentDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int _shipmentNumber;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipmentLine> _parts = new();

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _availableParts = new();

    [ObservableProperty]
    private Model_VolvoShipmentLine? _selectedPart;

    [ObservableProperty]
    private bool _canSave = false;

    [ObservableProperty]
    private bool _isSuccessMessageVisible = false;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _hasPendingShipment = false;

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading Volvo parts catalog...";

            // Load available parts
            var partsResult = await _volvoService.GetActivePartsAsync();
            if (partsResult.IsSuccess && partsResult.Data != null)
            {
                AvailableParts.Clear();
                foreach (var part in partsResult.Data)
                {
                    AvailableParts.Add(part);
                }
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    partsResult.ErrorMessage ?? "Failed to load parts catalog",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
            }

            // Check for pending shipment
            var pendingResult = await _volvoService.GetPendingShipmentAsync();
            if (pendingResult.IsSuccess && pendingResult.Data != null)
            {
                HasPendingShipment = true;
                // Load pending shipment details if exists
                await LoadPendingShipmentAsync();
            }
            else
            {
                // Add initial empty row for new shipment
                AddPartCommand.Execute(null);
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error initializing Volvo shipment entry",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPendingShipmentAsync()
    {
        var pendingResult = await _volvoService.GetPendingShipmentAsync();
        if (pendingResult.IsSuccess && pendingResult.Data != null)
        {
            var shipment = pendingResult.Data;
            ShipmentDate = new DateTimeOffset(shipment.ShipmentDate);
            ShipmentNumber = shipment.ShipmentNumber;
            Notes = shipment.Notes ?? string.Empty;

            // Load lines
            var linesResult = await _volvoService.GetShipmentLinesAsync(shipment.Id);
            if (linesResult.IsSuccess && linesResult.Data != null)
            {
                Parts.Clear();
                foreach (var line in linesResult.Data)
                {
                    Parts.Add(line);
                }
            }
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddPart()
    {
        var newLine = new Model_VolvoShipmentLine
        {
            PartNumber = string.Empty,
            ReceivedSkidCount = 0,
            CalculatedPieceCount = 0,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = string.Empty
        };
        Parts.Add(newLine);
        ValidateSaveEligibility();
    }

    [RelayCommand]
    private void RemovePart()
    {
        if (SelectedPart != null)
        {
            Parts.Remove(SelectedPart);
            ValidateSaveEligibility();
        }
    }

    [RelayCommand]
    private async Task GenerateLabelsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Generating labels...";

            // First, save the shipment if not already saved
            if (!HasPendingShipment)
            {
                var saveResult = await SaveShipmentInternalAsync();
                if (!saveResult.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        saveResult.ErrorMessage ?? "Failed to save shipment before generating labels",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true);
                    return;
                }
            }

            // Get the pending shipment ID
            var pendingResult = await _volvoService.GetPendingShipmentAsync();
            if (!pendingResult.IsSuccess || pendingResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    "No pending shipment found",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            // Generate CSV
            var csvResult = await _volvoService.GenerateLabelCsvAsync(pendingResult.Data.Id);
            if (csvResult.IsSuccess)
            {
                SuccessMessage = $"Labels generated successfully!\nFile: {csvResult.Data}";
                IsSuccessMessageVisible = true;
                StatusMessage = "Labels generated";
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "Failed to generate labels",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error generating labels",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PreviewEmailAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Formatting email...";

            // Build shipment object
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = ShipmentDate?.DateTime ?? DateTime.Today,
                ShipmentNumber = ShipmentNumber,
                Notes = Notes,
                Status = "pending_po"
            };

            // Format email
            var emailText = await _volvoService.FormatEmailTextAsync(shipment, Parts.ToList(), new Dictionary<string, int>());

            // Show email preview dialog
            await ShowEmailPreviewDialogAsync(emailText ?? string.Empty);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error previewing email",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowEmailPreviewDialogAsync(string emailText)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            await _errorHandler.HandleErrorAsync(
                "Cannot show dialog - XamlRoot not available",
                Enum_ErrorSeverity.Low,
                null,
                true);
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "PO Requisition Email Preview",
            PrimaryButtonText = "Copy to Clipboard",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var textBox = new TextBox
        {
            Text = emailText,
            IsReadOnly = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 500,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
        };

        var scrollViewer = new ScrollViewer
        {
            Content = textBox,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        dialog.Content = scrollViewer;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Copy to clipboard
            var dataPackage = new DataPackage();
            dataPackage.SetText(emailText);
            Clipboard.SetContent(dataPackage);

            SuccessMessage = "Email text copied to clipboard!";
            IsSuccessMessageVisible = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsPendingAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving shipment...";

            var result = await SaveShipmentInternalAsync();
            if (result.IsSuccess)
            {
                SuccessMessage = $"Shipment #{result.Data.ShipmentNumber} saved as pending PO";
                IsSuccessMessageVisible = true;
                HasPendingShipment = true;
                StatusMessage = "Shipment saved";
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage ?? "Failed to save shipment",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentInternalAsync()
    {
        // Validate
        if (!ValidateShipment())
        {
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>("Shipment validation failed");
        }

        // Build shipment object
        var shipment = new Model_VolvoShipment
        {
            ShipmentDate = ShipmentDate?.DateTime ?? DateTime.Today,
            ShipmentNumber = ShipmentNumber,
            Notes = Notes,
            Status = "pending_po",
            EmployeeNumber = string.Empty // TODO: Get from session
        };

        // Save
        return await _volvoService.SaveShipmentAsync(shipment, Parts.ToList());
    }

    [RelayCommand]
    private async Task CompleteShipmentAsync()
    {
        // This will be used when completing a pending shipment with PO/Receiver numbers
        try
        {
            IsBusy = true;
            StatusMessage = "Completing shipment...";

            var pendingResult = await _volvoService.GetPendingShipmentAsync();
            if (!pendingResult.IsSuccess || pendingResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    "No pending shipment found",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            // Show completion dialog
            await ShowCompletionDialogAsync(pendingResult.Data);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error completing shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowCompletionDialogAsync(Model_VolvoShipment shipment)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = $"Complete Shipment #{shipment.ShipmentNumber}",
            PrimaryButtonText = "Complete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 12 };

        var poTextBox = new TextBox
        {
            Header = "PO Number",
            PlaceholderText = "Enter PO number (e.g., PO-062450)"
        };

        var receiverTextBox = new TextBox
        {
            Header = "Receiver Number",
            PlaceholderText = "Enter receiver number (e.g., 134393)"
        };

        stackPanel.Children.Add(poTextBox);
        stackPanel.Children.Add(receiverTextBox);
        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (string.IsNullOrWhiteSpace(poTextBox.Text) || string.IsNullOrWhiteSpace(receiverTextBox.Text))
            {
                await _errorHandler.HandleErrorAsync(
                    "PO Number and Receiver Number are required",
                    Enum_ErrorSeverity.Low,
                    null,
                    true);
                return;
            }

            var completeResult = await _volvoService.CompleteShipmentAsync(
                shipment.Id,
                poTextBox.Text.Trim(),
                receiverTextBox.Text.Trim());

            if (completeResult.IsSuccess)
            {
                SuccessMessage = "Shipment completed successfully!";
                IsSuccessMessageVisible = true;
                HasPendingShipment = false;

                // Clear the form
                Parts.Clear();
                Notes = string.Empty;
                ShipmentNumber = 0;
                AddPartCommand.Execute(null);
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    completeResult.ErrorMessage ?? "Failed to complete shipment",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
            }
        }
    }

    [RelayCommand]
    private void StartNewEntry()
    {
        IsSuccessMessageVisible = false;
        SuccessMessage = string.Empty;
        Parts.Clear();
        Notes = string.Empty;
        ShipmentNumber = 0;
        HasPendingShipment = false;
        AddPartCommand.Execute(null);
    }

    #endregion

    #region Validation

    private bool ValidateShipment()
    {
        if (Parts.Count == 0)
        {
            _errorHandler.HandleErrorAsync(
                "At least one part is required",
                Enum_ErrorSeverity.Low,
                null,
                true).ConfigureAwait(false);
            return false;
        }

        foreach (var part in Parts)
        {
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                _errorHandler.HandleErrorAsync(
                    "All parts must have a part number selected",
                    Enum_ErrorSeverity.Low,
                    null,
                    true).ConfigureAwait(false);
                return false;
            }

            if (part.ReceivedSkidCount <= 0)
            {
                _errorHandler.HandleErrorAsync(
                    $"Part {part.PartNumber} must have at least 1 skid",
                    Enum_ErrorSeverity.Low,
                    null,
                    true).ConfigureAwait(false);
                return false;
            }

            if (part.HasDiscrepancy && !part.ExpectedSkidCount.HasValue)
            {
                _errorHandler.HandleErrorAsync(
                    $"Part {part.PartNumber} has discrepancy but no expected skid count",
                    Enum_ErrorSeverity.Low,
                    null,
                    true).ConfigureAwait(false);
                return false;
            }
        }

        return true;
    }

    private void ValidateSaveEligibility()
    {
        CanSave = Parts.Count > 0 && Parts.All(p => !string.IsNullOrWhiteSpace(p.PartNumber) && p.ReceivedSkidCount > 0);
    }

    #endregion

    #region Event Handlers

    partial void OnPartsChanged(ObservableCollection<Model_VolvoShipmentLine> value)
    {
        ValidateSaveEligibility();
    }

    #endregion
}
