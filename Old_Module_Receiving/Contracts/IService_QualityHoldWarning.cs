using MTM_Receiving_Application.Module_Receiving.Models;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Service for displaying quality hold warnings when restricted parts are entered.
/// Provides immediate feedback to users when MMFSR or MMCSR parts are detected.
/// </summary>
public interface IService_QualityHoldWarning
{
    /// <summary>
    /// Checks if a part ID requires a quality hold warning and displays it if needed.
    /// </summary>
    /// <param name="partID">The part ID to check</param>
    /// <param name="load">Optional receiving load context</param>
    /// <returns>True if warning was shown and acknowledged; false if cancelled or no warning needed</returns>
    Task<bool> CheckAndWarnAsync(string? partID, Model_ReceivingLoad? load = null);

    /// <summary>
    /// Determines if a part ID requires quality hold based on naming pattern.
    /// </summary>
    /// <param name="partID">The part ID to check</param>
    /// <returns>True if part requires quality hold (MMFSR or MMCSR)</returns>
    bool IsRestrictedPart(string? partID);
}
