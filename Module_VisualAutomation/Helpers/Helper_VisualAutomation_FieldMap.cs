using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_VisualAutomation.Enums;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Helpers;

/// <summary>
/// Maps the Infor VISUAL Purchase Receipt Entry macro variables (the list the module's macro editor
/// shows under Available Fields) to the labels actually painted on the window, and to the MTMFG
/// table and column each variable really represents.
/// </summary>
/// <remarks>
/// The macro editor exposes far more variables than the General tab displays. A variable with no
/// label here has no control on the visible surface. The verified bindings in
/// <see cref="Bindings"/> come from the live VMRCVENT process memory, read-only MTMFG queries, and the
/// VISUAL API Toolkit documentation; see docs/development/InforVisual-Macro-Variable-Reference.md.
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

    /// <summary>Gets the verified binding for every macro variable, keyed by variable name.</summary>
    public static IReadOnlyDictionary<string, Model_VisualMacroVariableBinding> Bindings { get; } =
        CreateBindings();

    /// <summary>
    /// Returns what a macro variable really is.
    /// </summary>
    /// <param name="key">Macro variable name.</param>
    /// <returns>The binding, or <see langword="null"/> when the variable is unknown.</returns>
    public static Model_VisualMacroVariableBinding? BindingFor(string key) =>
        Bindings.TryGetValue(key, out var binding) ? binding : null;

    /// <summary>
    /// Returns whether automation may safely type into a variable's control.
    /// </summary>
    /// <param name="key">Macro variable name.</param>
    /// <returns>True when the variable is receipt-only free text with no database consequence.</returns>
    public static bool IsSafeToWrite(string key) =>
        BindingFor(key)?.Safety == Enum_VisualVariableSafety.ReceiptOnly;

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

    private static Dictionary<string, Model_VisualMacroVariableBinding> CreateBindings()
    {
        var bindings = new Dictionary<string, Model_VisualMacroVariableBinding>(
            StringComparer.Ordinal
        );

        void Add(
            string key,
            string meaning,
            string table,
            string column,
            string dataType,
            Enum_VisualVariableSafety safety
        ) =>
            bindings[key] = new Model_VisualMacroVariableBinding
            {
                Key = key,
                Meaning = meaning,
                Table = table,
                Column = column,
                DataType = dataType,
                Safety = safety,
            };

        const string Po = "PURCHASE_ORDER";
        const string Pol = "PURC_ORDER_LINE";
        const string Rcv = "RECEIVER";

        Add("ORDER_ID", "Purchase order being received", Rcv, "PURC_ORDER_ID", "nvarchar(30)", Enum_VisualVariableSafety.Identity);
        Add("RECEIVER_ID", "Receipt document number", Rcv, "ID", "nvarchar(30)", Enum_VisualVariableSafety.Identity);
        Add("SITE_ID", "Site the receipt is booked in", Rcv, "SITE_ID", "nvarchar(30)", Enum_VisualVariableSafety.Identity);
        Add("VENDOR_ID", "Vendor on the purchase order", Po, "VENDOR_ID", "nvarchar(30)", Enum_VisualVariableSafety.Identity);
        Add("RECEIVED_DATE", "Receipt transaction date", Rcv, "RECEIVED_DATE", "datetime", Enum_VisualVariableSafety.Identity);
        Add("STATUS", "PO release status", Po, "STATUS", "nchar(2)", Enum_VisualVariableSafety.Identity);
        Add("POSTING_CANDIDATE", "Whether the PO may post to the GL", Po, "POSTING_CANDIDATE", "nchar(2)", Enum_VisualVariableSafety.Identity);

        Add("BUYER", "Buyer code copied onto the PO", Po, "BUYER", "nvarchar(40)", Enum_VisualVariableSafety.Consequential);
        Add("FREE_ON_BOARD", "The FOB field on screen", Po, "FREE_ON_BOARD", "nvarchar(50)", Enum_VisualVariableSafety.Consequential);
        Add("SHIP_VIA", "Ship-via text copied onto the PO", Po, "SHIP_VIA", "nvarchar(80)", Enum_VisualVariableSafety.Consequential);
        Add("PROMISE_DATE", "Promised delivery date", Po, "PROMISE_DATE", "datetime", Enum_VisualVariableSafety.Consequential);
        Add("PROMISE_SHIP_DATE", "Promised ship date", Po, "PROMISE_SHIP_DATE", "datetime", Enum_VisualVariableSafety.Consequential);
        Add("DESIRED_RECV_DATE", "The Receive on date", Pol, "DESIRED_RECV_DATE", "datetime", Enum_VisualVariableSafety.Consequential);
        Add("DISPATCH_ID", "Outside-service dispatch this receipt consumes", "SERVICE_RCVR_DISP", "DISPATCH_ID", "nvarchar", Enum_VisualVariableSafety.Consequential);
        Add("RECVR_LONG_DESC", "Long description for the receipt line", "RECEIVER_LINE", "DESCRIPTION", "nvarchar", Enum_VisualVariableSafety.Consequential);
        Add("LONG_DESCRIPTION", "Long description of the vendor part", "VENDOR_PART", "LONG_DESCRIPTION", "image", Enum_VisualVariableSafety.Consequential);

        Add("BOL_ID", "Bill of lading on the receipt", Rcv, "BOL_ID", "nvarchar", Enum_VisualVariableSafety.ReceiptOnly);
        Add("CARRIER_ID", "Carrier on the receipt", Rcv, "CARRIER_ID", "nvarchar(40)", Enum_VisualVariableSafety.ReceiptOnly);
        Add("VEND_PACK_ID", "Vendor packlist on the receipt", Rcv, "VEND_PACK_ID", "nvarchar(80)", Enum_VisualVariableSafety.ReceiptOnly);
        Add("VEND_PACK_DATE", "Vendor packlist date on the receipt", Rcv, "VEND_PACK_DATE", "datetime", Enum_VisualVariableSafety.ReceiptOnly);
        Add("VEND_FREIGHT_ID", "Vendor freight bill on the receipt", Rcv, "VEND_FREIGHT_ID", "nvarchar(80)", Enum_VisualVariableSafety.ReceiptOnly);

        Add("CONTACT_FIRST_NAME", "Vendor contact block", Po, "CONTACT_FIRST_NAME", "nvarchar(60)", Enum_VisualVariableSafety.Identity);
        Add("CONTACT_LAST_NAME", "Vendor contact block", Po, "CONTACT_LAST_NAME", "nvarchar(60)", Enum_VisualVariableSafety.Identity);
        Add("CONTACT_INITIAL", "Vendor contact block", Po, "CONTACT_INITIAL", "nvarchar(4)", Enum_VisualVariableSafety.Identity);
        Add("CONTACT_HONORIFIC", "Vendor contact block", Po, "CONTACT_HONORIFIC", "nvarchar(30)", Enum_VisualVariableSafety.Identity);
        Add("CONTACT_PHONE", "Vendor contact block", Po, "CONTACT_PHONE", "nvarchar(40)", Enum_VisualVariableSafety.Identity);
        Add("CONTACT_FAX", "Vendor contact block", Po, "CONTACT_FAX", "nvarchar(40)", Enum_VisualVariableSafety.Identity);

        for (var index = 1; index <= 10; index++)
        {
            Add(
                $"PO_USER_{index}",
                $"PO-level user-defined text {index}",
                Po,
                $"USER_{index}",
                "nvarchar(160)",
                Enum_VisualVariableSafety.Consequential
            );
        }

        Add("DATABASE", "Connected VISUAL database name", string.Empty, string.Empty, string.Empty, Enum_VisualVariableSafety.Runtime);
        Add("USER", "VISUAL login of the operator", string.Empty, string.Empty, string.Empty, Enum_VisualVariableSafety.Runtime);
        Add("LINES", "Count of the receipt line collection", string.Empty, string.Empty, string.Empty, Enum_VisualVariableSafety.Runtime);
        Add("MACRO_SUCCESS", "Flag a macro sets to report success", string.Empty, string.Empty, string.Empty, Enum_VisualVariableSafety.MacroOutput);
        Add("MACRO_MESSAGE", "Free text a macro returns to the caller", string.Empty, string.Empty, string.Empty, Enum_VisualVariableSafety.MacroOutput);

        foreach (var key in _macroVariables.Where(name => name.StartsWith("udf", StringComparison.Ordinal)))
        {
            Add(
                key,
                "Form-level user-defined slot - never persisted by this module",
                string.Empty,
                string.Empty,
                string.Empty,
                Enum_VisualVariableSafety.Scratch
            );
        }

        return bindings;
    }
}
