using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Probes;

/// <summary>
/// Catalogue of the Infor VISUAL Purchase Receipt Entry automation probes. Every entry becomes one
/// collapsible card on the page.
/// </summary>
public static class VisualProbeCatalog
{
    /// <summary>Attach to the running window.</summary>
    public const string Attach = "P01_Attach";

    /// <summary>Enumerate the controls the window exposes.</summary>
    public const string ControlSurface = "P02_ControlSurface";

    /// <summary>Read the record identity fields.</summary>
    public const string IdentityFields = "P03_IdentityFields";

    /// <summary>Read the date and party fields.</summary>
    public const string DateAndPartyFields = "P04_DateAndPartyFields";

    /// <summary>Read the reference fields the macro variable list also exposes.</summary>
    public const string ReferenceFields = "P05_ReferenceFields";

    /// <summary>Report which macro variables have a visible control.</summary>
    public const string VariableCoverage = "P06_VariableCoverage";

    /// <summary>Type into a field, read it back, and restore it.</summary>
    public const string FieldWriteRestore = "P07_FieldWriteRestore";

    /// <summary>Round-trip a value containing punctuation and mixed case.</summary>
    public const string RoundTripFidelity = "P08_RoundTripFidelity";

    /// <summary>Prove the run never saves anything.</summary>
    public const string NoSaveGuard = "P09_NoSaveGuard";

    /// <summary>Prove the window is left exactly as it was found.</summary>
    public const string RestoreIntegrity = "P10_RestoreIntegrity";

