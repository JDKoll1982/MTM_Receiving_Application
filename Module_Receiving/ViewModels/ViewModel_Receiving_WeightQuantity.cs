using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_WeightQuantity : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private string _warningMessage = string.Empty;

        [ObservableProperty]
        private bool _hasWarning;

        [ObservableProperty]
        private string _poQuantityInfo = string.Empty;

        private string _currentTotal = string.Empty;

        public string CurrentTotal
        {
            get => _currentTotal;
            private set
            {
                if (_currentTotal == value)
                {
                    return;
                }

                _currentTotal = value;
                OnPropertyChanged(nameof(CurrentTotal));
            }
        }

        [ObservableProperty]
        private string _currentPartId = string.Empty;

        [ObservableProperty]
        private string _currentPartDescription = string.Empty;

        [ObservableProperty]
        private int _currentNumberOfLoads;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _weightQuantityHeaderText = "Weight/Quantity";

        [ObservableProperty]
        private string _weightQuantityPlaceholderText = "Enter whole number";

        [ObservableProperty]
        private string _weightQuantityAutoFillText = "Auto Fill";

        // Accessibility Properties
        [ObservableProperty]
        private string _weightQuantityAccessibilityName = "Weight Quantity";

        public ViewModel_Receiving_WeightQuantity(
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
                WeightQuantityHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WeightQuantityHeader
                );
                WeightQuantityPlaceholderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WeightQuantityPlaceholder
                );
                WeightQuantityAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.WeightQuantityInput
                );

                _logger.LogInfo("Weight/Quantity UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error loading Weight/Quantity UI text from settings: {ex.Message}",
                    ex
                );
            }
        }

        private void OnStepChanged(object? sender, EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.WeightQuantityEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public async Task OnNavigatedToAsync()
        {
            UpdateHeaderInfo();
            HookLoadTracking(Loads);
            // Refresh loads from session
            IEnumerable<Model_ReceivingLoad> sessionLoads = _workflowService.CurrentSession is null
                ? new List<Model_ReceivingLoad>()
                : _workflowService.CurrentSession.Loads;
            Loads = new ObservableCollection<Model_ReceivingLoad>(sessionLoads);

            await UpdatePOQuantityInfoAsync();
            await CheckSameDayReceivingAsync();
        }

        /// <summary>
        /// Refreshes header values for part and load counts.
        /// </summary>
        private void UpdateHeaderInfo()
        {
            var part = _workflowService.CurrentPart;
            CurrentPartId = part?.PartID ?? string.Empty;
            CurrentPartDescription = part?.Description ?? string.Empty;
            CurrentNumberOfLoads = _workflowService.NumberOfLoads;
        }

        private async Task UpdatePOQuantityInfoAsync()
        {
            if (_workflowService.CurrentSession.IsNonPO)
            {
                PoQuantityInfo = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Messages.InfoNonPoItem
                );
            }
            else if (_workflowService.CurrentPart != null)
            {
                PoQuantityInfo = _workflowService.CurrentPart.QtyOrdered.ToString("N2");
            }
            else
            {
                PoQuantityInfo = string.Empty;
            }

            UpdateCurrentTotal();
        }

        private async Task CheckSameDayReceivingAsync()
        {
            HasWarning = false;
            WarningMessage = string.Empty;

            if (_workflowService.CurrentSession.IsNonPO)
            {
                return;
            }

            var poNumber = _workflowService.CurrentPONumber;
            var partId = _workflowService.CurrentPart?.PartID;

            if (string.IsNullOrEmpty(poNumber) || string.IsNullOrEmpty(partId))
            {
                return;
            }

            try
            {
                var totalWeight = Loads.Sum(static load => load.WeightQuantity);
                var validation = await _validationService.CheckSameDayReceivingAsync(
                    poNumber,
                    partId,
                    totalWeight
                );
                if (
                    validation.Severity == Enum_ValidationSeverity.Warning
                    && string.IsNullOrWhiteSpace(validation.Message) is false
                )
                {
                    HasWarning = true;
                    WarningMessage = validation.Message;
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.LogErrorAsync(
                    "Failed to check same-day receiving",
                    Enum_ErrorSeverity.Warning,
                    ex
                );
            }
        }

        [RelayCommand]
        private async Task ValidateAndContinueAsync()
        {
            foreach (var load in Loads)
            {
                var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
                if (!result.IsValid)
                {
                    await _errorHandler.HandleErrorAsync(
                        "There is a Load with a Weight Quantity error.",
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }
            }

            if (!_workflowService.CurrentSession.IsNonPO && _workflowService.CurrentPart != null)
            {
                var totalWeight = Loads.Sum(l => l.WeightQuantity);
                var validation = await _validationService.ValidateAgainstPOQuantityAsync(
                    totalWeight,
                    _workflowService.CurrentPart.QtyOrdered,
                    _workflowService.CurrentPart.PartID
                );

                if (
                    validation.Severity == Enum_ValidationSeverity.Warning
                    && string.IsNullOrWhiteSpace(validation.Message) is false
                )
                {
                    WarningMessage = validation.Message;
                    HasWarning = true;
                }
            }

            UpdateCurrentTotal();
        }

        private void HookLoadTracking(ObservableCollection<Model_ReceivingLoad> loads)
        {
            ArgumentNullException.ThrowIfNull(loads);

            loads.CollectionChanged += Loads_CollectionChanged;

            foreach (var load in loads)
            {
                load.PropertyChanged += Load_PropertyChanged;
            }

            UpdateCurrentTotal();
        }

        private void UnhookLoadTracking(ObservableCollection<Model_ReceivingLoad> loads)
        {
            ArgumentNullException.ThrowIfNull(loads);

            loads.CollectionChanged -= Loads_CollectionChanged;

            foreach (var load in loads)
            {
                load.PropertyChanged -= Load_PropertyChanged;
            }
        }

        partial void OnLoadsChanging(
            ObservableCollection<Model_ReceivingLoad>? oldValue,
            ObservableCollection<Model_ReceivingLoad> newValue
        )
        {
            if (oldValue is not null)
            {
                UnhookLoadTracking(oldValue);
            }
        }

        partial void OnLoadsChanged(ObservableCollection<Model_ReceivingLoad> value)
        {
            HookLoadTracking(value);
        }

        private void Loads_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (Model_ReceivingLoad load in e.OldItems)
                {
                    load.PropertyChanged -= Load_PropertyChanged;
                }
            }

            if (e.NewItems is not null)
            {
                foreach (Model_ReceivingLoad load in e.NewItems)
                {
                    load.PropertyChanged += Load_PropertyChanged;
                }
            }

            UpdateCurrentTotal();
        }

        private void Load_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Model_ReceivingLoad)
            {
                return;
            }

            UpdateCurrentTotal();
        }

        private void UpdateCurrentTotal()
        {
            CurrentTotal = Loads.Sum(load => load.WeightQuantity).ToString("N2");
        }

        [RelayCommand]
        private void AutoFill()
        {
            for (int i = 1; i < Loads.Count; i++)
            {
                var currentLoad = Loads[i];
                var previousLoad = Loads[i - 1];

                if (currentLoad.WeightQuantity == 0m && previousLoad.WeightQuantity != 0m)
                {
                    currentLoad.WeightQuantity = previousLoad.WeightQuantity;
                }
            }
        }

        /// <summary>
        /// Shows contextual help for weight/quantity entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.WeightQuantity");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}
