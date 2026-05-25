using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Helpers;

public sealed class Helper_VolvoGeneratedLabelDataBuilderTests
{
    [Fact]
    public void BuildRows_ShouldCreateOneRowPerSkid_WithQuantityPerSkidAndInforDescription()
    {
        var shipment = new Model_VolvoShipment
        {
            Id = 42,
            ShipmentNumber = 1007,
            ShipmentDate = new DateTime(2026, 4, 21),
        };

        var lines = new List<Model_VolvoShipmentLine>
        {
            new()
            {
                ShipmentId = 42,
                PartNumber = "V-EMB-1",
                QuantityPerSkid = 24,
                ReceivedSkidCount = 3,
                PartDescription = "Fallback Description",
            },
        };

        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["V-EMB-1"] = "Infor Description",
        };

        var rows = Helper_VolvoGeneratedLabelDataBuilder.BuildRows(shipment, lines, descriptions);

        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(row => row.Quantity == 24);
        rows.Should().OnlyContain(row => row.TotalSkids == 3);
        rows.Should().OnlyContain(row => row.PartDescription == "Infor Description");
        rows.Select(row => row.SkidNumber).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void BuildRows_ShouldFallBackToLineDescription_WhenInforDescriptionIsMissing()
    {
        var shipment = new Model_VolvoShipment
        {
            Id = 77,
            ShipmentNumber = 1008,
            ShipmentDate = new DateTime(2026, 4, 21),
        };

        var lines = new List<Model_VolvoShipmentLine>
        {
            new()
            {
                ShipmentId = 77,
                PartNumber = "V-EMB-2",
                QuantityPerSkid = 10,
                ReceivedSkidCount = 1,
                PartDescription = "Fallback Description",
            },
        };

        var rows = Helper_VolvoGeneratedLabelDataBuilder.BuildRows(
            shipment,
            lines,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        );

        rows.Should().ContainSingle();
        rows[0].PartDescription.Should().Be("Fallback Description");
    }
}
