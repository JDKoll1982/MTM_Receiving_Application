using System;
using System.Collections.Generic;
using System.Linq;
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

public class Dunnage_ManualEntryViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_DunnageCSVWriter> _mockCsvWriter;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_ManualEntryViewModel _viewModel;

    public Dunnage_ManualEntryViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockCsvWriter = new Mock<IService_DunnageCSVWriter>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _viewModel = new Dunnage_ManualEntryViewModel(
            _mockWorkflowService.Object,
            _mockDunnageService.Object,
            _mockCsvWriter.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region Row Management Tests

    [Fact]
    public void AddRow_ShouldAddNewLoad()
    {
        // Arrange
        var initialCount = _viewModel.Loads.Count;

        // Act
        _viewModel.AddRowCommand.Execute(null);

        // Assert
        Assert.Equal(initialCount + 1, _viewModel.Loads.Count);
        Assert.NotNull(_viewModel.SelectedLoad);
        Assert.Equal(1, _viewModel.SelectedLoad.Quantity);
    }

    [Fact]
    public void AddMultiple_WithValidCount_ShouldAddMultipleRows()
    {
        // Arrange
        var initialCount = _viewModel.Loads.Count;

        // Act
        _viewModel.AddMultipleCommand.Execute("5");

        // Assert
        Assert.Equal(initialCount + 5, _viewModel.Loads.Count);
    }

    [Fact]
    public void AddMultiple_WithInvalidCount_ShouldNotAdd()
    {
        // Arrange
        var initialCount = _viewModel.Loads.Count;

        // Act
        _viewModel.AddMultipleCommand.Execute("0");

        // Assert
        Assert.Equal(initialCount, _viewModel.Loads.Count);
        Assert.Contains("between 1 and 100", _viewModel.StatusMessage);
    }

    [Fact]
    public void AddMultiple_WithExcessiveCount_ShouldNotAdd()
    {
        // Arrange
        var initialCount = _viewModel.Loads.Count;

        // Act
        _viewModel.AddMultipleCommand.Execute("150");

        // Assert
        Assert.Equal(initialCount, _viewModel.Loads.Count);
    }

    [Fact]
    public void RemoveRow_WithSelectedLoad_ShouldRemove()
    {
        // Arrange
        _viewModel.AddRowCommand.Execute(null);
        _viewModel.AddRowCommand.Execute(null);
        var loadToRemove = _viewModel.SelectedLoad;
        var countBeforeRemove = _viewModel.Loads.Count;

        // Act
        _viewModel.RemoveRowCommand.Execute(null);

        // Assert
        Assert.Equal(countBeforeRemove - 1, _viewModel.Loads.Count);
        Assert.DoesNotContain(loadToRemove, _viewModel.Loads);
    }

    #endregion

    #region Fill Blank Spaces Tests

    [Fact]
    public void FillBlankSpacesCommand_ShouldCopyFromLastRow()
    {
        // Arrange
        _viewModel.Loads.Clear();
        
        // Add first load with empty fields
        var firstLoad = new Model_DunnageLoad
        {
            PartId = "P001",
            TypeName = "Pallet",
            Quantity = 1
        };
        _viewModel.Loads.Add(firstLoad);

        // Add second load with filled fields
        var lastLoad = new Model_DunnageLoad
        {
            PartId = "P002",
            TypeName = "Box",
            Quantity = 5,
            PoNumber = "PO12345",
            Location = "Warehouse A",
            SpecValues = new Dictionary<string, object> { { "Length", "48" } }
        };
        _viewModel.Loads.Add(lastLoad);

        // Act
        _viewModel.FillBlankSpacesCommand.Execute(null);

        // Assert
        Assert.Equal("PO12345", firstLoad.PoNumber);
        Assert.Equal("Warehouse A", firstLoad.Location);
        Assert.NotNull(firstLoad.SpecValues);
        Assert.Equal("48", firstLoad.SpecValues["Length"]);
    }

    [Fact]
    public void FillBlankSpaces_WhenOnlyOneRow_ShouldDoNothing()
    {
        // Arrange
        _viewModel.Loads.Clear();
        _viewModel.Loads.Add(new Model_DunnageLoad { PartId = "P001" });

        // Act
        _viewModel.FillBlankSpacesCommand.Execute(null);

        // Assert - no exception, no changes
        Assert.Single(_viewModel.Loads);
    }

    #endregion

    #region Sort For Printing Tests

    [Fact]
    public void SortForPrintingCommand_ShouldSortByPartIdThenPO()
    {
        // Arrange
        _viewModel.Loads.Clear();
        _viewModel.Loads.Add(new Model_DunnageLoad { PartId = "P003", PoNumber = "PO002", TypeName = "Box" });
        _viewModel.Loads.Add(new Model_DunnageLoad { PartId = "P001", PoNumber = "PO001", TypeName = "Pallet" });
        _viewModel.Loads.Add(new Model_DunnageLoad { PartId = "P002", PoNumber = "PO003", TypeName = "Skid" });
        _viewModel.Loads.Add(new Model_DunnageLoad { PartId = "P001", PoNumber = "PO002", TypeName = "Pallet" });

        // Act
        _viewModel.SortForPrintingCommand.Execute(null);

        // Assert
        Assert.Equal("P001", _viewModel.Loads[0].PartId);
        Assert.Equal("PO001", _viewModel.Loads[0].PoNumber);
        Assert.Equal("P001", _viewModel.Loads[1].PartId);
        Assert.Equal("PO002", _viewModel.Loads[1].PoNumber);
        Assert.Equal("P002", _viewModel.Loads[2].PartId);
        Assert.Equal("P003", _viewModel.Loads[3].PartId);
    }

    #endregion

    #region Auto Fill Tests

    [Fact]
    public async Task AutoFillCommand_ShouldSetDefaultQuantity()
    {
        // Arrange
        var load = new Model_DunnageLoad { PartId = "P001", Quantity = 0 };
        _viewModel.Loads.Add(load);

        // Act
        await _viewModel.AutoFillCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, load.Quantity);
    }

    [Fact]
    public async Task AutoFillCommand_ShouldSetInventoryMethod()
    {
        // Arrange
        var loadWithPO = new Model_DunnageLoad { PartId = "P001", PoNumber = "PO123" };
        var loadWithoutPO = new Model_DunnageLoad { PartId = "P002", PoNumber = "" };
        _viewModel.Loads.Add(loadWithPO);
        _viewModel.Loads.Add(loadWithoutPO);

        // Act
        await _viewModel.AutoFillCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("Receive In", loadWithPO.InventoryMethod);
        Assert.Equal("Adjust In", loadWithoutPO.InventoryMethod);
    }

    #endregion

    #region Save Tests

    [Fact]
    public async Task SaveAllCommand_ShouldSaveAndExportCSV()
    {
        // Arrange
        var load = new Model_DunnageLoad
        {
            PartId = "P001",
            TypeName = "Pallet",
            Quantity = 5
        };
        _viewModel.Loads.Add(load);

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

        // Assert
        _mockDunnageService.Verify(
            s => s.SaveLoadsAsync(It.Is<List<Model_DunnageLoad>>(l => l.Count == 1)),
            Times.Once);
        _mockCsvWriter.Verify(
            c => c.WriteToCSVAsync(It.Is<List<Model_DunnageLoad>>(l => l.Count == 1)),
            Times.Once);
        
        // Should clear and add new row
        Assert.Single(_viewModel.Loads);
    }

    [Fact]
    public async Task SaveAllCommand_WithMissingFields_ShouldNotSave()
    {
        // Arrange
        var load = new Model_DunnageLoad
        {
            PartId = "", // Missing
            TypeName = "", // Missing
            Quantity = 5
        };
        _viewModel.Loads.Add(load);

        // Act
        await _viewModel.SaveAllCommand.ExecuteAsync(null);

        // Assert
        _mockDunnageService.Verify(
            s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()),
            Times.Never);
        Assert.Contains("Type and Part ID", _viewModel.StatusMessage);
    }

    #endregion
}
