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

/// <summary>
/// Unit tests for Dunnage_TypeSelectionViewModel
/// </summary>
public class Dunnage_TypeSelectionViewModel_Tests
{
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_Pagination> _mockPaginationService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Dunnage_TypeSelectionViewModel _viewModel;

    public Dunnage_TypeSelectionViewModel_Tests()
    {
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockPaginationService = new Mock<IService_Pagination>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _viewModel = new Dunnage_TypeSelectionViewModel(
            _mockWorkflowService.Object,
            _mockDunnageService.Object,
            _mockPaginationService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    #region LoadTypes Tests

    [Fact]
    public async Task LoadTypesCommand_ShouldPopulate9TypesOnPage1()
    {
        // Arrange
        var testTypes = new List<Model_DunnageType>();
        for (int i = 1; i <= 15; i++)
        {
            testTypes.Add(new Model_DunnageType
            {
                Id = i,
                TypeName = $"Type {i}"
            });
        }

        var daoResult = new Model_Dao_Result<List<Model_DunnageType>>
        {
            Success = true,
            Data = testTypes
        };

        _mockDunnageService.Setup(s => s.GetAllTypesAsync())
            .ReturnsAsync(daoResult);

        _mockPaginationService.SetupProperty(p => p.PageSize);
        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageType>())
            .Returns(testTypes.GetRange(0, 9));
        _mockPaginationService.Setup(p => p.CurrentPage).Returns(1);
        _mockPaginationService.Setup(p => p.TotalPages).Returns(2);
        _mockPaginationService.Setup(p => p.HasNextPage).Returns(true);
        _mockPaginationService.Setup(p => p.HasPreviousPage).Returns(false);

        // Act
        await _viewModel.LoadTypesCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(9, _viewModel.DisplayedTypes.Count);
        Assert.Equal("Type 1", _viewModel.DisplayedTypes[0].TypeName);
        Assert.Equal("Type 9", _viewModel.DisplayedTypes[8].TypeName);
        Assert.Equal(1, _viewModel.CurrentPage);
        Assert.Equal(2, _viewModel.TotalPages);
        Assert.True(_viewModel.CanGoNext);
        Assert.False(_viewModel.CanGoPrevious);

        _mockPaginationService.Verify(p => p.SetSource(testTypes), Times.Once);
        _mockPaginationService.VerifySet(p => p.PageSize = 9, Times.Once);
    }

    [Fact]
    public async Task LoadTypesCommand_WhenDaoFails_ShouldShowError()
    {
        // Arrange
        var daoResult = new Model_Dao_Result<List<Model_DunnageType>>
        {
            Success = false,
            ErrorMessage = "Database connection failed"
        };

        _mockDunnageService.Setup(s => s.GetAllTypesAsync())
            .ReturnsAsync(daoResult);

        // Act
        await _viewModel.LoadTypesCommand.ExecuteAsync(null);

        // Assert
        _mockErrorHandler.Verify(
            e => e.HandleDaoErrorAsync(daoResult, It.IsAny<string>(), true),
            Times.Once);
        Assert.Empty(_viewModel.DisplayedTypes);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void NextPageCommand_ShouldAdvanceToPage2()
    {
        // Arrange
        _mockPaginationService.Setup(p => p.HasNextPage).Returns(true);
        _mockPaginationService.Setup(p => p.CurrentPage).Returns(2);
        _mockPaginationService.Setup(p => p.TotalPages).Returns(3);

        // Trigger property updates
        _viewModel.GetType().GetProperty("CanGoNext")?.SetValue(_viewModel, true);

        // Act
        _viewModel.NextPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.NextPage(), Times.Once);
    }

    [Fact]
    public void PreviousPageCommand_ShouldGoBackToPage1()
    {
        // Arrange
        _mockPaginationService.Setup(p => p.HasPreviousPage).Returns(true);
        _mockPaginationService.Setup(p => p.CurrentPage).Returns(1);

        _viewModel.GetType().GetProperty("CanGoPrevious")?.SetValue(_viewModel, true);

        // Act
        _viewModel.PreviousPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.PreviousPage(), Times.Once);
    }

