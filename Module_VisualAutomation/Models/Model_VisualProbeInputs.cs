using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// The values the probes send to Infor VISUAL. Each one is typed into a reference field, read back, and
/// restored, so a blank or wrong value cannot leave a lasting change on the receipt.
/// </summary>
public sealed class Model_VisualProbeInputs
{
    /// <summary>Gets or sets the value typed into Bill of Lading ID (<c>BOL_ID</c>).</summary>
    public string BolId { get; set; } = string.Empty;

    /// <summary>Gets or sets the value typed into Vendor Packlist ID (<c>VEND_PACK_ID</c>).</summary>
    public string VendPackId { get; set; } = string.Empty;

    /// <summary>Gets or sets the value typed into Vendor Freight Bill ID (<c>VEND_FREIGHT_ID</c>).</summary>
    public string VendFreightId { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether every value is filled in.</summary>
    public bool IsComplete => MissingFields().Count == 0;

    /// <summary>
    /// Lists the macro variables whose value is still blank.
    /// </summary>
    /// <returns>The blank variable names, in dialog order.</returns>
    public IReadOnlyList<string> MissingFields()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(BolId))
        {
            missing.Add("BOL_ID");
        }

        if (string.IsNullOrWhiteSpace(VendPackId))
        {
            missing.Add("VEND_PACK_ID");
        }

        if (string.IsNullOrWhiteSpace(VendFreightId))
        {
            missing.Add("VEND_FREIGHT_ID");
        }

        return missing;
    }

    /// <summary>
    /// Returns the variable/value pairs the probes will send, trimmed.
    /// </summary>
    /// <returns>Values keyed by macro variable name.</returns>
    public IReadOnlyDictionary<string, string> ToWriteValues() =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BOL_ID"] = BolId.Trim(),
            ["VEND_PACK_ID"] = VendPackId.Trim(),
            ["VEND_FREIGHT_ID"] = VendFreightId.Trim(),
        };

    /// <summary>
    /// Copies the values so a dialog can edit them without touching the live set until accepted.
    /// </summary>
    /// <returns>An independent copy.</returns>
    public Model_VisualProbeInputs Clone() =>
        new()
        {
            BolId = BolId,
            VendPackId = VendPackId,
            VendFreightId = VendFreightId,
        };

    /// <summary>
    /// Describes the missing values for a status message.
    /// </summary>
    /// <returns>A sentence naming what is still blank.</returns>
    public string DescribeMissing()
    {
        var missing = MissingFields();

        return missing.Count == 0
            ? string.Empty
            : "Enter a value for " + string.Join(", ", missing.Select(key => key)) + " before running.";
    }
}
