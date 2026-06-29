using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base, IResettableViewModel
    {
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_QualityHoldWarning _qualityHoldWarning;
        private readonly IService_InforVisualMockDataCatalog _mockDataCatalog;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_Window _windowService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IService_ReceivingSettings _receivingSettings;
        private DateTime? _currentPoHeaderPromiseDate;
        private NotifyCollectionChangedEventHandler? _partsCollectionChangedHandler;
        private bool _isClearingRestrictedSelection;
        private static readonly Regex CanonicalPoNumberPattern = new(
            @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[Bb]?)$",
            RegexOptions.IgnoreCase
        );

        [ObservableProperty]
        private string _poNumber = string.Empty;

        [ObservableProperty]
        private string _partID = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isNonPOItem;

        [ObservableProperty]
        private bool _isLoadPOEnabled = false;

        [ObservableProperty]
        private string _poValidationMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Model_InforVisualPart> _parts = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedPart))]
        private Model_InforVisualPart? _selectedPart;

        [ObservableProperty]
        private string _packageType = "Skids"; // Default package type

        [ObservableProperty]
        private string _poStatus = string.Empty;

        [ObservableProperty]
        private string _poStatusDescription = string.Empty;

        [ObservableProperty]
        private bool _isPOClosed = false;

        [ObservableProperty]
        private bool _isPartsListVisible;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _poNumberHeaderText = "Purchase Order Number";

        [ObservableProperty]
        private string _poStatusLabelText = "Status:";

        [ObservableProperty]
        private string _loadPoButtonText = "Load PO";

        [ObservableProperty]
        private string _switchToNonPoButtonText = "Switch to Non-PO";

        [ObservableProperty]
        private string _partIdentifierHeaderText = "Part Identifier";

        [ObservableProperty]
        private string _packageTypeAutoHeaderText = "Package Type (Auto-detected)";

        [ObservableProperty]
        private string _lookupPartButtonText = "Look Up Part";

        [ObservableProperty]
        private string _switchToPoButtonText = "Switch to PO Entry";

        [ObservableProperty]
        private string _availablePartsHeaderText = "Available Parts";

        [ObservableProperty]
        private string _columnPartIdHeaderText = "Part ID";

        [ObservableProperty]
        private string _columnDescriptionHeaderText = "Description";

        [ObservableProperty]
        private string _columnRemainingQtyHeaderText = "Remaining";

        [ObservableProperty]
        private string _columnQtyOrderedHeaderText = "Ordered";

        [ObservableProperty]
        private string _columnLineNumberHeaderText = "Line #";

        // Accessibility Properties
        [ObservableProperty]
        private string _poNumberAccessibilityName = "Purchase Order Number";

        [ObservableProperty]
        private string _loadPoAccessibilityName = "Load Purchase Order";

        [ObservableProperty]
        private string _partIdAccessibilityName = "Part Identifier";

        [ObservableProperty]
        private string _lookupPartAccessibilityName = "Look Up Part";

        [ObservableProperty]
        private string _partsListAccessibilityName = "Parts List";

        /// <summary>
        /// Gets a value indicating whether a part is currently selected for guided PO entry.
        /// </summary>
        public bool HasSelectedPart => SelectedPart is not null;

        public ViewModel_Receiving_POEntry(
            IService_InforVisual inforVisualService,
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Help helpService,
            IService_QualityHoldWarning qualityHoldWarning,
            IService_InforVisualMockDataCatalog mockDataCatalog,
            IService_ViewModelRegistry viewModelRegistry,
            IService_Window windowService,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IService_ReceivingSettings receivingSettings,
            IService_Notification notificationService
        )
            : base(errorHandler, logger, notificationService)
        {
            _inforVisualService = inforVisualService;
            _workflowService = workflowService;
            _helpService = helpService;
            _qualityHoldWarning = qualityHoldWarning;
            _mockDataCatalog = mockDataCatalog;
            _viewModelRegistry = viewModelRegistry;
            _windowService = windowService;
            _configuration = configuration;
            _receivingSettings = receivingSettings;

            _viewModelRegistry.Register(this);

            AttachPartsCollectionHandler();

            // Load UI text from settings
            _ = LoadUITextAsync();

            // Auto-fill PO number if using mock data
            InitializeAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                PoNumberHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryPurchaseOrderNumber
                );
                PoStatusLabelText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryStatusLabel
                );
                LoadPoButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryLoadPo
                );
                SwitchToNonPoButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntrySwitchToNonPo
                );
                PartIdentifierHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryPartIdentifier
                );
                PackageTypeAutoHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryPackageTypeAuto
                );
                LookupPartButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryLookupPart
                );
                SwitchToPoButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntrySwitchToPo
                );
                AvailablePartsHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryAvailableParts
                );

                ColumnPartIdHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryColumnPartId
                );
                ColumnDescriptionHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryColumnDescription
                );
                ColumnRemainingQtyHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryColumnRemainingQty
                );
                ColumnQtyOrderedHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryColumnQtyOrdered
                );
                ColumnLineNumberHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PoEntryColumnLineNumber
                );

                PoNumberAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.PoEntryPONumber
                );
                LoadPoAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.PoEntryLoadPo
                );
                PartIdAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.PoEntryPartId
                );
                LookupPartAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.PoEntryLookupPart
                );
                PartsListAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.PoEntryPartsList
                );

                _logger.LogInfo("PO Entry UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading PO Entry UI text from settings: {ex.Message}", ex);
            }
        }

        public void ResetToDefaults()
        {
            PoNumber = string.Empty;
            PartID = string.Empty;
            IsNonPOItem = false;
            IsLoadPOEnabled = false;
            PoValidationMessage = string.Empty;
            ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
            PackageType = "Skids";
            PoStatus = string.Empty;
            _currentPoHeaderPromiseDate = null;
            _workflowService.CurrentPODueDate = null;
        }

        [RelayCommand]
        private void PoTextBoxLostFocus()
        {
            if (string.IsNullOrWhiteSpace(PoNumber))
            {
                return;
            }

            if (TryNormalizePoNumber(PoNumber, out var normalizedPoNumber, out _))
            {
                PoNumber = normalizedPoNumber;
                return;
            }

            PoNumber = PoNumber.Trim().ToUpperInvariant();
        }

        [RelayCommand]
        private async Task LoadPOAsync()
        {
            if (string.IsNullOrWhiteSpace(PoNumber))
            {
                await _errorHandler.HandleErrorAsync(
                    await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Messages.ErrorPoRequired
                    ),
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
                if (result.IsSuccess && result.Data != null)
                {
                    var parts = result.Data.Parts.ToList();

                    // Set PO status in ViewModel
                    PoStatus = result.Data.Status;
                    PoStatusDescription = result.Data.StatusDescription;
                    IsPOClosed = result.Data.IsClosed;

                    // Set PO header data in Workflow Service for XLSX export
                    _workflowService.CurrentPOVendor = result.Data.Vendor;
                    _workflowService.CurrentPOStatus = result.Data.Status;
                    _currentPoHeaderPromiseDate = result.Data.HeaderPromiseDate;
                    _workflowService.CurrentPODueDate = _currentPoHeaderPromiseDate;

                    ReplaceParts(parts, clearSelection: true);

                    var msg = await _receivingSettings.FormatAsync(
                        ReceivingSettingsKeys.Messages.InfoPoLoadedWithParts,
                        PoNumber,
                        (object)Parts.Count
                    );
                    _workflowService.RaiseStatusMessage(msg);
                }
                else
                {
                    var errorMessage = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? result.ErrorMessage
                        : await _receivingSettings.GetStringAsync(
                            ReceivingSettingsKeys.Messages.ErrorPoNotFound
                        );
                    await _errorHandler.HandleErrorAsync(errorMessage, Enum_ErrorSeverity.Error);
                    ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
                    _currentPoHeaderPromiseDate = null;
                    _workflowService.CurrentPODueDate = null;
                    _workflowService.CurrentLocation = string.Empty;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleNonPO()
        {
            IsNonPOItem = !IsNonPOItem;
            ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
            PoNumber = string.Empty;
            PartID = string.Empty;
            _workflowService.IsNonPOItem = IsNonPOItem;
            _currentPoHeaderPromiseDate = null;
            _workflowService.CurrentPODueDate = null;
            _workflowService.CurrentLocation = string.Empty;
        }

        [RelayCommand]
        private async Task LookupPartAsync()
        {
            if (string.IsNullOrWhiteSpace(PartID))
            {
                await _errorHandler.HandleErrorAsync(
                    await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Messages.ErrorPartIdRequired
                    ),
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            IsLoading = true;
            try
            {
                var searchTerm = PartID.Trim();
                var result = await _inforVisualService.GetPartByIDAsync(searchTerm);
                if (result.IsSuccess && result.Data != null)
                {
                    PartID = result.Data.PartID.Trim();
                    _currentPoHeaderPromiseDate = null;
                    _workflowService.CurrentPODueDate = null;
                    ReplaceParts([result.Data], result.Data);
                    var msg = await _receivingSettings.FormatAsync(
                        ReceivingSettingsKeys.Messages.InfoPartFound,
                        result.Data.PartID
                    );
                    _workflowService.RaiseStatusMessage(msg);
                }
                else if (IsNonPOItem)
                {
                    var fuzzyResults = await _inforVisualService.FuzzySearchPartsAsync(searchTerm);
                    if (!fuzzyResults.IsSuccess)
                    {
                        await _errorHandler.HandleErrorAsync(
                            fuzzyResults.ErrorMessage
                                ?? await _receivingSettings.GetStringAsync(
                                    ReceivingSettingsKeys.Messages.ErrorPartNotFound
                                ),
                            Enum_ErrorSeverity.Error
                        );
                        ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
                        _workflowService.CurrentLocation = string.Empty;
                        return;
                    }

                    if (fuzzyResults.Data is null || fuzzyResults.Data.Count == 0)
                    {
                        await _errorHandler.HandleErrorAsync(
                            result.ErrorMessage
                                ?? await _receivingSettings.GetStringAsync(
                                    ReceivingSettingsKeys.Messages.ErrorPartNotFound
                                ),
                            Enum_ErrorSeverity.Error
                        );
                        ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
                        _workflowService.CurrentLocation = string.Empty;
                        return;
                    }

                    var selectedResult = await ShowPartFuzzyPickerAsync(
                        searchTerm,
                        fuzzyResults.Data
                    );
                    if (selectedResult is null)
                    {
                        return;
                    }

                    var selectedPartResult = await _inforVisualService.GetPartByIDAsync(
                        selectedResult.Key.Trim()
                    );

                    if (!selectedPartResult.IsSuccess || selectedPartResult.Data is null)
                    {
                        await _errorHandler.HandleErrorAsync(
                            selectedPartResult.ErrorMessage
                                ?? await _receivingSettings.GetStringAsync(
                                    ReceivingSettingsKeys.Messages.ErrorPartNotFound
                                ),
                            Enum_ErrorSeverity.Error
                        );
                        ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
                        _workflowService.CurrentLocation = string.Empty;
                        return;
                    }

                    PartID = selectedPartResult.Data.PartID.Trim();
                    _currentPoHeaderPromiseDate = null;
                    _workflowService.CurrentPODueDate = null;
                    ReplaceParts([selectedPartResult.Data], selectedPartResult.Data);
                    var msg = await _receivingSettings.FormatAsync(
                        ReceivingSettingsKeys.Messages.InfoPartFound,
                        selectedPartResult.Data.PartID
                    );
                    _workflowService.RaiseStatusMessage(msg);
                }
                else
                {
                    await _errorHandler.HandleErrorAsync(
                        result.ErrorMessage
                            ?? await _receivingSettings.GetStringAsync(
                                ReceivingSettingsKeys.Messages.ErrorPartNotFound
                            ),
                        Enum_ErrorSeverity.Error
                    );
                    ReplaceParts(Array.Empty<Model_InforVisualPart>(), clearSelection: true);
                    _workflowService.CurrentLocation = string.Empty;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<Model_FuzzySearchResult?> ShowPartFuzzyPickerAsync(
            string searchTerm,
            IReadOnlyList<Model_FuzzySearchResult> items
        )
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is null)
            {
                await _errorHandler.HandleErrorAsync(
                    "Unable to display part selection dialog.",
                    Enum_ErrorSeverity.Error
                );
                return null;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                items,
                "Select Part",
                $"No exact part match was found for '{searchTerm}'. Select a similar part to continue."
            )
            {
                XamlRoot = xamlRoot,
            };

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
                || dialog.SelectedResult is null
            )
            {
                return null;
            }

            return dialog.SelectedResult;
        }

        private void ReplaceParts(
            IEnumerable<Model_InforVisualPart> parts,
            Model_InforVisualPart? selectedPart = null,
            bool clearSelection = false
        )
        {
            var selectedPartId = clearSelection
                ? null
                : selectedPart?.PartID ?? SelectedPart?.PartID;

            Parts = new ObservableCollection<Model_InforVisualPart>(parts);
            AttachPartsCollectionHandler();
            IsPartsListVisible = Parts.Count > 0;
            SelectedPart = string.IsNullOrWhiteSpace(selectedPartId)
                ? null
                : Parts.FirstOrDefault(part =>
                    string.Equals(part.PartID, selectedPartId, StringComparison.OrdinalIgnoreCase)
                );
        }

        private void AttachPartsCollectionHandler()
        {
            if (_partsCollectionChangedHandler != null)
            {
                Parts.CollectionChanged -= _partsCollectionChangedHandler;
            }

            _partsCollectionChangedHandler = (s, e) => IsPartsListVisible = Parts.Count > 0;
            Parts.CollectionChanged += _partsCollectionChangedHandler;
            IsPartsListVisible = Parts.Count > 0;
        }

        partial void OnPoNumberChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _workflowService.CurrentPONumber = string.Empty;
                _currentPoHeaderPromiseDate = null;
                _workflowService.CurrentPODueDate = null;
                IsLoadPOEnabled = false;
                PoValidationMessage = string.Empty;
                return;
            }

            if (!TryNormalizePoNumber(value, out var validatedPoNumber, out var validationMessage))
            {
                PoValidationMessage = validationMessage;
                IsLoadPOEnabled = false;
                return;
            }

            _workflowService.CurrentPONumber = validatedPoNumber;
            IsLoadPOEnabled = true;
            PoValidationMessage = string.Empty;
        }

        private static bool TryNormalizePoNumber(
            string? value,
            out string normalizedPoNumber,
            out string validationMessage
        )
        {
            normalizedPoNumber = string.Empty;
            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var match = CanonicalPoNumberPattern.Match(value.Trim());
            if (!match.Success)
            {
                validationMessage =
                    "Invalid PO format. Enter: 66868, 66868B, PO-066868, or PO-066868B";
                return false;
            }

            var digits = match.Groups["digits"].Value;
            if (digits.Length > 6)
            {
                validationMessage = "PO number must be 6 digits or less";
                return false;
            }

            var suffix = match.Groups["suffix"].Value.ToUpperInvariant();
            normalizedPoNumber = $"PO-{digits.PadLeft(6, '0')}{suffix}";
            return true;
        }

        partial void OnPartIDChanged(string value)
        {
            // Auto-detect package type based on Part ID prefix
            if (string.IsNullOrWhiteSpace(value))
            {
                PackageType = "Skids";
                return;
            }

            var upperPart = value.Trim().ToUpper();

            if (upperPart.StartsWith("MMC"))
                PackageType = "Coils";
            else if (upperPart.StartsWith("MMF"))
                PackageType = "Sheets";
            else
                PackageType = "Skids";
        }

        partial void OnSelectedPartChanged(Model_InforVisualPart? value)
        {
            _workflowService.CurrentPart = value;
            _workflowService.CurrentPODueDate = IsNonPOItem
                ? null
                : value?.DueDate ?? _currentPoHeaderPromiseDate;
            _workflowService.CurrentLocation = value?.DefaultLocationId?.Trim() ?? string.Empty;

            if (_isClearingRestrictedSelection)
            {
                NotifyWorkflowNextButtonStateChanged();
                return;
            }

            // Auto-detect package type when a part is selected from PO
            if (value != null && !string.IsNullOrWhiteSpace(value.PartID))
            {
                var upperPart = value.PartID.Trim().ToUpper();

                if (upperPart.StartsWith("MMC"))
                    PackageType = "Coils";
                else if (upperPart.StartsWith("MMF"))
                    PackageType = "Sheets";
                else
                    PackageType = "Skids";

                _ = CheckQualityHoldOnSelectedPartAsync(value);
            }

            NotifyWorkflowNextButtonStateChanged();
        }

        private void NotifyWorkflowNextButtonStateChanged()
        {
            foreach (
                var workflowViewModel in _viewModelRegistry.GetViewModels<ViewModel_Receiving_Workflow>()
            )
            {
                workflowViewModel.RefreshNextButtonEnabled();
            }
        }

        private async Task CheckQualityHoldOnSelectedPartAsync(Model_InforVisualPart selectedPart)
        {
            if (_qualityHoldWarning.IsRestrictedPart(selectedPart.PartID) is false)
            {
                return;
            }

            bool acknowledged = await _qualityHoldWarning.CheckAndWarnAsync(selectedPart.PartID);
            if (acknowledged)
            {
                return;
            }

            _isClearingRestrictedSelection = true;
            try
            {
                SelectedPart = null;
                _workflowService.CurrentPart = null;
                _workflowService.CurrentLocation = string.Empty;
                _workflowService.CurrentPODueDate = IsNonPOItem
                    ? null
                    : _currentPoHeaderPromiseDate;

                if (IsNonPOItem)
                {
                    PartID = string.Empty;
                }
            }
            finally
            {
                _isClearingRestrictedSelection = false;
            }
        }

        /// <summary>
        /// Shows contextual help for PO entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.POEntry");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion

        /// <summary>
        /// Initialize component - autofill PO if mock data enabled
        /// </summary>
        private async void InitializeAsync()
        {
            try
            {
                var useMockData = _configuration.GetValue<bool>(
                    "AppSettings:UseInforVisualMockData"
                );

                if (useMockData)
                {
                    var defaultPO =
                        _mockDataCatalog.GetDefaultPurchaseOrderNumber()
                        ?? _configuration.GetValue<string>("AppSettings:DefaultMockPONumber");

                    if (string.IsNullOrWhiteSpace(defaultPO))
                    {
                        await _logger.LogWarningAsync(
                            "[MOCK DATA MODE] No default mock PO number was found in the mock catalog."
                        );
                        return;
                    }

                    await _logger.LogInfoAsync(
                        $"[MOCK DATA MODE] Auto-filling PO number: {defaultPO}"
                    );

                    PoNumber = defaultPO;
                    _workflowService.RaiseStatusMessage($"[MOCK DATA] Auto-filled PO: {defaultPO}");

                    // Auto-load mock data
                    await Task.Delay(500); // Small delay for UI update
                    await LoadPOAsync();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error during initialization: {ex.Message}", ex);
            }
        }
    }
}
