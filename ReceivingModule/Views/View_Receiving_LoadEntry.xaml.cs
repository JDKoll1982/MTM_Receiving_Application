using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_LoadEntry : UserControl
    {
        public ViewModel_Receiving_LoadEntry ViewModel { get; }

        public View_Receiving_LoadEntry()
        {
            ViewModel = App.GetService<ViewModel_Receiving_LoadEntry>();
            this.InitializeComponent();
        }
    }
}
