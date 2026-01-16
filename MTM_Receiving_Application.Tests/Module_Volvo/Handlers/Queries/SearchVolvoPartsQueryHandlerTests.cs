using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Unit tests for SearchVolvoPartsQueryHandler
/// </summary>
public class SearchVolvoPartsQueryHandlerTests
{
    private readonly Mock<Dao_VolvoPart> _mockPartDao;
    private readonly SearchVolvoPartsQueryHandler _handler;

    public SearchVolvoPartsQueryHandlerTests()
    {
        _mockPartDao = new Mock<Dao_VolvoPart>("test_connection_string");
        _handler = new SearchVolvoPartsQueryHandler(_mockPartDao.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredParts_WhenSearchTextMatches()
    {
        // Arrange
        var allParts = new List<Model_VolvoPart>
        {
            new Model_VolvoPart { PartNumber = "V-EMB-500", QuantityPerSkid = 10, IsActive = true },
            new Model_VolvoPart { PartNumber = "V-EMB-750", QuantityPerSkid = 15, IsActive = true },
            new Model_VolvoPart { PartNumber = "V-ABC-123", QuantityPerSkid = 20, IsActive = true }
        };

        _mockPartDao
            .Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new Model_Dao_Result<List<Model_VolvoPart>>
            {
                Success = true,
                Data = allParts
            });

        var query = new SearchVolvoPartsQuery
        {
            SearchText = "EMB",
            MaxResults = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(p => p.PartNumber == "V-EMB-500");
        result.Data.Should().Contain(p => p.PartNumber == "V-EMB-750");
    }

    [Fact]
    public async Task Handle_ShouldRespectMaxResults()
    {
        // Arrange
        var allParts = Enumerable.Range(1, 20)
            .Select(i => new Model_VolvoPart
            {
                PartNumber = $"V-EMB-{i:000}",
                QuantityPerSkid = 10,
                IsActive = true
            })
            .ToList();

        _mockPartDao
            .Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new Model_Dao_Result<List<Model_VolvoPart>>
            {
                Success = true,
                Data = allParts
            });

        var query = new SearchVolvoPartsQuery
        {
            SearchText = "EMB",
            MaxResults = 5
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllParts_WhenSearchTextIsEmpty()
    {
        // Arrange
        var allParts = new List<Model_VolvoPart>
        {
            new Model_VolvoPart { PartNumber = "V-EMB-500", QuantityPerSkid = 10, IsActive = true },
            new Model_VolvoPart { PartNumber = "V-EMB-750", QuantityPerSkid = 15, IsActive = true }
        };

        _mockPartDao
            .Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new Model_Dao_Result<List<Model_VolvoPart>>
            {
                Success = true,
                Data = allParts
            });

        var query = new SearchVolvoPartsQuery
        {
            SearchText = "",
            MaxResults = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDaoFails()
    {
        // Arrange
        var expectedError = "Database error";
        _mockPartDao
            .Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new Model_Dao_Result<List<Model_VolvoPart>>
            {
                Success = false,
                ErrorMessage = expectedError
            });

        var query = new SearchVolvoPartsQuery
        {
            SearchText = "EMB",
            MaxResults = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }
}
