using System.Reflection;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
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

    [Fact]
    public void HasRowsOrPendingRemovals_ShouldReturnTrue_WhenAllRowsWereRemovedButDeletesArePending()
    {
        var viewModel = CreateViewModel();

        SetPrivateField(viewModel, "_allLoads", new List<Model_DunnageLoad>());
        var removedLoads = GetPrivateField<List<Model_DunnageLoad>>(viewModel, "_removedLoads");
        removedLoads.Add(new Model_DunnageLoad { LoadUuid = Guid.NewGuid() });

        var result = InvokeHasRowsOrPendingRemovals(viewModel);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasRowsOrPendingRemovals_ShouldReturnFalse_WhenNoRowsAndNoPendingDeletesExist()
    {
        var viewModel = CreateViewModel();

        SetPrivateField(viewModel, "_allLoads", new List<Model_DunnageLoad>());

        var result = InvokeHasRowsOrPendingRemovals(viewModel);

        result.Should().BeFalse();
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

    private static bool InvokeHasRowsOrPendingRemovals(ViewModel_Dunnage_EditMode viewModel)
    {
        var methodInfo = typeof(ViewModel_Dunnage_EditMode).GetMethod(
            "HasRowsOrPendingRemovals",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(viewModel, null);
        return result.Should().BeOfType<bool>().Subject;
    }

    private static T GetPrivateField<T>(ViewModel_Dunnage_EditMode viewModel, string fieldName)
    {
        var fieldInfo = typeof(ViewModel_Dunnage_EditMode).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        fieldInfo.Should().NotBeNull();
        return fieldInfo!.GetValue(viewModel).Should().BeOfType<T>().Subject;
    }

    private static void SetPrivateField(
        ViewModel_Dunnage_EditMode viewModel,
        string fieldName,
        object value
    )
    {
        var fieldInfo = typeof(ViewModel_Dunnage_EditMode).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        fieldInfo.Should().NotBeNull();
        fieldInfo!.SetValue(viewModel, value);
    }

    private static ViewModel_Dunnage_EditMode CreateViewModel()
    {
        return new ViewModel_Dunnage_EditMode(
            new Mock<IService_MySQL_Dunnage>().Object,
            new Mock<IService_Pagination>().Object,
            new Mock<IService_DunnageWorkflow>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
