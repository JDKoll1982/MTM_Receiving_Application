using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_CustomerPullPackReportTests
{
    [Fact]
    public void ActivateView_ShouldShowSharedReportStatus()
    {
        var notificationServiceMock = new Mock<IService_Notification>();
        var viewModel = CreateViewModel(notificationService: notificationServiceMock.Object);

        viewModel.ActivateView();

        notificationServiceMock.Verify(
            service =>
                service.ShowStatus(
                    "Choose a customer, adjust filters if needed, and refresh the report.",
                    MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Informational
                ),
            Times.Once
        );
    }

    [Fact]
    public async void RefreshReportAsync_ShouldPopulateDemandLines_WhenQuerySucceeds()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackReport>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomerPullPack_DemandLine>
                    {
                        new()
                        {
                            SourceLineKey = "LINE-1",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1001",
                            ParentPartId = "PART-100",
                            PullDate = new DateTime(2026, 5, 27),
                            QuantityToPack = 24,
                            FgOnHandQuantity = 20,
                            ShortageFlag = true,
                            HasLinkedWaitlist = true,
                            LinkedWaitlistId = "WL-1",
                            WaitlistStateDisplay = "Requested (WL-1)",
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-200",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 12,
                            FgOnHandQuantity = 30,
                            LateOrderFlag = true,
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel.DemandLines.Should().HaveCount(2);
        viewModel.ShortageLineCount.Should().Be(1);
        viewModel.LateOrderLineCount.Should().Be(1);
        viewModel.LinkedWaitlistLineCount.Should().Be(1);
        viewModel.IsEmptyStateVisible.Should().BeFalse();
        viewModel.SelectedDemandLine.Should().NotBeNull();
        viewModel.ActiveCustomerId.Should().Be("VOLVO");
    }

    [Fact]
    public async void RefreshReportAsync_ShouldShowEmptyState_WhenQueryReturnsNoRows()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackReport>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_CustomerPullPack_DemandLine>())
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "MACK - Mack Trucks";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel.DemandLines.Should().BeEmpty();
        viewModel.IsEmptyStateVisible.Should().BeTrue();
        viewModel.EmptyStateTitle.Should().Contain("MACK");
        viewModel.StatusMessage.Should().Contain("No open demand found");
    }

    [Fact]
    public async void RefreshReportAsync_ShouldRejectMissingCustomerBeforeQuerying()
    {
        var mediatorMock = new Mock<IMediator>();
        var viewModel = CreateViewModel(mediatorMock);

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel.StatusMessage.Should().Be("Customer ID is required before loading the report.");
        mediatorMock.Verify(
            mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackReport>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private static ViewModel_Tool_CustomerPullPackReport CreateViewModel(
        Mock<IMediator>? mediatorMock = null,
        IService_Notification? notificationService = null
    )
    {
        return new ViewModel_Tool_CustomerPullPackReport(
            (mediatorMock ?? new Mock<IMediator>()).Object,
            CreateSessionManager(),
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            notificationService ?? new Mock<IService_Notification>().Object
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
