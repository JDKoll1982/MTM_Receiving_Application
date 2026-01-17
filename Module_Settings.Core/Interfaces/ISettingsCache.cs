using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// In-memory settings cache.
/// </summary>
public interface ISettingsCache
{
    bool TryGet(string cacheKey, out Model_SettingsValue? value);
    void Set(string cacheKey, Model_SettingsValue value);
    void Remove(string cacheKey);
    void Clear();
}
