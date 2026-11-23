using System;
using System.IO;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.UI;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ShellActionTests
{
    /// <summary>
    /// The per-page matrix from contracts/per-page-action-matrix.md, in the order the strip draws
    /// the actions: the mode group, then Back, Next, Clear All Labels, Help.
    /// </summary>
    private static readonly Dictionary<Enum_DunnageWorkflowStep, string[]> Matrix = new()
    {
        [Enum_DunnageWorkflowStep.TypeSelection] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Next",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.PartSelection] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Back",
            "Next",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.QuantityEntry] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Back",
            "Next",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.DetailsEntry] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Back",
            "Review",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.Review] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Back",
            "Save",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.EditMode] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Save",
            "Clear All Labels",
            "Help",
        ],
        [Enum_DunnageWorkflowStep.ImagePartSearch] =
        [
            "Guided",
            "Edit",
            "Image Search",
            "Back",
            "Clear All Labels",
            "Help",
        ],
    };

    [Fact]
    public async Task ShellActions_ShouldBeVisibleWithItsActions_OnEveryOneOfTheSevenPages()
    {
        foreach (var step in Matrix.Keys)
        {
            var page = await CreatePageAsync(step);

            var shell = page.ShellActions;
            shell.IsVisible.Should().BeTrue($"{step} is a Non-Inventoried Labels page");
            shell
                .ShowModeGroup.Should()
                .BeTrue("every Non-Inventoried Labels page offers the module's mode chips");
            Offered(shell).Should().NotBeEmpty($"{step} offers at least one action");
        }
    }

    [Fact]
    public async Task ShellActions_ShouldOfferExactlyGuidedEditAndImageSearchInThatOrder_WhenThePageIsNonInventoriedLabels()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.TypeSelection);

        var shell = page.ShellActions;
        shell
            .Guided.Mode.Should()
            .Be("guided", "the left group draws Guided first, then Edit, then Image Search");
        shell.Edit.Mode.Should().Be("edit");
        shell.ImageSearch.Mode.Should().Be("image-search");
        shell.Guided.Label.Should().Be("Guided");
        shell.Edit.Label.Should().Be("Edit");
        shell.ImageSearch.Label.Should().Be("Image Search");
        shell
            .Guided.ShowDividerAfter.Should()
            .BeTrue("a divider separates Guided from Edit");
        shell.Edit.ShowDividerAfter.Should().BeTrue("a divider separates Edit from Image Search");
        shell
            .ImageSearch.ShowDividerAfter.Should()
            .BeFalse("Image Search is the last chip, so a trailing divider would hang off the group");
    }

    [Fact]
    public void MainWindowXaml_ShouldSpaceModeChipsFromTheDividerMargin_SoHiddenChipsLeaveNoGap()
    {
        // Dunnage hides By Receiver and Manual, so Guided and Edit are three empty columns apart.
        // ColumnSpacing still inserts its gap between those empty columns, which pushed the Edit
        // button 24px away from Guided instead of 8, so the chip rhythm broke.
        var content = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "MainWindow.xaml")
        );

        var panelStart = content.IndexOf(
            "x:Name=\"ShellModeGroupPanel\"",
            StringComparison.Ordinal
        );
        panelStart.Should().BeGreaterThanOrEqualTo(0);

        var panelHeaderStart = content.LastIndexOf('<', panelStart);
        var panelDeclaration = content.Substring(
            panelHeaderStart,
            content.IndexOf('>', panelStart) - panelHeaderStart
        );

        panelDeclaration
            .Should()
            .NotContain(
                "ColumnSpacing",
                "spacing has to come from the dividers, or every collapsed chip adds a hidden gap"
            );

        content
            .Split("Margin=\"8,6,8,0\"")
            .Should()
            .HaveCount(6, "the five mode dividers and no leftover 4px margin carry the gaps");
        content.Should().NotContain("Margin=\"4,6,4,0\"");
    }

    [Fact]
    public async Task ShellActions_ShouldLeaveManualAndOverlayHidden_WhenThePageIsNonInventoriedLabels()
    {
        foreach (var step in Matrix.Keys)
        {
            var page = await CreatePageAsync(step);

            page.ShellActions.Manual.IsVisible.Should().BeFalse($"{step} does not own Manual");
            page.ShellActions.Overlay.IsVisible.Should().BeFalse($"{step} does not own Overlay");
            page.ShellActions.IsManualDefault.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ShellActions_ShouldOfferBackNextClearLabelDataAndHelpInThatOrder_WithTheDividerBetweenBackAndNextOnly_WhenBothAreShown()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.PartSelection);

        var shell = page.ShellActions;
        shell.Back.IsVisible.Should().BeTrue();
        shell.Back.Label.Should().Be("Back");
        shell.Next.IsVisible.Should().BeTrue();
        shell.Next.Label.Should().Be("Next");
        shell.ClearLabelData.IsVisible.Should().BeTrue();
        shell.ClearLabelData.Label.Should().Be("Clear All Labels");
        shell.Help.IsVisible.Should().BeTrue();
        shell.Help.Label.Should().Be("Help");

        shell
            .Back.ShowDividerAfter.Should()
            .BeFalse("the one divider in the centre group sits before Next");
        shell.Next.ShowDividerAfter.Should().BeFalse();
        shell.ClearLabelData.ShowDividerAfter.Should().BeFalse();
        shell.Help.ShowDividerAfter.Should().BeFalse();
        page.Bar.ShowDividerAfterBack.Should().BeTrue();
    }

    [Fact]
    public async Task ShellActions_ShouldOmitBack_WhenThePageHasNoWayBack()
    {
        var selectType = await CreatePageAsync(Enum_DunnageWorkflowStep.TypeSelection);
        selectType
            .ShellActions.Back.IsVisible.Should()
            .BeFalse("Select Type is the first guided page; Back is removed, not disabled");
        selectType.ShellActions.Back.Command.Should().BeNull();

        var editMode = await CreatePageAsync(Enum_DunnageWorkflowStep.EditMode);
        editMode
            .ShellActions.Back.IsVisible.Should()
            .BeFalse("Edit Mode is entered from the mode chip and offers no Back");
        editMode.Bar.ShowDividerAfterBack.Should().BeFalse("there is nothing to separate");
    }

    [Fact]
    public async Task ShellActions_ShouldDisableNext_UntilThePageConditionHolds()
    {
        var selectType = await CreatePageAsync(Enum_DunnageWorkflowStep.TypeSelection);
        selectType.ShellActions.Next.IsVisible.Should().BeTrue();
        selectType
            .ShellActions.Next.IsEnabled.Should()
            .BeFalse("no type is chosen yet");

        var selectPart = await CreatePageAsync(Enum_DunnageWorkflowStep.PartSelection);
        selectPart.ShellActions.Next.IsVisible.Should().BeTrue();
        selectPart
            .ShellActions.Next.IsEnabled.Should()
            .BeFalse("no part is chosen yet");

        var enterLoads = await CreatePageAsync(Enum_DunnageWorkflowStep.QuantityEntry);
        enterLoads.ShellActions.Next.IsVisible.Should().BeTrue();
        enterLoads
            .ShellActions.Next.IsEnabled.Should()
            .BeFalse("the entry is not valid yet");
    }

    [Fact]
    public async Task ClearLabelDataSlot_ShouldCarryParameterlessCommands_ForAPlainAndAShiftActivation()
    {
        var page = await CreatePageAsync(
            Enum_DunnageWorkflowStep.PartSelection,
            workflow => workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true)
        );

        var slot = page.ShellActions.ClearLabelData;
        slot.Command.Should().NotBeNull();
        slot.ShiftCommand.Should().NotBeNull();
        slot
            .Command!.CanExecute(null)
            .Should()
            .BeTrue(
                "the strip binds the command with no parameter, so a bool-parameter command would report false and leave the button dead"
            );
        slot
            .ShiftCommand!.CanExecute(null)
            .Should()
            .BeTrue("the strip invokes the shift variant the same way");
        slot.ShiftCommand.Should().NotBeSameAs(slot.Command);
    }

    [Fact]
    public async Task HelpSlot_ShouldOpenHelpForTheCurrentStep_WhenInvoked()
    {
        var help = new Mock<IService_Help>();
        var page = await CreatePageAsync(
            Enum_DunnageWorkflowStep.DetailsEntry,
            helpService: help
        );

        var command = page.ShellActions.Help.Command;
        command.Should().NotBeNull();
        command!.Execute(null);
        await Task.Delay(25);

        help.Verify(
            service =>
                service.ShowContextualHelpAsync(Enum_DunnageWorkflowStep.DetailsEntry),
            Times.Once
        );
    }

    [Fact]
    public async Task ShellActions_ShouldMatchThePerPageMatrixActionForAction_OnAllSevenPages()
    {
        foreach (var (step, expected) in Matrix)
        {
            var page = await CreatePageAsync(step);

            page.Offered.Should().Equal(
                expected,
                $"{step} offers exactly the actions the matrix gives it, and nothing extra"
            );
        }
    }

    [Fact]
    public async Task NextSlotAndReviewSlot_ShouldCarryTheCommandsTheHostedViewModelsAlreadyOwn()
    {
        var selectPart = await CreatePageAsync(Enum_DunnageWorkflowStep.PartSelection);
        selectPart
            .ShellActions.Next.Command.Should()
            .BeSameAs(selectPart.Hosts.PartSelection.SelectPartCommand);

        var enterLoads = await CreatePageAsync(Enum_DunnageWorkflowStep.QuantityEntry);
        enterLoads
            .ShellActions.Next.Command.Should()
            .BeSameAs(enterLoads.Hosts.QuantityEntry.GoNextCommand);

        var review = await CreatePageAsync(Enum_DunnageWorkflowStep.Review);
        review.ShellActions.Next.Command.Should().BeSameAs(review.Hosts.Review.SaveAllCommand);

        var editMode = await CreatePageAsync(Enum_DunnageWorkflowStep.EditMode);
        editMode
            .ShellActions.Next.Command.Should()
            .BeSameAs(editMode.Hosts.EditMode.SaveAllCommand);
    }

    [Fact]
    public async Task NavigationShortcuts_ShouldUseTheSameCommandsAsTheStrip_SoAHiddenOrDisabledActionCannotBeReached()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.PartSelection);

        page.ShellActions.Next.IsEnabled.Should().BeFalse();
        page
            .ShellActions.Next.Command!.CanExecute(null)
            .Should()
            .BeFalse(
                "a keyboard route that runs the strip's own command instance meets the same gate"
            );

        var editMode = await CreatePageAsync(Enum_DunnageWorkflowStep.EditMode);
        editMode.ShellActions.Back.IsVisible.Should().BeFalse();
        editMode
            .ShellActions.Back.Command.Should()
            .BeNull("a hidden Back has no command for an accelerator to run");
    }

    [Fact]
    public async Task ShellActions_ShouldMarkOnlyTheModeInForce_WithSuccessTextAndBorder()
    {
        foreach (var step in Matrix.Keys)
        {
            var page = await CreatePageAsync(step);

            var activeChips = new[] { page.ShellActions.Guided, page.ShellActions.Edit, page.ShellActions.ImageSearch }
                .Count(chip => chip.IsActive);

            activeChips.Should().Be(1, $"{step} is in exactly one mode");
        }
    }

    [Fact]
    public async Task ShellActions_ShouldMarkImageSearchAsTheActiveMode_WhenThePageIsImageSearch()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.ImagePartSearch);

        page.ShellActions.ImageSearch.IsActive.Should().BeTrue();
        page.ShellActions.Guided.IsActive.Should().BeFalse();
        page.ShellActions.Edit.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldRaisePropertyChangedAndReplaceTheWholeSnapshot_WhenTheStepChanges()
    {
        var currentStep = Enum_DunnageWorkflowStep.TypeSelection;
        var workflow = CreateWorkflowMock(() => currentStep);
        var page = await CreatePageAsync(workflow);

        var raised = new List<string?>();
        page.ViewModel.PropertyChanged += (_, args) => raised.Add(args.PropertyName);
        var before = page.ViewModel.ShellActions;

        currentStep = Enum_DunnageWorkflowStep.DetailsEntry;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            EventArgs.Empty
        );

        raised.Should().Contain(nameof(ViewModel_Dunnage_WorkFlowViewModel.ShellActions));
        page.ViewModel.ShellActions.Should().NotBeSameAs(before);
        page.ViewModel.ShellActions.Next.Label.Should().Be("Review");
    }

    [Fact]
    public async Task EnterEditModeCommand_ShouldAskTheSameLeaveConfirmation_AndStayPut_WhenRefused()
    {
        var workflow = CreateWorkflowMock(() => Enum_DunnageWorkflowStep.TypeSelection);
        var page = await CreatePageAsync(workflow);
        var asked = 0;
        page.ViewModel.RequestConfirmLeaveModule = _ =>
        {
            asked++;
            return Task.FromResult(false);
        };

        await page.ViewModel.EnterEditModeCommand.ExecuteAsync(null);

        asked.Should().Be(1, "a mode change asks through the page's leave confirmation");
        workflow.Verify(
            service => service.GoToStep(Enum_DunnageWorkflowStep.EditMode),
            Times.Never
        );
        page.ViewModel.ShellActions.Guided.IsActive.Should().BeTrue("the page did not move");
    }

    [Fact]
    public async Task EnterEditModeCommand_ShouldMoveOnceTheConfirmationIsAccepted()
    {
        var workflow = CreateWorkflowMock(() => Enum_DunnageWorkflowStep.TypeSelection);
        var page = await CreatePageAsync(workflow);
        page.ViewModel.RequestConfirmLeaveModule = _ => Task.FromResult(true);

        await page.ViewModel.EnterEditModeCommand.ExecuteAsync(null);

        workflow.Verify(service => service.GoToStep(Enum_DunnageWorkflowStep.EditMode), Times.Once);
    }

    [Fact]
    public async Task EnterImageSearchBack_ShouldReturnToTheStepTheOperatorCameFrom()
    {
        var currentStep = Enum_DunnageWorkflowStep.QuantityEntry;
        var workflow = CreateWorkflowMock(() => currentStep);
        var page = await CreatePageAsync(workflow);
        page.ViewModel.RequestConfirmLeaveModule = _ => Task.FromResult(true);

        await page.ViewModel.EnterImageSearchModeCommand.ExecuteAsync(null);

        currentStep = Enum_DunnageWorkflowStep.ImagePartSearch;
        page.ViewModel.PreviousStepCommand.Execute(null);

        workflow.Verify(
            service => service.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task ClearLabelDataAndHelpSlots_ShouldKeepAScreenReaderNameAndATooltip_WhenIconOnly()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.Review);

        page.ShellActions.ClearLabelData.AccessibilityName.Should().Be("Clear All Labels");
        page.ShellActions.ClearLabelData.Tooltip.Should().Be("Clear All Labels");
        page.ShellActions.Help.AccessibilityName.Should().Be("Help");
        page.ShellActions.Help.Tooltip.Should().Be("Help");
    }

    [Fact]
    public async Task ShellActions_ShouldMarkTheSaveVerbAsSave_AndBackAsBack()
    {
        var review = await CreatePageAsync(Enum_DunnageWorkflowStep.Review);
        review.ShellActions.Next.Kind.Should().Be(Enum_ShellActionKind.Save);
        review.ShellActions.Back.Kind.Should().Be(Enum_ShellActionKind.Back);

        var editMode = await CreatePageAsync(Enum_DunnageWorkflowStep.EditMode);
        editMode.ShellActions.Next.Kind.Should().Be(Enum_ShellActionKind.Save);

        var details = await CreatePageAsync(Enum_DunnageWorkflowStep.DetailsEntry);
        details
            .ShellActions.Next.Kind.Should()
            .Be(
                Enum_ShellActionKind.Plain,
                "Review moves to the next page rather than writing the session"
            );
    }

    [Fact]
    public void MainWindow_ShouldNameNoDunnageTypeForTheseActions()
    {
        var root = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")
        );
        var shellWiring = Path.Combine(root, "MainWindow.xaml.cs");
        var searchWiring = Path.Combine(root, "MainWindow.Search.cs");

        foreach (var file in new[] { shellWiring, searchWiring })
        {
            File.Exists(file).Should().BeTrue($"{file} must exist");

            var text = File.ReadAllText(file);
            text.Should()
                .NotContain("ViewModel_Dunnage", "the shell never names a module view model");
            text.Should()
                .NotContain("Enum_DunnageWorkflowStep", "the shell never names a module step");
            text.Should()
                .NotContain(
                    "\"image-search\" =>",
                    "a per-mode switch in the window is exactly what the projection replaced"
                );
        }

        var shellText = File.ReadAllText(shellWiring);
        shellText.Should().Contain("ShellActionBar.EnterMode(");
        shellText.Should().Contain("ShellActionBar.RememberModeAsDefault(");
        shellText
            .Should()
            .Contain(
                "ShellActionBar.Detach()",
                "a page outside both modules supplies no provider, so the strip must collapse"
            );
    }

    private static string[] Offered(Model_ShellActionBarState shell)
    {
        var offered = new List<string>();

        if (shell.ShowModeGroup)
        {
            foreach (
                var chip in new[] { shell.Guided, shell.Manual, shell.Edit, shell.Overlay, shell.ImageSearch }
            )
            {
                if (chip.IsVisible)
                {
                    offered.Add(chip.Label);
                }
            }
        }

        if (shell.Back.IsVisible)
        {
            offered.Add(shell.Back.Label);
        }

        if (shell.Next.IsVisible)
        {
            offered.Add(shell.Next.Label);
        }

        if (shell.ClearLabelData.IsVisible)
        {
            offered.Add(shell.ClearLabelData.Label);
        }

        if (shell.Help.IsVisible)
        {
            offered.Add(shell.Help.Label);
        }

        return offered.ToArray();
    }

    private sealed record Page(
        ViewModel_Dunnage_WorkFlowViewModel ViewModel,
        ViewModel_Shared_ShellActionBar Bar,
        Hosts Hosts
    )
    {
        public Model_ShellActionBarState ShellActions => ViewModel.ShellActions;

        public string[] Offered => ViewModel_Dunnage_ShellActionTests.Offered(ShellActions);
    }

    private sealed record Hosts(
        ViewModel_Dunnage_PartSelection PartSelection,
        ViewModel_Dunnage_QuantityEntry QuantityEntry,
        ViewModel_Dunnage_DetailsEntry DetailsEntry,
        ViewModel_Dunnage_Review Review,
        ViewModel_Dunnage_EditMode EditMode
    );

    private static Task<Page> CreatePageAsync(
        Enum_DunnageWorkflowStep step,
        Action<Mock<IService_DunnageWorkflow>>? configureWorkflow = null,
        Mock<IService_Help>? helpService = null
    ) => CreatePageAsync(CreateWorkflowMock(() => step, configureWorkflow), helpService);

    private static async Task<Page> CreatePageAsync(
        Mock<IService_DunnageWorkflow> workflow,
        Mock<IService_Help>? helpService = null
    )
    {
        var viewModel = new ViewModel_Dunnage_WorkFlowViewModel(
            workflow.Object,
            CreateSettingsMock().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Notification>().Object,
            CreateDispatcherMock().Object,
            new Mock<IService_UserPreferences>().Object,
            helpService?.Object ?? new Mock<IService_Help>().Object,
            new Mock<IService_UserSessionManager>().Object
        );

        var hosts = new Hosts(
            new ViewModel_Dunnage_PartSelection(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                CreateDispatcherMock().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_UserPrivileges>().Object,
                new Mock<IService_UserSessionManager>().Object,
                CreateSettingsMock().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_QuantityEntry(
                workflow.Object,
                CreateDispatcherMock().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_DetailsEntry(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                CreateDispatcherMock().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ReceivingValidation>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_SettingsCoreFacade>().Object,
                new Mock<IService_UserSessionManager>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_Review(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_Window>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_EditMode(
                new Mock<IService_MySQL_Dunnage>().Object,
                new Mock<IService_Pagination>().Object,
                workflow.Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_Window>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            )
        );

        viewModel.AttachShellActionHosts(
            hosts.PartSelection,
            hosts.QuantityEntry,
            hosts.DetailsEntry,
            hosts.Review,
            hosts.EditMode
        );

        await Task.Delay(25);

        var bar = new ViewModel_Shared_ShellActionBar();
        bar.Attach(viewModel);

        return new Page(viewModel, bar, hosts);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MainWindow.xaml")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find MainWindow.xaml walking up from {AppContext.BaseDirectory}"
        );
    }

    private static Mock<IService_DunnageWorkflow> CreateWorkflowMock(
        Func<Enum_DunnageWorkflowStep> currentStep,
        Action<Mock<IService_DunnageWorkflow>>? configure = null
    )
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        var session = new Model_DunnageSession();
        workflow.SetupGet(service => service.CurrentStep).Returns(currentStep);
        workflow.SetupGet(service => service.CurrentSession).Returns(session);
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);
        workflow.Setup(service => service.HasUnsavedData()).Returns(false);

        configure?.Invoke(workflow);

        return workflow;
    }

    private static Mock<IService_DunnageSettings> CreateSettingsMock()
    {
        var settings = new Mock<IService_DunnageSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    DunnageSettingsDefaults.StringDefaults.TryGetValue(key, out var value)
                        ? value
                        : string.Empty
            );

        return settings;
    }

    private static Mock<IService_Dispatcher> CreateDispatcherMock()
    {
        var dispatcher = new Mock<IService_Dispatcher>();
        dispatcher
            .Setup(service => service.TryEnqueue(It.IsAny<Action>()))
            .Returns<Action>(callback =>
            {
                callback();
                return true;
            });

        return dispatcher;
    }
}
