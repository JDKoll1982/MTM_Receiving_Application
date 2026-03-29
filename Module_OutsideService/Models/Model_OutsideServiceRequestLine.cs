using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Represents a single Outside Service request line.
/// </summary>
public class Model_OutsideServiceRequestLine
{
    /// <summary>
    /// Gets or sets the line identifier.
    /// </summary>
    public int OutsideServiceRequestLineId { get; set; }

    /// <summary>
    /// Gets or sets the parent request identifier.
    /// </summary>
    public int OutsideServiceRequestId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable request number.
    /// </summary>
    public string RequestNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number within the request.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the exact Infor Visual part identifier.
    /// </summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of physical packages on the line.
    /// </summary>
    public int PackageCount { get; set; }

    /// <summary>
    /// Gets or sets the current line phase.
    /// </summary>
    public Enum_OutsideServiceLinePhase LinePhase { get; set; } =
        Enum_OutsideServiceLinePhase.Initialize;

    /// <summary>
    /// Gets or sets the request creator username.
    /// </summary>
    public string CreatedByUser { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request creator display name.
    /// </summary>
    public string CreatedByDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request created UTC timestamp.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets request-level notes.
    /// </summary>
    public string? RequestNotes { get; set; }

    /// <summary>
    /// Gets or sets the suggested vendor identifier when sourced from Infor Visual history.
    /// </summary>
    public string? SetupVendorId { get; set; }

    /// <summary>
    /// Gets or sets the selected or custom vendor name.
    /// </summary>
    public string? SetupVendorName { get; set; }

    /// <summary>
    /// Gets or sets the vendor source label.
    /// </summary>
    public string? SetupVendorSource { get; set; }

    /// <summary>
    /// Gets or sets the BOL number.
    /// </summary>
    public string? BOLNumber { get; set; }

    /// <summary>
    /// Gets or sets the scheduled ship timestamp.
    /// </summary>
    public DateTime? ScheduledShipUtc { get; set; }

    /// <summary>
    /// Gets or sets the Shipping contact.
    /// </summary>
    public string? ShippingContact { get; set; }

    /// <summary>
    /// Gets or sets Setup-phase notes.
    /// </summary>
    public string? SetupNotes { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// Gets or sets completion notes.
    /// </summary>
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// Gets or sets the physical package collection.
    /// </summary>
    public List<Model_OutsideServiceRequestPackage> Packages { get; set; } = new();

    /// <summary>
    /// Gets the slash-delimited package quantity summary.
    /// </summary>
    public string PackageSummary =>
        Packages.Count == 0
            ? string.Empty
            : string.Join(
                " / ",
                Packages
                    .OrderBy(package => package.PackageSequence)
                    .Select(package => package.PackageQuantity.ToString("0.####"))
            );

    /// <summary>
    /// Gets a multi-line package summary suitable for side panels.
    /// </summary>
    public string PackageSummaryMultiline =>
        Packages.Count == 0
            ? string.Empty
            : string.Join(
                Environment.NewLine,
                Packages
                    .OrderBy(package => package.PackageSequence)
                    .Select(package =>
                        $"Pkg {package.PackageSequence}   {package.PackageQuantity:0.####}"
                    )
            );

    /// <summary>
    /// Gets whether the line is in Initialize.
    /// </summary>
    public bool IsInitialize => LinePhase == Enum_OutsideServiceLinePhase.Initialize;

    /// <summary>
    /// Gets whether the line is in Setup.
    /// </summary>
    public bool IsSetup => LinePhase == Enum_OutsideServiceLinePhase.Setup;

    /// <summary>
    /// Gets whether the line is in Complete.
    /// </summary>
    public bool IsComplete => LinePhase == Enum_OutsideServiceLinePhase.Complete;

    /// <summary>
    /// Gets the line key shown in queue-style UI.
    /// </summary>
    public string QueueKey =>
        string.IsNullOrWhiteSpace(RequestNumber)
            ? $"Line {LineNumber}"
            : $"{RequestNumber}-{LineNumber}";

    /// <summary>
    /// Gets the package count as display text.
    /// </summary>
    public string PackageCountText => PackageCount.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the current line phase as display text.
    /// </summary>
    public string LinePhaseText => LinePhase.ToString();

    /// <summary>
    /// Gets the completion timestamp as display text.
    /// </summary>
    public string CompletedUtcText =>
        CompletedUtc?.ToLocalTime().ToString("MM/dd/yy h:mm tt", CultureInfo.InvariantCulture)
        ?? string.Empty;

    /// <summary>
    /// Gets the next action label for queue UI.
    /// </summary>
    public string NextStepLabel =>
        LinePhase == Enum_OutsideServiceLinePhase.Initialize ? "Open Setup"
        : LinePhase == Enum_OutsideServiceLinePhase.Setup ? "Mark Complete"
        : "View History";

    /// <summary>
    /// Gets the elapsed wait time label.
    /// </summary>
    public string WaitTimeLabel
    {
        get
        {
            var end = CompletedUtc ?? DateTime.UtcNow;
            var elapsed = end - CreatedUtc;
            if (elapsed.TotalMinutes < 1)
            {
                return "<1 min";
            }

            if (elapsed.TotalHours < 1)
            {
                return $"{Math.Floor(elapsed.TotalMinutes)} mins";
            }

            if (elapsed.TotalDays < 1)
            {
                return $"{Math.Floor(elapsed.TotalHours)} hrs";
            }

            return $"{Math.Floor(elapsed.TotalDays)} days";
        }
    }
}
