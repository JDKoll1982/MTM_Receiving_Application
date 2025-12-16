using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

/// <summary>
/// Page for dunnage label entry
/// </summary>
public sealed partial class DunnageLabelPage : Page
{
    public DunnageLabelViewModel ViewModel { get; }

    public DunnageLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<DunnageLabelViewModel>();
        DataContext = ViewModel;
    }
}
