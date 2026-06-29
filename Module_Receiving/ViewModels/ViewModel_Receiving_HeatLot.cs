using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Text.Json;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_HeatLot : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _heatLotHeaderText = "Load Entries";

        [ObservableProperty]
        private string _heatLotAutoFillText = "Auto-Fill";

        [ObservableProperty]
        private string _heatLotAutoFillTooltipText = "Fill blank heat numbers from rows above";

        [ObservableProperty]
        private string _heatLotLoadPrefixText = "Load #{0}";

        [ObservableProperty]
        private string _heatLotFieldHeaderText = "Heat/Lot Number (Optional)";

        [ObservableProperty]
        private string _heatLotFieldPlaceholderText = "Enter Heat or Lot Number";

        [ObservableProperty]
        private ObservableCollection<string> _heatLotPresetFillers = new();

        [ObservableProperty]
        private string _selectedHeatLotPresetFiller = string.Empty;

        [ObservableProperty]
        private Visibility _userSetVariableFieldVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _userSetVariableFieldHeaderText = "User Set Variable";

        [ObservableProperty]
        private string _userSetVariableFieldPlaceholderText = "Enter variable value";

        [ObservableProperty]
        private string _userSetVariableAccessibilityName = "User Set Variable";

        private Model_ReceivingVendorVariableMapping? _activeVendorVariableMapping;

        // Accessibility Properties
        [ObservableProperty]
        private string _heatLotAccessibilityName = "Heat Lot Number";

        private string _currentPartId = string.Empty;

        public string CurrentPartId
        {
            get => _currentPartId;
            private set => SetProperty(ref _currentPartId, value);
        }

        private string _currentPartDescription = string.Empty;

        public string CurrentPartDescription
        {
            get => _currentPartDescription;
            private set => SetProperty(ref _currentPartDescription, value);
        }

        public ViewModel_Receiving_HeatLot(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService
        )
            : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;

            _workflowService.StepChanged += OnStepChanged;

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                HeatLotHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotHeader
                );
                HeatLotAutoFillText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotAutoFill
                );
                HeatLotAutoFillTooltipText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotAutoFillTooltip
                );
                HeatLotLoadPrefixText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotLoadPrefix
                );
                HeatLotFieldHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotFieldHeader
                );
                HeatLotFieldPlaceholderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotFieldPlaceholder
                );
                HeatLotAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.HeatLotNumber
                );

                await LoadHeatLotPresetFillersAsync();

                _logger.LogInfo("Heat/Lot UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading Heat/Lot UI text from settings: {ex.Message}", ex);
            }
        }

        public string FormatHeatLotLoadLabel(int loadNumber)
        {
            return string.Format(HeatLotLoadPrefixText, loadNumber);
        }

        private async Task LoadHeatLotPresetFillersAsync()
        {
            try
            {
                var presetFillersJson = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.HeatLotPresetFillers
                );

                HeatLotPresetFillers = new ObservableCollection<string>(
                    DeserializeHeatLotPresetFillers(presetFillersJson)
                );
                SelectedHeatLotPresetFiller = HeatLotPresetFillers.FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error loading Heat/Lot preset fillers from settings: {ex.Message}",
                    ex
                );

                HeatLotPresetFillers = new ObservableCollection<string>(
                    GetDefaultHeatLotPresetFillers()
                );
                SelectedHeatLotPresetFiller = HeatLotPresetFillers.FirstOrDefault() ?? string.Empty;
            }
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.HeatLotEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        [RelayCommand]
        private void ApplyHeatLotPreset()
        {
            if (string.IsNullOrWhiteSpace(SelectedHeatLotPresetFiller))
            {
                return;
            }

            foreach (var load in Loads)
            {
                load.HeatLotNumber = SelectedHeatLotPresetFiller;
            }
        }

        public async Task OnNavigatedToAsync()
        {
            IEnumerable<Model_ReceivingLoad> sessionLoads = _workflowService.CurrentSession is null
                ? new List<Model_ReceivingLoad>()
                : _workflowService.CurrentSession.Loads;
            Loads = new ObservableCollection<Model_ReceivingLoad>(sessionLoads);

            RefreshCurrentPartInfo();

            await RefreshVendorVariableFieldAsync();

            await Task.CompletedTask;
        }

        [RelayCommand]
        private void AutoFill()
        {
            // Fill down logic:
            // Iterate through loads. If a load has a blank HeatLotNumber,
            // copy it from the previous load (if available).
            for (int i = 1; i < Loads.Count; i++)
            {
                var currentLoad = Loads[i];
                var prevLoad = Loads[i - 1];

                if (
                    string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber)
                    && !string.IsNullOrWhiteSpace(prevLoad.HeatLotNumber)
                )
                {
                    currentLoad.HeatLotNumber = prevLoad.HeatLotNumber;
                }
            }
        }

        [RelayCommand]
        private Task ValidateAndContinueAsync()
        {
            // Set "Not Entered" for any blank heat/lot fields before advancing
            PrepareHeatLotFields();

            // Validation logic is handled by Service_ReceivingWorkflow when advancing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ensures all heat/lot fields have a value. Sets "Nothing Entered" for blank fields.
        /// </summary>
        private void PrepareHeatLotFields()
        {
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = "Nothing Entered";
                }
            }
        }

        private void RefreshCurrentPartInfo()
        {
            CurrentPartId = _workflowService.CurrentPart?.PartID?.Trim() ?? string.Empty;
            CurrentPartDescription = _workflowService.CurrentPart?.Description?.Trim() ?? string.Empty;
        }

        private static IEnumerable<string> DeserializeHeatLotPresetFillers(string json)
        {
            try
            {
                var fillers = string.IsNullOrWhiteSpace(json)
                    ? GetDefaultHeatLotPresetFillers()
                    : JsonSerializer.Deserialize<string[]>(json) ?? GetDefaultHeatLotPresetFillers();

                return fillers.Where(static item => string.IsNullOrWhiteSpace(item) is false).Select(
                    item => item.Trim()
                );
            }
            catch
            {
                return GetDefaultHeatLotPresetFillers();
            }
        }

        private static IReadOnlyList<string> GetDefaultHeatLotPresetFillers()
        {
            return
            [
                "Refer to Vendor Tag",
                "N/A",
                "Old Coil",
                "Old Flatstock",
                "Old Product",
            ];
        }

        private async Task RefreshVendorVariableFieldAsync()
        {
            try
            {
                var vendorMappingsJson = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UserPreferences.VendorVariableMappingsJson
                );
                var vendorMappings = DeserializeVendorVariableMappings(vendorMappingsJson)
                    .Select(m =>
                    {
                        m.VendorName = m.VendorName?.Trim() ?? string.Empty;
                        m.VariableName = m.VariableName?.Trim() ?? string.Empty;
                        return m;
                    })
                    .ToList();
                var currentVendorName = _workflowService.CurrentPOVendor?.Trim();

                _logger.LogInfo($"Refreshing vendor variable field. Current vendor: '{currentVendorName ?? "(null)"}'. Mappings count: {vendorMappings?.Count ?? 0}");

                _activeVendorVariableMapping = vendorMappings
                    .Where(mapping => mapping.MatchesVendorName(currentVendorName))
                    .OrderByDescending(mapping => mapping.VendorName.Length)
                    .ThenBy(mapping => mapping.VendorName, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (_activeVendorVariableMapping is not null)
                {
                    _logger.LogInfo($"Matched vendor mapping: VendorName='{_activeVendorVariableMapping.VendorName}', VariableName='{_activeVendorVariableMapping.VariableName}'");
                }

                if (_activeVendorVariableMapping is null)
                {
                    UserSetVariableFieldVisibility = Visibility.Collapsed;
                    UserSetVariableFieldHeaderText = "User Set Variable";
                    UserSetVariableFieldPlaceholderText = "Enter variable value";
                    UserSetVariableAccessibilityName = "User Set Variable";

                    foreach (var load in Loads)
                    {
                        load.UserSetCustomerName = currentVendorName ?? string.Empty;
                        load.UserSetVariable = string.Empty;
                        load.UserSetVariableFieldVisibility = Visibility.Collapsed;
                        load.UserSetVariableFieldHeaderText = "User Set Variable";
                        load.UserSetVariableFieldPlaceholderText = "Enter variable value";
                        load.UserSetVariableAccessibilityName = "User Set Variable";
                    }

                    return;
                }

                UserSetVariableFieldVisibility = Visibility.Visible;
                UserSetVariableFieldHeaderText = _activeVendorVariableMapping.VariableName;
                UserSetVariableFieldPlaceholderText =
                    $"Enter {_activeVendorVariableMapping.VariableName}";
                UserSetVariableAccessibilityName = _activeVendorVariableMapping.VariableName;

                foreach (var load in Loads)
                {
                    if (
                        string.Equals(
                            load.UserSetCustomerName,
                            currentVendorName,
                            StringComparison.OrdinalIgnoreCase
                        )
                        is false
                    )
                    {
                        load.UserSetVariable = string.Empty;
                    }

                    load.UserSetCustomerName = currentVendorName ?? string.Empty;
                    load.UserSetVariableFieldVisibility = Visibility.Visible;
                    load.UserSetVariableFieldHeaderText = _activeVendorVariableMapping.VariableName;
                    load.UserSetVariableFieldPlaceholderText =
                        $"Enter {_activeVendorVariableMapping.VariableName}";
                    load.UserSetVariableAccessibilityName =
                        _activeVendorVariableMapping.VariableName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading vendor variable mapping: {ex.Message}", ex);
                UserSetVariableFieldVisibility = Visibility.Collapsed;
                UserSetVariableFieldHeaderText = "User Set Variable";
                UserSetVariableFieldPlaceholderText = "Enter variable value";
                UserSetVariableAccessibilityName = "User Set Variable";
            }
        }

        private static IEnumerable<Model_ReceivingVendorVariableMapping> DeserializeVendorVariableMappings(
            string json
        )
        {
            try
            {
                return string.IsNullOrWhiteSpace(json)
                    ?
                    [
                        new Model_ReceivingVendorVariableMapping
                        {
                            VendorName = "Skana",
                            VariableName = "DM #",
                        },
                    ]
                    : System.Text.Json.JsonSerializer.Deserialize<Model_ReceivingVendorVariableMapping[]>(
                        json
                    )
                        ??
                        [
                            new Model_ReceivingVendorVariableMapping
                            {
                                VendorName = "Skana",
                                VariableName = "DM #",
                            },
                        ];
            }
            catch
            {
                return
                [
                    new Model_ReceivingVendorVariableMapping
                    {
                        VendorName = "Skana",
                        VariableName = "DM #",
                    },
                ];
            }
        }

        /// <summary>
        /// Shows contextual help for heat/lot entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.HeatLot");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}
