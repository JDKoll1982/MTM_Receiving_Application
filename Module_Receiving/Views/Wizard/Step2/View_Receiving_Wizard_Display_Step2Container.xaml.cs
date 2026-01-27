using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