    private static readonly IReadOnlyList<Model_VisualProbeDefinition> _allProbes =
    [
        new Model_VisualProbeDefinition
        {
            Id = Attach,
            Title = "Attach to the Purchase Receipt Entry window",
            Category = "Connection",
            Purpose =
                "Confirms the app can find the live VMRCVENT.EXE window and read it with UI Automation. "
                + "This is the equivalent of a macro being handed the receipt screen.",
            UiSteps =
                "Nothing on screen. The app asks Windows for the running VMRCVENT process, its window "
                + "handle, and its screen rectangle.",
            Expected = "The window is found, titled 'Purchase Receipt Entry - Infor VISUAL - MTMFG', and readable.",
            ReportedKeys = [],
        },
        new Model_VisualProbeDefinition
        {
            Id = ControlSurface,
            Title = "Enumerate the controls the window exposes",
            Category = "Connection",
            Purpose =
                "Shows how many automation elements the Gupta window publishes and how many of them are "
                + "field values the app can read.",
            UiSteps = "Nothing on screen. The app walks every automation element under the window.",
            Expected = "At least 30 elements are readable and the same count is reported twice in a row.",
            ReportedKeys = [],
        },
        new Model_VisualProbeDefinition
        {
            Id = IdentityFields,
            Title = "Read the receipt identity fields",
            Category = "Read access",
            Purpose =
                "Reads ORDER_ID, SITE_ID and RECEIVER_ID, the three variables that identify the open "
                + "receipt in the macro variable list.",
            UiSteps = "No change. The app reads the Order ID, Site ID and Receiver ID boxes.",
            Expected = "Order ID and Site ID both come back with a value.",
            ReportedKeys = ["ORDER_ID", "SITE_ID", "RECEIVER_ID"],
        },
        new Model_VisualProbeDefinition
        {
            Id = DateAndPartyFields,
            Title = "Read the date and party fields",
            Category = "Read access",
            Purpose =
                "Reads the date and trading-partner variables: the receive and promise dates plus the "
                + "buyer, FOB, ship via and carrier fields.",
            UiSteps =
                "No change. The app reads Receive on, Promise Delivery Date, Promise Ship Date, Buyer, FOB, "
                + "Ship via and Carrier ID.",
            Expected = "The receive date and buyer come back with values.",
            ReportedKeys =
            [
                "DESIRED_RECV_DATE",
                "PROMISE_DATE",
                "PROMISE_SHIP_DATE",
                "BUYER",
                "FREE_ON_BOARD",
                "SHIP_VIA",
                "CARRIER_ID",
            ],
        },
        new Model_VisualProbeDefinition
        {
            Id = ReferenceFields,
            Title = "Read the reference document fields",
            Category = "Read access",
            Purpose =
                "Reads the free-text reference variables: bill of lading, vendor packlist, packlist date "
                + "and vendor freight bill. These are the fields the write probes use.",
            UiSteps =
                "No change. The app reads Bill of Lading ID, Vendor Packlist ID, Vendor Packlist Date and "
                + "Vendor Freight Bill ID.",
            Expected = "Every reference field resolves to a control, even when the value is empty.",
            ReportedKeys = ["BOL_ID", "VEND_PACK_ID", "VEND_PACK_DATE", "VEND_FREIGHT_ID"],
        },
        new Model_VisualProbeDefinition
        {
            Id = VariableCoverage,
            Title = "Map the macro variables to the visible window",
            Category = "Read access",
            Purpose =
                "Compares the full variable list the VISUAL macro editor offers against the controls that "
                + "actually exist on the window, so it is clear which variables need the macro engine and "
                + "which the app can handle directly.",
            UiSteps =
                "Nothing on screen. The app resolves every mapped variable and counts the rest as not "
                + "present on the visible surface.",
            Expected = "Every mapped variable is found and the totals add up to the full variable list.",
            ReportedKeys = [],
        },
        new Model_VisualProbeDefinition
        {
            Id = FieldWriteRestore,
            Title = "Type into a field, read it back, restore it",
            Category = "Write access",
            Purpose =
                "The core macro-equivalent action: change a field value, confirm the window accepted it, "
                + "then put the original value back.",
            UiSteps =
                "Watch the Bill of Lading ID box. It briefly shows MTM-PROBE-&lt;time&gt;, is read back, then "
                + "is restored to its original text. Nothing is saved.",
            Expected = "The typed value is observed on screen and the original value is restored exactly.",
            WritesToWindow = true,
            ReportedKeys = ["BOL_ID"],
        },
        new Model_VisualProbeDefinition
        {
            Id = RoundTripFidelity,
            Title = "Verify exact round-trip of typed text",
            Category = "Write access",
            Purpose =
                "Proves the app can type arbitrary text - mixed case, digits, hyphens and slashes - and "
                + "that VISUAL keeps it exactly, which is what a macro needs when it writes a value.",
            UiSteps =
                "Watch the Vendor Packlist ID box. It receives MTM-PROBE-a1B2/3-4, is read back, and is "
                + "restored to its original text.",
            Expected = "The read-back text is character-for-character identical to what was typed.",
            WritesToWindow = true,
            ReportedKeys = ["VEND_PACK_ID"],
        },
        new Model_VisualProbeDefinition
        {
            Id = NoSaveGuard,
            Title = "Prove the run cannot save",
            Category = "Safety",
            Purpose =
                "Confirms the harness has no save path: it never clicks the toolbar Save control and never "
                + "sends a save key sequence, so an open receipt cannot be committed by a probe run.",
            UiSteps =
                "Nothing on screen. The app inspects which automation elements exist and reports whether a "
                + "Save control was ever touched.",
            Expected = "No save action was performed and the posting state is unchanged.",
            ReportedKeys = ["POSTING_STATE_BEFORE", "POSTING_STATE_AFTER"],
        },
        new Model_VisualProbeDefinition
        {
            Id = RestoreIntegrity,
            Title = "Leave the window exactly as it was found",
            Category = "Safety",
            Purpose =
                "Takes a full snapshot of every readable variable before and after the write probes and "
                + "proves the receipt is byte-for-byte the same as when the run started.",
            UiSteps = "No change. The app compares the before and after snapshots field by field.",
            Expected = "Every variable reads the same before and after the run.",
            ReportedKeys = [],
        },
    ];

    /// <summary>Gets every probe definition in display order.</summary>
    public static IReadOnlyList<Model_VisualProbeDefinition> All { get; } = _allProbes;

    /// <summary>
    /// Finds a probe definition by identifier.
    /// </summary>
    /// <param name="id">Probe identifier.</param>
    /// <returns>The matching definition, or <see langword="null"/>.</returns>
    public static Model_VisualProbeDefinition? Find(string id) =>
        All.FirstOrDefault(probe => string.Equals(probe.Id, id, System.StringComparison.Ordinal));
}
