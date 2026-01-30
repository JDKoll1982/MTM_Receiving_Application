using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Receiving.Views.Hub;

public sealed partial class View_Receiving_Hub_Display_ModeSelection : Page
{
    public View_Receiving_Hub_Display_ModeSelection()
    {
        InitializeComponent();
    }

    private Frame? GetHostFrame()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }
            parent = (parent as FrameworkElement)?.Parent;
        }
        return null;
    }

    private void OnSelectWizardMode(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(Wizard.Orchestration.View_Receiving_Wizard_Orchestration_MainWorkflow));
    }

    private void OnSelectManualMode(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Manual mode when implemented
    }

    private void OnSelectEditMode(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Edit mode when implemented
    }
}
