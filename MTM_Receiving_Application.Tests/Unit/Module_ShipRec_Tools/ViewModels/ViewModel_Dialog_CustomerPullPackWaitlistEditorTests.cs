using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Dialog_CustomerPullPackWaitlistEditorTests
{
    [Fact]
    public void ValidateInputs_ShouldRequireRequesterNote_WhenNoSelectableLocationsExist()
    {
        var viewModel = CreateViewModel(
            new Model_CustomerPullPack_DemandLine
            {
                SourceLineKey = "LINE-1",
                CustomerId = "VOLVO",
                CustomerName = "Volvo Group",
                CustomerOrderId = "CO-1001",
                ParentPartId = "PART-100",
                QuantityToPack = 12,
            }
        );

        var isValid = viewModel.ValidateInputs(out var validationMessage);

        isValid.Should().BeFalse();
        validationMessage.Should().Contain("requester note is required");
    }

    [Fact]
    public void BuildBatchEntries_ShouldPreserveOperationalFields_WhenLinkedItemWasAccepted()
    {
        var selectedLine = new Model_CustomerPullPack_DemandLine
        {
            SourceLineKey = "LINE-2",
            CustomerId = "VOLVO",
            CustomerName = "Volvo Group",
            CustomerOrderId = "CO-1002",
            ParentPartId = "PART-100",
            QuantityToPack = 25,
            LinkedWaitlistId = "WL-2002",
            LocationOptions =
            [
                new Model_CustomerPullPack_LocationOption
                {
                    LocationKey = "LINE-2|SUB-01",
                    SourceLineKey = "LINE-2",
                    LocationId = "SUB-01",
                    DisplayLabel = "SUB-01",
                    Selected = true,
                },
            ],
        };

        var acceptedEntry = new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = "WL-2002",
            SourceLineKey = "LINE-2",
            CustomerId = "VOLVO",
            CustomerName = "Volvo Group",
            CustomerOrderId = "CO-1002",
            ParentPartId = "PART-100",
            RequestedQuantity = 10,
            SelectedLocations = ["LOCKED-LOC"],
            RequestedByUserId = "jkoll",
            RequestedByDisplayName = "John Koll",
            RequesterContextNote = "Original note",
            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Accepted,
            CurrentOwnerUserId = "handler1",
            CurrentOwnerDisplayName = "Handler One",
            HandlerNote = "Existing handler note",
            LocationReviewFlag = false,
        };

        var viewModel = CreateViewModel(selectedLine, acceptedEntry);
        viewModel.RequesterContextNote = "Requester update";

        var entries = viewModel.BuildBatchEntries("jkoll", "John Koll (Emp #1)");

        entries.Should().ContainSingle();
        entries[0].CurrentStatus.Should().Be(Enum_CustomerPullPackWaitlistStatus.Accepted);
        entries[0].CurrentOwnerUserId.Should().Be("handler1");
        entries[0].SelectedLocations.Should().Equal(["LOCKED-LOC"]);
        entries[0].RequestedQuantity.Should().Be(10);
        entries[0].RequesterContextNote.Should().Be("Requester update");
    }

    private static ViewModel_Dialog_CustomerPullPackWaitlistEditor CreateViewModel(
        Model_CustomerPullPack_DemandLine selectedLine,
        Model_CustomerPullPack_WaitlistEntry? existingEntry = null
    )
    {
        return new ViewModel_Dialog_CustomerPullPackWaitlistEditor(
            new[] { selectedLine },
            existingEntry,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
