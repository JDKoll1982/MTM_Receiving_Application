using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

/// <summary>
/// Verifies the Infor Visual part-assignment dialog watcher that runs while the Workbench asks
/// "Was the transaction saved in Infor Visual?" - the "Inventory Transaction Entry" assignment
/// question and the "Add Part Location" form must be confirmed automatically so the save is
/// never blocked on a modal.
/// </summary>
public sealed class Service_ScannerExecutionDialogWatcherTests
{
    // Short connection timeout so nothing in this test waits on a real MySQL server.
    private const string DummyConnectionString =
        "Server=localhost;Database=test;Uid=test;Pwd=test;Connection Timeout=1;Default Command Timeout=1;";

    private const ushort VkReturn = 0x0D;

    private static Model_ScannerProfile CreateProfile()
    {
        return new Model_ScannerProfile { TargetExecutableName = "VMINVENT" };
    }

    private static Service_ScannerExecution CreateService(Mock<IService_ScannerInputEngine> engine)
    {
        return new Service_ScannerExecution(
            engine.Object,
            new Dao_ScannerBatchItem(DummyConnectionString),
            new Mock<IService_LoggingUtility>().Object
        )
        {
            TransferDialogPollInterval = TimeSpan.FromMilliseconds(10),
            TransferDialogWatchTimeout = TimeSpan.FromSeconds(5),
        };
    }

    [Theory]
    [InlineData("Inventory Transaction Entry")]
    [InlineData("Add Part Location")]
    public async Task Watcher_ShouldConfirmAssignmentDialog_BySendingEnter(string dialogTitle)
    {
        var dialogPressed = new TaskCompletionSource<ushort>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service =>
                service.TryFindWindow(null, It.IsAny<string?>(), out It.Ref<IntPtr>.IsAny)
            )
            .Returns(
                (string? _, string? title, out IntPtr handle) =>
                {
                    handle =
                        string.Equals(title, dialogTitle, StringComparison.Ordinal)
                            ? new IntPtr(4321)
                            : IntPtr.Zero;
                    return handle != IntPtr.Zero;
                }
            );
        engine
            .Setup(service =>
                service.TryGetWindowProcessName(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny)
            )
            .Returns(
                (IntPtr _, out string name) =>
                {
                    name = "VMINVENT";
                    return true;
                }
            );
        engine.Setup(service => service.TrySetForeground(It.IsAny<IntPtr>())).Returns(true);
        engine
            .Setup(service => service.SendKeyPress(It.IsAny<ushort>(), It.IsAny<bool>()))
            .Callback((ushort key, bool _) => dialogPressed.TrySetResult(key))
            .Returns(true);

        var service = CreateService(engine);

        service.StartTransferDialogWatcher(CreateProfile());
        try
        {
            var pressedKey = await dialogPressed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            pressedKey.Should().Be(VkReturn);
        }
        finally
        {
            service.StopTransferDialogWatcher();
        }
    }

    [Fact]
    public async Task Watcher_ShouldIgnoreDialog_WhenItBelongsToAnotherProcess()
    {
        var dialogPressed = new TaskCompletionSource<ushort>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service =>
                service.TryFindWindow(null, It.IsAny<string?>(), out It.Ref<IntPtr>.IsAny)
            )
            .Returns(
                (string? _, string? _, out IntPtr handle) =>
                {
                    handle = new IntPtr(4321);
                    return true;
                }
            );
        engine
            .Setup(service =>
                service.TryGetWindowProcessName(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny)
            )
            .Returns(
                (IntPtr _, out string name) =>
                {
                    // Same dialog title, but not Infor Visual - it must be left alone.
                    name = "NOTEPAD";
                    return true;
                }
            );
        engine
            .Setup(service => service.SendKeyPress(It.IsAny<ushort>(), It.IsAny<bool>()))
            .Callback((ushort key, bool _) => dialogPressed.TrySetResult(key))
            .Returns(true);

        var service = CreateService(engine);

        service.StartTransferDialogWatcher(CreateProfile());
        await Task.Delay(200);
        service.StopTransferDialogWatcher();

        dialogPressed.Task.IsCompleted.Should().BeFalse();
        engine.Verify(
            service => service.SendKeyPress(It.IsAny<ushort>(), It.IsAny<bool>()),
            Times.Never
        );
    }

    [Fact]
    public async Task StopTransferDialogWatcher_ShouldPreventFurtherConfirmation()
    {
        var dialogVisible = false;
        var dialogPressed = new TaskCompletionSource<ushort>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service =>
                service.TryFindWindow(null, It.IsAny<string?>(), out It.Ref<IntPtr>.IsAny)
            )
            .Returns(
                (string? _, string? _, out IntPtr handle) =>
                {
                    handle = dialogVisible ? new IntPtr(4321) : IntPtr.Zero;
                    return dialogVisible;
                }
            );
        engine
            .Setup(service =>
                service.TryGetWindowProcessName(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny)
            )
            .Returns(
                (IntPtr _, out string name) =>
                {
                    name = "VMINVENT";
                    return true;
                }
            );
        engine.Setup(service => service.TrySetForeground(It.IsAny<IntPtr>())).Returns(true);
        engine
            .Setup(service => service.SendKeyPress(It.IsAny<ushort>(), It.IsAny<bool>()))
            .Callback((ushort key, bool _) => dialogPressed.TrySetResult(key))
            .Returns(true);

        var service = CreateService(engine);

        service.StartTransferDialogWatcher(CreateProfile());
        await Task.Delay(120);

        // The operator answered the Workbench prompt, so the watcher must stop answering.
        service.StopTransferDialogWatcher();

        dialogVisible = true;
        await Task.Delay(300);

        dialogPressed.Task.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void StopTransferDialogWatcher_ShouldBeSafe_WhenNoWatcherIsRunning()
    {
        var service = CreateService(new Mock<IService_ScannerInputEngine>());

        var act = () => service.StopTransferDialogWatcher();

        act.Should().NotThrow();
    }
}
