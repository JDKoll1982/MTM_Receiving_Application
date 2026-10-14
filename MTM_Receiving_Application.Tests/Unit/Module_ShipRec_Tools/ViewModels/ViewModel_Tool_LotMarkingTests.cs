using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Covers the enabled-part list, which is the gate on the whole feature: no part on the list means
/// no lot is ever marked. Adding a part opens the gate and removing one closes it again, but a part
/// already marked keeps every marking it has, because the shape and color are on the receipt and on
/// the floor rather than in this list.
/// </summary>
public sealed class ViewModel_Tool_LotMarkingTests
{
    private const string TestPartId = "TEST-21-28841";

    private static Model_Tool_LotMarkingScope CreatePartRow(
        int id,
        string partId,
        bool isEnabled = true
    ) => new() { Id = id, PartId = partId, IsEnabled = isEnabled };

    private static ViewModel_Tool_LotMarking CreateViewModel(
        IService_Tool_LotMarking service,
        IService_InforVisual? inforVisual = null
    ) =>
        new(
            service,
            new Mock<IService_ShipRecToolsSettings>().Object,
            inforVisual ?? new Mock<IService_InforVisual>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Mock<IService_Tool_LotMarking> CreateServiceReturningParts(
        params Model_Tool_LotMarkingScope[] parts
    )
    {
        var service = new Mock<IService_Tool_LotMarking>();
        service
            .Setup(s => s.GetEnabledPartsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(parts.ToList()));
        return service;
    }

    [Fact]
    public async Task SearchPartsCommand_ShouldLeaveThePartListEmpty_WhenNoPartIsEnabled()
    {
        var service = CreateServiceReturningParts();
        var viewModel = CreateViewModel(service.Object);

        await viewModel.SearchPartsCommand.ExecuteAsync(null);

        viewModel.Parts.Should().BeEmpty();
        viewModel.IsPartListEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task AddPartCommand_ShouldAddTheConfirmedPart_WhenExactlyOneCandidateMatches()
    {
        var service = CreateServiceReturningParts();
        service
            .Setup(s => s.AddPartAsync(TestPartId, It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        service
            .Setup(s => s.GetEnabledPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_LotMarkingScope>
                    {
                        CreatePartRow(1, TestPartId),
                    }
                )
            );

        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(s => s.FuzzySearchPartsAsync(TestPartId))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = TestPartId, Label = TestPartId },
                    }
                )
            );

        var viewModel = CreateViewModel(service.Object, inforVisual.Object);
        viewModel.PartSearchText = TestPartId;

        await viewModel.AddPartCommand.ExecuteAsync(null);

        service.Verify(s => s.AddPartAsync(TestPartId, It.IsAny<string>()), Times.Once);
        viewModel
            .Parts.Should()
            .ContainSingle(part => part.PartId == TestPartId && part.IsEnabled);
        viewModel.PartSearchText.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovePartRowAsync_ShouldDeactivateTheRowAndKeepTheRecordedMarkings_WhenTheRowIsRemoved()
    {
        var row = CreatePartRow(7, TestPartId);
        var recorded = new Model_Tool_LotMarking
        {
            Id = 3,
            HeatNumber = "TEST-HEAT-1001",
            HeatNumberNormalized = "TESTHEAT1001",
            ShapeCode = "⚡",
            ColorCode = "BLUE",
        };

        var service = CreateServiceReturningParts(row);
        service
            .Setup(s => s.GetAllMarkingsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking> { recorded }));
        service
            .Setup(s => s.SetPartEnabledAsync(row.Id, false))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(service.Object);
        await viewModel.SearchPartsCommand.ExecuteAsync(null);
        await viewModel.SearchMarkingsCommand.ExecuteAsync(null);

        await viewModel.RemovePartRowAsync(row);

        service.Verify(s => s.SetPartEnabledAsync(row.Id, false), Times.Once);
        service.Verify(s => s.RemovePartAsync(It.IsAny<int>()), Times.Never);
        viewModel
            .Markings.Should()
            .ContainSingle(marking => marking.Id == recorded.Id && marking.ShapeCode == "⚡");
    }
}
