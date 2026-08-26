using System.Threading;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.ViewModels;

public sealed class ViewModel_Scanner_WorkbenchHistoryTests
{
    [Fact]
    public async Task StartDraftSessionCommand_ShouldCreateCurrentSession_WhenWorkflowStartSucceeds()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var sessionId = Guid.NewGuid();

        workflow
            .Setup(service =>
                service.StartSessionAsync(
                    It.IsAny<Model_ScannerSessionStartRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerSessionStartResponse
                    {
                        SessionId = sessionId,
                        Status = Enum_ScannerSessionStatus.Draft,
                        CreatedUtc = DateTime.UtcNow,
                        Message = "Scanner session initialized.",
                    }
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);

        await viewModel.StartDraftSessionCommand.ExecuteAsync(null);

        viewModel.HasActiveSession.Should().BeTrue();
        viewModel.CurrentSession.Should().NotBeNull();
        viewModel.CurrentSession!.SessionId.Should().Be(sessionId);
        viewModel.StatusMessage.Should().Be("Scanner session initialized.");
    }

    [Fact]
    public async Task AddDraftItemCommand_ShouldKeepItem_WhenValidationReturnsInvalidResult()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var sessionId = Guid.NewGuid();

        workflow
            .Setup(service =>
                service.StartSessionAsync(
                    It.IsAny<Model_ScannerSessionStartRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerSessionStartResponse
                    {
                        SessionId = sessionId,
                        Status = Enum_ScannerSessionStatus.Draft,
                        CreatedUtc = DateTime.UtcNow,
                        Message = "Scanner session initialized.",
                    }
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
                    new Model_ScannerItemValidationResult
                    {
                        SessionId = sessionId,
                        ItemId = Guid.NewGuid(),
                        State = Enum_ScannerValidationState.Invalid,
                        Message = "Source quantity is insufficient.",
                        Notes = "Requested 10 exceeds available 2.",
                    }
                )
            );

        workflow
            .Setup(service =>
                service.UpsertBatchItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Model_ScannerBatchSession session, Model_ScannerBatchItem item, CancellationToken _) =>
                {
                    session.Items.Add(item);
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(session);
                }
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.NewPartId = "MMCCS00740";
        viewModel.NewFromWarehouse = "002";
        viewModel.NewFromLocation = "A-01";
        viewModel.NewToWarehouse = "002";
        viewModel.NewToLocation = "B-01";
        viewModel.NewQuantity = "10";

        await viewModel.AddDraftItemCommand.ExecuteAsync(null);

