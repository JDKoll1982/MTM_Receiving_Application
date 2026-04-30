using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_DetailsEntryViewModelTests
{
    [Fact]
    public async Task GoNextAsync_ShouldApplySpecValuesToLoadsBeforeNavigatingToReview()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.UserPreferences.DefaultLocation,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(
                new Model_Dao_Result<Model_SettingsValue>
                {
                    Success = true,
                    Data = new Model_SettingsValue { Value = "RECV" },
                }
            );

        var receivingValidation = new Mock<IService_ReceivingValidation>();
        receivingValidation
            .Setup(service =>
                service.ValidateLocationAsync(It.IsAny<string?>(), It.IsAny<string>())
            )
            .ReturnsAsync(Model_ReceivingValidationResult.Success());

        var workflowService = new Service_DunnageWorkflow(
            new Mock<IService_MySQL_Dunnage>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            settingsCore.Object,
            receivingValidation.Object,
            new Mock<IService_UserPrivileges>().Object
        );

        workflowService.CurrentSession.SelectedTypeId = 5;
        workflowService.CurrentSession.SelectedTypeName = "Pallet";
        workflowService.CurrentSession.SelectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
        };
        workflowService.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "RACK-A1",
        };
        workflowService.NumberOfLoads = 2;
        workflowService.CurrentSession.LoadQuantities.Add(10m);
        workflowService.CurrentSession.LoadQuantities.Add(20m);
        workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
        await workflowService.AdvanceToNextStepAsync();

        var viewModel = new ViewModel_Dunnage_DetailsEntry(
            workflowService,
            new Mock<IService_MySQL_Dunnage>().Object,
            new Mock<IService_Dispatcher>().Object,
            new Mock<IService_Help>().Object,
            receivingValidation.Object,
            new Mock<IService_InforVisual>().Object,
            settingsCore.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        )
        {
            PoNumber = "7788",
            Location = "DOCK-4",
            SpecInputs = new ObservableCollection<Model_SpecInput>
            {
                new()
                {
                    SpecName = "Length",
                    SpecType = "number",
                    Value = 48,
                },
                new()
                {
                    SpecName = "Stackable",
                    SpecType = "boolean",
                    Value = true,
                },
            },
        };

        await viewModel.GoNextCommand.ExecuteAsync(null);

        workflowService.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.Review);
        workflowService.CurrentSession.Loads.Should().HaveCount(2);
        workflowService
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.PoNumber == "PO-007788");
        workflowService
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.Location == "DOCK-4");
        workflowService
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.SpecValues != null && load.SpecValues.Count == 2);
        workflowService.CurrentSession.Loads.Should().OnlyContain(load => load.Specs.Count == 2);
        workflowService
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.SpecValues!["Length"].ToString() == "48");
        workflowService
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.SpecValues!["Stackable"].ToString() == "True");
        workflowService.CurrentSession.PONumber.Should().Be("PO-007788");
    }

    [Fact]
    public async Task LoadSpecsForSelectedPartAsync_ShouldMergeConfiguredSpecsWithPartSpecificDefinitions()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetSpecsForTypeAsync(5))
            .ReturnsAsync(
                new Model_Dao_Result<List<Model_DunnageSpec>>
                {
                    Success = true,
                    Data =
                    [
                        new Model_DunnageSpec
                        {
                            SpecKey = "Length",
                            SpecValue = "{\"type\":\"Number\",\"required\":true,\"unit\":\"in\"}",
                        },
                    ],
                }
            );

        var selectedPart = new Model_DunnagePart
        {
            PartId = "DUN-NEW-100",
            TypeId = 5,
            SpecValues = JsonSerializer.Serialize(
                Helper_Dunnage_PartSpecs.BuildCombinedSpecPayload(
                    new Dictionary<string, object?> { ["Length"] = 48 },
                    [
                        new Model_SpecItem
                        {
                            Name = "Edge Guard",
                            DataType = "Choices",
                            IsRequired = true,
                            Choices = ["Yes", "No"],
                        },
                    ],
                    string.Empty
                )
            ),
        };

        var workflowService = new Service_DunnageWorkflow(
            dunnageService.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_UserPrivileges>().Object
        );

        workflowService.CurrentSession.SelectedTypeId = 5;
        workflowService.CurrentSession.SelectedPart = selectedPart;

        var viewModel = new ViewModel_Dunnage_DetailsEntry(
            workflowService,
            dunnageService.Object,
            new Mock<IService_Dispatcher>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await viewModel.LoadSpecsForSelectedPartAsync();

        viewModel.SpecInputs.Should().HaveCount(2);
        viewModel.NumberSpecs.Should().ContainSingle(spec => spec.SpecName == "Length");
        viewModel
            .NumberSpecs.Single(spec => spec.SpecName == "Length")
            .Value!.ToString()
            .Should()
            .Be("48");
        viewModel.ChoiceSpecs.Should().ContainSingle(spec => spec.SpecName == "Edge Guard");
        viewModel
            .ChoiceSpecs.Single(spec => spec.SpecName == "Edge Guard")
            .Choices.Should()
            .Equal("Yes", "No");
    }

    [Fact]
    public async Task LoadSpecsForSelectedPartAsync_ShouldPreFillLocationFromSelectedPartHomeLocation()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.UserPreferences.DefaultLocation,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(
                new Model_Dao_Result<Model_SettingsValue>
                {
                    Success = true,
                    Data = new Model_SettingsValue { Value = "RECV" },
                }
            );

        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetSpecsForTypeAsync(5))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnageSpec>()));

        var workflowService = new Service_DunnageWorkflow(
            dunnageService.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            settingsCore.Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_UserPrivileges>().Object
        );

        workflowService.CurrentSession.SelectedTypeId = 5;
        workflowService.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "BACK PAD",
        };

        var viewModel = new ViewModel_Dunnage_DetailsEntry(
            workflowService,
            dunnageService.Object,
            new Mock<IService_Dispatcher>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_InforVisual>().Object,
            settingsCore.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await viewModel.LoadSpecsForSelectedPartAsync();

        viewModel.Location.Should().Be("BACK PAD");
        workflowService.CurrentSession.Location.Should().Be("BACK PAD");
    }

    [Fact]
    public async Task LoadSpecsForSelectedPartAsync_ShouldRaiseCanProceedNotification_WhenInputsBecomeValid()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync(
                    "Dunnage",
                    DunnageSettingsKeys.UserPreferences.DefaultLocation,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(
                new Model_Dao_Result<Model_SettingsValue>
                {
                    Success = true,
                    Data = new Model_SettingsValue { Value = "RECV" },
                }
            );

        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetSpecsForTypeAsync(5))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnageSpec>()));

        var workflowService = new Service_DunnageWorkflow(
            dunnageService.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            settingsCore.Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_UserPrivileges>().Object
        );

        workflowService.CurrentSession.SelectedTypeId = 5;
        workflowService.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "BACK PAD",
        };

        var viewModel = new ViewModel_Dunnage_DetailsEntry(
            workflowService,
            dunnageService.Object,
            new Mock<IService_Dispatcher>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_InforVisual>().Object,
            settingsCore.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        var canProceedSnapshots = new List<bool>();
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel_Dunnage_DetailsEntry.CanProceedToNextStep))
            {
                canProceedSnapshots.Add(viewModel.CanProceedToNextStep);
            }
        };

        await viewModel.LoadSpecsForSelectedPartAsync();

        canProceedSnapshots.Should().Contain(true);
        viewModel.CanProceedToNextStep.Should().BeTrue();
    }
}
