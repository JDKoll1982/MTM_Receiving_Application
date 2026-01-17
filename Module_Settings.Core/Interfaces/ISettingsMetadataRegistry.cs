using System.Collections.Generic;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Registry for code-first settings definitions.
/// </summary>
public interface ISettingsMetadataRegistry
{
    IReadOnlyCollection<Model_SettingsDefinition> GetAll();
    Model_SettingsDefinition? GetDefinition(string category, string key);
    void Register(Model_SettingsDefinition definition);
}
