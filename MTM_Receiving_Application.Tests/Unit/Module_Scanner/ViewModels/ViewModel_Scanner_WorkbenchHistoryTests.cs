using System.Threading;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
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

        await viewModel.SendNextCommand.ExecuteAsync(null);

        viewModel
            .SessionItems.Single(item => item.ItemId == validItem.ItemId)
            .ExecutionState.Should()
            .Be(Enum_ScannerExecutionState.Sent);
        viewModel.CurrentSession!.Status.Should().Be(Enum_ScannerSessionStatus.Completed);
    }

    [Fact]
    public async Task StopAfterThisCommand_ShouldRequestStopAndPreserveCurrentBatch()
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

        var execution = new Mock<IService_ScannerExecution>();
        execution
            .Setup(service => service.RequestStopAsync(session))
            .ReturnsAsync(
                () =>
                {
                    session.StopRequested = true;
                    session.StopReason = Enum_ScannerStopReason.UserStop;
                    return Model_Dao_Result_Factory.Success();
                }
            );

        var viewModel = CreateWorkbenchViewModel(
            workflow.Object,
            validation.Object,
            execution.Object
        );
        viewModel.CurrentSession = session;
        viewModel.SessionItems = [.. session.Items];

        await viewModel.StopAfterThisCommand.ExecuteAsync(null);

        viewModel.CurrentSession!.StopRequested.Should().BeTrue();
        viewModel.CurrentSession!.StopReason.Should().Be(Enum_ScannerStopReason.UserStop);
        viewModel.StatusMessage.Should().Contain("Stop requested");
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
