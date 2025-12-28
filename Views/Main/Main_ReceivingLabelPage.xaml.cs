using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Main;

namespace MTM_Receiving_Application.Views.Main;

/// <summary>
/// Page for receiving label entry
/// </summary>
public sealed partial class Main_ReceivingLabelPage : Page
{
    public Main_ReceivingLabelViewModel ViewModel { get; }

    public Main_ReceivingLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<Main_ReceivingLabelViewModel>();
        DataContext = ViewModel;
    }
}
