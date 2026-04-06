using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Provides typed access to Dunnage settings values with fallback defaults.
/// </summary>
public interface IService_DunnageSettings
{
    Task<string> GetStringAsync(string key, int? userId = null);

    Task<bool> GetBoolAsync(string key, int? userId = null);

    Task<int> GetIntAsync(string key, int? userId = null);

    Task SaveStringAsync(string key, string value, int? userId = null);
}
