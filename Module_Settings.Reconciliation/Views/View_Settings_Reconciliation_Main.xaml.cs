using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reconciliation.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reconciliation.Views;

/// <summary>
/// The Receiving Validator's settings page. The code-behind carries no rule: it
/// only binds the view model (R-MVVM-02).
/// </summary>
public sealed partial class View_Settings_Reconciliation_Main : Page
{
    /// <summary>The page's view model.</summary>
    public ViewModel_Settings_Reconciliation_Main ViewModel { get; }

    /// <summary>Creates the page over its resolved view model.</summary>
    public View_Settings_Reconciliation_Main()
    {
        ViewModel = App.GetService<ViewModel_Settings_Reconciliation_Main>();

        InitializeComponent();

        DataContext = ViewModel;
    }
}
