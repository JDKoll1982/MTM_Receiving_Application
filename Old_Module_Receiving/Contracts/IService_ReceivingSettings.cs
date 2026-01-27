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
}
