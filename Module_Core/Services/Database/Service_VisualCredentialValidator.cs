using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services.Database;

/// <summary>
/// Validates whether a Visual username is permitted to drive UI Automation.
/// Blocks shared/service accounts SHOP2 and MTMDC — these accounts run unattended
/// on shop terminals and must never be used for individual automation sessions.
/// </summary>
public class Service_VisualCredentialValidator : IService_VisualCredentialValidator
{
    private static readonly string[] _blockedUsernames = ["SHOP2", "MTMDC"];

    /// <inheritdoc/>
    public bool IsAllowed(string visualUserName)
    {
        var normalised = visualUserName?.Trim().ToUpperInvariant() ?? string.Empty;
        foreach (var blocked in _blockedUsernames)
        {
            if (normalised == blocked)
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public string? GetBlockedReason(string visualUserName)
    {
        if (IsAllowed(visualUserName))
        {
            return null;
        }

        var normalised = visualUserName?.Trim().ToUpperInvariant() ?? string.Empty;
        return $"The Visual account '{normalised}' is a shared terminal account and cannot be used " +
               "for Bulk Inventory automation. Please enter your personal Visual username and password " +
               "in Settings → Users.";
    }
}
