namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// User-facing configuration for generating a dunnage book document.
/// </summary>
public class Model_Tool_DunnageBook_Config
{
    public const string DefaultCoverTitle = "Dunnage Reference Book";

    /// <summary>
    /// Title shown on the cover page and in the running page footer.
    /// </summary>
    public string CoverTitle { get; set; } = DefaultCoverTitle;

    /// <summary>
    /// When true, a cover page (title, logo, date, generated-by) is included.
    /// </summary>
    public bool IncludeCoverPage { get; set; } = true;

    /// <summary>
    /// When true, a table of contents page (grouped by type with page numbers) is included.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;
}
