using System;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

public class Model_ReportRow
{
    public string Id { get; set; } = string.Empty;

    public string? PONumber { get; set; }

    public string? PartNumber { get; set; }

    public string? PartDescription { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? WeightLbs { get; set; }

    public string? HeatLotNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string? EmployeeNumber { get; set; }

    public string? CreatedByUsername { get; set; }

    public string? DunnageType { get; set; }

    public string? SpecsCombined { get; set; }

    public int? ShipmentNumber { get; set; }

    public string? ReceiverNumber { get; set; }

    public string? Status { get; set; }

    public int? PartCount { get; set; }
}
