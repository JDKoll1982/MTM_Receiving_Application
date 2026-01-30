using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Orchestration;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step2;

/// <summary>
/// Container page for Step 2: Load Details Entry.
/// Hosts the load details DataGrid and bulk operation toolbar.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_Step2Container : Page
{
    public View_Receiving_Wizard_Display_Step2Container()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Get the orchestration ViewModel from the parent workflow page
        DependencyObject? parent = this;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is Orchestration.View_Receiving_Wizard_Orchestration_MainWorkflow workflowView &&
                workflowView.ViewModel is ViewModel_Receiving_Wizard_Orchestration_MainWorkflow orchestrationViewModel)
            {
                // Share the orchestration ViewModel's Loads collection with the grid ViewModel
                if (LoadDetailsGrid.ViewModel != null)
                {
                    LoadDetailsGrid.ViewModel.Loads = orchestrationViewModel.Loads;
                }
                break;
            }
        }
    }

    private void OnShowBulkCopyDialog(object sender, RoutedEventArgs e)
    {
        // TODO: Show bulk copy dialog when implemented
    }

    private void OnClearAutoFilled(object sender, RoutedEventArgs e)
    {
        // TODO: Implement clear auto-filled fields logic
    }

    private void OnShowHelp(object sender, RoutedEventArgs e)
    {
        // TODO: Show Step 2 help dialog when implemented
    }
}
