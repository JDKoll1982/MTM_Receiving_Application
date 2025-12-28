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

public class Dunnage_PartSelectionViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_PartSelectionViewModel _viewModel;
    private readonly Model_DunnageSession _testSession;

    public Dunnage_PartSelectionViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _testSession = new Model_DunnageSession
        {
            SelectedTypeId = 5,
            SelectedTypeName = "Pallet"
        };

        _mockWorkflowService.Setup(w => w.CurrentSession).Returns(_testSession);

        _viewModel = new Dunnage_PartSelectionViewModel(
            _mockWorkflowService.Object,
            _mockDunnageService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region LoadParts Tests

    [Fact]
    public async Task LoadPartsAsync_ShouldPopulateAvailableParts()
    {
        // Arrange
        var testParts = new List<Model_DunnagePart>
        {
            new Model_DunnagePart { PartId = "P001", Description = "48x40 Pallet", TypeId = 5 },
            new Model_DunnagePart { PartId = "P002", Description = "48x48 Pallet", TypeId = 5 },
            new Model_DunnagePart { PartId = "P003", Description = "40x48 Pallet", TypeId = 5 }
        };

        var daoResult = new Model_Dao_Result<List<Model_DunnagePart>>
        {
            Success = true,
            Data = testParts
        };

        _mockDunnageService.Setup(s => s.GetPartsByTypeAsync(5))
            .ReturnsAsync(daoResult);

        // Act
        await _viewModel.LoadPartsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(3, _viewModel.AvailableParts.Count);
        Assert.Equal("P001", _viewModel.AvailableParts[0].PartId);
        Assert.Equal("P002", _viewModel.AvailableParts[1].PartId);
        Assert.Equal("P003", _viewModel.AvailableParts[2].PartId);
    }

    [Fact]
    public async Task LoadPartsAsync_WhenDaoFails_ShouldHandleError()
    {
        // Arrange
        var daoResult = new Model_Dao_Result<List<Model_DunnagePart>>
        {
            Success = false,
            ErrorMessage = "Database connection failed"
        };

        _mockDunnageService.Setup(s => s.GetPartsByTypeAsync(5))
            .ReturnsAsync(daoResult);

        // Act
        await _viewModel.LoadPartsCommand.ExecuteAsync(null);

        // Assert
        _mockErrorHandler.Verify(
            e => e.HandleDaoErrorAsync(daoResult, It.IsAny<string>(), It.IsAny<bool>()),
            Times.Once);
        Assert.Empty(_viewModel.AvailableParts);
    }

    #endregion

    #region SelectPart Tests

    [Fact]
    public async Task SelectPartCommand_ShouldNavigateToQuantityEntry()
    {
        // Arrange
        var selectedPart = new Model_DunnagePart
        {
            PartId = "P001",
            Description = "48x40 Pallet",
            TypeId = 5
        };

        _viewModel.GetType().GetProperty("SelectedPart")?.SetValue(_viewModel, selectedPart);

        // Act
        await _viewModel.SelectPartCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(selectedPart, _testSession.SelectedPart);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SelectPartCommand_WhenPartIsNull_ShouldNotNavigate()
    {
        // Arrange
        _viewModel.GetType().GetProperty("SelectedPart")?.SetValue(_viewModel, null);

        // Act
        await _viewModel.SelectPartCommand.ExecuteAsync(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never);
    }

    #endregion

    #region Inventory Notification Tests

    [Fact]
    public async Task OnSelectedPartChanged_WhenPartIsInventoried_ShouldShowNotification()
    {
        // Arrange
        var part = new Model_DunnagePart { PartId = "P001", Description = "Test Part" };

        _mockDunnageService.Setup(s => s.IsPartInventoriedAsync("P001"))
            .ReturnsAsync(true);

        // Act
        _viewModel.GetType().GetProperty("SelectedPart")?.SetValue(_viewModel, part);
        await Task.Delay(100); // Give async operation time to complete

        // Assert
        Assert.True(_viewModel.IsInventoryNotificationVisible);
        Assert.Equal("Adjust In", _viewModel.InventoryMethod);
        Assert.Contains("⚠️", _viewModel.InventoryNotificationMessage);
        Assert.Contains("Adjust In", _viewModel.InventoryNotificationMessage);
    }

    [Fact]
    public async Task OnSelectedPartChanged_WhenPartIsNotInventoried_ShouldHideNotification()
    {
        // Arrange
        var part = new Model_DunnagePart { PartId = "P002", Description = "Non-Inventoried Part" };

        _mockDunnageService.Setup(s => s.IsPartInventoriedAsync("P002"))
            .ReturnsAsync(false);

        // Act
        _viewModel.GetType().GetProperty("SelectedPart")?.SetValue(_viewModel, part);
        await Task.Delay(100);

        // Assert
        Assert.False(_viewModel.IsInventoryNotificationVisible);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void GoBackCommand_ShouldNavigateToTypeSelection()
    {
        // Act
        _viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.TypeSelection),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task QuickAddPartCommand_ShouldLogNotImplemented()
    {
        // Act
        await _viewModel.QuickAddPartCommand.ExecuteAsync(null);

        // Assert
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
        _mockErrorHandler.Verify(
            e => e.HandleErrorAsync(
                It.IsAny<string>(),
                It.IsAny<Enum_ErrorSeverity>(),
                It.IsAny<Exception>(),
                It.IsAny<bool>()),
            Times.Once);
    }

    #endregion

    #region Initialize Tests

    [Fact]
    public async Task InitializeAsync_ShouldLoadPartsForSelectedType()
    {
        // Arrange
        var testParts = new List<Model_DunnagePart>
        {
            new Model_DunnagePart { PartId = "P001", Description = "Part 1", TypeId = 5 }
        };

        var daoResult = new Model_Dao_Result<List<Model_DunnagePart>>
        {
            Success = true,
            Data = testParts
        };

        _mockDunnageService.Setup(s => s.GetPartsByTypeAsync(5))
            .ReturnsAsync(daoResult);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.Equal(5, _viewModel.SelectedTypeId);
        Assert.Equal("Pallet", _viewModel.SelectedTypeName);
        Assert.Single(_viewModel.AvailableParts);
    }

    #endregion
}
