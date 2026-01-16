using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_LoadEntry : ViewModel_Shared_Base, IResettableViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_ViewModelRegistry _viewModelRegistry;

        [ObservableProperty]
        private int _numberOfLoads = 1;

        [ObservableProperty]
        private string _selectedPartInfo = string.Empty;

        public ViewModel_Receiving_LoadEntry(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_ViewModelRegistry viewModelRegistry)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;
            _viewModelRegistry = viewModelRegistry;

            _workflowService.StepChanged += OnStepChanged;
            _viewModelRegistry.Register(this);
        }

        public void ResetToDefaults()
        {
            NumberOfLoads = 1;
            SelectedPartInfo = string.Empty;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.LoadEntry)
            {
                NumberOfLoads = _workflowService.NumberOfLoads;
            }
        }

        [RelayCommand]
        private async Task CreateLoadsAsync()
        {
            var validationResult = _validationService.ValidateNumberOfLoads(NumberOfLoads);
            if (!validationResult.IsValid)
            {
                await _errorHandler.HandleErrorAsync(validationResult.Message, Enum_ErrorSeverity.Warning);
                return;
            }

            // Sync with service
            _workflowService.NumberOfLoads = NumberOfLoads;
        }

        partial void OnNumberOfLoadsChanged(int value)
        {
            _workflowService.NumberOfLoads = value;
        }

        public void UpdatePartInfo(string partId, string description)
        {
            SelectedPartInfo = $"{partId} - {description}";
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

