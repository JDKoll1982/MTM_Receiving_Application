using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_OutsideService.Data;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_OutsideService.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_OutsideService.Services;

public class Service_OutsideServiceTests
{
    [Fact]
    public async Task CreateRequestAsync_ShouldFail_WhenPackageRowsDoNotMatchPackageCount()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object);

        var request = new Model_OutsideServiceRequest
        {
            CreatedByUser = "tester",
            CreatedByDisplay = "Test User",
            Lines =
            {
                new Model_OutsideServiceRequestLine
                {
                    LineNumber = 1,
                    PartId = "PART-100",
                    PackageCount = 2,
                    Packages =
                    {
                        new Model_OutsideServiceRequestPackage
                        {
                            PackageSequence = 1,
                            PackageQuantity = 12,
                        },
                    },
                },
            },
        };

        var result = await service.CreateRequestAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("package rows do not match");
        inforVisualMock.Verify(mock => mock.PartExistsAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(
            mock => mock.LogInfo(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetVendorSuggestionsAsync_ShouldReturnDistinctVendorsOrderedByMostRecentDispatch()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object);

        var history = new List<Model_OutsideServiceHistory>
        {
            new()
            {
                VendorID = "V-100",
                VendorName = "Alpha Heat Treat",
                VendorCity = "Detroit",
                VendorState = "MI",
                DispatchDate = new DateTime(2026, 3, 20),
            },
            new()
            {
                VendorID = "V-200",
                VendorName = "Bravo Coating",
                VendorCity = "Toledo",
                VendorState = "OH",
                DispatchDate = new DateTime(2026, 3, 25),
            },
            new()
            {
                VendorID = "V-100",
                VendorName = "Alpha Heat Treat",
                VendorCity = "Detroit",
                VendorState = "MI",
                DispatchDate = new DateTime(2026, 3, 27),
            },
        };

        inforVisualMock
            .Setup(mock => mock.GetOutsideServiceHistoryByPartAsync("PART-100"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(history));

        var result = await service.GetVendorSuggestionsAsync("PART-100");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].VendorId.Should().Be("V-100");
        result.Data[0].DispatchCount.Should().Be(2);
        result.Data[0].LastDispatchDate.Should().Be(new DateTime(2026, 3, 27));
        result.Data[0].LocationDetail.Should().Be("Detroit, MI");
        result.Data[1].VendorId.Should().Be("V-200");
    }

    [Fact]
    public async Task ValidatePartAsync_ShouldSucceed_WhenInforVisualFailsAndMockModeIsEnabled()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object, useMockData: true);

        inforVisualMock
            .Setup(mock => mock.PartExistsAsync("PART-404"))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<bool>("Infor Visual unavailable"));

        var result = await service.ValidatePartAsync("PART-404");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        loggerMock.Verify(
            mock =>
                mock.LogWarning(
                    It.Is<string>(message => message.Contains("mock validation")),
                    It.IsAny<string?>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetVendorSuggestionsAsync_ShouldReturnMockSuggestions_WhenInforVisualFailsAndMockModeIsEnabled()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object, useMockData: true);

        inforVisualMock
            .Setup(mock => mock.GetOutsideServiceHistoryByPartAsync("PART-404"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                    "Infor Visual unavailable"
                )
            );

        var result = await service.GetVendorSuggestionsAsync("PART-404");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].VendorId.Should().Be("MOCK-VENDOR-001");
        result.Data[0].DispatchCount.Should().Be(3);
    }

    [Fact]
    public async Task SaveSetupAsync_ShouldFail_WhenPackageRowsDoNotMatchPackageCount()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object);

        var line = new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestLineId = 22,
            PartId = "PART-400",
            PackageCount = 2,
            SetupVendorName = "Acme Heat Treat",
            BOLNumber = "BOL-22",
            Packages =
            {
                new Model_OutsideServiceRequestPackage
                {
                    PackageSequence = 1,
                    PackageQuantity = 12,
                },
            },
        };

        var result = await service.SaveSetupAsync(line);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Package rows must match");
    }

    [Fact]
    public async Task SaveSetupAsync_ShouldFail_WhenAnyPackageQuantityIsInvalid()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var loggerMock = new Mock<IService_LoggingUtility>();
        var service = CreateService(inforVisualMock.Object, loggerMock.Object);

        var line = new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestLineId = 23,
            PartId = "PART-401",
            PackageCount = 2,
            SetupVendorName = "Acme Heat Treat",
            BOLNumber = "BOL-23",
            Packages =
            {
                new Model_OutsideServiceRequestPackage
                {
                    PackageSequence = 1,
                    PackageQuantity = 12,
                },
                new Model_OutsideServiceRequestPackage
                {
                    PackageSequence = 2,
                    PackageQuantity = 0,
                },
            },
        };

        var result = await service.SaveSetupAsync(line);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("greater than zero");
    }

    private static Service_OutsideService CreateService(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger,
        bool useMockData = false
    )
    {
        return new Service_OutsideService(
            new Dao_OutsideServiceRequest("Server=localhost;Database=test;Uid=test;Pwd=test;"),
            inforVisual,
            logger,
            Options.Create(new InforVisualSettings { UseMockData = useMockData })
        );
    }
}
