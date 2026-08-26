namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Which lookup the Workbench Part input performs: a part number (source locations are
/// picked from a modal) or a location (parts stocked at that location are picked).
/// </summary>
public enum Enum_ScannerSearchMode
{
    PartNumber,
    Location,
}
