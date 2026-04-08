namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a suggested destination location for the current guided receiving part.
/// </summary>
public sealed class Model_ReceivingRecommendedLocation
{
    public string WarehouseId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public decimal QuantityOnHand { get; set; }

    public string ReasonText { get; set; } = string.Empty;

    public string DisplayLocation =>
        string.IsNullOrWhiteSpace(WarehouseId) ? LocationId : $"{WarehouseId}/{LocationId}";

    public string QuantityDisplay => $"{QuantityOnHand:N2} on hand";
}
