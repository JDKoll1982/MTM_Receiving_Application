using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ReceivingWorkflowView : Page
    {
        public ReceivingWorkflowViewModel ViewModel { get; }

        public ReceivingWorkflowView()
        {
            ViewModel = App.GetService<ReceivingWorkflowViewModel>();
            this.InitializeComponent();
        }
    }
}
