using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Service contract for user-scoped Dunnage label path settings.
/// </summary>
public interface IService_DunnageUserLabelSettings
{
    /// <summary>
    /// Gets the current user's Dunnage label file path.
    /// </summary>
    /// <returns>The configured path, or empty string if not set.</returns>
    Task<string> GetDunnageLabelPathAsync();

    /// <summary>
    /// Saves the current user's Dunnage label file path.
    /// </summary>
    /// <param name="path">The path to save.</param>
    Task SaveDunnageLabelPathAsync(string path);
}
