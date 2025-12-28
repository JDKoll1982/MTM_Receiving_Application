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

public class Dunnage_EditModeViewModel_Tests
{
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_Pagination> _mockPaginationService;
    private readonly Mock<IService_DunnageCSVWriter> _mockCsvWriter;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_EditModeViewModel _viewModel;

    public Dunnage_EditModeViewModel_Tests()
    {
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockPaginationService = new Mock<IService_Pagination>();
        _mockCsvWriter = new Mock<IService_DunnageCSVWriter>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _mockPaginationService.Setup(p => p.TotalPages).Returns(1);
        _mockPaginationService.Setup(p => p.CurrentPage).Returns(1);

        _viewModel = new Dunnage_EditModeViewModel(
            _mockDunnageService.Object,
            _mockPaginationService.Object,
            _mockCsvWriter.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region Date Range Tests

    [Fact]
    public void SetDateRangeToday_ShouldSetDatesToToday()
    {
        // Act
        _viewModel.SetDateRangeTodayCommand.Execute(null);

        // Assert
        Assert.Equal(DateTime.Now.Date, _viewModel.FromDate.Date);
        Assert.Equal(DateTime.Now.Date, _viewModel.ToDate.Date);
    }

    [Fact]
    public void SetDateRangeLastWeek_ShouldSetLast7Days()
    {
        // Act
        _viewModel.SetDateRangeLastWeekCommand.Execute(null);

        // Assert
        Assert.Equal(DateTime.Now.Date.AddDays(-7), _viewModel.FromDate.Date);
        Assert.Equal(DateTime.Now.Date, _viewModel.ToDate.Date);
    }

    [Fact]
    public void SetDateRangeLastMonth_ShouldSetLast30Days()
    {
        // Act
        _viewModel.SetDateRangeLastMonthCommand.Execute(null);

        // Assert
        Assert.Equal(DateTime.Now.Date.AddMonths(-1), _viewModel.FromDate.Date);
        Assert.Equal(DateTime.Now.Date, _viewModel.ToDate.Date);
    }

    #endregion

    #region Load From History Tests

    [Fact]
    public async Task LoadFromHistoryCommand_ShouldFilterByDateRange()
    {
        // Arrange
        var testLoads = new List<Model_DunnageLoad>
        {
            new Model_DunnageLoad { PartId = "P001", TypeName = "Pallet" },
            new Model_DunnageLoad { PartId = "P002", TypeName = "Box" }
        };

        var result = new Model_Dao_Result<List<Model_DunnageLoad>>
        {
            Success = true,
            Data = testLoads
        };

        _mockDunnageService.Setup(s => s.GetLoadsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(result);

        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageLoad>())
            .Returns(testLoads);

        // Act
        await _viewModel.LoadFromHistoryCommand.ExecuteAsync(null);

        // Assert
        _mockDunnageService.Verify(
            s => s.GetLoadsByDateRangeAsync(_viewModel.FromDate, _viewModel.ToDate),
            Times.Once);
        Assert.Equal(2, _viewModel.TotalRecords);
    }

    [Fact]
    public async Task LoadFromHistory_WhenMultiplePages_ShouldEnableNavigation()
    {
        // Arrange
        var testLoads = new List<Model_DunnageLoad>();
        for (int i = 0; i < 100; i++)
        {
            testLoads.Add(new Model_DunnageLoad { PartId = $"P{i:000}", TypeName = "Pallet" });
        }

        var result = new Model_Dao_Result<List<Model_DunnageLoad>>
        {
            Success = true,
            Data = testLoads
        };

        _mockDunnageService.Setup(s => s.GetLoadsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(result);

        _mockPaginationService.Setup(p => p.TotalPages).Returns(2);
        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageLoad>())
            .Returns(testLoads.GetRange(0, 50));

        // Act
        await _viewModel.LoadFromHistoryCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(100, _viewModel.TotalRecords);
        Assert.True(_viewModel.CanNavigate);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void NextPageCommand_ShouldNavigateToNextPage()
    {
        // Arrange
        _viewModel.GetType().GetProperty("CurrentPage")?.SetValue(_viewModel, 1);
        _viewModel.GetType().GetProperty("TotalPages")?.SetValue(_viewModel, 3);

        _mockPaginationService.Setup(p => p.CurrentPage).Returns(2);
        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageLoad>())
            .Returns(new List<Model_DunnageLoad>());

        // Act
        _viewModel.NextPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.GoToPage(2), Times.Once);
    }

    [Fact]
    public void PreviousPageCommand_ShouldNavigateToPreviousPage()
    {
        // Arrange
        _viewModel.GetType().GetProperty("CurrentPage")?.SetValue(_viewModel, 2);

        _mockPaginationService.Setup(p => p.CurrentPage).Returns(1);
        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageLoad>())
            .Returns(new List<Model_DunnageLoad>());

        // Act
        _viewModel.PreviousPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.GoToPage(1), Times.Once);
    }

    #endregion

    #region Selection Tests

    [Fact]
    public void SelectAllCommand_ShouldSelectAllLoadsOnPage()
    {
        // Arrange
        _viewModel.FilteredLoads.Add(new Model_DunnageLoad { PartId = "P001" });
        _viewModel.FilteredLoads.Add(new Model_DunnageLoad { PartId = "P002" });
        _viewModel.FilteredLoads.Add(new Model_DunnageLoad { PartId = "P003" });

        // Act
        _viewModel.SelectAllCommand.Execute(null);

        // Assert
        Assert.Equal(3, _viewModel.SelectedLoads.Count);
    }

    [Fact]
    public void RemoveSelectedRowsCommand_ShouldRemoveSelected()
    {
        // Arrange
        var load1 = new Model_DunnageLoad { PartId = "P001" };
        var load2 = new Model_DunnageLoad { PartId = "P002" };
        var load3 = new Model_DunnageLoad { PartId = "P003" };

        _viewModel.FilteredLoads.Add(load1);
        _viewModel.FilteredLoads.Add(load2);
        _viewModel.FilteredLoads.Add(load3);

        _viewModel.SelectedLoads.Add(load1);
        _viewModel.SelectedLoads.Add(load3);

        // Act
        _viewModel.RemoveSelectedRowsCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.FilteredLoads);
        Assert.Equal("P002", _viewModel.FilteredLoads[0].PartId);
        Assert.Empty(_viewModel.SelectedLoads);
    }

    #endregion
}
