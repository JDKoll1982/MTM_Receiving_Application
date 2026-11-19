using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

/// <summary>
/// The PO status filter is a set of ticked statuses rather than one choice, so the stored value is a
/// comma-separated list that has to become a comma-separated list of SQL codes. Each status the
/// clerk ticks has to reach the query, or the filter would silently narrow to one of them.
/// </summary>
public sealed class Service_Tool_POLineSpecSearchStatusFilterTests
{
    [Fact]
    public async Task SearchAsync_ShouldPassEveryTickedStatus_AsCodes()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_InforVisualPOLineSpecSearchRow>()));

        var service = CreateService(inforVisual);

        await service.SearchAsync(
            "pump seal",
            new Model_Tool_POLineSpecSearchOptions { PoStatusFilter = "Firmed,Released" }
        );

        inforVisual.Verify(
            candidate =>
                candidate.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    "F,R"
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SearchAsync_ShouldPassNoStatusFilter_WhenTheSelectionIsEmpty()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_InforVisualPOLineSpecSearchRow>()));

        var service = CreateService(inforVisual);

        await service.SearchAsync(
            "pump seal",
            new Model_Tool_POLineSpecSearchOptions { PoStatusFilter = string.Empty }
        );

        inforVisual.Verify(
            candidate =>
                candidate.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    string.Empty
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("Firmed,Released,Closed,Cancelled/Void", "F,R,C,V")]
    [InlineData("Released", "R")]
    [InlineData("Cancelled/Void", "V")]
    [InlineData("All", "")]
    [InlineData("NotAStatus", "")]
    public async Task SearchAsync_ShouldMapTheStoredSelection_ToTheExpectedCodes(
        string stored,
        string expectedCodes
    )
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_InforVisualPOLineSpecSearchRow>()));

        var service = CreateService(inforVisual);

        await service.SearchAsync(
            "pump seal",
            new Model_Tool_POLineSpecSearchOptions { PoStatusFilter = stored }
        );

        inforVisual.Verify(
            candidate =>
                candidate.SearchPurchaseOrderLineSpecsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    expectedCodes
                ),
            Times.Once
        );
    }

    [Fact]
    public void BuildPoStatusFilter_ShouldKeepTheOptionOrder_RegardlessOfTickOrder()
    {
        var stored = Model_Tool_POLineSpecSearchOptions.BuildPoStatusFilter(
            ["Released", "Firmed"]
        );

        stored.Should().Be("Firmed,Released");
    }

    [Fact]
    public void ParsePoStatusFilters_ShouldIgnoreUnknownNames_SoAStaleSaveCannotHideEveryRow()
    {
        Model_Tool_POLineSpecSearchOptions
            .ParsePoStatusFilters("Released, Retired, Closed")
            .Should()
            .Equal("Released", "Closed");
    }

    private static Service_Tool_POLineSpecSearch CreateService(
        Mock<IService_InforVisual> inforVisual
    ) => new(inforVisual.Object, new Mock<IService_LoggingUtility>().Object);
}
