using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services;

public sealed class Service_ErrorHandlerTests
{
    [Fact]
    public void BuildDaoDialogTitle_ShouldConvertMethodNameIntoFriendlyAction()
    {
        var title = InvokePrivateStatic<string>(
            nameof(BuildDaoDialogTitle_ShouldConvertMethodNameIntoFriendlyAction),
            "BuildDaoDialogTitle",
            "DeleteTypeAsync",
            Enum_ErrorSeverity.Error
        );

        title.Should().Be("Unable to delete type");
    }

    [Fact]
    public void BuildDaoDialogMessage_ShouldReturnUserFacingMessageWithoutWrapper()
    {
        const string rawMessage =
            "This type can't be deleted because 2 parts still use it. Reassign or delete those parts first.";

        var message = InvokePrivateStatic<string>(
            nameof(BuildDaoDialogMessage_ShouldReturnUserFacingMessageWithoutWrapper),
            "BuildDaoDialogMessage",
            rawMessage
        );

        message.Should().Be(rawMessage);
    }

    [Fact]
    public void BuildDaoLogMessage_ShouldPreserveTechnicalOperationName()
    {
        var logMessage = InvokePrivateStatic<string>(
            nameof(BuildDaoLogMessage_ShouldPreserveTechnicalOperationName),
            "BuildDaoLogMessage",
            "DeleteTypeAsync",
            "This type can't be deleted because 2 parts still use it."
        );

        logMessage
            .Should()
            .Be(
                "Database operation 'DeleteTypeAsync' failed: This type can't be deleted because 2 parts still use it."
            );
    }

    private static T InvokePrivateStatic<T>(string because, string methodName, params object[] args)
    {
        var methodInfo = typeof(Service_ErrorHandler).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        methodInfo.Should().NotBeNull(because);

        var result = methodInfo!.Invoke(null, args);
        return result.Should().BeOfType<T>().Subject;
    }
}
