using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Helpers;

public sealed class Helper_VolvoShipmentCalculationsTests
{
    [Fact]
    public void CalculateRequestedComponentQuantity_ShouldUseParentSkidCountAndComponentQuantityOnly()
    {
        var component = new Model_VolvoPartComponent
        {
            ComponentPartNumber = "V-EMB-2",
            Quantity = 1,
            ComponentQuantityPerSkid = 20,
        };

        var requestedQuantity =
            Helper_VolvoShipmentCalculations.CalculateRequestedComponentQuantity(3, component);

        requestedQuantity.Should().Be(3);
    }

    [Fact]
    public void CalculateRequestedComponentQuantity_ShouldMultiplyByComponentInclusionCount()
    {
        var component = new Model_VolvoPartComponent
        {
            ComponentPartNumber = "V-EMB-26",
            Quantity = 3,
            ComponentQuantityPerSkid = 32,
        };

        var requestedQuantity =
            Helper_VolvoShipmentCalculations.CalculateRequestedComponentQuantity(2, component);

        requestedQuantity.Should().Be(6);
    }
}
