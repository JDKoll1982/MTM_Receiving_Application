using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;
using Windows.Foundation;
using Windows.System;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Main workflow view for Dunnage module.
/// Coordinates wizard steps and manages workflow navigation.
/// </summary>
public sealed partial class View_Dunnage_WorkflowView : Page
{
    public ViewModel_Dunnage_WorkFlowViewModel ViewModel { get; }

    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Help _helpService;
    private readonly IService_Focus _focusService;
    private readonly IService_DunnageShortcuts _dunnageShortcuts;
    private readonly List<KeyboardAccelerator> _registeredAccelerators = new();
    private Model_Settings_DunnageShortcuts _shortcutSettings =
        Model_Settings_DunnageShortcuts.CreateDefault();
    private bool _isSimpleNavigationToggleActive;
    private bool _isNextNavigationInProgress;

    /// <summary>
    /// Initializes a new instance of the View_Dunnage_WorkflowView class.
    /// </summary>
    /// <param name="viewModel">The workflow view model.</param>
    /// <param name="workflowService">The dunnage workflow service.</param>
    /// <param name="helpService">The help service.</param>
    /// <param name="focusService">The focus service.</param>
    /// <param name="dunnageShortcuts"></param>
    public View_Dunnage_WorkflowView(
        ViewModel_Dunnage_WorkFlowViewModel viewModel,
        IService_DunnageWorkflow workflowService,
        IService_Help helpService,
        IService_Focus focusService,
        IService_DunnageShortcuts dunnageShortcuts
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(workflowService);
        ArgumentNullException.ThrowIfNull(helpService);
        ArgumentNullException.ThrowIfNull(focusService);
        ArgumentNullException.ThrowIfNull(dunnageShortcuts);

        ViewModel = viewModel;
        _workflowService = workflowService;
        _helpService = helpService;
        _focusService = focusService;
        _dunnageShortcuts = dunnageShortcuts;

        InitializeComponent();
        KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
        DataContext = ViewModel;
        AddHandler(KeyDownEvent, new KeyEventHandler(WorkflowPage_KeyDown), true);

        // Subscribe to workflow step changes
        _workflowService.StepChanged += OnWorkflowStepChanged;
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
            App.GetService<IService_Help>(),
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

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        // No need to update flyout content anymore - help dialog shows current step
    }

