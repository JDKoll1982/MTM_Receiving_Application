using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    public interface IService_DunnageWorkflow
    {
        public Enum_DunnageWorkflowStep CurrentStep { get; }
        public Model_DunnageSession CurrentSession { get; }

        public event EventHandler StepChanged;
        public event EventHandler<string> StatusMessageRaised;

        public Task<bool> StartWorkflowAsync();
        public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();
        public void GoToStep(Enum_DunnageWorkflowStep step);
        public Task<Model_SaveResult> SaveSessionAsync();
        public void ClearSession();
        public void AddCurrentLoadToSession();
    }
}
