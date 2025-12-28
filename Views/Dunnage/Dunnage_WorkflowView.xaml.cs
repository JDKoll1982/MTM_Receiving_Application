using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Main;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_WorkflowView : Page
{
    public Main_DunnageLabelViewModel ViewModel { get; }

    public Dunnage_WorkflowView()
    {
        ViewModel = App.GetService<Main_DunnageLabelViewModel>();
        InitializeComponent();
    }
}
