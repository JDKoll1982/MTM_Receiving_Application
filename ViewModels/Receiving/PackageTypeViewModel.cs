using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class PackageTypeViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_PackagePreferences _preferencesService;
        private readonly IService_ReceivingValidation _validationService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private ObservableCollection<string> _packageTypes = new() { "Coils", "Sheets", "Skids", "Custom" };

        [ObservableProperty]
        private string _selectedPackageType = string.Empty;

        [ObservableProperty]
        private string _customPackageTypeName = string.Empty;

        [ObservableProperty]
        private bool _isCustomTypeVisible;

        [ObservableProperty]
        private bool _isSaveAsDefault;

        public PackageTypeViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_PackagePreferences preferencesService,
            IService_ReceivingValidation validationService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _preferencesService = preferencesService;
            _validationService = validationService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == WorkflowStep.PackageTypeEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public async Task OnNavigatedToAsync()
        {
            Loads.Clear();
            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                }
            }

            await LoadPreferencesAsync();
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
                if (PackageTypes.Contains(preference.PackageTypeName))
                {
                    SelectedPackageType = preference.PackageTypeName;
                }
                else
                {
                    SelectedPackageType = "Custom";
                    CustomPackageTypeName = preference.PackageTypeName;
                }
            }
            else
            {
                // Apply smart defaults
                var partType = _workflowService.CurrentPart?.PartType;
                if (partType == "MMC")
                {
                    SelectedPackageType = "Coils";
                }
                else if (partType == "MMF")
                {
                    SelectedPackageType = "Sheets";
                }
                else
                {
                    SelectedPackageType = "Skids"; // Default fallback
                }
            }
        }

        partial void OnSelectedPackageTypeChanged(string value)
        {
            IsCustomTypeVisible = value == "Custom";
            UpdateLoadsPackageType();
            if (IsSaveAsDefault) SavePreferenceAsync().ConfigureAwait(false);
        }

        partial void OnCustomPackageTypeNameChanged(string value)
        {
            if (SelectedPackageType == "Custom")
            {
                UpdateLoadsPackageType();
                if (IsSaveAsDefault) SavePreferenceAsync().ConfigureAwait(false);
            }
        }

        partial void OnIsSaveAsDefaultChanged(bool value)
        {
            if (value)
            {
                SavePreferenceAsync().ConfigureAwait(false);
            }
            else
            {
                // Optional: Delete preference if unchecked?
                // For now, we just don't update it.
                // Or we could delete it.
                // Let's delete it to be consistent with "Default" concept.
                DeletePreferenceAsync().ConfigureAwait(false);
            }
        }

        private void UpdateLoadsPackageType()
        {
            var typeName = SelectedPackageType == "Custom" ? CustomPackageTypeName : SelectedPackageType;
            
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

            var typeName = SelectedPackageType == "Custom" ? CustomPackageTypeName : SelectedPackageType;
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
    }
}
