using System;
using System.Globalization;

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

    public string? Location { get; set; }

    public string? Notes { get; set; }

    public int? LoadNumber { get; set; }

    public int? LabelNumber { get; set; }

    public int? PackagesPerLoad { get; set; }

    public string? PackageTypeName { get; set; }

    public int? CoilsOnSkid { get; set; }

    public int? QuantityPerSkid { get; set; }

    public int? ReceivedSkidCount { get; set; }

    public string DisplayPo => PONumber?.Trim() ?? string.Empty;

    public string DisplayPartOrDunnage
    {
        get
        {
            if (SourceModule.Equals("Dunnage", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(PartNumber) && !string.IsNullOrWhiteSpace(DunnageType))
                {
                    return $"{PartNumber} / {DunnageType}";
                }

                return PartNumber?.Trim() ?? DunnageType?.Trim() ?? string.Empty;
            }

            return PartNumber?.Trim() ?? DunnageType?.Trim() ?? string.Empty;
        }
    }

    public string DisplayQuantity => Quantity?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;

    public string DisplayLocation => Location?.Trim() ?? string.Empty;

    public string DisplayNotes => Notes?.Trim() ?? string.Empty;

    public string DisplayLoadsOrSkids
    {
        get
        {
            if (SourceModule.Equals("Volvo", StringComparison.OrdinalIgnoreCase))
            {
                return ReceivedSkidCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }

            if (SourceModule.Equals("Receiving", StringComparison.OrdinalIgnoreCase))
            {
                var value = LoadNumber ?? LabelNumber;
                return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return string.Empty;
        }
    }

    public string DisplayUnitsPerSkid
    {
        get
        {
            if (SourceModule.Equals("Receiving", StringComparison.OrdinalIgnoreCase))
            {
                if (CoilsOnSkid is > 0)
                {
                    return $"{CoilsOnSkid.Value.ToString(CultureInfo.InvariantCulture)} coils/skid";
                }

                if (PackagesPerLoad is > 0 && !string.IsNullOrWhiteSpace(PackageTypeName))
                {
                    return $"{PackagesPerLoad.Value.ToString(CultureInfo.InvariantCulture)} {PackageTypeName.Trim()}/load";
                }

                if (PackagesPerLoad is > 0)
                {
                    return $"{PackagesPerLoad.Value.ToString(CultureInfo.InvariantCulture)} pcs/load";
                }

                return PackageTypeName?.Trim() ?? string.Empty;
            }

            if (SourceModule.Equals("Volvo", StringComparison.OrdinalIgnoreCase))
            {
                return QuantityPerSkid is > 0
                    ? $"{QuantityPerSkid.Value.ToString(CultureInfo.InvariantCulture)} pcs/skid"
                    : string.Empty;
            }

            return string.Empty;
        }
    }

    public bool HasPo => !string.IsNullOrWhiteSpace(DisplayPo);

    public bool HasLocation => !string.IsNullOrWhiteSpace(DisplayLocation);

    public bool HasNotes => !string.IsNullOrWhiteSpace(DisplayNotes);

    public bool HasLoadsOrSkids => !string.IsNullOrWhiteSpace(DisplayLoadsOrSkids);

    public bool HasUnitsPerSkid => !string.IsNullOrWhiteSpace(DisplayUnitsPerSkid);
}
