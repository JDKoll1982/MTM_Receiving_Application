using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reprint.Services;

public sealed class Service_ReprintTests
{
    [Fact]
    public async Task ReceivingReprintAsync_ShouldBucketQueuedAlreadyQueuedAndFailed_WhenMixOfOutcomes()
    {
        var receivingService = new Mock<IService_MySQL_Receiving>();
        receivingService
            .Setup(s => s.InsertFromHistoryAsync(1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        receivingService
            .Setup(s => s.InsertFromHistoryAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<int>(
                    "This history record is already queued for reprint."
                )
            );
        receivingService
            .Setup(s => s.InsertFromHistoryAsync(3))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<int>("Database unavailable."));

        var service = new Service_Reprint_Receiving(
            receivingService.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.ReprintAsync(["1", "2", "3", "1"]);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.QueuedCount.Should().Be(1);
        result.Data.AlreadyQueuedCount.Should().Be(1);
        result.Data.FailedCount.Should().Be(1);
        result.Data.Queued.Should().Contain("1");
        result.Data.AlreadyQueued.Should().Contain("2");
        result.Data.Failed.Should().Contain("3");
    }

    [Fact]
    public async Task DunnageReprintAsync_ShouldBucketAlreadyQueuedAsDuplicate_WhenDaoReportsAlreadyQueued()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(s => s.InsertFromHistoryAsync("load-1"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        dunnageService
            .Setup(s => s.InsertFromHistoryAsync("load-2"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<int>(
                    "This history record is already queued for reprint."
                )
            );

        var service = new Service_Reprint_Dunnage(
            dunnageService.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.ReprintAsync(["load-1", "load-2"]);

        result.Data.Should().NotBeNull();
        result.Data!.QueuedCount.Should().Be(1);
        result.Data.AlreadyQueuedCount.Should().Be(1);
        result.Data.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task VolvoReprintAsync_ShouldPassUserContextAndBucketOutcomes()
    {
        var generatedLabelDao = new Mock<IDao_VolvoGeneratedLabelData>();
        generatedLabelDao
            .Setup(d => d.InsertFromHistoryAsync(1, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        generatedLabelDao
            .Setup(d => d.InsertFromHistoryAsync(2, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<int>(
                    "This history record is already queued for reprint."
                )
            );

        var service = new Service_Reprint_Volvo(
            generatedLabelDao.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.ReprintAsync(["1", "2"]);

        generatedLabelDao.Verify(d => d.InsertFromHistoryAsync(1, It.IsAny<string>(), It.IsAny<int>()));
        result.Data.Should().NotBeNull();
        result.Data!.QueuedCount.Should().Be(1);
        result.Data.AlreadyQueuedCount.Should().Be(1);
    }
}
