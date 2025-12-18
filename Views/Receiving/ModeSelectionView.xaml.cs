using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ModeSelectionView : UserControl
    {
        public ModeSelectionViewModel ViewModel { get; }

        public ModeSelectionView()
        {
            ViewModel = App.GetService<ModeSelectionViewModel>();
            this.InitializeComponent();
        }
    }
}
