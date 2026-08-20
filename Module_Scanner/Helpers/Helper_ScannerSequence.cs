using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Helpers;

/// <summary>
/// Pure decision logic for the scanner execution flow.
/// Kept free of Win32 and database dependencies so it can be unit-tested.
/// </summary>
public static class Helper_ScannerSequence
{
	/// <summary>
	/// Ordered field values emitted for one Infor Visual Inventory Transfer record.
	/// Field order matches the transfer data contract used by the VMINVENT screen:
	/// part id, from warehouse, from location, to warehouse, to location, quantity.
	/// </summary>
	public static IReadOnlyList<string> BuildFieldValues(Model_ScannerBatchItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		return
		[
			item.PayloadPartId,
			item.PayloadFromWarehouse,
			item.PayloadFromLocation,
			item.PayloadToWarehouse,
			item.PayloadToLocation,
			item.PayloadQuantity,
		];
	}

	/// <summary>
	/// True when the item is eligible for the next send: it is still waiting and its
	/// validation state is Valid. Invalid or already-sent items are never emitted.
	/// </summary>
	public static bool IsEligibleForSend(Model_ScannerBatchItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		return item.ExecutionState == Enum_ScannerExecutionState.Waiting
			&& item.ValidationState == Enum_ScannerValidationState.Valid;
	}

	/// <summary>Finds the next eligible item by sequence number, or null when none is ready.</summary>
	public static Model_ScannerBatchItem? FindNextEligible(IEnumerable<Model_ScannerBatchItem> items)
	{
		ArgumentNullException.ThrowIfNull(items);

		foreach (var item in items)
		{
			if (IsEligibleForSend(item))
			{
				return item;
			}
		}

		return null;
	}

	/// <summary>
	/// Normalizes a process name for comparison: strips a trailing ".exe" and lowercases.
	/// </summary>
	public static string NormalizeProcessName(string? processName)
	{
		if (string.IsNullOrWhiteSpace(processName))
		{
			return string.Empty;
		}

		var trimmed = processName.Trim();
		if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
		{
			trimmed = trimmed[..^4];
		}

		return trimmed.ToLowerInvariant();
	}

	/// <summary>
	/// True when the foreground process name matches the profile's target executable name.
	/// </summary>
	public static bool IsTargetProcess(string? foregroundProcessName, string? targetExecutableName)
	{
		if (string.IsNullOrWhiteSpace(targetExecutableName))
		{
			return true; // No target configured means no process check.
		}

		var foreground = NormalizeProcessName(foregroundProcessName);
		var target = NormalizeProcessName(targetExecutableName);
		return !string.IsNullOrWhiteSpace(foreground) && string.Equals(
			foreground,
			target,
			StringComparison.OrdinalIgnoreCase
		);
	}

	/// <summary>
	/// True when the foreground window title contains the expected target title (or the
	/// profile does not require a title check).
	/// </summary>
	public static bool IsTargetTitle(string? foregroundTitle, string? expectedTitle, bool requireExact)
	{
		if (string.IsNullOrWhiteSpace(expectedTitle))
		{
			return true; // No title configured means no title check.
		}

		if (string.IsNullOrWhiteSpace(foregroundTitle))
		{
			return false;
		}

		return requireExact
			? string.Equals(foregroundTitle.Trim(), expectedTitle.Trim(), StringComparison.OrdinalIgnoreCase)
			: foregroundTitle.Contains(expectedTitle.Trim(), StringComparison.OrdinalIgnoreCase);
	}
}
