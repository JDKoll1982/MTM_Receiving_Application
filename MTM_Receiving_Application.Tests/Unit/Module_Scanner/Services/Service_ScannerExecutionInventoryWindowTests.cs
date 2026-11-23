using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

/// <summary>
/// Covers the read-only window probe behind the Workbench Remove Selected gate: the Infor Visual
/// Inventory Transfers window counts as open only when it is actually found and owned by the
/// target process, and the probe never activates or launches anything.
/// </summary>
public sealed class Service_ScannerExecutionInventoryWindowTests
{
    // Short connection timeout so nothing in this test waits on a real MySQL server.
    private const string DummyConnectionString =
        "Server=172.16.1.104;Database=test;Uid=test;Pwd=test;Connection Timeout=1;Default Command Timeout=1;";

    private static Service_ScannerExecution CreateService(Mock<IService_ScannerInputEngine> engine) =>
        new(
            engine.Object,
            new Dao_ScannerBatchItem(DummyConnectionString),
            new Mock<IService_LoggingUtility>().Object
        );

    private static Mock<IService_ScannerInputEngine> CreateEngine(
        Func<string?, string?, IntPtr> windowLookup
    )
    {
        var engine = new Mock<IService_ScannerInputEngine>();
        engine
            .Setup(service =>
                service.TryFindWindow(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    out It.Ref<IntPtr>.IsAny
                )
            )
            .Returns(
                (string? className, string? windowTitle, out IntPtr handle) =>
                {
                    handle = windowLookup(className, windowTitle);
                    return handle != IntPtr.Zero;
                }
            );
        return engine;
    }

    private static void SetupProcessName(Mock<IService_ScannerInputEngine> engine, string processName) =>
        engine
            .Setup(service =>
                service.TryGetWindowProcessName(It.IsAny<IntPtr>(), out It.Ref<string>.IsAny)
            )
            .Returns(
                (IntPtr _, out string name) =>
                {
                    name = processName;
                    return true;
                }
            );

    [Fact]
    public void IsInventoryWindowOpen_ShouldBeTrue_WhenTheTransfersWindowIsFound()
    {
        var engine = CreateEngine(
            (_, windowTitle) =>
                string.Equals(windowTitle, "Inventory Transfers", StringComparison.Ordinal)
                    ? new IntPtr(1234)
                    : IntPtr.Zero
        );
        SetupProcessName(engine, "VMINVENT");

        var service = CreateService(engine);
        var profile = new Model_ScannerProfile { TargetExecutableName = "VMINVENT.exe" };

        service.IsInventoryWindowOpen(profile).Should().BeTrue();
    }

    [Fact]
    public void IsInventoryWindowOpen_ShouldBeFalse_WhenNoWindowMatches()
    {
        var engine = CreateEngine((_, _) => IntPtr.Zero);
        SetupProcessName(engine, "VMINVENT");

        var service = CreateService(engine);
        var profile = new Model_ScannerProfile { TargetExecutableName = "VMINVENT.exe" };

        service.IsInventoryWindowOpen(profile).Should().BeFalse();
    }

    [Fact]
    public void IsInventoryWindowOpen_ShouldBeFalse_WhenTheWindowBelongsToAnotherProcess()
    {
        var engine = CreateEngine(
            (_, windowTitle) =>
                string.Equals(windowTitle, "Inventory Transfers", StringComparison.Ordinal)
                    ? new IntPtr(1234)
                    : IntPtr.Zero
        );
        SetupProcessName(engine, "notepad");

        var service = CreateService(engine);
        var profile = new Model_ScannerProfile { TargetExecutableName = "VMINVENT.exe" };

        service.IsInventoryWindowOpen(profile).Should().BeFalse();
    }

    [Fact]
    public void IsInventoryWindowOpen_ShouldFallBackToTheAppWindow_WhenNoChildTitleIsConfigured()
    {
        var engine = CreateEngine(
            (_, windowTitle) =>
                string.Equals(windowTitle, "VMINVENT", StringComparison.Ordinal)
                    ? new IntPtr(4321)
                    : IntPtr.Zero
        );
        SetupProcessName(engine, "VMINVENT");

        var service = CreateService(engine);
        var profile = new Model_ScannerProfile
        {
            TargetExecutableName = "VMINVENT.exe",
            TargetChildWindowTitle = string.Empty,
            AppWindowTitle = "VMINVENT",
        };

        service.IsInventoryWindowOpen(profile).Should().BeTrue();
    }

    [Fact]
    public void IsInventoryWindowOpen_ShouldBeFalse_WhenTheProfileIsNull()
    {
        var service = CreateService(CreateEngine((_, _) => new IntPtr(1)));

        service.IsInventoryWindowOpen(null!).Should().BeFalse();
    }
}
