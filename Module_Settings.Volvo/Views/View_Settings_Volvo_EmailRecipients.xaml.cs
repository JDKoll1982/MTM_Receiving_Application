using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_EmailRecipients : Page
{
    public ViewModel_Settings_Volvo_EmailRecipients ViewModel { get; }

    public View_Settings_Volvo_EmailRecipients(ViewModel_Settings_Volvo_EmailRecipients viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
