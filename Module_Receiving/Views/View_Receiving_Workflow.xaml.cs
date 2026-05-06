using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;
using Windows.Foundation;
using Windows.System;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Workflow : Page
    {
        public ViewModel_Receiving_Workflow ViewModel { get; }
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingShortcuts _receivingShortcuts;
        private readonly List<KeyboardAccelerator> _registeredAccelerators = new();
        private Model_Settings_ReceivingShortcuts _shortcutSettings =
            Model_Settings_ReceivingShortcuts.CreateDefault();
        private bool _isSimpleNavigationToggleActive;

        public View_Receiving_Workflow(
            ViewModel_Receiving_Workflow viewModel,
            IService_ReceivingWorkflow workflowService,
            IService_Help helpService,
            IService_ReceivingShortcuts receivingShortcuts,
            View_Receiving_ModeSelection modeSelectionView,
            View_Receiving_ManualEntry manualEntryView,
            View_Receiving_EditMode editModeView,
            View_Receiving_POEntry poEntryView,
            View_Receiving_LoadEntry loadEntryView,
            View_Receiving_WeightQuantity weightQuantityView,
            View_Receiving_HeatLot heatLotView,
            View_Receiving_PackageType packageTypeView,
            View_Receiving_Review reviewView,
            View_Receiving_Dialog_LocationReconciliationReview reconciliationReviewView
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(workflowService);
            ArgumentNullException.ThrowIfNull(helpService);
            ArgumentNullException.ThrowIfNull(receivingShortcuts);
            ArgumentNullException.ThrowIfNull(modeSelectionView);
            ArgumentNullException.ThrowIfNull(manualEntryView);
            ArgumentNullException.ThrowIfNull(editModeView);
            ArgumentNullException.ThrowIfNull(poEntryView);
            ArgumentNullException.ThrowIfNull(loadEntryView);
            ArgumentNullException.ThrowIfNull(weightQuantityView);
            ArgumentNullException.ThrowIfNull(heatLotView);
            ArgumentNullException.ThrowIfNull(packageTypeView);
            ArgumentNullException.ThrowIfNull(reviewView);
            ArgumentNullException.ThrowIfNull(reconciliationReviewView);

            ViewModel = viewModel;
            _workflowService = workflowService;
            _helpService = helpService;
            _receivingShortcuts = receivingShortcuts;
            this.InitializeComponent();
            KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
            AddHandler(KeyDownEvent, new KeyEventHandler(WorkflowPage_KeyDown), true);

            ModeSelectionHost.Content = modeSelectionView;
            ManualEntryHost.Content = manualEntryView;
            EditModeHost.Content = editModeView;
            POEntryHost.Content = poEntryView;
            LoadEntryHost.Content = loadEntryView;
            WeightQuantityHost.Content = weightQuantityView;
            HeatLotHost.Content = heatLotView;
            PackageTypeHost.Content = packageTypeView;
            ReviewHost.Content = reviewView;
            ReconciliationReviewHost.Content = reconciliationReviewView;

            Loaded += View_Receiving_Workflow_Loaded;
            Unloaded += View_Receiving_Workflow_Unloaded;

            _ = LoadShortcutsAsync();
        }

        private void View_Receiving_Workflow_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;

            _workflowService.StepChanged -= WorkflowService_StepChanged;
            _workflowService.StepChanged += WorkflowService_StepChanged;
            RequestFocusForCurrentStep();
        }

        private void View_Receiving_Workflow_Unloaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;

            _workflowService.StepChanged -= WorkflowService_StepChanged;
        }

        private void WorkflowService_StepChanged(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;
            RequestFocusForCurrentStep();
        }

        private void RequestFocusForCurrentStep()
        {
            if (DispatcherQueue == null)
            {
                return;
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ResolveFocusableStepView()?.FocusForAccess();
                });
            });
        }

        private IReceivingWorkflowFocusable? ResolveFocusableStepView()
        {
            return _workflowService.CurrentStep switch
            {
                Enum_ReceivingWorkflowStep.POEntry => POEntryHost.Content as IReceivingWorkflowFocusable,
                Enum_ReceivingWorkflowStep.LoadEntry =>
                    LoadEntryHost.Content as IReceivingWorkflowFocusable,
                Enum_ReceivingWorkflowStep.WeightQuantityEntry =>
                    WeightQuantityHost.Content as IReceivingWorkflowFocusable,
                Enum_ReceivingWorkflowStep.HeatLotEntry =>
                    HeatLotHost.Content as IReceivingWorkflowFocusable,
                Enum_ReceivingWorkflowStep.PackageTypeEntry =>
                    PackageTypeHost.Content as IReceivingWorkflowFocusable,
                _ => null,
            };
        }

        private async void HelpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_helpService == null || _workflowService == null)
            {
                return;
            }

            await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
        }

        public async Task RefreshSettingsDependentStateAsync()
        {
            if (ModeSelectionHost.Content is View_Receiving_ModeSelection modeSelectionView)
            {
                await modeSelectionView.ViewModel.RefreshDefaultModeIndicatorsAsync();
            }

            await LoadShortcutsAsync();
        }

        public async Task<bool> ConfirmLeaveModuleAsync(string destinationName)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Saving)
            {
                var savingDialog = new ContentDialog
                {
                    Title = "Save In Progress",
                    Content =
                        "Please wait for the current Receiving save to finish before leaving this module.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot,
                };

                Helper_UI_ContentDialogTheme.ApplyTheme(savingDialog, XamlRoot);
                await savingDialog.ShowAsync();
                return false;
            }

            if (!HasUnsavedData())
            {
                await _workflowService.ResetWorkflowAsync();
                return true;
            }

            var dialog = new ContentDialog
            {
                Title = "Leave Receiving?",
                Content =
                    $"Leaving Receiving for {destinationName} will clear any unsaved workflow data. Continue?",
                PrimaryButtonText = "Leave Receiving",
                CloseButtonText = "Stay Here",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };

            Helper_UI_ContentDialogTheme.ApplyTheme(dialog, XamlRoot);

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return false;
            }

            await _workflowService.ResetWorkflowAsync();
            return true;
        }

        private bool HasUnsavedData()
        {
            if (
                _workflowService.CurrentStep == Enum_ReceivingWorkflowStep.ModeSelection
                || _workflowService.CurrentStep == Enum_ReceivingWorkflowStep.POEntry
            )
            {
                return false;
            }

            if (_workflowService.CurrentSession == null)
            {
                return false;
            }

            if (_workflowService.CurrentSession.Loads?.Count > 0)
            {
                return true;
            }

            return !string.IsNullOrEmpty(_workflowService.CurrentSession.PoNumber)
                || _workflowService.CurrentPart != null;
        }

        private async Task LoadShortcutsAsync()
        {
            var shortcuts = await _receivingShortcuts.GetShortcutsAsync();
            _shortcutSettings = (
                shortcuts ?? Model_Settings_ReceivingShortcuts.CreateDefault()
            ).Clone();
            ApplyKeyboardShortcuts(isSimpleNavigationToggleActive: false);
        }

        private void ApplyKeyboardShortcuts(bool isSimpleNavigationToggleActive)
        {
            _isSimpleNavigationToggleActive =
                _shortcutSettings.IsToggleSimpleNavigationEnabled && isSimpleNavigationToggleActive;

            ClearKeyboardAccelerators();

            RegisterAccelerator(
                _shortcutSettings.ModeSelectionShortcut,
                ModeSelectionAccelerator_Invoked
            );
            RegisterAccelerator(
                _shortcutSettings.ClearLabelDataShortcut,
                ClearLabelDataAccelerator_Invoked
            );
            RegisterNavigationAccelerators();
            RegisterAccelerator(_shortcutSettings.HelpShortcut, HelpAccelerator_Invoked);

            if (_shortcutSettings.IsToggleSimpleNavigationEnabled)
            {
                RegisterAccelerator(
                    new Model_KeyboardShortcutBinding { Key = "T", IsCtrlEnabled = true },
                    ToggleSimpleNavigationAccelerator_Invoked
                );
            }
        }

        private void ClearKeyboardAccelerators()
        {
            foreach (var accelerator in _registeredAccelerators)
            {
                KeyboardAccelerators.Remove(accelerator);
            }

            _registeredAccelerators.Clear();
        }

        private void RegisterNavigationAccelerators()
        {
            var nextShortcut = _isSimpleNavigationToggleActive
                ? new Model_KeyboardShortcutBinding { Key = "Right" }
                : _shortcutSettings.NextStepShortcut;
            var backShortcut = _isSimpleNavigationToggleActive
                ? new Model_KeyboardShortcutBinding { Key = "Left" }
                : _shortcutSettings.BackStepShortcut;

            RegisterAccelerator(nextShortcut, NextStepAccelerator_Invoked);
            RegisterAccelerator(backShortcut, BackStepAccelerator_Invoked);
        }

        private void RegisterAccelerator(
            Model_KeyboardShortcutBinding binding,
            TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> handler
        )
        {
            var accelerator = Helper_KeyboardShortcuts.CreateAccelerator(binding);
            if (accelerator == null)
            {
                return;
            }

            accelerator.Invoked += handler;
            KeyboardAccelerators.Add(accelerator);
            _registeredAccelerators.Add(accelerator);
        }

        private void ModeSelectionAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;

            if (ViewModel.ReturnToModeSelectionCommand.CanExecute(null))
            {
                ViewModel.ReturnToModeSelectionCommand.Execute(null);
                args.Handled = true;
            }
        }

        private void ClearLabelDataAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;

            if (ViewModel.ResetLabelDataCommand.CanExecute(false))
            {
                ViewModel.ResetLabelDataCommand.Execute(false);
                args.Handled = true;
            }
        }

        private void OnResetLabelDataClick(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool clearAllRows =
                (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down)
                == Windows.UI.Core.CoreVirtualKeyStates.Down;

            if (ViewModel.ResetLabelDataCommand.CanExecute(clearAllRows))
            {
                ViewModel.ResetLabelDataCommand.Execute(clearAllRows);
            }
        }

        private void NextStepAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;

            if (ViewModel.NextStepCommand.CanExecute(null))
            {
                ViewModel.NextStepCommand.Execute(null);
                args.Handled = true;
            }
        }

        private void BackStepAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;

            if (ViewModel.PreviousStepCommand.CanExecute(null))
            {
                ViewModel.PreviousStepCommand.Execute(null);
                args.Handled = true;
            }
        }

        private void HelpAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;
            HelpButton_Click(this, new RoutedEventArgs());
            args.Handled = true;
        }

        private void ToggleSimpleNavigationAccelerator_Invoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args
        )
        {
            _ = sender;

            if (!_shortcutSettings.IsToggleSimpleNavigationEnabled)
            {
                return;
            }

            ApplyKeyboardShortcuts(!_isSimpleNavigationToggleActive);
            args.Handled = true;
        }

        private void WorkflowPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            _ = sender;

            if (TryHandleNavigationShortcut(e))
            {
                return;
            }
        }

        private bool TryHandleNavigationShortcut(KeyRoutedEventArgs e)
        {
            if (_isSimpleNavigationToggleActive)
            {
                if (
                    Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                        e.Key,
                        new Model_KeyboardShortcutBinding { Key = "Right" }
                    ) && ViewModel.NextStepCommand.CanExecute(null)
                )
                {
                    ViewModel.NextStepCommand.Execute(null);
                    e.Handled = true;
                    return true;
                }

                if (
                    Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                        e.Key,
                        new Model_KeyboardShortcutBinding { Key = "Left" }
                    ) && ViewModel.PreviousStepCommand.CanExecute(null)
                )
                {
                    ViewModel.PreviousStepCommand.Execute(null);
                    e.Handled = true;
                    return true;
                }

                return false;
            }

            if (
                Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                    e.Key,
                    _shortcutSettings.NextStepShortcut
                ) && ViewModel.NextStepCommand.CanExecute(null)
            )
            {
                ViewModel.NextStepCommand.Execute(null);
                e.Handled = true;
                return true;
            }

            if (
                Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                    e.Key,
                    _shortcutSettings.BackStepShortcut
                ) && ViewModel.PreviousStepCommand.CanExecute(null)
            )
            {
                ViewModel.PreviousStepCommand.Execute(null);
                e.Handled = true;
                return true;
            }

            return false;
        }
    }
}
