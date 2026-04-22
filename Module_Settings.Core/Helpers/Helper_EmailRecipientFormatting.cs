using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Helpers;

/// <summary>
/// Formats and generates email-recipient values for module settings surfaces.
/// </summary>
public static class Helper_EmailRecipientFormatting
{
    private const string DefaultDomain = "mantoolmfg.com";

    public static string GenerateDefaultEmail(string firstName, string lastName)
    {
        var normalizedFirstName = NormalizeNamePart(firstName);
        var normalizedLastName = NormalizeNamePart(lastName);

        if (
            string.IsNullOrWhiteSpace(normalizedFirstName)
            || string.IsNullOrWhiteSpace(normalizedLastName)
        )
        {
            return string.Empty;
        }

        return $"{normalizedFirstName[0]}{normalizedLastName}@{DefaultDomain}".ToLowerInvariant();
    }

    public static string BuildFormattedRecipientList(
        IEnumerable<Model_EmailRecipientSetting> recipients,
        string recipientType
    )
    {
        return string.Join(
            "; ",
            recipients
                .Where(recipient =>
                    string.Equals(
                        recipient.RecipientType,
                        recipientType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderBy(recipient => recipient.Id)
                .Select(recipient => recipient.OutlookFormat)
        );
    }

    private static string NormalizeNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
