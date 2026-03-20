using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services;

/// <summary>
/// Unit tests for the GetOutsideServiceHistoryByPartAsync method on Service_InforVisualConnect.
/// DAO-delegation (live SQL Server path) is covered by integration tests.
/// The service's _useMockData guard and input validation are fully testable without a real connection.
/// </summary>
public class Service_InforVisualConnect_OutsideService_Tests
{
    private readonly Dao_InforVisualConnection _fakeDao;
    private readonly Mock<IService_LoggingUtility> _mockLogger;

    public Service_InforVisualConnect_OutsideService_Tests()
    {
        // Constructor only stores the connection string; no connection is opened until a query runs.
        _fakeDao = new Dao_InforVisualConnection("Server=localhost;Database=FAKE;");
        _mockLogger = new Mock<IService_LoggingUtility>();
    }

    // ─── Input validation (short-circuits before DAO) ──────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetOutsideServiceHistoryByPartAsync_EmptyOrNullPartNumber_ReturnsFailure(
        string? partNumber
    )
    {
        var service = new Service_InforVisualConnect(
            _fakeDao,
            useMockData: true,
            _mockLogger.Object
        );

        var result = await service.GetOutsideServiceHistoryByPartAsync(partNumber!);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Part number cannot be empty");
    }

    // ─── Mock-data mode (no DAO call, returns sample records) ───────────────

    [Fact]
    public async Task GetOutsideServiceHistoryByPartAsync_MockDataMode_ReturnsSampleRecords()
    {
        var service = new Service_InforVisualConnect(
            _fakeDao,
            useMockData: true,
            _mockLogger.Object
        );

        var result = await service.GetOutsideServiceHistoryByPartAsync("PART-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
        result.Data.Should().AllSatisfy(r => r.PartNumber.Should().Be("PART-001"));
    }

    [Fact]
    public async Task GetOutsideServiceHistoryByPartAsync_MockDataMode_LogsMockMessage()
    {
        var service = new Service_InforVisualConnect(
            _fakeDao,
            useMockData: true,
            _mockLogger.Object
        );

        await service.GetOutsideServiceHistoryByPartAsync("PART-LOG");

        _mockLogger.Verify(
            l =>
                l.LogInfo(
                    It.Is<string>(m => m.Contains("MOCK DATA MODE") && m.Contains("PART-LOG"))
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetOutsideServiceHistoryByPartAsync_MockDataMode_DoesNotOpenConnection()
    {
        // Confirms that using mock mode with a bad connection string still succeeds,
        // because it never calls the DAO.
        var badConnDao = new Dao_InforVisualConnection("Server=DOES_NOT_EXIST;Timeout=1;");
        var service = new Service_InforVisualConnect(badConnDao, useMockData: true);

        var result = await service.GetOutsideServiceHistoryByPartAsync("PART-001");

        result.IsSuccess.Should().BeTrue();
    }

    // ─── Constructor guard ─────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullDao_ThrowsArgumentNullException()
    {
        var act = () => new Service_InforVisualConnect(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dao");
    }
}
