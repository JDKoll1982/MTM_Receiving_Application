using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Service contract for user-scoped Receiving label path settings.
/// </summary>
public interface IService_ReceivingUserLabelSettings
{
    /// <summary>
    /// Gets the current user's Receiving label file path.
    /// </summary>
    /// <returns>The configured path, or empty string if not set.</returns>
    Task<string> GetReceivingLabelPathAsync();

    /// <summary>
    /// Gets the current user's Mini-Receiving label file path.
    /// </summary>
    /// <returns>The configured path, or empty string if not set.</returns>
    Task<string> GetMiniReceivingLabelPathAsync();

    /// <summary>
    /// Saves the current user's Receiving label file path.
    /// </summary>
    /// <param name="path">The path to save.</param>
    Task SaveReceivingLabelPathAsync(string path);

    /// <summary>
    /// Saves the current user's Mini-Receiving label file path.
    /// </summary>
    /// <param name="path">The path to save.</param>
    Task SaveMiniReceivingLabelPathAsync(string path);
}
