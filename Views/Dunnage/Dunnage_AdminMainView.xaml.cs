using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_AdminMainView : Page
{
    public Dunnage_AdminMainViewModel ViewModel { get; }

    public Dunnage_AdminMainView()
    {
        ViewModel = App.GetService<Dunnage_AdminMainViewModel>();
        InitializeComponent();
    }
}
