using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    public interface IService_DunnageWorkflow
    {
        Enum_DunnageWorkflowStep CurrentStep { get; }
        Model_DunnageSession CurrentSession { get; }

        event EventHandler StepChanged;
        event EventHandler<string> StatusMessageRaised;

        Task<bool> StartWorkflowAsync();
        Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();
        void GoToStep(Enum_DunnageWorkflowStep step);
        Task<Model_SaveResult> SaveSessionAsync();
        void ClearSession();
    }
}
