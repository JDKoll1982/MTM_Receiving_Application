using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

/// <summary>
/// UserControl for Dunnage Mode Selection view
/// </summary>
public sealed partial class Dunnage_ModeSelectionView : UserControl
{
    public Dunnage_ModeSelectionViewModel ViewModel { get; }

    public Dunnage_ModeSelectionView()
    {
        ViewModel = App.GetService<Dunnage_ModeSelectionViewModel>();
        InitializeComponent();
    }
}
