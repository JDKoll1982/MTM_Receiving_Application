using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageSettingsTests
{
    [Fact]
    public async Task GetStringAsync_ShouldReturnModuleDefault_WhenSettingIsMissing()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<int?>())
            )
            .ReturnsAsync(new Model_Dao_Result<Model_SettingsValue> { Success = false });

        var service = new Service_DunnageSettings(settingsCore.Object);

        var result = await service.GetStringAsync(
            DunnageSettingsKeys.UserPreferences.TypeSelectionSort,
            42
        );

        result.Should().Be("Name (A-Z)");
    }

    [Fact]
    public async Task GetBoolAsyncAndGetIntAsync_ShouldParseStoredValues()
    {
        var settingsValues = new Dictionary<string, string>
        {
            [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = "false",
            [DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize] = "144",
        };

        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<int?>())
            )
            .ReturnsAsync(
                (string _, string key, int? _) =>
                    new Model_Dao_Result<Model_SettingsValue>
                    {
                        Success = true,
                        Data = new Model_SettingsValue { Value = settingsValues[key] },
                    }
            );

        var service = new Service_DunnageSettings(settingsCore.Object);

        var boolResult = await service.GetBoolAsync(
            DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection
        );
        var intResult = await service.GetIntAsync(
            DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize
        );

        boolResult.Should().BeFalse();
        intResult.Should().Be(144);
    }

    [Fact]
    public async Task SaveStringAsync_ShouldDelegateToSettingsCore()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.SetSettingAsync(
                    "Dunnage",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var service = new Service_DunnageSettings(settingsCore.Object);

        await service.SaveStringAsync(
            DunnageSettingsKeys.UserPreferences.TypeSelectionSort,
            "Times Used",
            42
        );

        settingsCore.Verify(
            core =>
                core.SetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.UserPreferences.TypeSelectionSort,
                    "Times Used",
                    42
                ),
            Times.Once
        );
    }
}