        viewModel.CurrentSession.Should().NotBeNull();
        viewModel.SessionItems.Should().HaveCount(1);
        viewModel.SessionItems[0].ValidationState.Should().Be(Enum_ScannerValidationState.Invalid);
        viewModel.LastValidationStatus.Should().Be(nameof(Enum_ScannerValidationState.Invalid));
        viewModel.LastValidationNotes.Should().Be("Requested 10 exceeds available 2.");
    }

    [Fact]
    public async Task AddDraftItemCommand_ShouldPickSourceLocation_WhenSourceQuantityInsufficient()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var sessionId = Guid.NewGuid();

        workflow
            .Setup(service =>
                service.StartSessionAsync(
                    It.IsAny<Model_ScannerSessionStartRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerSessionStartResponse
                    {
                        SessionId = sessionId,
                        Status = Enum_ScannerSessionStatus.Draft,
                        CreatedUtc = DateTime.UtcNow,
                        Message = "Scanner session initialized.",
                    }
                )
            );

        var validationCall = 0;
        validation
            .Setup(service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                () =>
                {
                    validationCall++;
                    return validationCall == 1
                        ? Model_Dao_Result_Factory.Success(
                            new Model_ScannerItemValidationResult
                            {
                                SessionId = sessionId,
                                ItemId = Guid.NewGuid(),
                                State = Enum_ScannerValidationState.Invalid,
                                Message = "Source quantity is insufficient.",
                                Notes = "Requested 10 exceeds available 2 at A-01.",
                            }
                        )
                        : Model_Dao_Result_Factory.Success(
                            new Model_ScannerItemValidationResult
                            {
                                SessionId = sessionId,
                                ItemId = Guid.NewGuid(),
                                State = Enum_ScannerValidationState.Valid,
                                Message = string.Empty,
                                Notes = "Validation passed.",
                            }
                        );
                }
            );

        workflow
            .Setup(service =>
                service.UpsertBatchItemAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<Model_ScannerBatchItem>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (Model_ScannerBatchSession session, Model_ScannerBatchItem item, CancellationToken _) =>
                {
                    session.Items.Add(item);
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(session);
                }
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<string?>("LOC-B");

        viewModel.NewPartId = "MMCCS00740";
        viewModel.NewFromWarehouse = "002";
        viewModel.NewFromLocation = "A-01";
        viewModel.NewToWarehouse = "002";
        viewModel.NewToLocation = "B-01";
        viewModel.NewQuantity = "10";

        await viewModel.AddDraftItemCommand.ExecuteAsync(null);

        viewModel.NewFromLocation.Should().Be("LOC-B");
        viewModel.CurrentSession.Should().NotBeNull();
        viewModel.SessionItems.Should().HaveCount(1);
        viewModel.SessionItems[0].PayloadFromLocation.Should().Be("LOC-B");
        viewModel.SessionItems[0].ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
        validation.Verify(
            service =>
                service.ValidateNewItemAsync(
                    It.IsAny<Model_ScannerItemValidationRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task CheckAllCommand_ShouldRevalidateSessionItems_WhenSessionHasItems()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
        };
        var invalidItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "MMF0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A02",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B02",
            PayloadQuantity = "5",
        };

        session.Items.Add(validItem);
        session.Items.Add(invalidItem);

        validation
            .Setup(service =>
                service.ValidateSessionItemsAsync(
                    It.IsAny<Model_ScannerBatchSession>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(() =>
            {
                validItem.ApplyValidationResult(
                    new Model_ScannerItemValidationResult
                    {
                        SessionId = session.SessionId,
                        ItemId = validItem.ItemId,
                        State = Enum_ScannerValidationState.Valid,
                        Notes = "Validation passed.",
                    }
                );
                invalidItem.ApplyValidationResult(
                    new Model_ScannerItemValidationResult
                    {
                        SessionId = session.SessionId,
                        ItemId = invalidItem.ItemId,
                        State = Enum_ScannerValidationState.Invalid,
                        Notes = "Destination location requires review.",
                    }
                );

                return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ScannerItemValidationResult>>(
                    new List<Model_ScannerItemValidationResult>
                    {
                        new()
                        {
                            SessionId = session.SessionId,
                            ItemId = validItem.ItemId,
                            State = Enum_ScannerValidationState.Valid,
                            Notes = "Validation passed.",
                        },
                        new()
                        {
                            SessionId = session.SessionId,
                            ItemId = invalidItem.ItemId,
                            State = Enum_ScannerValidationState.Invalid,
                            Notes = "Destination location requires review.",
                        },
                    }
                );
            });

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        viewModel.SelectedSessionItem = viewModel.SessionItems[1];

        await viewModel.CheckAllCommand.ExecuteAsync(null);

        viewModel.SessionItems.Should().HaveCount(2);
        viewModel.SessionItems[1].ValidationState.Should().Be(Enum_ScannerValidationState.Invalid);
        viewModel.LastValidationStatus.Should().Be(nameof(Enum_ScannerValidationState.Invalid));
        viewModel.LastValidationNotes.Should().Be("Destination location requires review.");
        viewModel.StatusMessage.Should().Contain("2 scanner items");
        viewModel.StatusMessage.Should().Contain("1 item(s) still require review");
    }

    [Fact]
    public async Task SendNextCommand_ShouldMarkEligibleItemAsSent_WhenSessionHasValidItems()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };

        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.SendNextItemAsync(session, It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                () =>
                {
                    validItem.ExecutionState = Enum_ScannerExecutionState.Sent;
                    validItem.SentUtc = DateTime.UtcNow;
                    validItem.LastUpdatedUtc = DateTime.UtcNow;
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(
                        new Model_ScannerExecutionOutcome { SentCount = 1 }
                    );
                }
            );

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        SetupTransferLookup(validation, found: false);
        SetShortTransferConfirmTimeout(viewModel);

        await viewModel.SendNextCommand.ExecuteAsync(null);

        viewModel
            .SessionItems.Single(item => item.ItemId == validItem.ItemId)
            .ExecutionState.Should()
            .Be(Enum_ScannerExecutionState.Sent);
        viewModel.CurrentSession!.Status.Should().Be(Enum_ScannerSessionStatus.Completed);
    }

    [Fact]
    public async Task SendNextCommand_ShouldShowSavedPrompt_WhenItemSent()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };
        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.SendNextItemAsync(session, It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                () =>
                {
                    validItem.ExecutionState = Enum_ScannerExecutionState.Sent;
                    validItem.SentUtc = DateTime.UtcNow;
                    validItem.LastUpdatedUtc = DateTime.UtcNow;
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(
                        new Model_ScannerExecutionOutcome { SentCount = 1 }
                    );
                }
            );

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        SetupTransferLookup(validation, found: false);
        SetShortTransferConfirmTimeout(viewModel);

        await viewModel.SendNextCommand.ExecuteAsync(null);

        viewModel.IsSendPromptVisible.Should().BeTrue();
        viewModel.SendNextCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task SendNextCommand_ShouldAutoConfirm_WhenTransferRecorded()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };
        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.SendNextItemAsync(session, It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                () =>
                {
                    validItem.ExecutionState = Enum_ScannerExecutionState.Sent;
                    validItem.SentUtc = DateTime.UtcNow;
                    validItem.LastUpdatedUtc = DateTime.UtcNow;
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(
                        new Model_ScannerExecutionOutcome { SentCount = 1 }
                    );
                }
            );
        workflow
            .Setup(service => service.ReplaceSessionItemsAsync(session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        SetupTransferLookup(validation, found: true);
        SetShortTransferConfirmTimeout(viewModel);

        await viewModel.SendNextCommand.ExecuteAsync(null);

        viewModel.IsSendPromptVisible.Should().BeFalse();
        viewModel.SessionItems.Should().BeEmpty();
        viewModel.CurrentSession!.Items.Should().BeEmpty();
        viewModel.StatusMessage.Should().Contain("saved to history");
    }

    [Fact]
    public async Task SendNextCommand_ShouldEnable_WhenSessionHasItems()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };

        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);

        viewModel.SendNextCommand.CanExecute(null).Should().BeFalse();
        viewModel.CurrentSession = session;
        viewModel.SendNextCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task SendNextCommand_ShouldEnable_WhenItemAddedToSameSessionInstance()
    {
        // Reproduces the real AddDraftItem flow: UpsertBatchItemAsync mutates and returns the
        // SAME session instance, and AddDraftItemAsync then reassigns SessionItems to a new
        // collection. The Send command must re-evaluate on the SessionItems reassignment because
        // reassigning CurrentSession to the same reference raises no PropertyChanged.
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];

        viewModel.SendNextCommand.CanExecute(null).Should().BeFalse();

        session.Items.Add(
            new Model_ScannerBatchItem
            {
                ItemId = Guid.NewGuid(),
                SessionId = session.SessionId,
                SequenceNumber = 1,
                PayloadPartId = "MMC0000850",
                PayloadFromWarehouse = "002",
                PayloadFromLocation = "A01",
                PayloadToWarehouse = "002",
                PayloadToLocation = "B01",
                PayloadQuantity = "1",
                ValidationState = Enum_ScannerValidationState.Valid,
            }
        );
        session.RecalculateItemCounters();

        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];

        viewModel.SendNextCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmSavedCommand_ShouldRemoveItemAndHidePrompt_WhenAnsweredYes()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };
        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.SendNextItemAsync(session, It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                () =>
                {
                    validItem.ExecutionState = Enum_ScannerExecutionState.Sent;
                    validItem.SentUtc = DateTime.UtcNow;
                    validItem.LastUpdatedUtc = DateTime.UtcNow;
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(
                        new Model_ScannerExecutionOutcome { SentCount = 1 }
                    );
                }
            );
        workflow
            .Setup(service => service.ReplaceSessionItemsAsync(session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        SetupTransferLookup(validation, found: false);
        SetShortTransferConfirmTimeout(viewModel);

        await viewModel.SendNextCommand.ExecuteAsync(null);
        viewModel.IsSendPromptVisible.Should().BeTrue();

        await viewModel.ConfirmSavedCommand.ExecuteAsync(null);

        viewModel.IsSendPromptVisible.Should().BeFalse();
        viewModel.SessionItems.Should().BeEmpty();
        viewModel.CurrentSession!.Items.Should().BeEmpty();
        viewModel.StatusMessage.Should().Contain("saved to history");
    }

    [Fact]
    public async Task ConfirmNotSavedCommand_ShouldResetItemAndClearForm_WhenAnsweredNo()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var session = new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = "u-1",
            OwnerDisplayName = "Operator A",
            SessionName = "Draft",
            ActiveProfileId = Guid.NewGuid(),
        };

        var validItem = new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B01",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.Valid,
        };
        session.Items.Add(validItem);
        session.RecalculateItemCounters();

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.SendNextItemAsync(session, It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                () =>
                {
                    validItem.ExecutionState = Enum_ScannerExecutionState.Sent;
                    validItem.SentUtc = DateTime.UtcNow;
                    validItem.LastUpdatedUtc = DateTime.UtcNow;
                    session.RecalculateItemCounters();
                    return Model_Dao_Result_Factory.Success(
                        new Model_ScannerExecutionOutcome { SentCount = 1 }
                    );
                }
            );
        execution
            .Setup(service => service.ClearTargetFormAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items.OrderBy(item => item.SequenceNumber)];
        SetupTransferLookup(validation, found: false);
        SetShortTransferConfirmTimeout(viewModel);

        await viewModel.SendNextCommand.ExecuteAsync(null);
        viewModel.IsSendPromptVisible.Should().BeTrue();

        await viewModel.ConfirmNotSavedCommand.ExecuteAsync(null);

        viewModel.IsSendPromptVisible.Should().BeFalse();
        viewModel
            .SessionItems.Single(item => item.ItemId == validItem.ItemId)
            .ExecutionState.Should()
            .Be(Enum_ScannerExecutionState.Waiting);
        viewModel.StatusMessage.Should().Contain("Form cleared");
        execution.Verify(
            service => service.ClearTargetFormAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public void SetFromQuantityLimit_ShouldEnableQuantityAndCapMax()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);

        viewModel.SetFromQuantityLimit(10000m);

        viewModel.IsQuantityEnabled.Should().BeTrue();
        viewModel.MaxQuantity.Should().Be(10000m);
    }

    [Fact]
    public void ClearFromQuantityLimit_ShouldDisableQuantity()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.SetFromQuantityLimit(10000m);

        viewModel.ClearFromQuantityLimit();

        viewModel.IsQuantityEnabled.Should().BeFalse();
        viewModel.MaxQuantity.Should().BeNull();
    }

    [Fact]
    public async Task RefreshHistoryCommand_ShouldPopulateRuns_WhenWorkflowReturnsData()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var historyResult = new Model_ScannerRunHistoryQueryResult();
        var run = new Model_ScannerRun
        {
            RunId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerUserId = Environment.UserName,
            StartedUtc = DateTime.UtcNow,
            FinalStatus = Enum_ScannerSessionStatus.Completed,
        };
        run.Items.Add(
            new Model_ScannerRunItem
            {
                SequenceNumber = 1,
                PartId = "MMF-1",
                FromWarehouse = "001",
                FromLocation = "A01",
                ToWarehouse = "002",
                ToLocation = "RECV-A01",
            }
        );
        historyResult.Runs.Add(
            run
        );

        workflow
            .Setup(service =>
                service.GetRunHistoryAsync(
                    It.IsAny<Model_ScannerRunHistoryQueryRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(historyResult));

        var viewModel = CreateHistoryViewModel(workflow.Object);

        await viewModel.RefreshHistoryCommand.ExecuteAsync(null);

        viewModel.Runs.Should().HaveCount(1);
        viewModel.SelectedRun.Should().NotBeNull();
        viewModel.SelectedRunItems.Should().HaveCount(1);
        viewModel.IsBusy.Should().BeFalse();
        viewModel.StatusMessage.Should().Contain("Loaded 1 scanner run records");
    }

    [Fact]
    public void ClearFiltersCommand_ShouldResetDateStatusAndMaxResults()
    {
        var viewModel = CreateHistoryViewModel(new Mock<IService_ScannerWorkflow>().Object);
        viewModel.DateFromUtc = DateTimeOffset.UtcNow.AddDays(-7);
        viewModel.DateToUtc = DateTimeOffset.UtcNow.AddDays(-1);
        viewModel.StatusFilter = Enum_ScannerSessionStatus.Failed;
        viewModel.MaxResults = 5;

        viewModel.ClearFiltersCommand.Execute(null);

        viewModel.StatusFilter.Should().BeNull();
        viewModel.MaxResults.Should().Be(100);
        viewModel.DateToUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetFromInventoryLocationsAsync_ShouldReturnStockLocations_FromValidationService()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        var expected = new List<Model_InforVisualMaterialLocationRow>
        {
            new()
            {
                PartId = "MMC0000850",
                WarehouseCode = "002",
                LocationId = "A-01",
                Quantity = 10m,
            },
            new()
            {
                PartId = "MMC0000850",
                WarehouseCode = "002",
                LocationId = "B-01",
                Quantity = 4m,
            },
        };
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(
                    "MMC0000850",
                    "002",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<
                    IReadOnlyList<Model_InforVisualMaterialLocationRow>
                >(expected)
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.NewPartId = "MMC0000850";
        viewModel.NewFromWarehouse = "002";

        var result = await viewModel.GetFromInventoryLocationsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FormatLocation_ShouldReturnFormattedValue_FromValidationService()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.FormatLocation("VA101"))
            .Returns("V-A1-01");

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);

        var result = viewModel.FormatLocation("VA101");

        result.Should().Be("V-A1-01");
    }

    [Fact]
    public async Task ValidateFromLocationAsync_ShouldStoreCanonicalLocation_WhenResolved()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.ValidateLocationAsync("V-A1-01", "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ScannerLocationValidationResult
                    {
                        IsValid = true,
                        CanonicalLocation = "VA101",
                    }
                )
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.NewFromWarehouse = "002";
        viewModel.NewFromLocation = "V-A1-01";

        var result = await viewModel.ValidateFromLocationAsync();

        result.IsValid.Should().BeTrue();
        // The quantity/stock match in the view relies on the canonical (DB) location id, so the
        // ViewModel must replace the display-formatted value ("V-A1-01") with the canonical form.
        viewModel.NewFromLocation.Should().Be("VA101");
    }

    private static void SetupTransferLookup(
        Mock<IService_ScannerValidation> validation,
        bool found
    )
    {
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
            .ReturnsAsync(Model_Dao_Result_Factory.Success(found));
    }

    private static void SetShortTransferConfirmTimeout(ViewModel_Scanner_Workbench viewModel)
    {
        viewModel.TransferConfirmTimeout = TimeSpan.FromMilliseconds(20);
        viewModel.TransferConfirmPollInterval = TimeSpan.FromMilliseconds(1);
    }

    private static ViewModel_Scanner_Workbench CreateWorkbenchViewModel(
        IService_ScannerWorkflow workflow,
        IService_ScannerValidation validation,
        IService_ScannerExecution? execution = null,
        IService_ScannerHotkey? hotkey = null
    )
    {
        return new ViewModel_Scanner_Workbench(
            new Mock<IService_ScannerNavigation>().Object,
            workflow,
            validation,
            execution ?? new Mock<IService_ScannerExecution>().Object,
            hotkey ?? new Mock<IService_ScannerHotkey>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static ViewModel_Scanner_History CreateHistoryViewModel(IService_ScannerWorkflow workflow)
    {
        return new ViewModel_Scanner_History(
            new Mock<IService_ScannerNavigation>().Object,
            workflow,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
