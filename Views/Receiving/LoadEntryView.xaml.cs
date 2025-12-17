using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class LoadEntryView : UserControl
    {
        public LoadEntryViewModel ViewModel { get; }

        public LoadEntryView()
        {
            ViewModel = App.GetService<LoadEntryViewModel>();
            this.InitializeComponent();
        }
    }
}
