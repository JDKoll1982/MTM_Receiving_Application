using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Main;
using MTM_Receiving_Application.Models.Enums;
using System;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_WorkflowView : Page
{
    public Main_DunnageLabelViewModel ViewModel { get; }
    private Contracts.Services.IService_DunnageWorkflow? _workflowService;

    public Dunnage_WorkflowView()
    {
        ViewModel = App.GetService<Main_DunnageLabelViewModel>();
        InitializeComponent();

        // Subscribe to workflow step changes
        _workflowService = App.GetService<Contracts.Services.IService_DunnageWorkflow>();
        _workflowService.StepChanged += OnWorkflowStepChanged;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        // Update help content when step changes
        UpdateHelpContent();
    }

    private void HelpButton_Loaded(object sender, RoutedEventArgs e)
    {
        // Update help content when button loads
        UpdateHelpContent();
    }

    private void UpdateHelpContent()
    {
        if (HelpContentControl != null)
        {
            HelpContentControl.Content = GetHelpContent();
        }
    }

    private UIElement GetHelpContent()
    {
        var workflowService = _workflowService ?? App.GetService<Contracts.Services.IService_DunnageWorkflow>();

        string title;
        string content;

        switch (workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.ModeSelection:
                title = "Mode Selection";
                content = "Choose your workflow mode:\n\n" +
                         "• Guided Wizard - Step-by-step process for single items\n" +
                         "• Manual Entry - Bulk data entry for multiple loads\n" +
                         "• Edit Mode - Review and edit historical data";
                break;

            case Enum_DunnageWorkflowStep.TypeSelection:
                title = "Type Selection";
                content = "Select the type of dunnage:\n\n" +
                         "• Box - Cardboard or wooden boxes\n" +
                         "• Pallet - Wooden or plastic pallets\n" +
                         "• Crate - Heavy-duty wooden crates\n" +
                         "• Other - Custom dunnage types";
                break;

            case Enum_DunnageWorkflowStep.PartSelection:
                title = "Part Selection";
                content = "Choose a specific part or create a new one:\n\n" +
                         "• Select from existing parts in the dropdown\n" +
                         "• Click 'Add New Part' to create a custom part ID\n" +
                         "• Part IDs are typically formatted as TYPE-DIMENSIONS (e.g., BOX-24X24X18)";
                break;

            case Enum_DunnageWorkflowStep.QuantityEntry:
                title = "Quantity Entry";
                content = "Specify how many labels to generate:\n\n" +
                         "• Enter the number of items (1-9999)\n" +
                         "• Each item will get a unique label\n" +
                         "• You can adjust quantities later if needed";
                break;

            case Enum_DunnageWorkflowStep.DetailsEntry:
                title = "Details Entry";
                content = "Provide additional information:\n\n" +
                         "• PO Number - Optional, for purchase order tracking\n" +
                         "• Location - Physical storage location\n" +
                         "• Specifications - Type-specific measurements or details";
                break;

            case Enum_DunnageWorkflowStep.Review:
                title = "Review & Generate";
                content = "Review your entries before generating labels:\n\n" +
                         "• Verify all information is correct\n" +
                         "• Click 'Generate Labels' to create labels\n" +
                         "• Labels will be saved to history automatically";
                break;

            case Enum_DunnageWorkflowStep.ManualEntry:
                title = "Manual Entry Mode";
                content = "Bulk data entry for multiple loads:\n\n" +
                         "• Add rows for each load\n" +
                         "• Use Auto-Fill to copy from previous entries\n" +
                         "• Fill Blank Spaces to populate empty fields\n" +
                         "• Sort before printing for organization";
                break;

            case Enum_DunnageWorkflowStep.EditMode:
                title = "Edit Mode";
                content = "Review and modify historical data:\n\n" +
                         "• Load previous entries\n" +
                         "• Make corrections or updates\n" +
                         "• Re-generate labels if needed";
                break;

            default:
                title = "Dunnage Labels";
                content = "Create labels for dunnage material:\n\n" +
                         "Dunnage refers to packaging materials used to protect cargo during shipping, " +
                         "including wooden pallets, crates, blocking materials, and protective wrapping.";
                break;
        }

        var stackPanel = new StackPanel
        {
            Spacing = 20,
            Padding = new Microsoft.UI.Xaml.Thickness(4),
            MaxWidth = 450
        };

        // Title section
        var titleSection = new StackPanel { Spacing = 8 };
        titleSection.Children.Add(new TextBlock
        {
            Text = title,
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"]
        });
        titleSection.Children.Add(new Border
        {
            Height = 2,
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"],
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(1),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 60
        });
        stackPanel.Children.Add(titleSection);

        // Content section
        var contentBorder = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"],
            Padding = new Microsoft.UI.Xaml.Thickness(16),
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
            BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"]
        };
        contentBorder.Child = new TextBlock
        {
            Text = content,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        };
        stackPanel.Children.Add(contentBorder);

        return stackPanel;
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
