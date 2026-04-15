using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Constants for Volvo shipment status values
/// Prevents magic strings throughout the codebase
/// </summary>
public static class VolvoShipmentStatus
{
    public const string All = "all";

    /// <summary>
    /// Shipment is pending PO creation (user has not yet completed with PO/Receiver numbers)
    /// </summary>
    public const string PendingPo = "pending_po";

    /// <summary>
    /// Shipment has been completed with PO and Receiver numbers
    /// </summary>
    public const string Completed = "completed";

    /// <summary>
    /// Shipment has been archived (soft delete)
    /// </summary>
    public const string Archived = "archived";

    public const string AllDisplayName = "All";
    public const string PendingPoDisplayName = "Pending PO Number";
    public const string CompletedDisplayName = "Completed";
    public const string ArchivedDisplayName = "Archived";

    public static string NormalizeStorageValue(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return PendingPo;
        }

        var trimmedStatus = status.Trim();

        if (trimmedStatus.Equals(All, StringComparison.OrdinalIgnoreCase))
        {
            return All;
        }

        if (trimmedStatus.Equals(AllDisplayName, StringComparison.OrdinalIgnoreCase))
        {
            return All;
        }

        if (
            trimmedStatus.Equals(PendingPo, StringComparison.OrdinalIgnoreCase)
            || trimmedStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            || trimmedStatus.Equals("Pending PO", StringComparison.OrdinalIgnoreCase)
            || trimmedStatus.Equals(PendingPoDisplayName, StringComparison.OrdinalIgnoreCase)
        )
        {
            return PendingPo;
        }

        if (
            trimmedStatus.Equals(Completed, StringComparison.OrdinalIgnoreCase)
            || trimmedStatus.Equals(CompletedDisplayName, StringComparison.OrdinalIgnoreCase)
        )
        {
            return Completed;
        }

        if (
            trimmedStatus.Equals(Archived, StringComparison.OrdinalIgnoreCase)
            || trimmedStatus.Equals(ArchivedDisplayName, StringComparison.OrdinalIgnoreCase)
        )
        {
            return Archived;
        }

        return trimmedStatus;
    }

    public static string ToDisplayName(string? status)
    {
        var normalizedStatus = NormalizeStorageValue(status);

        return normalizedStatus switch
        {
            All => AllDisplayName,
            PendingPo => PendingPoDisplayName,
            Completed => CompletedDisplayName,
            Archived => ArchivedDisplayName,
            _ => HumanizeValue(normalizedStatus),
        };
    }

    private static string HumanizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var humanizedValue = value.Replace('_', ' ').Replace('-', ' ');
        var words = humanizedValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var index = 0; index < words.Length; index++)
        {
            if (words[index].Length == 0)
            {
                continue;
            }

            words[index] = char.ToUpperInvariant(words[index][0])
                + words[index][1..].ToLowerInvariant();
        }

        return string.Join(' ', words);
    }
}
