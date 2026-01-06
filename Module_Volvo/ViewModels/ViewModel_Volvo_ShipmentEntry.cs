using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo shipment entry
/// </summary>
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IService_Volvo _volvoService;
    private readonly IService_Window _windowService;
    private readonly IService_UserSessionManager _sessionManager;

    public ViewModel_Volvo_ShipmentEntry(
        IService_Volvo volvoService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_UserSessionManager sessionManager) : base(errorHandler, logger)
    {
        _volvoService = volvoService;
        _windowService = windowService;
        _sessionManager = sessionManager;
    }

    #region Observable Properties

    [ObservableProperty]
    private DateTimeOffset? _shipmentDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int _shipmentNumber = 1;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipmentLine> _parts = new();

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _availableParts = new();

    private List<Model_VolvoPart> _allParts = new();

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _suggestedParts = new();

    [ObservableProperty]
    private Model_VolvoShipmentLine? _selectedPart;

    [ObservableProperty]
    private Model_VolvoPart? _selectedPartToAdd;

    [ObservableProperty]
    private string _partSearchText = string.Empty;

    [ObservableProperty]
    private int _receivedSkidsToAdd = 0;

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
                _allParts = partsResult.Data;
                AvailableParts.Clear();
                foreach (var part in _allParts)
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

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error initializing Volvo shipment entry: {ex.Message}", ex);
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

    #region AutoSuggestBox Support

    /// <summary>
    /// Updates the part suggestions list based on user's search text
    /// Filters parts by case-insensitive substring match on part number
    /// Optimized to reduce collection change notifications
    /// </summary>
    /// <param name="queryText">Search text from AutoSuggestBox</param>
    public void UpdatePartSuggestions(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            SuggestedParts.Clear();
            SelectedPartToAdd = null;
            return;
        }

        var suggestions = _allParts
            .Where(p => p.PartNumber.Contains(queryText, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .ToList();

        // Optimize: Use bulk replace to reduce collection change events
        // Clear and add all at once instead of individual adds
        var tempList = new List<Model_VolvoPart>(suggestions);
        SuggestedParts.Clear();
        foreach (var part in tempList)
        {
            SuggestedParts.Add(part);
        }

        // Check if text matches a part exactly
        var exactMatch = _allParts.FirstOrDefault(p =>
            p.PartNumber.Equals(queryText, StringComparison.OrdinalIgnoreCase));
        SelectedPartToAdd = exactMatch;
    }

    /// <summary>
    /// Handles when user selects a part from the AutoSuggestBox dropdown
    /// Sets the search text and selected part for adding to shipment
    /// </summary>
    /// <param name="chosenPart">The part selected from suggestions</param>
    public void OnPartSuggestionChosen(Model_VolvoPart? chosenPart)
    {
        if (chosenPart != null)
        {
            PartSearchText = chosenPart.PartNumber;
            SelectedPartToAdd = chosenPart;
        }
    }

    partial void OnPartSearchTextChanged(string value)
    {
        UpdatePartSuggestions(value);
    }

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(CanAddPart))]
    private async void AddPart()
    {
        if (SelectedPartToAdd == null || ReceivedSkidsToAdd <= 0)
        {
            return;
        }

        // Input validation (1-99 range)
        if (ReceivedSkidsToAdd < 1 || ReceivedSkidsToAdd > 99)
        {
            await _errorHandler.HandleErrorAsync(
                "Received skid count must be between 1 and 99",
                Enum_ErrorSeverity.Low,
                null,
                true);
            return;
        }

        // Check for duplicate part number
        if (Parts.Any(p => p.PartNumber.Equals(SelectedPartToAdd.PartNumber, StringComparison.OrdinalIgnoreCase)))
        {
            await _errorHandler.HandleErrorAsync(
                $"Part {SelectedPartToAdd.PartNumber} is already in this shipment. Remove it first if you want to update the quantity.",
                Enum_ErrorSeverity.Low,
                null,
                true);
            return;
        }

        // Calculate pieces based on quantity per skid
        var calculatedPieces = SelectedPartToAdd.QuantityPerSkid * ReceivedSkidsToAdd;

        var newLine = new Model_VolvoShipmentLine
        {
            PartNumber = SelectedPartToAdd.PartNumber,
            QuantityPerSkid = SelectedPartToAdd.QuantityPerSkid,
            ReceivedSkidCount = ReceivedSkidsToAdd,
            CalculatedPieceCount = calculatedPieces,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = string.Empty
        };

        Parts.Add(newLine);

        // Log user action
        await _logger.LogInfoAsync($"User added part {SelectedPartToAdd.PartNumber}, {ReceivedSkidsToAdd} skids ({calculatedPieces} pcs)");

        // Reset input fields
        SelectedPartToAdd = null;
        ReceivedSkidsToAdd = 0;
        PartSearchText = string.Empty;
        SuggestedParts.Clear();

        ValidateSaveEligibility();
    }

    private bool CanAddPart()
    {
        return SelectedPartToAdd != null && ReceivedSkidsToAdd >= 1 && ReceivedSkidsToAdd <= 99;
    }

    [RelayCommand]
    private async void RemovePart()
    {
        if (SelectedPart != null)
        {
            await _logger.LogInfoAsync($"User removed part {SelectedPart.PartNumber} from shipment");
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
            await _logger.LogErrorAsync($"Error generating labels: {ex.Message}", ex);
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

            // First, ensure the shipment is saved
            if (!HasPendingShipment)
            {
                var saveResult = await SaveShipmentInternalAsync();
                if (!saveResult.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        saveResult.ErrorMessage ?? "Failed to save shipment before preview",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true);
                    return;
                }
            }

            // Get the saved pending shipment
            var pendingResult = await _volvoService.GetPendingShipmentWithLinesAsync();
            if (!pendingResult.IsSuccess || pendingResult.Data.Shipment == null)
            {
                await _errorHandler.HandleErrorAsync(
                    "No pending shipment found",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            var shipment = pendingResult.Data.Shipment;
            var lines = pendingResult.Data.Lines;

            // Calculate component explosion to get actual part quantities
            var explosionResult = await _volvoService.CalculateComponentExplosionAsync(lines);

            Dictionary<string, int> requestedLines;
            if (explosionResult.IsSuccess && explosionResult.Data != null)
            {
                requestedLines = explosionResult.Data;
                await _logger.LogInfoAsync($"Component explosion successful: {requestedLines.Count} parts");
            }
            else
            {
                // Fallback to empty if explosion fails
                requestedLines = new Dictionary<string, int>();
                await _logger.LogWarningAsync($"Component explosion failed for email preview: {explosionResult.ErrorMessage}");
            }

            await _logger.LogInfoAsync($"Email preview - RequestedLines count: {requestedLines.Count}");

            // Format email data with actual part quantities
            var emailData = await _volvoService.FormatEmailDataAsync(shipment, lines, requestedLines);

            await _logger.LogInfoAsync($"Email data formatted - RequestedLines count: {emailData.RequestedLines.Count}");

            // Show email preview dialog
            await ShowEmailPreviewDialogAsync(emailData);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error previewing email: {ex.Message}", ex);
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

    private async Task ShowEmailPreviewDialogAsync(Model_VolvoEmailData emailData)
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

        // Load email recipients from settings
        var settingsDao = new Data.Dao_VolvoSettings(Module_Core.Helpers.Database.Helper_Database_Variables.GetConnectionString());
        var toResult = await settingsDao.GetSettingAsync("email_to_recipients");
        var ccResult = await settingsDao.GetSettingAsync("email_cc_recipients");

        string toRecipients = FormatRecipientsFromJson(
            toResult.IsSuccess && toResult.Data != null ? toResult.Data.SettingValue : null,
            "\"Jose Rosas\" <jrosas@mantoolmfg.com>; \"Sandy Miller\" <smiller@mantoolmfg.com>; \"Steph Wittmus\" <swittmus@mantoolmfg.com>"
        );

        string ccRecipients = FormatRecipientsFromJson(
            ccResult.IsSuccess && ccResult.Data != null ? ccResult.Data.SettingValue : null,
            "\"Debra Alexander\" <dalexander@mantoolmfg.com>; \"Michelle Laurin\" <mlaurin@mantoolmfg.com>"
        );

        var dialog = new ContentDialog
        {
            Title = "PO Requisition Email Preview",
            PrimaryButtonText = "Copy Email Body",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
            MaxWidth = 800
        };

        // Build structured preview UI
        var mainStack = new StackPanel { Spacing = 12, Margin = new Microsoft.UI.Xaml.Thickness(0) };

        // TO Recipients
        var toPanel = new Grid { ColumnSpacing = 8 };
        toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
        toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
        var toBox = new TextBox
        {
            Header = "To:",
            Text = toRecipients,
            IsReadOnly = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            MaxHeight = 80
        };
        toBox.SetValue(Grid.ColumnProperty, 0);
        var toCopyButton = new Button
        {
            Content = "📋",
            Width = 40,
            Height = 40,
            FontSize = 16,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 4)
        };
        toCopyButton.SetValue(Grid.ColumnProperty, 1);
        Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(toCopyButton, "Copy To Recipients");
        toCopyButton.Click += (s, e) =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(toBox.Text);
            Clipboard.SetContent(dataPackage);
        };
        toPanel.Children.Add(toBox);
        toPanel.Children.Add(toCopyButton);
        mainStack.Children.Add(toPanel);

        // CC Recipients
        var ccPanel = new Grid { ColumnSpacing = 8 };
        ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
        ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
        var ccBox = new TextBox
        {
            Header = "Cc:",
            Text = ccRecipients,
            IsReadOnly = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            MaxHeight = 80
        };
        ccBox.SetValue(Grid.ColumnProperty, 0);
        var ccCopyButton = new Button
        {
            Content = "📋",
            Width = 40,
            Height = 40,
            FontSize = 16,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 4)
        };
        ccCopyButton.SetValue(Grid.ColumnProperty, 1);
        Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(ccCopyButton, "Copy CC Recipients");
        ccCopyButton.Click += (s, e) =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ccBox.Text);
            Clipboard.SetContent(dataPackage);
        };
        ccPanel.Children.Add(ccBox);
        ccPanel.Children.Add(ccCopyButton);
        mainStack.Children.Add(ccPanel);

        // Subject
        var subjectPanel = new Grid { ColumnSpacing = 8 };
        subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
        subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
        var subjectBox = new TextBox
        {
            Header = "Subject:",
            Text = emailData.Subject,
            IsReadOnly = true,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
        };
        subjectBox.SetValue(Grid.ColumnProperty, 0);
        var subjectCopyButton = new Button
        {
            Content = "📋",
            Width = 40,
            Height = 40,
            FontSize = 16,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 4)
        };
        subjectCopyButton.SetValue(Grid.ColumnProperty, 1);
        Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(subjectCopyButton, "Copy Subject");
        subjectCopyButton.Click += (s, e) =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(subjectBox.Text);
            Clipboard.SetContent(dataPackage);
        };
        subjectPanel.Children.Add(subjectBox);
        subjectPanel.Children.Add(subjectCopyButton);
        mainStack.Children.Add(subjectPanel);

        // Discrepancies (if any)
        if (emailData.Discrepancies.Count > 0)
        {
            var discHeader = new TextBlock
            {
                Text = "**DISCREPANCIES NOTED**",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 4)
            };
            mainStack.Children.Add(discHeader);

            var discBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                AcceptsReturn = true,
                MinHeight = 120,
                MaxHeight = 250
            };
            var discText = new StringBuilder();
            discText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
            discText.AppendLine(new string('-', 80));
            foreach (var disc in emailData.Discrepancies)
            {
                string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                discText.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
            }
            discBox.Text = discText.ToString();
            mainStack.Children.Add(discBox);
        }

        // Requested Lines
        var reqHeader = new TextBlock
        {
            Text = "Requested Lines:",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 4)
        };
        mainStack.Children.Add(reqHeader);

        var reqBox = new TextBox
        {
            IsReadOnly = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
            AcceptsReturn = true,
            MinHeight = 150,
            MaxHeight = 300
        };
        var reqText = new StringBuilder();
        reqText.AppendLine("Part Number\tQuantity (pcs)");
        reqText.AppendLine(new string('-', 40));
        foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
        {
            reqText.AppendLine($"{kvp.Key}\t{kvp.Value}");
        }
        reqBox.Text = reqText.ToString();
        mainStack.Children.Add(reqBox);

        // Additional Notes
        if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
        {
            var notesBox = new TextBox
            {
                Header = "Additional Notes:",
                Text = emailData.AdditionalNotes,
                IsReadOnly = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 60
            };
            mainStack.Children.Add(notesBox);
        }

        var scrollViewer = new ScrollViewer
        {
            Content = mainStack,
            Height = 500,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        dialog.Content = scrollViewer;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Copy HTML to clipboard for Outlook paste as table
            var htmlContent = _volvoService.FormatEmailAsHtml(emailData);
            var dataPackage = new DataPackage();
            dataPackage.SetHtmlFormat(htmlContent);
            // Also include plain text fallback
            var plainText = BuildPlainTextEmail(emailData);
            dataPackage.SetText(plainText);
            Clipboard.SetContent(dataPackage);

            SuccessMessage = "Email copied to clipboard (paste into Outlook as formatted table)!";
            IsSuccessMessageVisible = true;
        }
    }

    private string BuildPlainTextEmail(Model_VolvoEmailData emailData)
    {
        var text = new StringBuilder();
        text.AppendLine($"Subject: {emailData.Subject}");
        text.AppendLine();
        text.AppendLine(emailData.Greeting);
        text.AppendLine();
        text.AppendLine(emailData.Message);
        text.AppendLine();

        if (emailData.Discrepancies.Count > 0)
        {
            text.AppendLine("**DISCREPANCIES NOTED**");
            text.AppendLine();
            text.AppendLine("Part Number\tPacklist Qty\tReceived Qty\tDifference\tNote");
            text.AppendLine(new string('-', 80));
            foreach (var disc in emailData.Discrepancies)
            {
                string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                text.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
            }
            text.AppendLine();
        }

        text.AppendLine("Requested Lines:");
        text.AppendLine();
        text.AppendLine("Part Number\tQuantity (pcs)");
        text.AppendLine(new string('-', 40));
        foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
        {
            text.AppendLine($"{kvp.Key}\t{kvp.Value}");
        }
        text.AppendLine();

        if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
        {
            text.AppendLine("Additional Notes:");
            text.AppendLine(emailData.AdditionalNotes);
            text.AppendLine();
        }

        text.AppendLine(emailData.Signature);
        return text.ToString();
    }

    [RelayCommand]
    private void ViewHistory()
    {
        _logger.LogInfo("Navigating to Volvo Shipment History");
        if (App.MainWindow is MainWindow mainWindow)
        {
            var contentFrame = mainWindow.GetContentFrame();
            contentFrame?.Navigate(typeof(Views.View_Volvo_History));
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
            await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
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
            Status = VolvoShipmentStatus.PendingPo,
            EmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber.ToString() ?? "0"
        };

        // Save
        var saveResult = await _volvoService.SaveShipmentAsync(shipment, Parts.ToList());

        // Check if there's an existing pending shipment
        if (!saveResult.IsSuccess && saveResult.ErrorMessage.StartsWith("PENDING_EXISTS"))
        {
            var parts = saveResult.ErrorMessage.Split('|');
            var existingShipmentNumber = parts.Length > 1 ? parts[1] : "Unknown";
            var existingShipmentDate = parts.Length > 2 ? parts[2] : "Unknown Date";

            // Show confirmation dialog
            var dialog = new ContentDialog
            {
                Title = "Overwrite Pending Shipment?",
                Content = $"A pending shipment already exists (Shipment #{existingShipmentNumber} from {existingShipmentDate}).{Environment.NewLine}{Environment.NewLine}Do you want to delete it and create a new one?",
                PrimaryButtonText = "Overwrite",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // User confirmed - delete existing and save new
                await _logger.LogInfoAsync($"User confirmed overwrite of pending shipment #{existingShipmentNumber}");
                return await _volvoService.SaveShipmentAsync(shipment, Parts.ToList(), overwriteExisting: true);
            }
            else
            {
                // User canceled
                await _logger.LogInfoAsync("User canceled shipment overwrite");
                return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>("Shipment save canceled by user");
            }
        }

        return saveResult;
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
            await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
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
                ClearShipmentForm();
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
        HasPendingShipment = false;
        ClearShipmentForm();
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

    /// <summary>
    /// Formats JSON array of recipients into Outlook format
    /// </summary>
    private string FormatRecipientsFromJson(string? jsonValue, string fallbackValue)
    {
        if (string.IsNullOrWhiteSpace(jsonValue))
        {
            return fallbackValue;
        }

        try
        {
            var recipients = System.Text.Json.JsonSerializer.Deserialize<List<Models.Model_EmailRecipient>>(jsonValue);
            if (recipients == null || recipients.Count == 0)
            {
                return fallbackValue;
            }

            return string.Join("; ", recipients.Select(r => r.ToOutlookFormat()));
        }
        catch (Exception ex)
        {
            _logger.LogErrorAsync($"Error parsing email recipients JSON: {ex.Message}", ex).ConfigureAwait(false);
            return fallbackValue;
        }
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

    partial void OnSelectedPartToAddChanged(Model_VolvoPart? value)
    {
        AddPartCommand.NotifyCanExecuteChanged();
    }

    partial void OnReceivedSkidsToAddChanged(int value)
    {
        AddPartCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Clears all shipment form fields and resets to default state
    /// Extracted to avoid code duplication
    /// </summary>
    private void ClearShipmentForm()
    {
        Parts.Clear();
        Notes = string.Empty;
        ShipmentNumber = 1;
        SelectedPartToAdd = null;
        ReceivedSkidsToAdd = 0;
        PartSearchText = string.Empty;
        SuggestedParts.Clear();
    }

    #endregion
}
