using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.ViewModels.Dunnage;

public class Dunnage_DetailsEntryViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_DetailsEntryViewModel _viewModel;
    private readonly Model_DunnageSession _testSession;

    public Dunnage_DetailsEntryViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _testSession = new Model_DunnageSession
        {
            SelectedTypeId = 5,
            SelectedTypeName = "Pallet",
            SelectedType = new Model_DunnageType
            {
                Id = 5,
                TypeName = "Pallet"
            },
            SelectedPart = new Model_DunnagePart
            {
                PartId = "P001",
                TypeId = 5
            }
        };

        _mockWorkflowService.Setup(w => w.CurrentSession).Returns(_testSession);

        _viewModel = new Dunnage_DetailsEntryViewModel(
            _mockWorkflowService.Object,
            _mockDunnageService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region PO Number Change Tests

    [Fact]
    public void OnPoNumberChanged_WhenEmpty_ShouldSetInventoryMethodToAdjustIn()
    {
        // Arrange
        _viewModel.GetType().GetProperty("IsInventoryNotificationVisible")?.SetValue(_viewModel, true);

        // Act
        _viewModel.GetType().GetProperty("PoNumber")?.SetValue(_viewModel, string.Empty);

        // Assert
        Assert.Equal("Adjust In", _viewModel.InventoryMethod);
        Assert.Contains("Adjust In", _viewModel.InventoryNotificationMessage);
    }

    [Fact]
    public void OnPoNumberChanged_WhenProvided_ShouldSetInventoryMethodToReceiveIn()
    {
        // Arrange
        _viewModel.GetType().GetProperty("IsInventoryNotificationVisible")?.SetValue(_viewModel, true);

        // Act
        _viewModel.GetType().GetProperty("PoNumber")?.SetValue(_viewModel, "PO12345");

        // Assert
        Assert.Equal("Receive In", _viewModel.InventoryMethod);
        Assert.Contains("Receive In", _viewModel.InventoryNotificationMessage);
    }

    [Fact]
    public void OnPoNumberChanged_ShouldUpdateInventoryMessage()
    {
        // Arrange
        _viewModel.GetType().GetProperty("IsInventoryNotificationVisible")?.SetValue(_viewModel, true);

        // Act
        _viewModel.GetType().GetProperty("PoNumber")?.SetValue(_viewModel, "PO12345");

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.InventoryNotificationMessage));
        Assert.Contains("⚠️", _viewModel.InventoryNotificationMessage);
    }

    #endregion

    #region Load Specs Tests

    [Fact]
    public async Task LoadSpecsForSelectedPart_WhenTypeHasNoSpecs_ShouldCreateEmptyList()
    {
        // Arrange
        _mockDunnageService.Setup(s => s.IsPartInventoriedAsync("P001"))
            .ReturnsAsync(false);

        // Act
        await _viewModel.LoadSpecsForSelectedPartAsync();

        // Assert
        Assert.Empty(_viewModel.SpecInputs);
    }

    [Fact]
    public async Task LoadSpecsForSelectedPart_WhenPartIsInventoried_ShouldShowNotification()
    {
        // Arrange
        _mockDunnageService.Setup(s => s.IsPartInventoriedAsync("P001"))
            .ReturnsAsync(true);

        // Act
        await _viewModel.LoadSpecsForSelectedPartAsync();

        // Assert
        Assert.True(_viewModel.IsInventoryNotificationVisible);
        Assert.False(string.IsNullOrEmpty(_viewModel.InventoryNotificationMessage));
    }

    [Fact]
    public async Task LoadSpecsForSelectedPart_WhenPartNotInventoried_ShouldNotShowNotification()
    {
        // Arrange
        _mockDunnageService.Setup(s => s.IsPartInventoriedAsync("P001"))
            .ReturnsAsync(false);

        // Act
        await _viewModel.LoadSpecsForSelectedPartAsync();

        // Assert
        Assert.False(_viewModel.IsInventoryNotificationVisible);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void GoBackCommand_ShouldNavigateToQuantityEntry()
    {
        // Act
        _viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry),
            Times.Once);
    }

    [Fact]
    public async Task GoNextCommand_ShouldSaveDetailsAndNavigateToReview()
    {
        // Arrange
        _viewModel.GetType().GetProperty("PoNumber")?.SetValue(_viewModel, "PO12345");
        _viewModel.GetType().GetProperty("Location")?.SetValue(_viewModel, "Warehouse A");

        // Act
        await _viewModel.GoNextCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("PO12345", _testSession.PONumber);
        Assert.Equal("Warehouse A", _testSession.Location);
        Assert.NotNull(_testSession.SpecValues);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.Review),
            Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GoNextCommand_WithMissingRequiredSpec_ShouldNotNavigate()
    {
        // Arrange
        var requiredSpec = new Model_SpecInput
        {
            SpecName = "Length",
            IsRequired = true,
            Value = null
        };
        _viewModel.SpecInputs.Add(requiredSpec);

        // Act
        await _viewModel.GoNextCommand.ExecuteAsync(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never);
        Assert.Contains("Length", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task GoNextCommand_WithAllRequiredSpecsFilled_ShouldNavigate()
    {
        // Arrange
        var requiredSpec = new Model_SpecInput
        {
            SpecName = "Length",
            IsRequired = true,
            Value = "48"
        };
        _viewModel.SpecInputs.Add(requiredSpec);

        // Act
        await _viewModel.GoNextCommand.ExecuteAsync(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.Review),
            Times.Once);
    }

    #endregion
}
