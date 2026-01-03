using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

public sealed partial class View_Dunnage_QuantityEntryView : UserControl
{
    public ViewModel_Dunnage_QuantityEntry ViewModel { get; }

    public View_Dunnage_QuantityEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_QuantityEntry>();
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadContextData();
    }
}
