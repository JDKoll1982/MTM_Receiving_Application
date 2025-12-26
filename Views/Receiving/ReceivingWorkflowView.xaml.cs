using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Receiving;

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
            // Only do this if we're starting fresh (on ModeSelection step)
            var sessionManager = App.GetService<IService_UserSessionManager>();
            var workflowService = App.GetService<IService_ReceivingWorkflow>();
            
            // Only apply default mode if we're on the mode selection screen
            // and there's a valid user session
            if (workflowService.CurrentStep == WorkflowStep.ModeSelection && 
                sessionManager.CurrentSession?.User != null)
            {
                var defaultMode = sessionManager.CurrentSession.User.DefaultReceivingMode;
                
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
            }
            // If defaultMode is null or conditions not met, stay on current step
        }
    }
}
