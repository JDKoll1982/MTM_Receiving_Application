using System;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Scanner.Helpers;

/// <summary>
/// Scanner module access policy. The Scanner navigation entry and automation engine are
/// intended for developers during rollout; this helper decides whether the current user is
/// a developer who may use the Scanner module.
/// </summary>
public static class Helper_ScannerAccess
{
	/// <summary>Developer full name that unlocks the Scanner module.</summary>
	public const string DeveloperFullName = "John Koll";

	/// <summary>Developer Windows usernames (domain-stripped) that unlock the Scanner module.</summary>
	private static readonly string[] DeveloperUserNames = ["jkoll", "johnk"];

	/// <summary>Department value treated as a developer user type.</summary>
	public const string DeveloperUserType = "Developer";

	/// <summary>
	/// Returns true when the given session user is a developer (full name John Koll, or a
	/// domain-stripped Windows username of jkoll/johnk, or a Developer department). When the
	/// session user is unavailable, falls back to the raw Windows user name.
	/// </summary>
	/// <param name="user">The authenticated session user, or null.</param>
	/// <param name="fallbackWindowsUserName">Raw Environment.UserName used when user is null.</param>
	public static bool IsDeveloperUser(Model_User? user, string? fallbackWindowsUserName = null)
	{
		if (user is not null)
		{
			if (user.FullName?.Trim().Equals(DeveloperFullName, StringComparison.OrdinalIgnoreCase) == true)
			{
				return true;
			}

			var username = StripDomain(user.WindowsUsername);
			foreach (var candidate in DeveloperUserNames)
			{
				if (string.Equals(username, candidate, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			if (
				user.Department?.Trim().Equals(DeveloperUserType, StringComparison.OrdinalIgnoreCase)
				== true
			)
			{
				return true;
			}

			return false;
		}

		var fallback = StripDomain(fallbackWindowsUserName);
		foreach (var candidate in DeveloperUserNames)
		{
			if (string.Equals(fallback, candidate, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static string StripDomain(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		var trimmed = value.Trim();
		var backslash = trimmed.LastIndexOf('\\');
		return backslash >= 0 ? trimmed[(backslash + 1)..] : trimmed;
	}
}
