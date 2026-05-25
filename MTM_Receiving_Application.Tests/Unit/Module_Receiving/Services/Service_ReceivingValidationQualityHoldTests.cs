using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_ReceivingValidationQualityHoldTests
{
    [Fact]
    public async Task IsRestrictedPartAsync_ShouldRespectExplicitMockQualityHoldFlags()
    {
        var inforVisualServiceMock = new Mock<IService_InforVisual>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var mockCatalogMock = new Mock<IService_InforVisualMockDataCatalog>();
        var appSettingsMock = new Mock<IService_AppSettings>();

        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);
        mockCatalogMock
            .Setup(service => service.GetCatalog())
            .Returns(
                new Model_InforVisualMockDataCatalog
                {
                    Parts =
                    [
                        new Model_InforVisualPart
                        {
                            PartID = "MMFQH0001",
                            RequiresQualityHold = true,
                            QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
                        },
                        new Model_InforVisualPart
                        {
                            PartID = "MMF0005007",
                            RequiresQualityHold = false,
                        },
                    ],
                    PurchaseOrders = new List<Model_InforVisualPO>(),
                }
            );

        var service = new Service_ReceivingValidation(
            inforVisualServiceMock.Object,
            receivingSettingsMock.Object,
            mockCatalogMock.Object,
            appSettingsMock.Object
        );

        var restrictedResult = await service.IsRestrictedPartAsync("MMFQH0001");
        var unrestrictedResult = await service.IsRestrictedPartAsync("MMF0005007");

        restrictedResult.IsRestricted.Should().BeTrue();
        restrictedResult.RestrictionType.Should().Be("Sheet Material - Quality Hold Required");
        unrestrictedResult.IsRestricted.Should().BeFalse();
        unrestrictedResult.RestrictionType.Should().BeEmpty();
    }
}
