using System.ComponentModel;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.UI;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Shared.ViewModels;

public sealed class ViewModel_Shared_ShellActionBarTests
{
    [Fact]
    public async Task Detach_ShouldCollapseTheBarAndClearEverySlot_WhenTheActivePageSuppliesNoProvider()
    {
        var bar = new ViewModel_Shared_ShellActionBar();
        var provider = new ProviderStub(CreateReceivingSnapshot());
        bar.Attach(provider);
        bar.IsVisible.Should().BeTrue();

        bar.Detach();
        await Task.Yield();

        bar.IsVisible.Should().BeFalse();
        bar.ShowModeGroup.Should().BeFalse();
        bar.Back.IsVisible.Should().BeFalse();
        bar.Next.IsVisible.Should().BeFalse();
        bar.ClearLabelData.IsVisible.Should().BeFalse();
        bar.Help.IsVisible.Should().BeFalse();
        bar.IsGuidedDefault.Should().BeFalse();
        bar.IsManualDefault.Should().BeFalse();
        bar.IsEditDefault.Should().BeFalse();

        // A page that has gone away must not be able to push itself back in.
        provider.RaiseShellActions();
        bar.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void Attach_ShouldHideTheModeGroup_WhenTheSnapshotReportsNoReceivingModes()
    {
        var bar = new ViewModel_Shared_ShellActionBar();
        var provider = new ProviderStub(
            new Model_ShellActionBarState
            {
                IsVisible = true,
                ShowModeGroup = false,
                Help = new Model_ShellActionSlot
                {
                    IsVisible = true,
                    IsEnabled = true,
                    Label = "Help",
                },
            }
        );

        bar.Attach(provider);

        bar.IsVisible.Should().BeTrue();
        bar.ShowModeGroup.Should().BeFalse();
        bar.Guided.Label.Should().BeEmpty();
        bar.Overlay.Label.Should().BeEmpty();
        bar.Help.Label.Should().Be("Help");
    }

    [Fact]
    public void Attach_ShouldReportCollapsed_WhenTheProviderOffersNoActions()
    {
        var bar = new ViewModel_Shared_ShellActionBar();

        bar.Attach(new ProviderStub(Model_ShellActionBarState.Empty));

        bar
            .IsVisible.Should()
            .BeFalse("the strip collapses so the label-shortcut footer keeps its own pane");
    }

    private static Model_ShellActionBarState CreateReceivingSnapshot() =>
        new()
        {
            IsVisible = true,
            ShowModeGroup = true,
            Guided = new Model_ShellModeAction
            {
                Mode = "guided",
                Label = "Guided",
                CanBeDefault = true,
                DefaultTooltip = "make Guided my default",
                ShowDividerAfter = true,
                IsActive = true,
            },
            Manual = new Model_ShellModeAction
            {
                Mode = "manual",
                Label = "Manual",
                CanBeDefault = true,
                DefaultTooltip = "make Manual my default",
                ShowDividerAfter = true,
            },
            Edit = new Model_ShellModeAction
            {
                Mode = "edit",
                Label = "Edit",
                CanBeDefault = true,
                DefaultTooltip = "make Edit my default",
                ShowDividerAfter = true,
            },
            Overlay = new Model_ShellModeAction { Mode = "overlay", Label = "Overlay" },
            Back = new Model_ShellActionSlot
            {
                IsVisible = true,
                IsEnabled = true,
                Label = "Back",
            },
            Next = new Model_ShellActionSlot
            {
                IsVisible = true,
                IsEnabled = true,
                Label = "Next",
                ShowDividerAfter = true,
            },
            ClearLabelData = new Model_ShellActionSlot
            {
                IsVisible = true,
                IsEnabled = true,
                Label = "Clear Label Data",
            },
            Help = new Model_ShellActionSlot
            {
                IsVisible = true,
                IsEnabled = true,
                Label = "Help",
            },
            IsGuidedDefault = true,
        };

    private sealed class ProviderStub : IViewModel_ShellActionProvider
    {
        public ProviderStub(Model_ShellActionBarState state)
        {
            ShellActions = state;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Model_ShellActionBarState ShellActions { get; }

        public void SetDefaultMode(string? storedModeValue) => _ = storedModeValue;

        public void RaiseShellActions() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShellActions)));
    }
}
