using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Constants and normalization helpers for Volvo line PO status values.
/// </summary>
public static class VolvoLinePoStatus
{
    public const string Pending = "Pending";
    public const string Received = "Received";

    public static string NormalizeStorageValue(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Pending;
        }

        if (status.Trim().Equals(Received, StringComparison.OrdinalIgnoreCase))
        {
            return Received;
        }

        return Pending;
    }
}
