namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Validates whether a Visual username is permitted to run automation.
/// Blocks shared/service accounts that must never drive UI Automation (SHOP2, MTMDC).
/// Pure logic — no async, no external dependencies.
/// </summary>
public interface IService_VisualCredentialValidator
{
    /// <summary>
    /// Returns <see langword="true"/> when the username is allowed to drive Visual automation;
    /// <see langword="false"/> if the account is on the blocklist.
    /// </summary>
    /// <param name="visualUserName">The Visual username to check (trimmed before comparison).</param>
    bool IsAllowed(string visualUserName);

    /// <summary>
    /// Returns a user-facing reason string when the username is blocked, or <see langword="null"/>
    /// when the username is permitted.
    /// </summary>
    /// <param name="visualUserName">The Visual username to check.</param>
    string? GetBlockedReason(string visualUserName);
}
