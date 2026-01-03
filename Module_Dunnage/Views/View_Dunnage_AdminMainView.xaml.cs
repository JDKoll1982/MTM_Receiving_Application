using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminMainView : Page
{
    public ViewModel_Dunnage_AdminMain ViewModel { get; }

    public View_Dunnage_AdminMainView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminMain>();
        InitializeComponent();
    }
}
