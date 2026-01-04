using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class View_Routing_ModeSelection : UserControl
{
    public ViewModel_Routing_ModeSelection ViewModel { get; }

    public View_Routing_ModeSelection()
    {
        ViewModel = App.GetService<ViewModel_Routing_ModeSelection>();
        InitializeComponent();
    }
}
