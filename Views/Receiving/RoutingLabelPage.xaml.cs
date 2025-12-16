using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

/// <summary>
/// Page for routing label entry
/// </summary>
public sealed partial class RoutingLabelPage : Page
{
    public RoutingLabelViewModel ViewModel { get; }

    public RoutingLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<RoutingLabelViewModel>();
        DataContext = ViewModel;
    }
}
