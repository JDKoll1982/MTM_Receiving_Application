using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service interface for Quality Hold detection and two-step acknowledgment workflow
/// User Requirements:
///   - Configurable part patterns (not hardcoded)
///   - Two-step acknowledgment dialogs
///   - Hard block on save without acknowledgment
/// </summary>
public interface IService_Receiving_QualityHoldDetection
{
    /// <summary>
    /// Detects if a part number matches restricted part patterns and shows first acknowledgment dialog
    /// Step 1 of 2: User acknowledgment on part selection
    /// </summary>
    /// <param name="partNumber">Part number to check</param>
    /// <returns>Tuple: (IsRestricted, UserAcknowledged, MatchedPattern, RestrictionType)</returns>
    Task<(bool IsRestricted, bool UserAcknowledged, string? MatchedPattern, string? RestrictionType)> DetectAndShowFirstWarningAsync(string partNumber);

    /// <summary>
    /// Shows final acknowledgment dialog before save
    /// Step 2 of 2: Final acknowledgment required before save
    /// </summary>
    /// <param name="partNumber">Part number being saved</param>
    /// <param name="loadNumber">Load number for display</param>
    /// <param name="restrictionType">Type of restriction from Step 1</param>
    /// <returns>True if user acknowledged, false if cancelled</returns>
    Task<bool> ShowFinalAcknowledgmentDialogAsync(string partNumber, int loadNumber, string restrictionType);

    /// <summary>
    /// Validates if a part requires quality hold (without showing dialogs)
    /// Used for validation checks
    /// </summary>
    /// <param name="partNumber">Part number to validate</param>
    /// <returns>True if part requires quality hold</returns>
    Task<bool> IsRestrictedPartAsync(string partNumber);
}
