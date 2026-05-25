using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services;

public sealed class Service_NotificationTests
{
    [Fact]
    public async Task ShowStatusWithAction_ShouldExposeActionAndExecuteCallback()
    {
        var dispatcherMock = new Mock<IService_Dispatcher>();
        var notificationService = new Service_Notification(dispatcherMock.Object);
        var actionExecuted = false;

        notificationService.ShowStatusWithAction(
            "Duplicate waitlist found.",
            InfoBarSeverity.Warning,
            "Open Existing Item",
            () =>
            {
                actionExecuted = true;
                return Task.CompletedTask;
            }
        );

        notificationService.StatusMessage.Should().Be("Duplicate waitlist found.");
        notificationService.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
        notificationService.IsStatusOpen.Should().BeTrue();
        notificationService.IsStatusActionVisible.Should().BeTrue();
        notificationService.StatusActionLabel.Should().Be("Open Existing Item");

        await notificationService.ExecuteStatusActionAsync();

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void ShowStatus_ShouldClearExistingAction()
    {
        var dispatcherMock = new Mock<IService_Dispatcher>();
        var notificationService = new Service_Notification(dispatcherMock.Object);

        notificationService.ShowStatusWithAction(
            "Duplicate waitlist found.",
            InfoBarSeverity.Warning,
            "Open Existing Item",
            () => Task.CompletedTask
        );

        notificationService.ShowStatus("Reload complete.", InfoBarSeverity.Success);

        notificationService.StatusMessage.Should().Be("Reload complete.");
        notificationService.IsStatusActionVisible.Should().BeFalse();
        notificationService.StatusActionLabel.Should().BeEmpty();
    }
}
