using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ManualEntryView : UserControl
    {
        public ManualEntryViewModel ViewModel { get; }

        public ManualEntryView()
        {
            ViewModel = App.GetService<ManualEntryViewModel>();
            this.DataContext = ViewModel;
            this.InitializeComponent();
        }
    }
}
