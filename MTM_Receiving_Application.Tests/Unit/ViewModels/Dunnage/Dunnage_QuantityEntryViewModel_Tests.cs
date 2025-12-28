using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.ViewModels.Dunnage;

public class Dunnage_QuantityEntryViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_QuantityEntryViewModel _viewModel;
    private readonly Model_DunnageSession _testSession;

    public Dunnage_QuantityEntryViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _testSession = new Model_DunnageSession
        {
            SelectedTypeId = 5,
            SelectedTypeName = "Pallet",
            SelectedPart = new Model_DunnagePart
            {
                PartId = "P001",
                TypeId = 5
            }
        };

        _mockWorkflowService.Setup(w => w.CurrentSession).Returns(_testSession);

        _viewModel = new Dunnage_QuantityEntryViewModel(
            _mockWorkflowService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region Validation Tests

    [Fact]
    public void GoNextCommand_WithZeroQuantity_ShouldBeDisabled()
    {
        // Arrange
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 0);

        // Assert
        Assert.False(_viewModel.GoNextCommand.CanExecute(null));
        Assert.False(_viewModel.IsValid);
    }

    [Fact]
    public void GoNextCommand_WithNegativeQuantity_ShouldBeDisabled()
    {
        // Arrange
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, -5);

        // Assert
        Assert.False(_viewModel.GoNextCommand.CanExecute(null));
        Assert.False(_viewModel.IsValid);
    }

    [Fact]
    public void GoNextCommand_WithValidQuantity_ShouldBeEnabled()
    {
        // Arrange
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 5);

        // Assert
        Assert.True(_viewModel.GoNextCommand.CanExecute(null));
        Assert.True(_viewModel.IsValid);
    }

    [Fact]
    public void OnQuantityChanged_ShouldUpdateValidationMessage()
    {
        // Act - Set to invalid
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 0);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.ValidationMessage));
        Assert.Contains("greater than 0", _viewModel.ValidationMessage);

        // Act - Set to valid
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 10);

        // Assert
        Assert.True(string.IsNullOrEmpty(_viewModel.ValidationMessage));
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task GoNextCommand_WithValidQuantity_ShouldNavigateToDetails()
    {
        // Arrange
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 5);

        // Act
        await _viewModel.GoNextCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(5, _testSession.Quantity);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GoNextCommand_WithZeroQuantity_ShouldNotNavigate()
    {
        // Arrange
        _viewModel.GetType().GetProperty("Quantity")?.SetValue(_viewModel, 0);

        // Act
        await _viewModel.GoNextCommand.ExecuteAsync(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never);
    }

    [Fact]
    public void GoBackCommand_ShouldNavigateToPartSelection()
    {
        // Act
        _viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.PartSelection),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Context Loading Tests

    [Fact]
    public void LoadContextData_ShouldPopulateFromSession()
    {
        // Act
        _viewModel.LoadContextData();

        // Assert
        Assert.Equal("Pallet", _viewModel.SelectedTypeName);
        Assert.Equal("P001", _viewModel.SelectedPartName);
    }

    [Fact]
    public void LoadContextData_WhenSessionPartIsNull_ShouldHandleGracefully()
    {
        // Arrange
        _testSession.SelectedPart = null;

        // Act
        _viewModel.LoadContextData();

        // Assert
        Assert.Equal("Pallet", _viewModel.SelectedTypeName);
        Assert.Equal(string.Empty, _viewModel.SelectedPartName);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Quantity_ShouldDefaultToOne()
    {
        // Assert
        Assert.Equal(1, _viewModel.Quantity);
    }

    [Fact]
    public void IsValid_WithDefaultQuantity_ShouldBeTrue()
    {
        // Assert (default is 1)
        Assert.True(_viewModel.IsValid);
        Assert.True(_viewModel.GoNextCommand.CanExecute(null));
    }

    #endregion
}
