using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

/// <summary>
/// Page for carrier delivery label entry (UPS/FedEx/USPS shipping information)
/// </summary>
public sealed partial class CarrierDeliveryLabelPage : Page
{
    public CarrierDeliveryLabelViewModel ViewModel { get; }

    public CarrierDeliveryLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<CarrierDeliveryLabelViewModel>();
        DataContext = ViewModel;
    }
}
