using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Reads the receiving tags waiting to be overlaid and prints the lot marking onto them.
/// </summary>
public interface IService_ReceivingLabelOverlay
{
    /// <summary>
    /// The tags currently in the label queue, each with the marking already recorded for its lot. Read
    /// only: nothing here creates or changes a marking.
    /// </summary>
    Task<Model_Dao_Result<List<Model_ReceivingLabelOverlayItem>>> GetOverlayCandidatesAsync();

    /// <summary>The printers this machine can print to.</summary>
    Task<Model_Dao_Result<List<string>>> GetPrinterNamesAsync();

    /// <summary>The paper sizes a printer offers, so the saved media can be recognised by the driver.</summary>
    Task<Model_Dao_Result<List<Model_LabelOverlayPaperOption>>> GetPaperSizesAsync(
        string? printerName
    );

    /// <summary>The trays a printer offers, so the saved one can be recognised by the driver.</summary>
    Task<Model_Dao_Result<List<Model_LabelOverlayPaperSourceOption>>> GetPaperSourcesAsync(
        string? printerName
    );

    /// <summary>
    /// Prints the marking for each chosen tag, one page per tag. Tags with no marking are skipped.
    /// </summary>
    /// <param name="progress">Told about each page as the printer takes it, so the screen can show progress.</param>
    Task<Model_Dao_Result> PrintOverlayAsync(
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    );

    /// <summary>
    /// Prints the cut-out card sheet: eight cards to a page, each with its part, marking and lot.
    /// </summary>
    /// <param name="progress">Told about each sheet as the printer takes it.</param>
    Task<Model_Dao_Result> PrintCardSheetAsync(
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    );

    /// <summary>
    /// Shows the tag pages in the preview window, where the operator can print them or close without
    /// printing. Runs on the calling thread, because it opens a window.
    /// </summary>
    /// <param name="ownerHandle">The window the preview belongs to.</param>
    /// <param name="progress">Told about each page as the printer takes it.</param>
    Model_Dao_Result<Model_LabelOverlayPreviewOutcome> PreviewOverlay(
        IntPtr ownerHandle,
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    );

    /// <summary>
    /// Shows the cut-out card sheets in the preview window, where the operator can print them or close
    /// without printing. Runs on the calling thread, because it opens a window.
    /// </summary>
    /// <param name="ownerHandle">The window the preview belongs to.</param>
    /// <param name="progress">Told about each sheet as the printer takes it.</param>
    Model_Dao_Result<Model_LabelOverlayPreviewOutcome> PreviewCardSheet(
        IntPtr ownerHandle,
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    );
}

