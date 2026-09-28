using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

public sealed class Service_ScannerExecutionTests
{
    private const string DummyConnectionString =
        "Server=localhost;Database=mtm_receiving_application;Uid=root;Pwd=root;";

    [Fact]
    public async Task SendNextItemAsync_ShouldFail_WhenSessionIsNull()
    {
        var service = CreateExecutionService();

        var result = await service.SendNextItemAsync(null!, CreateProfile());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Session is required.");
    }

    [Fact]
    public async Task SendNextItemAsync_ShouldFail_WhenProfileIsNull()
    {
        var service = CreateExecutionService();

        var result = await service.SendNextItemAsync(new Model_ScannerBatchSession(), null!);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Profile is required.");
    }

    [Fact]
    public async Task SendNextItemAsync_ShouldReturnStopped_WhenStopRequested()
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        var service = CreateExecutionService(engine);
        var session = CreateSession(
            new Model_ScannerBatchItem
            {
                SequenceNumber = 1,
                PayloadPartId = "ABC-123",
                PayloadQuantity = "1",
                ExecutionState = Enum_ScannerExecutionState.Waiting,
                ValidationState = Enum_ScannerValidationState.Valid,
            }
        );
        session.StopRequested = true;

        var result = await service.SendNextItemAsync(session, CreateProfile());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Stopped.Should().BeTrue();
        engine.Verify(engine => engine.SendText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNextItemAsync_ShouldReturnSkipped_WhenNoEligibleItems()
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        var service = CreateExecutionService(engine);
        var session = CreateSession(
            new Model_ScannerBatchItem
            {
                SequenceNumber = 1,
                PayloadPartId = "ABC-123",
                PayloadQuantity = "1",
                ExecutionState = Enum_ScannerExecutionState.Sent,
                ValidationState = Enum_ScannerValidationState.Valid,
            }
        );

        var result = await service.SendNextItemAsync(session, CreateProfile());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SkippedCount.Should().Be(1);
        result.Data.SentCount.Should().Be(0);
        engine.Verify(engine => engine.SendText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendSpecificItemAsync_ShouldReturnSkipped_WhenItemNotEligible()
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        var service = CreateExecutionService(engine);
        var session = CreateSession();
        var item = new Model_ScannerBatchItem
        {
            SequenceNumber = 1,
            PayloadPartId = "ABC-123",
            PayloadQuantity = "1",
            ExecutionState = Enum_ScannerExecutionState.Sent,
            ValidationState = Enum_ScannerValidationState.Valid,
        };

        var result = await service.SendSpecificItemAsync(session, item, CreateProfile());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SkippedCount.Should().Be(1);
        engine.Verify(engine => engine.SendText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ClearTargetFormAsync_ShouldSendAltL()
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service => service.SendChord(0x0001, 0x4C))
            .Returns(true);
        var service = CreateExecutionService(engine);

        var result = await service.ClearTargetFormAsync();

        result.Success.Should().BeTrue();
        engine.Verify(service => service.SendChord(0x0001, 0x4C), Times.Once);
    }

    [Fact]
    public async Task ClearTargetFormAsync_ShouldFail_WhenAltLSendBlocked()
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service => service.SendChord(0x0001, 0x4C))
            .Returns(false);
        var service = CreateExecutionService(engine);

        var result = await service.ClearTargetFormAsync();

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Could not clear the target form.");
    }

    private static Service_ScannerExecution CreateExecutionService(
        Mock<IService_ScannerInputEngine>? engine = null
    )
    {
        return new Service_ScannerExecution(
            (engine ?? new Mock<IService_ScannerInputEngine>()).Object,
            new Dao_ScannerBatchItem(DummyConnectionString),
            new Dao_ScannerRunHistory(DummyConnectionString),
            new Mock<IService_LoggingUtility>().Object
        );
    }

    private static Model_ScannerBatchSession CreateSession(params Model_ScannerBatchItem[] items)
    {
        var session = new Model_ScannerBatchSession
        {
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator",
        };
        foreach (var item in items)
        {
            item.SessionId = session.SessionId;
            session.Items.Add(item);
        }

        return session;
    }

    private static Model_ScannerProfile CreateProfile()
    {
        return new Model_ScannerProfile
        {
            OwnerUserId = "u-1",
            ProfileName = "Default",
            TargetExecutableName = "VMINVENT.exe",
            AppWindowTitle = "Inventory Transfers",
            TargetChildWindowTitle = "Inventory Transfers",
        };
    }
}
