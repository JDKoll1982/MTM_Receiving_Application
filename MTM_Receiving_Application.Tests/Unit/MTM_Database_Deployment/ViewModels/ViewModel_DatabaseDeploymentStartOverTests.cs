using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.ViewModels;

/// <summary>
/// A finished run leaves the wizard on its outcome. Saving again would replay the same plan against
/// a database that already took it, so the save button turns into a fresh start that forgets the run
/// and returns to the connect screen.
/// </summary>
public sealed class ViewModel_DatabaseDeploymentStartOverTests
{
    [Fact]
    public void ShowStartOver_ShouldReplaceTheSaveButton_WhenTheRunSucceeded()
    {
        var viewModel = new ViewModel_DatabaseDeployment
        {
            StepIndex = 2,
            HasResult = true,
            ResultIsFailure = false,
        };

        viewModel.ShowStartOver.Should().BeTrue();
        viewModel.ShowSave.Should().BeFalse();
    }

    [Fact]
    public void ShowSave_ShouldStayVisible_WhenTheRunFailed()
    {
        var viewModel = new ViewModel_DatabaseDeployment
        {
            StepIndex = 2,
            HasResult = true,
            ResultIsFailure = true,
        };

        viewModel
            .ShowSave.Should()
            .BeTrue("a failed run has to stay saveable so the operator can try again");
        viewModel.ShowStartOver.Should().BeFalse();
    }

    [Fact]
    public void ShowStartOver_ShouldStayHidden_WhenTheOperatorStepsBackToTheCheckScreen()
    {
        var viewModel = new ViewModel_DatabaseDeployment
        {
            StepIndex = 1,
            HasResult = true,
            ResultIsFailure = false,
        };

        viewModel
            .ShowStartOver.Should()
            .BeFalse("the button slot belongs to Check on that step");
        viewModel.ShowSave.Should().BeFalse();
    }

    [Fact]
    public void StartOver_ShouldReturnToTheConnectScreen_AndForgetTheRun()
    {
        var viewModel = new ViewModel_DatabaseDeployment { StepIndex = 2 };
        viewModel.HasChecked = true;
        viewModel.HasResult = true;
        viewModel.ResultIsFailure = false;
        viewModel.ResultHeadline = "All done - the database is up to date";
        viewModel.ResultDetail = "277 item(s) saved.";
        viewModel.TechnicalDetails = "Applied: 277";
        viewModel.ReportPath = @"C:\reports\database-update.txt";
        viewModel.LogText = "[10:24:09] Saving the changes to the database.";
        viewModel.ModeIndex = 1;
        viewModel.ConfirmRebuild = true;
        viewModel.RemoveDatabaseOnlyObjects = true;
        viewModel.ChangeGroups.Add(
            new Model_ChangeGroup
            {
                Title = "New detail fields",
                Glyph = "\uE8FD",
                Summary = "12 fields will be added",
            }
        );

        viewModel.StartOverCommand.Execute(null);

        viewModel.StepIndex.Should().Be(0, "the connect screen is step 1");
        viewModel.IsStep1.Should().BeTrue();
        viewModel.HasResult.Should().BeFalse();
        viewModel.HasChecked.Should().BeFalse();
        viewModel.ChangeGroups.Should().BeEmpty();
        viewModel.ChangeHeadline.Should().Be("Nothing checked yet");
        viewModel.ResultHeadline.Should().BeEmpty();
        viewModel.ResultDetail.Should().BeEmpty();
        viewModel.TechnicalDetails.Should().BeEmpty();
        viewModel.ReportPath.Should().BeEmpty();
        viewModel.ModeIndex.Should().Be(0, "the next run starts on the recommended choice");
        viewModel.ConfirmRebuild.Should().BeFalse();
        viewModel.RemoveDatabaseOnlyObjects.Should().BeFalse();
        viewModel.LogText.Should().Contain("Started over");
        viewModel.LogText.Should().NotContain("Saving the changes", "the old run is forgotten");
        viewModel.ShowStartOver.Should().BeFalse();
        viewModel.CanSave.Should().BeFalse();
    }
}
