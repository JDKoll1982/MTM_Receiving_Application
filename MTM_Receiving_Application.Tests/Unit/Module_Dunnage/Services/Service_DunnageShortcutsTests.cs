using System.Text.Json;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageShortcutsTests
{
    [Fact]
    public async Task GetShortcutsAsync_ShouldReturnDefaults_WhenSettingsAreMissing()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<int?>())
            )
            .ReturnsAsync(new Model_Dao_Result<Model_SettingsValue> { Success = false });

        var service = CreateService(settingsCore.Object);

        var shortcuts = await service.GetShortcutsAsync();

        shortcuts.ModeSelectionShortcut.Key.Should().Be("M");
        shortcuts.ModeSelectionShortcut.IsCtrlEnabled.Should().BeTrue();
        shortcuts.NextStepShortcut.Key.Should().Be("Right");
        shortcuts.HelpShortcut.IsShiftEnabled.Should().BeTrue();
        shortcuts.IsToggleSimpleNavigationEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task SaveShortcutsAsync_ShouldPersistBindingsAndToggle()
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

        var service = CreateService(settingsCore.Object);
        var shortcuts = new Model_Settings_DunnageShortcuts
        {
            ModeSelectionShortcut = new Model_KeyboardShortcutBinding
            {
                Key = "M",
                IsCtrlEnabled = true,
            },
            ClearLabelDataShortcut = new Model_KeyboardShortcutBinding
            {
                Key = "C",
                IsCtrlEnabled = true,
                IsShiftEnabled = true,
            },
            NextStepShortcut = new Model_KeyboardShortcutBinding
            {
                Key = "R",
                IsCtrlEnabled = true,
            },
            BackStepShortcut = new Model_KeyboardShortcutBinding
            {
                Key = "L",
                IsCtrlEnabled = true,
            },
            HelpShortcut = new Model_KeyboardShortcutBinding
            {
                Key = "Slash",
                IsShiftEnabled = true,
            },
            IsToggleSimpleNavigationEnabled = false,
        };

        await service.SaveShortcutsAsync(shortcuts);

        settingsCore.Verify(
            core =>
                core.SetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.Shortcuts.NextStep,
                    It.Is<string>(json =>
                        JsonSerializer.Deserialize<Model_KeyboardShortcutBinding>(json)!.Key == "R"
                    ),
                    It.IsAny<int?>()
                ),
            Times.Once
        );
        settingsCore.Verify(
            core =>
                core.SetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
                    "False",
                    It.IsAny<int?>()
                ),
            Times.Once
        );
    }

    private static Service_DunnageShortcuts CreateService(IService_SettingsCoreFacade settingsCore)
    {
        return new Service_DunnageShortcuts(
            settingsCore,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object
        );
    }
}
