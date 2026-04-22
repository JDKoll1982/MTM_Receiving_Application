using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents one generated Volvo label row queued for LabelView printing.
/// </summary>
public class Model_VolvoGeneratedLabelData
{
    public int Id { get; set; }

    public int ShipmentId { get; set; }

    public int ShipmentNumber { get; set; }

    public DateTime ShipmentDate { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int SkidNumber { get; set; }

    public int TotalSkids { get; set; }

    public string PartDescription { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string SkidDisplay => $"{SkidNumber} of {TotalSkids}";
}
