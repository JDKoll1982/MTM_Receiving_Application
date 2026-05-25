using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services;

public sealed class Service_HeaderBackNavigationTests
{
    [Fact]
    public async Task RegisterBackAction_ShouldExposeAndExecuteBackAction()
    {
        var service = new Service_HeaderBackNavigation();
        var actionExecuted = false;

        service.RegisterBackAction(
            () =>
            {
                actionExecuted = true;
                return Task.CompletedTask;
            },
            "Back to Tools"
        );

        service.IsBackButtonVisible.Should().BeTrue();
        service.BackButtonToolTip.Should().Be("Back to Tools");

        await service.ExecuteBackActionAsync();

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void ClearBackAction_ShouldHideBackButton()
    {
        var service = new Service_HeaderBackNavigation();

        service.RegisterBackAction(() => Task.CompletedTask, "Back to Tools");

        service.ClearBackAction();

        service.IsBackButtonVisible.Should().BeFalse();
        service.BackButtonToolTip.Should().Be("Back");
    }
}
