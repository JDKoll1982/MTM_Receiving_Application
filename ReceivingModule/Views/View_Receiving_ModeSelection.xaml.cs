using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_ModeSelection : UserControl
    {
        public ViewModel_Receiving_ModeSelection ViewModel { get; }

        public View_Receiving_ModeSelection()
        {
            ViewModel = App.GetService<ViewModel_Receiving_ModeSelection>();
            this.InitializeComponent();
        }
    }
}
