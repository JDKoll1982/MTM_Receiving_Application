using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Package Type Entry step.
    /// Allows user to select package type and count for all loads with preference saving.
    /// </summary>
    public partial class PackageTypeViewModel : BaseStepViewModel<PackageTypeData>
    {
        private readonly IService_MySQL_PackagePreferences _preferencesService;
        private readonly IService_ReceivingValidation _validationService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        public PackageTypeViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_PackagePreferences preferencesService,
            IService_ReceivingValidation validationService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
            _preferencesService = preferencesService;
            _validationService = validationService;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.PackageTypeEntry;

        /// <summary>
        /// Called when this step becomes active. Load loads from session and preferences.
        /// </summary>
        protected override async Task OnNavigatedToAsync()
        {
            Loads.Clear();
            StepData.Loads.Clear();
            
            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                    StepData.Loads.Add(load);
                }
            }

            await LoadPreferencesAsync();
            await base.OnNavigatedToAsync();
        }

        private async Task LoadPreferencesAsync()
        {
            var partId = _workflowService.CurrentPart?.PartID;
            if (string.IsNullOrEmpty(partId)) return;

            // Try to load from DB
            var preference = await _preferencesService.GetPreferenceAsync(partId);
            if (preference != null)
            {
                // Apply saved preference
                if (StepData.AvailablePackageTypes.Contains(preference.PackageTypeName))
                {
                    StepData.SelectedPackageType = preference.PackageTypeName;
                }
                else
                {
                    StepData.SelectedPackageType = "Custom";
                    StepData.CustomPackageTypeName = preference.PackageTypeName;
                }
            }
            else
            {
                // Apply smart defaults based on part type
                var partType = _workflowService.CurrentPart?.PartType;
                if (partType == "MMC")
                {
                    StepData.SelectedPackageType = "Coils";
                }
                else if (partType == "MMF")
                {
                    StepData.SelectedPackageType = "Sheets";
                }
                else
                {
                    StepData.SelectedPackageType = "Skids";
                }
            }
            
            // Update visibility and apply to loads
            StepData.IsCustomTypeVisible = StepData.SelectedPackageType == "Custom";
            UpdateLoadsPackageType();
        }

        private void UpdateLoadsPackageType()
        {
            var typeName = StepData.SelectedPackageType == "Custom" 
                ? StepData.CustomPackageTypeName 
                : StepData.SelectedPackageType;
            
            foreach (var load in Loads)
            {
                load.PackageTypeName = typeName;
            }
        }

        [RelayCommand]
        private async Task SavePreferenceAsync()
        {
            var partId = _workflowService.CurrentPart?.PartID;
            if (string.IsNullOrEmpty(partId)) return;

            var typeName = StepData.SelectedPackageType == "Custom" 
                ? StepData.CustomPackageTypeName 
                : StepData.SelectedPackageType;
            if (string.IsNullOrWhiteSpace(typeName)) return;

            var preference = new Model_PackageTypePreference
            {
                PartID = partId,
                PackageTypeName = typeName
            };

            try
            {
                await _preferencesService.SavePreferenceAsync(preference);
                _workflowService.RaiseStatusMessage("Preference saved.");
            }
            catch (System.Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Failed to save preference", Enum_ErrorSeverity.Warning, ex);
            }
        }

        private async Task DeletePreferenceAsync()
        {
            var partId = _workflowService.CurrentPart?.PartID;
            if (string.IsNullOrEmpty(partId)) return;

            try
            {
                await _preferencesService.DeletePreferenceAsync(partId);
                _workflowService.RaiseStatusMessage("Preference deleted.");
            }
            catch (System.Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Failed to delete preference", Enum_ErrorSeverity.Warning, ex);
            }
        }

        /// <summary>
        /// Validates package type and count before advancing.
        /// </summary>
        protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            foreach (var load in StepData.Loads)
            {
                var countResult = _validationService.ValidatePackageCount(load.PackagesPerLoad);
                if (!countResult.IsValid)
                {
                    return Task.FromResult((false, $"Load {load.LoadNumber}: {countResult.Message}"));
                }

                if (string.IsNullOrWhiteSpace(load.PackageTypeName))
                {
                    return Task.FromResult((false, $"Load {load.LoadNumber}: Package Type is required."));
                }
            }

            return Task.FromResult((true, string.Empty));
        }
    }
}
