using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Contracts.Services;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check for default mode and skip mode selection if set
            var sessionManager = App.GetService<IService_UserSessionManager>();
            var workflowService = App.GetService<IService_ReceivingWorkflow>();
            
            var defaultMode = sessionManager.CurrentSession?.User?.DefaultReceivingMode;
            
            if (!string.IsNullOrEmpty(defaultMode))
            {
                // User has a default mode set - go directly to that mode
                if (defaultMode == "guided")
                {
                    workflowService.GoToStep(WorkflowStep.POEntry);
                }
                else if (defaultMode == "manual")
                {
                    workflowService.GoToStep(WorkflowStep.ManualEntry);
                }
            }
            // If defaultMode is null, stay on ModeSelection (default behavior)
        }
    }
}
