using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_PaginationTests
{
    private readonly Service_Pagination _sut;

    public Service_PaginationTests()
    {
        _sut = new Service_Pagination();
    }

    [Fact]
    public void CurrentPage_ShouldStartAtOne_WhenServiceIsCreated()
    {
        // Arrange

        // Act
        var currentPage = _sut.CurrentPage;

        // Assert
        currentPage.Should().Be(1);
    }

    [Fact]
    public void PageSize_ShouldDefaultToTwenty_WhenServiceIsCreated()
    {
        // Arrange

        // Act
        var pageSize = _sut.PageSize;

        // Assert
        pageSize.Should().Be(20);
    }

    [Fact]
    public void TotalPages_ShouldBeOne_WhenSourceIsNull()
    {
        // Arrange

        // Act
        var totalPages = _sut.TotalPages;

        // Assert
        totalPages.Should().Be(1);
    }

    [Fact]
    public void SetSource_ShouldSetTotalItems_WhenSourceProvided()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        _sut.SetSource(source);

        // Assert
        _sut.TotalItems.Should().Be(3);
    }

    [Fact]
    public void SetSource_ShouldResetCurrentPage_WhenSourceProvided()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.NextPage();

        // Act
        _sut.SetSource(source);

        // Assert
        _sut.CurrentPage.Should().Be(1);
    }

    [Fact]
    public void GetCurrentPageItems_ShouldReturnPageSizeItems_WhenMoreItemsExist()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        var pageItems = _sut.GetCurrentPageItems<int>();

        // Assert
        pageItems.Count().Should().Be(10);
    }

    [Fact]
    public void HasNextPage_ShouldBeTrue_WhenCurrentPageIsLessThanTotalPages()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        var hasNextPage = _sut.HasNextPage;

        // Assert
        hasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrue_WhenCurrentPageIsGreaterThanOne()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;
        _sut.NextPage();

        // Act
        var hasPreviousPage = _sut.HasPreviousPage;

        // Assert
        hasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void NextPage_ShouldReturnTrue_WhenNextPageExists()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        var result = _sut.NextPage();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void NextPage_ShouldAdvanceCurrentPage_WhenNextPageExists()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        _sut.NextPage();

        // Assert
        _sut.CurrentPage.Should().Be(2);
    }

    [Fact]
    public void PreviousPage_ShouldReturnFalse_WhenAtFirstPage()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        var result = _sut.PreviousPage();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PreviousPage_ShouldReturnTrue_WhenNotAtFirstPage()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;
        _sut.NextPage();

        // Act
        var result = _sut.PreviousPage();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FirstPage_ShouldReturnFalse_WhenAlreadyOnFirstPage()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);

        // Act
        var result = _sut.FirstPage();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LastPage_ShouldSetCurrentPageToTotalPages_WhenInvoked()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        _sut.LastPage();

        // Assert
        _sut.CurrentPage.Should().Be(3);
    }

    [Fact]
    public void GoToPage_ShouldClampToFirstPage_WhenPageIsBelowOne()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;
        _sut.NextPage();

        // Act
        _sut.GoToPage(0);

        // Assert
        _sut.CurrentPage.Should().Be(1);
    }

    [Fact]
    public void GoToPage_ShouldClampToLastPage_WhenPageExceedsTotalPages()
    {
        // Arrange
        var source = Enumerable.Range(1, 30).ToList();
        _sut.SetSource(source);
        _sut.PageSize = 10;

        // Act
        _sut.GoToPage(99);

        // Assert
        _sut.CurrentPage.Should().Be(3);
    }
}
