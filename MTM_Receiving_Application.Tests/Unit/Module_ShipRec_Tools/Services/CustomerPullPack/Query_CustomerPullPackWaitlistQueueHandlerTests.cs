using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Query_CustomerPullPackWaitlistQueueHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnQueueItems_FromDao()
    {
        var expectedItems = new List<Model_CustomerPullPack_WaitlistEntry>
        {
            new() { WaitlistId = "WL-1", CustomerOrderId = "CO-1001" },
        };

        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>("Server=localhost;");
        daoMock
            .Setup(dao =>
                dao.GetQueueAsync(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<IReadOnlyCollection<MTM_Receiving_Application.Module_ShipRec_Tools.Enums.Enum_CustomerPullPackWaitlistStatus>?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedItems));

        var handler = new Query_CustomerPullPackWaitlistQueueHandler(daoMock.Object);

        var result = await handler.Handle(
            new Query_CustomerPullPackWaitlistQueue(CustomerId: "VOLVO"),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedItems);
    }
}
