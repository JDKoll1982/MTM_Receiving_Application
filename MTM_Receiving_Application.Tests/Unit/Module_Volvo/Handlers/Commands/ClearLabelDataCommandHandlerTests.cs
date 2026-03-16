using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Handlers.Commands;

/// <summary>
/// Unit tests for <see cref="ClearLabelDataCommandHandler"/>.
/// All logic is exercised against mocked dependencies — no database required.
/// </summary>
public class ClearLabelDataCommandHandlerTests
{
    private readonly Mock<IDao_VolvoLabelHistory> _mockDao;
    private readonly Mock<IService_VolvoAuthorization> _mockAuth;

    public ClearLabelDataCommandHandlerTests()
    {
        _mockDao = new Mock<IDao_VolvoLabelHistory>();
        _mockAuth = new Mock<IService_VolvoAuthorization>();
    }

    private ClearLabelDataCommandHandler CreateHandler() =>
        new(_mockDao.Object, _mockAuth.Object);

    // ─── Constructor guards ────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullHistoryDao_ThrowsArgumentNullException()
    {
        Action act = () => new ClearLabelDataCommandHandler(null!, _mockAuth.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("historyDao");
    }

    [Fact]
    public void Constructor_NullAuthService_ThrowsArgumentNullException()
    {
        Action act = () => new ClearLabelDataCommandHandler(_mockDao.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("authService");
    }

    // ─── Authorization failure ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_AuthorizationFails_ReturnsFailureWithoutCallingDao()
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Failure("Not authorized"));

        var handler = CreateHandler();
        var result = await handler.Handle(new ClearLabelDataCommand { ArchivedBy = "user1" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not authorized");
        _mockDao.Verify(d => d.ClearToHistoryAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── DAO failure propagation ───────────────────────────────────────────

    [Fact]
    public async Task Handle_DaoFails_ReturnsFailureFromDao()
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        _mockDao
            .Setup(d => d.ClearToHistoryAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<(int, int)>("Stored procedure error"));

        var result = await CreateHandler().Handle(new ClearLabelDataCommand { ArchivedBy = "user1" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Stored procedure error");
    }

    // ─── Success path ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(3, 12, 15)]
    [InlineData(0, 0, 0)]
    [InlineData(1, 5, 6)]
    public async Task Handle_DaoSucceeds_ReturnsHeadersPlusLines(int headers, int lines, int expectedTotal)
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        _mockDao
            .Setup(d => d.ClearToHistoryAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<(int, int)>((headers, lines)));

        var result = await CreateHandler().Handle(new ClearLabelDataCommand { ArchivedBy = "user1" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedTotal);
    }

    // ─── ArchivedBy forwarding ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_PopulatedArchivedBy_PassesValueToDao()
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        _mockDao
            .Setup(d => d.ClearToHistoryAsync("jdoe"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<(int, int)>((2, 8)));

        var result = await CreateHandler().Handle(new ClearLabelDataCommand { ArchivedBy = "jdoe" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockDao.Verify(d => d.ClearToHistoryAsync("jdoe"), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_BlankArchivedBy_FallsBackToSYSTEM(string? archivedBy)
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        _mockDao
            .Setup(d => d.ClearToHistoryAsync("SYSTEM"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<(int, int)>((1, 3)));

        var result = await CreateHandler().Handle(
            new ClearLabelDataCommand { ArchivedBy = archivedBy! },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockDao.Verify(d => d.ClearToHistoryAsync("SYSTEM"), Times.Once);
    }

    // ─── Unexpected exception ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DaoThrowsUnexpectedException_ReturnsFailureResult()
    {
        _mockAuth
            .Setup(a => a.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        _mockDao
            .Setup(d => d.ClearToHistoryAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        var result = await CreateHandler().Handle(new ClearLabelDataCommand { ArchivedBy = "user1" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unexpected");
    }
}
