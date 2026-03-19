using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Provides access to Module_Receiving settings values (system or user scope) with defaults.
/// </summary>
public interface IService_ReceivingSettings
{
    Task<string> GetStringAsync(string key, int? userId = null);

    Task<bool> GetBoolAsync(string key, int? userId = null);

    Task<int> GetIntAsync(string key, int? userId = null);

    Task<string> FormatAsync(string key, object? arg0, int? userId = null);

    Task<string> FormatAsync(string key, object? arg0, object? arg1, int? userId = null);

    /// <summary>Persists a string value for the given key in the current user scope.</summary>
    /// <param name="key">The settings key to store the value under.</param>
    /// <param name="value">The string value to persist.</param>
    /// <param name="userId">Optional user ID for user-scoped settings.</param>
    Task SaveStringAsync(string key, string value, int? userId = null);
}
