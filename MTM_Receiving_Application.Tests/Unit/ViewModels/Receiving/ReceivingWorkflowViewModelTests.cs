using Xunit;
using Moq;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Receiving;
using System.Threading.Tasks;
using System;

namespace MTM_Receiving_Application.Tests.Unit.ViewModels.Receiving
{
    public class ReceivingWorkflowViewModelTests
    {
        private Mock<IService_ReceivingWorkflow> _mockWorkflowService;
        private Mock<IService_ErrorHandler> _mockErrorHandler;
        private Mock<ILoggingService> _mockLogger;
        private ReceivingWorkflowViewModel _viewModel;

        public ReceivingWorkflowViewModelTests()
        {
            _mockWorkflowService = new Mock<IService_ReceivingWorkflow>();
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _mockLogger = new Mock<ILoggingService>();
            _viewModel = new ReceivingWorkflowViewModel(_mockWorkflowService.Object, _mockErrorHandler.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializeWithDefaultState()
        {
            Assert.NotNull(_viewModel);
            Assert.Equal("Receiving Workflow", _viewModel.CurrentStepTitle);
        }

        [Fact]
        public async Task NextStepCommand_ShouldCallServiceNextStep()
        {
            // Arrange
            _mockWorkflowService.Setup(s => s.AdvanceToNextStepAsync()).ReturnsAsync(new WorkflowStepResult { Success = true, NewStep = WorkflowStep.LoadEntry });

            // Act
            await _viewModel.NextStepCommand.ExecuteAsync(null);

            // Assert
            _mockWorkflowService.Verify(s => s.AdvanceToNextStepAsync(), Times.Once);
        }

        [Fact]
        public void PreviousStepCommand_ShouldCallServicePreviousStep()
        {
            // Arrange
            _mockWorkflowService.Setup(s => s.GoToPreviousStep()).Returns(new WorkflowStepResult { Success = true, NewStep = WorkflowStep.POEntry });

            // Act
            _viewModel.PreviousStepCommand.Execute(null);

            // Assert
            _mockWorkflowService.Verify(s => s.GoToPreviousStep(), Times.Once);
        }

        [Fact]
        public async Task NextStepCommand_ShouldTriggerSave_WhenStepIsSaving()
        {
            // Arrange
            var saveResult = new SaveResult { Success = true, LoadsSaved = 5 };
            _mockWorkflowService.Setup(s => s.AdvanceToNextStepAsync()).ReturnsAsync(new WorkflowStepResult { Success = true, NewStep = WorkflowStep.Saving });
            _mockWorkflowService.Setup(s => s.CurrentStep).Returns(WorkflowStep.Saving);
            _mockWorkflowService.Setup(s => s.SaveSessionAsync(It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>()))
                .ReturnsAsync(saveResult);

            // Act
            await _viewModel.NextStepCommand.ExecuteAsync(null);

            // Assert
            _mockWorkflowService.Verify(s => s.SaveSessionAsync(It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>()), Times.Once);
            Assert.Equal(saveResult, _viewModel.LastSaveResult);
        }
    }
}
