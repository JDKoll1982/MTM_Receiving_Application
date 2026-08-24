using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// User-scoped scanner sending profile.
/// </summary>
public sealed partial class Model_ScannerProfile
{
	public Guid ProfileId { get; set; } = Guid.NewGuid();

	public string OwnerUserId { get; set; } = string.Empty;

	public string ProfileName { get; set; } = string.Empty;

	public bool IsDefaultForUser { get; set; }

	public string TargetExecutableName { get; set; } = "VMINVENT.exe";

	public string AppWindowTitle { get; set; } = string.Empty;

	public string TargetChildWindowTitle { get; set; } = "Inventory Transfers";

	public string AppWindowClass { get; set; } = string.Empty;

	public string TargetFrameworkFamily { get; set; } = "Gupta/Centura";

	public bool RequireExactTitleMatch { get; set; }

	public bool ActivateAppBeforeSend { get; set; } = true;

	public string FromWarehouseDefault { get; set; } = "002";

	public string ToWarehouseDefault { get; set; } = "002";

	public bool AllowPerItemWarehouseOverride { get; set; }

	public int ActivationDelayMs { get; set; } = 250;

	public int PauseAfterItemMs { get; set; } = 150;

	public int DelayBetweenFieldsMs { get; set; } = 50;

	public int PopupTimeoutMs { get; set; } = 1500;

	public int PopupCloseTimeoutMs { get; set; } = 1500;

	public string SendShortcutLabel { get; set; } = "Send";

	public string SendShortcutChord { get; set; } = "Ctrl+Alt+M";

	public bool StopBetweenSendsOnly { get; set; } = true;

	public bool AllowAdvancedTiming { get; set; }

	public int? MaxItemsPerSend { get; set; }

	public bool EnforceFocusEveryItem { get; set; }

	public string Notes { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}