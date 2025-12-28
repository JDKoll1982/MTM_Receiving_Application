using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_ModeSelectionView : UserControl
    {
        public Receiving_ReceivingModeSelectionViewModel ViewModel { get; }

        public Receiving_ModeSelectionView()
        {
            ViewModel = App.GetService<Receiving_ReceivingModeSelectionViewModel>();
            this.InitializeComponent();
        }
    }
}
