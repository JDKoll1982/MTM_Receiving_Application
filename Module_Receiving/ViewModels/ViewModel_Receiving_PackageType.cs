using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_PackageType : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_PackagePreferences _preferencesService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;

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
        private static readonly System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(@"^[\w\s\-\.\(\)]+$");

        public ViewModel_Receiving_PackageType(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_PackagePreferences preferencesService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _preferencesService = preferencesService;
            _validationService = validationService;
            _helpService = helpService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.PackageTypeEntry)
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
            if (string.IsNullOrEmpty(partId))
            {
                return;
            }

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
                // Apply smart defaults based on PartID prefix
                var partID = _workflowService.CurrentPart?.PartID ?? string.Empty;
                if (partID.StartsWith("MMC", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedPackageType = "Coils";
                    _logger?.LogInfo($"Auto-set package type to Coils for part {partID}");
                }
                else if (partID.StartsWith("MMF", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedPackageType = "Sheets";
                    _logger?.LogInfo($"Auto-set package type to Sheets for part {partID}");
                }
                else
                {
                    SelectedPackageType = "Skids"; // Default fallback
                    _logger?.LogInfo($"Auto-set package type to Skids (default) for part {partID}");
                }
            }
        }

        partial void OnSelectedPackageTypeChanged(string value)
        {
            IsCustomTypeVisible = value == "Custom";
            UpdateLoadsPackageType();
            if (IsSaveAsDefault)
                SavePreferenceAsync().ConfigureAwait(false);
        }

        partial void OnCustomPackageTypeNameChanged(string value)
        {
            if (SelectedPackageType == "Custom")
            {
                UpdateLoadsPackageType();
                if (IsSaveAsDefault)
                    SavePreferenceAsync().ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(partId))
            {
                return;
            }

            if (partId.Length > 50)
            {
                _workflowService.RaiseStatusMessage("Part ID too long (max 50 chars).");
                return;
            }

            var typeName = SelectedPackageType == "Custom" ? CustomPackageTypeName : SelectedPackageType;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                _workflowService.RaiseStatusMessage("Please enter a package type name.");
                return;
            }

            if (typeName.Length > 50)
            {
                _workflowService.RaiseStatusMessage("Package type name too long (max 50 chars).");
                return;
            }

            // T020b: Ensure PackageTypeName has valid characters
            // Allow alphanumeric, spaces, hyphens, dots, parentheses
            if (!_regex.IsMatch(typeName))
            {
                _workflowService.RaiseStatusMessage("Invalid characters in package type name.");
                return;
            }

            var preference = new Model_PackageTypePreference
            {
                PartID = partId,
                PackageTypeName = typeName,
                CustomTypeName = SelectedPackageType == "Custom" ? CustomPackageTypeName : null
            };

            try
            {
                await _preferencesService.SavePreferenceAsync(preference);
                _workflowService.RaiseStatusMessage("Preference saved.");
            }
            catch (System.Exception ex)
            {
                // T020d: Add more detailed error logging
                var msg = $"Failed to save preference. PartID: '{partId}', Type: '{typeName}', Custom: '{preference.CustomTypeName}'. Error: {ex.Message}";
                await _errorHandler.HandleErrorAsync(msg, Enum_ErrorSeverity.Warning, ex);
            }
        }

        private async Task DeletePreferenceAsync()
        {
            var partId = _workflowService.CurrentPart?.PartID;
            if (string.IsNullOrEmpty(partId))
            {
                return;
            }

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
        /// Shows contextual help for package type selection
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.PackageType");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

