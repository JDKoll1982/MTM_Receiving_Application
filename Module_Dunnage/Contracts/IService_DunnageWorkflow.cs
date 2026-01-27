using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts
{
    public interface IService_DunnageWorkflow
    {
        public Enum_DunnageWorkflowStep CurrentStep { get; }
        public Model_DunnageSession CurrentSession { get; }

        public event EventHandler StepChanged;
        public event EventHandler<string> StatusMessageRaised;

        public Task<bool> StartWorkflowAsync();
        public Task<Model_Dunnage_Result_WorkflowStep> AdvanceToNextStepAsync();
        public Task<Model_Dunnage_Result_CSVDelete> DeleteCSVAndResetAsync();
        public void GoToStep(Enum_DunnageWorkflowStep step);
        public Task<Model_Dunnage_Result_Save> SaveSessionAsync();
        public Task<Model_Dunnage_Result_Save> SaveToCSVOnlyAsync();
        public Task<Model_Dunnage_Result_Save> SaveToDatabaseOnlyAsync();
        public void ClearSession();

        public Task<Model_Dunnage_Result_CSVDelete> ResetCSVFilesAsync();
        public void AddCurrentLoaDataTransferObjectsession();
    }
}
