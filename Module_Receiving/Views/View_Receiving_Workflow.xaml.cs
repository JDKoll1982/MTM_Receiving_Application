using System;
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
using Windows.System;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Workflow : Page
    {
        private const int MaxFocusAttempts = 6;

        // Simple-navigation mode swaps the configured step shortcuts for bare arrow keys.
        private static readonly Model_KeyboardShortcutBinding SimpleNavigationNextShortcut = new()
        {
            Key = "Right",
        };

        private static readonly Model_KeyboardShortcutBinding SimpleNavigationBackShortcut = new()
        {
            Key = "Left",
        };

        private static readonly Model_KeyboardShortcutBinding SimpleNavigationToggleShortcut = new()
        {
            Key = "T",
            IsCtrlEnabled = true,
        };

        public ViewModel_Receiving_Workflow ViewModel { get; }
        private readonly IService_AdaptiveLayout _adaptiveLayout;
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingShortcuts _receivingShortcuts;
        private readonly IService_Focus _focusService;
        private Model_Settings_ReceivingShortcuts _shortcutSettings =
            Model_Settings_ReceivingShortcuts.CreateDefault();
        private bool _isSimpleNavigationToggleActive;
        private int _pendingFocusAttempts;
        private bool _isFocusAttemptScheduled;

        public View_Receiving_Workflow(
            ViewModel_Receiving_Workflow viewModel,
            IService_AdaptiveLayout adaptiveLayout,
            IService_ReceivingWorkflow workflowService,
            IService_Help helpService,
            IService_ReceivingShortcuts receivingShortcuts,
            IService_Focus focusService,
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
            ArgumentNullException.ThrowIfNull(adaptiveLayout);
            ArgumentNullException.ThrowIfNull(workflowService);
            ArgumentNullException.ThrowIfNull(helpService);
            ArgumentNullException.ThrowIfNull(receivingShortcuts);
            ArgumentNullException.ThrowIfNull(focusService);
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
            _adaptiveLayout = adaptiveLayout;
            _workflowService = workflowService;
            _helpService = helpService;
            _receivingShortcuts = receivingShortcuts;
            _focusService = focusService;
            this.InitializeComponent();
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
            RegisterGuidedStepFocusHooks();

            Loaded += View_Receiving_Workflow_Loaded;
            Unloaded += View_Receiving_Workflow_Unloaded;
            SizeChanged += View_Receiving_Workflow_SizeChanged;
            LayoutUpdated += View_Receiving_Workflow_LayoutUpdated;

            _ = LoadShortcutsAsync();
        }

        private void View_Receiving_Workflow_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;

            _workflowService.StepChanged -= WorkflowService_StepChanged;
            _workflowService.StepChanged += WorkflowService_StepChanged;
            ApplyAdaptiveLayout();
            QueueFocusForCurrentStep();
        }

        private void View_Receiving_Workflow_Unloaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;

            _workflowService.StepChanged -= WorkflowService_StepChanged;
            SizeChanged -= View_Receiving_Workflow_SizeChanged;
            LayoutUpdated -= View_Receiving_Workflow_LayoutUpdated;
        }

        private void View_Receiving_Workflow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void ApplyAdaptiveLayout()
        {
            var state = _adaptiveLayout.ResolveReceivingLayoutState(ActualWidth);
            _ = VisualStateManager.GoToState(this, state, false);
        }

        private void WorkflowService_StepChanged(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;
            QueueFocusForCurrentStep();
        }

        private void QueueFocusForCurrentStep()
        {
            if (DispatcherQueue == null)
            {
                return;
            }

            _pendingFocusAttempts = MaxFocusAttempts;
            ScheduleFocusAttempt();
        }

        private void View_Receiving_Workflow_LayoutUpdated(object? sender, object e)
        {
            _ = sender;
            _ = e;

            if (_pendingFocusAttempts > 0)
            {
                ScheduleFocusAttempt();
            }
        }

        private void ScheduleFocusAttempt()
        {
            if (_isFocusAttemptScheduled || DispatcherQueue == null)
            {
                return;
            }

            _isFocusAttemptScheduled = true;
            DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                () =>
                {
                    _isFocusAttemptScheduled = false;
                    AttemptFocusForCurrentStep();
                }
            );
        }

        private void AttemptFocusForCurrentStep()
        {
            if (_pendingFocusAttempts <= 0)
            {
                return;
            }

            _pendingFocusAttempts--;

            if (TryFocusCurrentStep())
            {
                _pendingFocusAttempts = 0;
                return;
            }

            if (_pendingFocusAttempts > 0)
            {
                ScheduleFocusAttempt();
            }
        }

        private void RegisterGuidedStepFocusHooks()
        {
            RegisterGuidedStepHostFocusHook(POEntryHost, Enum_ReceivingWorkflowStep.POEntry);
            RegisterGuidedStepHostFocusHook(LoadEntryHost, Enum_ReceivingWorkflowStep.LoadEntry);
            RegisterGuidedStepHostFocusHook(
                WeightQuantityHost,
                Enum_ReceivingWorkflowStep.WeightQuantityEntry
            );
            RegisterGuidedStepHostFocusHook(HeatLotHost, Enum_ReceivingWorkflowStep.HeatLotEntry);
            RegisterGuidedStepHostFocusHook(
                PackageTypeHost,
                Enum_ReceivingWorkflowStep.PackageTypeEntry
            );
            RegisterGuidedStepHostFocusHook(ReviewHost, Enum_ReceivingWorkflowStep.Review);
        }

        private void RegisterGuidedStepHostFocusHook(
            ContentControl host,
            Enum_ReceivingWorkflowStep step
        )
        {
            host.RegisterPropertyChangedCallback(
                UIElement.VisibilityProperty,
                (_, _) =>
                {
                    if (
                        host.Visibility == Visibility.Visible
                        && _workflowService.CurrentStep == step
                    )
                    {
                        QueueFocusForCurrentStep();
                    }
                }
            );
        }

        private bool TryFocusCurrentStep()
        {
            return _workflowService.CurrentStep switch
            {
                Enum_ReceivingWorkflowStep.POEntry => TryFocusHostedStep(POEntryHost),
                Enum_ReceivingWorkflowStep.LoadEntry => TryFocusHostedStep(LoadEntryHost),
                Enum_ReceivingWorkflowStep.WeightQuantityEntry => TryFocusHostedStep(
                    WeightQuantityHost
                ),
                Enum_ReceivingWorkflowStep.HeatLotEntry => TryFocusHostedStep(HeatLotHost),
                Enum_ReceivingWorkflowStep.PackageTypeEntry => TryFocusHostedStep(PackageTypeHost),
                Enum_ReceivingWorkflowStep.Review => TryFocusHostedStep(ReviewHost),
                Enum_ReceivingWorkflowStep.Complete => _focusService.TrySetFocus(
                    CompletionStartNewEntryButton
                ),
                _ => true,
            };
        }

        private static bool TryFocusHostedStep(ContentControl host)
        {
            return host.Visibility == Visibility.Visible
                && host.Content is IReceivingWorkflowFocusable view
                && view.FocusForAccess();
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
            try
            {
                var shortcuts = await _receivingShortcuts.GetShortcutsAsync();
                _shortcutSettings = (
                    shortcuts ?? Model_Settings_ReceivingShortcuts.CreateDefault()
                ).Clone();
            }
            catch (Exception ex)
            {
                _ = ex;
                _shortcutSettings = Model_Settings_ReceivingShortcuts.CreateDefault();
            }

            _isSimpleNavigationToggleActive = false;
        }

        /// <summary>
        /// Routes every configured workflow shortcut through the same gated commands the on-screen
        /// buttons use, so a shortcut can never reach a step, mode change, or data reset that the
        /// matching button would block.
        /// </summary>
        private void WorkflowPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            _ = sender;

            // Holding a shortcut key repeats KeyDown; a shortcut must behave like a single click.
            if (e.KeyStatus.WasKeyDown)
            {
                return;
            }

            if (
                TryHandleModeSelectionShortcut(e)
                || TryHandleClearLabelDataShortcut(e)
                || TryHandleStepNavigationShortcut(e)
                || TryHandleHelpShortcut(e)
                || TryHandleSimpleNavigationToggleShortcut(e)
            )
            {
                e.Handled = true;
            }
        }

        private bool TryHandleModeSelectionShortcut(KeyRoutedEventArgs e)
        {
            return IsShortcutMatch(e, _shortcutSettings.ModeSelectionShortcut)
                && TryExecuteCommand(ViewModel.ReturnToModeSelectionCommand);
        }

        private bool TryHandleClearLabelDataShortcut(KeyRoutedEventArgs e)
        {
            if (
                !IsShortcutMatch(e, _shortcutSettings.ClearLabelDataShortcut)
                || !ViewModel.CanClearLabelData
            )
            {
                return false;
            }

            // The Clear Label Data button always clears the active rows, never the full history.
            return TryExecuteCommand(ViewModel.ResetLabelDataCommand, false);
        }

        private bool TryHandleStepNavigationShortcut(KeyRoutedEventArgs e)
        {
            var nextShortcut = _isSimpleNavigationToggleActive
                ? SimpleNavigationNextShortcut
                : _shortcutSettings.NextStepShortcut;

            if (IsShortcutMatch(e, nextShortcut) && TryExecuteCommand(ViewModel.NextStepCommand))
            {
                return true;
            }

            var backShortcut = _isSimpleNavigationToggleActive
                ? SimpleNavigationBackShortcut
                : _shortcutSettings.BackStepShortcut;

            return IsShortcutMatch(e, backShortcut)
                && TryExecuteCommand(ViewModel.PreviousStepCommand);
        }

        private bool TryHandleHelpShortcut(KeyRoutedEventArgs e)
        {
            if (!IsShortcutMatch(e, _shortcutSettings.HelpShortcut))
            {
                return false;
            }

            HelpButton_Click(this, new RoutedEventArgs());
            return true;
        }

        private bool TryHandleSimpleNavigationToggleShortcut(KeyRoutedEventArgs e)
        {
            if (
                !_shortcutSettings.IsToggleSimpleNavigationEnabled
                || !IsShortcutMatch(e, SimpleNavigationToggleShortcut)
            )
            {
                return false;
            }

            _isSimpleNavigationToggleActive = !_isSimpleNavigationToggleActive;
            return true;
        }

        /// <summary>
        /// Determines whether a routed key event matches a binding and may act on the workflow.
        /// </summary>
        private bool IsShortcutMatch(KeyRoutedEventArgs e, Model_KeyboardShortcutBinding binding)
        {
            if (!Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(e.Key, binding))
            {
                return false;
            }

            return !Helper_KeyboardShortcuts.ShouldIgnoreForFocusedInput(
                GetFocusedElement(),
                binding
            );
        }

        private object? GetFocusedElement()
        {
            var xamlRoot = XamlRoot;
            return xamlRoot is null ? null : FocusManager.GetFocusedElement(xamlRoot);
        }

        private static bool TryExecuteCommand(
            System.Windows.Input.ICommand command,
            object? parameter = null
        )
        {
            if (!command.CanExecute(parameter))
            {
                return false;
            }

            command.Execute(parameter);
            return true;
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

    }
}
