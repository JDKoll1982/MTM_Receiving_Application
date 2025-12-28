using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_QuantityEntryView : UserControl
{
    public Dunnage_QuantityEntryViewModel ViewModel { get; }

    public Dunnage_QuantityEntryView()
    {
        ViewModel = App.GetService<Dunnage_QuantityEntryViewModel>();
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadContextData();
    }
}