    [Fact]
    public void FirstPageCommand_ShouldNavigateToFirstPage()
    {
        // Act
        _viewModel.FirstPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.FirstPage(), Times.Once);
    }

    [Fact]
    public void LastPageCommand_ShouldNavigateToLastPage()
    {
        // Act
        _viewModel.LastPageCommand.Execute(null);

        // Assert
        _mockPaginationService.Verify(p => p.LastPage(), Times.Once);
    }

    #endregion

    #region SelectType Tests

    [Fact]
    public async Task SelectTypeCommand_ShouldNavigateToPartSelection()
    {
        // Arrange
        var selectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet"
        };

        var mockSession = new Model_DunnageSession();
        _mockWorkflowService.Setup(w => w.CurrentSession).Returns(mockSession);

        // Act
        await _viewModel.SelectTypeCommand.ExecuteAsync(selectedType);

        // Assert
        Assert.Equal(selectedType, _viewModel.SelectedType);
        Assert.Equal(5, mockSession.SelectedTypeId);
        Assert.Equal("Pallet", mockSession.SelectedTypeName);
        
        _mockWorkflowService.Verify(
            w => w.GoToStep(Enum_DunnageWorkflowStep.PartSelection),
            Times.Once);
        
        _mockLogger.Verify(
            l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SelectTypeCommand_WhenTypeIsNull_ShouldNotNavigate()
    {
        // Act
        await _viewModel.SelectTypeCommand.ExecuteAsync(null);

        // Assert
        Assert.Null(_viewModel.SelectedType);
        _mockWorkflowService.Verify(
            w => w.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never);
    }

    [Fact]
    public async Task SelectTypeCommand_WhenException_ShouldHandleError()
    {
        // Arrange
        var selectedType = new Model_DunnageType { Id = 1, TypeName = "Test" };
        _mockWorkflowService.Setup(w => w.CurrentSession)
            .Throws(new InvalidOperationException("Workflow not initialized"));

        // Act
        await _viewModel.SelectTypeCommand.ExecuteAsync(selectedType);

        // Assert
        _mockErrorHandler.Verify(
            e => e.HandleErrorAsync(
                It.IsAny<string>(),
                It.IsAny<Enum_ErrorSeverity>(),
                It.IsAny<Exception>(),
                It.IsAny<bool>()),
            Times.Once);
    }

    #endregion

    #region QuickAdd Tests

    [Fact]
    public async Task QuickAddTypeCommand_ShouldLogNotImplemented()
    {
        // Act
        await _viewModel.QuickAddTypeCommand.ExecuteAsync(null);

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

    #region Pagination Event Tests

    [Fact]
    public void PageChanged_ShouldUpdatePaginationProperties()
    {
        // Arrange
        _mockPaginationService.Setup(p => p.CurrentPage).Returns(2);
        _mockPaginationService.Setup(p => p.TotalPages).Returns(5);
        _mockPaginationService.Setup(p => p.HasNextPage).Returns(true);
        _mockPaginationService.Setup(p => p.HasPreviousPage).Returns(true);

        var typesPage2 = new List<Model_DunnageType>
        {
            new Model_DunnageType { Id = 10, TypeName = "Type 10" }
        };
        _mockPaginationService.Setup(p => p.GetCurrentPageItems<Model_DunnageType>())
            .Returns(typesPage2);

        // Act - Raise the PageChanged event
        _mockPaginationService.Raise(p => p.PageChanged += null, EventArgs.Empty);

        // Assert
        Assert.Equal(2, _viewModel.CurrentPage);
        Assert.Equal(5, _viewModel.TotalPages);
        Assert.True(_viewModel.CanGoNext);
        Assert.True(_viewModel.CanGoPrevious);
        Assert.Equal("Page 2 of 5", _viewModel.PageInfo);
    }

    #endregion
}
