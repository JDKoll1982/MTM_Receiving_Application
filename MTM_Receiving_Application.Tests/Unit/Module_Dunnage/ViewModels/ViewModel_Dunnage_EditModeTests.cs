using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_EditModeTests
{
    [Fact]
    public void HasValidRequiredSelections_ShouldReturnFalse_WhenPartIdUsesPlaceholder()
    {
        var load = new Model_DunnageLoad { TypeName = "Gaylords", PartId = "Select Part ID" };

        var result = InvokeHasValidRequiredSelections(load);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasValidRequiredSelections_ShouldReturnTrue_WhenTypeAndPartIdAreSelected()
    {
        var load = new Model_DunnageLoad { TypeName = "Gaylords", PartId = "SHORT" };

        var result = InvokeHasValidRequiredSelections(load);

        result.Should().BeTrue();
    }

    [Fact]
    public void BuildSaveConfirmationMessage_ShouldDescribeEditedAndRemovedCounts()
    {
        var message = InvokeBuildSaveConfirmationMessage(2, 1);

        message.Should().Be("This will save 2 edited row(s) and remove 1 row(s).");
    }

    [Fact]
    public void BuildSaveCompletedMessage_ShouldDescribeRemovalOnly()
    {
        var message = InvokeBuildSaveCompletedMessage(0, 3);

        message.Should().Be("Removed 3 row(s).");
    }

    private static bool InvokeHasValidRequiredSelections(Model_DunnageLoad load)
    {
        var methodInfo = typeof(ViewModel_Dunnage_EditMode).GetMethod(
            "HasValidRequiredSelections",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, [load]);
        return result.Should().BeOfType<bool>().Subject;
    }

    private static string InvokeBuildSaveConfirmationMessage(
        int editedLoadCount,
        int removedLoadCount
    )
    {
        var methodInfo = typeof(ViewModel_Dunnage_EditMode).GetMethod(
            "BuildSaveConfirmationMessage",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, [editedLoadCount, removedLoadCount]);
        return result.Should().BeOfType<string>().Subject;
    }

    private static string InvokeBuildSaveCompletedMessage(int editedLoadCount, int removedLoadCount)
    {
        var methodInfo = typeof(ViewModel_Dunnage_EditMode).GetMethod(
            "BuildSaveCompletedMessage",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, [editedLoadCount, removedLoadCount]);
        return result.Should().BeOfType<string>().Subject;
    }
}
