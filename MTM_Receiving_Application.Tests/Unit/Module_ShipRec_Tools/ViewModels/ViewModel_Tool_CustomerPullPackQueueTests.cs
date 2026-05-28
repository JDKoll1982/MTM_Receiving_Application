using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_CustomerPullPackQueueTests
{
    [Fact]
    public async Task ActivateViewAsync_ShouldLoadQueueAndSelectFirstItem()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackWaitlistQueue>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomerPullPack_WaitlistEntry>
                    {
                        new()
                        {
                            WaitlistId = "WL-1",
                            CustomerOrderId = "CO-1001",
                            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);

        await viewModel.ActivateViewAsync();

        viewModel.QueueItems.Should().ContainSingle();
        viewModel.SelectedQueueItem.Should().NotBeNull();
        viewModel.SelectedQueueItem!.WaitlistId.Should().Be("WL-1");
    }

    [Fact]
    public async Task SaveStatusUpdateAsync_ShouldSendStatusCommand()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackWaitlistQueue>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomerPullPack_WaitlistEntry>
                    {
                        new()
                        {
                            WaitlistId = "WL-1",
                            CustomerOrderId = "CO-1001",
                            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
                        },
                    }
                )
            );
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Command_CustomerPullPackUpdateStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry
                    {
                        WaitlistId = "WL-1",
                        CustomerOrderId = "CO-1001",
                        CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Accepted,
                        CurrentOwnerUserId = "jkoll",
                        CurrentOwnerDisplayName = "jkoll",
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        await viewModel.ActivateViewAsync();
        viewModel.SelectedStatus = Enum_CustomerPullPackWaitlistStatus.Accepted;

        await viewModel.SaveStatusUpdateCommand.ExecuteAsync(null);

        mediatorMock.Verify(
            mediator =>
                mediator.Send(
                    It.Is<Command_CustomerPullPackUpdateStatus>(command =>
                        command.WaitlistId == "WL-1"
                        && command.NewStatus == Enum_CustomerPullPackWaitlistStatus.Accepted
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public void SelectedQueueItem_ShouldSetCanUnassignOwner_WhenOwnedByCurrentUser()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedQueueItem = new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = "WL-1",
            CurrentOwnerUserId = "jkoll",
            CurrentOwnerDisplayName = "Handler One",
            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Accepted,
        };

        viewModel.CanUnassignOwner.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshSelectedQueueLocationDetailsAsync_ShouldRehydrateAndSortCurrentLocationRows()
    {
        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        demandSourceMock
            .Setup(service =>
                service.GetDemandAsync(It.IsAny<Model_CustomerPullPack_DemandFilter>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomerPullPack_DemandLine>
                    {
                        new()
                        {
                            CustomerId = "VOLVO",
                            CustomerOrderId = "CO-1001",
                            ParentPartId = "PART-100",
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "B-02",
                                    ParentPartId = "PART-100",
                                    OnHandQuantity = 4,
                                },
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "A-01",
                                    ParentPartId = "PART-100",
                                    OnHandQuantity = 7,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(demandSourceMock: demandSourceMock);
        viewModel.SelectedQueueItem = new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = "WL-1",
            CustomerId = "VOLVO",
            CustomerOrderId = "CO-1001",
            ParentPartId = "PART-100",
            RequestTimestamp = new DateTime(2026, 5, 27, 12, 0, 0, DateTimeKind.Utc),
        };

        await viewModel.RefreshSelectedQueueLocationDetailsAsync();

        viewModel.SelectedQueueLocationDetails.Should().HaveCount(2);
        viewModel.SelectedQueueLocationDetails[0].LocationId.Should().Be("A-01");
        viewModel.SelectedQueueLocationDetails[1].LocationId.Should().Be("B-02");
    }

    private static ViewModel_Tool_CustomerPullPackQueue CreateViewModel(
        Mock<IMediator>? mediatorMock = null,
        Mock<IService_CustomerPullPackDemandSource>? demandSourceMock = null
    )
    {
        return new ViewModel_Tool_CustomerPullPackQueue(
            (mediatorMock ?? new Mock<IMediator>()).Object,
            (demandSourceMock ?? new Mock<IService_CustomerPullPackDemandSource>()).Object,
            CreateSessionManager(),
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static IService_UserSessionManager CreateSessionManager()
    {
        var sessionManagerMock = new Mock<IService_UserSessionManager>();
        sessionManagerMock
            .SetupGet(manager => manager.CurrentSession)
            .Returns(
                new Model_UserSession
                {
                    User = new Model_User
                    {
                        WindowsUsername = "jkoll",
                        FullName = "John Koll",
                        EmployeeNumber = 1,
                    },
                }
            );
        return sessionManagerMock.Object;
    }
}
