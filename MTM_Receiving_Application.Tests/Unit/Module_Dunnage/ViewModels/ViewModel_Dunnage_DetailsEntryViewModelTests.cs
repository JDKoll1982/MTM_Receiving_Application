using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
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
            receivingValidation.Object
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
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        )
        {
            PoNumber = "PO-7788",
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
            .OnlyContain(load => load.PoNumber == "PO-7788");
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
    }
}
