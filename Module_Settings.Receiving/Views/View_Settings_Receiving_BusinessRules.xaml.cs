using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_BusinessRules : Page
{
    public ViewModel_Settings_Receiving_BusinessRules ViewModel { get; }

    public View_Settings_Receiving_BusinessRules(ViewModel_Settings_Receiving_BusinessRules viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
