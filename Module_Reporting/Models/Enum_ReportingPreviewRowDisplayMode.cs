namespace MTM_Receiving_Application.Module_Reporting.Models;

/// <summary>
/// Controls how reporting preview detail rows are shaped for display and copied output.
/// </summary>
public enum Enum_ReportingPreviewRowDisplayMode
{
    RawRows,
    UniquePartNumbersEntireDateRange,
    UniquePartNumbersAndLotNumbersEntireDateRange,
    UniquePartNumbersPerDay,
    UniquePartNumbersAndLotNumbersPerDay,
}