using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Tests.Integration.Module_Scanner;

/// <summary>
/// ScannerUpdate.md Task 1/2b: verifies the Workbench locks out operator inputs while an
/// automated background send cycle is running and unlocks them when it completes.
/// Uses the real execution service (with a mocked input engine that blocks mid-send) so the
/// full automation -> lockout wiring is exercised asynchronously.
/// </summary>
public sealed class WorkbenchInputLockoutTests
{
    // Short connection timeout so the best-effort item-result persistence fails fast and the
    // test never waits on a real MySQL server.
    private const string DummyConnectionString =
        "Server=localhost;Database=test;Uid=test;Pwd=test;Connection Timeout=1;Default Command Timeout=1;";

    [Fact]
    public async Task InputsShouldToggleDisabled_WhileAutomationRuns_AndRestoreAfter()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var engine = new Mock<IService_ScannerInputEngine>();
        var isFirstSendText = true;

        // Block on the first emitted field so the send cycle stays active until released.
        // The block runs on a background thread (see Task.Run below) so the test thread is
        // never stuck and can release the gate.
        engine
            .Setup(service => service.SendText(It.IsAny<string>()))
            .Returns(() =>
            {
                if (isFirstSendText)
                {
                    isFirstSendText = false;
                    gate.Task.GetAwaiter().GetResult();
                }

                return true;
            });

        engine
            .Setup(service => service.TryGetForegroundWindow(out It.Ref<IntPtr>.IsAny))
            .Returns((out IntPtr hwnd) =>
            {
                hwnd = new IntPtr(0x1234);
                return true;
            });
        engine
            .Setup(service => service.TryGetWindowProcessName(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny))
            .Returns((IntPtr _, out string name) =>
            {
                name = "VMINVENT";
                return true;
            });
        engine
            .Setup(service => service.TryGetWindowTitle(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny))
            .Returns((IntPtr _, out string title) =>
            {
                title = "Inventory Transfers";
                return true;
            });
        engine.Setup(service => service.SendChord(It.IsAny<uint>(), It.IsAny<ushort>())).Returns(true);
        engine.Setup(service => service.SendKeyPress(It.IsAny<ushort>(), It.IsAny<bool>())).Returns(true);

        var execution = new Service_ScannerExecution(
            engine.Object,
            new Dao_ScannerBatchItem(DummyConnectionString),
            new Mock<IService_LoggingUtility>().Object
        );

        var viewModel = CreateWorkbenchViewModel(execution);

        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        session.Items.Add(
            new Model_ScannerBatchItem
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
            }
        );

        var profile = new Model_ScannerProfile
        {
            TargetExecutableName = "VMINVENT.exe",
            ActivateAppBeforeSend = true,
            RequireExactTitleMatch = false,
            ActivationDelayMs = 0,
            DelayBetweenFieldsMs = 0,
            PauseAfterItemMs = 0,
        };

        // Run the send on a background thread: the mock engine blocks synchronously on the
        // gate, so it must not run on the test thread or the test can never release it.
        var sendTask = Task.Run(() => execution.SendNextItemAsync(session, profile));

        // Wait until the send cycle actually starts (IsAutomationRunning flips true).
        await WaitUntilAsync(() => execution.IsAutomationRunning);

        // Task 2b: inputs toggle Enabled = false while the background process is active.
        viewModel.IsPartEditable.Should().BeFalse();
        viewModel.IsSendEnabled.Should().BeFalse();

        // Release the send cycle and let it complete.
        gate.SetResult();
        await sendTask;

        execution.IsAutomationRunning.Should().BeFalse();
        viewModel.IsPartEditable.Should().BeTrue();
    }

    private static ViewModel_Scanner_Workbench CreateWorkbenchViewModel(
        IService_ScannerExecution execution
    )
    {
        return new ViewModel_Scanner_Workbench(
            new Mock<IService_ScannerNavigation>().Object,
            new Mock<IService_ScannerWorkflow>().Object,
            new Mock<IService_ScannerValidation>().Object,
            execution,
            new Mock<IService_ScannerHotkey>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("Timed out waiting for automation to start.");
            }

            await Task.Delay(10);
        }
    }
}
