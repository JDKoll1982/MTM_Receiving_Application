using System;
using System.Globalization;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// Reads the value a clerk typed into the Edit Mode batch panel.
/// </summary>
/// <remarks>
/// A batch edit writes one value onto many rows at once, so a typo is multiplied by however many
/// rows are ticked. Every value is therefore read and checked here before anything is written, and a
/// value that cannot be read is refused with a reason instead of being coerced to zero.
/// </remarks>
public static class Helper_ReceivingEditModeBatchValue
{
    /// <summary>
    /// Reads <paramref name="text"/> as the field's own type.
    /// </summary>
    /// <param name="field">The field being changed.</param>
    /// <param name="text">What the clerk typed.</param>
    /// <param name="value">The value to write when this returns true.</param>
    /// <param name="error">Why the value was refused, in wording the clerk can act on.</param>
    public static bool TryRead(
        Model_EditModeBatchField field,
        string? text,
        out object? value,
        out string error)
    {
        ArgumentNullException.ThrowIfNull(field);

        value = null;
        error = string.Empty;
        var trimmed = text?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            error = $"Enter a {field.Label.ToLowerInvariant()} first.";
            return false;
        }

        switch (field.Kind)
        {
            case Enum_EditModeBatchFieldKind.Integer when int.TryParse(trimmed, out var number):
                value = number;
                return true;

            case Enum_EditModeBatchFieldKind.Integer:
                error = $"{field.Label} needs a whole number, for example 1.";
                return false;

            case Enum_EditModeBatchFieldKind.Decimal
                when decimal.TryParse(
                    trimmed,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out var amount
                ):
                value = amount;
                return true;

            case Enum_EditModeBatchFieldKind.Decimal:
                error = $"{field.Label} needs a number, for example 1250.5.";
                return false;

            case Enum_EditModeBatchFieldKind.PackageType:
                var match = MatchPackageType(trimmed);
                if (match is not null)
                {
                    value = match;
                    return true;
                }

                error =
                    $"{field.Label} must be one of {string.Join(", ", Enum.GetNames<Enum_PackageType>())}.";
                return false;

            default:
                value = trimmed;
                return true;
        }
    }

    /// <summary>
    /// Matches a typed package type name, accepting the plural the grid shows ("Skids").
    /// </summary>
    private static string? MatchPackageType(string typed) =>
        Enum.GetNames<Enum_PackageType>()
            .FirstOrDefault(name =>
                string.Equals(name, typed, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name + "s", typed, StringComparison.OrdinalIgnoreCase)
            );
}
