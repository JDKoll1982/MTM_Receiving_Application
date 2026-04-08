using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI model for one positive-quantity location row shown inside a material availability card.
/// </summary>
public class Model_Tool_MaterialAvailabilityLocation
{
    public string LocationId { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public bool IsSearchLocation { get; set; }

    public string LocationLabel => IsSearchLocation ? $"{LocationId} (selected)" : LocationId;

    public string QuantityDisplay => FormatWhole(Quantity);

    private static string FormatWhole(decimal value)
    {
        return decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0");
    }
}
