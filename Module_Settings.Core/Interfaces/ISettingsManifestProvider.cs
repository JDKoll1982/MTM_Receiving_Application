using System.Collections.Generic;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Provides settings definitions from the manifest defaults file.
/// </summary>
public interface ISettingsManifestProvider
{
    IReadOnlyCollection<Model_SettingsDefinition> LoadDefinitions();
}
