using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.ViewModels;

public sealed class ViewModel_Scanner_SettingsTests
{
    [Fact]
    public async Task LoadProfilesCommand_ShouldPopulateProfiles_WhenWorkflowReturnsData()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        workflow
            .Setup(service => service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ScannerProfile>
                    {
                        new() { ProfileId = Guid.NewGuid(), OwnerUserId = "u-1", ProfileName = "Default", AppWindowTitle = "Inventory Transfers", IsDefaultForUser = true },
                        new() { ProfileId = Guid.NewGuid(), OwnerUserId = "u-1", ProfileName = "Night", AppWindowTitle = "Inventory Transfers" },
                    }
                )
            );

        var viewModel = CreateViewModel(workflow.Object);

        await viewModel.LoadProfilesCommand.ExecuteAsync(null);

        viewModel.Profiles.Should().HaveCount(2);
        viewModel.SelectedProfile.Should().NotBeNull();
        viewModel.SelectedProfile!.ProfileName.Should().Be("Default");
    }

    [Fact]
    public async Task SaveProfileCommand_ShouldUseEditorValues_WhenSavingProfile()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();

        workflow
            .Setup(service =>
                service.SaveProfileAsync(It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Model_ScannerProfile profile, CancellationToken _) => Model_Dao_Result_Factory.Success(profile));

        workflow
            .Setup(service => service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ScannerProfile>()));

        var viewModel = CreateViewModel(workflow.Object);
        viewModel.OwnerUserId = "u-1";
        viewModel.ProfileName = "Shift A";
        viewModel.AppWindowTitle = "Inventory Transfers";
        viewModel.TargetExecutableName = "VMINVENT.exe";
        viewModel.FromWarehouseDefault = "002";
        viewModel.ToWarehouseDefault = "003";

        await viewModel.SaveProfileCommand.ExecuteAsync(null);

        workflow.Verify(
            service =>
                service.SaveProfileAsync(
                    It.Is<Model_ScannerProfile>(profile =>
                        profile.OwnerUserId == "u-1"
                        && profile.ProfileName == "Shift A"
                        && profile.AppWindowTitle == "Inventory Transfers"
                        && profile.ToWarehouseDefault == "003"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SaveProfileCommand_ShouldNotSave_WhenTargetChildWindowTitleMissing()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var viewModel = CreateViewModel(workflow.Object);
        viewModel.OwnerUserId = "u-1";
        viewModel.ProfileName = "Shift A";
        viewModel.TargetExecutableName = "VMINVENT.exe";
        viewModel.AppWindowTitle = "Inventory Transfers";
        viewModel.TargetChildWindowTitle = string.Empty;

        await viewModel.SaveProfileCommand.ExecuteAsync(null);

        workflow.Verify(
            service => service.SaveProfileAsync(It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        viewModel.StatusMessage.Should().Be("Target child screen title is required.");
    }

    [Fact]
    public async Task SaveProfileCommand_ShouldNotSave_WhenDuplicateProfileNameExistsForUser()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var viewModel = CreateViewModel(workflow.Object);
        viewModel.OwnerUserId = "u-1";
        viewModel.ProfileName = "Default";
        viewModel.TargetExecutableName = "VMINVENT.exe";
        viewModel.AppWindowTitle = "Inventory Transfers";
        viewModel.TargetChildWindowTitle = "Inventory Transfers";
        viewModel.Profiles.Add(
            new Model_ScannerProfile
            {
                ProfileId = Guid.NewGuid(),
                OwnerUserId = "u-1",
                ProfileName = "Default",
                AppWindowTitle = "Inventory Transfers",
                TargetChildWindowTitle = "Inventory Transfers",
            }
        );

        await viewModel.SaveProfileCommand.ExecuteAsync(null);

        workflow.Verify(
            service => service.SaveProfileAsync(It.IsAny<Model_ScannerProfile>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        viewModel.StatusMessage.Should().Be("Profile name must be unique for the current user.");
    }

    [Fact]
    public void NewProfileCommand_ShouldResetEditorAndClearSelection()
    {
        var viewModel = CreateViewModel(new Mock<IService_ScannerWorkflow>().Object);
        viewModel.SelectedProfile = new Model_ScannerProfile { ProfileName = "Existing" };
        viewModel.ProfileName = "Changed";

        viewModel.NewProfileCommand.Execute(null);

        viewModel.SelectedProfile.Should().BeNull();
        viewModel.ProfileName.Should().Be("New Profile");
        viewModel.TargetExecutableName.Should().Be("VMINVENT.exe");
    }

    [Fact]
    public async Task DeleteProfileCommand_ShouldCallWorkflow_WhenProfileIsSelected()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var profileId = Guid.NewGuid();

        workflow
            .Setup(service => service.DeleteProfileAsync(profileId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        workflow
            .Setup(service => service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ScannerProfile>()));

        var viewModel = CreateViewModel(workflow.Object);
        viewModel.OwnerUserId = "u-1";
        viewModel.SelectedProfile = new Model_ScannerProfile { ProfileId = profileId, OwnerUserId = "u-1", ProfileName = "Default" };

        await viewModel.DeleteProfileCommand.ExecuteAsync(null);

        workflow.Verify(
            service => service.DeleteProfileAsync(profileId, "u-1", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private static ViewModel_Scanner_Settings CreateViewModel(IService_ScannerWorkflow workflow)
    {
        return new ViewModel_Scanner_Settings(
            new Mock<IService_ScannerNavigation>().Object,
            workflow,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
