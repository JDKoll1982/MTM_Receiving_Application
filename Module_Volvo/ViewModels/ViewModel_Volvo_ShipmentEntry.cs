using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Windows.ApplicationModel.DataTransfer;
using AppInfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo shipment entry (CQRS-enabled with MediatR)
/// </summary>
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_ReceivingValidation _receivingValidation;

    private readonly IService_ReportingClipboard _reportingClipboard;
    private readonly IService_Window _windowService;
    private readonly IService_UserSessionManager _sessionManager;

    // Public accessors for code-behind dialog
    public IService_ErrorHandler ErrorHandler => _errorHandler;
    public IService_LoggingUtility Logger => _logger;
    public bool UseMockLocationList => _receivingValidation.UseMockLocationList;
    public IReadOnlyList<string> PresetLocations => _receivingValidation.PresetLocations;

    public ViewModel_Volvo_ShipmentEntry(
        IMediator mediator,
        IService_InforVisual inforVisualService,
        IService_ReceivingValidation receivingValidation,
        IService_ReportingClipboard reportingClipboard,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_UserSessionManager sessionManager,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _reportingClipboard =
            reportingClipboard ?? throw new ArgumentNullException(nameof(reportingClipboard));
        _windowService = windowService;
        _sessionManager = sessionManager;

        AttachPartsCollectionHandlers(Parts);
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

    public bool HasSelectedPart => SelectedPart != null;

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

    [ObservableProperty]
    private bool _hasActiveQueueRows;

    [ObservableProperty]
    private bool _isAutoSaveInProgress;

    private int? _currentShipmentId;
    private string _lastGeneratedShipmentFingerprint = string.Empty;
    private readonly SemaphoreSlim _autoSaveLock = new(1, 1);
    private Task _pendingAutoSaveTask = Task.CompletedTask;

    public bool HasAnyParts => Parts.Count > 0;

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
                    Enum_ErrorSeverity.Medium
                );
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
                _currentShipmentId = pendingResult.Data.Id;
                await LoadPendingShipmentAsync(pendingResult.Data.Id);
            }

            await RefreshActiveQueueAvailabilityAsync();

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error initializing Volvo shipment entry: {ex.Message}",
                ex
            );
            await _errorHandler.HandleErrorAsync(
                "Error initializing Volvo shipment entry",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
        }
    }

    /// <summary>
    /// Loads pending shipment data (CQRS)
    /// </summary>
    /// <param name="shipmentId"></param>
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
                _currentShipmentId = shipment.Id;
                ShipmentDate = new DateTimeOffset(shipment.ShipmentDate);
                ShipmentNumber = shipment.ShipmentNumber;
                Notes = shipment.Notes ?? string.Empty;

                // Load lines
                ReplaceParts(detailResult.Data.Lines);

                ApplyCachedQuantitiesToLines();

                await _logger.LogInfoAsync(
                    $"Loaded pending shipment #{shipment.ShipmentNumber} with {Parts.Count} parts"
                );
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
            var partsResult = await _mediator.Send(
                new GetAllVolvoPartsQuery { IncludeInactive = true }
            );

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
            foreach (var line in Parts)
            {
                line.PartDescription = string.Empty;
                line.PoStatus = VolvoLinePoStatus.NormalizeStorageValue(line.PoStatus);
            }

            return;
        }

        var partsByNumber = _allParts.ToDictionary(
            p => p.PartNumber,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var line in Parts)
        {
            line.PoStatus = VolvoLinePoStatus.NormalizeStorageValue(line.PoStatus);

            if (
                !string.IsNullOrWhiteSpace(line.PartNumber)
                && partsByNumber.TryGetValue(line.PartNumber, out var part)
            )
            {
                line.PartDescription = part.Description;

                if (line.QuantityPerSkid <= 0)
                {
                    line.QuantityPerSkid = part.QuantityPerSkid;
                }
            }
            else
            {
                line.PartDescription = string.Empty;
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
            var partsResult = await _mediator.Send(
                new GetAllVolvoPartsQuery
                {
                    IncludeInactive = false, // Only show active parts in dialog
                }
            );

            if (partsResult.IsSuccess && partsResult.Data != null)
            {
                AvailableParts = new ObservableCollection<Model_VolvoPart>(
                    partsResult.Data.OrderBy(p => p.PartNumber)
                );
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
            ReplaceSuggestedParts(Array.Empty<Model_VolvoPart>(), clearSelection: true);
            return;
        }

        try
        {
            // Use MediatR query for search
            var searchQuery = new SearchVolvoPartsQuery { SearchText = queryText, MaxResults = 20 };

            var searchResult = await _mediator.Send(searchQuery);

            if (searchResult.IsSuccess && searchResult.Data != null)
            {
                ReplaceSuggestedParts(searchResult.Data, preferredPartNumber: queryText);
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

    #region Card Actions

    public async Task WaitForPendingAutoSaveAsync()
    {
        await _pendingAutoSaveTask;
    }

    public async Task<Model_Dao_Result> AddPartFromDialogAsync(
        Model_VolvoPart selectedPart,
        int skidCount,
        string? location
    )
    {
        await WaitForPendingAutoSaveAsync();

        if (
            Parts.Any(p =>
                p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return Model_Dao_Result_Factory.Failure(
                $"Part {selectedPart.PartNumber} is already in this shipment. Remove or edit the existing card instead."
            );
        }

        var newLine = new Model_VolvoShipmentLine
        {
            PartNumber = selectedPart.PartNumber,
            PartDescription = selectedPart.Description,
            PoStatus = VolvoLinePoStatus.Pending,
            Location = location?.Trim() ?? string.Empty,
            QuantityPerSkid = selectedPart.QuantityPerSkid,
            ReceivedSkidCount = skidCount,
            CalculatedPieceCount = selectedPart.QuantityPerSkid * skidCount,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = string.Empty,
            IsExpanded = true,
        };

        Parts.Add(newLine);
        await _logger.LogInfoAsync(
            $"User added part {selectedPart.PartNumber}, {skidCount} skids ({newLine.CalculatedPieceCount} pcs)"
        );

        return await PersistCurrentShipmentAsync();
    }

    public async Task<Model_Dao_Result> RemovePartCardAsync(Model_VolvoShipmentLine line)
    {
        await WaitForPendingAutoSaveAsync();

        Parts.Remove(line);
        await _logger.LogInfoAsync($"User removed part {line.PartNumber} from shipment");
        return await PersistCurrentShipmentAsync();
    }

    public async Task<Model_Dao_Result> UpdatePartNumberAsync(
        Model_VolvoShipmentLine line,
        Model_VolvoPart replacementPart
    )
    {
        await WaitForPendingAutoSaveAsync();

        if (
            Parts.Any(p =>
                !ReferenceEquals(p, line)
                && p.PartNumber.Equals(
                    replacementPart.PartNumber,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            return Model_Dao_Result_Factory.Failure(
                $"Part {replacementPart.PartNumber} already exists in the current list. Remove or edit the existing card instead."
            );
        }

        line.PartNumber = replacementPart.PartNumber;
        line.PartDescription = replacementPart.Description;
        line.QuantityPerSkid = replacementPart.QuantityPerSkid;

        return await PersistCurrentShipmentAsync();
    }

    public async Task<Model_Dao_Result> UpdateReceivedSkidsAsync(
        Model_VolvoShipmentLine line,
        int receivedSkids
    )
    {
        line.ReceivedSkidCount = receivedSkids;
        return await PersistCurrentShipmentAsync();
    }

    public async Task<Model_Dao_Result> UpdateQuantityPerSkidAsync(
        Model_VolvoShipmentLine line,
        int quantityPerSkid
    )
    {
        await WaitForPendingAutoSaveAsync();
        line.QuantityPerSkid = quantityPerSkid;
        return await PersistCurrentShipmentAsync();
    }

    public async Task<Model_Dao_Result> ClearDiscrepancyAsync(Model_VolvoShipmentLine line)
    {
        await WaitForPendingAutoSaveAsync();
        line.HasDiscrepancy = false;
        return await PersistCurrentShipmentAsync();
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
            if (
                !int.TryParse(ReceivedSkidsToAdd, out int skidCount)
                || skidCount < 1
                || skidCount > 99
            )
            {
                await _errorHandler.HandleErrorAsync(
                    "Received skid count must be a number between 1 and 99",
                    Enum_ErrorSeverity.Low,
                    null,
                    true
                );
                return;
            }

            // Check for duplicate part number
            if (
                Parts.Any(p =>
                    p.PartNumber.Equals(
                        SelectedPartToAdd.PartNumber,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                await _errorHandler.HandleErrorAsync(
                    $"Part {SelectedPartToAdd.PartNumber} is already in this shipment. Remove it first if you want to update the quantity.",
                    Enum_ErrorSeverity.Low,
                    null,
                    true
                );
                return;
            }

            // Validate part using MediatR command (checks part exists in master data)
            var addCommand = new AddPartToShipmentCommand
            {
                PartNumber = SelectedPartToAdd.PartNumber,
                ReceivedSkidCount = skidCount,
                HasDiscrepancy = false,
            };

            var validationResult = await _mediator.Send(addCommand);

            if (!validationResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    validationResult.ErrorMessage ?? "Failed to validate part",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
                return;
            }

            // Validation passed - add to ObservableCollection
            var calculatedPieces = SelectedPartToAdd.QuantityPerSkid * skidCount;

            var newLine = new Model_VolvoShipmentLine
            {
                PartNumber = SelectedPartToAdd.PartNumber,
                Location = await ResolvePartLocationAsync(SelectedPartToAdd.PartNumber),
                QuantityPerSkid = SelectedPartToAdd.QuantityPerSkid,
                ReceivedSkidCount = skidCount,
                CalculatedPieceCount = calculatedPieces,
                HasDiscrepancy = false,
                ExpectedSkidCount = null,
                DiscrepancyNote = string.Empty,
            };

            Parts.Add(newLine);

            // Log user action
            await _logger.LogInfoAsync(
                $"User added part {SelectedPartToAdd.PartNumber}, {skidCount} skids ({calculatedPieces} pcs)"
            );

            // Reset input fields
            SelectedPartToAdd = null;
            ReceivedSkidsToAdd = string.Empty;
            PartSearchText = string.Empty;
            ReplaceSuggestedParts(Array.Empty<Model_VolvoPart>(), clearSelection: true);

            ValidateSaveEligibility();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error adding part: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error adding part to shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
    }

    private bool CanAddPart()
    {
        return SelectedPartToAdd != null
            && !string.IsNullOrWhiteSpace(ReceivedSkidsToAdd)
            && int.TryParse(ReceivedSkidsToAdd, out int count)
            && count >= 1
            && count <= 99;
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
                PartNumber = SelectedPart.PartNumber,
            };

            var validationResult = await _mediator.Send(removeCommand);

            if (!validationResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    validationResult.ErrorMessage ?? "Failed to validate part removal",
                    Enum_ErrorSeverity.Low,
                    null,
                    true
                );
                return;
            }

            // Validation passed - remove from ObservableCollection
            var removedPartNumber = SelectedPart.PartNumber;
            Parts.Remove(SelectedPart);

            if (Parts.Count == 0 && HasPendingShipment && _currentShipmentId.HasValue)
            {
                var deleteResult = await _mediator.Send(
                    new DeletePendingShipmentCommand { ShipmentId = _currentShipmentId.Value }
                );

                if (!deleteResult.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        deleteResult.ErrorMessage ?? "Failed to delete empty pending shipment",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true
                    );
                    return;
                }

                HasPendingShipment = false;
                _currentShipmentId = null;
                StatusMessage = "Pending shipment removed";
                await RefreshActiveQueueAvailabilityAsync();
            }

            await _logger.LogInfoAsync($"User removed part {removedPartNumber} from shipment");
            ValidateSaveEligibility();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error removing part: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error removing part from shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            RefreshCommandStates();
        }
    }

    /// <summary>
    /// Saves the shipment (if not already saved), runs component explosion, and confirms
    /// label data in the database.  Returns a summary of parts and piece counts.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateLabels))]
    private async Task GenerateLabelsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Generating labels...";

            int shipmentId;

            // First, save the shipment if not already saved
            if (!HasPendingShipment)
            {
                var saveResult = await SaveShipmentInternalAsync();
                if (!saveResult.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        saveResult.ErrorMessage
                            ?? "Failed to save shipment before generating labels",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true
                    );
                    return;
                }

                shipmentId = saveResult.Data.ShipmentId;
                _currentShipmentId = shipmentId;
                HasPendingShipment = true;
            }
            else if (_currentShipmentId.HasValue && _currentShipmentId.Value > 0)
            {
                shipmentId = _currentShipmentId.Value;
            }
            else
            {
                var pendingResult = await _mediator.Send(
                    new GetPendingShipmentQuery
                    {
                        UserName =
                            _sessionManager.CurrentSession?.User?.WindowsUsername
                            ?? Environment.UserName,
                    }
                );

                if (!pendingResult.IsSuccess || pendingResult.Data == null)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No pending shipment found",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true
                    );
                    return;
                }

                shipmentId = pendingResult.Data.Id;
                _currentShipmentId = shipmentId;
                HasPendingShipment = true;
            }

            // Generate label data using MediatR query
            var labelQuery = new GenerateLabelQuery { ShipmentId = shipmentId };
            var labelResult = await _mediator.Send(labelQuery);

            if (labelResult.IsSuccess)
            {
                _lastGeneratedShipmentFingerprint = BuildCurrentGenerationFingerprint(shipmentId);
                SuccessMessage = $"Labels generated successfully!\n{labelResult.Data}";
                IsSuccessMessageVisible = true;
                StatusMessage = "Labels generated";
                await _logger.LogInfoAsync(
                    $"Labels generated for shipment ID: {shipmentId}: {labelResult.Data}"
                );
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    labelResult.ErrorMessage ?? "Failed to generate labels",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error generating labels: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error generating labels",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
        }
    }

    [RelayCommand(CanExecute = nameof(CanPreviewEmail))]
    private async Task PreviewEmailAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Formatting email...";

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
                        true
                    );
                    return;
                }

                shipmentId = saveResult.Data.ShipmentId;
                HasPendingShipment = true;
            }
            else
            {
                if (_currentShipmentId.HasValue && _currentShipmentId.Value > 0)
                {
                    shipmentId = _currentShipmentId.Value;
                }
                else
                {
                    var pendingResult = await _mediator.Send(
                        new GetPendingShipmentQuery
                        {
                            UserName =
                                _sessionManager.CurrentSession?.User?.WindowsUsername
                                ?? Environment.UserName,
                        }
                    );

                    if (!pendingResult.IsSuccess || pendingResult.Data == null)
                    {
                        await _errorHandler.HandleErrorAsync(
                            "No pending shipment found",
                            Enum_ErrorSeverity.Medium,
                            null,
                            true
                        );
                        return;
                    }

                    shipmentId = pendingResult.Data.Id;
                    _currentShipmentId = shipmentId;
                }
            }

            var emailResult = await _mediator.Send(
                new FormatEmailDataQuery { ShipmentId = shipmentId }
            );

            if (!emailResult.IsSuccess || emailResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    emailResult.ErrorMessage ?? "Failed to format email data",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
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
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
        }
    }

    private async Task ShowEmailPreviewDialogAsync(Model_VolvoEmailData emailData)
    {
        try
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                await _errorHandler.ShowUserErrorAsync(
                    "Cannot show the email preview because the window host is unavailable.",
                    "Email Preview",
                    nameof(ShowEmailPreviewDialogAsync)
                );
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

            var formattedEmailDocument = BuildFormattedEmailDocument(emailData);
            var dialogModel = BuildEmailPreviewDialogModel(
                emailData,
                toRecipients,
                ccRecipients,
                formattedEmailDocument
            );
            var dialog = new Views.View_Volvo_EmailPreviewDialog { XamlRoot = xamlRoot };
            dialog.Initialize(dialogModel);

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var copySucceeded = await dialog.CopyEmailBodyAsync();
                if (!copySucceeded)
                {
                    return;
                }

                SuccessMessage =
                    "Email copied to clipboard (paste into Outlook as formatted table)!";
                IsSuccessMessageVisible = true;
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error showing email preview dialog: {ex.Message}", ex);
            await _errorHandler.ShowUserErrorAsync(
                "The email preview could not be opened.",
                "Email Preview",
                nameof(ShowEmailPreviewDialogAsync)
            );
        }
    }

    private Model_VolvoEmailPreviewDialog BuildEmailPreviewDialogModel(
        Model_VolvoEmailData emailData,
        string toRecipients,
        string ccRecipients,
        Model_FormattedReportDocument formattedEmailDocument
    )
    {
        return new Model_VolvoEmailPreviewDialog
        {
            ToRecipients = toRecipients,
            CcRecipients = ccRecipients,
            Subject = emailData.Subject,
            AdditionalNotes = emailData.AdditionalNotes,
            PreviewHtmlDocument = BuildPreviewHtmlDocument(formattedEmailDocument.HtmlFragment),
            PlainTextPreview = BuildPlainTextEmail(emailData),
            FormattedEmailDocument = formattedEmailDocument,
        };
    }

    private string BuildPlainTextEmail(Model_VolvoEmailData emailData)
    {
        var text = new StringBuilder();
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
                string diffStr =
                    disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                text.AppendLine(
                    $"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}"
                );
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
        return text.ToString();
    }

    private static Model_FormattedReportDocument BuildFormattedEmailDocument(
        Model_VolvoEmailData emailData
    )
    {
        var html = new StringBuilder();
        var plainText = new StringBuilder();

        html.AppendLine("<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
        plainText.AppendLine(emailData.Greeting);
        plainText.AppendLine();
        plainText.AppendLine(emailData.Message);
        plainText.AppendLine();

        AppendSectionStart(html, "Message", "#edf2f7", "#1f2937");
        html.AppendLine(
            $"<div style='margin: 0 0 8px 0;'>{System.Net.WebUtility.HtmlEncode(emailData.Greeting)}</div>"
        );
        html.AppendLine(
            $"<div style='margin: 0;'>{System.Net.WebUtility.HtmlEncode(emailData.Message)}</div>"
        );
        AppendSectionEnd(html);

        if (emailData.Discrepancies.Count > 0)
        {
            AppendSectionStart(html, "Discrepancies Noted", "#fde68a", "#1f2937");
            html.AppendLine(
                "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 6px 0 10px 0;'>"
            );
            html.AppendLine("<thead>");
            html.AppendLine(
                "<tr style='background-color: #fde68a; color: #1f2937; font-weight: 700;'>"
            );
            AppendHeaderCell(html, "Part Number");
            AppendHeaderCell(html, "Packlist Qty (pcs)");
            AppendHeaderCell(html, "Received Qty (pcs)");
            AppendHeaderCell(html, "Difference (pcs)");
            AppendHeaderCell(html, "Note");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            plainText.AppendLine("**DISCREPANCIES NOTED**");
            plainText.AppendLine();
            plainText.AppendLine(
                "Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote"
            );
            plainText.AppendLine(new string('-', 80));

            foreach (var disc in emailData.Discrepancies)
            {
                string diffStr =
                    disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                html.AppendLine("<tr style='background-color: #ffffff;'>");
                AppendBodyCell(html, disc.PartNumber);
                AppendBodyCell(html, disc.PacklistQty.ToString(), "right");
                AppendBodyCell(html, disc.ReceivedQty.ToString(), "right");
                AppendBodyCell(html, diffStr, "right");
                AppendBodyCell(html, disc.Note);
                html.AppendLine("</tr>");
                plainText.AppendLine(
                    $"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}"
                );
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            AppendSectionEnd(html);
            plainText.AppendLine();
        }

        AppendSectionStart(html, "Requested Lines", "#dbeafe", "#1d4ed8");
        html.AppendLine(
            "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 6px 0 10px 0;'>"
        );
        html.AppendLine("<thead>");
        html.AppendLine(
            "<tr style='background-color: #dbeafe; color: #1d4ed8; font-weight: 700;'>"
        );
        AppendHeaderCell(html, "Part Number");
        AppendHeaderCell(html, "Quantity (pcs)");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");

        plainText.AppendLine("Requested Lines:");
        plainText.AppendLine();
        plainText.AppendLine("Part Number\tQuantity (pcs)");
        plainText.AppendLine(new string('-', 40));

        foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
        {
            html.AppendLine("<tr style='background-color: #ffffff;'>");
            AppendBodyCell(html, kvp.Key);
            AppendBodyCell(html, kvp.Value.ToString(), "right");
            html.AppendLine("</tr>");
            plainText.AppendLine($"{kvp.Key}\t{kvp.Value}");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        AppendSectionEnd(html);
        plainText.AppendLine();

        if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
        {
            AppendSectionStart(html, "Additional Notes", "#e2e8f0", "#475569");
            html.AppendLine(
                $"<div style='white-space: pre-wrap;'>{System.Net.WebUtility.HtmlEncode(emailData.AdditionalNotes)}</div>"
            );
            AppendSectionEnd(html);

            plainText.AppendLine("Additional Notes:");
            plainText.AppendLine(emailData.AdditionalNotes);
            plainText.AppendLine();
        }

        html.AppendLine("</div>");

        return new Model_FormattedReportDocument
        {
            HtmlFragment = html.ToString(),
            PlainText = plainText.ToString(),
        };
    }

    private static string BuildPreviewHtmlDocument(string htmlFragment)
    {
        return "<!DOCTYPE html>"
            + "<html><head><meta charset=\"utf-8\" />"
            + "<style>body { margin: 0; padding: 12px; background: #ffffff; }</style>"
            + "</head><body>"
            + htmlFragment
            + "</body></html>";
    }

    private static void AppendSectionStart(
        StringBuilder html,
        string title,
        string accentBackground,
        string accentForeground
    )
    {
        html.AppendLine(
            "<div style='margin: 0 0 16px 0; border: 1px solid #cbd5e1; border-radius: 4px; overflow: hidden; background-color: #ffffff;'>"
        );
        html.AppendLine(
            $"<div style='background-color: {accentBackground}; color: {accentForeground}; padding: 10px 12px; font-weight: 700; display: block;'>{System.Net.WebUtility.HtmlEncode(title)}</div>"
        );
        html.AppendLine("<div style='padding: 10px 12px 12px 12px;'>");
    }

    private static void AppendSectionEnd(StringBuilder html)
    {
        html.AppendLine("</div>");
        html.AppendLine("</div>");
    }

    private static void AppendHeaderCell(StringBuilder html, string value)
    {
        html.AppendLine(
            $"<th style='border: 1px solid #c8d6e5; padding: 6px 10px; text-align: left; vertical-align: top; word-wrap: break-word;'>{System.Net.WebUtility.HtmlEncode(value)}</th>"
        );
    }

    private static void AppendBodyCell(StringBuilder html, string? value, string alignment = "left")
    {
        html.AppendLine(
            $"<td style='border: 1px solid #c8d6e5; padding: 6px 10px; text-align: {alignment}; vertical-align: top; word-wrap: break-word;'>{System.Net.WebUtility.HtmlEncode(value ?? string.Empty)}</td>"
        );
    }

    [RelayCommand]
    private void ViewHistory()
    {
        _logger.LogInfo("Navigating to Volvo Shipment History");
#pragma warning disable CS0618
        var view = App.GetService<Views.View_Volvo_History>();
#pragma warning restore CS0618
        if (view != null && App.MainWindow is MainWindow mainWindow)
        {
            mainWindow.SetContentPage(view, "Volvo Shipment History");
        }
    }

    /// <summary>
    /// Saves shipment as pending using SavePendingShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveAsPending))]
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
                    true
                );
                return;
            }

            // Build command with ShipmentLineDto list
            var partsDto = Parts
                .Select(p => new ShipmentLineDto
                {
                    PartNumber = p.PartNumber,
                    Location = p.Location,
                    QuantityPerSkid = p.QuantityPerSkid,
                    ReceivedSkidCount = p.ReceivedSkidCount,
                    PoStatus = VolvoLinePoStatus.NormalizeStorageValue(p.PoStatus),
                    ExpectedSkidCount = p.ExpectedSkidCount.HasValue
                        ? (int?)p.ExpectedSkidCount.Value
                        : null,
                    HasDiscrepancy = p.HasDiscrepancy,
                    DiscrepancyNote = p.DiscrepancyNote ?? string.Empty,
                })
                .ToList();

            var saveCommand = new SavePendingShipmentCommand
            {
                ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
                ShipmentNumber = ShipmentNumber,
                Notes = Notes ?? string.Empty,
                Parts = partsDto,
            };

            var result = await _mediator.Send(saveCommand);

            if (result.IsSuccess)
            {
                _currentShipmentId = result.Data;
                SuccessMessage = $"Shipment #{ShipmentNumber} saved as pending";
                IsSuccessMessageVisible = true;
                HasPendingShipment = true;
                HasActiveQueueRows = true;
                StatusMessage = "Shipment saved";
                await _logger.LogInfoAsync(
                    $"Shipment #{ShipmentNumber} saved as pending (ID: {result.Data})"
                );
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage ?? "Failed to save shipment",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error saving shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
        }
    }

    /// <summary>
    /// Internal save method for backward compatibility (legacy code)
    /// TODO: Migrate all callers to use SavePendingShipmentCommand directly
    /// </summary>
    private async Task<
        Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
    > SaveShipmentInternalAsync()
    {
        // Validate
        if (!ValidateShipment())
        {
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>(
                "Shipment validation failed"
            );
        }

        // Use MediatR command for save
        var partsDto = Parts
            .Select(p => new ShipmentLineDto
            {
                PartNumber = p.PartNumber,
                Location = p.Location,
                QuantityPerSkid = p.QuantityPerSkid,
                ReceivedSkidCount = p.ReceivedSkidCount,
                PoStatus = VolvoLinePoStatus.NormalizeStorageValue(p.PoStatus),
                ExpectedSkidCount = p.ExpectedSkidCount.HasValue
                    ? (int?)p.ExpectedSkidCount.Value
                    : null,
                HasDiscrepancy = p.HasDiscrepancy,
                DiscrepancyNote = p.DiscrepancyNote ?? string.Empty,
            })
            .ToList();

        var saveCommand = new SavePendingShipmentCommand
        {
            ShipmentId = _currentShipmentId,
            ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
            ShipmentNumber = ShipmentNumber,
            Notes = Notes ?? string.Empty,
            Parts = partsDto,
        };

        try
        {
            var result = await _mediator.Send(saveCommand);

            if (result.IsSuccess)
            {
                _currentShipmentId = result.Data;
                HasPendingShipment = true;
                HasActiveQueueRows = true;
                return new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
                {
                    Success = true,
                    Data = (result.Data, ShipmentNumber),
                };
            }

            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>(
                result.ErrorMessage
            );
        }
        catch (FluentValidation.ValidationException vex)
        {
            var errors = string.Join("; ", vex.Errors.Select(e => e.ErrorMessage));
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>(
                $"Validation failed: {errors}"
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<(int ShipmentId, int ShipmentNumber)>(
                $"Save failed: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Completes pending shipment using CompleteShipmentCommand (CQRS)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCompleteShipment))]
    private async Task CompleteShipmentAsync()
    {
        // This will be used when completing a pending shipment with PO/Receiver numbers
        try
        {
            IsBusy = true;
            StatusMessage = "Completing shipment...";

            Model_VolvoShipment? shipment = null;

            if (_currentShipmentId.HasValue && _currentShipmentId.Value > 0)
            {
                var detailResult = await _mediator.Send(
                    new GetShipmentDetailQuery { ShipmentId = _currentShipmentId.Value }
                );

                if (detailResult.IsSuccess && detailResult.Data != null)
                {
                    shipment = detailResult.Data.Shipment;
                }
            }

            if (shipment == null)
            {
                var pendingResult = await _mediator.Send(
                    new GetPendingShipmentQuery
                    {
                        UserName =
                            _sessionManager.CurrentSession?.User?.WindowsUsername
                            ?? Environment.UserName,
                    }
                );

                if (!pendingResult.IsSuccess || pendingResult.Data == null)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No pending shipment found",
                        Enum_ErrorSeverity.Medium,
                        null,
                        true
                    );
                    return;
                }

                shipment = pendingResult.Data;
                _currentShipmentId = shipment.Id;
                HasPendingShipment = true;
            }

            // Show completion dialog
            await ShowCompletionDialogAsync(shipment);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error completing shipment",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
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
            XamlRoot = xamlRoot,
        };

        var stackPanel = new StackPanel { Spacing = 12 };

        var poTextBox = new TextBox
        {
            Header = "PO Number",
            PlaceholderText = "Enter PO number (e.g., PO-062450)",
        };

        var receiverTextBox = new TextBox
        {
            Header = "Receiver Number",
            PlaceholderText = "Enter receiver number (e.g., 134393)",
        };

        stackPanel.Children.Add(poTextBox);
        stackPanel.Children.Add(receiverTextBox);
        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (
                string.IsNullOrWhiteSpace(poTextBox.Text)
                || string.IsNullOrWhiteSpace(receiverTextBox.Text)
            )
            {
                await _errorHandler.HandleErrorAsync(
                    "PO Number and Receiver Number are required",
                    Enum_ErrorSeverity.Low,
                    null,
                    true
                );
                return;
            }

            // Use MediatR CompleteShipmentCommand
            var partsDto = Parts
                .Select(p => new ShipmentLineDto
                {
                    PartNumber = p.PartNumber,
                    Location = p.Location,
                    QuantityPerSkid = p.QuantityPerSkid,
                    ReceivedSkidCount = p.ReceivedSkidCount,
                    PoStatus = VolvoLinePoStatus.NormalizeStorageValue(p.PoStatus),
                    ExpectedSkidCount = p.ExpectedSkidCount.HasValue
                        ? (int?)p.ExpectedSkidCount.Value
                        : null,
                    HasDiscrepancy = p.HasDiscrepancy,
                    DiscrepancyNote = p.DiscrepancyNote ?? string.Empty,
                })
                .ToList();

            var completeCommand = new CompleteShipmentCommand
            {
                ShipmentId = _currentShipmentId ?? shipment.Id,
                ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
                ShipmentNumber = ShipmentNumber,
                PONumber = poTextBox.Text.Trim(),
                ReceiverNumber = receiverTextBox.Text.Trim(),
                Notes = Notes ?? string.Empty,
                Parts = partsDto,
            };

            var completeResult = await _mediator.Send(completeCommand);

            if (completeResult.IsSuccess)
            {
                SuccessMessage = $"Shipment #{ShipmentNumber} completed successfully!";
                IsSuccessMessageVisible = true;
                HasPendingShipment = false;
                _currentShipmentId = null;
                await RefreshActiveQueueAvailabilityAsync();
                await _logger.LogInfoAsync(
                    $"Shipment #{ShipmentNumber} completed with PO: {poTextBox.Text.Trim()}"
                );

                // Clear the form
                ClearShipmentForm();
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    completeResult.ErrorMessage ?? "Failed to complete shipment",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
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
                XamlRoot = xamlRoot,
            };

            var confirmResult = await confirmDialog.ShowAsync();
            if (confirmResult == ContentDialogResult.Primary)
            {
                line.HasDiscrepancy = false;
                await PersistCurrentShipmentAsync();
            }

            return;
        }

        var expectedSkidsBox = new NumberBox
        {
            Header = "Expected Skids",
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Value = line.ExpectedSkidCount ?? 1,
        };

        var noteBox = new TextBox
        {
            Header = "Discrepancy Note",
            PlaceholderText = "Explain discrepancy",
            Text = line.DiscrepancyNote ?? string.Empty,
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
            XamlRoot = xamlRoot,
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
                true
            );
            return;
        }

        line.HasDiscrepancy = true;
        line.ExpectedSkidCount = expectedSkidsBox.Value;
        line.DiscrepancyNote = string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim();
        await PersistCurrentShipmentAsync();
    }

    [RelayCommand]
    private void StartNewEntry()
    {
        IsSuccessMessageVisible = false;
        SuccessMessage = string.Empty;
        HasPendingShipment = false;
        _currentShipmentId = null;
        ClearShipmentForm();
    }

    /// <summary>
    /// Moves all active Volvo shipment rows from the queue tables to the
    /// history archive tables after an explicit user confirmation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearLabelData))]
    private async Task ClearLabelDataAsync()
    {
        try
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Clear Label Data",
                Content =
                    "Are you sure you want to archive and clear the active Volvo label queue? All queued shipment rows will be moved to history. This action cannot be undone.",
                PrimaryButtonText = "Clear Label Data",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                confirmDialog,
                xamlRoot
            );

            var dialogResult = await confirmDialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary)
            {
                StatusMessage = "Clear label data cancelled";
                return;
            }

            IsBusy = true;
            StatusMessage = "Clearing label data...";

            var command = new ClearLabelDataCommand { ArchivedBy = Environment.UserName };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                if (result.Data > 0)
                {
                    SuccessMessage =
                        $"Label data cleared — {result.Data} record(s) moved to history";
                    IsSuccessMessageVisible = true;
                    StatusMessage = "Label data cleared";
                    HasPendingShipment = false;
                    _lastGeneratedShipmentFingerprint = string.Empty;
                    ClearShipmentForm();
                    await RefreshActiveQueueAvailabilityAsync();
                    await _logger.LogInfoAsync(
                        $"Clear label data completed: {result.Data} records archived by {Environment.UserName}"
                    );
                }
                else
                {
                    IsSuccessMessageVisible = false;
                    SuccessMessage = string.Empty;
                    StatusMessage = "No active label data found to clear";
                    HasActiveQueueRows = false;
                    await _logger.LogInfoAsync(
                        $"Clear label data requested by {Environment.UserName}, but no active queue rows were available to archive."
                    );
                }
            }
            else
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage ?? "Failed to clear label data",
                    Enum_ErrorSeverity.Medium,
                    null,
                    true
                );
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error clearing label data: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Error clearing label data",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            RefreshCommandStates();
        }
    }

    #endregion

    #region Validation

    private bool ValidateShipment()
    {
        if (Parts.Count == 0)
        {
            _errorHandler
                .HandleErrorAsync(
                    "At least one part is required",
                    Enum_ErrorSeverity.Low,
                    null,
                    true
                )
                .ConfigureAwait(false);
            return false;
        }

        foreach (var part in Parts)
        {
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                _errorHandler
                    .HandleErrorAsync(
                        "All parts must have a part number selected",
                        Enum_ErrorSeverity.Low,
                        null,
                        true
                    )
                    .ConfigureAwait(false);
                return false;
            }

            if (part.ReceivedSkidCount <= 0)
            {
                _errorHandler
                    .HandleErrorAsync(
                        $"Part {part.PartNumber} must have at least 1 skid",
                        Enum_ErrorSeverity.Low,
                        null,
                        true
                    )
                    .ConfigureAwait(false);
                return false;
            }

            if (part.HasDiscrepancy && !part.ExpectedSkidCount.HasValue)
            {
                _errorHandler
                    .HandleErrorAsync(
                        $"Part {part.PartNumber} has discrepancy but no expected skid count",
                        Enum_ErrorSeverity.Low,
                        null,
                        true
                    )
                    .ConfigureAwait(false);
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
            var recipients = System.Text.Json.JsonSerializer.Deserialize<
                List<Models.Model_EmailRecipient>
            >(jsonValue);
            if (recipients == null || recipients.Count == 0)
            {
                return fallbackValue;
            }

            return string.Join("; ", recipients.Select(r => r.ToOutlookFormat()));
        }
        catch (Exception ex)
        {
            _logger
                .LogErrorAsync($"Error parsing email recipients JSON: {ex.Message}", ex)
                .ConfigureAwait(false);
            return fallbackValue;
        }
    }

    private void ValidateSaveEligibility()
    {
        CanSave =
            Parts.Count > 0
            && Parts.All(p => !string.IsNullOrWhiteSpace(p.PartNumber) && p.ReceivedSkidCount > 0);
        RefreshCommandStates();
    }

    private bool CanGenerateLabels()
    {
        return HasShipmentData() && !HasLabelsGeneratedForCurrentShipment();
    }

    private bool CanPreviewEmail()
    {
        return HasShipmentData();
    }

    private bool CanSaveAsPending()
    {
        return !IsBusy && Parts.Count > 0;
    }

    private bool CanCompleteShipment()
    {
        return !IsBusy && !IsAutoSaveInProgress && Parts.Count > 0;
    }

    private bool CanClearLabelData()
    {
        return !IsBusy && HasActiveQueueRows;
    }

    private bool HasShipmentData()
    {
        return !IsBusy
            && !IsAutoSaveInProgress
            && Parts.Count > 0
            && Parts.All(p => !string.IsNullOrWhiteSpace(p.PartNumber) && p.ReceivedSkidCount > 0);
    }

    private async Task<Model_Dao_Result> PersistCurrentShipmentAsync()
    {
        var currentSaveTask = PersistCurrentShipmentInternalAsync();
        _pendingAutoSaveTask = currentSaveTask;
        return await currentSaveTask;
    }

    private async Task<Model_Dao_Result> PersistCurrentShipmentInternalAsync()
    {
        await _autoSaveLock.WaitAsync();

        try
        {
            IsAutoSaveInProgress = true;

            if (Parts.Count == 0)
            {
                if (HasPendingShipment && _currentShipmentId.HasValue)
                {
                    var deleteResult = await _mediator.Send(
                        new DeletePendingShipmentCommand { ShipmentId = _currentShipmentId.Value }
                    );

                    if (!deleteResult.IsSuccess)
                    {
                        ShowStatus(
                            deleteResult.ErrorMessage
                                ?? "Auto-save failed while clearing the empty shipment.",
                            AppInfoBarSeverity.Warning
                        );
                        return deleteResult;
                    }
                }

                HasPendingShipment = false;
                HasActiveQueueRows = false;
                _currentShipmentId = null;
                return Model_Dao_Result_Factory.Success();
            }

            var saveCommand = new SavePendingShipmentCommand
            {
                ShipmentId = _currentShipmentId,
                ShipmentDate = ShipmentDate ?? DateTimeOffset.Now,
                ShipmentNumber = ShipmentNumber,
                Notes = Notes ?? string.Empty,
                Parts = Parts
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

            var saveResult = await _mediator.Send(saveCommand);
            if (!saveResult.IsSuccess)
            {
                ShowStatus(
                    saveResult.ErrorMessage
                        ?? "Auto-save failed. Your changes are still shown and will be retried on the next action.",
                    AppInfoBarSeverity.Warning
                );
                return saveResult;
            }

            _currentShipmentId = saveResult.Data;
            HasPendingShipment = true;
            HasActiveQueueRows = true;
            StatusMessage = "Changes saved";
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error during Volvo auto-save: {ex.Message}", ex);
            ShowStatus(
                "Auto-save failed. Your changes are still shown and will be retried on the next action.",
                AppInfoBarSeverity.Warning
            );
            return Model_Dao_Result_Factory.Failure("Volvo auto-save failed", ex);
        }
        finally
        {
            IsAutoSaveInProgress = false;
            RefreshCommandStates();
            _autoSaveLock.Release();
        }
    }

    private bool HasLabelsGeneratedForCurrentShipment()
    {
        if (string.IsNullOrWhiteSpace(_lastGeneratedShipmentFingerprint))
        {
            return false;
        }

        return string.Equals(
            _lastGeneratedShipmentFingerprint,
            BuildCurrentGenerationFingerprint(_currentShipmentId),
            StringComparison.Ordinal
        );
    }

    private string BuildCurrentGenerationFingerprint(int? shipmentId)
    {
        var normalizedNotes = string.IsNullOrWhiteSpace(Notes) ? string.Empty : Notes.Trim();
        var orderedLines = Parts
            .OrderBy(part => part.PartNumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(part => part.Location, StringComparer.OrdinalIgnoreCase)
            .Select(part =>
                string.Join(
                    "|",
                    part.PartNumber?.Trim() ?? string.Empty,
                    part.Location?.Trim() ?? string.Empty,
                    part.QuantityPerSkid,
                    part.ReceivedSkidCount,
                    part.CalculatedPieceCount,
                    part.HasDiscrepancy,
                    part.ExpectedSkidCount?.ToString() ?? string.Empty,
                    part.DiscrepancyNote?.Trim() ?? string.Empty
                )
            );

        return string.Join(
            "||",
            shipmentId?.ToString() ?? string.Empty,
            ShipmentDate?.Date.ToString("yyyy-MM-dd") ?? string.Empty,
            ShipmentNumber,
            normalizedNotes,
            string.Join("||", orderedLines)
        );
    }

    private async Task RefreshActiveQueueAvailabilityAsync()
    {
        try
        {
            var activeQueueResult = await _mediator.Send(
                new GetShipmentHistoryQuery
                {
                    StartDate = new DateTimeOffset(new DateTime(2000, 1, 1)),
                    EndDate = new DateTimeOffset(new DateTime(2100, 12, 31)),
                    StatusFilter = VolvoShipmentStatus.AllDisplayName,
                }
            );

            HasActiveQueueRows =
                activeQueueResult.IsSuccess && activeQueueResult.Data is { Count: > 0 };
        }
        catch (Exception ex)
        {
            HasActiveQueueRows = false;
            await _logger.LogErrorAsync(
                $"Error checking Volvo active queue availability: {ex.Message}",
                ex
            );
        }
    }

    private void RefreshCommandStates()
    {
        AddPartCommand.NotifyCanExecuteChanged();
        GenerateLabelsCommand.NotifyCanExecuteChanged();
        PreviewEmailCommand.NotifyCanExecuteChanged();
        SaveAsPendingCommand.NotifyCanExecuteChanged();
        CompleteShipmentCommand.NotifyCanExecuteChanged();
        ClearLabelDataCommand.NotifyCanExecuteChanged();
    }

    public async Task<string> ResolvePartLocationAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return string.Empty;
        }

        if (UseMockLocationList)
        {
            return ResolveMockLocation(partNumber);
        }

        var partResult = await _inforVisualService.GetPartByIDAsync(partNumber.Trim());
        if (!partResult.IsSuccess || partResult.Data is null)
        {
            return string.Empty;
        }

        return partResult.Data.DefaultLocationId?.Trim() ?? string.Empty;
    }

    private string ResolveMockLocation(string partNumber)
    {
        if (PresetLocations.Count == 0)
        {
            return string.Empty;
        }

        var index =
            Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(partNumber))
            % PresetLocations.Count;
        return PresetLocations[index];
    }

    #endregion

    #region Event Handlers

    partial void OnPartsChanged(ObservableCollection<Model_VolvoShipmentLine> value)
    {
        AttachPartsCollectionHandlers(value);
        _lastGeneratedShipmentFingerprint = string.Empty;
        OnPropertyChanged(nameof(HasAnyParts));
        ValidateSaveEligibility();
        RefreshCommandStates();
    }

    partial void OnShipmentDateChanged(DateTimeOffset? value)
    {
        _lastGeneratedShipmentFingerprint = string.Empty;
        RefreshCommandStates();
    }

    partial void OnShipmentNumberChanged(int value)
    {
        _lastGeneratedShipmentFingerprint = string.Empty;
        RefreshCommandStates();
    }

    partial void OnNotesChanged(string value)
    {
        _lastGeneratedShipmentFingerprint = string.Empty;
        RefreshCommandStates();
    }

    partial void OnPartsChanging(
        ObservableCollection<Model_VolvoShipmentLine>? oldValue,
        ObservableCollection<Model_VolvoShipmentLine> newValue
    )
    {
        DetachPartsCollectionHandlers(oldValue);
    }

    partial void OnSelectedPartToAddChanged(Model_VolvoPart? value)
    {
        AddPartCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPartChanged(Model_VolvoShipmentLine? value)
    {
        OnPropertyChanged(nameof(HasSelectedPart));
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
        _lastGeneratedShipmentFingerprint = string.Empty;
        ReplaceParts(Array.Empty<Model_VolvoShipmentLine>());
        Notes = string.Empty;
        ShipmentNumber = 1;
        _currentShipmentId = null;
        SelectedPartToAdd = null;
        ReceivedSkidsToAdd = string.Empty;
        PartSearchText = string.Empty;
        ReplaceSuggestedParts(Array.Empty<Model_VolvoPart>(), clearSelection: true);
        RefreshCommandStates();
    }

    private void ReplaceParts(IEnumerable<Model_VolvoShipmentLine> lines)
    {
        var selectedPartNumber = SelectedPart?.PartNumber;
        var expandStatesByPartNumber = Parts.ToDictionary(
            part => part.PartNumber,
            part => part.IsExpanded,
            StringComparer.OrdinalIgnoreCase
        );

        var newLines = lines.ToList();
        foreach (var line in newLines)
        {
            if (expandStatesByPartNumber.TryGetValue(line.PartNumber, out var isExpanded))
            {
                line.IsExpanded = isExpanded;
            }
        }

        Parts = new ObservableCollection<Model_VolvoShipmentLine>(newLines);
        ApplyCachedQuantitiesToLines();
        SelectedPart = string.IsNullOrWhiteSpace(selectedPartNumber)
            ? null
            : Parts.FirstOrDefault(part =>
                part.PartNumber.Equals(selectedPartNumber, StringComparison.OrdinalIgnoreCase)
            );
    }

    private void ReplaceSuggestedParts(
        IEnumerable<Model_VolvoPart> parts,
        string? preferredPartNumber = null,
        bool clearSelection = false
    )
    {
        var selectedPartNumber = clearSelection ? null : preferredPartNumber;
        SuggestedParts = new ObservableCollection<Model_VolvoPart>(parts);
        SelectedPartToAdd = string.IsNullOrWhiteSpace(selectedPartNumber)
            ? null
            : SuggestedParts.FirstOrDefault(part =>
                part.PartNumber.Equals(selectedPartNumber, StringComparison.OrdinalIgnoreCase)
            );
    }

    private void AttachPartsCollectionHandlers(ObservableCollection<Model_VolvoShipmentLine>? parts)
    {
        if (parts == null)
        {
            return;
        }

        parts.CollectionChanged -= OnPartsCollectionChanged;
        parts.CollectionChanged += OnPartsCollectionChanged;

        foreach (var line in parts)
        {
            AttachShipmentLineHandlers(line);
        }
    }

    private void DetachPartsCollectionHandlers(ObservableCollection<Model_VolvoShipmentLine>? parts)
    {
        if (parts == null)
        {
            return;
        }

        parts.CollectionChanged -= OnPartsCollectionChanged;

        foreach (var line in parts)
        {
            DetachShipmentLineHandlers(line);
        }
    }

    private void OnPartsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems.OfType<Model_VolvoShipmentLine>())
            {
                DetachShipmentLineHandlers(oldItem);
            }
        }

        if (e.NewItems != null)
        {
            foreach (var newItem in e.NewItems.OfType<Model_VolvoShipmentLine>())
            {
                AttachShipmentLineHandlers(newItem);
            }
        }

        _lastGeneratedShipmentFingerprint = string.Empty;
        OnPropertyChanged(nameof(HasAnyParts));
        ValidateSaveEligibility();
        RefreshCommandStates();
    }

    private void AttachShipmentLineHandlers(Model_VolvoShipmentLine line)
    {
        if (line is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnShipmentLinePropertyChanged;
            notifyPropertyChanged.PropertyChanged += OnShipmentLinePropertyChanged;
        }
    }

    private void DetachShipmentLineHandlers(Model_VolvoShipmentLine line)
    {
        if (line is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnShipmentLinePropertyChanged;
        }
    }

    private void OnShipmentLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _lastGeneratedShipmentFingerprint = string.Empty;
        ValidateSaveEligibility();
        RefreshCommandStates();
    }

    #endregion
}
