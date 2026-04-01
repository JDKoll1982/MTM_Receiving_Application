using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

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

    /// <summary>
    /// Initializes a new instance of the View_Dunnage_WorkflowView class.
    /// </summary>
    /// <param name="viewModel">The workflow view model.</param>
    /// <param name="workflowService">The dunnage workflow service.</param>
    /// <param name="helpService">The help service.</param>
    /// <param name="focusService">The focus service.</param>
    public View_Dunnage_WorkflowView(
        ViewModel_Dunnage_WorkFlowViewModel viewModel,
        IService_DunnageWorkflow workflowService,
        IService_Help helpService,
        IService_Focus focusService
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(workflowService);
        ArgumentNullException.ThrowIfNull(helpService);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _workflowService = workflowService;
        _helpService = helpService;
        _focusService = focusService;

        InitializeComponent();
        DataContext = ViewModel;

        // Subscribe to workflow step changes
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _focusService.AttachFocusOnVisibility(this);
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
            App.GetService<IService_Focus>()
        ) { }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        // No need to update flyout content anymore - help dialog shows current step
    }

    private async void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        // Show help dialog for current step
        await ShowHelpForCurrentStepAsync();
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
        var result = await _workflowService.AdvanceToNextStepAsync();

        if (!result.IsSuccess)
        {
            // Show error message
            var dialog = new ContentDialog
            {
                Title = "Cannot Proceed",
                Content = result.ErrorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            var dialogResult = await dialog.ShowAsync();
        }
    }

    private async void OnSaveAndReviewClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!await EnsureDetailsLocationResolvedAsync())
        {
            return;
        }

        // If no PO entered, give the user a chance to supply a non-PO reference
        // before we advance. The dialog also persists commonly-used reasons.
        if (string.IsNullOrWhiteSpace(_workflowService.CurrentSession.PONumber))
        {
            var nonPoDialog = new View_Dunnage_Dialog_NonPOEntry { XamlRoot = this.XamlRoot };
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

        var result = await _workflowService.AdvanceToNextStepAsync();

        if (!result.IsSuccess)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Cannot Proceed",
                Content = result.ErrorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await errorDialog.ShowAsync();
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
            Title = "Cannot Proceed",
            Content = statusMessage,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot,
        };
        await errorDialog.ShowAsync();
        return false;
    }

    private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Navigate back based on current step
        switch (_workflowService.CurrentStep)
        {
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
