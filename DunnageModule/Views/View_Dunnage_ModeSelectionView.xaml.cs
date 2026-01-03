using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

/// <summary>
/// UserControl for Dunnage Mode Selection view
/// </summary>
public sealed partial class View_Dunnage_ModeSelectionView : UserControl
{
    public ViewModel_Dunnage_ModeSelection ViewModel { get; }

    public View_Dunnage_ModeSelectionView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_ModeSelection>();
        InitializeComponent();
    }
}
