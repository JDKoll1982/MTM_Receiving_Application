using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Main;

namespace MTM_Receiving_Application.Views.Main;

/// <summary>
/// Page for carrier delivery label entry (UPS/FedEx/USPS shipping information)
/// </summary>
public sealed partial class Main_CarrierDeliveryLabelPage : Page
{
    public Main_CarrierDeliveryLabelViewModel ViewModel { get; }

    public Main_CarrierDeliveryLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<Main_CarrierDeliveryLabelViewModel>();
        DataContext = ViewModel;
    }
}