    private async void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        // Show help dialog for current step
        await ShowHelpForCurrentStepAsync();
    }

    private void ModeSelectionAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        InvokeModeSelectionAction();
        args.Handled = true;
    }

    private void ClearLabelDataAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;

        if (ViewModel.ClearLabelDataCommand.CanExecute(null))
        {
            ViewModel.ClearLabelDataCommand.Execute(null);
            args.Handled = true;
        }
    }

    private async void NextStepAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        await TryInvokeNextActionAsync();
        args.Handled = true;
    }

    private void BackStepAccelerator_Invoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        _ = sender;
        InvokeBackAction();
        args.Handled = true;
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

    private async System.Threading.Tasks.Task ShowHelpForCurrentStepAsync()
    {
        if (_helpService == null || _workflowService == null)
        {
            return;
        }

        await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
    }

    private async void OnNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_isNextNavigationInProgress)
        {
            return;
        }

        _isNextNavigationInProgress = true;

        try
        {
            var result = await _workflowService.AdvanceToNextStepAsync();

            if (!result.IsSuccess)
            {
                var dialog = new ContentDialog
                {
                    Title = "Finish This Step First",
                    Content = result.ErrorMessage,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                    dialog,
                    this.XamlRoot
                );
                await dialog.ShowAsync();
            }
        }
        finally
        {
            _isNextNavigationInProgress = false;
        }
    }

    private async void OnSaveAndReviewClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_isNextNavigationInProgress)
        {
            return;
        }

        _isNextNavigationInProgress = true;

        try
        {
            if (!await EnsureDetailsLocationResolvedAsync())
            {
                return;
            }

            // If no PO entered, give the user a chance to supply a non-PO reference
            // before we advance. The dialog also persists commonly-used reasons.
            if (string.IsNullOrWhiteSpace(_workflowService.CurrentSession.PONumber))
            {
                var nonPoDialog = new View_Dunnage_Dialog_NonPOEntry(
                    _workflowService.CurrentSession.SelectedPart?.PartId
                )
                {
                    XamlRoot = this.XamlRoot,
                };
                await nonPoDialog.ShowAsync();

                if (nonPoDialog.Result is null)
                {
                    // User cancelled — remain on DetailsEntry so they can fill PO or reconsider
                    return;
                }

                // Apply the chosen reference so AddCurrentLoadToSession picks it up
                _workflowService.CurrentSession.PONumber = nonPoDialog.Result;

                // Mirror back to the ViewModel so the PO Number TextBox shows the chosen reference.
                // Without this the field stays visually empty even though the session has the value.
                DetailsEntryView.ViewModel.PoNumber = nonPoDialog.Result;
            }

            var result = await DetailsEntryView.ViewModel.SaveAndAdvanceAsync();

            if (!result.IsSuccess)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Finish This Step First",
                    Content = result.ErrorMessage,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                    errorDialog,
                    this.XamlRoot
                );
                await errorDialog.ShowAsync();
            }
        }
        finally
        {
            _isNextNavigationInProgress = false;
        }
    }

    private void OnModeSelectionClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsManualEntryVisible)
        {
            if (ManualEntryView.ViewModel.ReturnToModeSelectionCommand.CanExecute(null))
            {
                ManualEntryView.ViewModel.ReturnToModeSelectionCommand.Execute(null);
            }

            return;
        }

        if (ViewModel.IsEditModeVisible)
        {
            if (EditModeView.ViewModel.ReturnToModeSelectionCommand.CanExecute(null))
            {
                EditModeView.ViewModel.ReturnToModeSelectionCommand.Execute(null);
            }
        }
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
                _ = InvokeNextActionAsync();
                e.Handled = true;
                return true;
            }

            if (
                Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                    e.Key,
                    new Model_KeyboardShortcutBinding { Key = "Left" }
                )
            )
            {
                InvokeBackAction();
                e.Handled = true;
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
            _ = InvokeNextActionAsync();
            e.Handled = true;
            return true;
        }

        if (
            Helper_KeyboardShortcuts.DoesCurrentKeyEventMatch(
                e.Key,
                _shortcutSettings.BackStepShortcut
            )
        )
        {
            InvokeBackAction();
            e.Handled = true;
            return true;
        }

        return false;
    }

    private void InvokeModeSelectionAction()
    {
        if (ViewModel.IsManualEntryVisible || ViewModel.IsEditModeVisible)
        {
            OnModeSelectionClick(this, new RoutedEventArgs());
            return;
        }

        if (ViewModel.ReturnToModeSelectionCommand.CanExecute(null))
        {
            ViewModel.ReturnToModeSelectionCommand.Execute(null);
        }
    }

    private async Task InvokeNextActionAsync()
    {
        if (ViewModel.IsPartSelectionVisible || ViewModel.IsQuantityEntryVisible)
        {
            if (ViewModel.IsPartSelectionVisible && !PartSelectionView.ViewModel.IsPartSelected)
            {
                return;
            }

            if (ViewModel.IsQuantityEntryVisible && !QuantityEntryView.ViewModel.IsValid)
            {
                return;
            }

            OnNextClick(this, new RoutedEventArgs());
            return;
        }

        if (ViewModel.IsDetailsEntryVisible)
        {
            if (!DetailsEntryView.ViewModel.CanProceedToNextStep)
            {
                return;
            }

            OnSaveAndReviewClick(this, new RoutedEventArgs());
            return;
        }

        await Task.CompletedTask;
    }

    private async Task TryInvokeNextActionAsync()
    {
        if (_isNextNavigationInProgress)
        {
            return;
        }

        await InvokeNextActionAsync();
    }

    private void InvokeBackAction()
    {
        if (
            ViewModel.IsPartSelectionVisible
            || ViewModel.IsQuantityEntryVisible
            || ViewModel.IsDetailsEntryVisible
            || ViewModel.IsReviewVisible
        )
        {
            OnBackClick(this, new RoutedEventArgs());
        }
    }

    private async Task<bool> EnsureDetailsLocationResolvedAsync()
    {
        var validation = await DetailsEntryView.ViewModel.ValidateLocationAsync();
        if (validation.IsValid)
        {
            return true;
        }

        var suggestionsResult = await DetailsEntryView.ViewModel.GetLocationSuggestionsAsync();
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            var dialog = new Dialog_FuzzySearchPicker(
                suggestionsResult.Data,
                "Select Location",
                $"No exact match was found for '{DetailsEntryView.ViewModel.Location?.Trim()}'. Select a matching location."
            )
            {
                XamlRoot = this.XamlRoot,
            };

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult == ContentDialogResult.Primary
                && dialog.SelectedResult is not null
                && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
            )
            {
                DetailsEntryView.ViewModel.Location = dialog.SelectedResult.Label.Trim();
                return true;
            }
        }

        var statusMessage = validation.Message;
        if (
            !suggestionsResult.IsSuccess
            && string.IsNullOrWhiteSpace(suggestionsResult.ErrorMessage) is false
        )
        {
            statusMessage = $"{validation.Message} {suggestionsResult.ErrorMessage}";
        }

        var errorDialog = new ContentDialog
        {
            Title = "Finish This Step First",
            Content = statusMessage,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot,
        };
        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            errorDialog,
            this.XamlRoot
        );
        await errorDialog.ShowAsync();
        return false;
    }

    public async Task<bool> ConfirmLeaveModuleAsync(string destinationName)
    {
        if (_workflowService.IsNavigationLocked)
        {
            var savingDialog = new ContentDialog
            {
                Title = "Save In Progress",
                Content =
                    "Please wait for the current Dunnage save to finish before leaving this module.",
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
            Title = "Leave Dunnage?",
            Content =
                $"Leaving Dunnage for {destinationName} will clear the current entry and reset the on-screen fields. Continue?",
            PrimaryButtonText = "Leave Dunnage",
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
            _workflowService.CurrentStep == Enum_DunnageWorkflowStep.ModeSelection
            || _workflowService.CurrentStep == Enum_DunnageWorkflowStep.TypeSelection
            || _workflowService.CurrentStep == Enum_DunnageWorkflowStep.ImagePartSearch
        )
        {
            return false;
        }

        if (ViewModel.IsManualEntryVisible)
        {
            return ManualEntryView.ViewModel.HasUnsavedData;
        }

        if (ViewModel.IsEditModeVisible)
        {
            return EditModeView.ViewModel.HasUnsavedChanges;
        }

        return _workflowService.HasUnsavedData();
    }

    private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Navigate back based on current step
        switch (_workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.ImagePartSearch:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
                break;
            case Enum_DunnageWorkflowStep.PartSelection:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
                break;
            case Enum_DunnageWorkflowStep.QuantityEntry:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
                break;
            case Enum_DunnageWorkflowStep.DetailsEntry:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
                break;
            case Enum_DunnageWorkflowStep.Review:
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
                break;
        }
    }
}
