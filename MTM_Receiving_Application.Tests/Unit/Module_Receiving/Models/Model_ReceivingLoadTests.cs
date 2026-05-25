using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models;

public sealed class Model_ReceivingLoadTests
{
    [Fact]
    public void WeightPerPackageValueDisplay_ShouldUseCeilingAndUnitOfMeasure_WhenPackagesPerLoadChanges()
    {
        var load = new Model_ReceivingLoad { UnitOfMeasure = "ea", WeightQuantity = 10 };

        load.PackagesPerLoad = 3;

        load.WeightPerPackage.Should().Be(4);
        load.WeightPerPackageLabel.Should().Be("EA per Package");
        load.WeightPerPackageValueDisplay.Should().Be("4 EA");
    }

    [Fact]
    public void WeightPerPackageValueDisplay_ShouldUseUnitsFallback_WhenUnitOfMeasureMissing()
    {
        var load = new Model_ReceivingLoad
        {
            WeightQuantity = 9,
            PackagesPerLoad = 2,
            UnitOfMeasure = string.Empty,
        };

        load.WeightPerPackage.Should().Be(5);
        load.WeightPerPackageLabel.Should().Be("Units per Package");
        load.WeightPerPackageValueDisplay.Should().Be("5 Units");
    }

    [Fact]
    public void PoNumber_WhenBlanketOrderSuffixIsEntered_ShouldNormalizeToCanonicalUppercaseFormat()
    {
        var load = new Model_ReceivingLoad();

        load.PoNumber = "po-65421b";

        load.PoNumber.Should().Be("PO-065421B");
    }

    [Fact]
    public void HeatLotNumber_WhenLowercaseTextIsEntered_ShouldSaveAsTrimmedUppercase()
    {
        var load = new Model_ReceivingLoad();

        load.HeatLotNumber = "  heat-12ab  ";

        load.HeatLotNumber.Should().Be("HEAT-12AB");
    }
}
