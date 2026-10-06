using MTM_Receiving_Application.Module_Shared.Helpers;

namespace MTM_Receiving_Application.Module_Scanner.Helpers;

/// <summary>
/// Scanner-facing location sanitizer. Delegates to
/// <see cref="Helper_SharedLocationFormat"/> so the per-row <c>To</c> cell, the Workbench
/// location lookup, and every other module resolve a typed location to the exact same
/// canonical value ("VA-A001" and "VA-A0-1" both become "VA-A0-01", "r04" becomes "R-04").
/// Pure logic with no Win32 or database dependencies so it can be unit-tested.
/// </summary>
public static class Helper_ScannerLocationFormat
{
	/// <summary>
	/// Returns the canonical uppercase location, or <see langword="null"/> when
	/// <paramref name="rawInput"/> matches no supported layout.
	/// </summary>
	public static string? SanitizeLocationFormat(string? rawInput)
	{
		return Helper_SharedLocationFormat.Sanitize(rawInput);
	}
}
