using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class POEntryView : UserControl
    {
        public POEntryViewModel ViewModel { get; }

        public POEntryView()
        {
            ViewModel = App.GetService<POEntryViewModel>();
            this.InitializeComponent();
        }
    }
}
