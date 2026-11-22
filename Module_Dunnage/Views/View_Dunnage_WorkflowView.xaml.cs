using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.UI;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Main workflow view for Dunnage module.
/// </summary>
/// <remarks>
/// The page hosts the step views and hands their view models to the workflow view model, which
/// supplies every Back, Next, Review, Save, Clear All Labels, Help and mode action to the shared
/// shell action bar. The navigation verbs live there, so this code-behind holds only that wiring,
/// the keyboard accelerators, and the leave confirmation the page already owned.
/// </remarks>
public sealed partial class View_Dunnage_WorkflowView : Page
{
    public ViewModel_Dunnage_WorkFlowViewModel ViewModel { get; }

    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Focus _focusService;
    private readonly IService_DunnageShortcuts _dunnageShortcuts;
    private readonly List<KeyboardAccelerator> _registeredAccelerators = new();
    private Model_Settings_DunnageShortcuts _shortcutSettings =
        Model_Settings_DunnageShortcuts.CreateDefault();
    private bool _isSimpleNavigationToggleActive;

    /// <summary>
    /// Initializes a new instance of the View_Dunnage_WorkflowView class.
    /// </summary>
    /// <param name="viewModel">The workflow view model.</param>
    /// <param name="workflowService">The dunnage workflow service.</param>
    /// <param name="focusService">The focus service.</param>
    /// <param name="dunnageShortcuts">The dunnage shortcut service.</param>
    public View_Dunnage_WorkflowView(
        ViewModel_Dunnage_WorkFlowViewModel viewModel,
        IService_DunnageWorkflow workflowService,
        IService_Focus focusService,
        IService_DunnageShortcuts dunnageShortcuts
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(workflowService);
        ArgumentNullException.ThrowIfNull(focusService);
        ArgumentNullException.ThrowIfNull(dunnageShortcuts);

        ViewModel = viewModel;
        _workflowService = workflowService;
        _focusService = focusService;
        _dunnageShortcuts = dunnageShortcuts;

        InitializeComponent();
        KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
        DataContext = ViewModel;
        AddHandler(KeyDownEvent, new KeyEventHandler(WorkflowPage_KeyDown), true);

        ViewModel.RequestCommitDetailsInputs = DetailsEntryView.CommitPendingInputs;
        ViewModel.RequestConfirmLeaveModule = ConfirmLeaveModuleAsync;
        ViewModel.AttachShellActionHosts(
            PartSelectionView.ViewModel,
            QuantityEntryView.ViewModel,
            DetailsEntryView.ViewModel,
            ReviewView.ViewModel,
            EditModeView.ViewModel
        );

        _focusService.AttachFocusOnVisibility(this);
        _ = LoadShortcutsAsync();
    }

    /// <summary>
    /// Parameterless constructor for XAML navigation.
    /// Uses Service Locator temporarily until navigation supports constructor injection.
    /// </summary>
    public View_Dunnage_WorkflowView()
        : this(
            App.GetService<ViewModel_Dunnage_WorkFlowViewModel>(),
            App.GetService<IService_DunnageWorkflow>(),
            App.GetService<IService_Focus>(),
            App.GetService<IService_DunnageShortcuts>()
        ) { }

    private async Task LoadShortcutsAsync()
    {
        var shortcuts = await _dunnageShortcuts.GetShortcutsAsync();
        _shortcutSettings = (shortcuts ?? Model_Settings_DunnageShortcuts.CreateDefault()).Clone();
        ApplyKeyboardShortcuts(isSimpleNavigationToggleActive: false);
    }

    private void ApplyKeyboardShortcuts(bool isSimpleNavigationToggleActive)
    {
        _isSimpleNavigationToggleActive =
            _shortcutSettings.IsToggleSimpleNavigationEnabled && isSimpleNavigationToggleActive;

        ClearKeyboardAccelerators();

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

    private void ClearLabelDataAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        args.Handled = TryRunShellAction(ViewModel.ShellActions.ClearLabelData);
    }

    private void NextStepAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        args.Handled = TryRunShellAction(ViewModel.ShellActions.Next);
    }

    private void BackStepAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        args.Handled = TryRunShellAction(ViewModel.ShellActions.Back);
    }

    private void HelpAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        args.Handled = TryRunShellAction(ViewModel.ShellActions.Help);
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

    /// <summary>
    /// Double-tapping a part card means the same thing as pressing Next, so it runs the strip's own
    /// Next command and meets the same gate.
    /// </summary>
    private void OnPartCardActivated(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        TryRunShellAction(ViewModel.ShellActions.Next);
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
                )
            )
            {
                e.Handled = TryRunShellAction(ViewModel.ShellActions.Next);
                return true;
            }

            if (
                Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                    e.Key,
                    new Model_KeyboardShortcutBinding { Key = "Left" }
                )
            )
            {
                e.Handled = TryRunShellAction(ViewModel.ShellActions.Back);
                return true;
            }

            return false;
        }

        if (
            Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                e.Key,
                _shortcutSettings.NextStepShortcut
            )
        )
        {
            e.Handled = TryRunShellAction(ViewModel.ShellActions.Next);
            return true;
        }

        if (
            Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                e.Key,
                _shortcutSettings.BackStepShortcut
            )
        )
        {
            e.Handled = TryRunShellAction(ViewModel.ShellActions.Back);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Runs a strip position through the command the strip itself binds, so a keyboard route can
    /// never reach an action the strip has hidden or disabled.
    /// </summary>
    private static bool TryRunShellAction(Model_ShellActionSlot slot)
    {
        if (!slot.IsVisible || !slot.IsEnabled || slot.Command?.CanExecute(null) != true)
        {
            return false;
        }

        slot.Command.Execute(null);
        return true;
    }

    public async Task<bool> ConfirmLeaveModuleAsync(string destinationName)
    {
        if (_workflowService.IsNavigationLocked)
        {
            var savingDialog = new ContentDialog
            {
                Title = "Save In Progress",
                Content =
                    "Please wait for the current Non-Inventoried Labels save to finish before leaving this module.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                savingDialog,
                XamlRoot
            );
            await savingDialog.ShowAsync();
            return false;
        }

        if (!HasUnsavedData())
        {
            _workflowService.ClearSession();
            return true;
        }

        var prompt = new ContentDialog
        {
            Title = "Leave Non-Inventoried Labels?",
            Content =
                $"Leaving Non-Inventoried Labels for {destinationName} will clear the current entry and reset the on-screen fields. Continue?",
            PrimaryButtonText = "Leave Non-Inventoried Labels",
            CloseButtonText = "Stay Here",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            prompt,
            XamlRoot
        );

        var result = await prompt.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return false;
        }

        _workflowService.ClearSession();
        return true;
    }

    private bool HasUnsavedData()
    {
        if (
            _workflowService.CurrentStep == Enum_DunnageWorkflowStep.TypeSelection
            || _workflowService.CurrentStep == Enum_DunnageWorkflowStep.ImagePartSearch
        )
        {
            return false;
        }

        if (ViewModel.IsEditModeVisible)
        {
            return EditModeView.ViewModel.HasUnsavedChanges;
        }

        return _workflowService.HasUnsavedData();
    }
}
