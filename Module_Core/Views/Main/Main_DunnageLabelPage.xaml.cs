using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.ViewModels.Main;

namespace MTM_Receiving_Application.Module_Core.Views.Main;

/// <summary>
/// Page for dunnage label entry
/// </summary>
public sealed partial class Main_DunnageLabelPage : Page
{
    public Main_DunnageLabelViewModel ViewModel { get; }

    public Main_DunnageLabelPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<Main_DunnageLabelViewModel>();
        DataContext = ViewModel;
    }
}

