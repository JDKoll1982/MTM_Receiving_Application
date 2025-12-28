using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.ViewModels.Dunnage;

public class Dunnage_ReviewViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_DunnageCSVWriter> _mockCsvWriter;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_ReviewViewModel _viewModel;
    private readonly Model_DunnageSession _testSession;

    public Dunnage_ReviewViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockCsvWriter = new Mock<IService_DunnageCSVWriter>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _testSession = new Model_DunnageSession();
        _testSession.Loads.Add(new Model_DunnageLoad
        {
            TypeName = "Pallet",
            PartId = "P001",
            Quantity = 5,
            PONumber = "PO12345",
            Location = "Warehouse A"
        });

        _mockWorkflowService.Setup(w => w.CurrentSession).Returns(_testSession);

        _viewModel = new Dunnage_ReviewViewModel(
            _mockWorkflowService.Object,
            _mockDunnageService.Object,
            _mockCsvWriter.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region Load Session Tests

    [Fact]
    public void LoadSessionLoads_ShouldPopulateFromWorkflow()
    {
        // Act
        _viewModel.LoadSessionLoads();

        // Assert
        Assert.Single(_viewModel.SessionLoads);
        Assert.Equal("Pallet", _viewModel.SessionLoads[0].TypeName);
        Assert.Equal("P001", _viewModel.SessionLoads[0].PartId);
        Assert.Equal(1, _viewModel.LoadCount);
    }

    [Fact]
    public void LoadSessionLoads_WhenNoLoads_ShouldDisableSave()
    {
        // Arrange
        _testSession.Loads.Clear();

        // Act
        _viewModel.LoadSessionLoads();

        // Assert
        Assert.Empty(_viewModel.SessionLoads);
        Assert.Equal(0, _viewModel.LoadCount);
        Assert.False(_viewModel.CanSave);
    }

    #endregion

    #region Add Another Tests

    [Fact]
    public void AddAnotherCommand_ShouldPreserveSession()
    {
        // Arrange
        _viewModel.LoadSessionLoads();

        // Act
        _viewModel.AddAnotherCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.ClearSession(),
            Times.Never);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.TypeSelection),
            Times.Once);
    }

    #endregion

    #region Save All Tests

    [Fact]
    public async Task SaveAllCommand_ShouldInsertAndExportCSV()
    {
        // Arrange
        _viewModel.LoadSessionLoads();

        var saveResult = new Model_Dao_Result { Success = true };
        _mockDunnageService.Setup(s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()))
            .ReturnsAsync(saveResult);

        var csvResult = new Model_CSVWriteResult
        {
            LocalSuccess = true,
            LocalFilePath = "C:\\temp\\output.csv"
        };
        _mockCsvWriter.Setup(c => c.WriteToCSVAsync(It.IsAny<List<Model_DunnageLoad>>()))
            .ReturnsAsync(csvResult);

        // Act
        await _viewModel.SaveAllCommand.ExecuteAsync(null);
        await Task.Delay(100); // Allow async operations to complete

        // Assert
        _mockDunnageService.Verify(
            s => s.SaveLoadsAsync(It.Is<List<Model_DunnageLoad>>(l => l.Count == 1)),
            Times.Once);
        _mockCsvWriter.Verify(
            c => c.WriteToCSVAsync(It.Is<List<Model_DunnageLoad>>(l => l.Count == 1)),
            Times.Once);
        _mockWorkflowService.Verify(
            w => w.ClearSession(),
            Times.Once);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.ModeSelection),
            Times.Once);
    }

    [Fact]
    public async Task SaveAllCommand_WhenDatabaseFails_ShouldNotExportCSV()
    {
        // Arrange
        _viewModel.LoadSessionLoads();

        var saveResult = new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = "Database error"
        };
        _mockDunnageService.Setup(s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()))
            .ReturnsAsync(saveResult);

        // Act
        await _viewModel.SaveAllCommand.ExecuteAsync(null);

        // Assert
        _mockDunnageService.Verify(
            s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()),
            Times.Once);
        _mockCsvWriter.Verify(
            c => c.WriteToCSVAsync(It.IsAny<List<Model_DunnageLoad>>()),
            Times.Never);
        _mockWorkflowService.Verify(
            w => w.ClearSession(),
            Times.Never);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public void CancelCommand_ShouldClearSessionAndNavigate()
    {
        // Act
        _viewModel.CancelCommand.Execute(null);

        // Assert
        _mockWorkflowService.Verify(
            w => w.ClearSession(),
            Times.Once);
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.ModeSelection),
            Times.Once);
    }

    #endregion
}
