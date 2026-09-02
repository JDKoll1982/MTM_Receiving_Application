using System.ComponentModel;
using System.Threading;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;
using MTM_Receiving_Application.Module_Shared.Enums;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.ViewModels;

public sealed class ViewModel_Scanner_WorkbenchTests
{
    // ── ScannerUpdate.md Task 6: baseline input states ───────────────────────────

    [Fact]
    public void BaselineInputStates_ShouldMatchScannerWorkflow()
    {
        var viewModel = CreateWorkbenchViewModel();

        viewModel.IsPartEditable.Should().BeTrue();
        viewModel.HasSelectedSessionItem.Should().BeFalse();
        viewModel.IsSendEnabled.Should().BeFalse();
    }

    // ── Per-row To/Qty validation (editable table columns) ──────────────────────

    private static Mock<IService_ScannerWorkflow> SetupUpsertWorkflow(
        Model_ScannerBatchSession session
    )
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        workflow
            .Setup(service =>
                service.UpsertBatchItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Model_ScannerBatchSession current, Model_ScannerBatchItem item, CancellationToken _) =>
            {
                var index = current.Items.ToList().FindIndex(candidate => candidate.ItemId == item.ItemId);
                if (index >= 0)
                {
                    current.Items[index] = item;
                }
                else
                {
                    item.SequenceNumber = current.Items.Count + 1;
                    current.Items.Add(item);
                }

                current.RecalculateItemCounters();
                return Model_Dao_Result_Factory.Success(current);
            });
        return workflow;
    }

    [Fact]
    public async Task ValidateSessionItemAsync_ShouldSanitizeToAndMarkValid()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "VA-A001",
            PayloadQuantity = "25",
        };
        session.Items.Add(item);

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult
                    {
                        State = Enum_ScannerValidationState.Valid,
                        CanonicalToLocation = "VA-A0-01",
                    }
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [item];
        viewModel.SelectedSessionItem = item;

        var result = await viewModel.ValidateSessionItemAsync(item);

        result.Should().NotBeNull();
        item.PayloadToLocation.Should().Be("VA-A0-01");
        item.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
        viewModel.IsSendEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSessionItemAsync_ShouldMarkInvalid_WhenDestinationEmpty()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadQuantity = "25",
        };
        session.Items.Add(item);

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult
                    {
                        State = Enum_ScannerValidationState.Invalid,
                    }
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [item];
        viewModel.SelectedSessionItem = item;

        await viewModel.ValidateSessionItemAsync(item);

        item.ValidationState.Should().Be(Enum_ScannerValidationState.Invalid);
        viewModel.IsSendEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateSessionItemAsync_ShouldNotReplaceSessionItems()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "V-A0-01",
            PayloadQuantity = "25",
        };
        session.Items.Add(item);

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult { State = Enum_ScannerValidationState.Valid }
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [item];
        viewModel.SelectedSessionItem = item;
        var sessionItemsBefore = viewModel.SessionItems;

        await viewModel.ValidateSessionItemAsync(item);

        // Regression guard: validation must NOT rebuild SessionItems, which destroyed the
        // ListViewItem containers and made the operator lose focus while tabbing.
        viewModel.SessionItems.Should().BeSameAs(sessionItemsBefore);
        viewModel.SelectedSessionItem.Should().BeSameAs(item);
        item.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
    }

    [Fact]
    public async Task ValidateSessionItemAsync_ShouldMarkQuantityTooHigh_WhenOverOnHand()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "V-A0-01",
            PayloadQuantity = "30",
            MaxQuantity = 25m,
        };
        session.Items.Add(item);

        var viewModel = CreateWorkbenchViewModel(
            SetupUpsertWorkflow(session).Object,
            new Mock<IService_ScannerValidation>().Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [item];
        viewModel.SelectedSessionItem = item;

        await viewModel.ValidateSessionItemAsync(item);

        item.ValidationState.Should().Be(Enum_ScannerValidationState.Invalid);
        item.StatusText.Should().Be("! Qty too High");
        item.StatusIsValid.Should().BeFalse();
        viewModel.IsSendEnabled.Should().BeFalse();
    }

    // ── ScannerUpdate.md Step 6b-a2: no-stock header error + part refocus ───────

    [Fact]
    public async Task PartValidationCompletedAsync_WithNoStock_ShouldShowHeaderErrorAndRefocusPart()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>([])
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        var partFocusRaised = false;
        viewModel.PartIdFocusRequested += () => partFocusRaised = true;

        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();
        viewModel.HeaderErrorText.Should().Contain("does not have any quantity");
        partFocusRaised.Should().BeTrue();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WithStock_ShouldPopulateListImmediately()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var workflow = SetupUpsertWorkflow(session);
        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [
                        new Model_InforVisualMaterialLocationRow
                        {
                            LocationId = "V-A1-01",
                            WarehouseCode = "002",
                            Quantity = 25m,
                        },
                    ]
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>(
                [new Model_ScannerStockPick { Location = "V-A1-01", Quantity = "25", OnHand = 25m }]
            );

        await viewModel.EnsureCurrentSessionAsync();
        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        // The list populates immediately after the modal (no staging / Add step).
        viewModel.SessionItems.Should().HaveCount(1);
        var line = viewModel.SessionItems[0];
        line.PayloadFromLocation.Should().Be("V-A1-01");
        line.PayloadQuantity.Should().Be("25");
        line.PayloadToLocation.Should().BeEmpty();
        line.MaxQuantity.Should().Be(25m);
        line.ValidationState.Should().Be(Enum_ScannerValidationState.NotValidated);
        viewModel.NewPartId.Should().BeEmpty();
        viewModel.IsHeaderErrorVisible.Should().BeFalse();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WhenModalCancelled_ShouldClearPartAndClearRequested()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [new Model_InforVisualMaterialLocationRow { LocationId = "V-A1-01", Quantity = 25m }]
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>([]);
        var clearRaised = false;
        viewModel.PartIdClearRequested += () => clearRaised = true;

        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        // Step 6b-a1-b: cancel must clear the part input so the shared lookup cannot
        // re-validate and reopen the modal, and must not add any lines.
        clearRaised.Should().BeTrue();
        viewModel.NewPartId.Should().BeEmpty();
        viewModel.SessionItems.Should().BeEmpty();
        viewModel.IsHeaderErrorVisible.Should().BeFalse();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WithMultiSelectPicks_ShouldPopulateAllTransactionLines()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var workflow = SetupUpsertWorkflow(session);
        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [
                        new Model_InforVisualMaterialLocationRow { LocationId = "V-A1-01", Quantity = 25m },
                        new Model_InforVisualMaterialLocationRow { LocationId = "V-F0-02", Quantity = 10m },
                    ]
                )
            );
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult { State = Enum_ScannerValidationState.Valid }
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>(
                [
                    new Model_ScannerStockPick { Location = "V-A1-01", Quantity = "25", TransactionCount = 1 },
                    new Model_ScannerStockPick { Location = "V-F0-02", Quantity = "10", TransactionCount = 2 },
                ]
            );

        await viewModel.EnsureCurrentSessionAsync();
        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        // 1 line from the first pick + 2 lines (qty 1 each) from the second pick,
        // all populated immediately after the modal with an empty destination.
        viewModel.SessionItems.Should().HaveCount(3);
        viewModel.SessionItems[0].PayloadFromLocation.Should().Be("V-A1-01");
        viewModel.SessionItems[0].PayloadQuantity.Should().Be("25");
        viewModel.SessionItems[0].PayloadToLocation.Should().BeEmpty();
        viewModel.SessionItems[1].PayloadFromLocation.Should().Be("V-F0-02");
        viewModel.SessionItems[1].PayloadQuantity.Should().Be("1");
        viewModel.SessionItems[2].PayloadFromLocation.Should().Be("V-F0-02");
        viewModel.SessionItems[2].PayloadQuantity.Should().Be("1");
        viewModel.NewPartId.Should().BeEmpty();
    }

    [Fact]
    public async Task LocationValidationCompletedAsync_WithParts_ShouldPopulateRows()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var workflow = SetupUpsertWorkflow(session);
        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateLocationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerLocationValidationResult { IsValid = true, CanonicalLocation = "V-F0-01" }
                )
            );
        validation
            .Setup(service =>
                service.GetPartsAtLocationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [
                        new Model_InforVisualMaterialLocationRow
                        {
                            PartId = "MMC0000650",
                            LocationId = "V-F0-01",
                            Quantity = 25m,
                        },
                    ]
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.LocationPartsPickerRequested += (_, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>(
                [new Model_ScannerStockPick { PartId = "MMC0000650", OnHand = 25m, Quantity = "25" }]
            );

        await viewModel.EnsureCurrentSessionAsync();
        await viewModel.LocationValidationCompletedAsync("VF0-01");

        // Location search populates rows with the picked parts and the entered source location.
        viewModel.SessionItems.Should().HaveCount(1);
        var line = viewModel.SessionItems[0];
        line.PayloadPartId.Should().Be("MMC0000650");
        line.PayloadFromLocation.Should().Be("V-F0-01");
        line.MaxQuantity.Should().Be(25m);
        line.ValidationState.Should().Be(Enum_ScannerValidationState.NotValidated);
        viewModel.NewPartId.Should().BeEmpty();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WhenPartMissing_ShouldShowHeaderError()
    {
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));

        var viewModel = CreateWorkbenchViewModel(validation: validation.Object);

        await viewModel.PartValidationCompletedAsync("NOPE");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();
    }

    // ── Header error (non-blocking, clearable) ──────────────────────────────────

    [Fact]
    public void ClearHeaderError_ShouldHideImmediately()
    {
        var viewModel = CreateWorkbenchViewModel();
        viewModel.ShowHeaderError("Some problem");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();

        viewModel.ClearHeaderError();

        viewModel.IsHeaderErrorVisible.Should().BeFalse();
        viewModel.HeaderErrorText.Should().BeEmpty();
    }

    // ── ScannerUpdate.md Task 1/2b: input lockout during automation ─────────────

    [Fact]
    public void InputLockout_ShouldDisableInputs_WhenAutomationRuns()
    {
        var execution = new Mock<IService_ScannerExecution>();
        var viewModel = CreateWorkbenchViewModel(execution: execution.Object);

        execution.SetupGet(service => service.IsAutomationRunning).Returns(true);
        execution.Raise(
            service => service.PropertyChanged += null,
            new PropertyChangedEventArgs(nameof(IService_ScannerExecution.IsAutomationRunning))
        );

        viewModel.IsPartEditable.Should().BeFalse();
        viewModel.IsSendEnabled.Should().BeFalse();
    }

    // ── Send gating: only a validated selected line enables Send ────────────────

    [Fact]
    public void IsSendEnabled_ShouldBeTrueOnly_WhenSelectedLineIsValid()
    {
        var viewModel = CreateWorkbenchViewModel();

        var item = new Model_ScannerBatchItem { ValidationState = Enum_ScannerValidationState.Invalid };
        viewModel.SelectedSessionItem = item;
        viewModel.IsSendEnabled.Should().BeFalse();

        item.ValidationState = Enum_ScannerValidationState.Valid;
        viewModel.IsSendEnabled.Should().BeTrue();

        viewModel.SelectedSessionItem = null;
        viewModel.IsSendEnabled.Should().BeFalse();
    }

    [Fact]
    public void SendButtonText_ShouldAlwaysBeSend()
    {
        var viewModel = CreateWorkbenchViewModel();

        var item = new Model_ScannerBatchItem { ValidationState = Enum_ScannerValidationState.Invalid };
        viewModel.SelectedSessionItem = item;
        viewModel.SendButtonText.Should().Be("Send");

        item.ValidationState = Enum_ScannerValidationState.Valid;
        viewModel.SendButtonText.Should().Be("Send");

        viewModel.SelectedSessionItem = null;
        viewModel.SendButtonText.Should().Be("Send");
    }

    [Fact]
    public void SearchMode_ShouldSwitchLookupTypeLabelPlaceholder_AndClearPart()
    {
        var viewModel = CreateWorkbenchViewModel();
        viewModel.NewPartId = "MMCCS00740";

        viewModel.LookupType.Should().Be(Enum_SharedLookupType.PartNumber);
        viewModel.LookupHeaderText.Should().Be("Part");
        viewModel.LookupPlaceholderText.Should().Be("Enter part number");

        viewModel.IsLocationModeEnabled = true;

        viewModel.SearchMode.Should().Be(Enum_ScannerSearchMode.Location);
        viewModel.LookupType.Should().Be(Enum_SharedLookupType.Location);
        viewModel.LookupHeaderText.Should().Be("Location");
        viewModel.LookupPlaceholderText.Should().Be("Enter location");
        viewModel.NewPartId.Should().BeEmpty();
    }

    [Fact]
    public async Task SendSelectedCommand_ShouldSendTheSelectedLineOnly()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadQuantity = "5",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-05",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        session.Items.Add(item);

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service =>
                service.SendSpecificItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<Model_ScannerProfile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new Model_ScannerExecutionOutcome { SentCount = 1 }));

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.TransferSavedSinceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));

        var viewModel = CreateWorkbenchViewModel(
            new Mock<IService_ScannerWorkflow>().Object,
            validation.Object,
            execution.Object
        );
        viewModel.TransferConfirmTimeout = TimeSpan.FromMilliseconds(1);
        viewModel.TransferConfirmPollInterval = TimeSpan.FromMilliseconds(1);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.SelectedSessionItem = item;

        await viewModel.SendSelectedCommand.ExecuteAsync(null);

        execution.Verify(
            service => service.SendSpecificItemAsync(
                session,
                item,
                It.IsAny<Model_ScannerProfile>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendSelectedCommand_ShouldNotSend_WhenSelectedLineInvalid()
    {
        var execution = new Mock<IService_ScannerExecution>();
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadQuantity = "5",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-05",
            ValidationState = Enum_ScannerValidationState.Invalid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        session.Items.Add(item);

        var viewModel = CreateWorkbenchViewModel(execution: execution.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.SelectedSessionItem = item;

        await viewModel.SendSelectedCommand.ExecuteAsync(null);

        // Invalid line -> the "Validate" popup path (window root unavailable -> status);
        // the execution service must never be called.
        execution.Verify(
            service => service.SendSpecificItemAsync(
                It.IsAny<Model_ScannerBatchSession>(),
                It.IsAny<Model_ScannerBatchItem>(),
                It.IsAny<Model_ScannerProfile>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task OpenInventoryCommand_ShouldOpenTheInventoryWindow()
    {
        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service =>
                service.OpenInventoryWindowAsync(
                    It.IsAny<Model_ScannerProfile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateWorkbenchViewModel(execution: execution.Object);

        await viewModel.OpenInventoryCommand.ExecuteAsync(null);

        execution.Verify(
            service => service.OpenInventoryWindowAsync(
                It.IsAny<Model_ScannerProfile>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    // ── Clear List ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ClearListCommand_ShouldEmptyTheCurrentList()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        session.Items.Add(
            new Model_ScannerBatchItem
            {
                ItemId = Guid.NewGuid(),
                SessionId = session.SessionId,
                SequenceNumber = 1,
                PayloadPartId = "MMCCS00740",
            }
        );
        session.RecalculateItemCounters();

        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));
        workflow
            .Setup(service =>
                service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ScannerProfile>())
            );
        workflow
            .Setup(service =>
                service.ReplaceSessionItemsAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Model_ScannerBatchSession session, CancellationToken _) =>
                    Model_Dao_Result_Factory.Success(session)
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object);
        await viewModel.EnsureCurrentSessionAsync();

        viewModel.SessionItems.Should().HaveCount(1);

        await viewModel.ClearListCommand.ExecuteAsync(null);

        viewModel.SessionItems.Should().BeEmpty();
        viewModel.HasActiveSession.Should().BeTrue();
    }

    // ── Remove Selected ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveSelectedSessionItemCommand_ShouldRemoveSelectedAndPersist()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var first = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
        };
        var second = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "MMF0001",
        };
        session.Items.Add(first);
        session.Items.Add(second);
        session.RecalculateItemCounters();

        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));
        workflow
            .Setup(service =>
                service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ScannerProfile>())
            );
        workflow
            .Setup(service =>
                service.ReplaceSessionItemsAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Model_ScannerBatchSession s, CancellationToken _) =>
                    Model_Dao_Result_Factory.Success(s)
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object);
        await viewModel.EnsureCurrentSessionAsync();

        viewModel.SelectedSessionItem = first;
        var removedId = first.ItemId;

        await viewModel.RemoveSelectedSessionItemCommand.ExecuteAsync(null);

        viewModel.SessionItems.Should().HaveCount(1);
        viewModel.SessionItems.Should().NotContain(item => item.ItemId == removedId);
        viewModel.HasSelectedSessionItem.Should().BeTrue();
    }

    // ── Edge-case fixes: part-not-found message, duplicates, reentrancy, skip-valid ──

    [Fact]
    public async Task PartValidationCompletedAsync_WhenPartDoesNotExist_ShouldShowPartNotFoundMessage()
    {
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));

        var viewModel = CreateWorkbenchViewModel(validation: validation.Object);
        var partFocusRaised = false;
        viewModel.PartIdFocusRequested += () => partFocusRaised = true;

        await viewModel.PartValidationCompletedAsync("NOPE123");

        // "Part not found" is a distinct message from "no stock in-house", and the part box
        // regains focus so the operator can correct the number.
        viewModel.IsHeaderErrorVisible.Should().BeTrue();
        viewModel.HeaderErrorText.Should().Contain("was not found");
        partFocusRaised.Should().BeTrue();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WithDuplicatePicksDeclined_ShouldNotAddLines()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        session.Items.Add(
            new Model_ScannerBatchItem
            {
                ItemId = Guid.NewGuid(),
                SessionId = session.SessionId,
                SequenceNumber = 1,
                PayloadPartId = "MMC0000650",
                PayloadFromWarehouse = "002",
                PayloadFromLocation = "V-A1-01",
                PayloadToWarehouse = "002",
                PayloadQuantity = "25",
            }
        );

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [new Model_InforVisualMaterialLocationRow { LocationId = "V-A1-01", Quantity = 25m }]
                )
            );

        var viewModel = CreateWorkbenchViewModel(validation: validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>(
                [new Model_ScannerStockPick { Location = "V-A1-01", Quantity = "25", OnHand = 25m }]
            );
        viewModel.DuplicateAddConfirmationRequested += _ => Task.FromResult(false);

        await viewModel.PartValidationCompletedAsync("MMC0000650");

        // The duplicate warning is declined, so nothing is added and the part box is cleared.
        viewModel.SessionItems.Should().HaveCount(1);
        viewModel.NewPartId.Should().BeEmpty();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WithDuplicatePicksConfirmed_ShouldAddLines()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        session.Items.Add(
            new Model_ScannerBatchItem
            {
                ItemId = Guid.NewGuid(),
                SessionId = session.SessionId,
                SequenceNumber = 1,
                PayloadPartId = "MMC0000650",
                PayloadFromWarehouse = "002",
                PayloadFromLocation = "V-A1-01",
                PayloadToWarehouse = "002",
                PayloadQuantity = "25",
            }
        );

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [new Model_InforVisualMaterialLocationRow { LocationId = "V-A1-01", Quantity = 25m }]
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<IReadOnlyList<Model_ScannerStockPick>>(
                [new Model_ScannerStockPick { Location = "V-A1-01", Quantity = "25", OnHand = 25m }]
            );
        viewModel.DuplicateAddConfirmationRequested += _ => Task.FromResult(true);

        await viewModel.PartValidationCompletedAsync("MMC0000650");

        // The duplicate warning is accepted, so the new line is added alongside the existing one.
        viewModel.SessionItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task PartValidationCompletedAsync_ShouldIgnoreSecondRun_WhileFirstIsRunning()
    {
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                    [new Model_InforVisualMaterialLocationRow { LocationId = "V-A1-01", Quantity = 25m }]
                )
            );

        var viewModel = CreateWorkbenchViewModel(validation: validation.Object);
        var pickerGate = new TaskCompletionSource<IReadOnlyList<Model_ScannerStockPick>>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) => pickerGate.Task;

        // Start the first validation; it reaches the picker and suspends (guard is now set).
        var firstRun = viewModel.PartValidationCompletedAsync("MMC0000650");

        // A second trigger while the first is in flight must be ignored entirely.
        await viewModel.PartValidationCompletedAsync("MMC0000650");

        pickerGate.SetResult([]);
        await firstRun;

        validation.Verify(
            service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckAllCommand_ShouldOnlyValidateRowsNotAlreadyValid()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000650",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-05",
            PayloadQuantity = "5",
            ValidationState = Enum_ScannerValidationState.Valid,
        };
        var newItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "MMC0000651",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-02",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-06",
            PayloadQuantity = "2",
            ValidationState = Enum_ScannerValidationState.NotValidated,
        };
        session.Items.Add(validItem);
        session.Items.Add(newItem);

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult { State = Enum_ScannerValidationState.Valid }
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];

        await viewModel.CheckAllCommand.ExecuteAsync(null);

        // Only the not-yet-valid row is validated; the already-valid row is left untouched.
        validation.Verify(
            service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        newItem.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
        validItem.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
    }

    [Fact]
    public async Task RevalidateResumedValidItemsAsync_ShouldRevalidateOnlyPreviouslyValidWaitingRows()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var resumedValid = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000650",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-05",
            PayloadQuantity = "5",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
            MaxQuantity = null,
        };
        var notValidated = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "MMC0000651",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-02",
            PayloadToWarehouse = "002",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.NotValidated,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
            MaxQuantity = null,
        };
        session.Items.Add(resumedValid);
        session.Items.Add(notValidated);

        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerItemValidationResult
                    {
                        State = Enum_ScannerValidationState.Valid,
                        MaxQuantity = 25m,
                    }
                )
            );

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];

        await viewModel.RevalidateResumedValidItemsAsync();

        // Only the resumed-valid waiting row is rechecked; it regains its on-hand guard.
        validation.Verify(
            service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        resumedValid.MaxQuantity.Should().Be(25m);
        notValidated.MaxQuantity.Should().BeNull();
        notValidated.ValidationState.Should().Be(Enum_ScannerValidationState.NotValidated);
    }

    // ── Send confirmation: always ask Yes/No, one line per click ────────────────

    [Fact]
    public async Task SendSelectedCommand_WhenSent_ShouldShowPromptAndNotAutoRemove()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var item = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromLocation = "V-A1-01",
            PayloadToLocation = "R-05",
            PayloadQuantity = "5",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        session.Items.Add(item);

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service =>
                service.SendSpecificItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<Model_ScannerProfile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new Model_ScannerExecutionOutcome { SentCount = 1 }));

        var workflow = new Mock<IService_ScannerWorkflow>();
        var viewModel = CreateWorkbenchViewModel(workflow.Object, execution: execution.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.SelectedSessionItem = item;

        await viewModel.SendSelectedCommand.ExecuteAsync(null);

        // After a successful send the app must ask the operator (Yes/No) instead of
        // auto-removing the line or polling Infor Visual to decide for them.
        viewModel.IsSendPromptVisible.Should().BeTrue();
        viewModel.SessionItems.Should().HaveCount(1);
        workflow.Verify(
            service =>
                service.ReplaceSessionItemsAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ConfirmSavedCommand_ShouldClearOnlyTheSentLineAndSelectTheNext()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var first = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromLocation = "V-A1-01",
            PayloadToLocation = "R-05",
            PayloadQuantity = "5",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        var second = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "MMC0000650",
            PayloadFromLocation = "V-A1-02",
            PayloadToLocation = "R-06",
            PayloadQuantity = "2",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        session.Items.Add(first);
        session.Items.Add(second);

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service =>
                service.SendSpecificItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<Model_ScannerProfile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new Model_ScannerExecutionOutcome { SentCount = 1 }));

        var workflow = new Mock<IService_ScannerWorkflow>();
        workflow
            .Setup(service =>
                service.ReplaceSessionItemsAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Model_ScannerBatchSession current, CancellationToken _) =>
                    Model_Dao_Result_Factory.Success(current)
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, execution: execution.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.SelectedSessionItem = first;

        await viewModel.SendSelectedCommand.ExecuteAsync(null);
        await viewModel.ConfirmSavedCommand.ExecuteAsync(null);

        // Yes clears only the sent line, keeps the rest, and selects the next line.
        viewModel.IsSendPromptVisible.Should().BeFalse();
        viewModel.SessionItems.Should().HaveCount(1);
        viewModel.SessionItems.Should().ContainSingle().Which.ItemId.Should().Be(second.ItemId);
        viewModel.SelectedSessionItem.Should().NotBeNull();
        viewModel.SelectedSessionItem!.ItemId.Should().Be(second.ItemId);
    }

    [Fact]
    public async Task ConfirmNotSavedCommand_ShouldKeepTheLineWaitingForRetry()
    {
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        var first = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMCCS00740",
            PayloadFromLocation = "V-A1-01",
            PayloadToLocation = "R-05",
            PayloadQuantity = "5",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
        session.Items.Add(first);

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service =>
                service.SendSpecificItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<Model_ScannerProfile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new Model_ScannerExecutionOutcome { SentCount = 1 }));

        var viewModel = CreateWorkbenchViewModel(SetupUpsertWorkflow(session).Object, execution: execution.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];
        viewModel.SelectedSessionItem = first;

        await viewModel.SendSelectedCommand.ExecuteAsync(null);

        // Simulate the real execution service having marked the line sent before the operator
        // reported the transfer did not save in Infor Visual.
        first.ExecutionState = Enum_ScannerExecutionState.Sent;
        first.SentUtc = DateTime.UtcNow;

        await viewModel.ConfirmNotSavedCommand.ExecuteAsync(null);

        // No keeps the line, restores it to never-sent so it can be sent again, and keeps the
        // rest of the list untouched.
        viewModel.IsSendPromptVisible.Should().BeFalse();
        viewModel.SessionItems.Should().HaveCount(1);
        first.ExecutionState.Should().Be(Enum_ScannerExecutionState.Waiting);
        first.SentUtc.Should().BeNull();
        viewModel.SelectedSessionItem.Should().BeSameAs(first);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static ViewModel_Scanner_Workbench CreateWorkbenchViewModel(
        IService_ScannerWorkflow? workflow = null,
        IService_ScannerValidation? validation = null,
        IService_ScannerExecution? execution = null
    )
    {
        return new ViewModel_Scanner_Workbench(
            new Mock<IService_ScannerNavigation>().Object,
            workflow ?? new Mock<IService_ScannerWorkflow>().Object,
            validation ?? new Mock<IService_ScannerValidation>().Object,
            execution ?? new Mock<IService_ScannerExecution>().Object,
            new Mock<IService_ScannerHotkey>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
