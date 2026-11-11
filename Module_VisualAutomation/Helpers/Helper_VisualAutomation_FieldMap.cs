using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_VisualAutomation.Helpers;

/// <summary>
/// Maps the Infor VISUAL Purchase Receipt Entry macro variables (the list the module's macro editor
/// shows under Available Fields) to the labels actually painted on the window.
/// </summary>
/// <remarks>
/// The macro editor exposes far more variables than the General tab displays. A variable with no
/// label here has no control on the visible surface, so the harness can report it as resolved only
/// through the macro engine - which this app deliberately does not use.
/// </remarks>
public static class Helper_VisualAutomation_FieldMap
{
    private static readonly Dictionary<string, string> _labels = new(StringComparer.Ordinal)
    {
        ["ORDER_ID"] = "Order ID",
        ["SITE_ID"] = "Site ID",
        ["RECEIVER_ID"] = "Receiver ID",
        ["DISPATCH_ID"] = "Service Dispatch ID",
        ["DESIRED_RECV_DATE"] = "Receive on",
        ["PROMISE_DATE"] = "Promise Delivery Date",
        ["PROMISE_SHIP_DATE"] = "Promise Ship Date",
        ["BOL_ID"] = "Bill of Lading ID",
        ["VEND_PACK_ID"] = "Vendor Packlist ID",
        ["VEND_PACK_DATE"] = "Vendor Packlist Date",
        ["VEND_FREIGHT_ID"] = "Vendor Freight Bill ID",
        ["BUYER"] = "Buyer",
        ["FREE_ON_BOARD"] = "FOB",
        ["SHIP_VIA"] = "Ship via",
        ["CARRIER_ID"] = "Carrier ID",
        ["RECEIVED_DATE"] = "Act Recv Date",
    };

    /// <summary>
    /// Variables the harness is allowed to type into. Only free-text and non-validating fields are
    /// included so a probe can never disturb dates, quantities, or the posting state.
    /// </summary>
    private static readonly HashSet<string> _writable = new(StringComparer.Ordinal)
    {
        "BOL_ID",
        "VEND_PACK_ID",
        "VEND_FREIGHT_ID",
    };

    /// <summary>
    /// The complete variable list the Purchase Receipt Entry macro editor shows, in the editor's
    /// alphabetical order.
    /// </summary>
    private static readonly string[] _macroVariables =
    [
        "BOL_ID",
        "BUYER",
        "CARRIER_ID",
        "CONTACT_FAX",
        "CONTACT_FIRST_NAME",
        "CONTACT_HONORIFIC",
        "CONTACT_INITIAL",
        "CONTACT_LAST_NAME",
        "CONTACT_PHONE",
        "DATABASE",
        "DESIRED_RECV_DATE",
        "DISPATCH_ID",
        "FREE_ON_BOARD",
        "LINES",
        "LONG_DESCRIPTION",
        "MACRO_MESSAGE",
        "MACRO_SUCCESS",
        "ORDER_ID",
        "PO_USER_1",
        "PO_USER_2",
        "PO_USER_3",
        "PO_USER_4",
        "PO_USER_5",
        "PO_USER_6",
        "PO_USER_7",
        "PO_USER_8",
        "PO_USER_9",
        "PO_USER_10",
        "POSTING_CANDIDATE",
        "PROMISE_DATE",
        "PROMISE_SHIP_DATE",
        "RECEIVED_DATE",
        "RECEIVER_ID",
        "RECVR_LONG_DESC",
        "SHIP_VIA",
        "SITE_ID",
        "STATUS",
        "USER",
        "VEND_FREIGHT_ID",
        "VEND_PACK_DATE",
        "VEND_PACK_ID",
        "VENDOR_ID",
        "udfBoolean1",
        "udfBoolean2",
        "udfBoolean3",
        "udfBoolean4",
        "udfBoolean5",
        "udfDate1",
        "udfDate2",
        "udfDate3",
        "udfDate4",
        "udfDate5",
        "udfNumber1",
        "udfNumber2",
        "udfNumber3",
        "udfNumber4",
        "udfNumber5",
        "udfNumber6",
        "udfNumber7",
        "udfNumber8",
        "udfNumber9",
        "udfNumber10",
        "udfStrCombo1",
        "udfStrCombo2",
        "udfStrCombo3",
        "udfStrCombo4",
        "udfStrCombo5",
        "udfStrCombo6",
        "udfStrCombo7",
        "udfStrCombo8",
        "udfStrCombo9",
        "udfStrCombo10",
        "udfString1",
        "udfString2",
        "udfString3",
        "udfString4",
        "udfString5",
        "udfString6",
        "udfString7",
        "udfString8",
        "udfString9",
        "udfString10",
        "udfURL1",
        "udfURL2",
        "udfURL3",
        "udfURL4",
        "udfURL5",
    ];

    /// <summary>Gets every macro variable the receipt window's macro editor offers.</summary>
    public static IReadOnlyList<string> MacroVariables => _macroVariables;

    /// <summary>Gets the variables that have a control on the visible window.</summary>
    public static IReadOnlyList<string> VisibleVariables => [.. _labels.Keys];

    /// <summary>
    /// Returns the on-screen label for a macro variable.
    /// </summary>
    /// <param name="key">Macro variable name.</param>
    /// <returns>The label, or <see langword="null"/> when the variable has no visible control.</returns>
    public static string? LabelFor(string key) =>
        _labels.TryGetValue(key, out var label) ? label : null;

    /// <summary>
    /// Returns whether the harness may type into a variable's control.
    /// </summary>
    /// <param name="key">Macro variable name.</param>
    /// <returns>True when the field is safe to write.</returns>
    public static bool IsWritable(string key) => _writable.Contains(key);

    /// <summary>
    /// Normalizes a label so it can be matched against the name UI Automation reports.
    /// </summary>
    /// <param name="text">Label or automation name.</param>
    /// <returns>Upper-case text with runs of whitespace collapsed and no trailing colon.</returns>
    public static string NormalizeLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim().TrimEnd(':').Trim();
        return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    /// <summary>
    /// Resolves the macro variable that owns an on-screen label.
    /// </summary>
    /// <param name="label">Label text read from the window.</param>
    /// <returns>The macro variable name, or <see langword="null"/> when the label is not mapped.</returns>
    public static string? KeyForLabel(string? label)
    {
        var normalized = NormalizeLabel(label);
        if (normalized.Length == 0)
        {
            return null;
        }

        return _labels
            .FirstOrDefault(pair => NormalizeLabel(pair.Value) == normalized)
            .Key;
    }
}
