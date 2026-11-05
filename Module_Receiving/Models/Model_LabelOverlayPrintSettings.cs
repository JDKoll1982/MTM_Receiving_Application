using System;
using MTM_Receiving_Application.Module_Receiving.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// The print setup the label overlay remembers between sessions: which printer, which way round the
/// media goes, which paper, and how much room the cut-out cards leave for trimming.
/// </summary>
/// <remarks>
/// This is kept in a local file rather than the settings database because the values describe this
/// PC's printer, not the person using it. A second machine with a different printer needs its own
/// answer, and a printer that has been swapped out should not be remembered for the whole site.
/// </remarks>
public sealed class Model_LabelOverlayPrintSettings
{
    /// <summary>The short edge of a US Letter sheet, which is what the receiving tag is printed on.</summary>
    public const float DefaultPaperWidthInches = 8.5f;

    /// <summary>The long edge of a US Letter sheet.</summary>
    public const float DefaultPaperHeightInches = 11f;

    /// <summary>
    /// Room left around the cut-out cards so a printer that cannot print to the edge still produces
    /// cards whose part number and lot survive the trip through the paper path.
    /// </summary>
    public const float DefaultCardSheetMarginInches = 0.25f;

    /// <summary>One copy unless the operator asks for more.</summary>
    public const int DefaultCopies = 1;

    /// <summary>The printer the overlay was last sent to.</summary>
    public string PrinterName { get; set; } = string.Empty;

    /// <summary>Whether the last run produced tags or a cut-out card sheet.</summary>
    public Enum_LabelOverlayPrintFormat Format { get; set; } = Enum_LabelOverlayPrintFormat.TagOverlay;

    /// <summary>
    /// True to lay the media out a quarter turn — the tag artwork is 11 inches across, so this is the
    /// setting that makes it print the right way round.
    /// </summary>
    public bool Landscape { get; set; } = true;

    /// <summary>The paper name as the driver reported it, for showing a familiar label back to the user.</summary>
    public string PaperName { get; set; } = "Letter";

    /// <summary>The paper's short edge, in inches, as the driver reports it.</summary>
    public float PaperWidthInches { get; set; } = DefaultPaperWidthInches;

    /// <summary>The paper's long edge, in inches, as the driver reports it.</summary>
    public float PaperHeightInches { get; set; } = DefaultPaperHeightInches;

    /// <summary>
    /// The tray this output feeds from, as the driver names it. Kept by name and by the driver's own
    /// number, so the picker can be restored and the job can ask for the tray by either.
    /// </summary>
    /// <remarks>
    /// This is held here rather than only inside the driver settings block because several drivers — the
    /// receiving room's Konica among them — ignore the tray when a saved block is handed back to them, so
    /// the tray has to be applied through the page settings to be sure the job feeds from it.
    /// </remarks>
    public string PaperSourceName { get; set; } = string.Empty;

    /// <summary>The driver's number for <see cref="PaperSourceName"/>, or 0 when no tray is named.</summary>
    public short PaperSourceRawKind { get; set; }

    /// <summary>Room left around the cut-out cards, in inches.</summary>
    public float CardSheetMarginInches { get; set; } = DefaultCardSheetMarginInches;

    /// <summary>
    /// How many times every printed page is repeated.
    /// </summary>
    /// <remarks>
    /// A cut-out sheet reads this as duplicates per lot: one full page of eight identical cards for each
    /// selected lot, printed this many times. Two lots duplicated three times give six sheets, never a
    /// sheet mixing two lots. The tag output reads it as copies of each tag page.
    /// </remarks>
    public int Copies { get; set; } = DefaultCopies;

    /// <summary>
    /// The printer's own settings for this output — tray, media, duplex, quality — as a <c>DEVMODE</c>
    /// block, base64 encoded.
    /// </summary>
    /// <remarks>
    /// Kept per output rather than per printer so the overlay can run from one tray while the cut-out
    /// cards run from another. It is applied to the job only, never to the printer's saved defaults, so
    /// nothing chosen here leaks into another program or another user's printing.
    /// </remarks>
    public string DevModeBase64 { get; set; } = string.Empty;

    /// <summary>The saved driver settings, or <c>null</c> when this output uses the printer's own.</summary>
    public byte[]? GetDevMode() =>
        string.IsNullOrWhiteSpace(DevModeBase64)
            ? null
            : Convert.FromBase64String(DevModeBase64);

    /// <summary>Keeps a driver settings block, or clears it when given nothing.</summary>
    public void SetDevMode(byte[]? devMode) =>
        DevModeBase64 = devMode is { Length: > 0 } ? Convert.ToBase64String(devMode) : string.Empty;

    /// <summary>The most copies, or the most duplicates per lot, the overlay will make in one run.</summary>
    public const int MaximumCopies = 50;

    /// <summary>The copies to ask the printer for, held to the range the screen allows.</summary>
    public int ResolveCopies() => Math.Clamp(Copies, 1, MaximumCopies);

    /// <summary>A setup that matches the tag artwork: Letter, landscape, cards inset for trimming.</summary>
    public static Model_LabelOverlayPrintSettings CreateDefault() => new();

    /// <summary>
    /// The page as the printer will lay it out, in inches. The media is described by its short and long
    /// edges, and a landscape setting turns it a quarter turn, so an 8.5 x 11 sheet prints 11 inches
    /// across and 8.5 inches down — the space the tag's measured circle position assumes.
    /// </summary>
    public (float Width, float Height) ResolvePageSize()
    {
        var shortEdge = Math.Min(PaperWidthInches, PaperHeightInches);
        var longEdge = Math.Max(PaperWidthInches, PaperHeightInches);

        if (shortEdge <= 0f || longEdge <= 0f)
        {
            shortEdge = DefaultPaperWidthInches;
            longEdge = DefaultPaperHeightInches;
        }

        return Landscape ? (longEdge, shortEdge) : (shortEdge, longEdge);
    }

    /// <summary>A plain reading of the setup, so the advanced panel can show it without being opened.</summary>
    public string Describe()
    {
        var page = ResolvePageSize();

        var tray = string.IsNullOrWhiteSpace(PaperSourceName) ? string.Empty : $" · {PaperSourceName}";

        return $"{PaperName} · {(Landscape ? "Landscape" : "Portrait")} · "
            + $"{page.Width:0.##}\" x {page.Height:0.##}\"{tray}";
    }
}
