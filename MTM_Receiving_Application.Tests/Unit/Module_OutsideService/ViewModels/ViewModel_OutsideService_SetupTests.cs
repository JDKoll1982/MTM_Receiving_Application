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
    #region Constructor

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public void Constructor_ShouldThrow_WhenOutsideServiceIsNull()
    {
        var act = () =>
            new ViewModel_OutsideService_Setup(
                null!,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            );

        act.Should().Throw<ArgumentNullException>().WithParameterName("outsideService");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public void Constructor_ShouldSetDefaults_WhenAllArgsValid()
    {
        var viewModel = CreateViewModel(new Mock<IService_OutsideService>().Object);

        viewModel.Title.Should().Be("Outside Service Setup", "title is set by constructor");
        viewModel.IsBusy.Should().BeFalse();
        viewModel.EditablePackages.Should().BeEmpty();
    }

    #endregion

    #region LoadLineAsync — Phase & Computed Properties

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldShowInitializePhase_WhenLineIsInitialize()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-INIT");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-INIT", Enum_OutsideServiceLinePhase.Initialize);

        await viewModel.LoadLineAsync(line);

        viewModel.IsInitializePhase.Should().BeTrue();
        viewModel.IsSetupPhase.Should().BeFalse();
        viewModel.PrimaryActionText.Should().Be("Save Setup");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldSetCurrentPhaseText_WhenLineLoaded()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-PHASE");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-PHASE", Enum_OutsideServiceLinePhase.Setup);

        await viewModel.LoadLineAsync(line);

        viewModel.CurrentPhaseText.Should().Be(Enum_OutsideServiceLinePhase.Setup.ToString());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldMapFieldsFromLine_WhenLineLoaded()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-FIELDS");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var scheduledDate = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var line = CreateLine("PART-FIELDS", Enum_OutsideServiceLinePhase.Setup);
        line.BOLNumber = "BOL-FIELDS";
        line.ShippingContact = "Jane";
        line.SetupNotes = "Field notes";
        line.CompletionNotes = "Done notes";
        line.ScheduledShipUtc = scheduledDate;

        await viewModel.LoadLineAsync(line);

        viewModel.BolNumber.Should().Be("BOL-FIELDS");
        viewModel.ShippingContact.Should().Be("Jane");
        viewModel.SetupNotes.Should().Be("Field notes");
        viewModel.CompletionNotes.Should().Be("Done notes");
        viewModel.ScheduledShipDate.Should().NotBeNull();
        viewModel
            .ScheduledShipDate!.Value.UtcDateTime.Should()
            .BeCloseTo(scheduledDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldLeaveScheduledShipDateNull_WhenLineHasNoScheduledDate()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-NODATE");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-NODATE", Enum_OutsideServiceLinePhase.Setup);
        line.ScheduledShipUtc = null;

        await viewModel.LoadLineAsync(line);

        viewModel.ScheduledShipDate.Should().BeNull();
    }

    #endregion

    #region LoadLineAsync — Vendor Logic (existing: NoSuggestions)

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
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
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldEnableSuggestionPicker_WhenSuggestionsExistAndSourceIsNotCustom()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V1",
            VendorName = "Acme",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-V1"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-V1", Enum_OutsideServiceLinePhase.Setup);
        line.SetupVendorSource = null;

        await viewModel.LoadLineAsync(line);

        viewModel.HasVendorSuggestions.Should().BeTrue();
        viewModel.IsCustomVendorForced.Should().BeFalse();
        viewModel.CanToggleCustomVendor.Should().BeTrue();
        viewModel.IsVendorSuggestionPickerVisible.Should().BeTrue();
        viewModel.UseCustomVendor.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldUseCustomVendor_WhenSuggestionsExistButSourceIsCustom()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V2",
            VendorName = "Beta Corp",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-V2"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-V2", Enum_OutsideServiceLinePhase.Setup);
        line.SetupVendorSource = "custom";
        line.SetupVendorName = "My Custom Vendor";

        await viewModel.LoadLineAsync(line);

        viewModel.UseCustomVendor.Should().BeTrue();
        viewModel.CustomVendorName.Should().Be("My Custom Vendor");
        viewModel.SelectedVendorSuggestion.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldPreselectMatchingSuggestion_WhenVendorIdMatches()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V3",
            VendorName = "Gamma Ltd",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-V3"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-V3", Enum_OutsideServiceLinePhase.Setup);
        line.SetupVendorSource = "suggested";
        line.SetupVendorId = "V3";

        await viewModel.LoadLineAsync(line);

        viewModel.SelectedVendorSuggestion.Should().NotBeNull();
        viewModel.SelectedVendorSuggestion!.VendorId.Should().Be("V3");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldFallbackToCustom_WhenGetVendorSuggestionsReturnsFailure()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-FAIL"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceVendorSuggestion>>(
                    "DB error"
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-FAIL", Enum_OutsideServiceLinePhase.Setup);

        await viewModel.LoadLineAsync(line);

        viewModel.HasVendorSuggestions.Should().BeFalse();
        viewModel.IsCustomVendorForced.Should().BeTrue();
    }

    #endregion

    #region LoadLineAsync — Package Mapping

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldMapPackageQuantities_WhenLineHasPackages()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-PKG");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-PKG", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 2;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 10 },
            new() { PackageSequence = 2, PackageQuantity = 25 },
        };

        await viewModel.LoadLineAsync(line);

        viewModel.EditablePackages.Should().HaveCount(2);
        viewModel.EditablePackages[0].PackageQuantity.Should().Be(10);
        viewModel.EditablePackages[1].PackageQuantity.Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task LoadLineAsync_ShouldDefaultToOneRow_WhenPackageCountIsZeroOrNegative()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-ZERO");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-ZERO", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 0;
        line.Packages = new List<Model_OutsideServiceRequestPackage>();

        await viewModel.LoadLineAsync(line);

        viewModel.PackageCountInputValue.Should().Be(1);
        viewModel.EditablePackages.Should().HaveCount(1);
    }

    #endregion

    #region Package Count Changes

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task PackageCountInputValue_ShouldPreserveExistingQuantities_WhenCountIncreased()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-INC");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-INC", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 2;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 5 },
            new() { PackageSequence = 2, PackageQuantity = 10 },
        };
        await viewModel.LoadLineAsync(line);

        viewModel.PackageCountInputValue = 4;

        viewModel.EditablePackages.Should().HaveCount(4);
        viewModel.EditablePackages[0].PackageQuantity.Should().Be(5, "original row 1 preserved");
        viewModel.EditablePackages[1].PackageQuantity.Should().Be(10, "original row 2 preserved");
        viewModel.EditablePackages[2].PackageQuantity.Should().Be(0, "new row defaults to 0");
        viewModel.EditablePackages[3].PackageQuantity.Should().Be(0, "new row defaults to 0");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task PackageCountInputValue_ShouldPreserveSurvivingQuantities_WhenCountDecreased()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-DEC");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-DEC", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 3;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 7 },
            new() { PackageSequence = 2, PackageQuantity = 8 },
            new() { PackageSequence = 3, PackageQuantity = 9 },
        };
        await viewModel.LoadLineAsync(line);

        viewModel.PackageCountInputValue = 1;

        viewModel.EditablePackages.Should().HaveCount(1);
        viewModel
            .EditablePackages[0]
            .PackageQuantity.Should()
            .Be(7, "first row quantity preserved");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task PackageCountInputValue_ShouldNormalizeToOne_WhenValueIsNaN()
    {
        var outsideServiceMock = SetupNoSuggestions("PART-NAN");
        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-NAN", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 2;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 3 },
            new() { PackageSequence = 2, PackageQuantity = 4 },
        };
        await viewModel.LoadLineAsync(line);

        viewModel.PackageCountInputValue = double.NaN;

        viewModel.EditablePackages.Should().HaveCount(1);
    }

    #endregion

    #region Vendor Toggle

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task UseCustomVendor_ShouldClearSelectedSuggestion_WhenSetToTrue()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V10",
            VendorName = "Toggle Corp",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-TOG"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-TOG", Enum_OutsideServiceLinePhase.Setup);
        line.SetupVendorSource = "suggested";
        line.SetupVendorId = "V10";
        await viewModel.LoadLineAsync(line);
        viewModel
            .SelectedVendorSuggestion.Should()
            .NotBeNull("precondition: suggestion was selected");

        viewModel.UseCustomVendor = true;

        viewModel.SelectedVendorSuggestion.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task UseCustomVendor_ShouldNotClearSelectedSuggestion_WhenSetToFalse()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V11",
            VendorName = "No-Clear Corp",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-NC"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-NC", Enum_OutsideServiceLinePhase.Setup);
        line.SetupVendorSource = "suggested";
        line.SetupVendorId = "V11";
        await viewModel.LoadLineAsync(line);
        viewModel.UseCustomVendor = true;
        viewModel.UseCustomVendor = false;

        viewModel
            .SelectedVendorSuggestion.Should()
            .BeNull(
                "suggestion was cleared when going to custom; setting back to false doesn't restore it"
            );
    }

    #endregion

    #region SavePrimaryAction — Validation

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldNotCallService_WhenAnyPackageQuantityIsZero()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateSetupLine("PART-VALID");
        await viewModel.LoadLineAsync(line);
        viewModel.EditablePackages[0].PackageQuantity = 0;

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()),
            Times.Never
        );
        viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldNoOp_WhenCurrentLineIsNull()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        var viewModel = CreateViewModel(outsideServiceMock.Object);

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()),
            Times.Never
        );
    }

    #endregion

    #region SavePrimaryAction — Success Paths

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldTransitionToSetup_WhenLineIsInitializePhase()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );
        outsideServiceMock
            .Setup(service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-INIT2", Enum_OutsideServiceLinePhase.Initialize);
        line.PackageCount = 1;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 1 },
        };
        line.SetupVendorSource = "custom";
        line.SetupVendorName = "Init Vendor";
        await viewModel.LoadLineAsync(line);

        var lineSavedRaised = false;
        var returnRaised = false;
        viewModel.LineSaved += () => lineSavedRaised = true;
        viewModel.ReturnRequested += () => returnRaised = true;
        viewModel.EditablePackages[0].PackageQuantity = 5;

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service =>
                service.SaveSetupAsync(
                    It.Is<Model_OutsideServiceRequestLine>(saved =>
                        saved.LinePhase == Enum_OutsideServiceLinePhase.Setup
                    )
                ),
            Times.Once
        );
        lineSavedRaised.Should().BeTrue("LineSaved event must fire on success");
        returnRaised.Should().BeTrue("ReturnRequested event must fire on success");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
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

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldPersistCustomVendorData_WhenUseCustomVendorIsTrue()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );
        outsideServiceMock
            .Setup(service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateSetupLine("PART-CUST");
        await viewModel.LoadLineAsync(line);
        viewModel.CustomVendorName = "  Hand Typed Vendor  ";
        viewModel.EditablePackages[0].PackageQuantity = 3;
        viewModel.EditablePackages[1].PackageQuantity = 4;

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service =>
                service.SaveSetupAsync(
                    It.Is<Model_OutsideServiceRequestLine>(saved =>
                        saved.SetupVendorSource == "custom"
                        && saved.SetupVendorName == "Hand Typed Vendor"
                        && saved.SetupVendorId == null
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldPersistSuggestedVendorData_WhenUseCustomVendorIsFalse()
    {
        var suggestion = new Model_OutsideServiceVendorSuggestion
        {
            VendorId = "V99",
            VendorName = "Suggested Vendor",
        };
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync("PART-SUG"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceVendorSuggestion> { suggestion }
                )
            );
        outsideServiceMock
            .Setup(service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateLine("PART-SUG", Enum_OutsideServiceLinePhase.Setup);
        line.PackageCount = 1;
        line.Packages = new List<Model_OutsideServiceRequestPackage>
        {
            new() { PackageSequence = 1, PackageQuantity = 1 },
        };
        line.SetupVendorSource = "suggested";
        line.SetupVendorId = "V99";
        await viewModel.LoadLineAsync(line);
        viewModel.EditablePackages[0].PackageQuantity = 2;

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(
            service =>
                service.SaveSetupAsync(
                    It.Is<Model_OutsideServiceRequestLine>(saved =>
                        saved.SetupVendorSource == "suggested"
                        && saved.SetupVendorName == "Suggested Vendor"
                        && saved.SetupVendorId == "V99"
                    )
                ),
            Times.Once
        );
    }

    #endregion

    #region SavePrimaryAction — Failure Path

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public async Task SavePrimaryActionAsync_ShouldNotRaiseEvents_WhenServiceReturnsFailure()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetVendorSuggestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );
        outsideServiceMock
            .Setup(service => service.SaveSetupAsync(It.IsAny<Model_OutsideServiceRequestLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure("DB write failed"));

        var viewModel = CreateViewModel(outsideServiceMock.Object);
        var line = CreateSetupLine("PART-ERR");
        await viewModel.LoadLineAsync(line);
        viewModel.EditablePackages[0].PackageQuantity = 1;
        viewModel.EditablePackages[1].PackageQuantity = 2;

        var lineSavedRaised = false;
        var returnRaised = false;
        viewModel.LineSaved += () => lineSavedRaised = true;
        viewModel.ReturnRequested += () => returnRaised = true;

        await viewModel.SavePrimaryActionCommand.ExecuteAsync(null);

        lineSavedRaised.Should().BeFalse("LineSaved must not fire when service fails");
        returnRaised.Should().BeFalse("ReturnRequested must not fire when service fails");
        viewModel.IsBusy.Should().BeFalse("IsBusy must be reset in finally block");
    }

    #endregion

    #region ReturnToQueue Command

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Layer", "ViewModel")]
    public void ReturnToQueueCommand_ShouldRaiseReturnRequested_WhenExecuted()
    {
        var viewModel = CreateViewModel(new Mock<IService_OutsideService>().Object);

        var returnRaised = false;
        viewModel.ReturnRequested += () => returnRaised = true;

        viewModel.ReturnToQueueCommand.Execute(null);

        returnRaised.Should().BeTrue();
    }

    #endregion

    #region Helpers

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

    private static Mock<IService_OutsideService> SetupNoSuggestions(string partId)
    {
        var mock = new Mock<IService_OutsideService>();
        mock.Setup(service => service.GetVendorSuggestionsAsync(partId))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceVendorSuggestion>())
            );
        return mock;
    }

    private static Model_OutsideServiceRequestLine CreateLine(
        string partId,
        Enum_OutsideServiceLinePhase phase
    )
    {
        return new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestLineId = 1,
            RequestNumber = "OS-1000",
            LineNumber = 1,
            PartId = partId,
            LinePhase = phase,
            PackageCount = 1,
            SetupVendorSource = "custom",
            SetupVendorName = "Test Vendor",
            BOLNumber = "BOL-000",
            ScheduledShipUtc = DateTime.UtcNow.AddDays(3),
            ShippingContact = "Test Contact",
            SetupNotes = "Test notes",
            Packages = new List<Model_OutsideServiceRequestPackage>
            {
                new() { PackageSequence = 1, PackageQuantity = 1 },
            },
        };
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

    #endregion
}
