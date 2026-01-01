using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class Receiving_ReviewGridViewModel : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private bool _isSingleView = true;  // Default to single entry view

        [ObservableProperty]
        private int _currentEntryIndex = 0;

        [ObservableProperty]
        private Model_ReceivingLoad? _currentEntry;

        /// <summary>
        /// Display index (1-based) for UI
        /// </summary>
        public int DisplayIndex => CurrentEntryIndex + 1;

        /// <summary>
        /// Indicates if the Previous button should be enabled
        /// </summary>
        public bool CanGoBack => CurrentEntryIndex > 0;

        /// <summary>
        /// Indicates if the Next button should be enabled
        /// </summary>
        public bool CanGoNext => Loads.Count > 0 && CurrentEntryIndex < Loads.Count - 1;

        /// <summary>
        /// Inverse of IsSingleView for binding table view visibility
        /// </summary>
        public bool IsTableView => !IsSingleView;

        public Receiving_ReviewGridViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Review)
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

                // Set current entry to first load if available
                if (Loads.Count > 0)
                {
                    CurrentEntryIndex = 0;
                    CurrentEntry = Loads[0];
                }
            }

            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(DisplayIndex));
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void PreviousEntry()
        {
            if (CurrentEntryIndex > 0)
            {
                CurrentEntryIndex--;
                CurrentEntry = Loads[CurrentEntryIndex];
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }

        [RelayCommand]
        private void NextEntry()
        {
            if (CurrentEntryIndex < Loads.Count - 1)
            {
                CurrentEntryIndex++;
                CurrentEntry = Loads[CurrentEntryIndex];
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }

        [RelayCommand]
        private void SwitchToTableView()
        {
            IsSingleView = false;
            OnPropertyChanged(nameof(IsTableView));
        }

        [RelayCommand]
        private void SwitchToSingleView()
        {
            IsSingleView = true;
            OnPropertyChanged(nameof(IsTableView));
        }

        [RelayCommand]
        private async Task AddAnotherPartAsync()
        {
            // This will be implemented fully in User Story 3
            // For now, it should probably just reset to PO Entry or Part Selection
            // But we need to make sure current loads are saved/accumulated in session.

            // The workflow service should handle "AddCurrentPartToSessionAsync"
            await _workflowService.AddCurrentPartToSessionAsync();

            // Navigate to PO Entry (or Part Selection if same PO?)
            // For MVP US1, we might not fully support this yet, but the button is requested.
            // Let's assume we go back to PO Entry.
            _workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Trigger the save workflow
            // This moves to "Saving" step
            await _workflowService.AdvanceToNextStepAsync();
        }

        public void HandleCascadingUpdate(Model_ReceivingLoad changedLoad, string propertyName)
        {
            if (changedLoad == null)
            {
                return;
            }

            // Logic for cascading updates
            // If user changes a value in row X, should it update X+1, X+2...?
            // Spec says: "Review Grid with Cascading Updates"
            // Usually this means if I change Heat# on Load 1, it might ask to update others, 
            // or if I change PO# it updates all for that part?

            // T044 says: "cascading update logic for Part# and PO#"

            var index = Loads.IndexOf(changedLoad);
            if (index < 0)
            {
                return;
            }

            if (propertyName == nameof(Model_ReceivingLoad.PoNumber))
            {
                // Update all subsequent loads? Or all loads for this part?
                // If we are reviewing the current session, and it's a single part (US1), 
                // changing PO probably implies changing it for all loads of this part.
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PoNumber = changedLoad.PoNumber;
                    }
                }
            }
            else if (propertyName == nameof(Model_ReceivingLoad.PartID))
            {
                // If PartID changes, we might need to re-validate or update description?
                // For now, let's assume we update all loads to match if it's the same "group".
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PartID = changedLoad.PartID;
                    }
                }
            }
            // Add other cascading logic if needed (e.g. Heat Number cascading downwards)
        }

        /// <summary>
        /// Shows contextual help for review grid
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.Review");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}
