using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_OutsideServiceHistoryTests
{
    [Fact]
    public async Task SearchAsync_ShouldCombineMatchingVendorRows_WhenSearchingByPart()
    {
        var serviceMock = new Mock<IService_Tool_OutsideServiceHistory>();
        serviceMock
            .Setup(service => service.FuzzySearchPartsAsync("PART-1"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-1", Label = "PART-1" },
                    }
                )
            );
        serviceMock
            .Setup(service => service.GetHistoryByPartAsync("PART-1"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory>
                    {
                        new()
                        {
                            PartNumber = "PART-1",
                            VendorID = "V-100",
                            VendorName = "Vendor One",
                            DispatchID = "D-001",
                            DispatchDate = new System.DateTime(2026, 4, 1),
                            QuantitySent = 1.0m,
                            DispatchStatus = "Closed",
                        },
                        new()
                        {
                            PartNumber = "PART-1",
                            VendorID = "V-100",
                            VendorName = "Vendor One",
                            DispatchID = "D-002",
                            DispatchDate = new System.DateTime(2026, 4, 2),
                            QuantitySent = 2.0m,
                            DispatchStatus = "Closed",
                        },
                        new()
                        {
                            PartNumber = "PART-1",
                            VendorID = "V-200",
                            VendorName = "Vendor Two",
                            DispatchID = "D-003",
                            DispatchDate = new System.DateTime(2026, 4, 3),
                            QuantitySent = 7.0m,
                            DispatchStatus = "Open",
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object);
        viewModel.CanAggregateVendorNames.Should().BeTrue();
        viewModel.AggregateVendorNames = true;
        viewModel.SearchTerm = "PART-1";

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.Results.Should().HaveCount(2);

        var combinedVendorOne = viewModel
            .Results.Should()
            .ContainSingle(result => result.VendorName == "Vendor One")
            .Which;
        combinedVendorOne.IsCombinedRecord.Should().BeTrue();
        combinedVendorOne.DispatchID.Should().Be("Multiple");
        combinedVendorOne.DispatchDateDisplay.Should().Be("Combined Dates");
        combinedVendorOne.QuantitySentDisplay.Should().Be("3");

        var vendorTwo = viewModel
            .Results.Should()
            .ContainSingle(result => result.VendorName == "Vendor Two")
            .Which;
        vendorTwo.IsCombinedRecord.Should().BeFalse();
        vendorTwo.QuantitySentDisplay.Should().Be("7");
    }

    [Fact]
    public async Task SearchAsync_ShouldCombineMatchingVendorRows_WhenSearchingByVendor()
    {
        var serviceMock = new Mock<IService_Tool_OutsideServiceHistory>();
        serviceMock
            .Setup(service => service.FuzzySearchVendorsAsync("Vendor One"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-100", Label = "Vendor One" },
                    }
                )
            );
        serviceMock
            .Setup(service => service.GetPartsByVendorAsync("V-100"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-1", Label = "PART-1" },
                    }
                )
            );
        serviceMock
            .Setup(service => service.GetHistoryByVendorAndPartAsync("V-100", "PART-1"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory>
                    {
                        new()
                        {
                            PartNumber = "PART-1",
                            VendorID = "V-100",
                            VendorName = "Vendor One",
                            DispatchID = "D-010",
                            DispatchDate = new System.DateTime(2026, 4, 4),
                            QuantitySent = 3.0m,
                            DispatchStatus = "Closed",
                        },
                        new()
                        {
                            PartNumber = "PART-1",
                            VendorID = "V-100",
                            VendorName = "Vendor One",
                            DispatchID = "D-011",
                            DispatchDate = new System.DateTime(2026, 4, 5),
                            QuantitySent = 5.0m,
                            DispatchStatus = "Closed",
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object);
        viewModel.SetSearchByVendorCommand.Execute(null);
        viewModel.AggregateVendorNames = true;
        viewModel.SearchTerm = "Vendor One";

        await viewModel.SearchCommand.ExecuteAsync(null);

        var result = viewModel.Results.Should().ContainSingle().Which;
        result.IsCombinedRecord.Should().BeTrue();
        result.DispatchID.Should().Be("Multiple");
        result.DispatchDateDisplay.Should().Be("Combined Dates");
        result.QuantitySentDisplay.Should().Be("8");
    }

    private static ViewModel_Tool_OutsideServiceHistory CreateViewModel(
        IService_Tool_OutsideServiceHistory service
    )
    {
        return new ViewModel_Tool_OutsideServiceHistory(
            service,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}