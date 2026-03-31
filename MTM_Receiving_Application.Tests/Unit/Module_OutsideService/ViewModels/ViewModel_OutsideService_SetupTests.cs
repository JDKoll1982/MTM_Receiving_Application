using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_OutsideService.ViewModels;

public class ViewModel_OutsideService_SetupTests
{
    [Fact]
    public async Task LoadLineAsync_ShouldForceCustomVendor_WhenNoSuggestionsExist()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-100"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateSetupLine("PART-100");

        await viewModel.LoadLineAsync(line);

        viewModel.HasVendorSuggestions.Should().BeFalse();
        viewModel.IsCustomVendorForced.Should().BeTrue();
        viewModel.UseCustomVendor.Should().BeTrue();
        viewModel.CanToggleCustomVendor.Should().BeFalse();
        viewModel.IsVendorSuggestionPickerVisible.Should().BeFalse();
    }

    [Fact]
    public async Task SavePrimaryActionAsync_ShouldSaveChanges_WhenLineIsAlreadyInSetup()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-200"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );
        outsideServiceMock
            .Setup(service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateSetupLine("PART-200");
        await viewModel.LoadLineAsync(line);

        viewModel.PrimaryActionText.Should().Be("Save Changes");
        viewModel.PackageCountInputValue = 3;
        viewModel.EditablePackages[0].PackageQuantity = 4;
        viewModel.EditablePackages[1].PackageQuantity = 5;
        viewModel.EditablePackages[2].PackageQuantity = 6;
        viewModel.CustomVendorName = "Updated Vendor";
        viewModel.BolNumber = "BOL-UPDATED";
        viewModel.SetupNotes = "Adjusted setup";

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service =>
                service.SaveSetupAsync(
                    It.Is<Model_OutsideServiceRequestLine>(saved =>
                        saved.LinePhase == Enum_OutsideServiceLinePhase.Setup
                        && saved.PackageCount == 3
                        && saved.Packages.Count == 3
                        && saved.SetupVendorName == "Updated Vendor"
                        && saved.BOLNumber == "BOL-UPDATED"
                        && saved.SetupNotes == "Adjusted setup"
                    )
                ),
            Times.Once
        );
    }

    private static ViewModel_OutsideService_Setup CreateViewModel(
        IService_OutsideService outsideService
    )
    {
        return new ViewModel_OutsideService_Setup(
            outsideService,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Model_OutsideServiceRequestLine CreateSetupLine(string partId)
    {
        return new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestLineId = 11,
            RequestNumber = "OS-2000",
            LineNumber = 1,
            PartId = partId,
            LinePhase = Enum_OutsideServiceLinePhase.Setup,
            PackageCount = 2,
            SetupVendorSource = "custom",
            SetupVendorName = "Original Vendor",
            BOLNumber = "BOL-123",
            ScheduledShipUtc = DateTime.UtcNow.AddDays(1),
            ShippingContact = "John",
            SetupNotes = "Initial notes",
            Packages = new List<Model_OutsideServiceRequestPackage>
            {
                new() { PackageSequence = 1, PackageQuantity = 1 },
                new() { PackageSequence = 2, PackageQuantity = 2 },
            },
        };
    }
}
