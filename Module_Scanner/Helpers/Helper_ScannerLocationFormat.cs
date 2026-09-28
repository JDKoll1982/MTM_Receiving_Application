using System;
using System.Text.RegularExpressions;

namespace MTM_Receiving_Application.Module_Scanner.Helpers;

/// <summary>
/// Sanitizes warehouse-location text typed by the operator into the canonical
/// <c>PREFIX-MID-SUFFIX</c> layout (for example "VA-A001" becomes "VA-A0-01").
/// Resolves all 16 layout variants documented in ScannerUpdate.md (Step 6b-a1-c-b):
/// 8 standard 2-digit-padded forms and 8 unpadded forms that get a leading zero.
/// Pure logic with no Win32 or database dependencies so it can be unit-tested.
/// </summary>
public static class Helper_ScannerLocationFormat
{
	/// <summary>
	/// Matches <c>PREFIX(-)MID(-)SUFFIX</c> where prefix is 1-2 letters, mid is a letter plus
	/// a digit, and suffix is 1-2 digits. Hyphens are optional so every user variant is
	/// normalized to the canonical form. The trailing <c>$</c> anchors to end of string.
	/// </summary>
	private static readonly Regex LocationRegex = new(
		@"^(?<prefix>[A-Za-z]{1,2})-?(?<mid>[A-Za-z][0-9])-?(?<suffix>[0-9]{1,2})$",
		RegexOptions.Compiled
	);

	/// <summary>
	/// Returns the canonical uppercase <c>PREFIX-MID-SUFFIX</c> string, or <see langword="null"/>
	/// when <paramref name="rawInput"/> does not match the expected layout.
	/// </summary>
	public static string? SanitizeLocationFormat(string? rawInput)
	{
		if (string.IsNullOrWhiteSpace(rawInput))
		{
			return null;
		}

		var match = LocationRegex.Match(rawInput.Trim());
		if (!match.Success)
		{
			return null;
		}

		var prefix = match.Groups["prefix"].Value.ToUpperInvariant();
		var mid = match.Groups["mid"].Value.ToUpperInvariant();
		var suffix = match.Groups["suffix"].Value;

		if (suffix.Length == 1)
		{
			suffix = "0" + suffix;
		}

		return $"{prefix}-{mid}-{suffix}";
	}
}
