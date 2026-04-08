using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Dunnage.ViewModels;

public sealed class ViewModel_Settings_Dunnage_UserPreferencesTests
{
    [Fact]
    public async Task SaveAsync_ShouldNotPersistImageLocation_WhenFolderDoesNotExist()
    {
        var settingsCoreMock = CreateSettingsCoreFacadeMock();
        var dunnageSettingsMock = CreateDunnageSettingsMock();
        var viewModel = CreateViewModel(settingsCoreMock.Object, dunnageSettingsMock.Object);

        viewModel.DefaultImageLocation = @"C:\DefinitelyMissingFolder\DunnageImages";

        await viewModel.SaveCommand.ExecuteAsync(null);

        settingsCoreMock.Verify(
            service =>
                service.SetSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                ),
            Times.Never
        );
        dunnageSettingsMock.Verify(
            service =>
                service.SaveStringAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                ),
            Times.Never
        );
        viewModel.StatusMessage.Should().Be("The default image location must point to an existing folder.");
    }

    [Fact]
    public async Task LoadSettingsAsync_ShouldPopulateDefaultImageLocation_WhenStoredValueExists()
    {
        var settingsCoreMock = CreateSettingsCoreFacadeMock();
        var dunnageSettingsMock = CreateDunnageSettingsMock(
            new Dictionary<string, string>
            {
                ["Dunnage.UserPreferences.DefaultImageLocation"] = @"C:\DunnageImages",
            }
        );

        var viewModel = CreateViewModel(settingsCoreMock.Object, dunnageSettingsMock.Object);

        await Task.Delay(50);

        viewModel.DefaultImageLocation.Should().Be(@"C:\DunnageImages");
    }

    private static Mock<IService_SettingsCoreFacade> CreateSettingsCoreFacadeMock()
    {
        var settingsCoreMock = new Mock<IService_SettingsCoreFacade>();
        settingsCoreMock
            .Setup(service => service.GetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                new Model_Dao_Result<Model_SettingsValue>
                {
                    Success = true,
                    Data = new Model_SettingsValue { Value = "RECV" },
                }
            );
        settingsCoreMock
            .Setup(service =>
                service.SetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        return settingsCoreMock;
    }

    private static Mock<IService_DunnageSettings> CreateDunnageSettingsMock(
        IReadOnlyDictionary<string, string>? values = null
    )
    {
        values ??= new Dictionary<string, string>();

        var dunnageSettingsMock = new Mock<IService_DunnageSettings>();
        dunnageSettingsMock
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => values.TryGetValue(key, out var value) ? value : string.Empty);
        dunnageSettingsMock
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(96);
        dunnageSettingsMock
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(true);
        dunnageSettingsMock
            .Setup(service => service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);

        return dunnageSettingsMock;
    }

    private static ViewModel_Settings_Dunnage_UserPreferences CreateViewModel(
        IService_SettingsCoreFacade settingsCoreFacade,
        IService_DunnageSettings dunnageSettings
    )
    {
        var sessionManagerMock = new Mock<IService_UserSessionManager>();
        var receivingValidationMock = new Mock<IService_ReceivingValidation>();
        receivingValidationMock
            .Setup(service => service.ValidateLocationAsync(It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(Model_ReceivingValidationResult.Success());

        return new ViewModel_Settings_Dunnage_UserPreferences(
            settingsCoreFacade,
            dunnageSettings,
            sessionManagerMock.Object,
            receivingValidationMock.Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}