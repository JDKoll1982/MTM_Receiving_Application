using System;
using System.Collections.Concurrent;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// In-memory settings cache scoped to the current session.
/// </summary>
public class Service_SettingsCache : ISettingsCache
{
    private readonly ConcurrentDictionary<string, Model_SettingsValue> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string cacheKey, out Model_SettingsValue? value)
    {
        return _cache.TryGetValue(cacheKey, out value);
    }

    public void Set(string cacheKey, Model_SettingsValue value)
    {
        _cache[cacheKey] = value;
    }

    public void Remove(string cacheKey)
    {
        _cache.TryRemove(cacheKey, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
