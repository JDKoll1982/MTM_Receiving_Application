using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
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
        IService_Focus focusService)
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
            App.GetService<IService_Focus>())
    {
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
                XamlRoot = this.XamlRoot
            };
            var dialogResult = await dialog.ShowAsync();
        }
    }

    private async void OnSaveAndReviewClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Show confirmation dialog
        var confirmDialog = new ContentDialog
        {
            Title = "Save and Review",
            Content = "Are you ready to save this load and proceed to review?\n\nYou will be able to add more loads from the review screen.",
            PrimaryButtonText = "Save & Review",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var confirmResult = await confirmDialog.ShowAsync();

        if (confirmResult == ContentDialogResult.Primary)
        {
            // User confirmed, proceed to next step
            var result = await _workflowService.AdvanceToNextStepAsync();

            if (!result.IsSuccess)
            {
                // Show error message
                var errorDialog = new ContentDialog
                {
                    Title = "Cannot Proceed",
                    Content = result.ErrorMessage,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
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

