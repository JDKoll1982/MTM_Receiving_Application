using System.Globalization;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

public class Model_ReportSummaryRow
{
    public string Label { get; set; } = string.Empty;

    public int RecordCount { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalWeightLbs { get; set; }

    public int TotalPackages { get; set; }

    public string DisplayRecordCount => RecordCount.ToString("N0", CultureInfo.InvariantCulture);

    public string DisplayTotalQuantity => TotalQuantity.ToString("0.##", CultureInfo.InvariantCulture);

    public string DisplayTotalWeightLbs => TotalWeightLbs.ToString("0.##", CultureInfo.InvariantCulture);

    public string DisplayTotalPackages => TotalPackages.ToString("N0", CultureInfo.InvariantCulture);
}