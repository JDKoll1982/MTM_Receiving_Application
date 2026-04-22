using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnageSpecTemplateOption
{
    public string PartId { get; set; } = string.Empty;

    public string HomeLocation { get; set; } = string.Empty;

    public string QuantityType { get; set; } = "Quantity";

    public string Notes { get; set; } = string.Empty;

    public string SpecSummary { get; set; } = string.Empty;

    public Dictionary<string, object?> SpecValues { get; set; } = new();
}
