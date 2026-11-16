using FluentAssertions;
using MTM_Database_Deployment.Services;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Services;

/// <summary>
/// A crash before the window opens leaves the operator with nothing to report. These tests pin what
/// the failure log records - and that it never records the database password.
/// </summary>
public sealed class Service_FailureLogTests
{
    [Fact]
    public void Describe_ShouldNameTheSourceAndTheExecutableFolder()
    {
        var text = Service_FailureLog.Describe("Startup", new InvalidOperationException("boom"));

        text.Should().Contain("Where: Startup");
        text.Should().Contain("Executable: " + AppContext.BaseDirectory);
    }

    [Fact]
    public void Describe_ShouldIncludeEveryInnerException_SoTheRealCauseIsVisible()
    {
        var exception = new InvalidOperationException(
            "outer",
            new TypeInitializationException("Some.Type", new DllNotFoundException("missing.dll")));

        var text = Service_FailureLog.Describe("Startup", exception);

        text.Should().Contain("outer");
        text.Should().Contain("Some.Type");
        text.Should().Contain("missing.dll");
    }

    [Fact]
    public void Describe_ShouldCarryTheSuppliedContext_WhenTheCallerProvidesIt()
    {
        var text = Service_FailureLog.Describe(
            "Startup",
            new InvalidOperationException("boom"),
            "Database: mtm_receiving_application User: root");

        text.Should().Contain("Database: mtm_receiving_application");
    }

    [Fact]
    public void Write_ShouldPutTheFileUnderTheToolFolder_AndBeReadable()
    {
        var path = Service_FailureLog.Write("Test", new InvalidOperationException("written-by-test"));

        try
        {
            path.Should().NotBeNull();
            File.Exists(path).Should().BeTrue();
            File.ReadAllText(path!).Should().Contain("written-by-test");
            path.Should().StartWith(Service_FailureLog.LogDirectory);
        }
        finally
        {
            if (path is not null && File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
