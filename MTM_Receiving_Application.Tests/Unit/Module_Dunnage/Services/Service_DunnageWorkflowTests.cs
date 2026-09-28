using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageWorkflowTests
{
    [Fact]
    public async Task StartWorkflowAsync_ShouldNavigateToImageSearch_WhenUserDefaultModeIsImageSearch()
    {
        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new MTM_Receiving_Application.Module_Core.Models.Systems.Model_UserSession(
                    new MTM_Receiving_Application.Module_Core.Models.Systems.Model_User
                    {
                        DefaultDunnageMode = "image-search",
                    }
                )
            );

        var service = new Service_DunnageWorkflow(
            new Mock<IService_MySQL_Dunnage>().Object,
            sessionManager.Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_UserPrivileges>().Object
        );

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.ImagePartSearch);
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldGenerateLoadsAndApplyDetails_ForGuidedWorkflowBatch()
    {
        var service = CreateService();

        service.CurrentSession.SelectedTypeId = 5;
        service.CurrentSession.SelectedTypeName = "Pallet";
        service.CurrentSession.SelectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
        };
        service.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "RACK-A1",
        };
        service.CurrentSession.SetUdcValue(1, "48");
        service.NumberOfLoads = 3;
        service.CurrentSession.LoadQuantities.Add(10m);
        service.CurrentSession.LoadQuantities.Add(20m);
        service.CurrentSession.LoadQuantities.Add(30m);

        service.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);

        var quantityStepResult = await service.AdvanceToNextStepAsync();

        quantityStepResult.IsSuccess.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.DetailsEntry);
        service.CurrentSession.Loads.Should().HaveCount(3);
        service.CurrentSession.Loads.Select(load => load.LoadNumber).Should().Equal(1, 2, 3);
        service.CurrentSession.Loads.Select(load => load.Quantity).Should().Equal(10m, 20m, 30m);

        service.CurrentSession.PONumber = "PO-7788";
        service.CurrentSession.Location = "DOCK-4";

        var detailsStepResult = await service.AdvanceToNextStepAsync();

        detailsStepResult.IsSuccess.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.Review);
        service.CurrentSession.Loads.Should().OnlyContain(load => load.PoNumber == "PO-007788");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.Location == "DOCK-4");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.TypeName == "Pallet");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.TypeId == 5);
        service.CurrentSession.Loads.Should().OnlyContain(load => load.GetUdcValue(1) == "48");
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldRequireLocation_WhenSessionLocationIsBlank()
    {
        var service = CreateService(defaultLocation: "QA-RECV");

        service.CurrentSession.SelectedTypeId = 5;
        service.CurrentSession.SelectedTypeName = "Pallet";
        service.CurrentSession.SelectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
        };
        service.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "RACK-A1",
        };
        service.NumberOfLoads = 1;
        service.CurrentSession.LoadQuantities.Add(10m);

        service.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
        await service.AdvanceToNextStepAsync();

        service.CurrentSession.PONumber = "PO-7788";
        service.CurrentSession.Location = string.Empty;

        var detailsStepResult = await service.AdvanceToNextStepAsync();

        detailsStepResult.IsSuccess.Should().BeFalse();
        detailsStepResult.ErrorMessage.Should().Be("Please enter a location.");
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.DetailsEntry);
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldReturnFailure_WhenResolvedLocationIsInvalid()
    {
        var service = CreateService(
            defaultLocation: "BAD-LOC",
            locationValidationResult: Model_ReceivingValidationResult.Error("Invalid location")
        );

        service.CurrentSession.SelectedTypeId = 5;
        service.CurrentSession.SelectedTypeName = "Pallet";
        service.CurrentSession.SelectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
        };
        service.CurrentSession.SelectedPart = new Model_DunnagePart { PartId = "DUN-100" };
        service.NumberOfLoads = 1;
        service.CurrentSession.LoadQuantities.Add(10m);
        service.CurrentSession.Location = "BAD-LOC";

        service.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
        await service.AdvanceToNextStepAsync();

        var detailsStepResult = await service.AdvanceToNextStepAsync();

        detailsStepResult.IsSuccess.Should().BeFalse();
        detailsStepResult.ErrorMessage.Should().Be("Invalid location");
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.DetailsEntry);
    }

    [Fact]
    public async Task ClearLabelDataAsync_WhenClearAllRequestedByNonAdmin_ShouldFail()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new MTM_Receiving_Application.Module_Core.Models.Systems.Model_UserSession(
                    new MTM_Receiving_Application.Module_Core.Models.Systems.Model_User
                    {
                        EmployeeNumber = 42,
                        WindowsUsername = "tester",
                    }
                )
            );

        var userPrivileges = new Mock<IService_UserPrivileges>();
        userPrivileges.SetupGet(service => service.IsInitialized).Returns(true);
        userPrivileges.SetupGet(service => service.CurrentUserId).Returns(42);
        userPrivileges.Setup(service => service.HasAnyRole("Admin", "Developer")).Returns(false);

        var service = new Service_DunnageWorkflow(
            dunnageService.Object,
            sessionManager.Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            userPrivileges.Object
        );

        var result = await service.ClearLabelDataAsync(clearAllRows: true);

        result.IsSuccess.Should().BeFalse();
        result
            .ErrorMessage.Should()
            .Be("Only Admin or Developer users can clear all dunnage label rows.");
        dunnageService.Verify(
            db => db.ClearLabelDataAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never
        );
    }

    private static Service_DunnageWorkflow CreateService(
        string defaultLocation = "RECV",
        Model_ReceivingValidationResult? locationValidationResult = null
    )
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
                    Data = new Model_SettingsValue { Value = defaultLocation },
                }
            );

        var receivingValidation = new Mock<IService_ReceivingValidation>();
        receivingValidation
            .Setup(service =>
                service.ValidateLocationAsync(It.IsAny<string?>(), It.IsAny<string>())
            )
            .ReturnsAsync(locationValidationResult ?? Model_ReceivingValidationResult.Success());

        return new Service_DunnageWorkflow(
            new Mock<IService_MySQL_Dunnage>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            settingsCore.Object,
            receivingValidation.Object,
            new Mock<IService_UserPrivileges>().Object
        );
    }
}
