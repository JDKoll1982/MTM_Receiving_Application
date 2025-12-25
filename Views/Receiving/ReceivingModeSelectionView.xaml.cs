using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ReceivingModeSelectionView : UserControl
    {
        public ReceivingModeSelectionViewModel ViewModel { get; }

        public ReceivingModeSelectionView()
        {
            ViewModel = App.GetService<ReceivingModeSelectionViewModel>();
            this.InitializeComponent();
        }
    }
}
