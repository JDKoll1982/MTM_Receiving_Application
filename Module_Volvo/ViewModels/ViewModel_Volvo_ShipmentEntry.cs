using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo shipment entry (CQRS-enabled with MediatR)
/// </summary>
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    private readonly IService_Window _windowService;
    private readonly IService_UserSessionManager _sessionManager;

    // Public accessors for code-behind dialog
    public IService_ErrorHandler ErrorHandler => _errorHandler;
    public IService_LoggingUtility Logger => _logger;

    public ViewModel_Volvo_ShipmentEntry(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_UserSessionManager sessionManager,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
    private string _receivedSkidsToAdd = string.Empty;

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

    /// <summary>
    /// Initializes ViewModel - loads initial data and checks for pending shipment (CQRS)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Initializing shipment entry...";

            // Get initial shipment data (current date + next shipment number)
            var initialDataQuery = new GetInitialShipmentDataQuery();
            var initialDataResult = await _mediator.Send(initialDataQuery);

            if (initialDataResult.IsSuccess && initialDataResult.Data != null)
            {
                ShipmentDate = initialDataResult.Data.CurrentDate;
                ShipmentNumber = initialDataResult.Data.NextShipmentNumber;
                await _logger.LogInfoAsync($"Initialized with shipment number: {ShipmentNumber}");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    initialDataResult.ErrorMessage ?? "Failed to get initial shipment data",
                    Enum_ErrorSeverity.Medium);
            }

            // Load part master data for quantity-per-skid cache
            await LoadAllPartsAsync();

            // Check for any pending shipment (CQRS)
            var pendingQuery = new GetPendingShipmentQuery();
            // Legacy save method removed after CQRS migration completion.
            var pendingResult = await _mediator.Send(pendingQuery);

            if (pendingResult.IsSuccess && pendingResult.Data != null)
            {
                HasPendingShipment = true;
                await LoadPendingShipmentAsync(pendingResult.Data.Id);
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

    /// <summary>
    /// Loads pending shipment data (CQRS)
    /// </summary>
    private async Task LoadPendingShipmentAsync(int shipmentId)
    {
        try
        {
            // Get shipment detail using MediatR
            var detailQuery = new GetShipmentDetailQuery { ShipmentId = shipmentId };
            var detailResult = await _mediator.Send(detailQuery);

            if (detailResult.IsSuccess && detailResult.Data != null)
            {
                var shipment = detailResult.Data.Shipment;
                ShipmentDate = new DateTimeOffset(shipment.ShipmentDate);
                ShipmentNumber = shipment.ShipmentNumber;
                Notes = shipment.Notes ?? string.Empty;

                // Load lines
                Parts.Clear();
                foreach (var line in detailResult.Data.Lines)
                {
                    Parts.Add(line);
                }

                ApplyCachedQuantitiesToLines();

                await _logger.LogInfoAsync($"Loaded pending shipment #{shipment.ShipmentNumber} with {Parts.Count} parts");
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error loading pending shipment: {ex.Message}", ex);
        }
    }

    private async Task LoadAllPartsAsync()
    {
        try
        {
            var partsResult = await _mediator.Send(new GetAllVolvoPartsQuery
            {
                IncludeInactive = true
            });

            if (partsResult.IsSuccess && partsResult.Data != null)
            {
                _allParts = partsResult.Data;
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error loading Volvo parts cache: {ex.Message}", ex);
        }
    }

    private void ApplyCachedQuantitiesToLines()
    {
        if (_allParts == null || _allParts.Count == 0)
        {
            return;
        }

        var partsByNumber = _allParts.ToDictionary(p => p.PartNumber, StringComparer.OrdinalIgnoreCase);

        foreach (var line in Parts)
        {
            if (line.QuantityPerSkid <= 0 &&
                !string.IsNullOrWhiteSpace(line.PartNumber) &&
                partsByNumber.TryGetValue(line.PartNumber, out var part))
            {
                line.QuantityPerSkid = part.QuantityPerSkid;
            }

            if (line.CalculatedPieceCount <= 0 && line.QuantityPerSkid > 0)
            {
                line.CalculatedPieceCount = line.QuantityPerSkid * line.ReceivedSkidCount;
            }
        }
    }

    #endregion

    #region AutoSuggestBox Support

    /// <summary>
    /// Loads all parts for the Add Part dialog
    /// </summary>
    public async Task LoadAllPartsForDialogAsync()
    {
        try
        {
            var partsResult = await _mediator.Send(new GetAllVolvoPartsQuery
            {
                IncludeInactive = false // Only show active parts in dialog
            });

            if (partsResult.IsSuccess && partsResult.Data != null)
            {
                AvailableParts.Clear();
                foreach (var part in partsResult.Data.OrderBy(p => p.PartNumber))
                {
                    AvailableParts.Add(part);
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error loading parts for dialog: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates the part suggestions list based on user's search text (CQRS)
    /// Uses SearchVolvoPartsQuery for autocomplete functionality
    /// </summary>
    /// <param name="queryText">Search text from AutoSuggestBox</param>
    public async void UpdatePartSuggestions(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            SuggestedParts.Clear();
            SelectedPartToAdd = null;
            return;
        }

        try
        {
            // Use MediatR query for search
            var searchQuery = new SearchVolvoPartsQuery
            {
                SearchText = queryText,
                MaxResults = 20
            };

            var searchResult = await _mediator.Send(searchQuery);

            if (searchResult.IsSuccess && searchResult.Data != null)
            {
                // Update suggestions collection
                SuggestedParts.Clear();
                foreach (var part in searchResult.Data)
                {
                    SuggestedParts.Add(part);
                }

                // Check if text matches a part exactly
                var exactMatch = searchResult.Data.FirstOrDefault(p =>
                    p.PartNumber.Equals(queryText, StringComparison.OrdinalIgnoreCase));
                SelectedPartToAdd = exactMatch;
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error searching parts: {ex.Message}", ex);
        }
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

    /// <summary>
    /// Adds part to shipment using AddPartToShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddPart))]
    private async void AddPart()
    {
        if (SelectedPartToAdd == null || string.IsNullOrWhiteSpace(ReceivedSkidsToAdd))
        {
            return;
        }

        try
        {
            // Parse and validate skid count
            if (!int.TryParse(ReceivedSkidsToAdd, out int skidCount) || skidCount < 1 || skidCount > 99)
            {
                await _errorHandler.HandleErrorAsync(
                    "Received skid count must be a number between 1 and 99",
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

            // Validate part using MediatR command (checks part exists in master data)
            var addCommand = new AddPartToShipmentCommand
            {
                PartNumber = SelectedPartToAdd.PartNumber,
                ReceivedSkidCount = skidCount,
                HasDiscrepancy = false
            };

            var validationResult = await _mediator.Send(addCommand);

            if (!validationResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    validationResult.ErrorMessage ?? "Failed to validate part",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            // Validation passed - add to ObservableCollection
            var calculatedPieces = SelectedPartToAdd.QuantityPerSkid * skidCount;

            var newLine = new Model_VolvoShipmentLine
            {
                PartNumber = SelectedPartToAdd.PartNumber,
                QuantityPerSkid = SelectedPartToAdd.QuantityPerSkid,
                ReceivedSkidCount = skidCount,
                CalculatedPieceCount = calculatedPieces,
                HasDiscrepancy = false,
                ExpectedSkidCount = null,
                DiscrepancyNote = string.Empty
            };

            Parts.Add(newLine);

            // Log user action
            await _logger.LogInfoAsync($"User added part {SelectedPartToAdd.PartNumber}, {skidCount} skids ({calculatedPieces} pcs)");

            // Reset input fields
            SelectedPartToAdd = null;
            ReceivedSkidsToAdd = string.Empty;
            PartSearchText = string.Empty;
            SuggestedParts.Clear();

            ValidateSaveEligibility();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error adding part: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error adding part to shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
    }

    private bool CanAddPart()
    {
        return SelectedPartToAdd != null &&
               !string.IsNullOrWhiteSpace(ReceivedSkidsToAdd) &&
               int.TryParse(ReceivedSkidsToAdd, out int count) &&
               count >= 1 &&
               count <= 99;
    }

    /// <summary>
    /// Removes part from shipment using RemovePartFromShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand]
    private async void RemovePart()
    {
        if (SelectedPart == null)
        {
            return;
        }

        try
        {
            // Validate removal using MediatR command
            var removeCommand = new RemovePartFromShipmentCommand
            {
                PartNumber = SelectedPart.PartNumber
            };

            var validationResult = await _mediator.Send(removeCommand);

            if (!validationResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    validationResult.ErrorMessage ?? "Failed to validate part removal",
                    Enum_ErrorSeverity.Low,
                    null,
                    true);
                return;
            }

            // Validation passed - remove from ObservableCollection
            await _logger.LogInfoAsync($"User removed part {SelectedPart.PartNumber} from shipment");
            Parts.Remove(SelectedPart);
            ValidateSaveEligibility();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error removing part: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error removing part from shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
    }

    /// <summary>
    /// Generates CSV labels using GenerateLabelCsvQuery (CQRS)
    /// </summary>
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

            // Get the pending shipment ID using MediatR
            var pendingQuery = new GetPendingShipmentQuery
            {
                UserName = Environment.UserName
            };
            var pendingResult = await _mediator.Send(pendingQuery);

            if (!pendingResult.IsSuccess || pendingResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    "No pending shipment found",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            // Generate CSV using MediatR query
            var csvQuery = new GenerateLabelCsvQuery
            {
                ShipmentId = pendingResult.Data.Id
            };
            var csvResult = await _mediator.Send(csvQuery);

            if (csvResult.IsSuccess)
            {
                SuccessMessage = $"Labels generated successfully!\nFile: {csvResult.Data}";
                IsSuccessMessageVisible = true;
                StatusMessage = "Labels generated";
                await _logger.LogInfoAsync($"Labels generated for shipment ID: {pendingResult.Data.Id}, File: {csvResult.Data}");
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

            int shipmentId;

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

                shipmentId = saveResult.Data.ShipmentId;
                HasPendingShipment = true;
            }
            else
            {
                var pendingResult = await _mediator.Send(new GetPendingShipmentQuery
                {
                    UserName = Environment.UserName
                });

                if (!pendingResult.IsSuccess || pendingResult.Data == null)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No pending shipment found",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true);
                    return;
                }

                shipmentId = pendingResult.Data.Id;
            }

            var emailResult = await _mediator.Send(new FormatEmailDataQuery
            {
                ShipmentId = shipmentId
            });

            if (!emailResult.IsSuccess || emailResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    emailResult.ErrorMessage ?? "Failed to format email data",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true);
                return;
            }

            await ShowEmailPreviewDialogAsync(emailResult.Data);
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

        // Load email recipients from settings via Mediator
        var toResult = await _mediator.Send(new GetVolvoSettingQuery("email_to_recipients"));
        var ccResult = await _mediator.Send(new GetVolvoSettingQuery("email_cc_recipients"));

        string toRecipients = FormatRecipientsFromJson(
            toResult.IsSuccess && toResult.Data != null ? toResult.Data : null,
            "\"Jose Rosas\" <jrosas@mantoolmfg.com>; \"Sandy Miller\" <smiller@mantoolmfg.com>; \"Steph Wittmus\" <swittmus@mantoolmfg.com>"
        );

        string ccRecipients = FormatRecipientsFromJson(
            ccResult.IsSuccess && ccResult.Data != null ? ccResult.Data : null,
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
        toCopyButton.Click += (_, _) =>
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
        ccCopyButton.Click += (_, _) =>
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
        subjectCopyButton.Click += (_, _) =>
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
            var htmlContent = FormatEmailAsHtml(emailData);
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

    private static string FormatEmailAsHtml(Model_VolvoEmailData emailData)
    {
        var html = new StringBuilder();

        html.AppendLine("<html>");
        html.AppendLine("<body style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");

        html.AppendLine($"<p>{emailData.Greeting}</p>");
        html.AppendLine($"<p>{emailData.Message}</p>");

        if (emailData.Discrepancies.Count > 0)
        {
            html.AppendLine("<p><strong>**DISCREPANCIES NOTED**</strong></p>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
            html.AppendLine("<th>Part Number</th>");
            html.AppendLine("<th>Packlist Qty</th>");
            html.AppendLine("<th>Received Qty</th>");
            html.AppendLine("<th>Difference</th>");
            html.AppendLine("<th>Note</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var disc in emailData.Discrepancies)
            {
                string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{disc.PartNumber}</td>");
                html.AppendLine($"<td>{disc.PacklistQty}</td>");
                html.AppendLine($"<td>{disc.ReceivedQty}</td>");
                html.AppendLine($"<td>{diffStr}</td>");
                html.AppendLine($"<td>{disc.Note}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("<br/>");
        }

        html.AppendLine("<p><strong>Requested Lines:</strong></p>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
        html.AppendLine("<thead>");
        html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
        html.AppendLine("<th>Part Number</th>");
        html.AppendLine("<th>Quantity (pcs)</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");

        foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{kvp.Key}</td>");
            html.AppendLine($"<td>{kvp.Value}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("<br/>");

        if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
        {
            html.AppendLine("<p><strong>Additional Notes:</strong></p>");
            html.AppendLine($"<p>{emailData.AdditionalNotes}</p>");
        }

        html.AppendLine($"<p>{emailData.Signature.Replace("\\n", "<br/>")}</p>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
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

    /// <summary>
    /// Saves shipment as pending using SavePendingShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand]
    private async Task SaveAsPendingAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving shipment...";

            // Validate first
            if (!ValidateShipment())
            {
                await _errorHandler.HandleErrorAsync(
                    "Please add at least one part before saving",
                    Enum_ErrorSeverity.Low,
                    null,
                    true);
                return;
            }

            // Build command with ShipmentLineDto list
            var partsDto = Parts.Select(p => new ShipmentLineDto
            {
                PartNumber = p.PartNumber,
                ReceivedSkidCount = p.ReceivedSkidCount,
                ExpectedSkidCount = p.ExpectedSkidCount.HasValue ? (int?)p.ExpectedSkidCount.Value : null,
                HasDiscrepancy = p.HasDiscrepancy,
                DiscrepancyNote = p.DiscrepancyNote ?? string.Empty
            }).ToList();

            var saveCommand = new SavePendingShipmentCommand
            {
                ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
                ShipmentNumber = ShipmentNumber,
                Notes = Notes ?? string.Empty,
                Parts = partsDto
            };

            var result = await _mediator.Send(saveCommand);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Shipment #{ShipmentNumber} saved as pending";
                IsSuccessMessageVisible = true;
                HasPendingShipment = true;
                StatusMessage = "Shipment saved";
                await _logger.LogInfoAsync($"Shipment #{ShipmentNumber} saved as pending (ID: {result.Data})");
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

    /// <summary>
    /// Internal save method for backward compatibility (legacy code)
    /// TODO: Migrate all callers to use SavePendingShipmentCommand directly
    /// </summary>
    private async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentInternalAsync()
    {
        // Validate
        if (!ValidateShipment())
        {
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>("Shipment validation failed");
        }

        // Use MediatR command for save
        var partsDto = Parts.Select(p => new ShipmentLineDto
        {
            PartNumber = p.PartNumber,
            ReceivedSkidCount = p.ReceivedSkidCount,
            ExpectedSkidCount = p.ExpectedSkidCount.HasValue ? (int?)p.ExpectedSkidCount.Value : null,
            HasDiscrepancy = p.HasDiscrepancy,
            DiscrepancyNote = p.DiscrepancyNote ?? string.Empty
        }).ToList();

        var saveCommand = new SavePendingShipmentCommand
        {
            ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
            ShipmentNumber = ShipmentNumber,
            Notes = Notes ?? string.Empty,
            Parts = partsDto
        };

        var result = await _mediator.Send(saveCommand);

        if (result.IsSuccess)
        {
            return new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
            {
                Success = true,
                Data = (result.Data, ShipmentNumber)
            };
        }
        else
        {
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>(result.ErrorMessage);
        }
    }

    /// <summary>
    /// Completes pending shipment using CompleteShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand]
    private async Task CompleteShipmentAsync()
    {
        // This will be used when completing a pending shipment with PO/Receiver numbers
        try
        {
            IsBusy = true;
            StatusMessage = "Completing shipment...";

            // Get pending shipment using MediatR
            var pendingQuery = new GetPendingShipmentQuery
            {
                UserName = Environment.UserName
            };
            var pendingResult = await _mediator.Send(pendingQuery);

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

            // Use MediatR CompleteShipmentCommand
            var partsDto = Parts.Select(p => new ShipmentLineDto
            {
                PartNumber = p.PartNumber,
                ReceivedSkidCount = p.ReceivedSkidCount,
                ExpectedSkidCount = p.ExpectedSkidCount.HasValue ? (int?)p.ExpectedSkidCount.Value : null,
                HasDiscrepancy = p.HasDiscrepancy,
                DiscrepancyNote = p.DiscrepancyNote ?? string.Empty
            }).ToList();

            var completeCommand = new CompleteShipmentCommand
            {
                ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
                ShipmentNumber = ShipmentNumber,
                PONumber = poTextBox.Text.Trim(),
                ReceiverNumber = receiverTextBox.Text.Trim(),
                Notes = Notes ?? string.Empty,
                Parts = partsDto
            };

            var completeResult = await _mediator.Send(completeCommand);

            if (completeResult.IsSuccess)
            {
                SuccessMessage = $"Shipment #{ShipmentNumber} completed successfully!";
                IsSuccessMessageVisible = true;
                HasPendingShipment = false;
                await _logger.LogInfoAsync($"Shipment #{ShipmentNumber} completed with PO: {poTextBox.Text.Trim()}");

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
    private async Task ToggleDiscrepancyAsync(Model_VolvoShipmentLine? line)
    {
        if (line == null)
        {
            return;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            return;
        }

        if (line.HasDiscrepancy)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Remove Discrepancy",
                Content = "Remove the discrepancy for this line?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var confirmResult = await confirmDialog.ShowAsync();
            if (confirmResult == ContentDialogResult.Primary)
            {
                line.HasDiscrepancy = false;
            }

            return;
        }

        var expectedSkidsBox = new NumberBox
        {
            Header = "Expected Skids",
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Value = line.ExpectedSkidCount ?? 1
        };

        var noteBox = new TextBox
        {
            Header = "Discrepancy Note",
            PlaceholderText = "Explain discrepancy",
            Text = line.DiscrepancyNote ?? string.Empty
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(expectedSkidsBox);
        panel.Children.Add(noteBox);

        var dialog = new ContentDialog
        {
            Title = "Report Discrepancy",
            Content = panel,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (expectedSkidsBox.Value < 1)
        {
            await _errorHandler.HandleErrorAsync(
                "Expected skids must be greater than 0",
                Enum_ErrorSeverity.Low,
                null,
                true);
            return;
        }

        line.HasDiscrepancy = true;
        line.ExpectedSkidCount = expectedSkidsBox.Value;
        line.DiscrepancyNote = string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim();
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
    /// <param name="jsonValue"></param>
    /// <param name="fallbackValue"></param>
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

    partial void OnReceivedSkidsToAddChanged(string value)
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
        ReceivedSkidsToAdd = string.Empty;
        PartSearchText = string.Empty;
        SuggestedParts.Clear();
    }

    #endregion
}
