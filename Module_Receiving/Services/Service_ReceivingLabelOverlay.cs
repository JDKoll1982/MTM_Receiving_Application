using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Reads the tags in the label queue and prints a lot marking onto each one.
/// </summary>
/// <remarks>
/// The marking comes from the shape and color codes already stored on the queue row, which were written
/// when the receipt was saved. Nothing here recomputes a marking or writes a record: printing an overlay
/// is a repeat of something already decided, so it must not be able to invent a different answer.
/// </remarks>
public sealed class Service_ReceivingLabelOverlay : IService_ReceivingLabelOverlay
{
    private readonly Dao_ReceivingLabelData _labelDataDao;
    private readonly IService_LoggingUtility _logger;

    public Service_ReceivingLabelOverlay(
        Dao_ReceivingLabelData labelDataDao,
        IService_LoggingUtility logger
    )
    {
        _labelDataDao = labelDataDao ?? throw new ArgumentNullException(nameof(labelDataDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLabelOverlayItem>>> GetOverlayCandidatesAsync()
    {
        var result = await _labelDataDao.GetCurrentLabelDataAsync();
        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLabelOverlayItem>>(
                result.ErrorMessage,
                result.Exception
            );
        }

        var items = (result.Data ?? new List<Model_ReceivingLoad>())
            .Select(load =>
            {
                var marking = Helper_LabelOverlayPrinter.DescribeMarking(
                    load.HeatColorCode,
                    load.HeatShapeCode
                );

                return new Model_ReceivingLabelOverlayItem
                {
                    PartId = load.PartID,
                    Description = load.PartDescription,
                    HeatLotNumber = load.HeatLotNumber,
                    LabelNumber = load.LoadNumber,
                    ReceivedDate = load.ReceivedDate,
                    ShapeCode = load.HeatShapeCode,
                    ColorCode = load.HeatColorCode,
                    MarkingText = marking.Length > 0 ? marking : "no marking recorded",
                };
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(items);
    }

    public Task<Model_Dao_Result<List<string>>> GetPrinterNamesAsync()
    {
        try
        {
            var printers = Helper_LabelOverlayPrinter.GetInstalledPrinters().ToList();

            return Task.FromResult(
                printers.Count == 0
                    ? Model_Dao_Result_Factory.Failure<List<string>>(
                        "No printers are installed on this machine."
                    )
                    : Model_Dao_Result_Factory.Success(printers)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "The installed printers could not be listed.",
                ex,
                nameof(Service_ReceivingLabelOverlay)
            );

            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<List<string>>(
                    $"The installed printers could not be listed: {ex.Message}",
                    ex
                )
            );
        }
    }

    public Task<Model_Dao_Result<List<Model_LabelOverlayPaperOption>>> GetPaperSizesAsync(
        string? printerName
    )
    {
        var papers = Helper_LabelOverlayPrinter.GetPaperSizes(printerName).ToList();

        return Task.FromResult(
            papers.Count == 0
                ? Model_Dao_Result_Factory.Failure<List<Model_LabelOverlayPaperOption>>(
                    string.IsNullOrWhiteSpace(printerName)
                        ? "Choose a printer to see the paper sizes it takes."
                        : $"The printer '{printerName}' did not report any paper sizes."
                )
                : Model_Dao_Result_Factory.Success(papers)
        );
    }

    public async Task<Model_Dao_Result> PrintOverlayAsync(
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(printSettings);

        var markings = items
            .Where(item => item.IsSelected)
            .Select(item => (item.ShapeCode, item.ColorCode))
            .ToList();

        // Spooling a page at print resolution takes seconds. Run on the thread pool so the dialog keeps
        // animating instead of freezing until the driver is done with it.
        var result = await Task.Run(() =>
        {
            _logger.LogInfo(
                Helper_LabelOverlayPrinter.DescribePageSetup(printerName, printSettings),
                nameof(Service_ReceivingLabelOverlay)
            );

            return Helper_LabelOverlayPrinter.Print(
                printerName,
                markings,
                geometry,
                printSettings,
                progress
            );
        });

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                $"Label overlay was not printed: {result.ErrorMessage}",
                nameof(Service_ReceivingLabelOverlay)
            );
        }

        return result;
    }

    public async Task<Model_Dao_Result> PrintCardSheetAsync(
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(printSettings);

        var cards = items
            .Where(item => item.IsSelected)
            .Select(item => new Model_LabelOverlayCard(
                item.PartId,
                item.HeatLotNumber,
                item.ShapeCode,
                item.ColorCode
            ))
            .ToList();

        var result = await Task.Run(() =>
        {
            _logger.LogInfo(
                Helper_LabelOverlayPrinter.DescribePageSetup(printerName, printSettings),
                nameof(Service_ReceivingLabelOverlay)
            );

            return Helper_LabelOverlayPrinter.PrintCardSheet(
                printerName,
                cards,
                printSettings,
                progress
            );
        });

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                $"The marking card sheet was not printed: {result.ErrorMessage}",
                nameof(Service_ReceivingLabelOverlay)
            );
        }

