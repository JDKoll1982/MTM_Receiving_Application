using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_LoadEntry : ViewModel_Shared_Base, IResettableViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_Help _helpService;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_ReceivingSettings _receivingSettings;
        private readonly IService_ReceivingLocationReconciliation _locationReconciliationService;

        [ObservableProperty]
        private int _numberOfLoads = 1;

        [ObservableProperty]
        private string _selectedPartId = string.Empty;

        [ObservableProperty]
        private string _selectedPartDescription = string.Empty;

        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private bool _isMockLocationMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasRecommendedLocations))]
        private ObservableCollection<Model_ReceivingRecommendedLocation> _recommendedLocations =
            new();

        [ObservableProperty]
        private bool _isRecommendedLocationsLoading;

        [ObservableProperty]
        private string _recommendedLocationsMessage =
            "Current stock recommendations appear after a guided part is selected.";

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _loadEntryHeaderText = "Number of Loads (1-99)";

        [ObservableProperty]
        private string _loadEntryInstructionText =
            "Enter the total number of skids/loads for this part.";

        [ObservableProperty]
        private string _loadEntryLocationHeaderText = "Location (Optional)";

        [ObservableProperty]
        private string _loadEntryLocationInstructionText =
            "Leave blank to save as Nothing Entered.";

        // Accessibility Properties
        [ObservableProperty]
        private string _numberOfLoadsAccessibilityName = "Number of Loads";

        public ObservableCollection<string> PresetLocations { get; } = new();

        public bool IsLiveLocationMode => !IsMockLocationMode;

        public bool HasRecommendedLocations => RecommendedLocations.Count > 0;

        public ViewModel_Receiving_LoadEntry(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_InforVisual inforVisualService,
            IService_ReceivingLocationReconciliation locationReconciliationService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_ViewModelRegistry viewModelRegistry,
            IService_Notification notificationService
        )
            : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _inforVisualService = inforVisualService;
            _locationReconciliationService = locationReconciliationService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;
            _viewModelRegistry = viewModelRegistry;
            IsMockLocationMode = _validationService.UseMockLocationList;

            foreach (var presetLocation in _validationService.PresetLocations)
            {
                PresetLocations.Add(presetLocation);
            }

            _workflowService.StepChanged += OnStepChanged;
            _viewModelRegistry.Register(this);

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                LoadEntryHeaderText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.LoadEntryHeader
                );
                LoadEntryInstructionText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.LoadEntryInstruction
                );
                NumberOfLoadsAccessibilityName = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Accessibility.LoadEntryNumberOfLoads
                );
                _logger.LogInfo("Load Entry UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error loading Load Entry UI text from settings: {ex.Message}",
                    ex
                );
            }
        }

        public void ResetToDefaults()
        {
            NumberOfLoads = 1;
            SelectedPartId = string.Empty;
            SelectedPartDescription = string.Empty;
            Location = string.Empty;
            RecommendedLocations = new ObservableCollection<Model_ReceivingRecommendedLocation>();
            RecommendedLocationsMessage =
                "Current stock recommendations appear after a guided part is selected.";
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.LoadEntry)
            {
                NumberOfLoads = _workflowService.NumberOfLoads;
                var part = _workflowService.CurrentPart;
                if (part is not null)
                {
                    UpdatePartInfo(part.PartID, part.Description);
                }
                else
                {
                    SelectedPartId = string.Empty;
                    SelectedPartDescription = string.Empty;
                }

                Location = _workflowService.CurrentLocation;
                _ = RefreshRecommendedLocationsAsync();
            }
        }

        private async Task RefreshRecommendedLocationsAsync()
        {
            if (
                _workflowService.CurrentPart is null
                || string.IsNullOrWhiteSpace(_workflowService.CurrentPONumber)
            )
            {
                RecommendedLocations =
                    new ObservableCollection<Model_ReceivingRecommendedLocation>();
                RecommendedLocationsMessage =
                    "Current stock recommendations appear after a guided part is selected.";
                return;
            }

            try
            {
                IsRecommendedLocationsLoading = true;
                RecommendedLocationsMessage = "Looking up current stock locations...";

                var recommendationResult =
                    await _locationReconciliationService.GetRecommendedLocationsAsync(
                        _workflowService.CurrentPONumber,
                        _workflowService.CurrentPart.PartID,
                        _workflowService.CurrentPart.POLineNumber,
                        DateTime.Today
                    );

                if (!recommendationResult.IsSuccess || recommendationResult.Data is null)
                {
                    RecommendedLocations =
                        new ObservableCollection<Model_ReceivingRecommendedLocation>();
                    RecommendedLocationsMessage = string.IsNullOrWhiteSpace(
                        recommendationResult.ErrorMessage
                    )
                        ? "Recommended locations are unavailable right now."
                        : recommendationResult.ErrorMessage;
                    return;
                }

                RecommendedLocations = new ObservableCollection<Model_ReceivingRecommendedLocation>(
                    recommendationResult.Data
                );
                RecommendedLocationsMessage =
                    RecommendedLocations.Count == 0
                        ? "No recommended locations were found after applying the default and user-configured ignore list."
                        : $"{RecommendedLocations.Count} recommended location(s) found from current stock inventory.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading recommended locations: {ex.Message}", ex);
                RecommendedLocations =
                    new ObservableCollection<Model_ReceivingRecommendedLocation>();
                RecommendedLocationsMessage = "Recommended locations are unavailable right now.";
            }
            finally
            {
                IsRecommendedLocationsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CreateLoadsAsync()
        {
            var validationResult = _validationService.ValidateNumberOfLoads(NumberOfLoads);
            if (!validationResult.IsValid)
            {
                await _errorHandler.HandleErrorAsync(
                    validationResult.Message,
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            // Sync with service
            _workflowService.NumberOfLoads = NumberOfLoads;
        }

        partial void OnNumberOfLoadsChanged(int value)
        {
            _workflowService.NumberOfLoads = value;
        }

        partial void OnLocationChanged(string value)
        {
            _workflowService.CurrentLocation = value?.Trim() ?? string.Empty;
        }

        public void UpdatePartInfo(string partId, string description)
        {
            SelectedPartId = partId;
            SelectedPartDescription = description;
        }

        public async Task<Model_ReceivingValidationResult> ValidateLocationAsync()
        {
            var validation = await _validationService.ValidateLocationAsync(Location);
            if (validation.IsValid)
            {
                Location = Location?.Trim() ?? string.Empty;
                return validation;
            }

            return validation;
        }

        public async Task<
            Model_Dao_Result<List<Model_FuzzySearchResult>>
        > GetLocationSuggestionsAsync(string warehouseCode = "002")
        {
            if (string.IsNullOrWhiteSpace(Location))
            {
                return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
            }

            if (IsMockLocationMode)
            {
                var normalizedSearch = NormalizeLocationForMatch(Location);
                var locationSuggestions = PresetLocations
                    .Where(presetLocation =>
                    {
                        var normalizedPreset = NormalizeLocationForMatch(presetLocation);
                        return normalizedPreset.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase
                        );
                    })
                    .OrderByDescending(presetLocation =>
                        presetLocation.StartsWith(
                            Location.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ThenBy(presetLocation => presetLocation, StringComparer.OrdinalIgnoreCase)
                    .Select(presetLocation => new Model_FuzzySearchResult
                    {
                        Key = presetLocation,
                        Label = presetLocation,
                        Detail = "Preset mock location",
                    })
                    .ToList();

                return Model_Dao_Result_Factory.Success(locationSuggestions);
            }

            var fuzzyResult = await _inforVisualService.FuzzySearchLocationsAsync(
                Location.Trim(),
                warehouseCode
            );
            if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
            {
                return fuzzyResult;
            }

            var suggestions = fuzzyResult
                .Data.Where(static result => string.IsNullOrWhiteSpace(result.Label) is false)
                .GroupBy(static result => result.Label.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .ToList();

            return Model_Dao_Result_Factory.Success(suggestions);
        }

        private static string NormalizeLocationForMatch(string? location)
        {
            return new string(
                (location ?? string.Empty)
                    .Where(char.IsLetterOrDigit)
                    .Select(char.ToUpperInvariant)
                    .ToArray()
            );
        }

        /// <summary>
        /// Shows contextual help for load entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.LoadEntry");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}
