using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

/// <summary>
/// Page for receiving label entry
/// </summary>
public sealed partial class ReceivingLabelPage : Page
{
    public ReceivingLabelViewModel ViewModel { get; }

    public ReceivingLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ReceivingLabelViewModel>();
        DataContext = ViewModel;
    }
}
