using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Volvo.Contracts;

/// <summary>
/// Service contract for user-scoped Volvo label path settings.
/// </summary>
public interface IService_VolvoUserLabelSettings
{
    /// <summary>
    /// Gets the current user's Volvo label file path.
    /// </summary>
    /// <returns>The configured path, or empty string if not set.</returns>
    Task<string> GetVolvoLabelPathAsync();

    /// <summary>
    /// Saves the current user's Volvo label file path.
    /// </summary>
    /// <param name="path">The path to save.</param>
    Task SaveVolvoLabelPathAsync(string path);
}
