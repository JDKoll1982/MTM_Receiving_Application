using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.ViewModels.Dunnage;

/// <summary>
/// Unit tests for Dunnage_ModeSelectionViewModel
/// </summary>
public class Dunnage_ModeSelectionViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_ModeSelectionViewModel _viewModel;

    public Dunnage_ModeSelectionViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _viewModel = new Dunnage_ModeSelectionViewModel(
            _mockWorkflowService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region SelectGuidedMode Tests

    [Fact]
    public void SelectGuidedModeCommand_ShouldNavigateToTypeSelection()
    {
        // Act
        _viewModel.SelectGuidedModeCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.TypeSelection),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void SelectGuidedModeCommand_ShouldBeExecutable()
    {
        // Assert
        Assert.True(_viewModel.SelectGuidedModeCommand.CanExecute(null));
    }

    #endregion

    #region SelectManualMode Tests

    [Fact]
    public void SelectManualModeCommand_ShouldNavigateToManualEntry()
    {
        // Act
        _viewModel.SelectManualModeCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.ManualEntry),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region SelectEditMode Tests

    [Fact]
    public void SelectEditModeCommand_ShouldNavigateToEditMode()
    {
        // Act
        _viewModel.SelectEditModeCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.EditMode),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region SetGuidedAsDefault Tests

    [Fact]
    public void SetGuidedAsDefaultCommand_WhenChecked_ShouldSetGuidedAsDefault()
    {
        // Act
        _viewModel.SetGuidedAsDefaultCommand.Execute(true);

        // Assert
        Assert.True(_viewModel.IsGuidedModeDefault);
        Assert.False(_viewModel.IsManualModeDefault);
        Assert.False(_viewModel.IsEditModeDefault);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void SetGuidedAsDefaultCommand_WhenUnchecked_ShouldNotChangeOthers()
    {
        // Arrange
        _viewModel.SetManualAsDefaultCommand.Execute(true);

        // Act
        _viewModel.SetGuidedAsDefaultCommand.Execute(false);

        // Assert
        Assert.False(_viewModel.IsGuidedModeDefault);
        Assert.True(_viewModel.IsManualModeDefault); // Should still be true
    }

    #endregion

    #region SetManualAsDefault Tests

    [Fact]
    public void SetManualAsDefaultCommand_WhenChecked_ShouldSetManualAsDefault()
    {
        // Act
        _viewModel.SetManualAsDefaultCommand.Execute(true);

        // Assert
        Assert.False(_viewModel.IsGuidedModeDefault);
        Assert.True(_viewModel.IsManualModeDefault);
        Assert.False(_viewModel.IsEditModeDefault);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region SetEditAsDefault Tests

    [Fact]
    public void SetEditAsDefaultCommand_WhenChecked_ShouldSetEditAsDefault()
    {
        // Act
        _viewModel.SetEditAsDefaultCommand.Execute(true);

        // Assert
        Assert.False(_viewModel.IsGuidedModeDefault);
        Assert.False(_viewModel.IsManualModeDefault);
        Assert.True(_viewModel.IsEditModeDefault);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Default Mode Exclusivity Tests

    [Fact]
    public void DefaultModeCheckboxes_ShouldBeMutuallyExclusive()
    {
        // Act - Set Guided as default
        _viewModel.SetGuidedAsDefaultCommand.Execute(true);
        Assert.True(_viewModel.IsGuidedModeDefault);
        Assert.False(_viewModel.IsManualModeDefault);
        Assert.False(_viewModel.IsEditModeDefault);

        // Act - Set Manual as default
        _viewModel.SetManualAsDefaultCommand.Execute(true);
        Assert.False(_viewModel.IsGuidedModeDefault);
        Assert.True(_viewModel.IsManualModeDefault);
        Assert.False(_viewModel.IsEditModeDefault);

        // Act - Set Edit as default
        _viewModel.SetEditAsDefaultCommand.Execute(true);
        Assert.False(_viewModel.IsGuidedModeDefault);
        Assert.False(_viewModel.IsManualModeDefault);
        Assert.True(_viewModel.IsEditModeDefault);
    }

    #endregion

    #region Property Change Notification Tests

    [Fact]
    public void IsGuidedModeDefault_WhenSet_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsGuidedModeDefault))
                propertyChangedRaised = true;
        };

        // Act
        _viewModel.SetGuidedAsDefaultCommand.Execute(true);

        // Assert
        Assert.True(propertyChangedRaised);
    }

    #endregion
}
