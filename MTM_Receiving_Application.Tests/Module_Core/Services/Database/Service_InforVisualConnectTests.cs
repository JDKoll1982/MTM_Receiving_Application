using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Database;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_InforVisualConnectTests
{
    [Fact]
    public async Task TestConnectionAsync_ShouldReturnTrue_WhenUsingMockData()
    {
        // Arrange
        var dao = new Mock<Dao_InforVisualConnection>("test-connection");
        var sut = new Service_InforVisualConnect(dao.Object, useMockData: true);

        // Act
        var result = await sut.TestConnectionAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetPOWithPartsAsync_ShouldReturnMockData_WhenUsingMockData()
    {
        // Arrange
        var dao = new Mock<Dao_InforVisualConnection>("test-connection");
        var sut = new Service_InforVisualConnect(dao.Object, useMockData: true);

        // Act
        var result = await sut.GetPOWithPartsAsync("PO-123");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPartByIDAsync_ShouldReturnMockData_WhenUsingMockData()
    {
        // Arrange
        var dao = new Mock<Dao_InforVisualConnection>("test-connection");
        var sut = new Service_InforVisualConnect(dao.Object, useMockData: true);

        // Act
        var result = await sut.GetPartByIDAsync("PART-1");

        // Assert
        result.Success.Should().BeTrue();
    }
}
