using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class PackageTypeView : UserControl
    {
        public PackageTypeViewModel ViewModel { get; }

        public PackageTypeView()
        {
            ViewModel = App.GetService<PackageTypeViewModel>();
            this.InitializeComponent();
        }
    }
}