        return result;
    }

    public Task<Model_Dao_Result<List<Model_LabelOverlayPaperSourceOption>>> GetPaperSourcesAsync(
        string? printerName
    )
    {
        var sources = Helper_LabelOverlayPrinter.GetPaperSources(printerName).ToList();
        return Task.FromResult(
            sources.Count == 0
                ? Model_Dao_Result_Factory.Failure<List<Model_LabelOverlayPaperSourceOption>>(
                    string.IsNullOrWhiteSpace(printerName)
                        ? "Choose a printer to see the trays it takes."
                        : $"The printer '{printerName}' did not report any trays."
                )
                : Model_Dao_Result_Factory.Success(sources)
        );
    }

    public Model_Dao_Result<Model_LabelOverlayPreviewOutcome> PreviewOverlay(
        IntPtr ownerHandle,
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(printSettings);

        var markings = items
            .Where(item => item.IsSelected)
            .Select(item => (item.ShapeCode, item.ColorCode))
            .ToList();

        _logger.LogInfo(
            Helper_LabelOverlayPrinter.DescribePageSetup(printerName, printSettings),
            nameof(Service_ReceivingLabelOverlay)
        );

        var created = Helper_LabelOverlayPrinter.CreateTagDocument(
            printerName,
            markings,
            geometry,
            printSettings,
            progress
        );

        return ShowPreview(ownerHandle, created, printSettings);
    }

    public Model_Dao_Result<Model_LabelOverlayPreviewOutcome> PreviewCardSheet(
        IntPtr ownerHandle,
        string printerName,
        IReadOnlyList<Model_ReceivingLabelOverlayItem> items,
        Model_LabelOverlayPrintSettings printSettings,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(printSettings);

        var cards = items
            .Where(item => item.IsSelected)
            .Select(item => new Model_LabelOverlayCard(
                item.PartId,
                item.HeatLotNumber,
                item.ShapeCode,
                item.ColorCode
            ))
            .ToList();

        _logger.LogInfo(
            Helper_LabelOverlayPrinter.DescribePageSetup(printerName, printSettings),
            nameof(Service_ReceivingLabelOverlay)
        );

        var created = Helper_LabelOverlayPrinter.CreateCardSheetDocument(
            printerName,
            cards,
            printSettings,
            progress
        );

        return ShowPreview(ownerHandle, created, printSettings);
    }

    /// <summary>
    /// Shows a built job and reports what the operator did with it. The window opens on the calling
    /// thread, so this must not be marshalled onto a worker.
    /// </summary>
    private Model_Dao_Result<Model_LabelOverlayPreviewOutcome> ShowPreview(
        IntPtr ownerHandle,
        (PrintDocument? Document, Model_Dao_Result? Failure) created,
        Model_LabelOverlayPrintSettings printSettings
    )
    {
        if (created.Document is null)
        {
            _logger.LogWarning(
                $"The preview could not be built: {created.Failure?.ErrorMessage}",
                nameof(Service_ReceivingLabelOverlay)
            );

            return Model_Dao_Result_Factory.Failure<Model_LabelOverlayPreviewOutcome>(
                created.Failure?.ErrorMessage ?? "The preview could not be built.",
                created.Failure?.Exception
            );
        }

        using var document = created.Document;

        var outcome = Helper_LabelOverlayPrintPreview.Show(ownerHandle, document, printSettings);

        if (outcome.Error is not null)
        {
            _logger.LogWarning(
                $"The job was not printed from the preview: {outcome.Error}",
                nameof(Service_ReceivingLabelOverlay)
            );

            return Model_Dao_Result_Factory.Failure<Model_LabelOverlayPreviewOutcome>(outcome.Error);
        }

        return Model_Dao_Result_Factory.Success(outcome);
    }
}
