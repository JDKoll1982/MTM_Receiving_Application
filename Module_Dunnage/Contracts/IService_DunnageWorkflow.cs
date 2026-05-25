using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts
{
    public interface IService_DunnageWorkflow
    {
        public Enum_DunnageWorkflowStep CurrentStep { get; }
        public Model_DunnageSession CurrentSession { get; }
        public int NumberOfLoads { get; set; }
        public bool IsNavigationLocked { get; }

        public event EventHandler StepChanged;
        public event EventHandler<string> StatusMessageRaised;
        public event EventHandler LabelDataCleared;
        public event EventHandler NavigationLockChanged;

        public Task<bool> StartWorkflowAsync();
        public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();
        public void GoToStep(Enum_DunnageWorkflowStep step);
        public Task<Model_SaveResult> SaveSessionAsync();
        public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();
        public void ClearSession();
        public bool HasUnsavedData();
        public void SetNavigationLock(bool isLocked);

        public Task<Model_Dao_Result<int>> ClearLabelDataAsync(bool clearAllRows = false);
        public Task<bool> HasActiveLabelDataAsync();
        public void AddCurrentLoadToSession();
    }
}
