using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_LoadEntryView : UserControl
    {
        public Receiving_LoadEntryViewModel ViewModel { get; }

        public Receiving_LoadEntryView()
        {
            ViewModel = App.GetService<Receiving_LoadEntryViewModel>();
            this.InitializeComponent();
        }
    }
}
