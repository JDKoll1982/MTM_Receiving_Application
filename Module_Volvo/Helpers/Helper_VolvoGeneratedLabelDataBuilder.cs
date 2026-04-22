using System.Collections.Generic;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Helpers;

/// <summary>
/// Expands saved Volvo shipment lines into one generated-label row per skid.
/// </summary>
public static class Helper_VolvoGeneratedLabelDataBuilder
{
    public static List<Model_VolvoGeneratedLabelData> BuildRows(
        Model_VolvoShipment shipment,
        IReadOnlyCollection<Model_VolvoShipmentLine> lines,
        IReadOnlyDictionary<string, string> descriptionsByPartNumber
    )
    {
        var rows = new List<Model_VolvoGeneratedLabelData>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.PartNumber) || line.ReceivedSkidCount <= 0)
            {
                continue;
            }

            descriptionsByPartNumber.TryGetValue(line.PartNumber, out var description);
            var resolvedDescription = string.IsNullOrWhiteSpace(description)
                ? line.PartDescription?.Trim() ?? string.Empty
                : description.Trim();

            for (var skidNumber = 1; skidNumber <= line.ReceivedSkidCount; skidNumber++)
            {
                rows.Add(
                    new Model_VolvoGeneratedLabelData
                    {
                        ShipmentId = shipment.Id,
                        ShipmentNumber = shipment.ShipmentNumber,
                        ShipmentDate = shipment.ShipmentDate.Date,
                        PartNumber = line.PartNumber,
                        Quantity = line.QuantityPerSkid,
                        SkidNumber = skidNumber,
                        TotalSkids = line.ReceivedSkidCount,
                        PartDescription = resolvedDescription,
                    }
                );
            }
        }

        return rows;
    }
}
