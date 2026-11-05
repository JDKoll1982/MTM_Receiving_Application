namespace MTM_Receiving_Application.Module_Receiving.Enums;

/// <summary>
/// What the label overlay screen sends to the printer.
/// </summary>
public enum Enum_LabelOverlayPrintFormat
{
    /// <summary>Add the marking to tags that were already printed, one tag per page.</summary>
    TagOverlay = 0,

    /// <summary>A cut-out sheet of eight marking cards, four across and two down, on one page.</summary>
    CardSheet = 1,
}
