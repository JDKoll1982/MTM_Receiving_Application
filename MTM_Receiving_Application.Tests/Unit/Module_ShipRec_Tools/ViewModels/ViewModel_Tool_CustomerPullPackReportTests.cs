using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
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
        viewModel.CrystalReportGroups.Should().HaveCount(2);
        viewModel.CrystalReportGroups[0].ParentPartId.Should().Be("PART-100");
    }

    [Fact]
    public async void RefreshReportAsync_ShouldProjectCrystalReportGroups_FromLiveDemandLines()
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
                            ShipQuantity = 24,
                            FgOnHandQuantity = 20,
                            FgLocationId = "FG-01",
                            ShortageFlag = true,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-100",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            FgOnHandQuantity = 20,
                            FgLocationId = "FG-01",
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1B",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 15,
                                },
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-02",
                                    DisplayLabel = "SUB-02",
                                    OnHandQuantity = 8,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel.CrystalReportGroups.Should().ContainSingle();
        var group = viewModel.CrystalReportGroups[0];
        group.ParentPartId.Should().Be("PART-100");
        group.QuantityToPack.Should().Be(34);
        group.QuantitySelected.Should().Be(0);
        group.RequestLines.Should().HaveCount(2);
        group.SubPartLocations.Should().HaveCount(2);
        group
            .SubPartLocations.Single(location => location.LocationId == "SUB-01")
            .OnHandQuantity.Should()
            .Be(15);
    }

    [Fact]
    public async void ApplyCrystalRequestLineSelection_ShouldUpdateSingleDemandRowAndProjection()
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
                            ShipQuantity = 24,
                            LocationOptions = [],
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-100",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            LocationOptions = [],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-2");

        viewModel
            .DemandLines.Single(line => line.SourceLineKey == "LINE-2")
            .IsSelected.Should()
            .BeTrue();
        viewModel
            .DemandLines.Single(line => line.SourceLineKey == "LINE-1")
            .IsSelected.Should()
            .BeFalse();
        viewModel.SelectedDemandLine.Should().NotBeNull();
        viewModel.SelectedDemandLine!.SourceLineKey.Should().Be("LINE-2");
        viewModel.CrystalReportGroups[0].QuantitySelected.Should().Be(0);
        viewModel
            .CrystalReportGroups[0]
            .RequestLines.Single(line => line.SourceLineKey == "LINE-2")
            .IsSelected.Should()
            .BeTrue();
    }

    [Fact]
    public async void ApplyCrystalLocationSelection_ShouldUpdateLocationSelectionsAcrossParentPart()
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-100",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 8,
                                },
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-3",
                                    LocationId = "SUB-02",
                                    DisplayLabel = "SUB-02",
                                    OnHandQuantity = 5,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        viewModel
            .DemandLines.SelectMany(line =>
                line.LocationOptions.Where(option => option.LocationId == "SUB-01")
            )
            .Should()
            .OnlyContain(option => option.Selected);
        viewModel
            .DemandLines.SelectMany(line =>
                line.LocationOptions.Where(option => option.LocationId == "SUB-02")
            )
            .Should()
            .OnlyContain(option => !option.Selected);
        viewModel
            .CrystalReportGroups[0]
            .SubPartLocations.Single(location => location.LocationId == "SUB-01")
            .IsSelected.Should()
            .BeTrue();
    }

    [Fact]
    public async void ApplyCrystalLocationSelection_ShouldIgnoreSelections_WhenNoRequestLineIsSelected()
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        viewModel.DemandLines[0].IsSelected.Should().BeFalse();
        viewModel
            .DemandLines[0]
            .LocationOptions.Single(option => option.LocationId == "SUB-01")
            .Selected.Should()
            .BeFalse();
        viewModel.CrystalReportGroups[0].QuantitySelected.Should().Be(0);
        viewModel.CrystalReportGroups[0].HasSelectedRequestLine.Should().BeFalse();
    }

    [Fact]
    public async void ApplyCrystalRequestLineSelection_ShouldClearLocationSelections_WhenSwitchingToDifferentRow()
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-100",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 8,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        viewModel.ApplyCrystalRequestLineSelection("LINE-2");

        viewModel.SelectedDemandLine.Should().NotBeNull();
        viewModel.SelectedDemandLine!.SourceLineKey.Should().Be("LINE-2");
        viewModel
            .DemandLines.SelectMany(static line => line.LocationOptions)
            .Should()
            .OnlyContain(option => !option.Selected);
        viewModel.CrystalReportGroups[0].QuantitySelected.Should().Be(0);
    }

    [Fact]
    public async void ApplyCrystalRequestLineSelection_ShouldClearLocationSelections_WhenSelectionIsRemoved()
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        viewModel.ApplyCrystalRequestLineSelection(string.Empty);

        viewModel.SelectedDemandLine.Should().BeNull();
        viewModel.DemandLines.Should().OnlyContain(static line => !line.IsSelected);
        viewModel
            .DemandLines.SelectMany(static line => line.LocationOptions)
            .Should()
            .OnlyContain(option => !option.Selected);
        viewModel.CrystalReportGroups[0].QuantitySelected.Should().Be(0);
        viewModel.CrystalReportGroups[0].HasSelectedRequestLine.Should().BeFalse();
    }

    [Fact]
    public async void CreateOrUpdateWaitlistAsync_ShouldPreserveSelectedLocations_ForNewRequest()
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                    }
                )
            );

        ViewModel_Dialog_CustomerPullPackWaitlistEditor? capturedDialogViewModel = null;
        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";
        viewModel.ShowWaitlistEditorAsync = dialogViewModel =>
        {
            capturedDialogViewModel = dialogViewModel;
            return Task.FromResult(false);
        };

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        await viewModel.CreateOrUpdateWaitlistCommand.ExecuteAsync(null);

        capturedDialogViewModel.Should().NotBeNull();
        capturedDialogViewModel!
            .SelectedLine.LocationOptions.Single(option => option.LocationId == "SUB-01")
            .Selected.Should()
            .BeTrue();
    }

    [Fact]
    public async void CreateOrUpdateWaitlistAsync_ShouldPrompt_WhenSelectedQuantityExceedsTwentyPercentOverPack()
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
                            QuantityToPack = 100,
                            ShipQuantity = 100,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 70,
                                },
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-02",
                                    DisplayLabel = "SUB-02",
                                    OnHandQuantity = 60,
                                },
                            ],
                        },
                    }
                )
            );

        ViewModel_Dialog_CustomerPullPackWaitlistEditor? capturedDialogViewModel = null;
        decimal promptedSelectedQuantity = 0;
        decimal promptedQuantityToPack = 0;
        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";
        viewModel.ShowOverSelectedQuantityConfirmationAsync = (selectedQuantity, quantityToPack) =>
        {
            promptedSelectedQuantity = selectedQuantity;
            promptedQuantityToPack = quantityToPack;
            return Task.FromResult(true);
        };
        viewModel.ShowWaitlistEditorAsync = dialogViewModel =>
        {
            capturedDialogViewModel = dialogViewModel;
            return Task.FromResult(false);
        };

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01", "SUB-02"]);

        await viewModel.CreateOrUpdateWaitlistCommand.ExecuteAsync(null);

        promptedSelectedQuantity.Should().Be(130);
        promptedQuantityToPack.Should().Be(100);
        capturedDialogViewModel.Should().NotBeNull();
    }

    [Fact]
    public async void CreateOrUpdateWaitlistAsync_ShouldStop_WhenOverSelectedQuantityPromptIsRejected()
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
                            QuantityToPack = 100,
                            ShipQuantity = 100,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 70,
                                },
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-02",
                                    DisplayLabel = "SUB-02",
                                    OnHandQuantity = 60,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";
        viewModel.ShowOverSelectedQuantityConfirmationAsync = (_, _) => Task.FromResult(false);
        viewModel.ShowWaitlistEditorAsync = _ =>
            throw new InvalidOperationException(
                "Editor should not open when user cancels overage prompt."
            );

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01", "SUB-02"]);

        await viewModel.CreateOrUpdateWaitlistCommand.ExecuteAsync(null);

        viewModel.StatusMessage.Should().Contain("canceled");
    }

    [Fact]
    public async void ApplyCrystalLocationSelection_ShouldClearSelectionsOutsideActiveParentPart()
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
                            ShipQuantity = 24,
                            IsSelected = true,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                    Selected = true,
                                },
                            ],
                        },
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-200",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            IsSelected = true,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-2",
                                    LocationId = "SUB-09",
                                    DisplayLabel = "SUB-09",
                                    OnHandQuantity = 8,
                                    Selected = true,
                                },
                            ],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        viewModel
            .DemandLines.Single(line => line.ParentPartId == "PART-200")
            .IsSelected.Should()
            .BeFalse();
        viewModel
            .DemandLines.Single(line => line.ParentPartId == "PART-200")
            .LocationOptions.Should()
            .OnlyContain(option => !option.Selected);
    }

    [Fact]
    public async void RefreshReportAsync_ShouldProjectWaitlistNote_WhenDemandLineRequiresRecheck()
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
                            ShipQuantity = 24,
                            RecheckIndicator = true,
                            HasLinkedWaitlist = true,
                            WaitlistStateDisplay = "Completed (WL-1)",
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel.CrystalReportGroups.Should().ContainSingle();
        viewModel.CrystalReportGroups[0].RequestLines.Should().ContainSingle();
        viewModel.CrystalReportGroups[0].RequestLines[0].StatusNoteText.Should().Be("Waitlist");
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
    public async void RefreshReportAsync_ShouldWarnWhenRefreshClearsActiveSelections()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .SetupSequence(mediator =>
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
                            ShipQuantity = 24,
                            LocationOptions =
                            [
                                new Model_CustomerPullPack_LocationOption
                                {
                                    LocationKey = "LOC-1",
                                    LocationId = "SUB-01",
                                    DisplayLabel = "SUB-01",
                                    OnHandQuantity = 12,
                                },
                            ],
                        },
                    }
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomerPullPack_DemandLine>
                    {
                        new()
                        {
                            SourceLineKey = "LINE-2",
                            CustomerId = "VOLVO",
                            CustomerName = "Volvo Group",
                            CustomerOrderId = "CO-1002",
                            ParentPartId = "PART-200",
                            PullDate = new DateTime(2026, 5, 28),
                            QuantityToPack = 10,
                            ShipQuantity = 10,
                            LocationOptions = [],
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);
        viewModel.CustomerSearchText = "VOLVO - Volvo Group";

        await viewModel.RefreshReportCommand.ExecuteAsync(null);
        viewModel.ApplyCrystalRequestLineSelection("LINE-1");
        viewModel.ApplyCrystalLocationSelection("PART-100", ["SUB-01"]);

        await viewModel.RefreshReportCommand.ExecuteAsync(null);

        viewModel
            .StatusMessage.Should()
            .Contain("Previous line and location selections were cleared.");
        viewModel.DemandLines.Should().OnlyContain(line => !line.IsSelected);
        viewModel
            .DemandLines.SelectMany(line => line.LocationOptions)
            .Should()
            .OnlyContain(option => !option.Selected);
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

    [Fact]
    public async void LoadUserDefaultsAsync_ShouldSeedMockFavoritesAndCustomer_WhenMockModeEnabledWithoutSavedDefaults()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackDefaults>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_UserDefaults>("No defaults")
            );

        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);

        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetCustomerPullPackDemandRows())
            .Returns([
                new Model_InforVisualCustomerPullPackDemandRow
                {
                    CustomerId = "VOLVO",
                    CustomerName = "Volvo Trucks",
                },
                new Model_InforVisualCustomerPullPackDemandRow
                {
                    CustomerId = "MACK",
                    CustomerName = "Mack Trucks",
                },
            ]);

        var viewModel = CreateViewModel(mediatorMock, appSettingsMock.Object, mockCatalog.Object);

        await viewModel.LoadUserDefaultsAsync();

        viewModel.FavoriteCustomerIds.Should().Contain("VOLVO - Volvo Trucks");
        viewModel.FavoriteCustomerIds.Should().Contain("MACK - Mack Trucks");
        viewModel.CustomerSearchText.Should().Be("VOLVO - Volvo Trucks");
    }

    [Fact]
    public async void LoadUserDefaultsAsync_ShouldResolveDefaultCustomerToMockDisplay_WhenMockModeEnabled()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<Query_CustomerPullPackDefaults>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_UserDefaults
                    {
                        UserId = "jkoll",
                        DefaultCustomerId = "VOLVO",
                        FavoriteCustomerIds = [],
                    }
                )
            );

        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);

        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetCustomerPullPackDemandRows())
            .Returns([
                new Model_InforVisualCustomerPullPackDemandRow
                {
                    CustomerId = "VOLVO",
                    CustomerName = "Volvo Trucks",
                },
            ]);

        var viewModel = CreateViewModel(mediatorMock, appSettingsMock.Object, mockCatalog.Object);

        await viewModel.LoadUserDefaultsAsync();

        viewModel.CustomerSearchText.Should().Be("VOLVO - Volvo Trucks");
        viewModel.FavoriteCustomerIds.Should().Contain("VOLVO - Volvo Trucks");
    }

    private static ViewModel_Tool_CustomerPullPackReport CreateViewModel(
        Mock<IMediator>? mediatorMock = null,
        IService_AppSettings? appSettings = null,
        IService_InforVisualMockDataCatalog? mockDataCatalog = null,
        IService_Notification? notificationService = null
    )
    {
        return new ViewModel_Tool_CustomerPullPackReport(
            (mediatorMock ?? new Mock<IMediator>()).Object,
            appSettings ?? CreateAppSettings(),
            mockDataCatalog ?? CreateMockDataCatalog(),
            CreateSessionManager(),
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            notificationService ?? new Mock<IService_Notification>().Object
        );
    }

    private static IService_AppSettings CreateAppSettings()
    {
        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(false);
        return appSettingsMock.Object;
    }

    private static IService_InforVisualMockDataCatalog CreateMockDataCatalog()
    {
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog.Setup(service => service.GetCustomerPullPackDemandRows()).Returns([]);
        return mockCatalog.Object;
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
