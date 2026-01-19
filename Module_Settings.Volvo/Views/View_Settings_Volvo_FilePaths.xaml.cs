using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_FilePaths : Page
{
    public ViewModel_Settings_Volvo_FilePaths ViewModel { get; }

    public View_Settings_Volvo_FilePaths(ViewModel_Settings_Volvo_FilePaths viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
