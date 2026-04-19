using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Prepared dialog state for the Volvo PO requisition email preview.
/// </summary>
public class Model_VolvoEmailPreviewDialog
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string DialogTitle { get; set; } = "PO Requisition Email Preview";

    /// <summary>
    /// Gets or sets the resolved TO recipients string.
    /// </summary>
    public string ToRecipients { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved CC recipients string.
    /// </summary>
    public string CcRecipients { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject shown in the preview.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional additional notes block.
    /// </summary>
    public string? AdditionalNotes { get; set; }

    /// <summary>
    /// Gets or sets the HTML document rendered in the embedded preview.
    /// </summary>
    public string PreviewHtmlDocument { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plain-text fallback shown if the web preview cannot be rendered.
    /// </summary>
    public string PlainTextPreview { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted report document used for clipboard export.
    /// </summary>
    public Model_FormattedReportDocument FormattedEmailDocument { get; set; } = new();
}
