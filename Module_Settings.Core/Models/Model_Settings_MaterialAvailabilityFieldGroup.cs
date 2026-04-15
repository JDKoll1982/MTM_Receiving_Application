using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Groups field options under one card heading in the Material Availability settings page.
/// </summary>
public sealed class Model_Settings_MaterialAvailabilityFieldGroup
{
    public string Title { get; set; } = string.Empty;

    public ObservableCollection<Model_Settings_MaterialAvailabilityFieldOption> Fields { get; set; } =
    [];
}
