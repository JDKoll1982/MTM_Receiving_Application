using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Receiving;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_WorkflowView : Page
    {
        public Receiving_ReceivingWorkflowViewModel ViewModel { get; }
        private readonly IService_ReceivingWorkflow? _workflowService;
        private readonly IService_Help? _helpService;

        public Receiving_WorkflowView()
        {
            ViewModel = App.GetService<Receiving_ReceivingWorkflowViewModel>();
            this.InitializeComponent();

            _workflowService = App.GetService<IService_ReceivingWorkflow>();
            _helpService = App.GetService<IService_Help>();
        }

        private async void HelpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_helpService == null || _workflowService == null)
            {
                return;
            }

            await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
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
            if (workflowService.CurrentStep == Enum_ReceivingWorkflowStep.ModeSelection &&
                sessionManager.CurrentSession?.User != null)
            {
                var defaultMode = sessionManager.CurrentSession.User.DefaultReceivingMode;

                if (!string.IsNullOrEmpty(defaultMode))
                {
                    // User has a default mode set - go directly to that mode
                    if (defaultMode == "guided")
                    {
                        workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
                    }
                    else if (defaultMode == "manual")
                    {
                        workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
                    }
                }
            }
            // If defaultMode is null or conditions not met, stay on current step
        }
    }
}
