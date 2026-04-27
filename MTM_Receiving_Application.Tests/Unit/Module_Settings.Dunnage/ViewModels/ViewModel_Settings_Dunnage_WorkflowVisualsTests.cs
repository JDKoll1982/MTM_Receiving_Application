using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Dunnage.ViewModels;

public sealed class ViewModel_Settings_Dunnage_WorkflowVisualsTests
{
    [Fact]
    public async Task Constructor_ShouldLoadSavedWorkflowVisualSettings()
    {
        var dunnageSettings = CreateDunnageSettingsMock(
            new Dictionary<string, bool>
            {
                [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = false,
                [DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection] = true,
                [DunnageSettingsKeys.Workflow.ShowImagesOnReview] = false,
                [DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing] = true,
            }
        );

        var viewModel = CreateViewModel(dunnageSettings: dunnageSettings);
        await Task.Delay(50);

        viewModel.ShowTypeImagesOnTypeSelection.Should().BeFalse();
        viewModel.ShowPartImagesOnPartSelection.Should().BeTrue();
        viewModel.ShowImagesOnReview.Should().BeFalse();
        viewModel.FallbackToTypeImageWhenPartMissing.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistAllWorkflowVisualFlags()
    {
        var dunnageSettings = CreateDunnageSettingsMock();
        var viewModel = CreateViewModel(dunnageSettings: dunnageSettings);

        viewModel.ShowTypeImagesOnTypeSelection = false;
        viewModel.ShowPartImagesOnPartSelection = false;
        viewModel.ShowImagesOnReview = true;
        viewModel.FallbackToTypeImageWhenPartMissing = false;

        await viewModel.SaveCommand.ExecuteAsync(null);

        dunnageSettings.Verify(
            service => service.SaveStringAsync(DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection, "False", It.IsAny<int?>()),
            Times.Once
        );
        dunnageSettings.Verify(
            service => service.SaveStringAsync(DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection, "False", It.IsAny<int?>()),
            Times.Once
        );
        dunnageSettings.Verify(
            service => service.SaveStringAsync(DunnageSettingsKeys.Workflow.ShowImagesOnReview, "True", It.IsAny<int?>()),
            Times.Once
        );
        dunnageSettings.Verify(
            service => service.SaveStringAsync(DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing, "False", It.IsAny<int?>()),
            Times.Once
        );
        viewModel.StatusMessage.Should().Be("Dunnage workflow visual settings saved.");
    }

    [Fact]
    public async Task ResetAsync_ShouldResetAllSettingsAndReloadDefaults()
    {
        var workflowValues = new Dictionary<string, bool>
        {
            [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = false,
            [DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection] = false,
            [DunnageSettingsKeys.Workflow.ShowImagesOnReview] = false,
            [DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing] = false,
        };

        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service => service.ResetSettingAsync("Dunnage", It.IsAny<string>(), null))
            .Callback((string _, string key, int? _) => workflowValues[key] = true)
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var dunnageSettings = CreateDunnageSettingsMock(workflowValues);
        var viewModel = CreateViewModel(settingsCore, dunnageSettings);
        await Task.Delay(50);

        await viewModel.ResetCommand.ExecuteAsync(null);

        settingsCore.Verify(
            service => service.ResetSettingAsync("Dunnage", DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection, null),
            Times.Once
        );
        settingsCore.Verify(
            service => service.ResetSettingAsync("Dunnage", DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection, null),
            Times.Once
        );
        settingsCore.Verify(
            service => service.ResetSettingAsync("Dunnage", DunnageSettingsKeys.Workflow.ShowImagesOnReview, null),
            Times.Once
        );
        settingsCore.Verify(
            service => service.ResetSettingAsync("Dunnage", DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing, null),
            Times.Once
        );
        dunnageSettings.Verify(
            service => service.GetBoolAsync(DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection, It.IsAny<int?>()),
            Times.AtLeast(2)
        );
        dunnageSettings.Verify(
            service => service.GetBoolAsync(DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection, It.IsAny<int?>()),
            Times.AtLeast(2)
        );
        dunnageSettings.Verify(
            service => service.GetBoolAsync(DunnageSettingsKeys.Workflow.ShowImagesOnReview, It.IsAny<int?>()),
            Times.AtLeast(2)
        );
        dunnageSettings.Verify(
            service => service.GetBoolAsync(DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing, It.IsAny<int?>()),
            Times.AtLeast(2)
        );
    }

    private static ViewModel_Settings_Dunnage_WorkflowVisuals CreateViewModel(
        Mock<IService_SettingsCoreFacade>? settingsCore = null,
        Mock<IService_DunnageSettings>? dunnageSettings = null
    )
    {
        settingsCore ??= new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service => service.ResetSettingAsync("Dunnage", It.IsAny<string>(), null))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        dunnageSettings ??= CreateDunnageSettingsMock();

        return new ViewModel_Settings_Dunnage_WorkflowVisuals(
            settingsCore.Object,
            dunnageSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_DunnageSettings> CreateDunnageSettingsMock(
        IReadOnlyDictionary<string, bool>? workflowValues = null
    )
    {
        workflowValues ??= new Dictionary<string, bool>
        {
            [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = true,
            [DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection] = true,
            [DunnageSettingsKeys.Workflow.ShowImagesOnReview] = true,
            [DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing] = true,
        };

        var dunnageSettings = new Mock<IService_DunnageSettings>();
        dunnageSettings
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => workflowValues[key]);
        dunnageSettings
            .Setup(service => service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);

        return dunnageSettings;
    }
}