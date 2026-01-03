using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Main;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Contracts.Services;
using System;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_WorkflowView : Page
{
    public Main_DunnageLabelViewModel ViewModel { get; }
    private IService_DunnageWorkflow? _workflowService;
    private IService_Help? _helpService;

    public View_Dunnage_WorkflowView()
    {
        ViewModel = App.GetService<Main_DunnageLabelViewModel>();
        InitializeComponent();

        // Subscribe to workflow step changes
        _workflowService = App.GetService<IService_DunnageWorkflow>();
        _helpService = App.GetService<IService_Help>();
        _workflowService.StepChanged += OnWorkflowStepChanged;
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
            return;

        await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
    }

    private async void OnNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var workflowService = App.GetService<Contracts.Services.IService_DunnageWorkflow>();
        var result = await workflowService.AdvanceToNextStepAsync();

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
            var workflowService = App.GetService<Contracts.Services.IService_DunnageWorkflow>();
            var result = await workflowService.AdvanceToNextStepAsync();

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
        var workflowService = App.GetService<Contracts.Services.IService_DunnageWorkflow>();

        // Navigate back based on current step
        switch (workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.PartSelection:
                workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
                break;
            case Enum_DunnageWorkflowStep.QuantityEntry:
                workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
                break;
            case Enum_DunnageWorkflowStep.DetailsEntry:
                workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
                break;
            case Enum_DunnageWorkflowStep.Review:
                workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
                break;
        }
    }
}
