using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Receiving.Models; // For Model_WorkflowStepResult if needed
using MTM_Receiving_Application.Module_Core.Models.Enums; // For other enums if needed
namespace MTM_Receiving_Application.Module_Core.Contracts.Services
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


