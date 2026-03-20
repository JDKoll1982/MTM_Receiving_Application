using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.Settings;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public class Service_ReceivingValidationTests
{
    private readonly Mock<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_InforVisual> _inforVisualService =
        new();
    private readonly Mock<IService_ReceivingSettings> _receivingSettings = new();

    public Service_ReceivingValidationTests()
    {
        _receivingSettings
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    key switch
                    {
                        ReceivingSettingsKeys.Validation.RequirePoNumber => true,
                        ReceivingSettingsKeys.Validation.RequireHeatLot => false,
                        ReceivingSettingsKeys.Validation.AllowNegativeQuantity => false,
                        ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo => false,
                        ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving => false,
                        _ => false,
                    }
            );

        _receivingSettings
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    key switch
                    {
                        ReceivingSettingsKeys.Validation.MinLoadCount => 1,
                        ReceivingSettingsKeys.Validation.MaxLoadCount => 99,
                        ReceivingSettingsKeys.Validation.MinQuantity => 0,
                        ReceivingSettingsKeys.Validation.MaxQuantity => 999999,
                        _ => 0,
                    }
            );
    }

    [Fact]
    public void ValidateReceivingLoad_PORowWithoutPOLine_ReturnsError()
    {
        var service = CreateSut();
        var load = CreateValidLoad();
        load.IsNonPOItem = false;
        load.PoNumber = "PO-123456";
        load.PoLineNumber = string.Empty;

        var result = service.ValidateReceivingLoad(load);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("PO part selection is required"));
    }

    [Fact]
    public void ValidateReceivingLoad_NonPORowWithoutPOFields_ReturnsSuccess()
    {
        var service = CreateSut();
        var load = CreateValidLoad();
        load.IsNonPOItem = true;
        load.PoNumber = null;
        load.PoLineNumber = string.Empty;

        var result = service.ValidateReceivingLoad(load);

        result.IsValid.Should().BeTrue();
    }

    private Service_ReceivingValidation CreateSut()
    {
        return new Service_ReceivingValidation(
            _inforVisualService.Object,
            _receivingSettings.Object,
            Options.Create(new InforVisualSettings { UseMockData = true })
        );
    }

    private static Model_ReceivingLoad CreateValidLoad()
    {
        return new Model_ReceivingLoad
        {
            LoadNumber = 1,
            PartID = "MMC0001000",
            PartType = "Coil",
            WeightQuantity = 10,
            PackagesPerLoad = 1,
            PackageTypeName = "Skid",
            HeatLotNumber = string.Empty,
            InitialLocation = "A-RECV-01",
        };
    }
}
