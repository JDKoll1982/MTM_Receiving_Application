using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step3;

/// <summary>
/// Container page for Step 3: Review &amp; Save.
/// Hosts the review summary grid and save action buttons.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_Step3Container : Page
{
    public View_Receiving_Wizard_Display_Step3Container()
    {
        InitializeComponent();
    }

    private void OnEditDetails(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate back to Step 2 for editing
    }

    private void OnSaveTransaction(object sender, RoutedEventArgs e)
    {
        // TODO: Trigger save operation via ViewModel
    }
}
