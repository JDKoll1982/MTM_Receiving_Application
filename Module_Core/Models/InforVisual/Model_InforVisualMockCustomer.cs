namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// A mock customer record used when Infor Visual mock data mode is enabled.
/// </summary>
public class Model_InforVisualMockCustomer
{
    /// <summary>The Infor Visual customer key (e.g. "VOLVO").</summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>The customer display name (e.g. "Volvo Trucks").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional customer city used for picker detail text.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Optional customer state used for picker detail text.</summary>
    public string State { get; set; } = string.Empty;
}
